using System;
using System.Collections.Generic;
using System.Linq;

namespace Xref_Standalone
{
    public static class XrefScanner
    {
        public static unsafe List<XrefInstance> XrefScan(IntPtr nativeMethod)
        {
            if (nativeMethod == IntPtr.Zero) return new List<XrefInstance>();

            XrefScanMetadataRuntimeUtil.CallMetadataInitForMethod(nativeMethod);
            return XrefScannerHelper.XrefScanImpl(XrefScannerHelper.DecoderForAddress(*(IntPtr*)nativeMethod)).ToList();
        }
    }
}
