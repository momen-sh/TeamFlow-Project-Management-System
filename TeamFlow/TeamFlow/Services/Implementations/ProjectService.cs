using AutoMapper.Execution;
using Microsoft.EntityFrameworkCore;
using TeamFlow.Entities;
using TeamFlow.Repositories.Interfaces;
using TeamFlow.Services.Interfaces;
using TeamFlow.Services.Results;

namespace TeamFlow.Services.Implementations
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;

        public ProjectService(IProjectRepository projectRepository, IUserRepository userRepository)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
            => await _projectRepository.GetAllAsync();

        public async Task<IEnumerable<Project>> GetAssignedToUserAsync(int userId)
            => await _projectRepository.GetAssignedToUserAsync(userId);

        public async Task<Project?> GetByIdAsync(int id)
            => await _projectRepository.GetWithMembersAsync(id);

        public async Task<IEnumerable<Project>> GetByWorkspaceIdAsync(int workspaceId)
            => await _projectRepository.GetByWorkspaceIdAsync(workspaceId);

        public async Task<Project> CreateAsync(Project project)
        {
            project.Members.Add(new ProjectMember
            {
                UserId = project.OwnerId,
                Role = "Owner"
            });

            await _projectRepository.AddAsync(project);
            await _projectRepository.SaveAsync();
            return project;
        }

        public async Task<ServiceResult<Project>> UpdateAsync(Project project)
        {
            _projectRepository.Update(project);
            await _projectRepository.SaveAsync();
            return ServiceResult<Project>.Success(project);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _projectRepository.GetWithMembersAsync(id);
            if (entity is null)
                return false;

            _projectRepository.Delete(entity);

            await _projectRepository.SaveAsync();

            return true;
        }

        public async Task<ServiceResult<int>> AssignUsersAsync(int projectId, IEnumerable<int> userIds)
        {
            if (!await _projectRepository.ExistsAsync(projectId))
                return ServiceResult<int>.Failure("Project not found");

            var distinctUserIds = userIds.Distinct().ToList();
            if (distinctUserIds.Count == 0)
            {
                return ServiceResult<int>.Failure("No users provided");
            }

            var existingUserIds = await _userRepository.GetExistingUserIdsAsync(distinctUserIds);
            var missingUserId = distinctUserIds.FirstOrDefault(x => !existingUserIds.Contains(x));
            if (missingUserId > 0)
            {
                return ServiceResult<int>.Failure($"User {missingUserId} not found");
            }

            var assignedCount = await _projectRepository.AssignUsersAsync(projectId, distinctUserIds);
            await _projectRepository.SaveAsync();
            return ServiceResult<int>.Success(assignedCount);
        }
        public async Task<ServiceResult<int>> UnassignUserAsync(int projectId, int userId)
        {
            if (!await _projectRepository.ExistsAsync(projectId))
                return ServiceResult<int>.Failure("Project not found");

            var project = await _projectRepository.GetWithMembersAsync(projectId);
            if (project is null)
                return ServiceResult<int>.Failure("Project not found");

            var member = project.Members
                .FirstOrDefault(x => x.UserId == userId);

            if (member is null)
                return ServiceResult<int>.Failure("User is not assigned to this project");

            if (member.Role == "Owner")
                return ServiceResult<int>.Failure("Owner cannot be removed from project");

            var result = await _projectRepository.UnassignUserAsync(projectId, userId);

            if (result == 0)
                return ServiceResult<int>.Failure("User could not be unassigned");

            await _projectRepository.SaveAsync();

            return ServiceResult<int>.Success(result);
        }
    }
}
