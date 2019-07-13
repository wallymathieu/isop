# Isop [![Build Status](https://travis-ci.org/wallymathieu/isop.png?branch=isop)](https://travis-ci.org/wallymathieu/isop) [![Build status](https://ci.appveyor.com/api/projects/status/r4fbt9onjg3yfojv/branch/isop?svg=true)](https://ci.appveyor.com/project/wallymathieu/isop/branch/isop)

## The name

Isop is the swedish name for hyssop. Like any spice it is intended to give flavor to the development of command line apps in .net. 

## Goal

The goal is to be able to write code like:

```powershell
someprogram.exe My Action --argument value
```

Or if you prefer:

```powershell
someprogram.exe My Action /argument value
```

Isop will also figure out what you mean if you write with an equals sign between argument and value:

```powershell
someprogram.exe My Action --argument=value
```

Or if you want to write it shorter you can skip the argument name:

```powershell
someprogram.exe My Action value
```

So that the class with the name My or MyController and the method with the name Action gets invoked.

This library is intended to be like chocolate pudding mix. Not something that will replace your dinner, but rather something easy to make for dessert. A way of helping you build for instance the essential administrative apps. It's not a replacement for baking cake (building a full blown administrative interface in html, silverlight, wpf ... ). 

## When to use Isop

- Early in your development life cycle (before having tools around for your business app)
- You want an exe with many different commands organized into categories (here called controllers)

## When not to use Isop

- When your exe only has one command, then a simpler library like ndesc options is preferable.
- You do not want any magic. Isop tries to help you with binding arguments to method arguments.
- When you have a micro service solution together with some other authentication mechanism you can use [Swagger](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) to fill the same role.

## License

MIT License

## Nuget packages

[Isop](http://nuget.org/packages/Isop/)

## Example

### Having your own Main

You're hooking it up by writing something like:

```csharp
static void Main(string[] args)
{
  new Build()
       .Recognize(typeof(CustomerController))
       .Parse(args)
       .Invoke(Console.Out);
}
```

Where your controller looks something like this:

```csharp
public class MyController
{
    private readonly CustomerRepository _repository;
    public MyController()
    {
        _repository = new CustomerRepository();
    }
    public IEnumerable&lt;string&gt; Add(string name)
    { 
        yield return "Starting to insert customer";
        _repository.Insert( new Customer{ Name = name } );
        yield return "Customer inserted";  
    }
}
```

When invoked it will output two lines to the command prompt, the yielded lines above.

### Handling errors and unrecognized parameters

```csharp
class Program
{
    static void Main(string[] args)
    {
        var parserBuilder = new Build()
                  .SetCulture(CultureInfo.GetCultureInfo("sv-SE"))
                  .Recognize(typeof(CustomerController))
                  .ShouldRecognizeHelp();
        try
        {
            var parsedMethod = parserBuilder.Parse(args);
            if (parsedMethod.UnRecognizedArguments.Any())//Warning:
            {
                var unRecognizedArgumentsMessage = string.Format(
@"Unrecognized arguments: 
{0}
Did you mean any of these arguments?
{1}", String.Join(",", parsedMethod.UnRecognizedArguments.Select(unrec => unrec.Value).ToArray()),
  String.Join(",", parsedMethod.ArgumentWithOptions.Select(rec => rec.Argument.ToString()).ToArray()));
                Console.WriteLine(unRecognizedArgumentsMessage);
            }else
            {
                parsedMethod.Invoke(Console.Out);
            }
        }
        catch (TypeConversionFailedException ex)
        {
             Console.WriteLine(String.Format("Could not convert argument {0} with value {1} to type {2}", 
                ex.Argument, ex.Value, ex.TargetType));
             if (null!=ex.InnerException)
            {
                Console.WriteLine("Inner exception: ");
                Console.WriteLine(ex.InnerException.Message);
            }
        }
        catch (MissingArgumentException ex)
        {
            Console.WriteLine(String.Format("Missing argument(s): {0}",String.Join(", ",ex.Arguments.Select(a=>String.Format("{0}: {1}",a.Key,a.Value)).ToArray())));
            Console.WriteLine(parserBuilder.Help());
        }

    }
}
```

Why all this code? Mostly it's because I want the programmer to be able to have as much freedom as possible to handle errors and show error messages as he/she sees fit.

## Alternative

If you want to have something simpler for simple command line applications then I would recommend using [ndesc options](https://github.com/wallymathieu/ndesk-options-mirror) or [commandline](https://github.com/commandlineparser/commandline).
