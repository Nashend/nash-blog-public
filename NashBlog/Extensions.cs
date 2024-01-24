using System.Security.Claims;
using System.Text.RegularExpressions;

namespace NashBlog
{
    public static partial class Extensions
    {
        public static string GetUserName(this ClaimsPrincipal principal) => 
            principal.FindFirstValue(AppConstants.ClaimNames.FullName)!;

        public static string GetUserId(this ClaimsPrincipal principal) =>
            principal.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public static string ToSlug(this string text)
        {
            text = SlugRegex().Replace(text.ToLowerInvariant(), "-");

            return text.Replace("--", "-")
                        .Trim('-');
        }

        [GeneratedRegex(@"[^0-9a-z_]")]
        private static partial Regex SlugRegex();

        public static string ToDisplay(this DateTime? dateTime) => dateTime?.ToString("MMM dd") ?? string.Empty;
    }
}
