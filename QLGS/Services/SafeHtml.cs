using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace QLGiaSuLopHoc.Services
{
    /// <summary>
    /// Bộ lọc HTML dạng "danh sách trắng" để chống XSS với nội dung từ CKEditor (phần Bio gia sư).
    /// Chỉ giữ lại các thẻ định dạng an toàn, loại bỏ mọi thuộc tính (trừ href http/https của thẻ a).
    /// </summary>
    public static class SafeHtml
    {
        private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
        {
            "p", "br", "strong", "b", "em", "i", "u", "s", "ul", "ol", "li",
            "h2", "h3", "h4", "blockquote", "a", "span", "hr"
        };

        private static readonly Regex DangerousBlocks = new(
            @"<(script|style|iframe|object|embed|svg|math)\b[^>]*>.*?</\1\s*>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex Tag = new(
            @"<(/?)([a-zA-Z][a-zA-Z0-9]*)\b([^>]*)>", RegexOptions.Compiled);

        private static readonly Regex Href = new(
            @"href\s*=\s*(?:""([^""]*)""|'([^']*)')", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string Clean(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            html = DangerousBlocks.Replace(html, string.Empty);
            var sb = new StringBuilder();
            int last = 0;

            foreach (Match m in Tag.Matches(html))
            {
                sb.Append(EscapeText(html.Substring(last, m.Index - last)));
                last = m.Index + m.Length;

                var closing = m.Groups[1].Value == "/";
                var name = m.Groups[2].Value.ToLowerInvariant();
                if (!Allowed.Contains(name)) continue;

                if (closing)
                {
                    sb.Append("</").Append(name).Append('>');
                    continue;
                }

                if (name == "a")
                {
                    var hm = Href.Match(m.Groups[3].Value);
                    var url = hm.Success ? WebUtility.HtmlDecode(hm.Groups[1].Success ? hm.Groups[1].Value : hm.Groups[2].Value).Trim() : "";
                    if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append("<a href=\"").Append(WebUtility.HtmlEncode(url))
                          .Append("\" target=\"_blank\" rel=\"noopener noreferrer nofollow\">");
                    }
                    else
                    {
                        sb.Append("<a>");
                    }
                }
                else
                {
                    sb.Append('<').Append(name).Append('>');
                }
            }

            sb.Append(EscapeText(html.Substring(last)));
            return sb.ToString();
        }

        // Văn bản thường: chỉ vô hiệu hóa ký tự < > còn sót (không mã hóa '&' để tránh mã hóa kép các entity của CKEditor).
        private static string EscapeText(string text) => text.Replace("<", "&lt;").Replace(">", "&gt;");

        /// <summary>Lấy văn bản thuần (bỏ thẻ) để hiển thị đoạn trích ngắn.</summary>
        public static string StripTags(string? html, int maxLength = 160)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;
            var text = WebUtility.HtmlDecode(Regex.Replace(html, "<[^>]*>", " "));
            text = Regex.Replace(text, @"\s+", " ").Trim();
            return text.Length <= maxLength ? text : text[..maxLength].TrimEnd() + "…";
        }
    }
}
