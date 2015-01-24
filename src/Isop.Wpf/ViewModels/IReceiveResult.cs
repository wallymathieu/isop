using System;
using Isop.Client.Models;
namespace Isop.Gui.ViewModels
{
    public interface IReceiveResult
    {
        string Result { get; set; }
        string ErrorMessage { get; set; }
        IErrorMessage Error { get; set; }
    }
}
