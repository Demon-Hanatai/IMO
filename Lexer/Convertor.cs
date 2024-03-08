using System.Data;

namespace Lexer;
public class Convertor
{
    public static object ConvertToArrayType(string value, object[] arrays)
    {
        switch (value.Replace("::", null))
        {
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
    public static dynamic GetType(string type, dynamic value)
    {
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
                if(value is IntPtr str)
                {
                    value = StringHelper.GetString(str);
                }
            }
            catch { }
            switch (Convert.ToString(type).ToLower())
            {
                case "object":
                    return value;
                case "string":
                    return Convert.ToString(value); // No conversion needed
                case "int32":
                    return Convert.ToInt32(value);
                case "int64":
                    return Convert.ToInt64(value);
                case "uint32":
                    return Convert.ToUInt32(value);
                case "uint64":
                    return Convert.ToUInt64(value);
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
            ErrorHandler.Send(e.Message, value);
        }
        return 0;
    }
}