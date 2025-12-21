using System.Text.Json.Serialization;

public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }



    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Data { get; set; }

    public ApiResponse() { }

    public ApiResponse(int statusCode, string? message = null, T data = default)
    {
        Data = data;
        Message = message;
        StatusCode = statusCode;
    }

    public static ApiResponse<T> Success(T data, string? message = null)
    {
        return new ApiResponse<T>(200, message, data);
    }

    // SUCCESS with no data
    public static ApiResponse<T> Success(string? message = null)
    {
        return new ApiResponse<T>(200, message, default);
    }

    // FAIL helper
    public static ApiResponse<T> Fail(string message, int statusCode = 400)
    {
        return new ApiResponse<T>(statusCode, message);
    }
}
