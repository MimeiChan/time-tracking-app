using System.ComponentModel.DataAnnotations;

namespace TimeTrackingApp.Data
{
    /// <summary>
    /// レポートのモデル
    /// </summary>
    public class Report
    {
        /// <summary>
        /// レポートID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// レポート名
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// レポートタイプ (Individual, Team, Department, Vendor など)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// レポート期間タイプ (Daily, Weekly, Monthly, Custom など)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string PeriodType { get; set; } = string.Empty;

        /// <summary>
        /// 開始日
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 終了日
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// ユーザーID (個人レポートの場合)
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// ユーザー (個人レポートの場合)
        /// </summary>
        public virtual ApplicationUser? User { get; set; }

        /// <summary>
        /// 部署ID (部署レポートの場合)
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// 部署 (部署レポートの場合)
        /// </summary>
        public virtual Department? Department { get; set; }

        /// <summary>
        /// チームID (チームレポートの場合)
        /// </summary>
        public int? TeamId { get; set; }

        /// <summary>
        /// チーム (チームレポートの場合)
        /// </summary>
        public virtual Team? Team { get; set; }

        /// <summary>
        /// レポート生成パラメータ (JSONとして保存)
        /// </summary>
        [StringLength(4000)]
        public string? Parameters { get; set; }

        /// <summary>
        /// レポートデータ (JSONとして保存)
        /// </summary>
        public string? ReportData { get; set; }

        /// <summary>
        /// ファイルパス (生成されたレポートファイルへのパス)
        /// </summary>
        [StringLength(500)]
        public string? FilePath { get; set; }

        /// <summary>
        /// レポートフォーマット (PDF, Excel など)
        /// </summary>
        [StringLength(10)]
        public string? Format { get; set; }

        /// <summary>
        /// スケジュール設定 (定期レポートの場合)
        /// </summary>
        [StringLength(100)]
        public string? Schedule { get; set; }

        /// <summary>
        /// 作成者ID
        /// </summary>
        [Required]
        public string CreatedById { get; set; } = string.Empty;

        /// <summary>
        /// 作成者
        /// </summary>
        public virtual ApplicationUser? CreatedBy { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 最終更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
