namespace Isop.Example
{
    public class CustomerController
    {
        public string Add(string name)
        {
            return "invoking action Add on customercontroller with name : " + name;
        }
    }
}