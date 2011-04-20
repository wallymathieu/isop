using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;

namespace ConsoleHelpers
{
	delegate void ActionDelegate();
	public class Argument
	{
		public string Longname{get;private set;}
		public Argument (string longname)
		{
			Longname=longname;
		}
		
		public bool Recognizes(string argument)
		{
			if (argument.StartsWith("-"+Longname[0]))
				return true;
			if (argument.StartsWith("--"+Longname))
				return true;
			return false;
		}
	}
	
	public class ArgumentWithParameters
	{
		public Argument Argument{get;private set;}
		public string Parameter{get;private set;}
		public ArgumentWithParameters(Argument argument,string parameter)
		{
			Argument = argument;
			Parameter = parameter;
		}
	}
	public class ArgumentParser
	{
		private IEnumerable<string> arguments;
		private	IEnumerable<Argument> actions;
		public ArgumentParser(IEnumerable<string> arguments,IEnumerable<Argument> actions)
		{
			this.arguments = arguments;
			this.actions = actions;
		}
		public IEnumerable<ArgumentWithParameters> GetInvokedArguments ()
		{
			return actions.Select(act=>
				new{
					arguments= arguments.Where(arg=>act.Recognizes(arg)),
					action=act
				})
				.Where(couple=>couple.arguments.Any())
				.Select(couple=> new ArgumentWithParameters(couple.action,couple.arguments.First()))
				;
		}
	}
	
	[TestFixture]
	public class ArgumentParserTests
	{
		[SetUp]
		public void SetUp(){}
		[TearDown]
		public void TearDown(){}
		
		[Test]
		public void Recognizes_shortform()
		{
			var arg = new Argument("argument");
			
			var parser = new ArgumentParser(new []{"-a"},new[]{arg});	
			var arguments = parser.GetInvokedArguments();
			Assert.That(arguments.Count(),Is.EqualTo(1));
			var arg1=arguments.First();
			Assert.That(arg1.Argument,Is.EqualTo(arg));
		}

		[Test]
		public void Given_several_arguments_Then_the_correct_one_is_recognized()
		{
			var arg = new Argument("beta");
			
			var parser = new ArgumentParser(new []{"-a","-b"},new[]{arg});	
			var arguments = parser.GetInvokedArguments();
			Assert.That(arguments.Count(),Is.EqualTo(1));
			var arg1=arguments.First();
			Assert.That(arg1.Parameter,Is.EqualTo("-b"));
		}
		
		[Test]
		public void Recognizes_longform()
		{
			var arg = new Argument("beta");
			
			var parser = new ArgumentParser(new []{"-a","--beta"},new[]{arg});	
			var arguments = parser.GetInvokedArguments();
			Assert.That(arguments.Count(),Is.EqualTo(1));
			var arg1=arguments.First();
			Assert.That(arg1.Parameter,Is.EqualTo("--beta"));
		}
		
	}
}
