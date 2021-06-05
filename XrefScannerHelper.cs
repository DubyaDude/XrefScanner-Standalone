using Iced.Intel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Xref_Standalone
{
    /* CHANGES
     * - Class 'XrefScannerHelper' was added.
     */
    internal static class XrefScannerHelper
    {
        internal static unsafe Decoder DecoderForAddress(IntPtr codeStart, int lengthLimit = 1000)
        {
            if (codeStart == IntPtr.Zero) throw new NullReferenceException(nameof(codeStart));

            var stream = new UnmanagedMemoryStream((byte*)codeStart, lengthLimit, lengthLimit, FileAccess.Read);
            var codeReader = new StreamCodeReader(stream);
            var decoder = Decoder.Create(IntPtr.Size * 8, codeReader);
            decoder.IP = (ulong)codeStart;

            return decoder;
        }

        internal static ulong ExtractTargetAddress(in Instruction instruction)
        {
            switch (instruction.Op0Kind)
            {
                case OpKind.NearBranch16:
                    return instruction.NearBranch16;
                case OpKind.NearBranch32:
                    return instruction.NearBranch32;
                case OpKind.NearBranch64:
                    return instruction.NearBranch64;
                case OpKind.FarBranch16:
                    return instruction.FarBranch16;
                case OpKind.FarBranch32:
                    return instruction.FarBranch32;
                default:
                    return 0;
            }
        }

        /* CHANGES
         * - Method 'XrefScanImpl' was modified to use Console.Writeline to print the exception.
         * - Method 'XrefScanImpl' was modified to use ExtractTargetAddress and XrefGlobalClassFilter from XrefScannerHelper.
         */
        internal static IEnumerable<XrefInstance> XrefScanImpl(Decoder decoder, bool skipClassCheck = false)
        {
            while (true)
            {
                decoder.Decode(out var instruction);
                if (decoder.LastError == DecoderError.NoMoreBytes) yield break;

                if (instruction.FlowControl == FlowControl.Return)
                    yield break;

                if (instruction.Mnemonic == Mnemonic.Int || instruction.Mnemonic == Mnemonic.Int1)
                    yield break;

                if (instruction.Mnemonic == Mnemonic.Call || instruction.Mnemonic == Mnemonic.Jmp)
                {
                    var targetAddress = ExtractTargetAddress(instruction);
                    if (targetAddress != 0)
                        yield return new XrefInstance(XrefType.Method, (IntPtr)targetAddress, (IntPtr)instruction.IP);
                    continue;
                }

                if (instruction.FlowControl == FlowControl.UnconditionalBranch)
                    continue;

                if (instruction.Mnemonic == Mnemonic.Mov)
                {
                    XrefInstance? result = null;
                    try
                    {
                        if (instruction.Op1Kind == OpKind.Memory && instruction.IsIPRelativeMemoryOperand)
                        {
                            var movTarget = (IntPtr)instruction.IPRelativeMemoryAddress;
                            if (instruction.MemorySize != MemorySize.UInt64)
                                continue;

                            if (skipClassCheck || XrefGlobalClassFilter(movTarget))
                                result = new XrefInstance(XrefType.Global, movTarget, (IntPtr)instruction.IP);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("XrefScanImpl\n" + ex);
                    }

                    if (result != null)
                        yield return result.Value;
                }
            }
        }


        /* CHANGES
         * - Method 'XrefGlobalClassFilter' was modified to get the String Pointer and Type Pointer from XrefPtrStorage.
         */
        internal static bool XrefGlobalClassFilter(IntPtr movTarget)
        {
            var valueAtMov = (IntPtr)Marshal.ReadInt64(movTarget);
            if (valueAtMov != IntPtr.Zero)
            {
                var targetClass = (IntPtr)Marshal.ReadInt64(valueAtMov);
                return targetClass == XrefPtrStorage.stringPtr ||
                       targetClass == XrefPtrStorage.typePtr;
            }

            return false;
        }
    }
}
