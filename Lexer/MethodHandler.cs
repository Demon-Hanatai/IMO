using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lexer
{
    public class MethodHandler
    {
        public static void Run(string code)
        {
            Regex regex = new Regex(
            @"(?:.method)\s{1,}(call|create).{1,}" );
            Match match = regex.Match(code);
            if (match.Success)
                switch (match.Groups[1].Value)
                {
                    case "call":
                        code = code.Replace(".method", "");
                        CallMethod(code);
                    break;
                    default:
                        ErrorHandler.Send(code, match.Groups[1].Value + " instruction");
                    break;
                }
            
        }
        private static void CallMethod(string code)
        {
            Regex regex = new Regex(@"\s{0,}call\s{1,}(\D{1,}\w{0,})[(]((string|int)->[\w\d]{1,}[)])*");
            Match match = regex.Match(code);
            if (match.Success)
            {
                Helper helper = new Helper();
                var GetMethod = helper.GetType().GetMethods().FirstOrDefault(x => x.Name == match.Groups[1].Value);
                if (GetMethod != null)
                {
                   
                    var MethodReqPerms = GetMethod.GetParameters().Count();
                    List<dynamic> SendPerms = new List<dynamic>(MethodReqPerms);
                    MatchCollection Perms  = Regex.Matches(code, @"(Array::)?(string|int32)->([\w\d\s]{1,})(::)?(?>\w{0,}),?");

                    if (Perms.Count > 0)
                    {

                        

                        for (int i = 0; i < Perms.Count; i++)
                        {
                            if (Perms[i].Groups[1].Value == "Array::")
                            {
                                Type objectType = GetMethod.GetParameters()[i].GetType();
                                List<object> Arrays = new List<object> ();
                                for (int s = i; s < Perms.Count; s++)
                                {
                                    if (Perms[s].Groups[4].Value == "::")
                                    {
                                        Arrays.Add(Convertor.GetType(Perms[s].Groups[2].Value, Perms[s].Groups[3].Value));
                                        SendPerms.Add(Convertor.ConvertToArrayType(Perms[s].Groups[2].Value,Arrays.ToArray())); ;

                                        // tell me what to do
                                        i += Arrays.Count-1;//flow me
                                        break;
                                    }
                                    else
                                    {
                                        Arrays.Add(Convertor.GetType(Perms[s].Groups[2].Value, Perms[s].Groups[3].Value));
                                    }
                                }
                            }
                            else
                            {
                                SendPerms.Add(Convertor.GetType(Perms[i].Groups[2].Value, Perms[i].Groups[3].Value));
                            }
                        }
                        if (SendPerms.Count > MethodReqPerms ||
                            SendPerms.Count < MethodReqPerms)
                            ErrorHandler.Send(code, $"Method Req {MethodReqPerms} perms");
try                    }
                    else
                        ErrorHandler.Send(code, "Can't find this method");
                }
                else
                    ErrorHandler.Send(code, "Unable to Find Method");
            }
            else
                ErrorHandler.Send(code, "Invalid Instruction");
        }

    }
}
