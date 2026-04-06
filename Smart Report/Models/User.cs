using SQLite;

namespace Smart_Report.Models;

public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique, NotNull]
    public string Email { get; set; } = "";

    public string DisplayName { get; set; } = "";

    [NotNull]
    public string PasswordHash { get; set; } = "";

    [NotNull]
    public string Salt { get; set; } = "";

    // 管理员标记
    // Admin flag
    public bool IsAdmin { get; set; } = false;
}