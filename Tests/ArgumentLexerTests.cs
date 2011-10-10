using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Isop;
using NUnit.Framework;

namespace Isop.Tests
{
    /// <summary>
    /// argument --parameter parametervalue
    /// </summary>
    [TestFixture]
    public class ArgumentLexerTests
    {

        [Test]
        public void It_can_tokenize_simple_argument()
        {
            var lexer = new ArgumentLexer(new[] { "argument" });
            var tokens = lexer.ToArray();
            Assert.That(tokens, Is.EquivalentTo(new[] { new Token("argument", TokenType.Argument, 0) }));
        }

        [Test]
        public void It_can_tokenize_parameter()
        {
            var lexer = new ArgumentLexer(new[] { "--parameter" });
            var tokens = lexer.ToArray();
            Assert.That(tokens, Is.EquivalentTo(new[] { new Token("parameter", TokenType.Parameter, 0) }));
        }

        [Test]
        public void It_can_tokenize_parameter2()
        {
            var lexer = new ArgumentLexer(new[] { "/parameter" });
            var tokens = lexer.ToArray();
            Assert.That(tokens, Is.EquivalentTo(new[] { new Token("parameter", TokenType.Parameter, 0) }));
        }

        [Test]
        public void It_can_tokenize_parametervalue()
        {
            var lexer = new ArgumentLexer(new[] { "--parameter","parametervalue" });
            var tokens = lexer.ToArray();
            Assert.That(tokens, Is.EquivalentTo(new[] { new Token("parameter", TokenType.Parameter, 0), new Token("parametervalue", TokenType.ParameterValue, 1) }));
        }
        [Test]
        public void It_can_tokenize_parametervalue2()
        {
            var lexer = new ArgumentLexer(new[] { "--parameter=parametervalue" });
            var tokens = lexer.ToArray();
            Assert.That(tokens, Is.EquivalentTo(new[] { new Token("parameter", TokenType.Parameter, 0), new Token("parametervalue", TokenType.ParameterValue, 1) }));
        }
        [Test]
        public void It_can_peek_tokenized_value()
        {
            var lexer = new PeekEnumerable<Token>( new ArgumentLexer(new[] { "--parameter=parametervalue" , "argument"}));
            lexer.Next();
            var first = lexer.Peek();
            Assert.That(first, Is.EqualTo(new Token("parametervalue", TokenType.ParameterValue, 1)));
            Assert.That(lexer.Next(),Is.EqualTo(first));
            Assert.That(lexer.Peek(), Is.EqualTo(new Token("argument", TokenType.Argument, 2)));
        }
    }
}
