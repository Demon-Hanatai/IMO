using System.Text.RegularExpressions;

namespace Lexer
{
    public static class ArraysHandler
    {
        public static void HandleArrayOperation(string code)
        {
            Regex arrayDeclarationPattern = new(@"\s*(\w+)\s*\[\s*\]\s*(\w+)\s*->\s*\[(.*)\]\s*");
            Regex arrayElementAccessPattern = new(@"\s*(\w+)\s*\[\s*(\d+)\s*\]\s*->\s*(.*)");

            if (arrayDeclarationPattern.IsMatch(code))
            {
                Match match = arrayDeclarationPattern.Match(code);
                DeclareArray(match.Groups[2].Value, match.Groups[1].Value, match.Groups[3].Value);
            }
            else if (arrayElementAccessPattern.IsMatch(code))
            {
                Match match = arrayElementAccessPattern.Match(code);
                AccessOrSetArrayElement(match.Groups[1].Value, int.Parse(match.Groups[2].Value), match.Groups[3].Value);
            }
        }

        private static void DeclareArray(string name, string type, string elements)
        {
            string[] elementList = elements.Split(',').Select(el => el.Trim()).ToArray();
            MemoryHandler.Memorys[name] = Convertor.ConvertToArrayType(type, elementList);
        }

        private static void AccessOrSetArrayElement(string name, int index, string value)
        {
            if (!MemoryHandler.Memorys.ContainsKey(name))
            {
                ErrorHandler.Send("Array Not Found Error", $"The array '{name}' has not been declared.");
                return;
            }

            if (MemoryHandler.Memorys[name] is not Array array)
            {
                ErrorHandler.Send("Type Mismatch Error", $"The identifier '{name}' is not an array.");
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                Console.WriteLine(array.GetValue(index));
            }
            else
            {
                array.SetValue(Convert.ChangeType(value, conversionType: array.GetType().GetElementType()), index);
                MemoryHandler.Memorys[name] = array;
            }
        }


    }
}
