using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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

        //register events
        Tools.SetPhaseChangedCallback(converter, OnPhaseChanged);
        Tools.SetProgressChangedCallback(converter, OnProgressChanged);
        Tools.SetFinishedCallback(converter, OnFinished);
        Tools.SetWarningCallback(converter, OnWarning);
        Tools.SetErrorCallback(converter, OnError);

        var converted = Tools.DoConversion(converter);

        if (converted)
        {
            result = Tools.GetConversionResult(converter);
        }

        Tools.DestroyConverter(converter);

        return result;
    }

    private void OnPhaseChanged(IntPtr converter)
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

    private void OnProgressChanged(IntPtr converter)
    {
        var eventArgs = new ProgressChangedArgs()
        {
            Document = ProcessingDocument,
            Description = Tools.GetProgressString(converter)
        };

        ProgressChanged?.Invoke(this, eventArgs);
    }

    private void OnFinished(IntPtr converter, int success)
    {
        var eventArgs = new FinishedArgs()
        {
            Document = ProcessingDocument,
            Success = success == 1 ? true : false
        };

        Finished?.Invoke(this, eventArgs);
    }

    private void OnError(IntPtr converter, string message)
    {
        var eventArgs = new ErrorArgs()
        {
            Document = ProcessingDocument,
            Message = message
        };

        Error?.Invoke(this, eventArgs);
    }

    private void OnWarning(IntPtr converter, string message)
    {
        var eventArgs = new WarningArgs()
        {
            Document = ProcessingDocument,
            Message = message
        };

        Warning?.Invoke(this, eventArgs);
    }

    private IntPtr CreateConverter(IDocument document) {

        var converter = IntPtr.Zero;

        {
            var settings = Tools.CreateGlobalSettings();
                
            ApplyConfig(settings, document, true);

            converter = Tools.CreateConverter(settings);
        }
            
        foreach (var obj in document.GetObjects())
        {
            if (obj != null)
            {
                var settings = Tools.CreateObjectSettings();

                ApplyConfig(settings, obj, false);

                Tools.AddObject(converter, settings, obj.GetContent());
            }
        }
            
        return converter;
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