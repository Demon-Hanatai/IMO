using System.Text;
using System.Text.RegularExpressions;

namespace Lexer
{
    public static class Lexer
    {
        public static uint CurrentLine = 0;
        private static void StartUp()
        {
            ValueHandler.Run("object lpreturn = 0");
            for (int i = 0; i < 10; i++)
                ValueHandler.Run($"object lpparm{i} = 0");
           

        }
        private static string LoadMethods(string code)
        {
            Regex regex = new(@"\.method\s+call\s+(\w+\d*)\s+from\s+(.+)");
            MatchCollection match = regex.Matches(code);
            List<string> functions = new();
            if (match.Count > 0)
            {
                var MethodName = match[0].Groups[1].Value;
                var From = match[0].Groups[2].Value;
                if(File.Exists("./"+From))
                {
                    Regex FindFunction = new($@"\s*\.method\s+create\s+{MethodName}\s+{{\s+[\s\S]*?\s+}}");
                    MatchCollection matches = FindFunction.Matches(File.ReadAllText(From));
                    if (matches.Count > 0)
                    {
                        if (!functions.Contains(matches[0].Value))
                        {
                            functions.Add(matches[0].Value);
                        }
                    
                    }
                    else
                    {
                        ErrorHandler.Send(message: "Function Not Found", reason: $"The function {MethodName} was not found in {From}.");
                    }
                }
                else
                {
                    ErrorHandler.Send(message: "File Not Found", reason: $"The file {From} was not found.");
                }
            }
            Regex findfrom = new(@"\s+from\s+.*");
            foreach (MatchCollection item in findfrom.Matches(code))
            {
                code = code.Replace(item[0].Value,null);
            }
            string newcode = default;
            foreach (string item in functions)
            {
                newcode += item+";";
            }
            newcode+="\n"+code;
            return newcode;
        }
        private static string PreprocessCode(string code)
        {
            var lines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var processedLines = new StringBuilder();
            bool foundSemicolon = false;

            foreach (var line in lines)
            {
                if (line.TrimEnd().EndsWith(";"))
                {
                    processedLines.Append(line.Trim());
                    foundSemicolon = true;
                }
                else
                {
                    processedLines.AppendLine(line);
                }
            }

            if (!foundSemicolon)
            {
                ErrorHandler.Send(message: "Preprocessing Error", reason: "No semicolon ';' found in code.");
                return "...";
            }

            return processedLines.ToString();
        }

        public static void Run(string code)
        {
            StartUp();
            code = PreprocessCode(LoadMethods(code));
            
            string pattern = @"(\.method\s+create[^\{]*\{[\s\S]*?[.\n\r\s]*\})";
            MatchCollection matches = Regex.Matches(code, pattern);

            foreach (Match match in matches)
            {
                string methodBlock = match.Value;

                string modifiedBlock = Regex.Replace(methodBlock, "\n", "*34M#9823");
                code = code.Replace(match.Value, modifiedBlock);

            }
            string removeemtryspaces = "";
            string[] _ = code.TrimEnd().TrimStart().Split('\n');


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

                CurrentLine = (uint)i;
                string[] tokens = codes[i].Split(' ');
                bool ISType = StringHelper.AllTypes.Contains(tokens[0].Replace(" ", null).TrimEnd());
                if (tokens[0][0] == '/' && tokens[0][1] == '/')
                {
                    continue;
                }

                if (tokens[0] == ".method")
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
