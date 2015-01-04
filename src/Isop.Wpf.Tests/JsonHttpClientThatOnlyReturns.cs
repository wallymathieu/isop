using Isop.Gui;

namespace Isop.Wpf.Tests
{
    class JsonHttpClientThatOnlyReturns : JsonHttpClientFakeBase
    {
        private string data;

        public JsonHttpClientThatOnlyReturns(string data)
        {
            this.data = data;
        }

        public override JsonResponse RequestSync(Request jsonRequest)
        {
            return new JsonResponse(System.Net.HttpStatusCode.OK, data);
        }
    }
}
