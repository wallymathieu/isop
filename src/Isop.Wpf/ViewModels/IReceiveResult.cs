using System;
using Isop.Client.Transfer;
namespace Isop.Gui.ViewModels
{
    public interface IReceiveResult
    {
        string Result { get; set; }
        IErrorMessage[] Errors { get; set; }
    }
}
