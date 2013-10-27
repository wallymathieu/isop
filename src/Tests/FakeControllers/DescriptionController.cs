namespace Isop.Tests.FakeControllers
{
    internal class DescriptionController
    {
        public void Action1 ()
        {
        }

        public void Action2 ()
        {
        }

        public string Help (string command)
        {
            switch (command) {
                case "Action1":
                    return "Some description 1";
                case "Action2":
                    return "Some description 2";
                default:
                    return "Some description";
            }
        }
    }
}