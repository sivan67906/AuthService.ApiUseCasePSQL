using System.Text.RegularExpressions;

namespace AuthService.Application.Common.Helpers;

public static class InputSanitizer
{
    // Patterns to detect and remove
    private static readonly Regex HtmlPattern = new(@"<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex ScriptPattern = new(@"<script[^>]*>.*?</script>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex EmojiPattern = new(@"[\uD800-\uDBFF][\uDC00-\uDFFF]|[\u2600-\u27BF]|[\uE000-\uF8FF]|[\u1F300-\u1F9FF]", RegexOptions.Compiled);
    private static readonly Regex EmailPattern = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
    
    /// <summary>
    /// Sanitizes general text input by removing HTML, scripts, and emojis
    /// </summary>
    public static string SanitizeText(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove HTML tags
        string sanitized = HtmlPattern.Replace(input, string.Empty);
        
        // Remove script tags
        sanitized = ScriptPattern.Replace(sanitized, string.Empty);
        
        // Remove emojis
        sanitized = EmojiPattern.Replace(sanitized, string.Empty);
        
        // Remove dangerous characters
        sanitized = sanitized.Replace("<", "").Replace(">", "")
                             .Replace("\"", "&quot;").Replace("'", "&#39;")
                             .Replace("&", "&amp;");
        
        return sanitized.Trim();
    }

    /// <summary>
    /// Sanitizes string and ensures it doesn't exceed max length
    /// </summary>
    public static string SanitizeWithMaxLength(string? input, int maxLength)
    {
        var sanitized = SanitizeText(input);
        
        if (sanitized.Length > maxLength)
        {
            return sanitized.Substring(0, maxLength);
        }
        
        return sanitized;
    }

    /// <summary>
    /// Validates and sanitizes email
    /// </summary>
    public static string SanitizeEmail(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove everything except valid email characters
        var sanitized = Regex.Replace(input, @"[^a-zA-Z0-9._%+-@]", string.Empty);
        return sanitized.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Checks if email is valid
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailPattern.IsMatch(email);
    }

    /// <summary>
    /// Checks if input contains potentially dangerous content
    /// </summary>
    public static bool ContainsDangerousContent(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return HtmlPattern.IsMatch(input) || 
               ScriptPattern.IsMatch(input) ||
               input.Contains("<") || 
               input.Contains(">") ||
               input.Contains("javascript:", StringComparison.OrdinalIgnoreCase) ||
               input.Contains("onerror=", StringComparison.OrdinalIgnoreCase) ||
               input.Contains("onload=", StringComparison.OrdinalIgnoreCase);
    }
}
