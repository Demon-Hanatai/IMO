using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lexer
{
    public static class ValueHandler
    {
        public static dynamic Run(string code)
        {
            Regex regex = new Regex($@"\s{{0,}}({StringHelper.AllTypes})\s{{1,}}(\D{{1,}}\w{{0,}})*\s{{0,}}(->|<-)\s{{0,}}([\w\d]{{0,}})\s{{0,}}");
            Match match = regex.Match(code);
            if (match.Success)
            {
                switch (match.Groups[3].Value)
                {
                    case "->":
                        SetValue(code);
                    break;
                    case "<-":
                        return GetValue(code);

                     
                    default:
                        string identifier = code.Split(' ')[0]; // Extract the first word as the identifier
                        ErrorHandler.Send(message: $"{identifier} Error", reason: $"{identifier} is not a valid name/type in this context. Please check for typos or refer to the documentation for allowed identifiers.");

                    break;
                }
            }
            else
                ErrorHandler.Send(message: code.Split(' ')[0], reason: "(Syntax Error)The provided code does not match the expected syntax for assignments or access operations.");

            return "null";
        }
        private static void SetValue(string code)
        {
            Regex regex = new Regex($@"\s{{0,}}({StringHelper.AllTypes})\s{{1,}}(\D{{1,}}\w{{0,}})\s{{0,}}->\s{{0,}}([\w\d\D\d]{{0,}})\s{{0,}}"); 
            Match match = regex.Match(code);
            if (match.Success)
            {
                if (!MemoryHandler.Memorys.ContainsKey(match.Groups[2].Value))
                {
                    if (match.Groups[1].Value == "string")
                        MemoryHandler.Memorys.Add(match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd(), StringHelper.AllocString(match.Groups[3].Value));
                    else
                        MemoryHandler.Memorys.Add(match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd(), Convertor.GetType(match.Groups[1].Value, match.Groups[3].Value));
                }

                else
                {
                    if (match.Groups[1].Value == "string")
                        MemoryHandler.Memorys[match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd()] = StringHelper.AllocString(match.Groups[3].Value);

                    else
                    {
                        MemoryHandler.Memorys[match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd()] = Convertor.GetType(match.Groups[1].Value.Replace(" ", null).Trim().TrimEnd(), match.Groups[3].Value.Replace(" ", null).Trim().TrimEnd());

                    }
                }
                
            }
        }
        private static object GetValue(string code)
        {
            Regex regex = new Regex($@"\s{{0,}}({StringHelper.AllTypes})\s{{0,}}<-(\D{{1,}}\w{{0,}})\s{{0,}}");
            Match match = regex.Match(code);
            if (match.Success)
            {
                if(MemoryHandler.Memorys.Any(x=> x.Key == match.Groups[2].Value.Trim()))
                {
                    if (match.Groups[1].Value == "string")
                    {
                        string m =  StringHelper.GetString(((IntPtr)MemoryHandler.Memorys.First(x => x.Key == match.Groups[2].Value.Replace(" ",null).Trim().TrimEnd()).Value).ToInt64());
                        return m;
                    }

                    else
                        return Convertor.GetType(match.Groups[1].Value.Replace(" ", null).Trim().TrimEnd(), (string)MemoryHandler.Memorys[match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd()]);
                }
                else
                    ErrorHandler.Send(message: "Undefined Identifier Error", reason: $"The identifier '{match.Groups[2].Value}' has not been declared or initialized. Please ensure that '{match.Groups[2].Value}' is declared before it is used.");


            }
            else
                ErrorHandler.Send(message: "Update Mismatch Error", reason: $"The update operation '{match.Groups[2].Value}' does not match expected patterns or types. Ensure the operation and value types are correct and compatible.");

            return 0;
        }
    }
}
