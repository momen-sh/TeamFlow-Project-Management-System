using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.Authorization;
using TeamFlow.DTOs.Common;
using TeamFlow.DTOs.Comments;
using TeamFlow.DTOs.Tasks;
using TeamFlow.Entities;
using TeamFlow.Services.Interfaces;

namespace TeamFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ITaskAttachmentService _attachmentService;
        private readonly ITaskWorkRecordService _workRecordService;
        private readonly IQaTestCaseService _qaTestCaseService;
        private readonly ICommentService _commentService;
        private readonly IMapper _mapper;

        public TasksController(
            ITaskService taskService,
            ITaskAttachmentService attachmentService,
            ITaskWorkRecordService workRecordService,
            IQaTestCaseService qaTestCaseService,
            ICommentService commentService,
            IMapper mapper)
        {
            _taskService = taskService;
            _attachmentService = attachmentService;
            _workRecordService = workRecordService;
            _qaTestCaseService = qaTestCaseService;
            _commentService = commentService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = AppPolicies.ViewTaskPolicy)]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<IEnumerable<TaskDto>>.Fail("Invalid token"));
            }

            var tasks = await _taskService.GetVisibleToUserAsync(userId.Value);

            var result = tasks.Select(ToTaskDto);
            return Ok(ApiResponse<IEnumerable<TaskDto>>.Ok(result));
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = AppPolicies.ViewTaskPolicy)]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task is null)
                return NotFound(ApiResponse<TaskDto>.Fail("Task not found"));

            return Ok(ApiResponse<TaskDto>.Ok(ToTaskDto(task)));
        }

        [HttpPost]
        [Authorize(Policy = AppPolicies.ManageTasksPolicy)]
        public async Task<IActionResult> Create(CreateTaskDto dto)
        {
            var validation = ValidateTaskEnums(dto);
            if (validation is not null) return validation;

            var task = _mapper.Map<TaskItem>(dto);
            var created = await _taskService.CreateAsync(task);
            if (!created.Succeeded || created.Data is null)
                return BadRequest(ApiResponse<TaskDto>.Fail(created.Error ?? "Task could not be created"));

            var result = ToTaskDto(created.Data);
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Data.Id },
                ApiResponse<TaskDto>.Ok(result, "Task created successfully"));
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = AppPolicies.ManageTasksPolicy)]
        public async Task<IActionResult> Update(int id, UpdateTaskDto dto)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task is null)
                return NotFound(ApiResponse<TaskDto>.Fail("Task not found"));

            var validation = ValidateTaskEnums(dto);
            if (validation is not null) return validation;

            _mapper.Map(dto, task);
            var updated = await _taskService.UpdateAsync(task);
            if (!updated.Succeeded || updated.Data is null)
                return BadRequest(ApiResponse<TaskDto>.Fail(updated.Error ?? "Task could not be updated"));

            return Ok(ApiResponse<TaskDto>.Ok(ToTaskDto(updated.Data), "Task updated successfully"));
        }

        [HttpPatch("{id:int}/status")]
        [Authorize(Policy = AppPolicies.UpdateTaskStatusPolicy)]
        public async Task<IActionResult> UpdateStatus(int id, UpdateTaskStatusDto dto)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task is null)
                return NotFound(ApiResponse<TaskDto>.Fail("Task not found"));

            if (!Enum.IsDefined(typeof(TaskStatus), dto.Status))
                return BadRequest(ApiResponse<TaskDto>.Fail("Invalid task status"));

            await _taskService.UpdateStatusAsync(task, (TaskStatus)dto.Status);

            return Ok(ApiResponse<TaskDto>.Ok(ToTaskDto(task), "Task status updated successfully"));
        }

        [HttpPost("{id:int}/self-assign")]
        [Authorize(Policy = AppPolicies.SelfAssignTaskPolicy)]
        public async Task<IActionResult> SelfAssign(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<TaskDto>.Fail("Invalid token"));

            var assigned = await _taskService.SelfAssignAsync(id, userId.Value);
            if (!assigned.Succeeded || assigned.Data is null)
                return BadRequest(ApiResponse<TaskDto>.Fail(assigned.Error ?? "Task could not be assigned"));

            return Ok(ApiResponse<TaskDto>.Ok(ToTaskDto(assigned.Data), "Task assigned successfully"));
        }

        [HttpPatch("{id:int}/unassign")]
        [Authorize(Policy = AppPolicies.UnassignTaskPolicy)]
        public async Task<IActionResult> Unassign(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task is null)
                return NotFound(ApiResponse<TaskDto>.Fail("Task not found"));

            var result = await _taskService.UnassignAsync(task);

            if (!result.Succeeded || result.Data is null)
                return BadRequest(ApiResponse<TaskDto>.Fail(result.Error ?? "Unassign failed"));

            return Ok(ApiResponse<TaskDto>.Ok(
                ToTaskDto(result.Data),
                "Task unassigned successfully"));
        }

        [HttpPost("{id:int}/send-to-qa")]
        [Authorize(Policy = AppPolicies.ViewTaskPolicy)]
        public async Task<IActionResult> SendToQa(int id, SendTaskToQaDto dto)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<TaskDto>.Fail("Invalid token"));

            var result = await _taskService.SendToQaAsync(id, dto.QaUserIds, userId.Value, User.GetRole());
            if (!result.Succeeded || result.Data is null)
                return BadRequest(ApiResponse<TaskDto>.Fail(result.Error ?? "Task could not be sent to QA"));

            return Ok(ApiResponse<TaskDto>.Ok(ToTaskDto(result.Data), "Task sent to QA"));
        }

        [HttpGet("{id:int}/work-records")]
        [Authorize(Policy = AppPolicies.ViewTaskPolicy)]
        public async Task<IActionResult> GetWorkRecords(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task is null)
                return NotFound(ApiResponse<IEnumerable<TaskWorkRecordDto>>.Fail("Task not found"));

            var records = await _workRecordService.GetByTaskIdAsync(id);
            return Ok(ApiResponse<IEnumerable<TaskWorkRecordDto>>.Ok(_mapper.Map<IEnumerable<TaskWorkRecordDto>>(records)));
        }

        [HttpPost("{id:int}/work-records")]
        [Authorize(Policy = AppPolicies.ViewTaskPolicy)]
        public async Task<IActionResult> CreateWorkRecord(int id, CreateTaskWorkRecordDto dto)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<TaskWorkRecordDto>.Fail("Invalid token"));

            var record = _mapper.Map<TaskWorkRecord>(dto);
            var created = await _workRecordService.CreateAsync(id, record, userId.Value, User.GetRole());
            if (!created.Succeeded || created.Data is null)
                return BadRequest(ApiResponse<TaskWorkRecordDto>.Fail(created.Error ?? "Work record could not be created"));

            return Ok(ApiResponse<TaskWorkRecordDto>.Ok(_mapper.Map<TaskWorkRecordDto>(created.Data), "Work record added"));
        }

        [HttpGet("{id:int}/qa-test-cases")]
        [Authorize(Policy = AppPolicies.ViewTaskPolicy)]
        public async Task<IActionResult> GetQaTestCases(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task is null)
                return NotFound(ApiResponse<IEnumerable<QaTestCaseDto>>.Fail("Task not found"));

            var testCases = await _qaTestCaseService.GetByTaskIdAsync(id);
            return Ok(ApiResponse<IEnumerable<QaTestCaseDto>>.Ok(_mapper.Map<IEnumerable<QaTestCaseDto>>(testCases)));
        }

        [HttpPost("{id:int}/qa-test-cases")]
        [Authorize(Policy = AppPolicies.ViewTaskPolicy)]
        public async Task<IActionResult> CreateQaTestCase(int id, CreateQaTestCaseDto dto)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<QaTestCaseDto>.Fail("Invalid token"));

            var validation = ValidateQaStatus(dto.Status);
            if (validation is not null) return validation;

            var testCase = _mapper.Map<QaTestCase>(dto);
            var created = await _qaTestCaseService.CreateAsync(id, testCase, userId.Value, User.GetRole());
            if (!created.Succeeded || created.Data is null)
                return BadRequest(ApiResponse<QaTestCaseDto>.Fail(created.Error ?? "QA test case could not be created"));

            return Ok(ApiResponse<QaTestCaseDto>.Ok(_mapper.Map<QaTestCaseDto>(created.Data), "QA test case added"));
        }

        [HttpPatch("{id:int}/qa-test-cases/{testCaseId:int}/status")]
        [Authorize(Policy = AppPolicies.ViewTaskPolicy)]
        public async Task<IActionResult> UpdateQaTestCaseStatus(int id, int testCaseId, UpdateQaTestCaseStatusDto dto)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<QaTestCaseDto>.Fail("Invalid token"));

            var validation = ValidateQaStatus(dto.Status);
            if (validation is not null) return validation;

            var updated = await _qaTestCaseService.UpdateStatusAsync(id, testCaseId, dto.Status, userId.Value, User.GetRole());
            if (!updated.Succeeded || updated.Data is null)
                return BadRequest(ApiResponse<QaTestCaseDto>.Fail(updated.Error ?? "QA test case could not be updated"));

            return Ok(ApiResponse<QaTestCaseDto>.Ok(_mapper.Map<QaTestCaseDto>(updated.Data), "QA test case updated"));
        }

        [HttpGet("{id:int}/comments")]
        [Authorize(Policy = AppPolicies.ViewTaskPolicy)]
        public async Task<IActionResult> GetComments(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task is null)
                return NotFound(ApiResponse<IEnumerable<CommentDto>>.Fail("Task not found"));

            var comments = await _commentService.GetByTaskIdAsync(id);
            return Ok(ApiResponse<IEnumerable<CommentDto>>.Ok(_mapper.Map<IEnumerable<CommentDto>>(comments)));
        }

        [HttpPost("{id:int}/comments")]
        [Authorize(Policy = AppPolicies.ViewTaskPolicy)]
        public async Task<IActionResult> CreateComment(int id, CreateCommentDto dto)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<CommentDto>.Fail("Invalid token"));

            var task = await _taskService.GetByIdAsync(id);
            if (task is null)
                return NotFound(ApiResponse<CommentDto>.Fail("Task not found"));

            dto.TaskItemId = id;
            dto.UserId = userId.Value;
            var comment = _mapper.Map<Comment>(dto);
            await _commentService.CreateAsync(comment, dto.MentionedUserIds);

            return Ok(ApiResponse<CommentDto>.Ok(_mapper.Map<CommentDto>(comment), "Comment added"));
        }

        [HttpPost("{id:int}/attachments")]
        [Authorize(Policy = AppPolicies.ManageTasksPolicy)]
        public async Task<IActionResult> UploadAttachment(int id, IFormFile file, CancellationToken cancellationToken)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task is null)
                return NotFound(ApiResponse<TaskAttachmentDto>.Fail("Task not found"));

            var uploaded = await _attachmentService.UploadAsync(id, file, cancellationToken);
            if (!uploaded.Succeeded || uploaded.Data is null)
                return BadRequest(ApiResponse<TaskAttachmentDto>.Fail(uploaded.Error ?? "Upload failed"));

            return Ok(ApiResponse<TaskAttachmentDto>.Ok(
                _mapper.Map<TaskAttachmentDto>(uploaded.Data),
                "Attachment uploaded successfully"));
        }

        [HttpGet("{id:int}/attachments")]
        [Authorize(Policy = AppPolicies.ViewTaskPolicy)]
        public async Task<IActionResult> GetAttachments(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task is null)
                return NotFound(ApiResponse<IEnumerable<TaskAttachmentDto>>.Fail("Task not found"));

            var attachments = await _attachmentService.GetByTaskIdAsync(id);
            return Ok(ApiResponse<IEnumerable<TaskAttachmentDto>>.Ok(
                _mapper.Map<IEnumerable<TaskAttachmentDto>>(attachments)));
        }

        [HttpDelete("{taskId:int}/attachments/{attachmentId:int}")]
        [Authorize(Policy = AppPolicies.ManageTasksPolicy)]
        public async Task<IActionResult> DeleteAttachment(int taskId, int attachmentId, CancellationToken cancellationToken)
        {
            var task = await _taskService.GetByIdAsync(taskId);
            if (task is null)
                return NotFound(ApiResponse<object>.Fail("Task not found"));

            var deleted = await _attachmentService.DeleteAsync(taskId, attachmentId, cancellationToken);
            if (!deleted.Succeeded)
                return NotFound(ApiResponse<object>.Fail(deleted.Error ?? "Attachment not found"));

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppPolicies.ManageTasksPolicy)]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task is null)
                return NotFound(ApiResponse<object>.Fail("Task not found"));

            var deleted = await _taskService.DeleteAsync(id);
            if (!deleted)
                return NotFound(ApiResponse<object>.Fail("Task not found"));

            return NoContent();
        }

        private BadRequestObjectResult? ValidateTaskEnums(CreateTaskDto dto)
        {
            if (!Enum.IsDefined(typeof(TaskStatus), dto.Status))
                return BadRequest(ApiResponse<TaskDto>.Fail("Invalid task status"));

            if (!Enum.IsDefined(typeof(TaskPriority), dto.Priority))
                return BadRequest(ApiResponse<TaskDto>.Fail("Invalid task priority"));

            if (!Enum.IsDefined(typeof(TaskType), dto.Type))
                return BadRequest(ApiResponse<TaskDto>.Fail("Invalid task type"));

            return null;
        }

        private BadRequestObjectResult? ValidateTaskEnums(UpdateTaskDto dto)
        {
            if (!Enum.IsDefined(typeof(TaskStatus), dto.Status))
                return BadRequest(ApiResponse<TaskDto>.Fail("Invalid task status"));

            if (!Enum.IsDefined(typeof(TaskPriority), dto.Priority))
                return BadRequest(ApiResponse<TaskDto>.Fail("Invalid task priority"));

            if (!Enum.IsDefined(typeof(TaskType), dto.Type))
                return BadRequest(ApiResponse<TaskDto>.Fail("Invalid task type"));

            return null;
        }

        private BadRequestObjectResult? ValidateQaStatus(QaTestCaseStatus status)
        {
            if (!Enum.IsDefined(typeof(QaTestCaseStatus), status))
                return BadRequest(ApiResponse<QaTestCaseDto>.Fail("Invalid QA test case status"));

            return null;
        }

        private TaskDto ToTaskDto(TaskItem task)
        {
            var dto = _mapper.Map<TaskDto>(task);
            dto.Permissions = BuildPermissions(task);
            return dto;
        }

        private TaskPermissionsDto BuildPermissions(TaskItem task)
        {
            var userId = User.GetUserId();
            var role = User.GetRole();
            var isAdminOrLeader = role is AppRoles.Admin or AppRoles.TeamLeader;
            var isAssignedUser = userId.HasValue && task.AssignedUserId == userId.Value;
            var isQa = role == AppRoles.QA;
            var isProjectOwner =
                userId.HasValue &&
                task.Project != null &&
                task.Project.OwnerId == userId.Value;
            var isAssignedQa = userId.HasValue && task.QaAssignments.Any(assignment => assignment.QaUserId == userId.Value);

            return new TaskPermissionsDto
            {
                CanManage = isAdminOrLeader || isProjectOwner,
                CanAddWorkRecord = isAdminOrLeader || isAssignedUser,
                CanSendToQa = isAdminOrLeader || isProjectOwner || isAssignedUser,
                CanAddQaTestCase = (task.SentToQaAt.HasValue || isAssignedQa) && (isAdminOrLeader || (isQa && isAssignedQa)),
                CanComment = userId.HasValue,
                CanUnassign = isAdminOrLeader&& task.AssignedUserId.HasValue || isProjectOwner&& task.AssignedUserId.HasValue || isAssignedUser && task.AssignedUserId.HasValue
            };
        }
    }
}
