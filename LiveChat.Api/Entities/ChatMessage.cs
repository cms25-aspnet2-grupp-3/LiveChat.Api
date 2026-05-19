namespace LiveChat.Api.Entities;

public record ChatMessage
(
    string MessageId,
    int CourseId,
    string User,
    string Text,
    DateTimeOffset createdAtUtc,
    string? ImageUrl = null
);
