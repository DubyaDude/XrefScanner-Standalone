﻿using System;

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

        /* CHANGES
         * - Method 'ReadAsObject' was removed
         * - Method 'TryResolve' was removed
         */
    }
}
