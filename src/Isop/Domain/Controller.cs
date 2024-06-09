using System.Linq;
using System.Text.RegularExpressions;
using Isop.Help;

namespace Isop.Domain;
public class Controller(Type type) : IEquatable<Controller>
{
    public Type Type { get; } = type;
    public string GetName(Conventions conventions)
    {
        if (conventions is null) throw new ArgumentNullException(nameof(conventions));
        return Regex.Replace(Type.Name, conventions.ControllerName + "$", "", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
    }

    public IEnumerable<Method> GetControllerActionMethods(Conventions conventions) =>
        GetControllerActionMethods(conventions, Type).Select(m => new Method(m));

    private static IEnumerable<MethodInfo> GetControllerActionMethods(Conventions conventions, Type type) =>
        GetOwnPublicMethods(type)
            .Where(m => !m.Name.EqualsIgnoreCase(conventions.Help));

    private static IEnumerable<MethodInfo> GetOwnPublicMethods(Type type) =>
        type.GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.DeclaringType != typeof(object) 
                        && !m.Name.StartsWithIgnoreCase("get_")
                        && !m.Name.StartsWithIgnoreCase("set_"));

    public Method? GetMethod(Conventions conventions, string name) =>
        GetControllerActionMethods(conventions).SingleOrDefault(m => m.Name.EqualsIgnoreCase(name));

    public bool IsHelp() => Type == typeof(HelpController);

    public override bool Equals(object? other) => other is Controller controller && Equals(controller);
    public bool Equals(Controller? other) => Type == other?.Type;

    public override int GetHashCode() => Type.GetHashCode();

    public override string ToString() => $"[Controller: Type={Type}]";
}
