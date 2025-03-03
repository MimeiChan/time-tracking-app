using Microsoft.EntityFrameworkCore;
using TimeTrackingApp.Data;

namespace TimeTrackingApp.Services
{
    /// <summary>
    /// 時間計測サービス
    /// </summary>
    public class TimeEntryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TimeEntryService> _logger;
        private readonly NotificationService _notificationService;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="logger">ロガー</param>
        /// <param name="notificationService">通知サービス</param>
        public TimeEntryService(
            ApplicationDbContext context,
            ILogger<TimeEntryService> logger,
            NotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        /// <summary>
        /// ユーザーの時間計測エントリを取得
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <returns>時間計測エントリのリスト</returns>
        public async Task<List<TimeEntry>> GetUserTimeEntriesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.TimeEntries
                    .Include(te => te.Task)
                    .ThenInclude(t => t!.Category)
                    .Where(te => te.UserId == userId);

                if (startDate.HasValue)
                {
                    query = query.Where(te => te.StartTime >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(te => te.StartTime <= endDate.Value);
                }

                return await query.OrderByDescending(te => te.StartTime).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ユーザーID {UserId} の時間計測エントリ取得中にエラーが発生しました", userId);
                throw;
            }
        }

        /// <summary>
        /// 進行中の時間計測エントリを取得
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>進行中の時間計測エントリ</returns>
        public async Task<TimeEntry?> GetActiveTimeEntryAsync(string userId)
        {
            try
            {
                return await _context.TimeEntries
                    .Include(te => te.Task)
                    .ThenInclude(t => t!.Category)
                    .FirstOrDefaultAsync(te => te.UserId == userId && te.EndTime == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ユーザーID {UserId} の進行中時間計測エントリ取得中にエラーが発生しました", userId);
                throw;
            }
        }

        /// <summary>
        /// 時間計測を開始
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="taskId">タスクID</param>
        /// <param name="notes">メモ</param>
        /// <returns>作成された時間計測エントリ</returns>
        public async Task<TimeEntry> StartTimeTrackingAsync(string userId, int taskId, string? notes = null)
        {
            try
            {
                // 既に進行中のエントリがあるか確認
                var activeEntry = await GetActiveTimeEntryAsync(userId);
                if (activeEntry != null)
                {
                    throw new InvalidOperationException("既に進行中の時間計測があります。新しい計測を開始する前に、現在の計測を終了してください。");
                }

                var timeEntry = new TimeEntry
                {
                    UserId = userId,
                    TaskId = taskId,
                    StartTime = DateTime.Now,
                    Notes = notes,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.TimeEntries.Add(timeEntry);
                await _context.SaveChangesAsync();

                return timeEntry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ユーザーID {UserId} の時間計測開始中にエラーが発生しました", userId);
                throw;
            }
        }

        /// <summary>
        /// 時間計測を終了
        /// </summary>
        /// <param name="timeEntryId">時間計測エントリID</param>
        /// <param name="notes">追加メモ</param>
        /// <returns>更新された時間計測エントリ</returns>
        public async Task<TimeEntry?> EndTimeTrackingAsync(int timeEntryId, string? notes = null)
        {
            try
            {
                var timeEntry = await _context.TimeEntries
                    .Include(te => te.Task)
                    .FirstOrDefaultAsync(te => te.Id == timeEntryId);

                if (timeEntry == null || timeEntry.EndTime != null)
                {
                    return null;
                }

                timeEntry.EndTime = DateTime.Now;
                
                if (!string.IsNullOrEmpty(notes))
                {
                    timeEntry.Notes = string.IsNullOrEmpty(timeEntry.Notes) 
                        ? notes 
                        : $"{timeEntry.Notes}\n\n{notes}";
                }

                timeEntry.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return timeEntry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "時間計測エントリID {TimeEntryId} の終了中にエラーが発生しました", timeEntryId);
                throw;
            }
        }

        /// <summary>
        /// 時間計測を一時停止
        /// </summary>
        /// <param name="timeEntryId">時間計測エントリID</param>
        /// <returns>更新された時間計測エントリ</returns>
        public async Task<TimeEntry?> PauseTimeTrackingAsync(int timeEntryId)
        {
            try
            {
                var timeEntry = await _context.TimeEntries.FindAsync(timeEntryId);
                
                if (timeEntry == null || timeEntry.EndTime != null || timeEntry.IsPaused)
                {
                    return null;
                }

                timeEntry.IsPaused = true;
                timeEntry.PauseStartTime = DateTime.Now;
                timeEntry.UpdatedAt = DateTime.Now;
                
                await _context.SaveChangesAsync();
                
                return timeEntry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "時間計測エントリID {TimeEntryId} の一時停止中にエラーが発生しました", timeEntryId);
                throw;
            }
        }

        /// <summary>
        /// 時間計測を再開
        /// </summary>
        /// <param name="timeEntryId">時間計測エントリID</param>
        /// <returns>更新された時間計測エントリ</returns>
        public async Task<TimeEntry?> ResumeTimeTrackingAsync(int timeEntryId)
        {
            try
            {
                var timeEntry = await _context.TimeEntries.FindAsync(timeEntryId);
                
                if (timeEntry == null || timeEntry.EndTime != null || !timeEntry.IsPaused || !timeEntry.PauseStartTime.HasValue)
                {
                    return null;
                }

                // 一時停止時間を計算して追加
                var pauseDuration = (int)(DateTime.Now - timeEntry.PauseStartTime.Value).TotalMinutes;
                timeEntry.PauseDurationMinutes += pauseDuration;
                
                timeEntry.IsPaused = false;
                timeEntry.PauseStartTime = null;
                timeEntry.UpdatedAt = DateTime.Now;
                
                await _context.SaveChangesAsync();
                
                return timeEntry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "時間計測エントリID {TimeEntryId} の再開中にエラーが発生しました", timeEntryId);
                throw;
            }
        }

        /// <summary>
        /// 手動での時間入力
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="taskId">タスクID</param>
        /// <param name="startTime">開始時間</param>
        /// <param name="endTime">終了時間</param>
        /// <param name="notes">メモ</param>
        /// <returns>作成された時間計測エントリ</returns>
        public async Task<TimeEntry> AddManualTimeEntryAsync(string userId, int taskId, DateTime startTime, DateTime endTime, string? notes = null)
        {
            try
            {
                if (endTime <= startTime)
                {
                    throw new ArgumentException("終了時間は開始時間より後である必要があります。");
                }

                var timeEntry = new TimeEntry
                {
                    UserId = userId,
                    TaskId = taskId,
                    StartTime = startTime,
                    EndTime = endTime,
                    Notes = notes,
                    IsManualEntry = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.TimeEntries.Add(timeEntry);
                await _context.SaveChangesAsync();

                return timeEntry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ユーザーID {UserId} の手動時間入力中にエラーが発生しました", userId);
                throw;
            }
        }

        /// <summary>
        /// ベンダー用の一括時間入力
        /// </summary>
        /// <param name="vendorUserId">ベンダーユーザーID</param>
        /// <param name="taskId">タスクID</param>
        /// <param name="date">日付</param>
        /// <param name="durationMinutes">作業時間（分）</param>
        /// <param name="notes">メモ</param>
        /// <returns>作成された時間計測エントリ</returns>
        public async Task<TimeEntry> AddVendorTimeEntryAsync(string vendorUserId, int taskId, DateTime date, int durationMinutes, string? notes = null)
        {
            try
            {
                // ベンダーユーザーかどうか確認
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == vendorUserId);
                if (user == null || user.IsInternal)
                {
                    throw new ArgumentException("指定されたユーザーはベンダーではありません。");
                }

                var startTime = date.Date;
                var endTime = startTime.AddMinutes(durationMinutes);

                var timeEntry = new TimeEntry
                {
                    UserId = vendorUserId,
                    TaskId = taskId,
                    StartTime = startTime,
                    EndTime = endTime,
                    Notes = notes,
                    IsManualEntry = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.TimeEntries.Add(timeEntry);
                await _context.SaveChangesAsync();

                return timeEntry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ベンダーID {VendorId} の一括時間入力中にエラーが発生しました", vendorUserId);
                throw;
            }
        }

        /// <summary>
        /// 時間計測エントリを更新
        /// </summary>
        /// <param name="timeEntry">時間計測エントリ</param>
        /// <returns>更新された時間計測エントリ</returns>
        public async Task<TimeEntry> UpdateTimeEntryAsync(TimeEntry timeEntry)
        {
            try
            {
                timeEntry.UpdatedAt = DateTime.Now;
                
                _context.TimeEntries.Update(timeEntry);
                await _context.SaveChangesAsync();
                
                return timeEntry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "時間計測エントリID {TimeEntryId} の更新中にエラーが発生しました", timeEntry.Id);
                throw;
            }
        }

        /// <summary>
        /// 時間計測エントリを削除
        /// </summary>
        /// <param name="timeEntryId">時間計測エントリID</param>
        /// <returns>成功したかどうか</returns>
        public async Task<bool> DeleteTimeEntryAsync(int timeEntryId)
        {
            try
            {
                var timeEntry = await _context.TimeEntries.FindAsync(timeEntryId);
                if (timeEntry == null)
                {
                    return false;
                }

                _context.TimeEntries.Remove(timeEntry);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "時間計測エントリID {TimeEntryId} の削除中にエラーが発生しました", timeEntryId);
                throw;
            }
        }

        /// <summary>
        /// 長時間同一業務のチェック
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="thresholdMinutes">閾値（分）</param>
        /// <returns>長時間同一タスクリスト</returns>
        public async Task<List<TimeEntry>> CheckLongDurationTasksAsync(string userId, int thresholdMinutes)
        {
            try
            {
                var activeEntry = await GetActiveTimeEntryAsync(userId);
                if (activeEntry == null || activeEntry.IsPaused)
                {
                    return new List<TimeEntry>();
                }

                var currentDuration = (int)(DateTime.Now - activeEntry.StartTime).TotalMinutes - activeEntry.PauseDurationMinutes;
                if (currentDuration > thresholdMinutes)
                {
                    return new List<TimeEntry> { activeEntry };
                }

                return new List<TimeEntry>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ユーザーID {UserId} の長時間同一業務チェック中にエラーが発生しました", userId);
                throw;
            }
        }
    }
}
