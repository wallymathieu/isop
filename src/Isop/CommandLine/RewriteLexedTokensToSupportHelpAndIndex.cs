using System.Collections.Generic;
using Isop.Infrastructure;
using Isop.CommandLine.Lex;
using Isop.Domain;

namespace Isop.CommandLine;
internal static class RewriteLexedTokensToSupportHelpAndIndex
{
    ///<summary>Arg(ControllerName),Param(..),.. -> Arg(ControllerName),Arg('Index'),... </summary>
    public static IReadOnlyList<Token> Rewrite(Conventions conventions, List<Token> tokens)
    {
        //"--command"
        if (tokens.Count >= 2
            && tokens[0].TokenType == TokenType.Argument
            && tokens[0].Value.EqualsIgnoreCase(conventions.Help)
            && tokens[1].TokenType == TokenType.Argument)
        {
            tokens[1] = new Token(tokens[1].Value, TokenType.ParameterValue, tokens[1].Index);
            tokens.Insert(1, new Token("command", TokenType.Parameter, 1));
            //index:2
            if (tokens.Count >= 4) { tokens[3] = new Token(tokens[3].Value, TokenType.ParameterValue, tokens[1].Index); }
            tokens.Insert(3, new Token("action", TokenType.Parameter, 2));
        }
        //help maps to index (should have routing here)
        if (tokens.Count == 0)
        {
            tokens.Add(new Token(conventions.Help, TokenType.Argument, 0));
        }

        //Index rewrite:
        var indexToken = new Token(conventions.Index, TokenType.Argument, 1);
        if (tokens.Count >= 2
            && tokens[1].TokenType != TokenType.Argument
            && tokens[0].TokenType == TokenType.Argument)
        {
            tokens.Insert(1, indexToken);
        }
        else if (tokens.Count == 1
            && tokens[0].TokenType == TokenType.Argument)
        {
            tokens.Add(indexToken);
        }
        return tokens;
    }
}


