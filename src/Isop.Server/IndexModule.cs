using Nancy;
namespace Isop.Server
{
    public class IndexModule : NancyModule
    {
        public IndexModule(IIsopServer data)
        {
            Get["/"] = _ =>
            {
                return Negotiate
                 .WithModel(data.GetModel())
                 .WithView("Index.html");
            };
        }
    }
}
