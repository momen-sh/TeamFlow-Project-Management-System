using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.Authorization;
using TeamFlow.DTOs.Comments;
using TeamFlow.DTOs.Common;
using TeamFlow.Entities;
using TeamFlow.Services.Interfaces;

namespace TeamFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IMapper _mapper;

        public CommentsController(ICommentService commentService, IMapper mapper)
        {
            _commentService = commentService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var comments = await _commentService.GetAllAsync();
            var result = _mapper.Map<IEnumerable<CommentDto>>(comments);
            return Ok(ApiResponse<IEnumerable<CommentDto>>.Ok(result));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment is null)
                return NotFound(ApiResponse<CommentDto>.Fail("Comment not found"));

            return Ok(ApiResponse<CommentDto>.Ok(_mapper.Map<CommentDto>(comment)));
        }

        [HttpGet("task/{taskId:int}")]
        public async Task<IActionResult> GetByTaskId(int taskId)
        {
            var comments = await _commentService.GetByTaskIdAsync(taskId);
            var result = _mapper.Map<IEnumerable<CommentDto>>(comments);
            return Ok(ApiResponse<IEnumerable<CommentDto>>.Ok(result));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCommentDto dto)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<CommentDto>.Fail("Invalid token"));

            dto.UserId = userId.Value;
            var comment = _mapper.Map<Comment>(dto);
            await _commentService.CreateAsync(comment, dto.MentionedUserIds);

            var result = _mapper.Map<CommentDto>(comment);
            return CreatedAtAction(
                nameof(GetById),
                new { id = comment.Id },
                ApiResponse<CommentDto>.Ok(result, "Comment created successfully"));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _commentService.DeleteAsync(id);
            if (!deleted)
                return NotFound(ApiResponse<object>.Fail("Comment not found"));

            return NoContent();
        }
    }
}
