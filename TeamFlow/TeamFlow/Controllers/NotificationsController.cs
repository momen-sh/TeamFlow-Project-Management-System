using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.Authorization;
using TeamFlow.DTOs.Common;
using TeamFlow.DTOs.Notifications;
using TeamFlow.Entities;
using TeamFlow.Services.Interfaces;

namespace TeamFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public NotificationsController(INotificationService notificationService, IMapper mapper)
        {
            _notificationService = notificationService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetMine()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<IEnumerable<NotificationDto>>.Fail("Invalid token"));

            var notifications = await _notificationService.GetByUserIdAsync(userId.Value);
            return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(_mapper.Map<IEnumerable<NotificationDto>>(notifications)));
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var currentUserId = User.GetUserId();
            var role = User.GetRole();
            if (!currentUserId.HasValue)
                return Unauthorized(ApiResponse<IEnumerable<NotificationDto>>.Fail("Invalid token"));

            if (currentUserId.Value != userId && role is not (AppRoles.Admin or AppRoles.TeamLeader))
                return Forbid();

            var notifications = await _notificationService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(_mapper.Map<IEnumerable<NotificationDto>>(notifications)));
        }

        [HttpPost]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.TeamLeader}")]
        public async Task<IActionResult> Create(CreateNotificationDto dto)
        {
            var notification = _mapper.Map<Notification>(dto);
            var result = await _notificationService.CreateAsync(notification);
            if (!result.Succeeded || result.Data is null)
                return BadRequest(ApiResponse<NotificationDto>.Fail(result.Error ?? "Notification could not be created"));

            return Ok(ApiResponse<NotificationDto>.Ok(_mapper.Map<NotificationDto>(result.Data), "Notification created"));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<int>.Fail("Invalid token"));

            var count = await _notificationService.GetUnreadCountAsync(userId.Value);
            return Ok(ApiResponse<int>.Ok(count));
        }

        [HttpPatch("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<NotificationDto>.Fail("Invalid token"));

            var result = await _notificationService.MarkAsReadAsync(id, userId.Value);
            if (!result.Succeeded || result.Data is null)
                return NotFound(ApiResponse<NotificationDto>.Fail(result.Error ?? "Notification not found"));

            return Ok(ApiResponse<NotificationDto>.Ok(_mapper.Map<NotificationDto>(result.Data)));
        }
    }
}
