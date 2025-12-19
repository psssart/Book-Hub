using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebApp.Hubs;

[Authorize]
public class DiscussionHub : Hub
{
    private readonly ILogger<DiscussionHub> _logger;

    public DiscussionHub(ILogger<DiscussionHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("User {UserId} connected to DiscussionHub with connection {ConnectionId}",
            userId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public async Task JoinDiscussion(string discussionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"discussion_{discussionId}");
        _logger.LogInformation("Connection {ConnectionId} joined discussion {DiscussionId}",
            Context.ConnectionId, discussionId);
    }

    public async Task LeaveDiscussion(string discussionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"discussion_{discussionId}");
        _logger.LogInformation("Connection {ConnectionId} left discussion {DiscussionId}",
            Context.ConnectionId, discussionId);
    }

    public async Task JoinTopic(string topicId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"topic_{topicId}");
        _logger.LogInformation("Connection {ConnectionId} joined topic {TopicId}",
            Context.ConnectionId, topicId);
    }

    public async Task LeaveTopic(string topicId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"topic_{topicId}");
        _logger.LogInformation("Connection {ConnectionId} left topic {TopicId}",
            Context.ConnectionId, topicId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (exception != null)
        {
            _logger.LogWarning(exception, "User {UserId} disconnected from DiscussionHub with error", userId);
        }
        else
        {
            _logger.LogInformation("User {UserId} disconnected from DiscussionHub", userId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}
