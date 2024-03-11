using System.Text.RegularExpressions;

namespace Lexer
{
    public static class ValueHandler
    {
        public static dynamic Run(string code)
        {
            Regex regex = new($@".*(=|<-).*");
            Match match = regex.Match(code.Split('\n')[0]);
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

            return "null";
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
                    dynamic[] convertedElements = elements.Select(el => Convertor.GetType(type, el.Trim())).ToArray();

                    MemoryHandler.Memorys[identifier] = convertedElements;
                    return;
                }
                regex = new Regex($@"\s*({StringHelper.AllTypes})\s+(\w+)\s*=\s*(\w+)[[](\d+)[]]");
                arrayMatch = regex.Match(code);
                if (arrayMatch.Success)
                {


                    object GetValue = ((object[])MemoryHandler.Memorys[arrayMatch.Groups[3].Value])[Convertor.GetType("int32", arrayMatch.Groups[4].Value)];
                    SetValue($"{arrayMatch.Groups[1].Value} {arrayMatch.Groups[2].Value}={GetValue}");

                }
            }
            var  NestedValue = new Regex($@"\s*({StringHelper.AllTypes})\s*(\w+\d*)\s*=\s*([\w\d\S]*)\.([\w\d\S]*)!extern");
            var NestedValue_match = NestedValue.Match(code);

            if (NestedValue_match.Success)
            {
                if (!MemoryHandler.Memorys.ContainsKey(NestedValue_match.Groups[2].Value.Trim().Replace(" ", null)))
                {
                    SetValue($"{NestedValue_match.Groups[1].Value} {NestedValue_match.Groups[2].Value} = 0 ");
                }
                var Value = GetValue("object <- " + NestedValue_match.Groups[3].Value.Trim());
                if (NestedValue_match.Groups[4].Value.Trim().EndsWith("()"))
                {
                    string methodName = NestedValue_match.Groups[4].Value.Trim().TrimEnd('(', ')');
                    var method = Value?.GetType().GetMethod(methodName, Type.EmptyTypes);

                    if (method != null)
                    {
                        SetValue($"{NestedValue_match.Groups[1].Value} {NestedValue_match.Groups[2].Value} = {method.Invoke(Value, null)}");
                        return;
                    }
                    else
                    {
                        ErrorHandler.Send(methodName, $"The method '{methodName}' does not exist in the type '{Value?.GetType().Name}'.");
                        return;
                    }
                }
                else
                {
                    var property = Value?.GetType().GetProperty(NestedValue_match.Groups[4].Value.Trim());
                    if (property != null)
                    {
                        SetValue($"{NestedValue_match.Groups[1].Value} {NestedValue_match.Groups[2].Value} = {property.GetValue(Value, null)}");
                        return;
                    }
                }
            }
            else if (match.Success)
            {
                if (!MemoryHandler.Memorys.ContainsKey(match.Groups[2].Value.Trim().Replace(" ", null)))
                {

                    if (MemoryHandler.Memorys.ContainsKey(match.Groups[3].Value.Trim()))
                    {
                        if (match.Groups[1].Value == "string")
                        {
                            try
                            {
                                MemoryHandler.Memorys.Add(match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd(), StringHelper.AllocString(StringHelper.GetString((IntPtr)MemoryHandler.Memorys[match.Groups[3].Value.Trim()])));
                                return;
                            }
                            catch
                            {
                                MemoryHandler.Memorys.Add(match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd(), StringHelper.AllocString(Convert.ToString(MemoryHandler.Memorys[match.Groups[3].Value.Trim()])));
                                return;
                            }
                            finally
                            {

                            }
                        }
                        MemoryHandler.Memorys.Add(match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd(), MemoryHandler.Memorys[match.Groups[3].Value.Trim()]);
                        return;
                    }

                    if (match.Groups[1].Value == "string")
                    {
                        MemoryHandler.Memorys.Add(match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd(), StringHelper.AllocString(match.Groups[3].Value));
                    }
                    else
                    {
                        MemoryHandler.Memorys.Add(match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd(), Convertor.GetType(match.Groups[1].Value, match.Groups[3].Value));
                    }
                }

                else
                {
                    MemoryHandler.Memorys[match.Groups[2].Value.Replace(" ", null).Trim().TrimEnd()] = match.Groups[1].Value == "string"
                        ? StringHelper.AllocString(match.Groups[3].Value)
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
            var NestedValue = new Regex($@"\s*({StringHelper.AllTypes})\s*<-\s*([\w\d\S]*)\.([\w\d\S]*)");
            var NestedValue_match = NestedValue.Match(code.Replace("!extern", " "));
            if (NestedValue_match.Success)
            {
              
                if (NestedValue_match.Success)
                {
                    var Value = GetValue(NestedValue_match.Groups[1].Value +" <- "+NestedValue_match.Groups[2].Value.Trim());
                    if (NestedValue_match.Groups[3].Value.Trim().EndsWith("()"))
                    {
                        string methodName = NestedValue_match.Groups[3].Value.Trim().TrimEnd('(', ')');

                       
                        var method = Value?.GetType().GetMethod(methodName, Type.EmptyTypes);

                      
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
                        return Value?.GetType().GetProperty(NestedValue_match.Groups[3].Value.Trim())?.GetValue(Value);
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