using System;
using System.Collections.Generic;
using Iced.Intel;
using Decoder = Iced.Intel.Decoder;

namespace Xref_Standalone
{
    public static class XrefScannerLowLevel
    {
        public static IEnumerable<IntPtr> JumpTargets(IntPtr codeStart)
        {
            return JumpTargetsImpl(XrefScannerHelper.DecoderForAddress(codeStart));
        }

        private static IEnumerable<IntPtr> JumpTargetsImpl(Decoder myDecoder)
        {
            while (true)
            {
                myDecoder.Decode(out var instruction);
                if (myDecoder.LastError == DecoderError.NoMoreBytes) yield break;
                if (instruction.FlowControl == FlowControl.Return)
                    yield break;

                if (instruction.FlowControl == FlowControl.UnconditionalBranch || instruction.FlowControl == FlowControl.Call)
                {
                    yield return (IntPtr)ExtractTargetAddress(in instruction);
                    if (instruction.FlowControl == FlowControl.UnconditionalBranch) yield break;
                }
            }
        }

        private static ulong ExtractTargetAddress(in Instruction instruction)
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
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
