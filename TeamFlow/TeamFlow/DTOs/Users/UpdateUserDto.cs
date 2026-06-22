using System.ComponentModel.DataAnnotations;
using TeamFlow.Authorization;

namespace TeamFlow.DTOs.Users
{
    public class UpdateUserDto
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = AppRoles.Developer;
    }
}
