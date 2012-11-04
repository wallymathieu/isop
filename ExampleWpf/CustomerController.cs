using System.IO;

namespace Isop.Gui
{
    public class CustomerController
    {
        /// <summary>
        /// add a customer
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string Add(string name)
        {
            return "invoking action Add on customercontroller with name : " + name;
        }
        /// <summary>
        /// open a file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public string OpenFile(FileStream file)
        {
            var name = file.Name;
            file.Dispose();
            return "opening file: "+name;
        }

    }
}