using System.ComponentModel.DataAnnotations;

namespace TimeTrackingApp.Data
{
    /// <summary>
    /// 業務タスクのモデル
    /// </summary>
    public class Task
    {
        /// <summary>
        /// タスクID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// タスクコード
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// タスク名
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// タスクの説明
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// タスクカテゴリID
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// タスクカテゴリ
        /// </summary>
        public virtual TaskCategory? Category { get; set; }

        /// <summary>
        /// 重要度（1-5）
        /// </summary>
        public int Importance { get; set; } = 3;

        /// <summary>
        /// 有効フラグ
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 担当者IDまたはベンダーID (nullの場合、誰でも担当可)
        /// </summary>
        public string? AssigneeId { get; set; }

        /// <summary>
        /// 担当者またはベンダー
        /// </summary>
        public virtual ApplicationUser? Assignee { get; set; }

        /// <summary>
        /// 担当部署ID
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// 担当部署
        /// </summary>
        public virtual Department? Department { get; set; }

        /// <summary>
        /// タスクに関連する時間計測エントリのコレクション
        /// </summary>
        public virtual ICollection<TimeEntry>? TimeEntries { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
