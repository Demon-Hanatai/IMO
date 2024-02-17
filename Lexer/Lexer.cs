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
            var codes = code.Replace("\r",null).Trim().Split('\n');
            for (int i = 0; i < codes.Length; i++)
            {

                var tokens = codes[i].Split(' ');
                var ISType = StringHelper.AllTypes.Contains(tokens[0].Replace(" ", null).TrimEnd());
                if(tokens[0]==".method")
                    MethodHandler.Run(codes[i]);
                else
                {
                    if (ISType)
                        ValueHandler.Run(codes[i]);
                    else
                        ErrorHandler.Send(codes[i], "invaild Instruction");
                }  
                
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
        public static void Pi()
        {
            Console.WriteLine("LOL");
        }
        public void Add(int value1,int value2) {
            Print(value2 + value1);
         }
        //watch this
        public void Print(object text)
        {
            Console.Write(text);
        }
        public static void Read(string me)
        {
            Console.WriteLine("LOL "+me);
        }
    }
}
