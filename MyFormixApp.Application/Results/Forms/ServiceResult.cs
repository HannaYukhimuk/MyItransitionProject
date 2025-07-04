namespace MyFormixApp.Application.Results.Forms
{
    public class ServiceResult<T>
    {
        public bool Success { get; }
        public T? Data { get; }
        public string ErrorMessage { get; }

        public ServiceResult(T data)
        {
            Success = true;
            Data = data;
        }

        public ServiceResult(string errorMessage)
        {
            Success = false;
            ErrorMessage = errorMessage;
        }
    }
}