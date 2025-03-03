using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TimeTrackingApp.Data
{
    /// <summary>
    /// アプリケーションのユーザーモデル
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// 氏名
        /// </summary>
        [Required]
        [StringLength(50)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// 社内ユーザーかどうかのフラグ
        /// </summary>
        [Required]
        public bool IsInternal { get; set; }

        /// <summary>
        /// ベンダー名 (ベンダーユーザーの場合のみ使用)
        /// </summary>
        [StringLength(100)]
        public string? VendorName { get; set; }

        /// <summary>
        /// 部署ID (社内ユーザーの場合のみ使用)
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// 部署との関連性 (社内ユーザーの場合のみ使用)
        /// </summary>
        public virtual Department? Department { get; set; }

        /// <summary>
        /// ユーザーが所属しているチームのコレクション (社内ユーザーの場合のみ使用)
        /// </summary>
        public virtual ICollection<UserTeam>? UserTeams { get; set; }

        /// <summary>
        /// ユーザーの時間計測エントリのコレクション
        /// </summary>
        public virtual ICollection<TimeEntry>? TimeEntries { get; set; }
    }
}
