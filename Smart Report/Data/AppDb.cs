using Microsoft.Maui.Storage;
using SQLite;
using Smart_Report.Models;

namespace Smart_Report.Data;

public class AppDb
{
    private readonly SQLiteAsyncConnection _conn;
    private bool _initialized;

    public AppDb() : this(Path.Combine(FileSystem.AppDataDirectory, "smart_report.db3"))
    {
    }

    public AppDb(string dbPath)
    {
        _conn = new SQLiteAsyncConnection(
            dbPath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache
        );
    }

    // 用来检查旧数据库表结构
    // Used to inspect existing SQLite table columns
    private sealed class TableInfo
    {
        public int cid { get; set; }
        public string name { get; set; } = "";
        public string type { get; set; } = "";
        public int notnull { get; set; }
        public string dflt_value { get; set; } = "";
        public int pk { get; set; }
    }

    public async Task InitAsync()
    {
        if (_initialized) return;

        await _conn.CreateTableAsync<User>();
        await _conn.CreateTableAsync<TodoItem>();
        await _conn.CreateTableAsync<ReportItem>();

        await EnsureUserTableColumnsAsync();
        await EnsureReportTableColumnsAsync();
        await EnsureAdminAccountAsync();

        _initialized = true;
    }

    private async Task EnsureUserTableColumnsAsync()
    {
        var cols = await _conn.QueryAsync<TableInfo>("PRAGMA table_info(User);");

        if (!cols.Any(c => c.name == "IsAdmin"))
        {
            await _conn.ExecuteAsync(
                "ALTER TABLE User ADD COLUMN IsAdmin INTEGER NOT NULL DEFAULT 0;"
            );
        }
    }

    private async Task EnsureReportTableColumnsAsync()
    {
        var cols = await _conn.QueryAsync<TableInfo>("PRAGMA table_info(ReportItem);");

        if (!cols.Any(c => c.name == "IsResolved"))
        {
            await _conn.ExecuteAsync(
                "ALTER TABLE ReportItem ADD COLUMN IsResolved INTEGER NOT NULL DEFAULT 0;"
            );
        }

        if (!cols.Any(c => c.name == "ResolvedBy"))
        {
            await _conn.ExecuteAsync(
                "ALTER TABLE ReportItem ADD COLUMN ResolvedBy TEXT NOT NULL DEFAULT '';"
            );
        }

        if (!cols.Any(c => c.name == "ResolvedAt"))
        {
            await _conn.ExecuteAsync(
                "ALTER TABLE ReportItem ADD COLUMN ResolvedAt TEXT NULL;"
            );
        }

        // 新增：定位预览图路径列
        // New: location preview image path column
        if (!cols.Any(c => c.name == "LocationPreviewPath"))
        {
            await _conn.ExecuteAsync(
                "ALTER TABLE ReportItem ADD COLUMN LocationPreviewPath TEXT NOT NULL DEFAULT '';"
            );
        }
    }

    private async Task EnsureAdminAccountAsync()
    {
        const string adminEmail = "admin@smartreport.com";

        var admin = await _conn.Table<User>()
            .Where(u => u.Email == adminEmail)
            .FirstOrDefaultAsync();

        if (admin == null)
        {
            await _conn.InsertAsync(new User
            {
                Email = adminEmail,
                DisplayName = "Administrator",
                PasswordHash = "__ADMIN__",
                Salt = "",
                IsAdmin = true
            });
        }
        else if (!admin.IsAdmin)
        {
            admin.IsAdmin = true;
            await _conn.UpdateAsync(admin);
        }
    }

    // ---------------- Users ----------------
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await InitAsync();
        email = (email ?? "").Trim().ToLowerInvariant();
        return await _conn.Table<User>()
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        await InitAsync();
        return await _conn.Table<User>()
            .Where(u => u.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> InsertUserAsync(User user)
    {
        await InitAsync();
        return await _conn.InsertAsync(user);
    }

    // ---------------- Todos ----------------
    public async Task<List<TodoItem>> GetTodosAsync(int userId)
    {
        await InitAsync();
        return await _conn.Table<TodoItem>()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> InsertTodoAsync(TodoItem item)
    {
        await InitAsync();
        return await _conn.InsertAsync(item);
    }

    public async Task<int> UpdateTodoAsync(TodoItem item)
    {
        await InitAsync();
        return await _conn.UpdateAsync(item);
    }

    public async Task<int> DeleteTodoAsync(TodoItem item)
    {
        await InitAsync();
        return await _conn.DeleteAsync(item);
    }

    // ---------------- Reports ----------------

    // 如果你某些页面还需要“只看自己的”，这个方法保留
    // Keep this if some pages still need "my reports only"
    public async Task<List<ReportItem>> GetReportsAsync(int userId)
    {
        await InitAsync();
        return await _conn.Table<ReportItem>()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    // 所有人都能看到全部 report
    public async Task<List<ReportItem>> GetAllReportsAsync()
    {
        await InitAsync();
        return await _conn.Table<ReportItem>()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<ReportItem?> GetReportByIdAsync(int id)
    {
        await InitAsync();
        return await _conn.Table<ReportItem>()
            .Where(r => r.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> InsertReportAsync(ReportItem item)
    {
        await InitAsync();
        return await _conn.InsertAsync(item);
    }

    public Task<int> AddReportAsync(ReportItem item)
        => InsertReportAsync(item);

    public async Task<int> UpdateReportAsync(ReportItem item)
    {
        await InitAsync();
        return await _conn.UpdateAsync(item);
    }

    public async Task<int> DeleteReportAsync(ReportItem item)
    {
        await InitAsync();
        return await _conn.DeleteAsync(item);
    }

    // 标记为已解决
    public async Task<int> MarkReportResolvedAsync(int reportId, string resolvedBy)
    {
        await InitAsync();

        var item = await GetReportByIdAsync(reportId);
        if (item == null) return 0;

        item.IsResolved = true;
        item.ResolvedBy = resolvedBy ?? "";
        item.ResolvedAt = DateTime.UtcNow;

        return await _conn.UpdateAsync(item);
    }

    // 取消已解决
    public async Task<int> MarkReportUnresolvedAsync(int reportId)
    {
        await InitAsync();

        var item = await GetReportByIdAsync(reportId);
        if (item == null) return 0;

        item.IsResolved = false;
        item.ResolvedBy = "";
        item.ResolvedAt = null;

        return await _conn.UpdateAsync(item);
    }
}