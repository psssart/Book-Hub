using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.Extensions.FileProviders;

namespace WebApp.Infrastructure.Email;

public interface IEmailTemplateService
{
    Task<string> RenderAsync(string templateName, IDictionary<string, string> vars, CancellationToken ct = default);
    Task<string> RenderConfirmEmailAsync(string callbackUrl, CancellationToken ct = default);
    Task<string> RenderResetPasswordAsync(string callbackUrl, CancellationToken ct = default);
}

public sealed class EmailTemplateService : IEmailTemplateService
{
    private readonly IFileProvider _files;
    private readonly string _root;
    private readonly HtmlEncoder _html;
    public EmailTemplateService(IWebHostEnvironment env, HtmlEncoder htmlEncoder)
    {
        _files = env.ContentRootFileProvider;
        _root = Path.Combine("Infrastructure", "Email", "EmailTemplates");
        _html = htmlEncoder;
    }

    public async Task<string> RenderAsync(string templateName, IDictionary<string, string> vars, CancellationToken ct = default)
    {
        if (templateName != Path.GetFileName(templateName))
            throw new ArgumentException("Invalid template name", nameof(templateName));

        var relativePath = Path.Combine(_root, templateName).Replace('\\', '/');
        var file = _files.GetFileInfo(relativePath);
        if (!file.Exists)
            throw new FileNotFoundException($"Email template not found: {templateName}", relativePath);

        using var stream = file.CreateReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var html = await reader.ReadToEndAsync(ct);
        
        foreach (var kv in vars)
        {
            var token = "{{" + kv.Key + "}}";
            html = html.Replace(token, _html.Encode(kv.Value));
        }

        return html;
    }

    public Task<string> RenderConfirmEmailAsync(string callbackUrl, CancellationToken ct = default) =>
        RenderAsync("ConfirmEmail.html", new Dictionary<string, string> { ["CallbackUrl"] = callbackUrl }, ct);

    public Task<string> RenderResetPasswordAsync(string callbackUrl, CancellationToken ct = default) =>
        RenderAsync("ResetPassword.html", new Dictionary<string, string> { ["CallbackUrl"] = callbackUrl }, ct);
}