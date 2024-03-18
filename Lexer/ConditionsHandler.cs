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
            else if (tokens[0].Trim().StartsWith("while"))
            {
                //remove if from the code and send it to the if handler
                return WhileHandler(code.Trim().TrimStart().TrimEnd());

            }
            return true;
        }
        private static bool WhileHandler(string code)
        {
            Regex Regex = new(@"while\s*\((.*)\s*(!=|==|<=|>=|<|>)\s*(.*)\)\s*(:\s*\d*)?\s*{\s*([\n\s\w\d\S\s]*)\s*}");
            MatchCollection Match = Regex.Matches(code);

            if (Match.Count > 0)
            {
                Match m_;

                Regex __ = new(@"\s*\(\s*(.*)\s*(!=|==|<=|>=|<<|>>)\s*(.*)\s*\)");
                m_ = __.Match(Match[0].Groups[0].Value);
                _ = Match[0].Groups[0].Value;
                if (!m_.Success)
                {
                    ErrorHandler.Send(reason: "Invaild if token", message: code);
                }
                string leftopr = m_.Groups[1].Value;
                string condition = m_.Groups[2].Value;
                string rightopr = m_.Groups[3].Value;
                string[] value_ = new string[2];
                dynamic? GetValue = default;
                string FirstOptype = "object";
                if (leftopr.Contains("="))
                {
                    MemoryHandler.WatchForChanges();
                    _ = ValueHandler.Run(leftopr);
                    Regex Rege = new($"\\s*({StringHelper.AllTypes})\\s+(.*)\\s*=\\s*(.*)\\s*");
                    Match Match_ = Rege.Match(leftopr);

                    GetValue = ValueHandler.Run(Match_.Groups[1].Value + " <- " + Match_.Groups[2].Value);
                    value_[0] = Match_.Groups[1].Value + " <- " + Match_.Groups[2].Value;
                    FirstOptype = Match_.Groups[1].Value;
                    MemoryHandler.StopWatcher = true;
                    MemoryHandler.RemoveLastChangesFromMemory();
                    if (GetValue is string)
                    {
                        GetValue = GetValue.TrimEnd();
                    }

                }
                else if (leftopr.Contains("<-") || leftopr.Contains("!extern"))
                {
                    Regex regex = new($@"\s{{0,}}({StringHelper.AllTypes})\s*<-\s*(\D+\w*)\s*");
                    Match match = regex.Match(code);
                    if (match.Success)
                    {
                        FirstOptype = match.Groups[1].Value;
                    }
                    value_[0] = leftopr;
                    GetValue = ValueHandler.Run(leftopr);
                }

                else
                {
                    GetValue = Convertor.GetType(!StringHelper.AllTypes.Split("|").Any(x => x == leftopr.Trim()) ? "string" : leftopr, leftopr);

                }
                dynamic GetSecoValue;
                if (rightopr.Contains("="))
                {
                    MemoryHandler.WatchForChanges();
                    _ = ValueHandler.Run(rightopr);
                    Regex Rege = new($"\\s*({StringHelper.AllTypes})\\s+(.*)\\s*=\\s*(.*)\\s*");
                    Match Match_ = Rege.Match(rightopr);
                    value_[2] = Match_.Groups[1].Value + " <- " + Match_.Groups[2].Value;
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
                        _ = match.Groups[1].Value;
                    }
                    value_[1] = rightopr;
                    GetSecoValue = ValueHandler.Run(rightopr);
                }
                else
                {


                    GetSecoValue = rightopr.Contains("<-") ? ValueHandler.Run(rightopr) : Convertor.GetType(!StringHelper.AllTypes.Split("|").Any(x => x == FirstOptype.Trim()) ? "string" : FirstOptype, rightopr);
                }

                Regex replace = new(@"{\s*([\n\s\w\d\S\s]*)\s*}");

                MatchCollection m = replace.Matches(code);

                if (m.Count > 0)
                {
                    code = m[0].Groups[1].Value.TrimStart();

                }
                void Code()
                {
                    

                    if (leftopr.Contains("<-") || leftopr.Contains("!extern"))
                    {
                        Regex regex = new($@"\s{{0,}}({StringHelper.AllTypes})\s*<-\s*(\D+\w*)\s*");
                        Match match = regex.Match(code);
                        if (match.Success)
                        {
                            FirstOptype = match.Groups[1].Value;
                        }
                        value_[0] = leftopr;
                        GetValue = ValueHandler.Run(leftopr);
                    }
                    else
                    {
                        GetValue = Convertor.GetType(!StringHelper.AllTypes.Split("|").Any(x => x == leftopr.Trim()) ? "string" : leftopr, leftopr);
                    }
                    if (rightopr.Contains("<-") || rightopr.Contains("!extern"))
                    {
                        Regex regex = new($@"\s{{0,}}({StringHelper.AllTypes})\s*<-\s*(\D+\w*)\s*");
                        Match match = regex.Match(code);
                        if (match.Success)
                        {
                            _ = match.Groups[1].Value;
                        }
                        value_[1] = rightopr;
                        GetSecoValue = ValueHandler.Run(rightopr);
                    }
                    else
                    {
                        GetSecoValue = rightopr.Contains("<-") ? ValueHandler.Run(rightopr) : Convertor.GetType(!StringHelper.AllTypes.Split("|").Any(x => x == FirstOptype.Trim()) ? "string" : FirstOptype, rightopr);
                    }

                    Regex replace = new(@"{\s*([\n\s\w\d\S\s]*)\s*}");

                    MatchCollection m = replace.Matches(code);

                    if (m.Count > 0)
                    {
                        code = m[0].Groups[1].Value.TrimStart();

                    }

                    Lexer.Run(code.Trim());



                }
                switch (condition)
                {
                    case "!=":
                        while (GetValue != GetSecoValue)
                        {
                            Code();
                        }


                        break;
                    case "==":
                        while (GetValue == GetSecoValue)
                        {
                            Code();
                        }

                        break;
                    case ">=":
                        if (GetValue >= GetSecoValue)
                        {
                            Code();
                        }

                        break;
                    case "<=":
                        while (GetValue <= GetSecoValue)
                        {
                            Code();
                        }

                        break;
                    case ">>":
                        while (GetValue > GetSecoValue)
                        {
                            Code();
                        }

                        break;
                    case "<<":
                        while (GetValue < GetSecoValue)
                        {
                            Code();
                        }

                        break;

                }


            }
            return true;

        }
        private static bool Ifhandler(string code, int currentcount = 0)
        {
            bool Success = false;
            Regex Regex = new(@"\s*(if\s*\((.*)\s*(!=|==|<=|>=|<|>)\s*(.*)\)\s*{\s*([\n\s\w\d\S\s]*?)\s*})|(elif\s*\((.*)\s*(!=|==|<=|>=|<|>)\s*(.*)\)\s*{\s*([\n\s\w\d\S\s]*?)\s*})|(else\s*{\s*([\n\s\w\d\S\s]*?)\s*})");
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
                    else if (leftopr.Contains("<-") || leftopr.Contains("!extern"))
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
                    dynamic GetSecoValue;
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
                            _ = match.Groups[1].Value;
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
                    List<string> values = [];
                    string code_ = "";
                    try
                    {


                        for (int s = currentcount; s < Match.Count; s++)
                        {
                            if (!string.IsNullOrEmpty(Match[s].Value) && Match[s].Value != oldcode)
                            {
                                values.Add(Match[s].Value);

                            }
                        }

                    }
                    catch
                    {

                    }
                    if (Success && !values[0].StartsWith("if"))
                    {

                        return true;
                    }
                    else
                    {
                        try
                        {

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
                            else if (values[0].Trim().StartsWith("if"))
                            {
                                foreach (string item in values)
                                {

                                    code_ += item + "\n";

                                }

                                return Run(code_);
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
