using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test43
{
    internal class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine();
            args = new string[] { @"C:\Users\DEMON404\respo\Program\Main.cs" };
            if(args.Length < 1)
            {
                Console.WriteLine("Usage: Lexer.exe <file path>");
                return 1;
            }
            var filePath = args[0];
            if(filePath == null)
            {
                Console.WriteLine("File path is null");
            }
            else
            {
               if(File.Exists(filePath))
                {
                    string file = File.ReadAllText(filePath);
                    Lexer.Lexer.Run(file,Path.GetDirectoryName(filePath));
                }
               else
               {
                    // File not found
                    Console.WriteLine("File not found");
               }
            }
            Console.ReadLine();
            return 0;
        }
     
    }
   
}
