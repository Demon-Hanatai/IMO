using System.Text.RegularExpressions;

namespace Lexer
{
    public static class ConditionsHandler
    {
        public static bool Run(string code, int currentcount = 0)
        {
            code = code.Replace("*34M#", "\n");
            string[] tokens = code.Trim().Split(' ');
            if (tokens[0].Trim().StartsWith("if"))
            {
                //remove if from the code and send it to the if handler
                return Ifhandler(code.Trim().TrimStart().TrimEnd(), currentcount);

            }
            return true;
        }

        private static bool Ifhandler(string code, int currentcount = 0)
        {
            bool Success = false;
            Regex Regex = new(@"\s*(if\s*\((.*)\s*(!=|==|<=|>=|<|>)\s*(.*)\)\s*{\s*([\s\S]+?)\s*})|(elif\s*\((.*)\s*(!=|==|<=|>=|<|>)\s*(.*)\)\s*{\s*([\s\S]+?)\s*})|(else\s*{\s*([\s\S]+?)\s*})");
            MatchCollection Match = Regex.Matches(code);

            if (Match.Count > 0)
            {
                Match m_;
                for (; currentcount < Match.Count; currentcount++)
                {

                    Regex __ = new(@"\s*\(\s*(.*)\s*(!=|==|<=|>=|<<|>>)\s*(.*)\s*\)");
                    m_ = __.Match(Match[0].Groups[0].Value);
                    string oldcode = Match[0].Groups[0].Value;
                    if (!m_.Success)
                    {
                        ErrorHandler.Send(reason: "Invaild if token", message: code);
                    }
                    string leftopr = m_.Groups[1].Value;
                    string condition = m_.Groups[2].Value;
                    string rightopr = m_.Groups[3].Value;
                    dynamic? GetValue = default;
                    string FirstOptype = "object";
                    string LastOptype = "object";
                    if (leftopr.Contains("="))
                    {
                        MemoryHandler.WatchForChanges();
                        _ = ValueHandler.Run(leftopr);
                        Regex Rege = new($"\\s*({StringHelper.AllTypes})\\s+(.*)\\s*=\\s*(.*)\\s*");
                        Match Match_ = Rege.Match(leftopr);
                        GetValue = ValueHandler.Run(Match_.Groups[1].Value + " <- " + Match_.Groups[2].Value);
                        FirstOptype = Match_.Groups[1].Value;
                        MemoryHandler.StopWatcher = true;
                        MemoryHandler.RemoveLastChangesFromMemory();
                        if (GetValue is string)
                        {
                            GetValue = GetValue.TrimEnd();
                        }

                    }
                    else if (leftopr.Contains("<-")||leftopr.Contains("!extern"))
                    {
                        Regex regex = new($@"\s{{0,}}({StringHelper.AllTypes})\s*<-\s*(\D+\w*)\s*");
                        Match match = regex.Match(code);
                        if (match.Success)
                        {
                            FirstOptype = match.Groups[1].Value;
                        }
                        GetValue = ValueHandler.Run(leftopr);
                    }
                    else
                    {
                        _ = Convertor.GetType(leftopr, rightopr);



                    }
                    dynamic GetSecoValue =default;
                    if (rightopr.Contains("="))
                    {
                        MemoryHandler.WatchForChanges();
                        _ = ValueHandler.Run(rightopr);
                        Regex Rege = new($"\\s*({StringHelper.AllTypes})\\s+(.*)\\s*=\\s*(.*)\\s*");
                        Match Match_ = Rege.Match(rightopr);
                        GetSecoValue = ValueHandler.Run(Match_.Groups[1].Value + " <- " + Match_.Groups[2].Value);
                        _ = Match_.Groups[1].Value;
                        MemoryHandler.StopWatcher = true;
                        MemoryHandler.RemoveLastChangesFromMemory();
                        if (GetSecoValue is string)
                        {
                            GetSecoValue = GetSecoValue.TrimEnd();
                        }


                    }
                    else if (rightopr.Contains("<-") || rightopr.Contains("!extern"))
                    {
                        Regex regex = new($@"\s{{0,}}({StringHelper.AllTypes})\s*<-\s*(\D+\w*)\s*");
                        Match match = regex.Match(code);
                        if (match.Success)
                        {
                            LastOptype = match.Groups[1].Value;
                        }
                        GetSecoValue = ValueHandler.Run(rightopr);
                    }
                    else
                    {
                        GetSecoValue = rightopr.Contains("<-") ? ValueHandler.Run(rightopr) : Convertor.GetType(FirstOptype, rightopr);
                    }

                    Regex replace = new(@".*{([\s\S]*?)}");

                    MatchCollection m = replace.Matches(code);

                    if (m.Count > 0)
                    {
                        code = m[0].Groups[1].Value.TrimStart();

                    }
                    switch (condition)
                    {
                        case "!=":
                            if (GetValue != GetSecoValue)
                            {
                                Lexer.Run(code);
                                return true;
                            }

                            break;
                        case "==":
                            if (GetValue == GetSecoValue)
                            {
                                Lexer.Run(code);
                                return true;
                            }

                            break;
                        case ">=":
                            if (GetValue >= GetSecoValue)
                            {
                                Lexer.Run(code);
                                return true;
                            }

                            break;
                        case "<=":
                            if (GetValue <= GetSecoValue)
                            {
                                Lexer.Run(code);
                                return true;
                            }

                            break;
                        case ">>":
                            if (GetValue > GetSecoValue)
                            {
                                Lexer.Run(code);
                                return true;
                            }

                            break;
                        case "<<":
                            if (GetValue < GetSecoValue)
                            {
                                Lexer.Run(code);
                                return true;
                            }

                            break;
                        default:
                            Lexer.Run(code);
                            break;
                    }
                    if (Success)
                    {

                        return true;
                    }
                    else
                    {
                        try
                        {
                            List<string> values = [];
                            string code_ = "";
                            for (int s = currentcount; s < Match.Count; s++)
                            {
                                if (!string.IsNullOrEmpty(Match[s].Value) && Match[s].Value != oldcode)
                                {
                                    values.Add(Match[s].Value);

                                }
                            }
                            if (values[0].Trim().StartsWith("else"))
                            {
                                Regex r = new(@".*{([\s\S]*?)}");
                                if (r.IsMatch(values[0]))
                                {
                                    Lexer.Run(r.Match(values[0]).Groups[1].Value);
                                }
                                else
                                {
                                    ErrorHandler.Send(code_, "Invaild else token");
                                }

                                return true;
                            }
                            else if (values[0].Trim().StartsWith("elif"))
                            {
                                foreach (string item in values)
                                {

                                    code_ += item + "\n";


                                }
                                code = code_.TrimStart().Trim().TrimEnd();


                                code = "if" + code.Remove(code.IndexOf("elif"), "elif".Length);


                                return Run(code.Trim());
                            }
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }

            }
            else
            {
                ErrorHandler.Send(reason: "Invaild if token", message: code);
            }
            return true;
        }
    }
}
