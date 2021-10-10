namespace TT.Deliveries.Web.Api.Models
{
    public class ApiResponse<T>
    {
        public T? Result { get; init; }

        public bool Success { get; init; }

        public string? Error { get; init; }
    }
}