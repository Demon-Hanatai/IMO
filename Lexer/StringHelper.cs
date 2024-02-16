using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    public unsafe sealed class StringHelper
    {
        [DllImport("Ole32.dll")]
        public static extern IntPtr CoTaskMemAlloc(int size);
        public static string AllTypes = "string|int32|int64|uint32|uint64|double|bool|byte|char|datetime|decimal|float|sbyte|short|ushort";
        public static IntPtr AllocString(string Value)
        {
            IntPtr Address = CoTaskMemAlloc(Value.Length);
            for (int i = 0; i < Value.Length; i++)
                *(char*)(Address + i) = Value[i];
            return Address;

        }

        public static string GetString(long Address)
        {
            List<byte> bytes = new List<byte>();
            while (*(byte*)Address != 0)
            {
                bytes.Add(*(byte*)Address);
                Address++;
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }
    }
}
