using System;
using Isop.Client.Models;
namespace Isop.Gui.ViewModels
{
    public interface IReceiveResult
    {
        string Result { get; set; }
        IErrorMessage[] Errors { get; set; }
    }
}
