using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

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
        public string Value { get; private set; }
	    public Argument Argument{get;private set;}
		public string Parameter{get;private set;}

	    public ArgumentWithParameters(Argument argument,string parameter,string value=null)
		{
	        Value = value;
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
		    var argumentList = arguments.ToList();
		    return actions.Select(act=>
				new{
					arguments= argumentList.FindIndexAndValues(act.Recognizes),
					action=act
				})
				.Where(couple=>couple.arguments.Any())
				.Select(couple=> new ArgumentWithParameters(
                    couple.action,
                    couple.arguments.First().Value,
                    argumentList.GetForIndexOrDefault( couple.arguments.First().Key+1)))
				;
		}
	}
}
