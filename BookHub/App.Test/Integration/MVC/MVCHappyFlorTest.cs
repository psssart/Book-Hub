using System.Net;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Mvc.Testing;
using Shared.Test.Helpers;

namespace Core.Test.Integration.MVC;

[Collection("NonParallel")]
public class MVCHappyFlorTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private string FirstName { get; } = "test";
    private string LastName { get; } = "test";
    private string Email { get; } = "test@test.ee";
    private string Pass { get; } = "Kala.maja0";
    
    public MVCHappyFlorTest(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }
    
    [Fact]
    public async Task RegisterNewUserWithTopic()
    {
        const string registerUri = "/Identity/Account/Register";
        var getRegisterResponse = await _client.GetAsync(registerUri);
        getRegisterResponse.EnsureSuccessStatusCode();

        var getRegisterContent = await HtmlHelpers.GetDocumentAsync(getRegisterResponse);
        
        var formRegister = (IHtmlFormElement) getRegisterContent.QuerySelector("#registerForm")!;
        var formRegisterValues = new Dictionary<string, string>
        {
            ["Input_FirstName"] = FirstName,
            ["Input_LastName"] = LastName,
            ["Input_Email"] = Email,
            ["Input_Password"] = Pass,
            ["Input_ConfirmPassword"] = Pass,
        };
        
        var postRegisterResponse = await _client.SendAsync(formRegister, formRegisterValues);

        Assert.True(postRegisterResponse.StatusCode is HttpStatusCode.Found or HttpStatusCode.OK);

        const string protectedUri = "";
        var getResponse = await _client.GetAsync(protectedUri);
        //Redirects to avatarCreationPage
        Assert.True(getResponse.StatusCode is HttpStatusCode.Found or HttpStatusCode.OK);

        var getAvatarCreationForm = await _client.GetAsync($"/Topics/Create/");
        getRegisterResponse.EnsureSuccessStatusCode();
        var getContent = await HtmlHelpers.GetDocumentAsync(getAvatarCreationForm);
        var formAvatar = (IHtmlFormElement) getContent.QuerySelector("#topicCreate")!;
        var formAvatarValues = new Dictionary<string, string>
        {
            ["DiscussionId"] = Guid.NewGuid().ToString(),
            ["Tittle"] = "This is test tittle.",
            ["Content"] = "This is test content."
        };
        var postAvatarResponse = await _client.SendAsync(formAvatar, formAvatarValues);
        
        Assert.Equal(HttpStatusCode.Found, postAvatarResponse.StatusCode);
    }
    
    [Fact]
    public async Task CreateMessage()
    {
        await RegisterNewUserWithTopic();

        const string protectedUri = "";
        var getResponse = await _client.GetAsync(protectedUri);
        //Redirects to avatarCreationPage
        Assert.True(getResponse.StatusCode is HttpStatusCode.Found or HttpStatusCode.OK);

        var getAvatarCreationForm = await _client.GetAsync($"/Messages/Create/");
        var getContent = await HtmlHelpers.GetDocumentAsync(getAvatarCreationForm);
        var formAvatar = (IHtmlFormElement) getContent.QuerySelector("#messageCreate")!;
        var formAvatarValues = new Dictionary<string, string>
        {
            ["TopicId"] = Guid.NewGuid().ToString(),
            ["AppUserId"] = Guid.NewGuid().ToString(),
            ["Content"] = "This is test content."
        };
        var postAvatarResponse = await _client.SendAsync(formAvatar, formAvatarValues);
        
        Assert.Equal(HttpStatusCode.Found, postAvatarResponse.StatusCode);
    }
    
    [Fact]
    public async Task CreateRating()
    {
        await RegisterNewUserWithTopic();

        const string protectedUri = "";
        var getResponse = await _client.GetAsync(protectedUri);
        //Redirects to avatarCreationPage
        Assert.True(getResponse.StatusCode is HttpStatusCode.Found or HttpStatusCode.OK);

        var getAvatarCreationForm = await _client.GetAsync($"/Ratings/Create/");
        var getContent = await HtmlHelpers.GetDocumentAsync(getAvatarCreationForm);
        var formAvatar = (IHtmlFormElement) getContent.QuerySelector("#ratingCreate")!;
        var formAvatarValues = new Dictionary<string, string>
        {
            ["BookId"] = Guid.NewGuid().ToString(),
            ["AppUserId"] = Guid.NewGuid().ToString(),
            ["Value"] = "4",
            ["Comment"] = "This is test comment."
        };
        var postAvatarResponse = await _client.SendAsync(formAvatar, formAvatarValues);
        
        Assert.Equal(HttpStatusCode.Found, postAvatarResponse.StatusCode);
    }
}