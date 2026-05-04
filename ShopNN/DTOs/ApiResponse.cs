namespace ShopNN.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }

        public ApiResponse()
        {
            Success = true;
            Errors = new List<string>();
        }

        public ApiResponse(T data, string message = null)
        {
            Success = true;
            Data = data;
            Message = message;
            Errors = new List<string>();
        }

        public static ApiResponse<T> SuccessResult(T data, string message = null)
        {
            return new ApiResponse<T>(data, message);
        }

        public static ApiResponse<T> FailureResult(string message, List<string> errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }

    // Lớp bổ trợ cho các phản hồi không cần dữ liệu (Data)
    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse SuccessResult(string message = null)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                Data = null
            };
        }

        public static new ApiResponse FailureResult(string message, List<string> errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>(),
                Data = null
            };
        }
    }
}
