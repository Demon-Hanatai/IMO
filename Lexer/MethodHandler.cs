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
                        _ = ConditionsHandler.Run(code.Replace("if ", null).Trim().TrimStart().TrimEnd());
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
            Regex regex = new(@"\s*call::local\s+(\w+)");
            Match match = regex.Match(code);
            if (match.Success)
            {

                Function? LocalFunction = LocalFuntions.FirstOrDefault(x => x.FunctionName == match.Groups[1].Value);
                if (LocalFunction != null)
                {
                    MemoryHandler.WatchForChanges();
                    Lexer.Run(LocalFunction.Code, true);
                    MemoryHandler.StopWatcher = true;
                    MemoryHandler.RemoveLastChangesFromMemory();

                }
                else
                {
                    ErrorHandler.Send(match.Groups[0].Value, "Function not decleared");
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
            _ = $"{StringHelper.AllTypes})\\s{{0,}}(=|<-)\\s{{0,}}([\\w\\d\\s]{{1,}}([[]\\d+[]])?";
            Regex regex = new(@"\s*create\s+(.+)\(\)\s*\{\s*([\s\S]*?)\s*\}");
            Match match = regex.Match(code);
            if (match.Success is true)
            {
                if (LocalFuntions.Any(x => x.FunctionName == match.Groups[1].Value))
                {
                    ErrorHandler.Send(code, "Function Already decleared.use overwrite keyword instead.");
                }
                LocalFuntions.Add(new Function()
                {
                    FunctionName = match.Groups[1].Value,
                    Code = match.Groups[2].Value.Replace("{", null).Replace("}", null),
                });
            }
            else
            {
                ErrorHandler.Send(code, "invaild Function syntax.");
            }
        }
        private static void CallMethod(string code)
        {
            Regex regex = new(@"^(call)\s+\[([^\]]+)\](\w+)\.(\w+)");
            Match match = regex.Match(code);
            if (match.Success)
            {
                Type helper = GetTypeFromInput(match.Value.Replace("call", null).Trim());
                bool MethodExsit = helper != null;
                if (MethodExsit)
                {

                    // replace the = by = L // hi
                    MatchCollection Perms = Regex.Matches(code, $@"(Array::)?({StringHelper.AllTypes})\s{{0,}}(->|<-)\s{{0,}}([\w\d\s\+\-]{{1,}}([[]\d+[]])?)(::)?(?>\w{{0,}}),?");
                    string methodName = new Regex(@"\.(\w+)").Match(code).Groups[1].Value.TrimStart('.'); // Extract method name from input

                    MethodInfo? TheMethod = helper.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                        .FirstOrDefault(x => x.Name == methodName && x.GetParameters().Length == Perms.Count &&
                            x.GetParameters().ToList().TrueForAll(param =>
                            {
                                for (int i = 0; i < Perms.Count; i++)
                                {
                                    if (!Perms[i].Groups[2].Value.ToLower().Contains(param.ParameterType.Name.ToLower()))
                                    {
                                        return false;
                                    }
                                }
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
                    List<dynamic> SendPerms = new(Perms.Count + 1);


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
                                var NestedValue = new Regex($@"\s*({StringHelper.AllTypes})\s*<-\s*([\w\d\S]*)\.([\w\d\S]*)!extern");
                                var NestedValue_match = NestedValue.Match(code);
                                if (NestedValue_match.Success)
                                {

                                    if (NestedValue_match.Success)
                                    {
                                        value = ValueHandler.Run(NestedValue_match.Groups[1].Value + " <- " + NestedValue_match.Groups[2].Value.Trim() +"."+ NestedValue_match.Groups[3].Value.Trim());
                                      
                                    }
                                    else
                                    {
                                        ErrorHandler.Send(code, "Invalid Identifier Error.The provided code does not match the expected syntax for assignments or access operations.");
                                    }
                                }
                                else
                                {
                                    value = Perms[where].Groups[3].Value == "<-"
                                   ? ValueHandler.Run($"{Perms[where].Groups[2].Value} {Perms[where].Groups[3].Value} {Perms[where].Groups[4].Value}")
                                   : Perms[where].Groups[4].Value;
                                }
                            }
                            if (Perms[i].Groups[1].Value == "Array::")
                            {

                                Type? objectType = TheMethod?.GetParameters()[i]?.GetType();
                                List<object> Arrays = [];
                                for (int s = i; s < Perms.Count; s++)
                                {
                                    ValueType_Get_Set(s);
                                    if (Perms[s].Groups[6].Value == "::")
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
                        object? @return = default;
                        try
                        {

                            @return = TheMethod?.Invoke(helper, SendPerms.ToArray());
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
                                ValueHandler.Run($"object lpreturn = {@return}");
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


            // Regex to extract the namespace and type from the input
            Match match = Regex.Match(input, @"^\[([^\]]+)](\w+)\.\w+$");

            if (!match.Success)
            {
                throw new ArgumentException("Input format is incorrect. Expected format: [Namespace]Type.Method");
            }

            string namespaceName = match.Groups[1].Value;
            string typeName = match.Groups[2].Value;
            string fullTypeName = $"{namespaceName}.{typeName}";

            // Attempt to find the type in all loaded assemblies
            Type foundType = Type.GetType(fullTypeName) ??
                             AppDomain.CurrentDomain.GetAssemblies()
                                      .SelectMany(a => a.GetTypes())
                                      .FirstOrDefault(t => t.FullName?.Equals(fullTypeName, StringComparison.Ordinal) == true);

            return foundType ?? throw new InvalidOperationException($"Type '{fullTypeName}' not found.");
        }


    }
}
