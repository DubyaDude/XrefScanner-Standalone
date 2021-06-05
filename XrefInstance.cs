using System;

namespace Xref_Standalone
{
    public readonly struct XrefInstance
    {
        public readonly XrefType Type;
        public readonly IntPtr Pointer;
        public readonly IntPtr FoundAt;

        public XrefInstance(XrefType type, IntPtr pointer, IntPtr foundAt)
        {
            Type = type;
            Pointer = pointer;
            FoundAt = foundAt;
        }

        internal XrefInstance RelativeToBase(long baseAddress)
        {
            return new XrefInstance(Type, (IntPtr)((long)Pointer - baseAddress), (IntPtr)((long)FoundAt - baseAddress));
        }

        //public string ReadAsString()
        //{
        //    if (Type != XrefType.Global) throw new InvalidOperationException("Can't read non-global xref as string");

        //    var valueAtPointer = Marshal.ReadIntPtr(Pointer);
        //    return IL2CPP.IntPtrToString(valueAtPointer);
        //}
    }
}
