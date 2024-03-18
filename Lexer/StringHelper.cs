using System.Runtime.InteropServices;
using System.Text;

namespace Lexer
{
    public sealed unsafe class StringHelper
    {
        [DllImport("Ole32.dll")]
        public static extern IntPtr CoTaskMemAlloc(int size);

        [DllImport("Ole32.dll")]
        public static extern void CoTaskMemFree(IntPtr ptr);

        public static IntPtr AllocString(string value)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr Address = CoTaskMemAlloc(value.Length);
                for (int i = 0; i < value.Length; i++)
                {
                    *(char*)(Address + i) = value[i];
                }

                return Address;
            }
            else
            {
                return CustomAllocString(value);
            }
        }

        private static IntPtr CustomAllocString(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            Marshal.WriteByte(ptr + bytes.Length, 0);
            return ptr;
        }

        public static void FreeString(IntPtr ptr)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CoTaskMemFree(ptr);
            }
            else
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public static string AllTypes = @"string|int32|int64|uint32|extern!|uint64|nint|IntPtr|double|bool|byte|char|datetime|decimal|float|sbyte|short|ushort|object|int32::|string\*|int32\*|double\*";
        public static string AllOperations = "-|\\|*|%";

        public static string GetString(long address)
        {
            List<byte> bytes = [];
            while (*(byte*)address != 0)
            {
                bytes.Add(*(byte*)address);
                address++;
            }
            return Encoding.ASCII.GetString(bytes.ToArray());
        }
    }
}
