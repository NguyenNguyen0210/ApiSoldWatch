using ShopNN.Shared.Exeptions;

namespace ShopNN.Shared.Exceptions
{
    public class SecurityTokenException : AppException
    {
        public SecurityTokenException(string message ="Invalid Token", int statusCode = 401) : base(message, statusCode)
        {
        }
    }
}
