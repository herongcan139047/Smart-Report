using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Smart_Report.Data;
using Smart_Report.Models;
using Smart_Report.Services;

namespace Smart_Report.ViewModels;

public partial class ReportViewModel : BaseViewModel
{
    private readonly AppDb _db;
    private readonly AuthService _auth;

    [ObservableProperty] private string title = "";
    [ObservableProperty] private string description = "";
    [ObservableProperty] private string? photoPath;
    [ObservableProperty] private string locationText = "Location: (not set)";
    [ObservableProperty] private double? latitude;
    [ObservableProperty] private double? longitude;

    private int _userId;

    public ReportViewModel(AppDb db, AuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    public async Task EnsureAuthAsync()
    {
        var uid = await _auth.GetCurrentUserIdAsync();
        if (uid is null)
        {
            await Shell.Current.GoToAsync("//login");
            return;
        }
        _userId = uid.Value;
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            var result = await MediaPicker.CapturePhotoAsync();
            if (result == null) return;

            var newFile = Path.Combine(FileSystem.AppDataDirectory, $"{Guid.NewGuid():N}{Path.GetExtension(result.FileName)}");
            await using var src = await result.OpenReadAsync();
            await using var dst = File.OpenWrite(newFile);
            await src.CopyToAsync(dst);

            PhotoPath = newFile;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task GetLocationAsync()
    {
        try
        {
            var req = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var loc = await Geolocation.GetLocationAsync(req);

            if (loc == null) return;

            Latitude = loc.Latitude;
            Longitude = loc.Longitude;
            LocationText = $"Location: {Latitude:F6}, {Longitude:F6}";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        var t = (Title ?? "").Trim();
        if (string.IsNullOrWhiteSpace(t))
        {
            ErrorMessage = "Title required";
            return;
        }

        var report = new ReportItem
        {
            UserId = _userId,
            Title = t,
            Description = (Description ?? "").Trim(),
            PhotoPath = PhotoPath,
            Latitude = Latitude,
            Longitude = Longitude,
            CreatedAt = DateTime.UtcNow
        };

        await _db.InsertReportAsync(report);

        Title = "";
        Description = "";
        PhotoPath = null;
        Latitude = Longitude = null;
        LocationText = "Location: (not set)";
        ErrorMessage = "";

        await Application.Current.MainPage.DisplayAlert("OK", "Report saved", "OK");
    }
}
