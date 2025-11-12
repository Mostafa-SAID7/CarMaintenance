using AutoMapper;
using CommunityCar.Application.DTOs.Community;
using CommunityCar.Application.DTOs.Community.Badge;
using CommunityCar.Application.DTOs.Community.Conversations;
using CommunityCar.Application.DTOs.Community.Groups;
using CommunityCar.Application.DTOs.Community.Moderation;
using CommunityCar.Application.DTOs.Community.Posts;
using CommunityCar.Domain.Entities.Community;

namespace CommunityCar.Application.Mappings;

public class CommunityMappingProfile : Profile
{
    public CommunityMappingProfile()
    {
        // Post mappings
        CreateMap<CreatePostRequest, Post>()
            .ForMember(dest => dest.ForumId, opt => opt.MapFrom(src => src.ForumId))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

        CreateMap<UpdatePostRequest, Post>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

        // Comment mappings
        CreateMap<CreateCommentRequest, Comment>()
            .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.PostId))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.ParentCommentId, opt => opt.MapFrom(src => src.ParentCommentId));

        CreateMap<UpdateCommentRequest, Comment>()
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content));

        // Vote mappings
        CreateMap<VotePostRequest, PostVote>()
            .ForMember(dest => dest.PostId, opt => opt.MapFrom(src => src.PostId))
            .ForMember(dest => dest.IsUpvote, opt => opt.MapFrom(src => src.IsUpvote));

        CreateMap<VoteCommentRequest, CommentVote>()
            .ForMember(dest => dest.CommentId, opt => opt.MapFrom(src => src.CommentId))
            .ForMember(dest => dest.IsUpvote, opt => opt.MapFrom(src => src.IsUpvote));

        // Conversation mappings
        CreateMap<CreateConversationRequest, Conversation>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.IsGroupChat, opt => opt.MapFrom(src => src.IsGroupChat));

        CreateMap<SendMessageRequest, Message>()
            .ForMember(dest => dest.ConversationId, opt => opt.MapFrom(src => src.ConversationId))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.MessageType, opt => opt.MapFrom(src => src.MessageType));

        // Group mappings
        CreateMap<CreateGroupRequest, Group>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.IsPrivate, opt => opt.MapFrom(src => src.IsPrivate));

        // Moderation mappings
        CreateMap<CreateReportRequest, ModerationReport>()
            .ForMember(dest => dest.ReportedContentId, opt => opt.MapFrom(src => src.ContentId))
            .ForMember(dest => dest.ReportedContentType, opt => opt.MapFrom(src => src.ContentType))
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

        CreateMap<ModerateContentRequest, ModerationReport>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Action))
            .ForMember(dest => dest.ModeratorNotes, opt => opt.MapFrom(src => src.Reason));

        CreateMap<ResolveReportRequest, ModerationReport>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.ModeratorNotes, opt => opt.MapFrom(src => src.Notes));

        // Badge mappings
        CreateMap<AwardBadgeRequest, UserBadge>()
            .ForMember(dest => dest.BadgeId, opt => opt.MapFrom(src => src.BadgeId))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

        // Entity to DTO mappings (for responses where DTOs exist)
        CreateMap<Post, PostDto>();
        CreateMap<Comment, CommentDto>();
        CreateMap<Conversation, ConversationDto>();
        CreateMap<Message, MessageDto>();
        CreateMap<ConversationParticipant, ConversationParticipantDto>();
        CreateMap<Group, GroupDto>();
        CreateMap<GroupMember, GroupMemberDto>();
        CreateMap<GroupEvent, GroupEventDto>();
        CreateMap<Forum, ForumDto>();
        CreateMap<ForumCategory, ForumCategoryDto>();
        CreateMap<Badge, BadgeDto>();
        CreateMap<UserBadge, UserBadgeDto>();
        CreateMap<ReputationScore, ReputationScoreDto>();
        CreateMap<ModerationReport, ModerationReportDto>();
    }
}