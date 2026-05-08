namespace ShopNN.Exceptions
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

    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, 404) { }
    }

    public class BadRequestException : AppException
    {
        public BadRequestException(string message, List<string>? errors = null) : base(message, 400, errors) { }
    }

    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "Unauthorized") : base(message, 401) { }
    }
}
