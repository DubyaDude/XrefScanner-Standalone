using System;
using Iced.Intel;

namespace Xref_Standalone
{
    internal static class XrefScanUtilFinder
    {
        /* CHANGES
         * - Method 'FindLastRcxReadAddressBeforeCallTo' was modified to use 'DecoderForAddress' and 'DecoderForAddress' at XrefScannerHelper.
         */
        public static IntPtr FindLastRcxReadAddressBeforeCallTo(IntPtr codeStart, IntPtr callTarget)
        {
            var decoder = XrefScannerHelper.DecoderForAddress(codeStart);
            IntPtr lastRcxRead = IntPtr.Zero;

            while (true)
            {
                decoder.Decode(out var instruction);
                if (decoder.LastError == DecoderError.NoMoreBytes) return IntPtr.Zero;

                if (instruction.FlowControl == FlowControl.Return)
                    return IntPtr.Zero;

                if (instruction.FlowControl == FlowControl.UnconditionalBranch)
                    continue;

                if (instruction.Mnemonic == Mnemonic.Int || instruction.Mnemonic == Mnemonic.Int1)
                    return IntPtr.Zero;

                if (instruction.Mnemonic == Mnemonic.Call)
                {
                    var target = XrefScannerHelper.ExtractTargetAddress(instruction);
                    if ((IntPtr)target == callTarget)
                        return lastRcxRead;
                }

                if (instruction.Mnemonic == Mnemonic.Mov)
                {
                    if (instruction.Op0Kind == OpKind.Register && instruction.Op0Register == Register.ECX && instruction.Op1Kind == OpKind.Memory && instruction.IsIPRelativeMemoryOperand)
                    {
                        var movTarget = (IntPtr)instruction.IPRelativeMemoryAddress;
                        if (instruction.MemorySize != MemorySize.UInt32 && instruction.MemorySize != MemorySize.Int32)
                            continue;

                        lastRcxRead = movTarget;
                    }
                }
            }
        }

        /* CHANGES
         * - Method 'FindByteWriteTargetRightAfterCallTo' was modified to use 'DecoderForAddress' and 'DecoderForAddress' at XrefScannerHelper.
         */
        public static IntPtr FindByteWriteTargetRightAfterCallTo(IntPtr codeStart, IntPtr callTarget)
        {
            var decoder = XrefScannerHelper.DecoderForAddress(codeStart);
            var seenCall = false;

            while (true)
            {
                decoder.Decode(out var instruction);
                if (decoder.LastError == DecoderError.NoMoreBytes) return IntPtr.Zero;

                if (instruction.FlowControl == FlowControl.Return)
                    return IntPtr.Zero;

                if (instruction.FlowControl == FlowControl.UnconditionalBranch)
                    continue;

                if (instruction.Mnemonic == Mnemonic.Int || instruction.Mnemonic == Mnemonic.Int1)
                    return IntPtr.Zero;

                if (instruction.Mnemonic == Mnemonic.Call)
                {
                    var target = XrefScannerHelper.ExtractTargetAddress(instruction);
                    if ((IntPtr)target == callTarget)
                        seenCall = true;
                }

                if (instruction.Mnemonic == Mnemonic.Mov && seenCall)
                {
                    if (instruction.Op0Kind == OpKind.Memory && (instruction.MemorySize == MemorySize.Int8 || instruction.MemorySize == MemorySize.UInt8))
                        return (IntPtr)instruction.IPRelativeMemoryAddress;
                }
            }
        }

        /* CHANGES
         * - Method 'ExtractTargetAddress' was removed
         */
    }
}
