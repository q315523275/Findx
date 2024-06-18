﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using Findx.DinkToPdf.Contracts;
using Findx.DinkToPdf.Utils;

namespace Findx.DinkToPdf;

public sealed class PdfTools : ITools
{
    public bool IsLoaded { get; private set; }

    /// <summary>
    ///     Ctor
    /// </summary>
    public PdfTools()
    {
        IsLoaded = false;
    }
        
    public void Load() {

        if (IsLoaded)
        {
            return;
        }

        if (WkHtmlToXBindings.wkhtmltopdf_init(0) == 1)
        {
            IsLoaded = true;
        }
    }

    public bool ExtendedQt()
    {
        return WkHtmlToXBindings.wkhtmltopdf_extended_qt() == 1;
    }

    public string GetLibraryVersion()
    {
        return Marshal.PtrToStringAnsi(WkHtmlToXBindings.wkhtmltopdf_version());
    }

    public IntPtr CreateGlobalSettings()
    {
        return WkHtmlToXBindings.wkhtmltopdf_create_global_settings();
    }

    public int SetGlobalSetting(IntPtr settings, string name, string value)
    {
        return WkHtmlToXBindings.wkhtmltopdf_set_global_setting(settings, name, value);
    }

    public unsafe string GetGlobalSetting(IntPtr settings, string name)
    {
        // default const char * size is 2048 bytes 
        var buffer = new byte[2048];

        fixed (byte* tempBuffer = buffer  )
        {
            WkHtmlToXBindings.wkhtmltopdf_get_global_setting(settings, name, tempBuffer, buffer.Length);
        }

        return GetString(buffer);
    }

    public void DestroyGlobalSetting(IntPtr settings)
    {
        WkHtmlToXBindings.wkhtmltopdf_destroy_global_settings(settings);
    }

    public IntPtr CreateObjectSettings()
    {
        return WkHtmlToXBindings.wkhtmltopdf_create_object_settings();
    }

    public int SetObjectSetting(IntPtr settings, string name, string value)
    {
        return WkHtmlToXBindings.wkhtmltopdf_set_object_setting(settings, name, value);
    }

    public unsafe string GetObjectSetting(IntPtr settings, string name)
    {
        //default const char * size is 2048 bytes 
        var buffer = new byte[2048];

        fixed (byte* tempBuffer = buffer)
        {
            WkHtmlToXBindings.wkhtmltopdf_get_object_setting(settings, name, tempBuffer, buffer.Length);
        }

        return GetString(buffer);
    }

    public void DestroyObjectSetting(IntPtr settings)
    {
        WkHtmlToXBindings.wkhtmltopdf_destroy_object_settings(settings);
    }

    public IntPtr CreateConverter(IntPtr globalSettings)
    {
        return WkHtmlToXBindings.wkhtmltopdf_create_converter(globalSettings);
    }

    public void AddObject(IntPtr converter, IntPtr objectSettings, byte[] data)
    {
        WkHtmlToXBindings.wkhtmltopdf_add_object(converter, objectSettings, data);
    }

    public void AddObject(IntPtr converter, IntPtr objectSettings, string data)
    {
        WkHtmlToXBindings.wkhtmltopdf_add_object(converter, objectSettings, data);
    }

    public bool DoConversion(IntPtr converter)
    {
        return WkHtmlToXBindings.wkhtmltopdf_convert(converter);
    }

    public void DestroyConverter(IntPtr converter)
    {
        WkHtmlToXBindings.wkhtmltopdf_destroy_converter(converter);
    }

    public byte[] GetConversionResult(IntPtr converter)
    {
        IntPtr resultPointer;

        var length = WkHtmlToXBindings.wkhtmltopdf_get_output(converter, out resultPointer);
        var result = new byte[length];
        Marshal.Copy(resultPointer, result, 0, length);

        return result;
    }

    public int SetPhaseChangedCallback(IntPtr converter, VoidCallback callback)
    {
        return WkHtmlToXBindings.wkhtmltopdf_set_phase_changed_callback(converter, callback);
    }

    public int SetProgressChangedCallback(IntPtr converter, VoidCallback callback)
    {
        return WkHtmlToXBindings.wkhtmltopdf_set_progress_changed_callback(converter, callback);
    }

    public int SetFinishedCallback(IntPtr converter, IntCallback callback)
    {
        return WkHtmlToXBindings.wkhtmltopdf_set_finished_callback(converter, callback);
    }

    public int SetWarningCallback(IntPtr converter, StringCallback callback)
    {
        return WkHtmlToXBindings.wkhtmltopdf_set_warning_callback(converter, callback);
    }

    public int SetErrorCallback(IntPtr converter, StringCallback callback)
    {
        return WkHtmlToXBindings.wkhtmltopdf_set_error_callback(converter, callback);
    }

    public int GetPhaseCount(IntPtr converter)
    {
        return WkHtmlToXBindings.wkhtmltopdf_phase_count(converter);
    }

    public int GetCurrentPhase(IntPtr converter)
    {
        return WkHtmlToXBindings.wkhtmltopdf_current_phase(converter);
    }

    public string GetPhaseDescription(IntPtr converter, int phase)
    {
        return Marshal.PtrToStringAnsi(WkHtmlToXBindings.wkhtmltopdf_phase_description(converter, phase));
    }

    public string GetProgressString(IntPtr converter)
    {
        return Marshal.PtrToStringAnsi(WkHtmlToXBindings.wkhtmltopdf_progress_string(converter));
    }

    #region IDisposable Support

    private bool _disposedValue = false; // To detect redundant calls

    void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            //free unmanaged resources (unmanaged objects) and override a finalizer below.
            WkHtmlToXBindings.wkhtmltopdf_deinit();
            // TODO: set large fields to null.

            _disposedValue = true;
        }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    ~PdfTools()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(false);
    }

    // This code added to correctly implement the disposable pattern.
    void IDisposable.Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        GC.SuppressFinalize(this);
    }

    #endregion

    private string GetString(byte[] buffer)
    {
        var value = "";

        var walk = 0;

        while (walk < buffer.Length && buffer[walk] != 0)
        {
            walk++;
        }

        value = Encoding.UTF8.GetString(buffer, 0, walk);

        return value;
    }
}