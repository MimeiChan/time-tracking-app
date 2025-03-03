using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TimeTrackingApp.Data;

namespace TimeTrackingApp.Services
{
    /// <summary>
    /// レポートサービス
    /// </summary>
    public class ReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportService> _logger;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="logger">ロガー</param>
        public ReportService(
            ApplicationDbContext context,
            ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// レポートを取得
        /// </summary>
        /// <param name="reportId">レポートID</param>
        /// <returns>レポート</returns>
        public async Task<Report?> GetReportAsync(int reportId)
        {
            try
            {
                return await _context.Reports
                    .Include(r => r.User)
                    .Include(r => r.Department)
                    .Include(r => r.Team)
                    .Include(r => r.CreatedBy)
                    .FirstOrDefaultAsync(r => r.Id == reportId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "レポートID {ReportId} の取得中にエラーが発生しました", reportId);
                throw;
            }
        }

        /// <summary>
        /// ユーザーのレポート一覧を取得
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>レポートのリスト</returns>
        public async Task<List<Report>> GetUserReportsAsync(string userId)
        {
            try
            {
                return await _context.Reports
                    .Include(r => r.User)
                    .Include(r => r.Department)
                    .Include(r => r.Team)
                    .Where(r => r.CreatedById == userId || r.UserId == userId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ユーザーID {UserId} のレポート一覧取得中にエラーが発生しました", userId);
                throw;
            }
        }

        /// <summary>
        /// 部署のレポート一覧を取得
        /// </summary>
        /// <param name="departmentId">部署ID</param>
        /// <returns>レポートのリスト</returns>
        public async Task<List<Report>> GetDepartmentReportsAsync(int departmentId)
        {
            try
            {
                return await _context.Reports
                    .Include(r => r.CreatedBy)
                    .Where(r => r.DepartmentId == departmentId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "部署ID {DepartmentId} のレポート一覧取得中にエラーが発生しました", departmentId);
                throw;
            }
        }

        /// <summary>
        /// チームのレポート一覧を取得
        /// </summary>
        /// <param name="teamId">チームID</param>
        /// <returns>レポートのリスト</returns>
        public async Task<List<Report>> GetTeamReportsAsync(int teamId)
        {
            try
            {
                return await _context.Reports
                    .Include(r => r.CreatedBy)
                    .Where(r => r.TeamId == teamId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "チームID {TeamId} のレポート一覧取得中にエラーが発生しました", teamId);
                throw;
            }
        }

        /// <summary>
        /// レポートを作成
        /// </summary>
        /// <param name="report">レポート</param>
        /// <returns>作成されたレポート</returns>
        public async Task<Report> CreateReportAsync(Report report)
        {
            try
            {
                report.CreatedAt = DateTime.Now;
                report.UpdatedAt = DateTime.Now;
                
                _context.Reports.Add(report);
                await _context.SaveChangesAsync();
                
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "レポート作成中にエラーが発生しました");
                throw;
            }
        }

        /// <summary>
        /// 個人用レポートを生成
        /// </summary>
        /// <param name="userId">対象ユーザーID</param>
        /// <param name="creatorId">作成者ID</param>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <param name="name">レポート名</param>
        /// <param name="format">出力形式</param>
        /// <returns>作成されたレポート</returns>
        public async Task<Report> GenerateIndividualReportAsync(
            string userId, 
            string creatorId, 
            DateTime startDate, 
            DateTime endDate, 
            string name, 
            string format = "PDF")
        {
            try
            {
                // 該当期間のユーザーの時間計測データを取得
                var timeEntries = await _context.TimeEntries
                    .Include(te => te.Task)
                    .ThenInclude(t => t!.Category)
                    .Where(te => te.UserId == userId && 
                                te.StartTime >= startDate && 
                                te.StartTime <= endDate && 
                                te.EndTime != null)
                    .ToListAsync();

                // タスク別の作業時間集計
                var taskSummary = timeEntries
                    .GroupBy(te => te.TaskId)
                    .Select(g => new
                    {
                        TaskId = g.Key,
                        TaskName = g.First().Task!.Name,
                        CategoryName = g.First().Task!.Category!.Name,
                        TotalMinutes = g.Sum(te => te.DurationMinutes ?? 0),
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.TotalMinutes)
                    .ToList();

                // カテゴリ別の作業時間集計
                var categorySummary = taskSummary
                    .GroupBy(t => t.CategoryName)
                    .Select(g => new
                    {
                        CategoryName = g.Key,
                        TotalMinutes = g.Sum(t => t.TotalMinutes),
                        TaskCount = g.Count()
                    })
                    .OrderByDescending(x => x.TotalMinutes)
                    .ToList();

                // 日別の作業時間集計
                var dailySummary = timeEntries
                    .GroupBy(te => te.StartTime.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalMinutes = g.Sum(te => te.DurationMinutes ?? 0),
                        EntryCount = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                // 合計作業時間
                var totalMinutes = timeEntries.Sum(te => te.DurationMinutes ?? 0);
                var totalHours = Math.Round(totalMinutes / 60.0, 1);

                // レポートデータを構築
                var reportData = new
                {
                    Summary = new
                    {
                        StartDate = startDate,
                        EndDate = endDate,
                        TotalTimeEntries = timeEntries.Count,
                        TotalMinutes = totalMinutes,
                        TotalHours = totalHours,
                        DailyAverageMinutes = timeEntries.Count > 0 ? Math.Round(totalMinutes / ((endDate - startDate).TotalDays + 1), 1) : 0
                    },
                    TaskData = taskSummary,
                    CategoryData = categorySummary,
                    DailyData = dailySummary
                };

                // レポートパラメータを構築
                var parameters = new
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    UserId = userId
                };

                // レポートを作成
                var report = new Report
                {
                    Name = name,
                    Type = "Individual",
                    PeriodType = (endDate - startDate).TotalDays <= 1 ? "Daily" :
                               (endDate - startDate).TotalDays <= 7 ? "Weekly" :
                               (endDate - startDate).TotalDays <= 31 ? "Monthly" : "Custom",
                    StartDate = startDate,
                    EndDate = endDate,
                    UserId = userId,
                    CreatedById = creatorId,
                    Format = format,
                    Parameters = JsonSerializer.Serialize(parameters),
                    ReportData = JsonSerializer.Serialize(reportData)
                };

                return await CreateReportAsync(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "個人用レポート生成中にエラーが発生しました");
                throw;
            }
        }

        /// <summary>
        /// 部署用レポートを生成
        /// </summary>
        /// <param name="departmentId">部署ID</param>
        /// <param name="creatorId">作成者ID</param>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <param name="name">レポート名</param>
        /// <param name="format">出力形式</param>
        /// <returns>作成されたレポート</returns>
        public async Task<Report> GenerateDepartmentReportAsync(
            int departmentId, 
            string creatorId, 
            DateTime startDate, 
            DateTime endDate, 
            string name, 
            string format = "PDF")
        {
            try
            {
                // 部署に所属するユーザーを取得
                var departmentUserIds = await _context.Users
                    .Where(u => u.DepartmentId == departmentId && u.IsInternal)
                    .Select(u => u.Id)
                    .ToListAsync();

                if (departmentUserIds.Count == 0)
                {
                    throw new InvalidOperationException("部署に所属するユーザーが見つかりません。");
                }

                // 該当期間の部署ユーザーの時間計測データを取得
                var timeEntries = await _context.TimeEntries
                    .Include(te => te.User)
                    .Include(te => te.Task)
                    .ThenInclude(t => t!.Category)
                    .Where(te => departmentUserIds.Contains(te.UserId) && 
                                te.StartTime >= startDate && 
                                te.StartTime <= endDate && 
                                te.EndTime != null)
                    .ToListAsync();

                // ユーザー別の作業時間集計
                var userSummary = timeEntries
                    .GroupBy(te => te.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        UserName = g.First().User!.FullName,
                        TotalMinutes = g.Sum(te => te.DurationMinutes ?? 0),
                        EntryCount = g.Count()
                    })
                    .OrderByDescending(x => x.TotalMinutes)
                    .ToList();

                // タスク別の作業時間集計
                var taskSummary = timeEntries
                    .GroupBy(te => te.TaskId)
                    .Select(g => new
                    {
                        TaskId = g.Key,
                        TaskName = g.First().Task!.Name,
                        CategoryName = g.First().Task!.Category!.Name,
                        TotalMinutes = g.Sum(te => te.DurationMinutes ?? 0),
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.TotalMinutes)
                    .ToList();

                // カテゴリ別の作業時間集計
                var categorySummary = taskSummary
                    .GroupBy(t => t.CategoryName)
                    .Select(g => new
                    {
                        CategoryName = g.Key,
                        TotalMinutes = g.Sum(t => t.TotalMinutes),
                        TaskCount = g.Count()
                    })
                    .OrderByDescending(x => x.TotalMinutes)
                    .ToList();

                // 日別の作業時間集計
                var dailySummary = timeEntries
                    .GroupBy(te => te.StartTime.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalMinutes = g.Sum(te => te.DurationMinutes ?? 0),
                        EntryCount = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                // 合計作業時間
                var totalMinutes = timeEntries.Sum(te => te.DurationMinutes ?? 0);
                var totalHours = Math.Round(totalMinutes / 60.0, 1);

                // レポートデータを構築
                var reportData = new
                {
                    Summary = new
                    {
                        StartDate = startDate,
                        EndDate = endDate,
                        TotalTimeEntries = timeEntries.Count,
                        TotalUsers = userSummary.Count,
                        TotalMinutes = totalMinutes,
                        TotalHours = totalHours,
                        DailyAverageMinutes = timeEntries.Count > 0 ? Math.Round(totalMinutes / ((endDate - startDate).TotalDays + 1), 1) : 0
                    },
                    UserData = userSummary,
                    TaskData = taskSummary,
                    CategoryData = categorySummary,
                    DailyData = dailySummary
                };

                // レポートパラメータを構築
                var parameters = new
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    DepartmentId = departmentId
                };

                // レポートを作成
                var report = new Report
                {
                    Name = name,
                    Type = "Department",
                    PeriodType = (endDate - startDate).TotalDays <= 1 ? "Daily" :
                               (endDate - startDate).TotalDays <= 7 ? "Weekly" :
                               (endDate - startDate).TotalDays <= 31 ? "Monthly" : "Custom",
                    StartDate = startDate,
                    EndDate = endDate,
                    DepartmentId = departmentId,
                    CreatedById = creatorId,
                    Format = format,
                    Parameters = JsonSerializer.Serialize(parameters),
                    ReportData = JsonSerializer.Serialize(reportData)
                };

                return await CreateReportAsync(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "部署用レポート生成中にエラーが発生しました");
                throw;
            }
        }

        /// <summary>
        /// ベンダー用レポートを生成
        /// </summary>
        /// <param name="vendorName">ベンダー名</param>
        /// <param name="creatorId">作成者ID</param>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <param name="name">レポート名</param>
        /// <param name="format">出力形式</param>
        /// <returns>作成されたレポート</returns>
        public async Task<Report> GenerateVendorReportAsync(
            string vendorName, 
            string creatorId, 
            DateTime startDate, 
            DateTime endDate, 
            string name, 
            string format = "PDF")
        {
            try
            {
                // ベンダーに所属するユーザーを取得
                var vendorUserIds = await _context.Users
                    .Where(u => !u.IsInternal && u.VendorName == vendorName)
                    .Select(u => u.Id)
                    .ToListAsync();

                if (vendorUserIds.Count == 0)
                {
                    throw new InvalidOperationException("ベンダーに所属するユーザーが見つかりません。");
                }

                // 該当期間のベンダーユーザーの時間計測データを取得
                var timeEntries = await _context.TimeEntries
                    .Include(te => te.Task)
                    .ThenInclude(t => t!.Category)
                    .Where(te => vendorUserIds.Contains(te.UserId) && 
                                te.StartTime >= startDate && 
                                te.StartTime <= endDate && 
                                te.EndTime != null)
                    .ToListAsync();

                // タスク別の作業時間集計
                var taskSummary = timeEntries
                    .GroupBy(te => te.TaskId)
                    .Select(g => new
                    {
                        TaskId = g.Key,
                        TaskName = g.First().Task!.Name,
                        CategoryName = g.First().Task!.Category!.Name,
                        TotalMinutes = g.Sum(te => te.DurationMinutes ?? 0),
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.TotalMinutes)
                    .ToList();

                // 日別の作業時間集計
                var dailySummary = timeEntries
                    .GroupBy(te => te.StartTime.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalMinutes = g.Sum(te => te.DurationMinutes ?? 0),
                        EntryCount = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                // 合計作業時間
                var totalMinutes = timeEntries.Sum(te => te.DurationMinutes ?? 0);
                var totalHours = Math.Round(totalMinutes / 60.0, 1);

                // レポートデータを構築
                var reportData = new
                {
                    Summary = new
                    {
                        VendorName = vendorName,
                        StartDate = startDate,
                        EndDate = endDate,
                        TotalTimeEntries = timeEntries.Count,
                        TotalMinutes = totalMinutes,
                        TotalHours = totalHours,
                        DailyAverageMinutes = timeEntries.Count > 0 ? Math.Round(totalMinutes / ((endDate - startDate).TotalDays + 1), 1) : 0
                    },
                    TaskData = taskSummary,
                    DailyData = dailySummary
                };

                // レポートパラメータを構築
                var parameters = new
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    VendorName = vendorName
                };

                // ベンダー担当者IDを取得
                var vendorUserId = await _context.Users
                    .Where(u => !u.IsInternal && u.VendorName == vendorName)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();

                // レポートを作成
                var report = new Report
                {
                    Name = name,
                    Type = "Vendor",
                    PeriodType = (endDate - startDate).TotalDays <= 1 ? "Daily" :
                               (endDate - startDate).TotalDays <= 7 ? "Weekly" :
                               (endDate - startDate).TotalDays <= 31 ? "Monthly" : "Custom",
                    StartDate = startDate,
                    EndDate = endDate,
                    UserId = vendorUserId, // ベンダー代表ユーザー
                    CreatedById = creatorId,
                    Format = format,
                    Parameters = JsonSerializer.Serialize(parameters),
                    ReportData = JsonSerializer.Serialize(reportData)
                };

                return await CreateReportAsync(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ベンダー用レポート生成中にエラーが発生しました");
                throw;
            }
        }

        /// <summary>
        /// レポートを更新
        /// </summary>
        /// <param name="report">更新するレポート</param>
        /// <returns>更新されたレポート</returns>
        public async Task<Report> UpdateReportAsync(Report report)
        {
            try
            {
                report.UpdatedAt = DateTime.Now;
                
                _context.Reports.Update(report);
                await _context.SaveChangesAsync();
                
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "レポートID {ReportId} の更新中にエラーが発生しました", report.Id);
                throw;
            }
        }

        /// <summary>
        /// レポートを削除
        /// </summary>
        /// <param name="reportId">レポートID</param>
        /// <returns>成功したかどうか</returns>
        public async Task<bool> DeleteReportAsync(int reportId)
        {
            try
            {
                var report = await _context.Reports.FindAsync(reportId);
                if (report == null)
                {
                    return false;
                }

                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "レポートID {ReportId} の削除中にエラーが発生しました", reportId);
                throw;
            }
        }
    }
}
