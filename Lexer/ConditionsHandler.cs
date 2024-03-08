using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Lexer
{
    public static class ConditionsHandler
    {
        public static void Run(string code)
        {
         var tokens = code.Trim().Split(' ');
           switch (tokens[0])
           {
           
                case "if":
                    Ifhandler(code.Replace("if ",null).Trim().TrimStart().TrimEnd());
                    break;

           }
        }
        private static void Ifhandler(string code)
        {
            var Regex = new Regex(@"\s*(.*)\s*(!=|==|<=|>=|<|>)\s*(.*)\s+endl(\s*.*\s*)endif");
            var Match = Regex.Match(code);
            if(Match.Success)
            {
                var leftopr = Match.Groups[1].Value;
                var Type = Match.Groups[2].Value;
                var rightopr = Match.Groups[3].Value;
                dynamic GetValue = default;
                dynamic GetSecoValue = default;
                string FirstOptype = null;
                string LastOptype = null;
                if (leftopr.Contains("="))
                {
                    MemoryHandler.WatchForChanges();

                    var SetValue = ValueHandler.Run(leftopr);
                    var Rege = new Regex($"\\s*({StringHelper.AllTypes})\\s+(.*)\\s*=\\s*(.*)\\s*");
                    var Match_ = Rege.Match(leftopr);
                    GetValue = ValueHandler.Run(Match_.Groups[1].Value + " <- " + Match_.Groups[2].Value);
                    FirstOptype = Match_.Groups[1].Value;
                    MemoryHandler.StopWatcher = true;
                    MemoryHandler.RemoveLastChangesFromMemory();
                    if (GetValue is string)
                    {
                        GetValue = GetValue.TrimEnd();
                    }

                }
                else
                {
                     GetValue = ValueHandler.Run(leftopr);

                }
                if (rightopr.Contains("="))
                {
                    MemoryHandler.WatchForChanges();

                    var SetValue = ValueHandler.Run(rightopr);
                    var Rege = new Regex($"\\s*({StringHelper.AllTypes})\\s+(.*)\\s*=\\s*(.*)\\s*");
                    var Match_ = Rege.Match(rightopr);
                    GetSecoValue = ValueHandler.Run(Match_.Groups[1].Value + " <- " + Match_.Groups[2].Value);
                    LastOptype = Match_.Groups[1].Value;
                    MemoryHandler.StopWatcher = true;
                    MemoryHandler.RemoveLastChangesFromMemory();
                    if (GetSecoValue is string)
                    {
                        GetSecoValue = GetSecoValue.TrimEnd();
                    }


                }
                else if(rightopr.Contains("<-")) 
                {
                    GetSecoValue = ValueHandler.Run(leftopr);
                }
                else
                {
                    GetSecoValue = Convertor.GetType(FirstOptype, rightopr);
                }
                code = code.Replace("endif", null);
                switch (Type)
                {
                    case "!=":
                        if (GetValue != GetSecoValue)
                            Lexer.Run(code);
                    break;
                    case "==":
                        if (GetValue ==  GetSecoValue)
                            Lexer.Run(code);
                        break;
                    case ">=":
                        if (GetValue >= GetSecoValue)
                            Lexer.Run(code);
                        break;
                    case "<=":
                        if (GetValue <= GetSecoValue)
                            Lexer.Run(code);
                        break;
                    case ">":
                        if (GetValue > GetSecoValue)
                            Lexer.Run(code);
                        break;
                    case "<":
                        if (GetValue < GetSecoValue)
                            Lexer.Run(code);
                        break;
                    default:
                        ErrorHandler.Send(Type, "invaild token");
                        break;
                }
            }
            else
            {
                ErrorHandler.Send(reason: "Invaild if token", message: code);
            }
        }
    }
}
