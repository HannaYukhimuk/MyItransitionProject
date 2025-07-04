namespace MyFormixApp.Application.Results
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string RedirectAction { get; set; }
        public string RedirectController { get; set; }
    }
}