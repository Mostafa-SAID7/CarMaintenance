using CommunityCar.Application.Features.Groups.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Api.Controllers.Community;

[ApiController]
[Route("api/community/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GroupsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("my-groups")]
    [ProducesResponseType(typeof(IEnumerable<GroupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyGroups()
    {
        var query = new GetUserGroupsQuery
        {
            UserId = User.Identity?.Name ?? string.Empty
        };

        var groups = await _mediator.Send(query);
        return Ok(groups);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<GroupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroups([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // TODO: Implement GetGroupsQuery
        // var query = new GetGroupsQuery { Page = page, PageSize = pageSize };
        // var groups = await _mediator.Send(query);
        // return Ok(groups);
        return NotImplemented();
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGroup(int id)
    {
        // TODO: Implement GetGroupByIdQuery
        // var query = new GetGroupByIdQuery { GroupId = id };
        // var group = await _mediator.Send(query);
        // if (group == null)
        //     return NotFound();
        // return Ok(group);
        return NotImplemented();
    }

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
    {
        // TODO: Implement CreateGroupCommand
        // var command = new CreateGroupCommand
        // {
        //     OwnerId = User.Identity?.Name ?? string.Empty,
        //     Name = request.Name,
        //     Description = request.Description,
        //     Privacy = request.Privacy,
        //     CoverImageUrl = request.CoverImageUrl
        // };

        // var groupId = await _mediator.Send(command);
        // return CreatedAtAction(nameof(GetGroup), new { id = groupId }, groupId);
        return NotImplemented();
    }

    [HttpPost("{id}/join")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> JoinGroup(int id)
    {
        // TODO: Implement JoinGroupCommand
        // var command = new JoinGroupCommand
        // {
        //     GroupId = id,
        //     UserId = User.Identity?.Name ?? string.Empty
        // };

        // var result = await _mediator.Send(command);
        // return result ? Ok() : BadRequest();
        return NotImplemented();
    }

    [HttpPost("{id}/leave")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LeaveGroup(int id)
    {
        // TODO: Implement LeaveGroupCommand
        // var command = new LeaveGroupCommand
        // {
        //     GroupId = id,
        //     UserId = User.Identity?.Name ?? string.Empty
        // };

        // var result = await _mediator.Send(command);
        // return result ? Ok() : BadRequest();
        return NotImplemented();
    }

    [HttpGet("{id}/members")]
    [ProducesResponseType(typeof(IEnumerable<GroupMemberDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupMembers(int id)
    {
        // TODO: Implement GetGroupMembersQuery
        // var query = new GetGroupMembersQuery { GroupId = id };
        // var members = await _mediator.Send(query);
        // return Ok(members);
        return NotImplemented();
    }

    [HttpGet("{id}/events")]
    [ProducesResponseType(typeof(IEnumerable<GroupEventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupEvents(int id)
    {
        // TODO: Implement GetGroupEventsQuery
        // var query = new GetGroupEventsQuery { GroupId = id };
        // var events = await _mediator.Send(query);
        // return Ok(events);
        return NotImplemented();
    }
}

public class CreateGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Domain.Entities.Community.GroupPrivacy Privacy { get; set; }
    public string? CoverImageUrl { get; set; }
}

public class GroupMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public Domain.Entities.Community.GroupMemberRole Role { get; set; }
    public Domain.Entities.Community.GroupMemberStatus Status { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class GroupEventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Location { get; set; }
    public int AttendeeCount { get; set; }
}