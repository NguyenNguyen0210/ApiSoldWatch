namespace ShopNN.Shared.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; }
        public List<string>? Errors { get; }

        public AppException(string message, int statusCode = 500, List<string>? errors = null) : base(message)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }
}
