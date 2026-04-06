using SQLite;

namespace Smart_Report.Models;

public class TodoItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }

    // 统一存数据库
    public string Title { get; set; } = "";

    public bool IsDone { get; set; }

    // ✅ 你 VM 需要 CreatedAt
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ✅ 兼容旧代码/旧 XAML：Text <-> Title
    [Ignore]
    public string Text
    {
        get => Title;
        set => Title = value;
    }
}