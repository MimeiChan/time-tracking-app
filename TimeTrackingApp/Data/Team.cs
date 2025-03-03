using System.ComponentModel.DataAnnotations;

namespace TimeTrackingApp.Data
{
    /// <summary>
    /// チームのモデル
    /// </summary>
    public class Team
    {
        /// <summary>
        /// チームID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// チーム名
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// チームの説明
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// 所属部署ID
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// 所属部署
        /// </summary>
        public virtual Department? Department { get; set; }

        /// <summary>
        /// チームに所属するユーザーのコレクション
        /// </summary>
        public virtual ICollection<UserTeam>? UserTeams { get; set; }
    }
}
