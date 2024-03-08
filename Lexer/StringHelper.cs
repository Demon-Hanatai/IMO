using System.Runtime.InteropServices;
using System.Text;

namespace Lexer
{
    public sealed unsafe class StringHelper
    {
        [DllImport("Ole32.dll")]
        public static extern IntPtr CoTaskMemAlloc(int size);
        //<<<<<<< HEAD
        public static string AllTypes = @"string|int32|int64|uint32|uint64|double|bool|byte|char|datetime|decimal|float|sbyte|short|ushort|object|int32::|string\*|int32\*|double\*";
        public static string AllOperations = "-|\\|*|%";
        public static IntPtr AllocString(string Value)
        {
            IntPtr Address = CoTaskMemAlloc(Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                *(char*)(Address + i) = Value[i];
            }

            return Address;
        }
        //=======
        //>>>>>>> 8df27a5ff0ad62a734c6c790badb04c4ce1ab3f3

        [DllImport("Ole32.dll")] // tf is ole32.dll L L
        public static extern void CoTaskMemFree(IntPtr ptr);


        public static string GetString(long address)
        {
            List<byte> bytes = [];
            while (*(byte*)address != 0)
            {
                bytes.Add(*(byte*)address);
                address++;

            }
            return Encoding.ASCII.GetString(bytes.ToArray()); // Assuming UTF-16 encoding
        }
    }
}
