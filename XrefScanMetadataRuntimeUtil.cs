using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Xref_Standalone
{
    internal static class XrefScanMetadataRuntimeUtil
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void InitMetadataForMethod(int metadataUsageToken);

        private static InitMetadataForMethod ourMetadataInitForMethodDelegate;
        private static IntPtr ourMetadataInitForMethodPointer;

        /* CHANGES
         * - Method 'FindMetadataInitForMethod' was modified to get the Unity Object Constructor Pointer from XrefPtrStorage.
         */
        private static unsafe void FindMetadataInitForMethod()
        {
            ourMetadataInitForMethodPointer = XrefScannerLowLevel.JumpTargets(*(IntPtr*)XrefPtrStorage.unityObjectCctorPtr).First();
            ourMetadataInitForMethodDelegate = Marshal.GetDelegateForFunctionPointer<InitMetadataForMethod>(ourMetadataInitForMethodPointer);
        }

        /* CHANGES
         * - Method 'CallMetadataInitForMethod' was modified to take a IntPtr instead of a MethodBase for the parameter.
         */
        internal static unsafe bool CallMetadataInitForMethod(IntPtr nativeMethod)
        {
            if (ourMetadataInitForMethodPointer == IntPtr.Zero)
                FindMetadataInitForMethod();

            var codeStart = *(IntPtr*)nativeMethod;
            var firstCall = XrefScannerLowLevel.JumpTargets(codeStart).FirstOrDefault();
            if (firstCall != ourMetadataInitForMethodPointer || firstCall == IntPtr.Zero) return false;

            var tokenPointer = XrefScanUtilFinder.FindLastRcxReadAddressBeforeCallTo(codeStart, ourMetadataInitForMethodPointer);
            var initFlagPointer = XrefScanUtilFinder.FindByteWriteTargetRightAfterCallTo(codeStart, ourMetadataInitForMethodPointer);

            if (tokenPointer == IntPtr.Zero || initFlagPointer == IntPtr.Zero) return false;

            if (Marshal.ReadByte(initFlagPointer) == 0)
            {
                ourMetadataInitForMethodDelegate(Marshal.ReadInt32(tokenPointer));
                Marshal.WriteByte(initFlagPointer, 1);
            }

            return true;
        }
    }
}
