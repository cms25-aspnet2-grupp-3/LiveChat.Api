using LiveChat.Api.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace LiveChat.Api.Stores;

public class LiveChatCacheStore(IMemoryCache cache)
{
    private const int MaxMessagesPerChat = 500;
    private static readonly TimeSpan ChatLifetime = TimeSpan.FromHours(1);

    private static string GetCacheKey(int courseId)
        => $"course:{courseId}:messages";

    public void ClearMessages(int courseId)
        => cache.Remove(GetCacheKey(courseId));

    public ChatMessage AddMessage(int courseId, string user, string text, string? imageUrl = null)
    {
        var message = new ChatMessage(
            Guid.NewGuid().ToString("N"),
            courseId,
            user,
            text,
            DateTimeOffset.UtcNow,
            imageUrl
        );

        var state = GetOrCreateState(courseId);

        lock (state.SyncRoot)
        {
            state.Messages.Add(message);

            if (state.Messages.Count > MaxMessagesPerChat)
            {
                var removeCount = state.Messages.Count - MaxMessagesPerChat;
                state.Messages.RemoveRange(0, removeCount);
            }
        }

        RefreshExpiration(courseId, state);

        return message;
    }

    public IReadOnlyList<ChatMessage> GetMessages(int courseId)
    {
        var state = GetOrCreateState(courseId);

        lock (state.SyncRoot)
        {
            return [.. state.Messages.OrderBy(m => m.createdAtUtc)];
        }
    }

    private void RefreshExpiration(int courseId, ChatStreamState state)
    {
        var cacheKey = GetCacheKey(courseId);

        cache.Set(cacheKey, state, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ChatLifetime,
            SlidingExpiration = ChatLifetime
        });
    }

    private ChatStreamState GetOrCreateState(int courseId)
    {
        var cacheKey = GetCacheKey(courseId);

        var state = cache.Get<ChatStreamState>(cacheKey);

        if (state == null)
        {
            state = new ChatStreamState();

            cache.Set(cacheKey, state, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ChatLifetime,
                SlidingExpiration = ChatLifetime
            });
        }

        return state;
    }

    private class ChatStreamState
    {
        public object SyncRoot { get; set; } = new();
        public List<ChatMessage> Messages { get; } = [];
    }
}