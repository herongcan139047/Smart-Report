using SQLite;

namespace Smart_Report.Models;

public class ReportItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string PhotoPath { get; set; } = "";

    // 新增：定位预览图路径
    // New: location preview image path
    public string LocationPreviewPath { get; set; } = "";

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 新增：问题状态
    public bool IsResolved { get; set; } = false;

    // 新增：是谁标记为已解决
    public string ResolvedBy { get; set; } = "";

    // 新增：什么时候解决
    public DateTime? ResolvedAt { get; set; }
}