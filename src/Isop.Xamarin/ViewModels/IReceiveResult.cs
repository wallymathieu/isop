using System;
using Isop.Client.Transfer;
namespace Isop.Xamarin
{
    public interface IReceiveResult
    {
        string Result { get; set; }
        IErrorMessage[] Errors { get; set; }
    }
}
