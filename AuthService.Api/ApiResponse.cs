namespace AuthService.Api;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> FailResponse(string message, List<string>? errors = null, T? data = default)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = data,
            Message = message,
            Errors = errors
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors ?? new List<string>()
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = new List<string> { error }
        };
    }

    /// <summary>
    /// Creates a fail response from an exception, automatically splitting validation errors
    /// if the message contains common delimiters (|||, ;, or newlines).
    /// </summary>
    public static ApiResponse<T> FailFromException(string message, Exception ex)
    {
        var errors = new List<string>();
        var exMessage = ex.Message;

        // Remove common prefixes like "Unable to register user: "
        var prefixPatterns = new[] { "Unable to register user: ", "Registration failed: ", "Validation failed: " };
        foreach (var prefix in prefixPatterns)
        {
            if (exMessage.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                exMessage = exMessage.Substring(prefix.Length);
                break;
            }
        }

        // Check if this is a validation exception with multiple errors
        // Priority: ||| > ; > newlines
        if (exMessage.Contains("|||"))
        {
            errors = exMessage.Split("|||", StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .Where(e => !string.IsNullOrEmpty(e))
                .ToList();
        }
        else if (exMessage.Contains(";"))
        {
            errors = exMessage.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .Where(e => !string.IsNullOrEmpty(e))
                .ToList();
        }
        else if (exMessage.Contains("\n"))
        {
            errors = exMessage.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .Where(e => !string.IsNullOrEmpty(e))
                .ToList();
        }
        else
        {
            errors.Add(exMessage);
        }

        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Message = message,
            Errors = errors
        };
    }
}