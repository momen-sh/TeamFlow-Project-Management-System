namespace TeamFlow.Authorization
{
    public interface IRoleHierarchyService
    {
        int GetRank(string? role);
        bool IsAtLeast(string? roleA, string roleB);
    }

    public sealed class RoleHierarchyService : IRoleHierarchyService
    {
        private static string Normalize(string? role)
            => string.IsNullOrWhiteSpace(role) ? string.Empty : role!;

        public int GetRank(string? role)
        {
            return Normalize(role) switch
            {
                AppRoles.Admin => 3,
                AppRoles.TeamLeader => 2,
                AppRoles.Developer => 1,
                AppRoles.QA => 1,
                _ => 0
            };
        }

        public bool IsAtLeast(string? roleA, string roleB)
            => GetRank(roleA) >= GetRank(roleB);
    }
}
