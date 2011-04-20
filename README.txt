This project is intended to collect some of the helpers for console applications. The hope is that this will simplify the design of such programs.

Some simple tests in order to have something that recognizes parameters of the form:
someprogram.exe --argument value -p

The goal is to be able to write code like:
someprogram.exe My Action --argument value
And hooking it up by writing something like:
static void Main(string[] args)
{
  var arguments = ArgumentParser.Build()
                .Recognize(typeof(MyController))
                .Parse(args);
  arguments.RecognizedAction.Invoke(Activator.CreateInstance(typeof(MyController)),
      arguments.RecognizedActionParameters.ToArray());
}