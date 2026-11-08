using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Findx.DinkToPdf.Contracts;
using Findx.DinkToPdf.EventDefinitions;
using Findx.DinkToPdf.Utils;

namespace Findx.DinkToPdf;

public class BasicConverter : IConverter
{
    public readonly ITools Tools;

    public IDocument ProcessingDocument { get; private set; }

    public event EventHandler<PhaseChangedArgs> PhaseChanged;

    public event EventHandler<ProgressChangedArgs> ProgressChanged;

    public event EventHandler<FinishedArgs> Finished;

    public event EventHandler<ErrorArgs> Error;

    public event EventHandler<WarningArgs> Warning;

    // 回调委托必须以实例字段持有：原生层仅持有函数指针，
    // 无任何托管根时转换途中的 GC 会回收委托导致悬空回调崩溃。
    private VoidCallback _phaseChangedCallback;

    private VoidCallback _progressChangedCallback;

    private IntCallback _finishedCallback;

    private StringCallback _warningCallback;

    private StringCallback _errorCallback;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="tools"></param>
    public BasicConverter(ITools tools) 
    {
        Tools = tools;
    }

    public virtual byte[] Convert(IDocument document)
    {
        if (!document.GetObjects().Any())
        {
            throw new ArgumentException("No objects is defined in document that was passed. At least one object must be defined.");
        }

        ProcessingDocument = document;

        var result = Array.Empty<byte>();
        Tools.Load();

        var converter = CreateConverter(document);

        try
        {
            //register events（委托存实例字段，保证 P/Invoke 全程不被 GC 回收）
            _phaseChangedCallback = OnPhaseChanged;
            _progressChangedCallback = OnProgressChanged;
            _finishedCallback = OnFinished;
            _warningCallback = OnWarning;
            _errorCallback = OnError;

            Tools.SetPhaseChangedCallback(converter, _phaseChangedCallback);
            Tools.SetProgressChangedCallback(converter, _progressChangedCallback);
            Tools.SetFinishedCallback(converter, _finishedCallback);
            Tools.SetWarningCallback(converter, _warningCallback);
            Tools.SetErrorCallback(converter, _errorCallback);

            var converted = Tools.DoConversion(converter);

            if (converted)
            {
                result = Tools.GetConversionResult(converter);
            }
        }
        finally
        {
            if (converter != IntPtr.Zero)
            {
                Tools.DestroyConverter(converter);
            }

            //转换结束，解除委托引用
            _phaseChangedCallback = null;
            _progressChangedCallback = null;
            _finishedCallback = null;
            _warningCallback = null;
            _errorCallback = null;
        }

        return result;
    }

    private void OnPhaseChanged(IntPtr converter)
    {
        try
        {
            var currentPhase = Tools.GetCurrentPhase(converter);
            var eventArgs = new PhaseChangedArgs()
            {
                Document = ProcessingDocument,
                PhaseCount = Tools.GetPhaseCount(converter),
                CurrentPhase = currentPhase,
                Description = Tools.GetPhaseDescription(converter, currentPhase)
            };

            PhaseChanged?.Invoke(this, eventArgs);
        }
        catch (Exception ex)
        {
            //订阅者异常不得穿透进原生 wkhtmltox 边界
            Trace.WriteLine("DinkToPdf PhaseChanged handler exception: " + ex);
        }
    }

    private void OnProgressChanged(IntPtr converter)
    {
        try
        {
            var eventArgs = new ProgressChangedArgs()
            {
                Document = ProcessingDocument,
                Description = Tools.GetProgressString(converter)
            };

            ProgressChanged?.Invoke(this, eventArgs);
        }
        catch (Exception ex)
        {
            Trace.WriteLine("DinkToPdf ProgressChanged handler exception: " + ex);
        }
    }

    private void OnFinished(IntPtr converter, int success)
    {
        try
        {
            var eventArgs = new FinishedArgs()
            {
                Document = ProcessingDocument,
                Success = success == 1 ? true : false
            };

            Finished?.Invoke(this, eventArgs);
        }
        catch (Exception ex)
        {
            Trace.WriteLine("DinkToPdf Finished handler exception: " + ex);
        }
    }

    private void OnError(IntPtr converter, IntPtr message)
    {
        try
        {
            var eventArgs = new ErrorArgs()
            {
                Document = ProcessingDocument,
                Message = message == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(message)
            };

            Error?.Invoke(this, eventArgs);
        }
        catch (Exception ex)
        {
            Trace.WriteLine("DinkToPdf Error handler exception: " + ex);
        }
    }

    private void OnWarning(IntPtr converter, IntPtr message)
    {
        try
        {
            var eventArgs = new WarningArgs()
            {
                Document = ProcessingDocument,
                Message = message == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(message)
            };

            Warning?.Invoke(this, eventArgs);
        }
        catch (Exception ex)
        {
            Trace.WriteLine("DinkToPdf Warning handler exception: " + ex);
        }
    }

    private IntPtr CreateConverter(IDocument document) {

        var settings = Tools.CreateGlobalSettings();

        try
        {
            ApplyConfig(settings, document, true);

            var converter = Tools.CreateConverter(settings);

            foreach (var obj in document.GetObjects())
            {
                if (obj != null)
                {
                    var objectSettings = Tools.CreateObjectSettings();

                    try
                    {
                        ApplyConfig(objectSettings, obj, false);

                        Tools.AddObject(converter, objectSettings, obj.GetContent());
                    }
                    finally
                    {
                        //add_object 之后 object settings 句柄即可释放
                        Tools.DestroyObjectSetting(objectSettings);
                    }
                }
            }

            return converter;
        }
        finally
        {
            //create_converter 之后 global settings 句柄即可释放
            Tools.DestroyGlobalSetting(settings);
        }
    }

    private void ApplyConfig(IntPtr config, ISettings settings, bool isGlobal) {

        if (settings == null)
        {
            return;
        }

        var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        var props = settings.GetType().GetProperties(bindingFlags);

        foreach (var prop in props)
        {
            var attrs = (Attribute[])prop.GetCustomAttributes();
            object propValue = prop.GetValue(settings);

            if (propValue == null)
            {
                continue;
            }

            if (attrs.Length > 0 && attrs[0] is WkHtmlAttribute)
            {
                var attr = attrs[0] as WkHtmlAttribute;

                Apply(config, attr.Name, propValue, isGlobal);
            }
            else if (propValue is ISettings value)
            {
                ApplyConfig(config, value, isGlobal);
            }

        }
    }

    private void Apply(IntPtr config, string name, object value, bool isGlobal)
    {
        var type = value.GetType();

        Func<IntPtr, string, string, int> applySetting;
        if (isGlobal)
        {
            applySetting = Tools.SetGlobalSetting;
        }
        else
        {
            applySetting = Tools.SetObjectSetting;
        }

        if (typeof(bool) == type)
        {
            applySetting(config, name, ((bool)value == true ? "true" : "false"));
        }
        else if (typeof(double) == type)
        {
            applySetting(config, name, ((double)value).ToString("0.##", CultureInfo.InvariantCulture));
        }
        else if (typeof(Dictionary<string, string>).IsAssignableFrom(type))
        {
            var dictionary = (Dictionary<string, string>)value;
            var index = 0;

            foreach (var pair in dictionary)
            {
                if (pair.Key == null || pair.Value == null)
                {
                    continue;
                }

                //https://github.com/wkhtmltopdf/wkhtmltopdf/blob/c754e38b074a75a51327df36c4a53f8962020510/src/lib/reflect.hh#L192
                applySetting(config, name + ".append", null);
                applySetting(config, $"{name}[{index}]", pair.Key + "\n" + pair.Value);

                index++;
            }
        }
        else
        {
            applySetting(config, name, value.ToString());
        }
            
    }
}