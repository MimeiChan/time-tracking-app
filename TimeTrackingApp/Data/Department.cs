using System.ComponentModel.DataAnnotations;

namespace TimeTrackingApp.Data
{
    /// <summary>
    /// 部署のモデル
    /// </summary>
    public class Department
    {
        /// <summary>
        /// 部署ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 部署名
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 部署コード
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 部署の説明
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// 部署に所属するユーザーのコレクション
        /// </summary>
        public virtual ICollection<ApplicationUser>? Users { get; set; }

        /// <summary>
        /// 部署に属するチームのコレクション
        /// </summary>
        public virtual ICollection<Team>? Teams { get; set; }
    }
}
