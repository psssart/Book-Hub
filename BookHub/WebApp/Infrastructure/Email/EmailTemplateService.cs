using System.Net;
using System.Text;

namespace WebApp.Infrastructure.Email;

public interface IEmailTemplateService
{
    Task<string> RenderAsync(string templateName, IDictionary<string, string> vars, CancellationToken ct = default);
    Task<string> RenderConfirmEmailAsync(string callbackUrl, CancellationToken ct = default);
    Task<string> RenderResetPasswordAsync(string callbackUrl, CancellationToken ct = default);
}

public sealed class EmailTemplateService : IEmailTemplateService
{
    private readonly string _root;
    public EmailTemplateService(IWebHostEnvironment env) =>
        _root = Path.Combine(env.ContentRootPath, "Infrastructure", "Email", "EmailTemplates");

    public async Task<string> RenderAsync(string templateName, IDictionary<string, string> vars, CancellationToken ct = default)
    {
        var path = Path.Combine(_root, templateName);
        if (!File.Exists(path)) throw new FileNotFoundException($"Email template not found: {templateName}", path);

        var html = await File.ReadAllTextAsync(path, Encoding.UTF8, ct);
        
        foreach (var kv in vars)
        {
            var token = "{{" + kv.Key + "}}";
            html = html.Replace(token, WebUtility.HtmlEncode(kv.Value));
        }
        return html;
    }

    public Task<string> RenderConfirmEmailAsync(string callbackUrl, CancellationToken ct = default) =>
        RenderAsync("ConfirmEmail.html", new Dictionary<string, string> { ["CallbackUrl"] = callbackUrl }, ct);

    public Task<string> RenderResetPasswordAsync(string callbackUrl, CancellationToken ct = default) =>
        RenderAsync("ResetPassword.html", new Dictionary<string, string> { ["CallbackUrl"] = callbackUrl }, ct);
}