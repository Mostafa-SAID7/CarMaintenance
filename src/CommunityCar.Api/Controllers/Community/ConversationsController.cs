using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Api.Controllers.Community;

[ApiController]
[Route("api/community/[controller]")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ConversationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyConversations()
    {
        // TODO: Implement GetUserConversationsQuery
        // var query = new GetUserConversationsQuery
        // {
        //     UserId = User.Identity?.Name ?? string.Empty
        // };
        // var conversations = await _mediator.Send(query);
        // return Ok(conversations);
        return NotImplemented();
    }

    [HttpGet("{id}/messages")]
    [ProducesResponseType(typeof(IEnumerable<MessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversationMessages(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        // TODO: Implement GetConversationMessagesQuery
        // var query = new GetConversationMessagesQuery
        // {
        //     ConversationId = id,
        //     UserId = User.Identity?.Name ?? string.Empty,
        //     Page = page,
        //     PageSize = pageSize
        // };
        // var messages = await _mediator.Send(query);
        // return Ok(messages);
        return NotImplemented();
    }

    [HttpPost("{id}/messages")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageRequest request)
    {
        // TODO: Implement SendMessageCommand
        // var command = new SendMessageCommand
        // {
        //     ConversationId = id,
        //     SenderId = User.Identity?.Name ?? string.Empty,
        //     Content = request.Content,
        //     MessageType = request.MessageType,
        //     AttachmentUrl = request.AttachmentUrl,
        //     ReplyToMessageId = request.ReplyToMessageId
        // };

        // var messageId = await _mediator.Send(command);
        // return CreatedAtAction(nameof(GetConversationMessages), new { id = messageId }, messageId);
        return NotImplemented();
    }

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        // TODO: Implement CreateConversationCommand
        // var command = new CreateConversationCommand
        // {
        //     CreatorId = User.Identity?.Name ?? string.Empty,
        //     Title = request.Title,
        //     Type = request.Type,
        //     ParticipantIds = request.ParticipantIds
        // };

        // var conversationId = await _mediator.Send(command);
        // return CreatedAtAction(nameof(GetMyConversations), new { id = conversationId }, conversationId);
        return NotImplemented();
    }
}

public class ConversationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Domain.Entities.Community.ConversationType Type { get; set; }
    public string? LastMessagePreview { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public IEnumerable<ConversationParticipantDto> Participants { get; set; } = new List<ConversationParticipantDto>();
}

public class ConversationParticipantDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public Domain.Entities.Community.ConversationParticipantRole Role { get; set; }
    public DateTime? LastReadAt { get; set; }
}

public class MessageDto
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? MessageType { get; set; }
    public string? AttachmentUrl { get; set; }
    public int? ReplyToMessageId { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendMessageRequest
{
    public string Content { get; set; } = string.Empty;
    public string? MessageType { get; set; } = "Text";
    public string? AttachmentUrl { get; set; }
    public int? ReplyToMessageId { get; set; }
}

public class CreateConversationRequest
{
    public string Title { get; set; } = string.Empty;
    public Domain.Entities.Community.ConversationType Type { get; set; }
    public IEnumerable<string> ParticipantIds { get; set; } = new List<string>();
}