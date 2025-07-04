namespace MyFormixApp.Application.Results.Forms
{
    public class FormResult
    {
        public bool IsSuccess { get; }
        public string Message { get; }

        public FormResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public FormResult(string errorMessage) : this(false, errorMessage) { }
    }
}