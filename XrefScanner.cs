using System;
using System.Collections.Generic;
using System.Linq;

namespace Xref_Standalone
{
    public static class XrefScanner
    {
        /* CHANGES
         * - Method 'XrefScan' was modified to take a IntPtr instead of a MethodBase for the parameter.
         * - Method 'XrefScan' was modified to remove anything regarding cache.
         * - Method 'XrefScan' was modified to use 'DecoderForAddress' and 'XrefScanImpl' at XrefScannerHelper.
         */
        public static unsafe List<XrefInstance> XrefScan(IntPtr nativeMethod)
        {
            if (nativeMethod == IntPtr.Zero) return new List<XrefInstance>();

            XrefScanMetadataRuntimeUtil.CallMetadataInitForMethod(nativeMethod);
            return XrefScannerHelper.XrefScanImpl(XrefScannerHelper.DecoderForAddress(*(IntPtr*)nativeMethod)).ToList();
        }

        /* CHANGES
         * - Method 'UsedBy' was removed
         * - Method 'DecoderForAddress' was moved to XrefScannerHelper
         * - Method 'XrefScanImpl' was moved to XrefScannerHelper
         * - Method 'XrefGlobalClassFilter' was moved to XrefScannerHelper
         * - Method 'ExtractTargetAddress' was moved to XrefScannerHelper
         */
    }
}
