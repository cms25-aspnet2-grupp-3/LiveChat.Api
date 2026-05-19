using LiveChat.Api.Entities;
using LiveChat.Api.Stores;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

public class LiveChatCacheStoreTests
{
    private LiveChatCacheStore CreateStore()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        return new LiveChatCacheStore(cache);
    }

    [Fact]
    public void AddMessage_ShouldStoreMessage()
    {
        var store = CreateStore();

        var result = store.AddMessage(1, "John", "Hello");

        var messages = store.GetMessages(1);

        Assert.Single(messages);
        Assert.Equal("Hello", result.Text);
    }

    [Fact]
    public void AddMessage_ShouldLimitTo500()
    {
        var store = CreateStore();

        for (int i = 0; i < 600; i++)
        {
            store.AddMessage(1, "user", $"msg {i}");
        }

        var messages = store.GetMessages(1);

        Assert.Equal(500, messages.Count);
    }

    [Fact]
    public void GetMessages_ShouldBeSorted()
    {
        var store = CreateStore();

        store.AddMessage(1, "u", "1");
        store.AddMessage(1, "u", "2");

        var messages = store.GetMessages(1).ToList();

        Assert.True(messages[0].createdAtUtc <= messages[1].createdAtUtc);
    }

}
