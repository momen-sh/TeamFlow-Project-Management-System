using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using TeamFlow.Entities;
using TeamFlow.Repositories.Interfaces;

namespace TeamFlow.Authorization
{
    public sealed class ManageUsersRequirement : IAuthorizationRequirement { }

    public sealed class ManageProjectsRequirement : IAuthorizationRequirement { }

    public sealed class ManageTasksRequirement : IAuthorizationRequirement { }

    public sealed class ViewProjectRequirement : IAuthorizationRequirement { }

    public sealed class ViewTaskRequirement : IAuthorizationRequirement { }

    public sealed class UpdateTaskStatusRequirement : IAuthorizationRequirement { }

    public sealed class SelfAssignTaskRequirement : IAuthorizationRequirement { }

    public sealed class ManageUsersHandler : AuthorizationHandler<ManageUsersRequirement>
    {
        private readonly IRoleHierarchyService _roleHierarchy;

        public ManageUsersHandler(IRoleHierarchyService roleHierarchy)
        {
            _roleHierarchy = roleHierarchy;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ManageUsersRequirement requirement)
        {
            if (_roleHierarchy.GetRank(context.User.GetRole()) == _roleHierarchy.GetRank(AppRoles.Admin))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public sealed class ManageProjectsHandler : AuthorizationHandler<ManageProjectsRequirement>
    {
        private readonly IRoleHierarchyService _roleHierarchy;

        public ManageProjectsHandler(IRoleHierarchyService roleHierarchy)
        {
            _roleHierarchy = roleHierarchy;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ManageProjectsRequirement requirement)
        {
            if (_roleHierarchy.IsAtLeast(context.User.GetRole(), AppRoles.TeamLeader))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public sealed class ManageTasksHandler : AuthorizationHandler<ManageTasksRequirement>
    {
        private readonly IRoleHierarchyService _roleHierarchy;

        public ManageTasksHandler(IRoleHierarchyService roleHierarchy)
        {
            _roleHierarchy = roleHierarchy;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ManageTasksRequirement requirement)
        {
            if (_roleHierarchy.IsAtLeast(context.User.GetRole(), AppRoles.TeamLeader))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public sealed class ViewProjectHandler : AuthorizationHandler<ViewProjectRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProjectRepository _projectRepository;

        public ViewProjectHandler(IHttpContextAccessor httpContextAccessor, IProjectRepository projectRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _projectRepository = projectRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ViewProjectRequirement requirement)
        {
            var userId = context.User.GetUserId();
            if (!userId.HasValue)
            {
                return;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return;
            }

            if (!AuthorizationResourceHelpers.TryGetRouteInt(httpContext, "id", out var projectId))
            {
                // List endpoints are filtered in service by membership.
                context.Succeed(requirement);
                return;
            }

            var project = await _projectRepository.GetWithMembersAsync(projectId);
            if (project is not null && AuthorizationResourceHelpers.IsProjectMember(project, userId.Value))
            {
                context.Succeed(requirement);
            }
        }
    }

    public sealed class ViewTaskHandler : AuthorizationHandler<ViewTaskRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITaskRepository _taskRepository;

        public ViewTaskHandler(IHttpContextAccessor httpContextAccessor, ITaskRepository taskRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _taskRepository = taskRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ViewTaskRequirement requirement)
        {
            var userId = context.User.GetUserId();
            if (!userId.HasValue)
            {
                return;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return;
            }

            if (!AuthorizationResourceHelpers.TryGetRouteInt(httpContext, "id", out var taskId))
            {
                context.Succeed(requirement);
                return;
            }

            var task = await _taskRepository.GetWithProjectMembersAsync(taskId);
            if (task is null)
            {
                return;
            }

            if (task.AssignedUserId == userId.Value ||
                AuthorizationResourceHelpers.IsProjectMember(task.Project, userId.Value))
            {
                context.Succeed(requirement);
            }
        }
    }

    public sealed class UpdateTaskStatusHandler : AuthorizationHandler<UpdateTaskStatusRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITaskRepository _taskRepository;
        private readonly IRoleHierarchyService _roleHierarchy;

        public UpdateTaskStatusHandler(
            IHttpContextAccessor httpContextAccessor,
            ITaskRepository taskRepository,
            IRoleHierarchyService roleHierarchy)
        {
            _httpContextAccessor = httpContextAccessor;
            _taskRepository = taskRepository;
            _roleHierarchy = roleHierarchy;
        }

        protected override async Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    UpdateTaskStatusRequirement requirement)
        {
            var userId = context.User.GetUserId();
            if (!userId.HasValue)
                return;

            var role = context.User.GetRole();

            // ✅ Admin bypass
            if (role == AppRoles.Admin)
            {
                context.Succeed(requirement);
                return;
            }

            var isDeveloperOrQa =
                _roleHierarchy.IsAtLeast(role, AppRoles.Developer);

            if (!isDeveloperOrQa)
                return;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null ||
                !AuthorizationResourceHelpers.TryGetRouteInt(httpContext, "id", out var taskId))
                return;

            var task = await _taskRepository.GetByIdAsync(taskId);

            if (task is not null && task.AssignedUserId == userId.Value)
            {
                context.Succeed(requirement);
            }
        }
    }

    public sealed class SelfAssignTaskHandler : AuthorizationHandler<SelfAssignTaskRequirement>
    {
        private readonly IRoleHierarchyService _roleHierarchy;

        public SelfAssignTaskHandler(IRoleHierarchyService roleHierarchy)
        {
            _roleHierarchy = roleHierarchy;
        }

        protected override Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    SelfAssignTaskRequirement requirement)
        {
            var userRank = _roleHierarchy.GetRank(context.User.GetRole());
            var devRank = _roleHierarchy.GetRank(AppRoles.Developer);

            if (userRank >= devRank)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    internal static class AuthorizationResourceHelpers
    {
        public static bool IsProjectMember(Project project, int userId)
        {
            return project.Members.Any(member => member.UserId == userId);
        }

        public static bool TryGetRouteInt(HttpContext httpContext, string key, out int value)
        {
            value = 0;
            if (!httpContext.Request.RouteValues.TryGetValue(key, out var routeValue) || routeValue is null)
            {
                return false;
            }

            return int.TryParse(routeValue.ToString(), out value);
        }
    }
}
