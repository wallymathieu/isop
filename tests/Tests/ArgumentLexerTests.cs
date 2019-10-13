using System.Linq;
using Isop.CommandLine.Lex;
using Isop.Infrastructure;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class ArgumentLexerTests
    {

        [Test]
        public void It_can_tokenize_simple_argument()
        {
            var lexed = ArgumentLexer.Lex(new[] { "argument" });
            var tokens = lexed.ToArray();
            Assert.That(tokens, Is.EquivalentTo(new[] { new Token("argument", TokenType.Argument, 0) }));
        }

        [Test]
        public void It_can_tokenize_parameter_minus_minus()
        {
            var lexed = ArgumentLexer.Lex(new[] { "--parameter" });
            var tokens = lexed.ToArray();
            Assert.That(tokens, Is.EquivalentTo(new[] { new Token("parameter", TokenType.Parameter, 0) }));
        }

        [Test]
        public void It_can_tokenize_parameter_slash()
        {
            var lexed = ArgumentLexer.Lex(new[] { "/parameter" });
            var tokens = lexed.ToArray();
            Assert.That(tokens, Is.EquivalentTo(new[] { new Token("parameter", TokenType.Parameter, 0) }));
        }
        
        [Test]
        public void It_can_tokenize_parameter_value_slash_and_equals()
        {
            var lexed = ArgumentLexer.Lex(new[] { "/parameter=parametervalue" });
            var tokens = lexed.ToArray();
            Assert.That(tokens, Is.EquivalentTo(new[] { new Token("parameter", TokenType.Parameter, 0), new Token("parametervalue", TokenType.ParameterValue, 1) }));
        }

        [Test]
        public void It_can_tokenize_parameter_value_minus_minus()
        {
            var lexed = ArgumentLexer.Lex(new[] { "--parameter", "parametervalue" });
            var tokens = lexed.ToArray();
            Assert.That(tokens, Is.EquivalentTo(new[] { new Token("parameter", TokenType.Parameter, 0), new Token("parametervalue", TokenType.ParameterValue, 1) }));
        }
        
        [Test]
        public void It_can_tokenize_parameter_value_minus_minus_and_equals()
        {
            var lexed = ArgumentLexer.Lex(new[] { "--parameter=parametervalue" });
            var tokens = lexed.ToArray();
            Assert.That(tokens, Is.EquivalentTo(new[] { new Token("parameter", TokenType.Parameter, 0), new Token("parametervalue", TokenType.ParameterValue, 1) }));
        }
    }
}
