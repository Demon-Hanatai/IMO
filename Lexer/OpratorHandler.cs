using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lexer
{
    public static class OperatorHandler
    {
        public static object Run(string code)
        {
            // Example pattern: assumes AllOperations contains escaped operators like \+ for addition
            Regex regex = new Regex($@"([\w\d]+)\s*({StringHelper.AllOperations})\s*([\w\d]+)");
            object value = null; // Changed to null to signify uninitialized state
            MatchCollection matchCollectionOperations = regex.Matches(code);

            if (matchCollectionOperations.Count > 0)
            {
                foreach (Match oper in matchCollectionOperations)
                {
                    string leftOperand = oper.Groups[1].Value;
                    string operation = oper.Groups[2].Value;
                    string rightOperand = oper.Groups[3].Value;

                    // Assuming ValueHandler.Run returns an object representing the value of the operand
                    object leftValue = ValueHandler.Run($"object <- {leftOperand}");
                    object rightValue = ValueHandler.Run($"object <- {rightOperand}");

                    // Handle conversion and operation; this example only covers addition for brevity
                    switch (operation)
                    {
                        case "+":
                            // Example addition handling; adjust as needed for type safety and conversion
                            if (leftValue is int leftInt && rightValue is int rightInt)
                            {
                                value = (leftInt + rightInt);
                            }
                            break;
                            // Implement other operations here
                    }
                }
            }
            return value;
        }
    }
}
