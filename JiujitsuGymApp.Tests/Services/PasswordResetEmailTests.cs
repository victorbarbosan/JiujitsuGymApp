using JiujitsuGymApp.Services;

namespace JiujitsuGymApp.Tests.Services;

public sealed class PasswordResetEmailTests
{
    // A realistic link: Identity tokens are Base64 and the query string carries
    // two parameters, so both the '&' separator and the percent-encoding have to
    // survive into the markup.
    private const string Link =
        "https://gym.example.com/Account/ResetPassword?email=member%40example.com&token=CfDJ8A%2Bb%2Fc";

    [Fact]
    public void Create_AddressesTheMessageAndSetsTheSubject()
    {
        var message = PasswordResetEmail.Create("member@example.com", Link);

        Assert.Equal("member@example.com", message.To);
        Assert.Equal(PasswordResetEmail.Subject, message.Subject);
    }

    /// <summary>
    /// A bare '&amp;' between query parameters is not valid inside an href, so the
    /// template has to escape it. Getting this wrong is invisible until a mail
    /// client drops everything after the '&amp;' and the token arrives truncated.
    /// </summary>
    [Fact]
    public void Create_EscapesTheLinkForUseInAnHref()
    {
        var message = PasswordResetEmail.Create("member@example.com", Link);

        Assert.Contains("&amp;token=", message.HtmlBody);
        Assert.DoesNotContain("?email=member%40example.com&token=", message.HtmlBody);

        // The percent-encoding of the token itself must be left alone.
        Assert.Contains("CfDJ8A%2Bb%2Fc", message.HtmlBody);
    }

    [Fact]
    public void Create_RendersBothTheButtonAndTheCopyPasteFallback()
    {
        var message = PasswordResetEmail.Create("member@example.com", Link);

        // Two hrefs: the CTA button and the visible fallback URL beneath it.
        var hrefCount = message.HtmlBody.Split("href=\"https://gym.example.com").Length - 1;
        Assert.Equal(2, hrefCount);

        Assert.Contains("Reset Password", message.HtmlBody);
        Assert.Contains("expire in <strong>24 hours</strong>", message.HtmlBody);
    }

    [Fact]
    public void Create_ProducesAStandaloneHtmlDocument()
    {
        var message = PasswordResetEmail.Create("member@example.com", Link);

        // Mail clients are far happier with a full document than a fragment.
        Assert.StartsWith("<!DOCTYPE html>", message.HtmlBody.TrimStart());
        Assert.Contains("</html>", message.HtmlBody);
    }
}
