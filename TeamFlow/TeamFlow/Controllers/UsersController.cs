using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.Authorization;
using TeamFlow.DTOs.Common;
using TeamFlow.DTOs.Users;
using TeamFlow.Entities;
using TeamFlow.Services.Interfaces;

namespace TeamFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UsersController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            var result = _mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(ApiResponse<IEnumerable<UserDto>>.Ok(result));
        }

        [HttpGet("qa")]
        [Authorize(Policy = AppPolicies.ViewTaskPolicy)]
        public async Task<IActionResult> GetQaUsers()
        {
            var users = await _userService.GetByRoleAsync(AppRoles.QA);
            var result = _mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(ApiResponse<IEnumerable<UserDto>>.Ok(result));
        }

        [HttpGet("mention-targets")]
        [Authorize(Policy = AppPolicies.ViewTaskPolicy)]
        public async Task<IActionResult> GetMentionTargets()
        {
            var users = await _userService.GetAllAsync();
            var result = _mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(ApiResponse<IEnumerable<UserDto>>.Ok(result));
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = AppPolicies.ManageUsersPolicy)]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user is null)
                return NotFound(ApiResponse<UserDto>.Fail("User not found"));

            return Ok(ApiResponse<UserDto>.Ok(_mapper.Map<UserDto>(user)));
        }

        [HttpPost]
        [Authorize(Policy = AppPolicies.ManageUsersPolicy)]
        public async Task<IActionResult> Create(CreateUserDto dto)
        {
            var user = _mapper.Map<User>(dto);
            var created = await _userService.CreateAsync(user, dto.Password, dto.Role);
            if (!created.Succeeded || created.Data is null)
                return BadRequest(ApiResponse<UserDto>.Fail(created.Error ?? "User could not be created"));

            var result = _mapper.Map<UserDto>(created.Data);
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Data.Id },
                ApiResponse<UserDto>.Ok(result, "User created successfully"));
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = AppPolicies.ManageUsersPolicy)]
        public async Task<IActionResult> Update(int id, UpdateUserDto dto)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user is null)
                return NotFound(ApiResponse<UserDto>.Fail("User not found"));

            _mapper.Map(dto, user);
            var updated = await _userService.UpdateAsync(user, dto.Role);
            if (!updated.Succeeded || updated.Data is null)
                return BadRequest(ApiResponse<UserDto>.Fail(updated.Error ?? "User could not be updated"));

            return Ok(ApiResponse<UserDto>.Ok(_mapper.Map<UserDto>(updated.Data), "User updated successfully"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppPolicies.ManageUsersPolicy)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _userService.DeleteAsync(id);
            if (!deleted)
                return NotFound(ApiResponse<object>.Fail("User not found"));

            return NoContent();
        }
    }
}
