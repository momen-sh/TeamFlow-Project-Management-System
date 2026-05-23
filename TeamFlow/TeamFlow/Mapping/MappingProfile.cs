using AutoMapper;
using TeamFlow.DTOs.Users;
using TeamFlow.DTOs.Projects;
using TeamFlow.DTOs.Tasks;
using TeamFlow.DTOs.Comments;
using TeamFlow.DTOs.Notifications;
using TeamFlow.Entities;

namespace TeamFlow.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Projects, opt => opt.Ignore())
                .ForMember(dest => dest.Tasks, opt => opt.Ignore());

            CreateMap<CreateProjectDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.Owner, opt => opt.Ignore())
                .ForMember(dest => dest.Workspace, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Tasks, opt => opt.Ignore())
                .ForMember(dest => dest.Members, opt => opt.Ignore());
            CreateMap<UpdateProjectDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.Owner, opt => opt.Ignore())
                .ForMember(dest => dest.Workspace, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Tasks, opt => opt.Ignore())
                .ForMember(dest => dest.Members, opt => opt.Ignore());
            CreateMap<ProjectMember, ProjectMemberDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => (src.User.FirstName + " " + src.User.LastName).Trim()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner != null ? (src.Owner.FirstName + " " + src.Owner.LastName).Trim() : null));

            
            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Projects, opt => opt.Ignore())
                .ForMember(dest => dest.Tasks, opt => opt.Ignore());
            CreateMap<CreateTaskDto, TaskItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedUser, opt => opt.Ignore())
                .ForMember(dest => dest.DueDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                .ForMember(dest => dest.WorkRecords, opt => opt.Ignore())
                .ForMember(dest => dest.QaTestCases, opt => opt.Ignore())
                .ForMember(dest => dest.QaAssignments, opt => opt.Ignore())
                .ForMember(dest => dest.SentToQaAt, opt => opt.Ignore())
                .ForMember(dest => dest.SentToQaByUserId, opt => opt.Ignore());
            CreateMap<UpdateTaskDto, TaskItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedUser, opt => opt.Ignore())
                .ForMember(dest => dest.DueDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                .ForMember(dest => dest.WorkRecords, opt => opt.Ignore())
                .ForMember(dest => dest.QaTestCases, opt => opt.Ignore())
                .ForMember(dest => dest.QaAssignments, opt => opt.Ignore())
                .ForMember(dest => dest.SentToQaAt, opt => opt.Ignore())
                .ForMember(dest => dest.SentToQaByUserId, opt => opt.Ignore());
            CreateMap<TaskItem, TaskDto>()
    .ForMember(
        dest => dest.AssignedUserName,
        opt => opt.MapFrom(src =>
            src.AssignedUser != null
                ? (src.AssignedUser.FirstName + " " + src.AssignedUser.LastName).Trim()
                : null
        )
    )
    .ForMember(
        dest => dest.ProjectName,
        opt => opt.MapFrom(src => src.Project.Name)
    )
    .ForMember(dest => dest.Permissions, opt => opt.Ignore());
            CreateMap<TaskAttachment, TaskAttachmentDto>();
            CreateMap<TaskWorkRecord, TaskWorkRecordDto>()
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src =>
                    src.CreatedByUser != null ? (src.CreatedByUser.FirstName + " " + src.CreatedByUser.LastName).Trim() : null));
            CreateMap<CreateTaskWorkRecordDto, TaskWorkRecord>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.TaskId, opt => opt.Ignore())
                .ForMember(dest => dest.Task, opt => opt.Ignore());
            CreateMap<QaTestCase, QaTestCaseDto>()
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src =>
                    src.CreatedByUser != null ? (src.CreatedByUser.FirstName + " " + src.CreatedByUser.LastName).Trim() : null));
            CreateMap<TaskQaAssignment, TaskQaAssignmentDto>()
                .ForMember(dest => dest.QaUserName, opt => opt.MapFrom(src =>
                    src.QaUser != null ? (src.QaUser.FirstName + " " + src.QaUser.LastName).Trim() : null))
                .ForMember(dest => dest.QaUserEmail, opt => opt.MapFrom(src => src.QaUser != null ? src.QaUser.Email : null));
            CreateMap<CreateQaTestCaseDto, QaTestCase>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.TaskId, opt => opt.Ignore())
                .ForMember(dest => dest.Task, opt => opt.Ignore());

            CreateMap<CreateCommentDto, Comment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TaskItem, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Mentions, opt => opt.Ignore());
            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.TaskItemId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                    src.User != null ? (src.User.FirstName + " " + src.User.LastName).Trim() : null))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.MentionedUserIds, opt => opt.MapFrom(src => src.Mentions.Select(x => x.UserId)));
            CreateMap<Notification, NotificationDto>();
            CreateMap<CreateNotificationDto, Notification>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsRead, opt => opt.Ignore());
        }
    }
}
