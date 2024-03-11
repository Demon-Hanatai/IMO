using System.Text;
using System.Text.RegularExpressions;

namespace Lexer
{
    public static class Lexer
    {
        private static bool StartupCreated = false;
        private static bool LoadIncludes = false;
        private static bool LoadIncludes2 = false;
        private static readonly bool IsPreprocessed = false;
        private static bool LibraryLoaded = false;
        private static readonly bool DuplicateIncludes = false;
        public static uint CurrentLine = 0;
        private static void StartUp()
        {
            if (!StartupCreated)
            {
                ValueHandler.Run("object lpreturn = 0");
                for (int i = 0; i < 10; i++)
                {
                    ValueHandler.Run($"object lpparm{i} = 0");
                }
            }
        }
        private static string LoadInclude(string code, string defaultFolderPath)
        {
            string newcode = "";

            Regex regex = new(@"\.include\s+(.*)\s+from\s+(.*)");
            MatchCollection match = regex.Matches(code);
            List<string> functions = [];
            if (match.Count > 0)
            {
                for (int i = 0; i < match.Count; i++)
                {
                    string MethodName = match[0].Groups[1].Value;
                    string From = match[0].Groups[2].Value.TrimEnd();
                    if (!From.EndsWith(".sc"))
                    {
                        From += ".sc";
                    }

                    string fullPath = Path.IsPathRooted(From) ? From : Path.Combine(defaultFolderPath, From);

                    if (File.Exists(fullPath))
                    {

                        Regex FindFunction = new($@"\.method\s+create\s+{MethodName}\s*\(\)\s*{{(\s*.*\s*)*}}");
                        MatchCollection matches = FindFunction.Matches(File.ReadAllText(fullPath));
                        if (matches.Count > 0)
                        {
                            if (!functions.Contains(matches[0].Value))
                            {
                                functions.Add(matches[0].Value);
                            }
                        }
                        else
                        {
                            ErrorHandler.Send(message: "Function Not Found", reason: $"The function {MethodName} was not found in {fullPath}.");
                        }
                    }
                    else
                    {
                        ErrorHandler.Send(message: "File Not Found", reason: $"The file {From} was not found.");
                    }
                }
                foreach (string item in functions)
                {
                    newcode += item + "\n";
                }
            }

            foreach (Match item in match)
            {
                code = code.Replace(item.Value.TrimEnd(), null);
            }
            newcode += code;
            return newcode;

        }
        private static string LoadMethods(string code, string defaultFolderPath)
        {
            Regex regex = new(@"\.method\s+call\s+(\w+\d*)\s+from\s+(.+)");
            MatchCollection match = regex.Matches(code);
            List<string> functions = [];
            if (match.Count > 0)
            {
                string MethodName = match[0].Groups[1].Value;
                string From = match[0].Groups[2].Value;

                // Append '.sc' if it's not present in the file path
                if (!From.EndsWith(".sc"))
                {
                    From += ".sc";
                }

                string fullPath = Path.IsPathRooted(From) ? From : Path.Combine(defaultFolderPath, From);

                if (File.Exists(fullPath))
                {
                    Regex FindFunction = new($@"\.method\s+create\s+{MethodName}\s*\(\)\s*{{(\s*.*\s*)*}}");
                    MatchCollection matches = FindFunction.Matches(File.ReadAllText(fullPath));
                    if (matches.Count > 0)
                    {
                        if (!functions.Contains(matches[0].Value) && !code.Contains(matches[0].Value))
                        {
                            functions.Add(matches[0].Value);
                        }
                    }
                    else
                    {
                        ErrorHandler.Send(message: "Function Not Found", reason: $"The function {MethodName} was not found in {fullPath}.");
                    }
                }
                else
                {
                    ErrorHandler.Send(message: "File Not Found", reason: $"The file {fullPath} was not found.");
                }
            }

            Regex findFrom = new(@"call\s+(\w+\d*)\s+from\s+(.*)");
            MatchCollection matchesFrom = findFrom.Matches(code);
            for (int i = 0; i < matchesFrom.Count; i++)
            {
                code = code.Replace(matchesFrom[i].Value, $"call::local {matchesFrom[i].Groups[1]}");
            }

            string newCode = string.Join("\n", functions) + "\n" + code;
            return newCode;
        }
        private static string IncludeLoader2(string code, string defaultFolderPath)
        {
            Regex includeRegex = new(@"\.include\s+(.+)\s*");
            MatchCollection includeMatches = includeRegex.Matches(code);
            HashSet<string> includedFiles = [];
            StringBuilder newCode = new();

            // Process each include statement
            foreach (Match m in includeMatches)
            {
                string From = m.Groups[1].Value.TrimEnd();
                if (!From.EndsWith(".sc"))
                {
                    From += ".sc";
                }

                string fullPath = Path.IsPathRooted(From) ? From : Path.Combine(defaultFolderPath, From);

                if (File.Exists(fullPath) && !includedFiles.Contains(fullPath))
                {
                    _ = includedFiles.Add(fullPath);
                    string fileContent = File.ReadAllText(fullPath);
                    _ = newCode.AppendLine(fileContent);
                }
                else if (!File.Exists(fullPath))
                {
                    ErrorHandler.Send(message: "File Not Found", reason: $"The file {fullPath} was not found.");
                }
            }

            // Remove the include directives from the code
            code = includeRegex.Replace(code, "");

            _ = newCode.Append(code);
            return newCode.ToString();
        }
        private static string PreprocessCode(string code)
        {
            string[] lines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            StringBuilder processedLines = new();
            bool foundSemicolon = false;

            foreach (string line in lines)
            {
                if (line.TrimEnd().EndsWith(";"))
                {
                    _ = processedLines.Append(line.Trim());
                    foundSemicolon = true;
                }
                else
                {
                    _ = processedLines.AppendLine(line);
                }
            }

            if (!foundSemicolon)
            {
                ErrorHandler.Send(message: "Preprocessing Error", reason: "No semicolon ';' found in code.");
                return "...";
            }

            return processedLines.ToString();
        }
        private static string RemoveDuplicateIncludes(string code)
        {
            Regex includeRegex = new(@"\.include\s+(.+?)\s*$", RegexOptions.Multiline);
            MatchCollection includeMatches = includeRegex.Matches(code);

            HashSet<string> includedFiles = [];
            _ = new StringBuilder();

            foreach (Match match in includeMatches)
            {
                string includePath = match.Groups[1].Value.Trim();

                if (!includedFiles.Contains(includePath))
                {
                    _ = includedFiles.Add(includePath);
                }
                else
                {
                    // Replace duplicate include with an empty string
                    code = code.Replace(match.Value, "");
                }
            }

            return code;
        }

        public static void Run(string code, bool fromaMethodcaller = false, string Path = "")
        {

            //Comment Remover
            Regex regex = new(@"\/\/(.*)");
            MatchCollection matchs = regex.Matches(code);
            for (int i = 0; i < matchs.Count; i++)
            {
                code = code.Replace(matchs[i].Value, null);
            }
            //End Comment Remover\
            //Remove Duplicate Includes
            //if (!DuplicateIncludes)
            //{
            //    code = RemoveDuplicateIncludes(code);
            //    DuplicateIncludes = true;
            //}
            //End Remove Duplicate Includes
            if (!StartupCreated)
            {
                StartUp();
                StartupCreated = true;
            }
            //if(!IsPreprocessed)
            //{
            //    code = PreprocessCode(code);
            //    IsPreprocessed = true;
            //}
            if (!LoadIncludes)
            {
                code = LoadInclude(code, Path);
                LoadIncludes = true;
            }
            if (!LoadIncludes2)
            {
                code = IncludeLoader2(code, Path);
                LoadIncludes2 = true;
            }

            if (!LibraryLoaded)
            {
                code = LoadMethods(code, Path);
                LibraryLoaded = true;
            }



            string pattern = @"#start_cond([\s\S]*?)#end_cond";
            MatchCollection matches = new Regex(pattern).Matches(code);
            foreach (Match item in matches)
            {
                string block = item.Value.Replace("\n", "*34M#");
                try
                {
                    block = block.Remove(block.IndexOf("#start_cond"), "#start_cond".Length);
                    block = block.Remove(block.LastIndexOf("#end_cond"));
                    block = block.Remove(block.IndexOf("*34M#"), "*34M#".Length);
                }
                catch
                {
                    ErrorHandler.Send(block, "Invalid Condition Block");
                    return;
                }
                code = code.Replace(item.Groups[0].Value, block);
            }




            if (!fromaMethodcaller)
            {
                pattern = @"(\.method\s+(create|overwrite)[^\{]*\{[\s\S]*?[.\n\r\s]*\})";
                matches = Regex.Matches(code, pattern);

                foreach (Match match in matches)
                {
                    string methodBlock = match.Value;

                    string modifiedBlock = Regex.Replace(methodBlock, "\n", "*34M#9823");
                    code = code.Replace(match.Value, modifiedBlock);

                }
            }


            string removeemtryspaces = "";
            _ = code.TrimEnd().TrimStart().Split('\n');


            foreach (string c in code.TrimEnd().TrimStart().Split('\n'))
            {
                removeemtryspaces += c.Length >= 1 ? c + "\n" : string.Empty;
            }

            List<string> codeslines = [];
            foreach (string item in removeemtryspaces.Split('\n'))
            {
                if (item.Length > 0 && item != "\r")
                {
                    codeslines.Add(item.Replace("\n", null).Trim());
                }
            }

            string[] codes = codeslines.ToArray();


            for (int i = 0; i < codes.Length; i++)
            {
                codes[i] = codes[i].TrimStart().TrimEnd();
            }

            for (int i = 0; i < codes.Length; i++)
            {

                CurrentLine = (uint)i;
                if (codes[i] == "")
                {
                    continue;
                }
                string[] tokens = codes[i].Split(' ');
                bool ISType = StringHelper.AllTypes.Contains(tokens[0].Replace(" ", null).TrimEnd());
                if (tokens[0].StartsWith("if"))
                {
                    _ = ConditionsHandler.Run(codes[i]);
                }
                else if (tokens[0] == ".method")
                {
                    MethodHandler.Run(codes[i]);
                }
                else
                {

                    if (ISType)
                    {
                        ValueHandler.Run(codes[i]);
                    }
                    else
                    {
                        ErrorHandler.Send(codes[i], "invaild Instruction");
                    }
                }

            }

        }



    }

    public class Helper
    {
        // Existing methods
        public static void Max(int[][] e)
        {
            Console.WriteLine(e);
        }
        public static void FindMax(int[] t, string[] id, int eo)
        {
            for (int i = 0; i < t.Length; i++)
            {
                Console.WriteLine(t[i]);
            }


        }

        public static void Pi()
        {
            Console.WriteLine("LOL");
        }

        public void Add(int value1, int value2)
        {
            Print(value2 + value1);
        }
        public void Prints(int[] text)
        {
            foreach (int item in text)
            {
                Console.Write(item + " ");
            }
        }
        public void Print(object text)
        {
            Console.WriteLine(text);
        }

        public static void Read(string me)
        {
            Console.WriteLine("LOL " + me);
        }

        // New enhanced methods
        public static int Max(int[] numbers)
        {
            return numbers.Max();
        }

        public static double CalculateAreaOfCircle(double radius)
        {
            return Math.PI * Math.Pow(radius, 2);
        }

        public static string ReverseString(string input)
        {
            return new string(input.Reverse().ToArray());
        }

        public static bool IsPalindrome(string input)
        {
            string normalized = input.ToLower().Replace(" ", "");
            return normalized.SequenceEqual(normalized.Reverse());
        }

        public static void Fibonacci(int n)
        {
            int a = 0, b = 1;
            Console.Write($"{a} {b} ");
            for (int i = 2; i < n; i++)
            {
                int c = a + b;
                Console.Write($"{c} ");
                a = b;
                b = c;
            }
            Console.WriteLine();
        }

        public static void ConvertToBinary(int number)
        {
            Console.WriteLine(Convert.ToString(number, 2));
        }

        public static void PrintEvenOrOdd(int number)
        {
            Console.WriteLine(number % 2 == 0 ? $"{number} is Even" : $"{number} is Odd");
        }
    }

}
