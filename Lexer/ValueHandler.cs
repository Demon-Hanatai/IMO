using System.Text.RegularExpressions;

namespace Lexer
{
    public static class ValueHandler
    {
        public static dynamic Run(string code)
        {
            Regex regex = new($@"(Add|Sub)\s+({StringHelper.AllTypes})<-\s*(\w+\d*)\s+(\w+\d*|\w*\d*|\""(.*)"")");
            Match match  = regex.Match(code);
            if (match.Success)
            {

                Operation(code);
            }

            else
            {
                regex = new($@".*(=|<-).*");
                match = regex.Match(code.Split('\n')[0]);
                if (match.Success)
                {
                    switch (match.Groups[1].Value)
                    {
                        case "=":
                            SetValue(code);
                            break;

                        case "<-":
                            return GetValue(code);


                        default:
                            string identifier = code.Split(' ')[0];
                            ErrorHandler.Send(message: $"{identifier} Error", reason: $"{identifier} is not a valid name/type in this context. Please check for typos or refer to the documentation for allowed identifiers.");

                            break;
                    }
                }

                else
                {
                    ErrorHandler.Send(message: code.Split(' ')[0], reason: "(Syntax Error)The provided code does not match the expected syntax for assignments or access operations.");
                }


            }
            return null!;

        }
        private static void Operation(string code)
        {
            Regex regex = new($@"(Add|Sub)\s+({StringHelper.AllTypes})<-\s*(\w+\d*)\s+(\w+\d*|\w*\d*|\""(.*)"")");
            Match match = regex.Match(code);
            if (MemoryHandler.Memorys.ContainsKey(match.Groups[3].Value.Trim()))
            {
                if (match.Groups[1].Value == "Add")
                {
                    if (match.Groups[2].Value.Trim()=="string")
                    {

                        var Value = GetValue($"{match.Groups[2].Value} <- {match.Groups[3].Value}");
                        MemoryHandler.Memorys[match.Groups[3].Value.Trim()] =Value+ Convertor.GetType(match.Groups[2].Value, match.Groups[5].Value);
                    }
                    else
                    {
                        MemoryHandler.Memorys[match.Groups[3].Value.Trim()] += Convertor.GetType(match.Groups[2].Value, match.Groups[4].Value);
                    }
                 
                }
                if (match.Groups[1].Value == "Sub")
                {
                    if (match.Groups[2].Value.Trim() != "string")
                    {
                        var Value = GetValue($"{match.Groups[2].Value} <- {match.Groups[3].Value}");
                        MemoryHandler.Memorys[match.Groups[3].Value.Trim()] -= Convertor.GetType(match.Groups[2].Value, match.Groups[4].Value);
                    }
                    else
                    {
                        ErrorHandler.Send(match.Groups[2].Value, "Invalid Operation Error. The operation 'Sub' is not supported for the type 'string'.");
                    }
                }
            }
            else
            {
                ErrorHandler.Send(code, "Undefined Identifier Error.The provided code does not match the expected syntax for assignments or access operations.");
            }
        }
        private static void SetValue(string code)
        {
            

            Regex regex = new($@"\s{{0,}}({StringHelper.AllTypes})\s{{1,}}(\D{{1,}}\w{{0,}})\s{{0,}}=\s{{0,}}([\w\d\D\d]{{0,}})\s{{0,}}");
            Match match = regex.Match(code);
            if (code.Contains("=") && code.Contains("[") && code.Contains("]"))
            {

                Regex arrayRegex = new($@"\s*({StringHelper.AllTypes})\s+(\w+)\s*=\s*\s*(\[.*\])\s*\s*");
                Match arrayMatch = arrayRegex.Match(code);
                if (arrayMatch.Success)
                {

                    string type = arrayMatch.Groups[1].Value;
                    string identifier = arrayMatch.Groups[2].Value;
                    string value = arrayMatch.Groups[3].Value;
                    string[] elements = value.TrimStart('[').TrimEnd(']').Split(',');
                    for (int i = 0; i < elements.Length; i++)
                    {
                        Regex _regex = new(@"""([^""""]*)""");
                        Match _match = _regex.Match(elements[i]);
                        if (_match.Success)
                        {
                            elements[i] = _match.Groups[1].Value;
                        }

                        if (MemoryHandler.Memorys.ContainsKey(elements[i]))
                        {
                            if (elements[i].GetType() == typeof(string) && elements[i].GetType() != typeof(int))
                            {
                                try
                                {
                                    elements[i] = StringHelper.GetString((IntPtr)MemoryHandler.Memorys[elements[i]]);
                                }
                                catch
                                {
                                    //Meaning the value is not a pointer

                                    elements[i] = Convertor.GetType("string", MemoryHandler.Memorys[elements[i]]);
                                }
                            }
                            else
                            {
                                elements[i] = Convertor.GetType("string", MemoryHandler.Memorys[elements[i]]);
                            }
                        }
                    }

                    dynamic[] convertedElements = elements.Select(el => Convertor.GetType(type, el)).ToArray();

                    MemoryHandler.Memorys[identifier] = convertedElements;
                    return;
                }
                regex = new Regex($@"\s*({StringHelper.AllTypes})\s+(\w+)\s*=\s*(\w+)[[](\d+)[]]");
                arrayMatch = regex.Match(code);
                if (arrayMatch.Success)
                {


                    object GetValue = ((object[])MemoryHandler.Memorys[arrayMatch.Groups[3].Value])[Convertor.GetType("int32", arrayMatch.Groups[4].Value)];
                    GetValue = arrayMatch.Groups[1].Value == "string" ? "\"" + GetValue + "\"" : GetValue;
                    SetValue($"{arrayMatch.Groups[1].Value} {arrayMatch.Groups[2].Value}={GetValue}");

                }
            }
            Regex NestedValue = new($@"\s*({StringHelper.AllTypes})\s*(\w+\d*)\s*(->|=)\s*(.*)\.(.*)!extern");
            Match NestedValue_match = NestedValue.Match(code);

            if (NestedValue_match.Success)
            {
                bool isNotADirectValue = false;
                string ScopeCreatedValueName = "";
                if (!MemoryHandler.Memorys.ContainsKey(NestedValue_match.Groups[2].Value.Trim().Replace(" ", null)))
                {
                    SetValue($"{NestedValue_match.Groups[1].Value} {NestedValue_match.Groups[2].Value} = 0 ");
                }
                dynamic? Value = default;
                if (NestedValue_match.Groups[0].Value.Contains("->"))
                {
                    Regex Regex = new($"\\s*({StringHelper.AllTypes})\\s*->\\s*([\\w\\d\\S]*)");
                    var  m = Regex.Match(NestedValue_match.Groups[4].Value.Trim());
                    if (m.Success)
                    {
                        string RandomIDname()
                        {
                            var _ = "";
                            for (int i = 0; i < 10; i++)
                            {
                                _ += "1234567890AB234823943"[new Random().Next(0, "1234567890AB234823943".Length - 1)];
                            }
                            return _!;
                        }
                        //isstring
                        Regex _regex = new(@"""([^""""]*)""");
                        Match _match = _regex.Match(match.Groups[3].Value);
                        //end
                        ScopeCreatedValueName = m.Groups[2].Value + RandomIDname();
                        SetValue($"{m.Groups[1].Value} {ScopeCreatedValueName.Trim()} = {(_match.Success ? _match.Groups[1].Value : m.Groups[2].Value)} ");
                        isNotADirectValue = true;
                    }
                    else
                    {


                        ErrorHandler.Send(message: "invaild tokens", NestedValue_match.Groups[4].Value.Trim());
                    }

                }
                void RemoveKey()
                {
                    if (MemoryHandler.Memorys.ContainsKey(ScopeCreatedValueName))
                    {
                        _ = MemoryHandler.Memorys.Remove(ScopeCreatedValueName);
                    }
                }
                Value = GetValue("object <- " + (isNotADirectValue ? ScopeCreatedValueName : NestedValue_match.Groups[4].Value.Trim()));

                if (NestedValue_match.Groups[5].Value.Trim().EndsWith("()"))
                {
                    string methodName = NestedValue_match.Groups[5].Value.Trim().TrimEnd('(', ')');
                    dynamic? method = Value?.GetType().GetMethod(methodName, Type.EmptyTypes);

                    if (method != null)
                    {
                        SetValue($"{NestedValue_match.Groups[1].Value} {NestedValue_match.Groups[2].Value} = {method.Invoke(Value, null)}");
                        RemoveKey();
                        return;
                    }
                    else
                    {
                        ErrorHandler.Send(methodName, $"The method '{methodName}' does not exist in the type '{Value?.GetType().Name}'.");
                        RemoveKey();
                        return;
                    }
                }
                else
                {
                    dynamic? property = Value?.GetType().GetProperty(NestedValue_match.Groups[5].Value.Trim());
                    if (property != null)
                    {
                        SetValue($"{NestedValue_match.Groups[1].Value} {NestedValue_match.Groups[2].Value} = {property.GetValue(Value, null)}");
                        RemoveKey();
                        return;
                    }
                }
            }
            else if (match.Success)
            {
                if (!MemoryHandler.Memorys.ContainsKey(match.Groups[2].Value.Trim().Replace(" ", null)))
                {

                    if (MemoryHandler.Memorys.ContainsKey(match.Groups[2].Value.Trim()))
                    {
                        Regex _regex = new(@"""([^""""]*)""");
                        Match _match = _regex.Match(match.Groups[3].Value);
                        if (match.Groups[1].Value == "string")
                        {
                            try
                            {

                                MemoryHandler.Memorys.Add(match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd(), StringHelper.AllocString(StringHelper.GetString((IntPtr)MemoryHandler.Memorys[match.Groups[3].Value.Trim()])));
                                return;
                            }
                            catch
                            {

                                MemoryHandler.Memorys.Add(match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd(), StringHelper.AllocString(Convert.ToString(_match.Success ? _match.Groups[1].Value : MemoryHandler.Memorys[match.Groups[3].Value.Trim()])));
                                return;
                            }
                            finally
                            {

                            }
                        }

                        MemoryHandler.Memorys.Add(match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd(), _match.Success ? _match.Groups[1].Value : MemoryHandler.Memorys[match.Groups[3].Value.Trim()]);
                        return;
                    }

                    if (match.Groups[1].Value == "string")
                    {
                        Regex _regex = new(@"""([^""""]*)""");
                        Match _match = _regex.Match(match.Groups[3].Value);
                        if (_match.Success)
                        {
                            MemoryHandler.Memorys.Add(match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd(), StringHelper.AllocString(_match.Groups[1].Value));

                        }
                        else
                        {
                            MemoryHandler.Memorys.Add(match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd(), StringHelper.AllocString(match.Groups[3].Value));

                        }


                    }
                    else
                    {
                        MemoryHandler.Memorys.Add(match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd(), Convertor.GetType(match.Groups[1].Value, match.Groups[3].Value));
                    }
                }

                else
                {
                    Regex _regex = new(@"""([^""""]*)""");
                    Match _match = _regex.Match(match.Groups[3].Value);



                    MemoryHandler.Memorys[match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd()] = match.Groups[1].Value == "string"
                        ? StringHelper.AllocString(_match.Success ? _match.Groups[3].Value : match.Groups[3].Value)
                        : (object)Convertor.GetType(match.Groups[1].Value.Replace(" ", null).Trim().TrimEnd(), match.Groups[3].Value.Replace(" ", null).Trim().TrimEnd());
                }

            }

        }
        private static object? GetValue(string code)
        {
            code = code.Replace("::", null);
            Regex regex = new($@"\s{{0,}}({StringHelper.AllTypes})\s*<-\s*(\D+\w*)(.*)*\s*");
            Match match = regex.Match(code);
            if (code.Contains("[") && code.Contains("]"))
            {
                regex = new Regex($@"\s{{0,}}({StringHelper.AllTypes})\s{{0,}}<-\s*(\w+)((?:[[])\d+(?:[]]))");
                match = regex.Match(code);
                if (match.Success)
                {
                    int index = Convertor.GetType("int32", match.Groups[3].Value.Replace("[", null).Replace("]", null));
                    object[] Array = (object[])Convertor.ConvertToArrayType(match.Groups[1].Value, GetValue($"{match.Groups[1].Value} <-{match.Groups[2].Value}") as object[]);
                    dynamic? Value = default;
                    Value = ((object[])Convertor.ConvertToArrayType(match.Groups[1].Value, Array))[index];

                    return Value;

                }
            }
            Regex NestedValue = new($@"\s*({StringHelper.AllTypes})\s*(->|<-)\s*([\w\d\S]*)\.([\w\d\S]*)");
            Match NestedValue_match = NestedValue.Match(code.Replace("!extern", " "));
            if (NestedValue_match.Success)
            {

                if (NestedValue_match.Success)
                {
                    dynamic? Value = default;
                    if (NestedValue_match.Groups[2].Value == "<-")
                    {
                        Value = GetValue(NestedValue_match.Groups[1].Value + " <- " + NestedValue_match.Groups[3].Value.Trim());
                    }
                    else
                    {
                        //string start
                        Regex _regex = new(@"""([^""""]*)""");
                        Match _match = _regex.Match(match.Groups[3].Value);
                        //is string end
                        Value = Convertor.GetType(NestedValue_match.Groups[1].Value, _match.Success ? _match.Groups[1].Value : NestedValue_match.Groups[3].Value);
                    }

                    if (NestedValue_match.Groups[4].Value.Trim().EndsWith("()"))
                    {
                        string methodName = NestedValue_match.Groups[4].Value.Trim().TrimEnd('(', ')');


                        dynamic? method = Value?.GetType().GetMethod(methodName, Type.EmptyTypes);


                        if (method != null)
                        {
                            return method.Invoke(Value, null);

                        }
                        else
                        {
                            ErrorHandler.Send(methodName, $"The method '{methodName}' does not exist in the type '{Value?.GetType().Name}'.");
                        }
                    }
                    else
                    {
                        return Value?.GetType().GetProperty(NestedValue_match.Groups[4].Value.Trim())?.GetValue(Value);
                    }


                }
                else
                {

                    ErrorHandler.Send(code, "Invalid Identifier Error.The provided code does not match the expected syntax for assignments or access operations.");
                }
            }
            else if (match.Success)
            {
                if (MemoryHandler.Memorys.Any(x => x.Key == match.Groups[2].Value.Trim()))
                {
                    if (match.Groups[1].Value == "string")
                    {
                        dynamic m = MemoryHandler.Memorys.First(x => x.Key == match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd()).Value;
                        try
                        {
                            string text = StringHelper.GetString(((IntPtr)m).ToInt64());
                            return text;

                        }
                        catch { return m; }
                    }
                    else if (match.Groups[1].Value.Contains("*"))
                    {
                        dynamic m = MemoryHandler.Memorys.First(x => x.Key == match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd()).Value;
                        unsafe
                        {
                            return (long)&m;
                        }
                    }

                    else
                    {
                        dynamic @return = Convertor.GetType(match.Groups[1].Value.Replace(" ", null).Trim().TrimEnd(), MemoryHandler.Memorys[match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd()]);
                        if (@return.GetType() == typeof(double[]))
                        {
                            @return = Convertor.ConvertToArrayType("object", @return as object[]);
                        }

                        return @return;
                    }
                }
                else
                {
                    ErrorHandler.Send(message: "Undefined Identifier Error", reason: $"The identifier '{match.Groups[2].Value}' has not been declared or initialized. Please ensure that '{match.Groups[2].Value}' is declared before it is used.");
                }
            }

            else
            {
                ErrorHandler.Send(message: "Update Mismatch Error", reason: $"The update operation '{match.Groups[2].Value}' does not match expected patterns or types. Ensure the operation and value types are correct and compatible.");
            }

            return 0;
        }
    }
}