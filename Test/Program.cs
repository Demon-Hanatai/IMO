using Lexer;



Lexer.Lexer.Run(@"
string MyName -> Demon 404
double L -> 384.88
.method call Print(double<-L)");
Lexer.OperatorHandler.Run("L<-MyName");