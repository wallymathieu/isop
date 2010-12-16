
using System;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
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
		public ArgumentParser(IEnumerable<string> arguments,IEnumerable<Argument> actions)
		{
			
		}
		public IEnumerable<ArgumentWithParameters> GetArguments ()
		{
			throw new System.NotImplementedException ();
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
			var arguments = parser.GetArguments();
			Assert.That(arguments.Count(),Is.EqualTo(1));
			var arg1=arguments.First();
			Assert.That(arg1.Argument,Is.EqualTo(arg));
		}



	}
}
