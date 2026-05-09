namespace ShopNN.Shared.Exeptions
{
    public class BadRequestException : AppException
    {
        public BadRequestException(string message, List<string>? errors = null) : base(message, 400, errors) { }
    }
}
