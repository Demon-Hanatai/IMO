using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Lexer
{
    public unsafe sealed class StringHelper
    {
        [DllImport("Ole32.dll")]
        public static extern IntPtr CoTaskMemAlloc(int size);

        [DllImport("Ole32.dll")]
        public static extern void CoTaskMemFree(IntPtr ptr);

        public static string AllTypes = "string|int32|int64|uint32|uint64|double|bool|byte|char|datetime|decimal|float|sbyte|short|ushort";

        public static IntPtr AllocString(string value)
        {
            int size = value.Length * sizeof(char);
            IntPtr address = CoTaskMemAlloc(size);
            if (address == IntPtr.Zero) throw new OutOfMemoryException("Unable to allocate memory.");

            for (int i = 0; i < value.Length; i++)
                *((char*)address + i) = value[i];

            return address;
        }

        public static string GetString(long address)
        {
            List<byte> bytes = new List<byte>();
            while (*(byte*)address != 0)
            {
                bytes.Add(*(byte*)address);
                address++;
            }
            return Encoding.Unicode.GetString(bytes.ToArray()); // Assuming UTF-16 encoding
        }
    }
}
