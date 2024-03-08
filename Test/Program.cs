using System;

namespace test
{
    public class Program
    {
        static void Main(string arg)
        {
          
            Lexer.Lexer.Run(@"
 string Text = Test.txt
.method call [System]Console.WriteLine(string<-Text)
.method call [System]Console.ReadLine()
.method call [System]Console.WriteLine(string<-Text)
");
        }
    }
}