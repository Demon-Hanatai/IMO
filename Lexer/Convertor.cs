using System.Data;

namespace Lexer
{
    public class Convertor
    {
        public static dynamic ConvertToArrayType(string value, object[] arrays)
        {
            switch (value.Replace("::", null))
            {
                case "nint":
                    IEnumerable<nint> nint = arrays.Cast<nint>();
                    return nint.ToArray();
                case "IntPtr":
                    IEnumerable<IntPtr> ptr = arrays.Cast<IntPtr>();
                    return ptr.ToArray();
                case "string":
                    IEnumerable<string> strs = arrays.Cast<string>();
                    return strs.ToArray();
                case "int32":
                    IEnumerable<int> ints = arrays.Cast<int>();
                    return ints.ToArray();
                case "int64":
                    IEnumerable<long> longs = arrays.Cast<long>();
                    return longs.ToArray();
                case "uint32":
                    IEnumerable<uint> uints = arrays.Cast<uint>();
                    return uints.ToArray();
                case "uint64":
                    IEnumerable<ulong> ulongs = arrays.Cast<ulong>();
                    return ulongs.ToArray();
                case "double":
                    IEnumerable<double> doubles = arrays.Cast<double>();
                    return doubles.ToArray();
                case "bool":
                    IEnumerable<bool> bools = arrays.Cast<bool>();
                    return bools.ToArray();
                case "byte":
                    IEnumerable<byte> bytes = arrays.Cast<byte>();
                    return bytes.ToArray();
                case "char":
                    IEnumerable<char> chars = arrays.Cast<char>();
                    return chars.ToArray();
                // wtf is datetime? Convert.ToDateTime();

                default:
                    IEnumerable<object> obj = arrays.Cast<object>();
                    return obj.ToArray();
            }
        }
        private static dynamic HexInt32(string value)
        {
            return value.Contains("0x") ? Convert.ToInt32(value, 16) : Convert.ToInt32(value);
        }

        private static dynamic HexInt64(string value)
        {
            return value.Contains("0x") ? Convert.ToInt64(value, 16) : Convert.ToInt64(value);
        }

        private static dynamic HexUInt32(string value)
        {
            return value.Contains("0x") ? Convert.ToUInt32(value, 16) : Convert.ToUInt32(value);
        }

        private static dynamic HexUInt64(string value)
        {
            return value.Contains("0x") ? Convert.ToUInt64(value, 16) : Convert.ToUInt64(value);
        }

        public static dynamic GetType(string type, dynamic value)
        {
            if (value is string st)
            {
                value = st == "null" ? null! : st;
            }
            if (value == null)
            {
                return null!;
            }
            if (((object)value).GetType() == typeof(object[]) || ((object)value).GetType() == typeof(int[]))
            {
                return ConvertToArrayType(type, value);
            }
            if (type.Contains("*"))
            {
                return "0x" + new IntPtr(Convert.ToInt64(value)).ToString("X");
            }
            if (type.Contains("int"))
            {
                try
                {
                    DataTable table = new();
                    dynamic result = table.Compute(value, "");
                    value = result;
                }
                catch
                {

                }
            }


            try
            {
                try
                {
                    if (type == "string" && value is IntPtr str)
                    {
                        value = StringHelper.GetString(str.ToInt64());
                    }
                }
                catch { }
                switch (Convert.ToString(type))
                {
                    case "nint":
                        return (nint)Convert.ToInt64(value);
                    case "IntPtr":
                        return value is not IntPtr ? new IntPtr(Convert.ToInt64(value)) : value;
                    case "object":
                        return value;
                    case "string":
                        return Convert.ToString(value); // No conversion needed
                    case "int32":
                        return HexInt32(value.ToString());
                    case "int64":
                        return HexInt64(value.ToString());
                    case "uint32":
                        return HexUInt32(value.ToString());
                    case "uint64":
                        return HexUInt64(value.ToString());
                    case "double":
                        return Convert.ToDouble(value);
                    case "bool":
                        return Convert.ToBoolean(value);
                    case "byte":
                        return Convert.ToByte(value);
                    case "char":
                        return Convert.ToChar(value);
                    case "datetime":
                        return Convert.ToDateTime(value);
                    case "decimal":
                        return Convert.ToDecimal(value);
                    case "float":
                        return Convert.ToSingle(value);
                    case "sbyte":
                        return Convert.ToSByte(value);
                    case "short":
                        return Convert.ToInt16(value);
                    case "ushort":
                        return Convert.ToUInt16(value);
                    default:
                        ErrorHandler.Send("", $"The type '{type}' is not supported.");
                        return 0;
                }

            }
            catch (Exception e)
            {
                ErrorHandler.Send(e.Message, value ?? "Null");
            }
            return 0;
        }
    }
}