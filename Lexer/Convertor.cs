namespace Lexer;
public class Convertor
{
    public static dynamic ConvertToArrayType(string value, object[] arrays)
    {
        switch(value)
        {
            case "string":
            // k //use IEnumerable<objecttype> instead of objectytpe[]...
            //pc might die anytime k
            case "int32":
                IEnumerable<int> ints = arrays.Cast<int>();
                return ints.ToArray(); // k
            default:
                ErrorHandler.Send("Arrays", ".....");
                Environment.Exit(0);
                return 0;
        }
    }
    public static dynamic GetType(string type, string value)
    {
        switch (type.ToLower())
        {
            case "string":
                return value; // No conversion needed
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
                Console.Write($"The type '{type}' is not supported.");
                return 0;
        }
    }
}