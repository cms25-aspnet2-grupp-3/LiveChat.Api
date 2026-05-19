using LiveChat.Api.Stores;
using Microsoft.AspNetCore.SignalR;

namespace LiveChat.Api.Hubs;

public class LiveChatHub(LiveChatCacheStore store) : Hub
{
    private const int MaxMessageLength = 2000;

    private static string GetGroupName(int courseId)
        => $"course:{courseId}";

    public async Task JoinChat(int courseId)
    {
        if (courseId <= 0)
            throw new HubException("Course Id is required.");

        var groupName = GetGroupName(courseId);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        var messages = store.GetMessages(courseId);

        await Clients.Caller.SendAsync("LoadMessages", messages);
    }

    public async Task LeaveChat(int courseId)
    {
        if (courseId <= 0)
            return;

        var groupName = GetGroupName(courseId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task EndChat(int courseId)
    {
        if (courseId <= 0)
            return;

        store.ClearMessages(courseId);

        var groupName = GetGroupName(courseId);

        await Clients
            .Group(groupName)
            .SendAsync("ChatEnded");
    }

    public async Task SendMessage(int courseId, string user, string text, string? imageUrl = null)
    {
        if (courseId <= 0)
            throw new HubException("Course Id is required.");

        if (string.IsNullOrWhiteSpace(user))
            user = "Anonymous";

        if (string.IsNullOrWhiteSpace(text))
            return;

        var trimmedText = text.Trim();

        if (trimmedText.Length > MaxMessageLength)
            throw new HubException("Message is too long");

        try
        {
            var message = store.AddMessage(courseId, user, trimmedText, imageUrl);

            var groupName = GetGroupName(courseId);

            await Clients
                .Group(groupName)
                .SendAsync("ReceiveMessage", message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SendMessage: {ex.Message}");
            throw new HubException("Internal server error when sending message.");
        }
    }
}