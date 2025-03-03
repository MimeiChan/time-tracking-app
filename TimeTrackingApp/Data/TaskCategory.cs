using System.ComponentModel.DataAnnotations;

namespace TimeTrackingApp.Data
{
    /// <summary>
    /// タスクカテゴリのモデル
    /// </summary>
    public class TaskCategory
    {
        /// <summary>
        /// カテゴリID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// カテゴリ名
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// カテゴリの説明
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// 色コード (表示用)
        /// </summary>
        [StringLength(7)]
        public string ColorCode { get; set; } = "#3498db";

        /// <summary>
        /// 親カテゴリID
        /// </summary>
        public int? ParentCategoryId { get; set; }

        /// <summary>
        /// 親カテゴリとの関連
        /// </summary>
        public virtual TaskCategory? ParentCategory { get; set; }

        /// <summary>
        /// 子カテゴリのコレクション
        /// </summary>
        public virtual ICollection<TaskCategory>? ChildCategories { get; set; }

        /// <summary>
        /// このカテゴリに属するタスクのコレクション
        /// </summary>
        public virtual ICollection<Task>? Tasks { get; set; }
    }
}
