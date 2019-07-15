using System.Threading.Tasks;

namespace Example
{
    public class CustomerController
    {
        public Task<string> Add(string name) =>
            Task.FromResult("invoking action Add on customer controller with name : " + name);
    }
}