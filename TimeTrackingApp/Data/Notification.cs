using System.ComponentModel.DataAnnotations;

namespace TimeTrackingApp.Data
{
    /// <summary>
    /// 通知のモデル
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// 通知ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ユーザーID
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// ユーザー
        /// </summary>
        public virtual ApplicationUser? User { get; set; }

        /// <summary>
        /// 通知タイトル
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 通知メッセージ
        /// </summary>
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 通知タイプ (Alert, Reminder, TaskInfo など)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 関連するタスクID (オプション)
        /// </summary>
        public int? TaskId { get; set; }

        /// <summary>
        /// 関連するタスク
        /// </summary>
        public virtual Task? Task { get; set; }

        /// <summary>
        /// 関連する時間計測エントリID (オプション)
        /// </summary>
        public int? TimeEntryId { get; set; }

        /// <summary>
        /// 関連する時間計測エントリ
        /// </summary>
        public virtual TimeEntry? TimeEntry { get; set; }

        /// <summary>
        /// 既読フラグ
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// 通知の作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 通知を表示する日時
        /// </summary>
        public DateTime DisplayAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 通知の有効期限
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
}
