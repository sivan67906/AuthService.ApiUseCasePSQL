using System.Text.RegularExpressions;

namespace AuthService.Application.Common.Helpers;

public static class InputSanitizer
{
    // Pattern to validate email
    private static readonly Regex EmailValidationPattern = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);

    /// <summary>
    /// Sanitizes general text input by removing HTML, scripts, and encoding special characters
    /// </summary>
    public static string SanitizeText(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        string sanitized = input;

        // Step 1: Remove script tags with their content (case-insensitive)
        sanitized = RemoveScriptTags(sanitized);

        // Step 2: Remove remaining HTML tags (keeping their content)
        sanitized = RemoveHtmlTags(sanitized);

        // Step 3: Encode special characters for security
        // Important: Encode & first to avoid double-encoding
        sanitized = sanitized
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");

        return sanitized.Trim();
    }

    private static string RemoveScriptTags(string input)
    {
        // Simple approach: find <script and </script> and remove everything between
        string result = input;
        int maxIterations = 100; // Prevent infinite loops
        int iterations = 0;

        while (iterations < maxIterations)
        {
            int scriptStart = result.IndexOf("<script", StringComparison.OrdinalIgnoreCase);
            if (scriptStart == -1) break;

            int scriptEnd = result.IndexOf("</script>", scriptStart, StringComparison.OrdinalIgnoreCase);
            if (scriptEnd == -1)
            {
                // No closing tag found, just remove the opening tag portion
                int tagEnd = result.IndexOf('>', scriptStart);
                if (tagEnd != -1)
                {
                    result = result.Remove(scriptStart, tagEnd - scriptStart + 1);
                }
                else
                {
                    break;
                }
            }
            else
            {
                // Remove from <script to </script> inclusive
                result = result.Remove(scriptStart, scriptEnd - scriptStart + "</script>".Length);
            }
            iterations++;
        }

        return result;
    }

    private static string RemoveHtmlTags(string input)
    {
        // Simple approach: remove anything between < and > that looks like a tag
        var result = new System.Text.StringBuilder();
        bool inTag = false;
        bool tagHasLetter = false;
        var tagContent = new System.Text.StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '<')
            {
                // Check if this looks like an HTML tag (next char is letter or /)
                if (i + 1 < input.Length)
                {
                    char next = input[i + 1];
                    if (char.IsLetter(next) || next == '/')
                    {
                        inTag = true;
                        tagHasLetter = char.IsLetter(next);
                        tagContent.Clear();
                        tagContent.Append(c);
                        continue;
                    }
                }
                result.Append(c);
            }
            else if (c == '>' && inTag)
            {
                // End of tag - if it had a letter after <, skip it (it's an HTML tag)
                inTag = false;
                if (!tagHasLetter)
                {
                    result.Append(tagContent.ToString());
                    result.Append(c);
                }
                tagContent.Clear();
            }
            else if (inTag)
            {
                tagContent.Append(c);
            }
            else
            {
                result.Append(c);
            }
        }

        // If we ended while still in a tag, append what we have
        if (inTag)
        {
            result.Append(tagContent.ToString());
        }

        return result.ToString();
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
        var result = new System.Text.StringBuilder();
        foreach (char c in input)
        {
            if (char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '%' || c == '+' || c == '-' || c == '@')
            {
                result.Append(c);
            }
        }
        return result.ToString().Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Checks if email is valid
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailValidationPattern.IsMatch(email);
    }

    /// <summary>
    /// Checks if input contains potentially dangerous content
    /// </summary>
    public static bool ContainsDangerousContent(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Check for angle brackets
        if (input.Contains('<') || input.Contains('>'))
            return true;

        // Check for javascript: protocol
        if (input.Contains("javascript:", StringComparison.OrdinalIgnoreCase))
            return true;

        // Check for event handlers
        if (input.Contains("onerror=", StringComparison.OrdinalIgnoreCase))
            return true;

        if (input.Contains("onload=", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
