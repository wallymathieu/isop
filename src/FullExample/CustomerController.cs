using System.Threading.Tasks;

namespace FullExample;
public class CustomerController
{
    public Task<string> Add(string name) =>
        Task.FromResult("invoking action Add on customer controller with name : " + name);
}
