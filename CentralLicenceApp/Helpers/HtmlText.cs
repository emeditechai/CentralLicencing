using System.Net;
using System.Text.RegularExpressions;

namespace CentralLicenceApp.Helpers
{
    /// <summary>Converts rich-text / email HTML to readable plain text with line breaks kept.</summary>
    public static class HtmlText
    {
        public static string ToPlainText(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            var text = Regex.Replace(html, @"<(script|style|head)[^>]*>.*?</\1>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"<li[^>]*>", "• ", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"</(p|div|tr|li|h[1-6]|table|ul|ol|blockquote|pre)>", "\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"</t[dh]>", "  ", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "<[^>]+>", "");
            text = WebUtility.HtmlDecode(text).Replace(' ', ' ');
            text = Regex.Replace(text, @"[ \t]+", " ");
            text = Regex.Replace(text, @" *\n *", "\n");
            text = Regex.Replace(text, @"\n{3,}", "\n\n");
            return text.Trim();
        }

        public static string Truncate(string text, int max) =>
            text.Length <= max ? text : text[..max].TrimEnd() + "…";
    }
}
