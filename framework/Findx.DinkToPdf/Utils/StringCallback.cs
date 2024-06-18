using System;
using System.Runtime.InteropServices;

namespace Findx.DinkToPdf.Utils;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void StringCallback(IntPtr converter, [MarshalAs(UnmanagedType.LPStr)] string str);