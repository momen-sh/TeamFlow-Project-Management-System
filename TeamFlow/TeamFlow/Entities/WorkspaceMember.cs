namespace TeamFlow.Entities
{
    public class WorkspaceMember
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        public string Role { get; set; } = "Member";
    }
}
