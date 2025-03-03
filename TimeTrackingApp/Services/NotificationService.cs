using Microsoft.EntityFrameworkCore;
using TimeTrackingApp.Data;

namespace TimeTrackingApp.Services
{
    /// <summary>
    /// 通知サービス
    /// </summary>
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="logger">ロガー</param>
        /// <param name="configuration">設定</param>
        public NotificationService(
            ApplicationDbContext context,
            ILogger<NotificationService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// ユーザーの未読通知を取得
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>未読通知のリスト</returns>
        public async Task<List<Notification>> GetUnreadNotificationsAsync(string userId)
        {
            try
            {
                var now = DateTime.Now;
                
                return await _context.Notifications
                    .Include(n => n.Task)
                    .Include(n => n.TimeEntry)
                    .Where(n => n.UserId == userId && !n.IsRead && n.DisplayAt <= now && (n.ExpiresAt == null || n.ExpiresAt > now))
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ユーザーID {UserId} の未読通知取得中にエラーが発生しました", userId);
                throw;
            }
        }

        /// <summary>
        /// ユーザーの通知履歴を取得
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="limit">取得数</param>
        /// <returns>通知履歴のリスト</returns>
        public async Task<List<Notification>> GetNotificationHistoryAsync(string userId, int limit = 50)
        {
            try
            {
                return await _context.Notifications
                    .Include(n => n.Task)
                    .Include(n => n.TimeEntry)
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ユーザーID {UserId} の通知履歴取得中にエラーが発生しました", userId);
                throw;
            }
        }

        /// <summary>
        /// 通知を作成
        /// </summary>
        /// <param name="notification">通知</param>
        /// <returns>作成された通知</returns>
        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            try
            {
                notification.CreatedAt = DateTime.Now;
                
                if (notification.DisplayAt == default)
                {
                    notification.DisplayAt = DateTime.Now;
                }
                
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                
                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "通知作成中にエラーが発生しました");
                throw;
            }
        }

        /// <summary>
        /// 通知を既読にする
        /// </summary>
        /// <param name="notificationId">通知ID</param>
        /// <returns>更新された通知</returns>
        public async Task<Notification?> MarkNotificationAsReadAsync(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification == null)
                {
                    return null;
                }

                notification.IsRead = true;
                await _context.SaveChangesAsync();
                
                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "通知ID {NotificationId} の既読処理中にエラーが発生しました", notificationId);
                throw;
            }
        }

        /// <summary>
        /// すべての通知を既読にする
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>成功したかどうか</returns>
        public async Task<bool> MarkAllNotificationsAsReadAsync(string userId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ユーザーID {UserId} のすべての通知既読処理中にエラーが発生しました", userId);
                throw;
            }
        }

        /// <summary>
        /// 長時間同一業務のアラートを作成
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="taskId">タスクID</param>
        /// <param name="timeEntryId">時間計測エントリID</param>
        /// <param name="durationMinutes">継続時間（分）</param>
        /// <returns>作成された通知</returns>
        public async Task<Notification> CreateLongTaskAlertAsync(string userId, int taskId, int timeEntryId, int durationMinutes)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null)
                {
                    throw new ArgumentException("指定されたタスクが見つかりません。");
                }

                var notification = new Notification
                {
                    UserId = userId,
                    Title = "長時間同一業務アラート",
                    Message = $"タスク「{task.Name}」を{durationMinutes}分間継続しています。休憩や次の業務に移るタイミングかもしれません。",
                    Type = "Alert",
                    TaskId = taskId,
                    TimeEntryId = timeEntryId,
                    DisplayAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddHours(1)
                };

                return await CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "長時間同一業務アラート作成中にエラーが発生しました");
                throw;
            }
        }

        /// <summary>
        /// 時間入力リマインダーを作成
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>作成された通知</returns>
        public async Task<Notification> CreateTimeEntryReminderAsync(string userId)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = "時間入力リマインダー",
                    Message = "業務時間の入力を忘れていませんか？現在の業務状況を記録してください。",
                    Type = "Reminder",
                    DisplayAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddHours(4)
                };

                return await CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "時間入力リマインダー作成中にエラーが発生しました");
                throw;
            }
        }

        /// <summary>
        /// 業務未完了通知を作成
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="taskId">タスクID</param>
        /// <returns>作成された通知</returns>
        public async Task<Notification> CreateTaskIncompleteNotificationAsync(string userId, int taskId)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null)
                {
                    throw new ArgumentException("指定されたタスクが見つかりません。");
                }

                var notification = new Notification
                {
                    UserId = userId,
                    Title = "業務未完了通知",
                    Message = $"タスク「{task.Name}」が未完了です。タスクの状態を更新してください。",
                    Type = "TaskInfo",
                    TaskId = taskId,
                    DisplayAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddDays(1)
                };

                return await CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "業務未完了通知作成中にエラーが発生しました");
                throw;
            }
        }

        /// <summary>
        /// 期限切れの通知を削除
        /// </summary>
        /// <returns>削除された通知数</returns>
        public async Task<int> CleanupExpiredNotificationsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var expiredNotifications = await _context.Notifications
                    .Where(n => n.ExpiresAt != null && n.ExpiresAt < now)
                    .ToListAsync();

                _context.Notifications.RemoveRange(expiredNotifications);
                await _context.SaveChangesAsync();

                return expiredNotifications.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "期限切れ通知の削除中にエラーが発生しました");
                throw;
            }
        }
    }
}
