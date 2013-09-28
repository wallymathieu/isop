using System.Linq;
using Isop.Infrastructure;
using Isop.Lex;
using NUnit.Framework;

namespace Isop.Tests
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
        public void It_can_tokenize_parameter()
        {
            var lexed = ArgumentLexer.Lex(new[] { "--parameter" });
            var tokens = lexed.ToArray();
            Assert.That(tokens, Is.EquivalentTo(new[] { new Token("parameter", TokenType.Parameter, 0) }));
        }

        [Test]
        public void It_can_tokenize_parameter2()
        {
            var lexed = ArgumentLexer.Lex(new[] { "/parameter" });
            var tokens = lexed.ToArray();
            Assert.That(tokens, Is.EquivalentTo(new[] { new Token("parameter", TokenType.Parameter, 0) }));
        }

        [Test]
        public void It_can_tokenize_parametervalue()
        {
            var lexed = ArgumentLexer.Lex(new[] { "--parameter", "parametervalue" });
            var tokens = lexed.ToArray();
            Assert.That(tokens, Is.EquivalentTo(new[] { new Token("parameter", TokenType.Parameter, 0), new Token("parametervalue", TokenType.ParameterValue, 1) }));
        }
        [Test]
        public void It_can_tokenize_parametervalue2()
        {
            var lexed = ArgumentLexer.Lex(new[] { "--parameter=parametervalue" });
            var tokens = lexed.ToArray();
            Assert.That(tokens, Is.EquivalentTo(new[] { new Token("parameter", TokenType.Parameter, 0), new Token("parametervalue", TokenType.ParameterValue, 1) }));
        }
        [Test]
        public void It_can_peek_tokenized_value()
        {
            var lexed = new PeekEnumerable<Token>(ArgumentLexer.Lex(new[] { "--parameter=parametervalue", "argument" }));
            lexed.Next();
            var first = lexed.Peek();
            Assert.That(first, Is.EqualTo(new Token("parametervalue", TokenType.ParameterValue, 1)));
            Assert.That(lexed.Next(),Is.EqualTo(first));
            Assert.That(lexed.Peek(), Is.EqualTo(new Token("argument", TokenType.Argument, 2)));
        }
    }
}
