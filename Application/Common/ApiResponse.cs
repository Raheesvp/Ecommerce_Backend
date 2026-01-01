using System.Text.Json.Serialization;



    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }

        
        public bool Success => StatusCode >= 200 && StatusCode < 300;

         

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public T? Data { get; set; }

        public ApiResponse() { }

        public ApiResponse(int statusCode, string? message = null, T? data = default)
        {
            StatusCode = statusCode;
            Message = message;
            Data = data;
        }

        public static ApiResponse<T> SuccessResponse(T data, int statusCode = 200, string? message = null)
        {
            return new ApiResponse<T>(statusCode, message ?? "Success", data);
        }

        public static ApiResponse<T> FailureResponse(int statusCode, string message)
        {
            return new ApiResponse<T>(statusCode, message);
        }
    }
