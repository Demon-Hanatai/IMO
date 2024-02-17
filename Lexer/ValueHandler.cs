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
        private static readonly Regex assignmentRegex = new Regex($@"\s*({StringHelper.AllTypes})\s+(\D\w*)\s*(->|<-)\s*([\w\d]+)\s*", RegexOptions.Compiled);
        private static readonly Regex valueSetRegex = new Regex($@"\s*({StringHelper.AllTypes})\s+(\D\w*)\s*->\s*([\w\d\D]+)\s*", RegexOptions.Compiled);
        private static readonly Regex valueGetRegex = new Regex($@"\s*({StringHelper.AllTypes})\s*<-(\D\w*)\s*", RegexOptions.Compiled);

        public static dynamic Run(string code)
        {
            var match = assignmentRegex.Match(code);
            if (!match.Success)
            {
                ErrorHandler.Send(code.Split(' ')[0], "(Syntax Error)The provided code does not match the expected syntax for assignments or access operations.");
                return "null";
            }

            switch (match.Groups[3].Value)
            {
                case "->":
                    SetValue(match);
                    break;
                case "<-":
                    return GetValue(match);
                default:
                    ErrorHandler.Send($"{match.Groups[2].Value} Error", $"{match.Groups[2].Value} is not a valid name/type in this context. Please check for typos or refer to the documentation for allowed identifiers.");
                    break;
            }

            return "null";
        }

        private static void SetValue(Match match)
        {
            string key = match.Groups[2].Value.Trim();
            if (!MemoryHandler.Memorys.ContainsKey(key))
            {
<<<<<<< HEAD
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
                        return Convertor.GetType(match.Groups[1].Value.Replace(" ", null).Trim().TrimEnd(), MemoryHandler.Memorys[match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd()]);
                }
                else
                    ErrorHandler.Send(message: "Undefined Identifier Error", reason: $"The identifier '{match.Groups[2].Value}' has not been declared or initialized. Please ensure that '{match.Groups[2].Value}' is declared before it is used.");


=======
                var value = match.Groups[1].Value == "string" ? StringHelper.AllocString(match.Groups[3].Value) : Convertor.GetType(match.Groups[1].Value, match.Groups[3].Value);
                MemoryHandler.Memorys.Add(key, value);
>>>>>>> 8df27a5ff0ad62a734c6c790badb04c4ce1ab3f3
            }
            else
            {
                var value = match.Groups[1].Value == "string" ? StringHelper.AllocString(match.Groups[3].Value) : Convertor.GetType(match.Groups[1].Value, match.Groups[3].Value);
                MemoryHandler.Memorys[key] = value;
            }
        }

        private static object GetValue(Match match)
        {
            string key = match.Groups[2].Value.Trim();
            if (MemoryHandler.Memorys.TryGetValue(key, out var memoryValue))
            {
                return match.Groups[1].Value == "string" ? StringHelper.GetString(((IntPtr)memoryValue).ToInt64()) : Convertor.GetType(match.Groups[1].Value, memoryValue.ToString());
            }

            ErrorHandler.Send("Undefined Identifier Error", $"The identifier '{key}' has not been declared or initialized. Please ensure that '{key}' is declared before it is used.");
            return 0;
        }
    }
}
