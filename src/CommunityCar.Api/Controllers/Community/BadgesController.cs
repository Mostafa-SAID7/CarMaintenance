using CommunityCar.Application.DTOs.Community;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Api.Controllers.Community;

[ApiController]
[Route("api/community/[controller]")]
public class BadgesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BadgesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<BadgeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBadges()
    {
        // TODO: Implement GetBadgesQuery
        // var query = new GetBadgesQuery();
        // var badges = await _mediator.Send(query);
        // return Ok(badges);
        return NotImplemented();
    }

    [HttpGet("my-badges")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<UserBadgeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyBadges()
    {
        // TODO: Implement GetUserBadgesQuery
        // var query = new GetUserBadgesQuery
        // {
        //     UserId = User.Identity?.Name ?? string.Empty
        // };
        // var badges = await _mediator.Send(query);
        // return Ok(badges);
        return NotImplemented();
    }

    [HttpGet("leaderboard")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ReputationLeaderboardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReputationLeaderboard([FromQuery] int limit = 50)
    {
        // TODO: Implement GetReputationLeaderboardQuery
        // var query = new GetReputationLeaderboardQuery { Limit = limit };
        // var leaderboard = await _mediator.Send(query);
        // return Ok(leaderboard);
        return NotImplemented();
    }

    [HttpGet("user/{userId}/reputation")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReputationScoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserReputation(string userId)
    {
        // TODO: Implement GetUserReputationQuery
        // var query = new GetUserReputationQuery { UserId = userId };
        // var reputation = await _mediator.Send(query);
        // if (reputation == null)
        //     return NotFound();
        // return Ok(reputation);
        return NotImplemented();
    }

    [HttpPost("award")]
    [Authorize]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<IActionResult> AwardBadge([FromBody] AwardBadgeRequest request)
    {
        // TODO: Implement AwardBadgeCommand (Admin/Moderator only)
        // var command = new AwardBadgeCommand
        // {
        //     BadgeId = request.BadgeId,
        //     UserId = request.UserId,
        //     AwardedById = User.Identity?.Name ?? string.Empty,
        //     Reason = request.Reason
        // };

        // var userBadgeId = await _mediator.Send(command);
        // return CreatedAtAction(nameof(GetMyBadges), new { id = userBadgeId }, userBadgeId);
        return NotImplemented();
    }
}