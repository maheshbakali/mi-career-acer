using System.Text.RegularExpressions;

namespace MiCareerAcer.Api.Services;

public class HtmlToPlainTextService : IHtmlToPlainTextService
{
    public string ToPlainText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return "";

        var text = Regex.Replace(html, "<script[^>]*>.*?</script>", " ", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<style[^>]*>.*?</style>", " ", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<br\\s*/?>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "</p>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<[^>]+>", " ");
        text = System.Net.WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, "[ \t]+", " ");
        text = Regex.Replace(text, "(\r?\n){3,}", "\n\n");
        return text.Trim();
    }
}
