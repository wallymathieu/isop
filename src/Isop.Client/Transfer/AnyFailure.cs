namespace Isop.Client.Transfer
{
    public class AnyFailure : IErrorMessage
    {
        public string Message { get; set; }

        public string Argument { get; set; }

        public string ErrorType { get; set; }
    }
}
