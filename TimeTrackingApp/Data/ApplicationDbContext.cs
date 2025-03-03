using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TimeTrackingApp.Data
{
    /// <summary>
    /// アプリケーションのデータベースコンテキスト
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="options">DbContextオプション</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// 部署
        /// </summary>
        public DbSet<Department> Departments { get; set; } = null!;

        /// <summary>
        /// チーム
        /// </summary>
        public DbSet<Team> Teams { get; set; } = null!;

        /// <summary>
        /// ユーザーチーム関連
        /// </summary>
        public DbSet<UserTeam> UserTeams { get; set; } = null!;

        /// <summary>
        /// タスク
        /// </summary>
        public DbSet<Task> Tasks { get; set; } = null!;

        /// <summary>
        /// タスクカテゴリ
        /// </summary>
        public DbSet<TaskCategory> TaskCategories { get; set; } = null!;

        /// <summary>
        /// 時間計測エントリ
        /// </summary>
        public DbSet<TimeEntry> TimeEntries { get; set; } = null!;

        /// <summary>
        /// 通知
        /// </summary>
        public DbSet<Notification> Notifications { get; set; } = null!;

        /// <summary>
        /// レポート
        /// </summary>
        public DbSet<Report> Reports { get; set; } = null!;

        /// <summary>
        /// モデル構築時の設定
        /// </summary>
        /// <param name="modelBuilder">モデルビルダー</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // タスクカテゴリの自己参照関係の設定
            modelBuilder.Entity<TaskCategory>()
                .HasOne(tc => tc.ParentCategory)
                .WithMany(tc => tc.ChildCategories)
                .HasForeignKey(tc => tc.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // 部署とチームの関係
            modelBuilder.Entity<Team>()
                .HasOne(t => t.Department)
                .WithMany(d => d.Teams)
                .HasForeignKey(t => t.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ユーザーとチームの関係
            modelBuilder.Entity<UserTeam>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.UserTeams)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserTeam>()
                .HasOne(ut => ut.Team)
                .WithMany(t => t.UserTeams)
                .HasForeignKey(ut => ut.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // ユーザーと部署の関係
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // タスクとタスクカテゴリの関係
            modelBuilder.Entity<Task>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // タスクと担当者の関係
            modelBuilder.Entity<Task>()
                .HasOne(t => t.Assignee)
                .WithMany()
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            // タスクと部署の関係
            modelBuilder.Entity<Task>()
                .HasOne(t => t.Department)
                .WithMany()
                .HasForeignKey(t => t.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // 時間計測エントリとユーザーの関係
            modelBuilder.Entity<TimeEntry>()
                .HasOne(te => te.User)
                .WithMany(u => u.TimeEntries)
                .HasForeignKey(te => te.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 時間計測エントリとタスクの関係
            modelBuilder.Entity<TimeEntry>()
                .HasOne(te => te.Task)
                .WithMany(t => t.TimeEntries)
                .HasForeignKey(te => te.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // 通知とユーザーの関係
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 通知とタスクの関係
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Task)
                .WithMany()
                .HasForeignKey(n => n.TaskId)
                .OnDelete(DeleteBehavior.SetNull);

            // 通知と時間計測エントリの関係
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.TimeEntry)
                .WithMany()
                .HasForeignKey(n => n.TimeEntryId)
                .OnDelete(DeleteBehavior.SetNull);

            // レポートとユーザーの関係
            modelBuilder.Entity<Report>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // レポートと部署の関係
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Department)
                .WithMany()
                .HasForeignKey(r => r.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // レポートとチームの関係
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Team)
                .WithMany()
                .HasForeignKey(r => r.TeamId)
                .OnDelete(DeleteBehavior.SetNull);

            // レポートと作成者の関係
            modelBuilder.Entity<Report>()
                .HasOne(r => r.CreatedBy)
                .WithMany()
                .HasForeignKey(r => r.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
