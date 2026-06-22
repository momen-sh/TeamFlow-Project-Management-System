namespace TeamFlow.Authorization
{
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string TeamLeader = "TeamLeader";
        public const string Developer = "Developer";
        public const string QA = "QA";

        public static readonly ISet<string> All = new HashSet<string>(StringComparer.Ordinal)
        {
            Admin,
            TeamLeader,
            Developer,
            QA
        };
    }
}
