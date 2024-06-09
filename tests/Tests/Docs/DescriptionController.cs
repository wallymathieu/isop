namespace Tests.Docs;
internal class DescriptionController
{
    public void Action1()
    {
    }

    public void Action2()
    {
    }

    public string Help(string command)
    {
        return command switch
        {
            "Action1" => "Some description 1",
            "Action2" => "Some description 2",
            _ => "Some description",
        };
    }
}
