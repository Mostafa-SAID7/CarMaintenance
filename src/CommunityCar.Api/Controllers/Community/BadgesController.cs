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

public class BadgeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public Domain.Entities.Community.BadgeType Type { get; set; }
    public Domain.Entities.Community.BadgeRarity Rarity { get; set; }
    public int PointsValue { get; set; }
    public string? Criteria { get; set; }
    public bool IsActive { get; set; }
    public int AwardedCount { get; set; }
}

public class UserBadgeDto
{
    public int Id { get; set; }
    public int BadgeId { get; set; }
    public BadgeDto Badge { get; set; } = null!;
    public string AwardedById { get; set; } = string.Empty;
    public string AwardedByName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime AwardedAt { get; set; }
    public bool IsDisplayed { get; set; }
}

public class ReputationScoreDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public int PostsScore { get; set; }
    public int CommentsScore { get; set; }
    public int VotesReceivedScore { get; set; }
    public int ModerationScore { get; set; }
    public int BadgesScore { get; set; }
    public Domain.Entities.Community.ReputationLevel Level { get; set; }
    public int NextLevelThreshold { get; set; }
    public int BadgeCount { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class ReputationLeaderboardDto
{
    public int Rank { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public Domain.Entities.Community.ReputationLevel Level { get; set; }
    public int BadgeCount { get; set; }
}

public class AwardBadgeRequest
{
    public int BadgeId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}