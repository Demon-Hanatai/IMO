namespace Lexer
{
    public class ErrorHandler
    {
        public static void Send(string message, string reason)
        {

            string error = $"Exception \n {message}";
            Console.WriteLine(error);
            error = "\n             ";
            for (int i = 0; message.Length > i; i++)
            {
                error += "^";
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Line : {Lexer.CurrentLine}" + error + $" {reason}");
            Console.ResetColor();
            Environment.Exit(0);
        }
    }
}