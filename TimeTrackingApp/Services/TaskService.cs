using Microsoft.EntityFrameworkCore;
using TimeTrackingApp.Data;

namespace TimeTrackingApp.Services
{
    /// <summary>
    /// タスク管理サービス
    /// </summary>
    public class TaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TaskService> _logger;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">データベースコンテキスト</param>
        /// <param name="logger">ロガー</param>
        public TaskService(
            ApplicationDbContext context,
            ILogger<TaskService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// すべてのタスクを取得
        /// </summary>
        /// <param name="includeInactive">非アクティブなタスクも含めるかどうか</param>
        /// <returns>タスクのリスト</returns>
        public async Task<List<Task>> GetAllTasksAsync(bool includeInactive = false)
        {
            try
            {
                var query = _context.Tasks
                    .Include(t => t.Category)
                    .Include(t => t.Assignee)
                    .Include(t => t.Department)
                    .AsQueryable();

                if (!includeInactive)
                {
                    query = query.Where(t => t.IsActive);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "タスク一覧の取得中にエラーが発生しました");
                throw;
            }
        }

        /// <summary>
        /// 部署別のタスクを取得
        /// </summary>
        /// <param name="departmentId">部署ID</param>
        /// <param name="includeInactive">非アクティブなタスクも含めるかどうか</param>
        /// <returns>タスクのリスト</returns>
        public async Task<List<Task>> GetTasksByDepartmentAsync(int departmentId, bool includeInactive = false)
        {
            try
            {
                var query = _context.Tasks
                    .Include(t => t.Category)
                    .Include(t => t.Assignee)
                    .Where(t => t.DepartmentId == departmentId);

                if (!includeInactive)
                {
                    query = query.Where(t => t.IsActive);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "部署ID {DepartmentId} のタスク取得中にエラーが発生しました", departmentId);
                throw;
            }
        }

        /// <summary>
        /// ユーザー別のタスクを取得
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="includeInactive">非アクティブなタスクも含めるかどうか</param>
        /// <returns>タスクのリスト</returns>
        public async Task<List<Task>> GetTasksByUserAsync(string userId, bool includeInactive = false)
        {
            try
            {
                var query = _context.Tasks
                    .Include(t => t.Category)
                    .Where(t => t.AssigneeId == userId);

                if (!includeInactive)
                {
                    query = query.Where(t => t.IsActive);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ユーザーID {UserId} のタスク取得中にエラーが発生しました", userId);
                throw;
            }
        }

        /// <summary>
        /// タスク詳細を取得
        /// </summary>
        /// <param name="taskId">タスクID</param>
        /// <returns>タスク詳細</returns>
        public async Task<Task?> GetTaskDetailAsync(int taskId)
        {
            try
            {
                return await _context.Tasks
                    .Include(t => t.Category)
                    .Include(t => t.Assignee)
                    .Include(t => t.Department)
                    .FirstOrDefaultAsync(t => t.Id == taskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "タスクID {TaskId} の詳細取得中にエラーが発生しました", taskId);
                throw;
            }
        }

        /// <summary>
        /// 新規タスクを作成
        /// </summary>
        /// <param name="task">タスク</param>
        /// <returns>作成されたタスク</returns>
        public async Task<Task> CreateTaskAsync(Task task)
        {
            try
            {
                task.CreatedAt = DateTime.Now;
                task.UpdatedAt = DateTime.Now;
                
                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();
                
                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "タスク {TaskName} の作成中にエラーが発生しました", task.Name);
                throw;
            }
        }

        /// <summary>
        /// タスクを更新
        /// </summary>
        /// <param name="task">更新するタスク</param>
        /// <returns>更新されたタスク</returns>
        public async Task<Task> UpdateTaskAsync(Task task)
        {
            try
            {
                task.UpdatedAt = DateTime.Now;
                
                _context.Tasks.Update(task);
                await _context.SaveChangesAsync();
                
                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "タスクID {TaskId} の更新中にエラーが発生しました", task.Id);
                throw;
            }
        }

        /// <summary>
        /// タスク有効状態の切り替え
        /// </summary>
        /// <param name="taskId">タスクID</param>
        /// <param name="isActive">有効状態</param>
        /// <returns>成功したかどうか</returns>
        public async Task<bool> ToggleTaskActiveStatusAsync(int taskId, bool isActive)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null)
                {
                    return false;
                }

                task.IsActive = isActive;
                task.UpdatedAt = DateTime.Now;
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "タスクID {TaskId} のステータス変更中にエラーが発生しました", taskId);
                throw;
            }
        }

        /// <summary>
        /// タスクを削除
        /// </summary>
        /// <param name="taskId">削除するタスクID</param>
        /// <returns>成功したかどうか</returns>
        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null)
                {
                    return false;
                }

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "タスクID {TaskId} の削除中にエラーが発生しました", taskId);
                throw;
            }
        }

        /// <summary>
        /// すべてのタスクカテゴリを取得
        /// </summary>
        /// <returns>カテゴリのリスト</returns>
        public async Task<List<TaskCategory>> GetAllCategoriesAsync()
        {
            try
            {
                return await _context.TaskCategories
                    .Include(c => c.ParentCategory)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "タスクカテゴリ一覧の取得中にエラーが発生しました");
                throw;
            }
        }

        /// <summary>
        /// 新規タスクカテゴリを作成
        /// </summary>
        /// <param name="category">カテゴリ</param>
        /// <returns>作成されたカテゴリ</returns>
        public async Task<TaskCategory> CreateCategoryAsync(TaskCategory category)
        {
            try
            {
                _context.TaskCategories.Add(category);
                await _context.SaveChangesAsync();
                
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "カテゴリ {CategoryName} の作成中にエラーが発生しました", category.Name);
                throw;
            }
        }
    }
}
