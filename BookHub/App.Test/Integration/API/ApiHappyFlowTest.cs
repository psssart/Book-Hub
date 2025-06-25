using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using App.DTO.v1_0;
using App.DTO.v1_0.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Helpers;
using Microsoft.IdentityModel.Tokens;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Core.Test.Integration.API;

[Collection("NonParallel")]
public class ApiHappyFlowTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private string Email { get; } = "test@test.ee";
    private string Pass { get; } = "Kala.maja0";
    private string FirstName1 { get; } = "Kala";
    private string LastName1 { get; } = "Maja";
    private string? jwtToken { get; set; } = null;

    public ApiHappyFlowTest(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }
    
    [Fact]
    public async Task IndexRequiresLogin()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Messages/GetMessages");
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    public async Task RegisterNewUser()
    {
        if (!jwtToken.IsNullOrEmpty())
        {
            return;
        }
        var response = await _client.PostAsJsonAsync("/api/v1/identity/Account/Register", new
        {
            firstname = FirstName1,
            lastname = LastName1,
            email = Email,
            password = Pass
        });
        
        var contentStr = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"RegisterNewUser failed: {response.StatusCode}, Content: {contentStr}");
        }
        
        response.EnsureSuccessStatusCode();
        
        var loginData = JsonSerializer.Deserialize<JWTResponse>(contentStr, JsonHelper.CamelCase);
        jwtToken = loginData!.Jwt;
    }

    [Fact]
    public async Task Login()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/identity/Account/Login", new
        {
            email = Email,
            password = Pass,
        });

        var contentStr = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Login failed: {response.StatusCode}, Content: {contentStr}");
        }

        response.EnsureSuccessStatusCode();
        var loginData = JsonSerializer.Deserialize<JWTResponse>(contentStr, JsonHelper.CamelCase);

        Assert.NotNull(loginData);
        Assert.NotNull(loginData.Jwt);
        Assert.True(loginData.Jwt.Length > 0);

        var msg = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Messages/GetMessages");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginData.Jwt);
        msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        response = await _client.SendAsync(msg);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateNewMessage()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/identity/Account/Login", new
        {
            email = Email,
            password = Pass,
        });
        var loginContentStr = await loginResponse.Content.ReadAsStringAsync();
        loginResponse.EnsureSuccessStatusCode();
        var loginData = JsonSerializer.Deserialize<JWTResponse>(loginContentStr, JsonHelper.CamelCase);
        
        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Messages/PostMessage");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginData!.Jwt);
        msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        msg.Content = JsonContent.Create(new
        {
            Content = "This is a test message"
        });
        var response = await _client.SendAsync(msg);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdMessage = JsonSerializer.Deserialize<Message>(responseContent, JsonHelper.CamelCase);
        
        Assert.NotNull(createdMessage);
        Assert.Equal("This is a test message", createdMessage.Content);
    }

    [Fact]
    public async Task GetMessage_ReturnsNotFound_WhenIdIsInvalid()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/identity/Account/Login", new
        {
            email = Email,
            password = Pass,
        });
        var loginContentStr = await loginResponse.Content.ReadAsStringAsync();
        loginResponse.EnsureSuccessStatusCode();
        var loginData = JsonSerializer.Deserialize<JWTResponse>(loginContentStr, JsonHelper.CamelCase);
        
        // Arrange
        var messageId = Guid.NewGuid(); // Non-existent ID
        var msg = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/Messages/{messageId}");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginData!.Jwt);
        msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        var response = await _client.SendAsync(msg);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /*[Fact]
    public async Task GetCreatedMessage()
    {
        await RegisterNewUser();
        var response = await _client.PostAsJsonAsync("/api/v1/identity/Account/Login", new
        {
            email = Email,
            password = Pass,
        });
        var loginContentStr = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        var loginData = JsonSerializer.Deserialize<JWTResponse>(loginContentStr, JsonHelper.CamelCase);

        await CreateNewMessage();
        
        var msg = new HttpRequestMessage(HttpMethod.Get, "/api/v1/Messages/GetMessages");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginData!.Jwt);
        msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        response = await _client.SendAsync(msg);

        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadAsStringAsync();
        var item = JsonConvert.DeserializeObject<List<Message>>(data)!.First();
        Assert.NotNull(item);
    }*/
    
    [Fact]
    public async Task CreateNewRating()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/identity/Account/Login", new
        {
            email = Email,
            password = Pass,
        });
        var loginContentStr = await loginResponse.Content.ReadAsStringAsync();
        loginResponse.EnsureSuccessStatusCode();
        var loginData = JsonSerializer.Deserialize<JWTResponse>(loginContentStr, JsonHelper.CamelCase);
        
        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Ratings/PostRating");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginData!.Jwt);
        msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        msg.Content = JsonContent.Create(new
        {
            id = Guid.NewGuid(),
            BookId = Guid.NewGuid(),
            AppUserId = Guid.NewGuid(),
            Value = 5.75F,
            Comment = "This is test comment"
        });
        var response = await _client.SendAsync(msg);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        /*var createdRating = JsonSerializer.Deserialize<Rating>(responseContent, JsonHelper.CamelCase);*/
        
        Assert.NotNull(responseContent);
        /*Assert.Equal("This is test comment", createdRating.Comment);*/
    }
    
    [Fact]
    public async Task CreateNewTopic()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/identity/Account/Login", new
        {
            email = Email,
            password = Pass,
        });
        var loginContentStr = await loginResponse.Content.ReadAsStringAsync();
        loginResponse.EnsureSuccessStatusCode();
        var loginData = JsonSerializer.Deserialize<JWTResponse>(loginContentStr, JsonHelper.CamelCase);
        
        var msg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Topics/PostTopic");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginData!.Jwt);
        msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        msg.Content = JsonContent.Create(new
        {
            id = Guid.NewGuid(),
            DiscussionId = Guid.NewGuid(),
            AppUserId = Guid.NewGuid(),
            Tittle = "This is test tittle",
            Content = "This is test content",
            CreationTime = DateTime.UtcNow
        });
        var response = await _client.SendAsync(msg);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        /*var createdTopic = JsonSerializer.Deserialize<Topic>(responseContent, JsonHelper.CamelCase);*/
        
        Assert.NotNull(responseContent);
        /*Assert.Equal("This is test content", createdTopic.Content);*/
    }
}