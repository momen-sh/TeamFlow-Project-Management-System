namespace TeamFlow.DTOs.Tasks
{
    public class TaskPermissionsDto
    {
        public bool CanManage { get; set; }
        public bool CanAddWorkRecord { get; set; }
        public bool CanSendToQa { get; set; }
        public bool CanAddQaTestCase { get; set; }
        public bool CanComment { get; set; }
        public bool CanUnassign { get; set; }
    }
}
