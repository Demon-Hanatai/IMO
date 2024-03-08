using System.Text.RegularExpressions;

namespace Lexer
{
    public static class OperatorHandler
    {
        public static object Run(string code)
        {
            string[] parts = Regex.Split(code, $@"\s*({StringHelper.AllOperations})\s*");
            object? result = null;
            for (int i = 0; i < parts.Length; i++)
            {

            }

            return result;
        }
    }
}
