namespace Lexer
{
    public partial class MethodHandler
    {
        public class Function
        {
            public string? FunctionName;
            public string? Code;
            public uint ParametersCount;
            public List<(string Type,string Name)> ParametersInformation = new List<(string Type, string Name)>();
            


        }

    }
}
