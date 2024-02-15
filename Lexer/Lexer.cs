using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lexer
{
    public static class Lexer
    {
        public static void Run(string code)
        {
            var tokens = code.Split(' ');
            switch (tokens[0])
            {
                case ".method":
                    MethodHandler.Run(code);
                    break;
                default:
                    ErrorHandler.Send(code,"invaild Instruction");
                break;
            }
        }
      
 
      
    }
    public class Helper
    {
        public static void FindMax(int[] t,string id,int eo)
        {
            for (int i = 0; i < t.Length; i++)
            {
                Console.WriteLine(t[i] + $"{eo}");
            }
            Console.WriteLine(id);
        }
        public void Add(int value1,int value2) {
            Print(value2 + value1);
         }
        //watch this
        public void Print(object text)
        {
            Console.Write(0);
        }
        public static void Read(string me)
        {
            Console.WriteLine("LOL "+me);
        }
    }
}
