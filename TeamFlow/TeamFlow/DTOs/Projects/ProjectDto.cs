namespace TeamFlow.DTOs.Projects
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public int? WorkspaceId { get; set; }
        public IEnumerable<ProjectMemberDto> Members { get; set; } = new List<ProjectMemberDto>();
    }

    public class ProjectMemberDto
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string Role { get; set; } = "Member";
    }
}
