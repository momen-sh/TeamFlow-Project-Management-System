namespace TeamFlow.Authorization
{
    public static class AppPolicies
    {
        public const string ManageUsersPolicy = "ManageUsersPolicy";
        public const string ManageProjectsPolicy = "ManageProjectsPolicy";
        public const string ManageTasksPolicy = "ManageTasksPolicy";
        public const string ViewProjectPolicy = "ViewProjectPolicy";
        public const string ViewTaskPolicy = "ViewTaskPolicy";
        public const string UpdateTaskStatusPolicy = "UpdateTaskStatusPolicy";
        public const string SelfAssignTaskPolicy = "SelfAssignTaskPolicy";
        public const string UnassignTaskPolicy = "UnassignTaskPolicy";
    }
}
