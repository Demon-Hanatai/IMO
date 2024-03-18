using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Lexer
{
    public partial class MethodHandler
    {
        public static void Run(string code)
        {

            Regex regex = new(
            @"(?:.method)\s{1,}(call::local|call|create|overwrite).{1,}");
            Match match = regex.Match(code);
            string[] j = code.Split("\n")[0].Split(" ");
            for (int i = 0; i < j.Length; i++)
            {
                j[i] = j[i] + " ";
            }
            for (int i = 0; i < j.Length; i++)
            {
                j[i] = j[i].Replace(".method ", null);

                break;
            }
            code = "";
            foreach (string item in j)
            {
                code += item;
            }
            code = code.Replace("*34M#9823", "\n");
            if (match.Success)
            {
                switch (match.Groups[1].Value)
                {
                    case "call":

                        code = code.Replace(".method", "");
                        CallMethod(code);
                        break;
                    case "if":
                        _ = ConditionsHandler.Run(code.Remove(code.IndexOf("if"), "if".Length).Trim().TrimStart().TrimEnd());
                        break;
                    case "call::local":
                        CallLocal(code);
                        break;
                    case "create":
                        CreateMethod(code);
                        break;
                    case "overwrite":
                        OverWriteMethod(code);
                        break;
                    default:
                        ErrorHandler.Send(code, match.Groups[1].Value + " instruction");
                        break;
                }
            }
            else
            {
                ErrorHandler.Send(code, "invaild Method syntax");
            }
        }
        public static List<Function> LocalFuntions = [];
        private static void CallLocal(string code)
        {
            Regex regex = new(@"\s*call::local\s+(\w+)\s*\(.*\)");
            Match match = regex.Match(code);
            if (match.Success)
            {
                int currentcount = 0;
                List<dynamic> SendPerms = [];

                MatchCollection Perms = Regex.Matches(Regex.Match(code, @"\((.*)\)").Groups[1].Value, $@"(Array::)?({StringHelper.AllTypes})\s{{0,}}(->|<-)\s{{0,}}([\w\d\s\+\-]{{1,}}([[]\d+[]])?)(::)?(?>\w{{0,}}),?");
                if(Perms.Count>0)
                {
                    if (Perms.Count > 0)
                    {


                        dynamic value = "";
                        for (int i = 0; i < Perms.Count; i++)
                        {
                            if (Perms[i].Groups[1].Value != "Array::")
                            {
                                ValueType_Get_Set(i);
                            }
                            void ValueType_Get_Set(int where)
                            {
                                Regex NestedValue = new($@"\s*({StringHelper.AllTypes})\s*<-\s*([\w\d\S]*)\.([\w\d\S]*)!extern");
                                Match NestedValue_match = NestedValue.Match(code);
                                if (NestedValue_match.Success)
                                {

                                    if (NestedValue_match.Success)
                                    {
                                        value = ValueHandler.Run(NestedValue_match.Groups[2].Value + " <- " + NestedValue_match.Groups[3].Value.Trim() + "." + NestedValue_match.Groups[3].Value.Trim());

                                    }
                                    else
                                    {
                                        ErrorHandler.Send(code, "Invalid Identifier Error.The provided code does not match the expected syntax for assignments or access operations.");
                                    }
                                }
                                else
                                {
                                    Regex _regex = new(@"""([^""""]*)""");
                                    Match _match = _regex.Match(Perms[where].Groups[4].Value);
                                    Regex _regex2 = new(@"(.*)\)");
                                    Match _match2 = _regex2.Match(Perms[where].Groups[4].Value);
                                    value = Perms[where].Groups[3].Value == "<-"
                                   ? ValueHandler.Run($"{Perms[where].Groups[2].Value} {Perms[where].Groups[3].Value} {(_match2.Success ? _match2.Groups[1].Value : Perms[where].Groups[4].Value)}")
                                   : _match.Success ? _match.Groups[^1].Value : Perms[where].Groups[4].Value;
                                }
                            }
                            if (Perms[i].Groups[1].Value == "Array::")
                            {

                                List<object> Arrays = [];
                                for (int s = i; s < Perms.Count; s++)
                                {
                                    ValueType_Get_Set(s);
                                    if (Perms[s].Groups[5].Value == "::")
                                    {
                                        Arrays.Add(Convertor.GetType(Perms[s].Groups[2].Value, value));
                                        SendPerms.Add(Convertor.ConvertToArrayType(Perms[s].Groups[2].Value, Arrays.ToArray()));

                                        i += Arrays.Count - 1;
                                        Arrays = [];
                                        break;
                                    }
                                    else
                                    {
                                        Arrays.Add(value.GetType() == typeof(int[]) ? value : Convertor.GetType(Perms[s].Groups[2].Value, value));
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    SendPerms.Add(value.GetType() == typeof(int[]) ? value : Convertor.GetType(Perms[i].Groups[2].Value, value));
                                }
                                catch (Exception ex)
                                {
                                    ErrorHandler.Send(code, ex.Message);

                                }
                            }
                        }

                    }
                }
                Function? LocalFunction = LocalFuntions.FirstOrDefault(x => x.FunctionName == match.Groups[1].Value
                && x.ParametersCount == Perms.Count && x.ParametersInformation.TrueForAll(pinfo =>
                {
                    if(pinfo.Type != Perms[currentcount].Groups[2].Value)
                    {
                        return false;
                    }
                    currentcount++;
                    return true;
                }));
                if (LocalFunction != null)
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
                    List<(string permName,string id)> LPreturnId = [];
                    for (int i = 0; i < LocalFunction.ParametersCount; i++)
                    {
                        var id ="A"+ RandomIDname();
                        LPreturnId.Add((LocalFunction.ParametersInformation[i].Name, id));
                        ValueHandler.Run($"{LocalFunction.ParametersInformation[i].Type} {id} = {SendPerms[i]}");
                    }
                    string oldCode = LocalFunction.Code;
                    var Per_ = Regex.Matches(LocalFunction.Code, $@"({StringHelper.AllTypes})\s*(\W[\W\w\d\D]*)\s*=\s*(.*)");
                    foreach (Match item in Per_)
                    {
                        LocalFunction.Code = LocalFunction.Code.Replace(item.Groups[0].Value, $"{item.Groups[1].Value} {LPreturnId.FirstOrDefault(x=>x.permName == item.Groups[2].Value).id.Trim()} = {item.Groups[3].Value}");
                    }
                    Per_ = Regex.Matches(LocalFunction.Code, $@"({StringHelper.AllTypes})\s*<-\s*([\w]*)");
                    
                    foreach (Match item in Per_)
                    {
                        LocalFunction.Code = LocalFunction.Code.Replace(item.Groups[0].Value, $"{item.Groups[1].Value} <- {LPreturnId.FirstOrDefault(x => x.permName.Trim() == item.Groups[2].Value.Trim()).id}");
                    }
                    string _ = null!;
                    foreach (var item in LocalFunction.Code.Split("\n"))
                    {
                        if (item != ""&&item!="\r" && item != "\r\n")
                        {
                            _+= item + "\n";
                        }
                    }
                    Lexer.Run(_.TrimEnd(), true);
                    LocalFunction.Code = oldCode;
                }
                else
                {
                    currentcount = 0;
                    if(!LocalFuntions.Any(x=>x.FunctionName == match.Groups[1].Value))
                    {
                        ErrorHandler.Send(match.Groups[0].Value, "Function as not be decleared;");
                    }
                    else if(!LocalFuntions.Any(x=>x.FunctionName == match.Groups[1].Value && x.ParametersCount == Perms.Count))
                    {
                        ErrorHandler.Send(match.Groups[0].Value, "Function as not be decleared;Parameters Count not match.");
                    }
                    else if(!LocalFuntions.Any(x=>x.FunctionName == match.Groups[1].Value &&
                    x.ParametersCount == Perms.Count && x.ParametersInformation.TrueForAll(pinfo => {
                        if (Perms[currentcount].Groups[1].Value != pinfo.Type)
                        {
                            return false;
                        }
                        currentcount++;
                        return true;
                    })))
                    {
                        ErrorHandler.Send(match.Groups[0].Value, "No function match this Parameter's");
                    }
                }
            }
        }
        private static void OverWriteMethod(string code)
        {
            Regex regex = new(@"\s*overwrite\s*create\s+(.+)\(\)\s*\{\s*([\s\S]*?)\s*\}");

            Match match = regex.Match(code);
            if (match.Success)
            {
                Function? LocalFunction = LocalFuntions.FirstOrDefault(x => x.FunctionName == match.Groups[1].Value);
                if (LocalFunction != null)
                {

                    LocalFunction.Code = match.Groups[2].Value;
                }
                else
                {
                    ErrorHandler.Send(match.Groups[0].Value, "Function as not be decleared;overwrite can't be use.");
                }
            }
        }
        private static void CreateMethod(string code)
        {

            Regex regex = new(@"\s*create\s+(\w+\d*)\s*\(.*\)\s*:\s*(Auto|[\d]*)\s*\{([\n\s\w\d\S\s]*)\}");
            Match match = regex.Match(code);
            if (match.Success is true)
            {
              
                MatchCollection Perms = Regex.Matches(Regex.Match(code, @"\((.*)\)").Groups[1].Value, $@"\s*({StringHelper.AllTypes})\s+(\W*\w*)\s*");
              
                List<(string type,string name)> ThisMethodPermInfo = new();
               

                if (match.Groups[2].Value != "Auto" && !uint.TryParse(match.Groups[2].Value, out _))
                {
                    ErrorHandler.Send(code, "Invalid Parameters Count");
                }
                if (Perms.Count > 0)
                {
                    

                    dynamic value = "";
                    for (int i = 0; i < (match.Groups[2].Value == "Auto" ? Perms.Count : int.Parse(match.Groups[2].Value)); i++)
                    {
                        if (Perms[i].Groups[1].Value == "Array::")
                        {
                            ErrorHandler.Send(code, "Array Type not allowed in method parameters");
                        }
                        if (i > Perms.Count && match.Groups[2].Value == "Auto")
                        {
                            break;
                        }
                        else if (i > Perms.Count)
                        {
                            ErrorHandler.Send(code, "OutofIndex Parameters/invaild type");
                        }
                        if (ThisMethodPermInfo.Any(x => x.name == Perms[i].Groups[2].Value))
                        { 
                            ErrorHandler.Send(code, "Duplicate Parameters name");
                        }
                        if (Regex.Match(Perms[i].Groups[2].Value,StringHelper.AllTypes).Success)
                        {
                            ErrorHandler.Send(code, "Invalid Parameters Name.Parameters Name cannot be a type name.");
                        }

                        
                        (string type, string name) Info= (Perms[i].Groups[1].Value, Perms[i].Groups[2].Value);
                        ThisMethodPermInfo.Add(Info);
                    }

                }
                else if (Regex.Match(code, @"\(.+\)\:").Success)
                {
                    ErrorHandler.Send(code, "Invalid Parameters");
                }
         
                int currentcount = 0;
                if (LocalFuntions.Any(x => x.FunctionName == match.Groups[1].Value &&
                x.ParametersCount == (match.Groups[2].Value == "Auto" ? Perms.Count:uint.Parse(match.Groups[2].Value))&&
                x.ParametersInformation.TrueForAll(PInfo =>
                {
                    if (ThisMethodPermInfo[currentcount].type != PInfo.Type)
                    {
                        return false;
                    }
                    currentcount++;
                    return true;
                })))
                {
                    ErrorHandler.Send(code, "Function Already decleared.use overwrite keyword instead.");
                }
                LocalFuntions.Add(new Function()
                {
                    FunctionName = match.Groups[1].Value,
                    ParametersCount = match.Groups[2].Value == "Auto" ? (uint)Perms.Count : uint.Parse(match.Groups[2].Value),
                    ParametersInformation = ThisMethodPermInfo,
                    Code = match.Groups[3].Value,
                });
            }
            else
            {
                ErrorHandler.Send(code, "invaild Function syntax.");
            }
        }
        private static void CallMethod(string code)
        {
            Regex regex = new(@"^(call)\s+\[([^\]]+)\](([\w\.]+))\.(\w+)");
            Match match = regex.Match(code);
            if (match.Success)
            {
                Type helper = GetTypeFromInput(match.Value.Remove(match.Value.IndexOf("call"), "call".Length).Trim());
                bool MethodExsit = helper != null;

                if (MethodExsit)
                {

                    // replace the = by = L // hi
                    //MatchCollection Perms = Regex.Matches(code, $@"(Array::)?({StringHelper.AllTypes})\s{{0,}}(->|<-)\s{{0,}}(""[^""]*""|[^\)[\]]+)\s*(::)?\s*(?>\w{{0,}}),?\s*");
                    MatchCollection Perms = Regex.Matches(code, $@"(Array::)?({StringHelper.AllTypes})\s{{0,}}(->|<-)\s{{0,}}([\w\d\s\+\-]{{1,}}([[]\d+[]])?)(::)?(?>\w{{0,}}),?");
                    string methodName = new Regex(@"\.(\w+)\s*\(").Match(code).Groups[1].Value.Trim(); // Extract method name from input
                    int currentcount = 0;
                    MethodInfo? TheMethod = helper!.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                        .FirstOrDefault(x => x.Name == methodName && x.GetParameters().Length == Perms.Count &&
                            x.GetParameters().ToList().TrueForAll(param =>
                            {

                                if (!Perms[currentcount].Groups[2].Value.ToLower().Contains(param.ParameterType.Name.ToLower()))
                                {

                                    return false;
                                }
                                currentcount++;
                                return true;
                            }));
                    if (TheMethod == null)
                    {
                        TheMethod = helper.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
      .FirstOrDefault(x => x.Name == methodName && x.GetParameters().Length == Perms.Count &&
          x.GetParameters().Select((param, index) => param.ParameterType.FullName.ToLower().Contains(Perms[index].Groups[2].Value.ToLower()))
          .All(match => match));
                    }
                    if (TheMethod == null)
                    {
                        ErrorHandler.Send(code, "Unable to Find Method");
                    }
                    List<dynamic> SendPerms = [];


                    if (Perms.Count > 0)
                    {


                        dynamic value = "";
                        for (int i = 0; i < Perms.Count; i++)
                        {
                            if (Perms[i].Groups[1].Value != "Array::")
                            {
                                ValueType_Get_Set(i);
                            }
                            void ValueType_Get_Set(int where)
                            {
                                Regex NestedValue = new($@"\s*({StringHelper.AllTypes})\s*<-\s*([\w\d\S]*)\.([\w\d\S]*)!extern");
                                Match NestedValue_match = NestedValue.Match(code);
                                if (NestedValue_match.Success)
                                {

                                    if (NestedValue_match.Success)
                                    {
                                        value = ValueHandler.Run(NestedValue_match.Groups[1].Value + " <- " + NestedValue_match.Groups[2].Value.Trim() + "." + NestedValue_match.Groups[3].Value.Trim());

                                    }
                                    else
                                    {
                                        ErrorHandler.Send(code, "Invalid Identifier Error.The provided code does not match the expected syntax for assignments or access operations.");
                                    }
                                }
                                else
                                {
                                    Regex _regex = new(@"""([^""""]*)""");
                                    Match _match = _regex.Match(Perms[where].Groups[4].Value);
                                    Regex _regex2 = new(@"(.*)\)");
                                    Match _match2 = _regex2.Match(Perms[where].Groups[4].Value);
                                    value = Perms[where].Groups[3].Value == "<-"
                                   ? ValueHandler.Run($"{Perms[where].Groups[2].Value} {Perms[where].Groups[3].Value} {(_match2.Success ? _match2.Groups[1].Value : Perms[where].Groups[4].Value)}")
                                   : _match.Success ? _match.Groups[^1].Value : Perms[where].Groups[4].Value;
                                }
                            }
                            if (Perms[i].Groups[1].Value == "Array::")
                            {

                                Type? objectType = TheMethod?.GetParameters()[i]?.GetType();
                                List<object> Arrays = [];
                                for (int s = i; s < Perms.Count; s++)
                                {
                                    ValueType_Get_Set(s);
                                    if (Perms[s].Groups[5].Value == "::")
                                    {
                                        Arrays.Add(Convertor.GetType(Perms[s].Groups[2].Value, value));
                                        SendPerms.Add(Convertor.ConvertToArrayType(Perms[s].Groups[2].Value, Arrays.ToArray()));

                                        i += Arrays.Count - 1;
                                        Arrays = [];
                                        break;
                                    }
                                    else
                                    {
                                        Arrays.Add(value.GetType() == typeof(int[]) ? value : Convertor.GetType(Perms[s].Groups[2].Value, value));
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    SendPerms.Add(value.GetType() == typeof(int[]) ? value : Convertor.GetType(Perms[i].Groups[2].Value, value));
                                }
                                catch (Exception ex)
                                {
                                    ErrorHandler.Send(code, ex.Message);

                                }
                            }
                        }

                    }

                    try
                    {
                        dynamic? @return = default;
                        try
                        {
                            @return = TheMethod.IsStatic ? TheMethod.Invoke(null, SendPerms.ToArray()) : TheMethod.Invoke(Activator.CreateInstance(helper), SendPerms.ToArray());
                        }
                        catch { }
                        if (@return != null)
                        {
                            if (TheMethod.ReturnType == typeof(string))
                            {
                                ValueHandler.Run($"string lpreturn = {@return}");
                            }
                            else
                            {
                                ValueHandler.Run($"{(StringHelper.AllTypes.Contains(TheMethod?.ReturnType.Name) ? TheMethod.ReturnType : "object")} lpreturn = {@return}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Message : " + ex.Message);
                        Console.WriteLine($"Source : " + ex.Source);
                        Console.WriteLine($"StackTrace : " + ex.StackTrace);
                        Console.WriteLine($"Data : " + ex.Data);

                    }
                }
                else
                {
                    ErrorHandler.Send(code, "Unable to Find Method");
                }
            }
            else
            {
                ErrorHandler.Send(code, "Invalid Instruction");
            }

        }
        public static Type GetTypeFromInput(string input)
        {

            Match dllimport_match = Regex.Match(input, "^\\[!dll:([^\\]]+)]([\\w\\.]+)\\.\\w+$");
            Match match = Regex.Match(input, @"^\[([^\]]+)]([\w\.]+)\.\w+$");
            //make this also take the last \\dll* the regex  new regex code will be \[!dll:([^\\]]+)](\w+)\.\w+$
            if (dllimport_match.Success)
            {
                //give me code
                //it will have to load the dll(.dll) and get the type from the dll
                string dll = dllimport_match.Groups[1].Value;
                string type = dllimport_match.Groups[2].Value;
                dllimport_match = Regex.Match(dll, @"([^\\\/]+$)");
                Assembly assembly = Assembly.LoadFrom(dll + ".dll");
                Type foundType = assembly.GetType(dllimport_match.Groups[1].Value + "." + type);
                return foundType ?? throw new InvalidOperationException($"Type '{type}' not found in '{dll}'");


            }

            else
            {
                string namespaceName = match.Groups[1].Value;
                string typeName = match.Groups[2].Value;
                string fullTypeName = $"{namespaceName}.{typeName}";
                Stopwatch stopwatch = new();
                
                Type foundType = Type.GetType(fullTypeName) ??
                                 AppDomain.CurrentDomain.GetAssemblies()
                                          .SelectMany(a => a.GetTypes())
                                          .FirstOrDefault(t => t.FullName?.Equals(fullTypeName, StringComparison.Ordinal) == true);

                return foundType ?? throw new InvalidOperationException($"Type '{fullTypeName}' not found.");
            }


        }


    }
}
