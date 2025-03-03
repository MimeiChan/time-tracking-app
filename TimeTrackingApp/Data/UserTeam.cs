namespace TimeTrackingApp.Data
{
    /// <summary>
    /// ユーザーとチームの関連付けを表すモデル
    /// </summary>
    public class UserTeam
    {
        /// <summary>
        /// ユーザーチーム関連ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ユーザーID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// チームID
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// ユーザー
        /// </summary>
        public virtual ApplicationUser? User { get; set; }

        /// <summary>
        /// チーム
        /// </summary>
        public virtual Team? Team { get; set; }

        /// <summary>
        /// チームリーダーかどうかのフラグ
        /// </summary>
        public bool IsTeamLeader { get; set; }

        /// <summary>
        /// 参加日
        /// </summary>
        public DateTime JoinDate { get; set; }
    }
}
