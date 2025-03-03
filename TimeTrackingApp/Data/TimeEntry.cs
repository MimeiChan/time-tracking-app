using System.ComponentModel.DataAnnotations;

namespace TimeTrackingApp.Data
{
    /// <summary>
    /// 時間計測エントリのモデル
    /// </summary>
    public class TimeEntry
    {
        /// <summary>
        /// 時間計測エントリID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 担当者ID
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 担当者
        /// </summary>
        public virtual ApplicationUser? User { get; set; }

        /// <summary>
        /// タスクID
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// タスク
        /// </summary>
        public virtual Task? Task { get; set; }

        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 終了時間 (nullの場合、まだ進行中)
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 一時停止時間（分）
        /// </summary>
        public int PauseDurationMinutes { get; set; } = 0;

        /// <summary>
        /// メモ・コメント
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// 実際の労働時間（分）
        /// </summary>
        public int? DurationMinutes
        {
            get
            {
                if (EndTime.HasValue)
                {
                    var totalMinutes = (int)(EndTime.Value - StartTime).TotalMinutes - PauseDurationMinutes;
                    return totalMinutes > 0 ? totalMinutes : 0;
                }
                
                return null;
            }
        }

        /// <summary>
        /// 手動入力フラグ
        /// </summary>
        public bool IsManualEntry { get; set; } = false;

        /// <summary>
        /// 中断中フラグ
        /// </summary>
        public bool IsPaused { get; set; } = false;

        /// <summary>
        /// 中断開始時間
        /// </summary>
        public DateTime? PauseStartTime { get; set; }

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
