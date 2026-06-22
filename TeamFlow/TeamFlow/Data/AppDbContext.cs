using Microsoft.EntityFrameworkCore;
using TeamFlow.Entities;

namespace TeamFlow.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<TaskAttachment> TaskAttachments { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentMention> CommentMentions { get; set; }
        public DbSet<TaskWorkRecord> TaskWorkRecords { get; set; }
        public DbSet<QaTestCase> QaTestCases { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<TaskQaAssignment> TaskQaAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProjectMember>()
                .HasKey(pm => new { pm.UserId, pm.ProjectId });

            modelBuilder.Entity<WorkspaceMember>()
                .HasKey(wm => new { wm.UserId, wm.WorkspaceId });

            modelBuilder.Entity<CommentMention>()
                .HasKey(cm => new { cm.CommentId, cm.UserId });

            modelBuilder.Entity<TaskQaAssignment>()
                .HasKey(qa => new { qa.TaskId, qa.QaUserId });

            modelBuilder.Entity<WorkspaceMember>()
                .HasOne(wm => wm.User)
                .WithMany()
                .HasForeignKey(wm => wm.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WorkspaceMember>()
                .HasOne(wm => wm.Workspace)
                .WithMany(w => w.Members)
                .HasForeignKey(wm => wm.WorkspaceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.User)
                .WithMany(u => u.Projects)
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.AssignedUser)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.TaskItem)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CommentMention>()
                .HasOne(cm => cm.Comment)
                .WithMany(c => c.Mentions)
                .HasForeignKey(cm => cm.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommentMention>()
                .HasOne(cm => cm.User)
                .WithMany()
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TaskWorkRecord>()
                .HasOne(wr => wr.Task)
                .WithMany(t => t.WorkRecords)
                .HasForeignKey(wr => wr.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskWorkRecord>()
                .HasOne(wr => wr.CreatedByUser)
                .WithMany()
                .HasForeignKey(wr => wr.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<QaTestCase>()
                .HasOne(tc => tc.Task)
                .WithMany(t => t.QaTestCases)
                .HasForeignKey(tc => tc.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QaTestCase>()
                .HasOne(tc => tc.CreatedByUser)
                .WithMany()
                .HasForeignKey(tc => tc.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TaskQaAssignment>()
                .HasOne(qa => qa.Task)
                .WithMany(t => t.QaAssignments)
                .HasForeignKey(qa => qa.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskQaAssignment>()
                .HasOne(qa => qa.QaUser)
                .WithMany()
                .HasForeignKey(qa => qa.QaUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TaskQaAssignment>()
                .HasOne(qa => qa.AssignedByUser)
                .WithMany()
                .HasForeignKey(qa => qa.AssignedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TaskAttachment>()
                .HasOne(a => a.Task)
                .WithMany(t => t.Attachments)
                .HasForeignKey(a => a.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
