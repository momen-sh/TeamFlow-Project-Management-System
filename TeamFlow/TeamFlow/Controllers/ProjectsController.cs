using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.Authorization;
using TeamFlow.DTOs.Common;
using TeamFlow.DTOs.Projects;
using TeamFlow.Entities;
using TeamFlow.Services.Interfaces;

namespace TeamFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IMapper _mapper;

        public ProjectsController(
            IProjectService projectService,
            IMapper mapper)
        {
            _projectService = projectService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = AppPolicies.ViewProjectPolicy)]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<IEnumerable<ProjectDto>>.Fail("Invalid token"));
            }

            var projects = await _projectService.GetAssignedToUserAsync(userId.Value);

            var result = _mapper.Map<IEnumerable<ProjectDto>>(projects);
            return Ok(ApiResponse<IEnumerable<ProjectDto>>.Ok(result));
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = AppPolicies.ViewProjectPolicy)]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound(ApiResponse<ProjectDto>.Fail("Project not found"));

            return Ok(ApiResponse<ProjectDto>.Ok(_mapper.Map<ProjectDto>(project)));
        }

        [HttpPost]
        [Authorize(Policy = AppPolicies.ManageProjectsPolicy)]
        public async Task<IActionResult> Create(CreateProjectDto dto)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var project = _mapper.Map<Project>(dto);
            project.OwnerId = userId.Value;

            await _projectService.CreateAsync(project);

            var result = _mapper.Map<ProjectDto>(project);
            return CreatedAtAction(
                nameof(GetById),
                new { id = project.Id },
                ApiResponse<ProjectDto>.Ok(result, "Project created successfully"));
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = AppPolicies.ManageProjectsPolicy)]
        public async Task<IActionResult> Update(int id, UpdateProjectDto dto)
        {
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound(ApiResponse<ProjectDto>.Fail("Project not found"));

            _mapper.Map(dto, project);
            var updated = await _projectService.UpdateAsync(project);
            if (!updated.Succeeded || updated.Data is null)
                return BadRequest(ApiResponse<ProjectDto>.Fail(updated.Error ?? "Project could not be updated"));

            return Ok(ApiResponse<ProjectDto>.Ok(_mapper.Map<ProjectDto>(updated.Data), "Project updated successfully"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppPolicies.ManageProjectsPolicy)]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound(ApiResponse<object>.Fail("Project not found"));

            var deleted = await _projectService.DeleteAsync(id);
            if (!deleted)
                return NotFound(ApiResponse<object>.Fail("Project not found"));

            return NoContent();
        }

        [HttpPost("{projectId:int}/assign-users")]
        [Authorize(Policy = AppPolicies.ManageProjectsPolicy)]
        public async Task<IActionResult> AssignUsers(int projectId, AssignProjectUsersDto dto)
        {
            var project = await _projectService.GetByIdAsync(projectId);
            if (project is null)
                return NotFound(ApiResponse<object>.Fail("Project not found"));

            var assigned = await _projectService.AssignUsersAsync(projectId, dto.UserIds);
            if (!assigned.Succeeded)
                return BadRequest(ApiResponse<object>.Fail(assigned.Error ?? "Users could not be assigned"));

            return Ok(ApiResponse<object>.Ok(new { assignedCount = assigned.Data }, "Users assigned successfully"));
        }

        [HttpDelete("{projectId:int}/assign-users/{userId:int}")]
        [Authorize(Policy = AppPolicies.ManageProjectsPolicy)]
        public async Task<IActionResult> UnassignUser(int projectId, int userId)
        {
            var project = await _projectService.GetByIdAsync(projectId);
            if (project is null)
                return NotFound(ApiResponse<object>.Fail("Project not found"));

            var result = await _projectService.UnassignUserAsync(projectId, userId);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.Fail(result.Error ?? "User could not be unassigned"));

            return Ok(ApiResponse<object>.Ok(
                new { unassignedCount = result.Data },
                "User unassigned successfully"
            ));
        }
    }
}
