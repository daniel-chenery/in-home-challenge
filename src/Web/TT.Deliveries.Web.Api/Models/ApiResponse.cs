namespace TT.Deliveries.Web.Api.Models
{
    public class ApiResponse
    {
        public static ApiResponse<T> Success<T>(T result) => new()
        {
            Result = result,
            Success = true
        };

        public static ApiResponse<object> Success() => new()
        {
            Success = true
        };

        public static ApiResponse<T> Failed<T>(string errorMessage) => new()
        {
            Result = default,
            Success = false,
            Error = errorMessage
        };

        public static ApiResponse<object> Failed(string errorMessage) => new()
        {
            Success = false,
            Error = errorMessage
        };
    }
}