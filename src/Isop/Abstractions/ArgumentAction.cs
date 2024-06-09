using System.Threading.Tasks;

namespace Isop.Abstractions;
/// <summary>
/// Action that should be invoked when argument is encountered
/// </summary>
public delegate Task<object?> ArgumentAction(string? value);

