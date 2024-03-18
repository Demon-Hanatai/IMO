using Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace test43
{
    internal class Program
    {
        static int Main(string[] args)
        {

            // Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "true");
           
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: Lexer.exe <file path>");
                return 1;
            }
            var filePath = args[0];
            if (filePath == null)
            {
                Console.WriteLine("File path is null");
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {

                }
                if (File.Exists(filePath))
                {
                    string file = File.ReadAllText(filePath);
                    Lexer.Lexer.Run(file,Path: Path.GetDirectoryName(filePath));
                }
                else
                {
                    // File not found
                    Console.WriteLine("File not found");
                }
                
            }
            return 0;
        }
     
    }
   
}
