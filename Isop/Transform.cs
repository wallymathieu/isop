using System;
using System.Linq;
namespace Isop
{
    public class Transform
    {
        // Lexer -> 
        // Arg(ControllerName),Param(..),.. -> Arg(ControllerName),Arg('Index'),... 
        public ArgumentLexer Rewrite(ArgumentLexer tokens)
        {
            //"--command"
            if (tokens.Count() >= 2 
                && tokens[0].TokenType==TokenType.Argument 
                && tokens[0].Value.Equals("help",StringComparison.OrdinalIgnoreCase)
                && tokens[1].TokenType==TokenType.Argument)
            {
                tokens[1] = new Token(tokens[1].Value,TokenType.ParameterValue,tokens[1].Index);
                tokens.Insert(1,new Token("command",TokenType.Parameter,1));
                //index:2
                if (tokens.Count() >= 4) { tokens[3] = new Token(tokens[3].Value, TokenType.ParameterValue, tokens[1].Index); }
                tokens.Insert(3, new Token("action", TokenType.Parameter, 2));
            }
            //help maps to index (should have routing here)
            if (tokens.Count() == 0)
            {
                tokens.Add(new Token("help",TokenType.Argument,0));
            }

            //Index rewrite:
            var indexToken= new Token("Index", TokenType.Argument,1);
            if (tokens.Count()>=2 
                && tokens[1].TokenType!=TokenType.Argument 
                && tokens[0].TokenType==TokenType.Argument)
            {
                tokens.Insert(1,indexToken);
            }
            else if (tokens.Count()==1 
                && tokens[0].TokenType==TokenType.Argument)
            {
                tokens.Add(indexToken);
            }
            return new ArgumentLexer(tokens);
        }
    }
}
