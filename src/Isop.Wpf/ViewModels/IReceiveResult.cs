using System;
namespace Isop.Gui.ViewModels
{
    public interface IReceiveResult
    {
        string Result { get; set; }
        string ErrorMessage { get; set; }
        object Error { get; set; }
    }
}
