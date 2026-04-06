using System.Security.Cryptography;
using System.Text;
using Microsoft.Maui.Storage;
using Smart_Report.Data;
using Smart_Report.Models;

namespace Smart_Report.Services;

public class AuthService
{
    private readonly AppDb _db;
    private const string CurrentUserIdKey = "current_user_id";

    // 管理员账号
    // Admin account
    public const string AdminEmail = "admin@smartreport.com";
    public const string AdminPassword = "admin123456";

    public User? CurrentUser { get; private set; }

    // 常用属性
    // Common properties
    public int? CurrentUserId => CurrentUser?.Id;
    public string CurrentName => CurrentUser?.DisplayName ?? "";
    public string CurrentEmail => CurrentUser?.Email ?? "";

    // 只要当前邮箱是管理员邮箱，就按管理员处理
    // Treat the reserved admin email as admin even if DB flag is not set
    public bool IsAdmin =>
        CurrentUser != null &&
        IsAdminEmail(CurrentUser.Email);

    public AuthService(AppDb db)
    {
        _db = db;
    }

    public Task<User?> GetCurrentUserAsync()
        => Task.FromResult(CurrentUser);

    public Task<int?> GetCurrentUserIdAsync()
        => Task.FromResult(CurrentUserId);

    public Task LogoutAsync()
    {
        Logout();
        return Task.CompletedTask;
    }

    public async Task RestoreSessionAsync()
    {
        await EnsureAdminAccountAsync();

        var id = Preferences.Get(CurrentUserIdKey, 0);
        if (id <= 0) return;

        CurrentUser = await _db.GetUserByIdAsync(id);

        // 即使数据库里 IsAdmin 没设好，也强制把 admin 邮箱识别为管理员
        if (CurrentUser != null && IsAdminEmail(CurrentUser.Email))
            CurrentUser.IsAdmin = true;
    }

    public async Task<(bool ok, string msg)> LoginAsync(string email, string password)
    {
        await EnsureAdminAccountAsync();

        email = (email ?? "").Trim().ToLowerInvariant();
        password ??= "";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Email and password are required.");

        var user = await _db.GetUserByEmailAsync(email);
        if (user == null) return (false, "User not found.");

        // 管理员特殊登录：只要邮箱是管理员邮箱，就走管理员密码
        // Admin special login
        if (IsAdminEmail(user.Email))
        {
            if (password != AdminPassword)
                return (false, "Wrong admin password.");

            user.IsAdmin = true;
            CurrentUser = user;
            Preferences.Set(CurrentUserIdKey, user.Id);
            return (true, "Admin login success.");
        }

        // 普通用户登录
        // Normal user login
        if (!SlowEquals(user.PasswordHash, Hash(password)))
            return (false, "Wrong password.");

        CurrentUser = user;
        Preferences.Set(CurrentUserIdKey, user.Id);

        return (true, "Login success.");
    }

    public async Task<(bool ok, string msg)> RegisterAsync(string email, string displayName, string password)
    {
        await EnsureAdminAccountAsync();

        email = (email ?? "").Trim().ToLowerInvariant();
        displayName = (displayName ?? "").Trim();
        password ??= "";

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return (false, "Please enter a valid email.");
        if (string.IsNullOrWhiteSpace(displayName))
            return (false, "Please enter a display name.");
        if (password.Length < 4)
            return (false, "Password must be at least 4 characters (demo).");

        // 不允许普通注册占用管理员邮箱
        // Reserve admin email
        if (IsAdminEmail(email))
            return (false, "This email is reserved for the admin account.");

        var exists = await _db.GetUserByEmailAsync(email);
        if (exists != null) return (false, "Email already registered.");

        var user = new User
        {
            Email = email,
            DisplayName = displayName,
            PasswordHash = Hash(password),
            Salt = "",
            IsAdmin = false
        };

        await _db.InsertUserAsync(user);

        CurrentUser = await _db.GetUserByEmailAsync(email);
        if (CurrentUser != null)
            Preferences.Set(CurrentUserIdKey, CurrentUser.Id);

        return (true, "Registered successfully.");
    }

    public void Logout()
    {
        CurrentUser = null;
        Preferences.Remove(CurrentUserIdKey);
    }

    private async Task EnsureAdminAccountAsync()
    {
        var admin = await _db.GetUserByEmailAsync(AdminEmail);
        if (admin != null) return;

        var adminUser = new User
        {
            Email = AdminEmail,
            DisplayName = "Admin",
            PasswordHash = "",
            Salt = "",
            IsAdmin = true
        };

        await _db.InsertUserAsync(adminUser);
    }

    private static bool IsAdminEmail(string? email)
    {
        return string.Equals(
            (email ?? "").Trim(),
            AdminEmail,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    private static bool SlowEquals(string a, string b)
    {
        if (a is null || b is null) return false;
        if (a.Length != b.Length) return false;

        var diff = 0;
        for (int i = 0; i < a.Length; i++)
            diff |= a[i] ^ b[i];

        return diff == 0;
    }
}