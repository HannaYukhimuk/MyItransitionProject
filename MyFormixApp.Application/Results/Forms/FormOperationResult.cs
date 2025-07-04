namespace MyFormixApp.Application.Results.Forms
{
    public class FormOperationResult : OperationResult
    {
        public string RedirectAction { get; set; } = "Details";

        public FormOperationResult(bool isSuccess, string message) : base(isSuccess, message) { }
        public FormOperationResult(string errorMessage) : base(errorMessage) { }
    }
}