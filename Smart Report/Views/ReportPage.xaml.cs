using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Smart_Report.Data;
using Smart_Report.Models;
using Smart_Report.Services;

namespace Smart_Report.Views;

public partial class ReportPage : ContentPage
{
    private readonly ObservableCollection<ReportItem> _reports = new();
    private readonly AppDb _db;
    private readonly AuthService _auth;

    private string? _photoPath;
    private double _lat;
    private double _lng;
    private bool _hasLocation;

    public ReportPage()
    {
        InitializeComponent();

        _db = MauiProgram.Services.GetRequiredService<AppDb>();
        _auth = MauiProgram.Services.GetRequiredService<AuthService>();

        ReportList.ItemsSource = _reports;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _db.InitAsync();
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        _reports.Clear();

        var userId = _auth.CurrentUserId;
        if (userId == null || userId <= 0)
        {
            InfoLabel.Text = "Please login first.";
            return;
        }

        // 所有人都看全部 report
        InfoLabel.Text = "Viewing all reports.";
        var list = await _db.GetAllReportsAsync();

        foreach (var item in list)
            _reports.Add(item);
    }

    private async void OnTakePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                InfoLabel.Text = "This device does not support taking photos.";
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo == null) return;

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
            var newFile = Path.Combine(FileSystem.AppDataDirectory, fileName);

            await using (var stream = await photo.OpenReadAsync())
            await using (var newStream = File.OpenWrite(newFile))
            {
                await stream.CopyToAsync(newStream);
            }

            _photoPath = newFile;

            PreviewImage.Source = ImageSource.FromFile(newFile);
            PreviewImage.IsVisible = true;

            InfoLabel.Text = "Photo captured.";
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Photo error: {ex.Message}";
        }
    }

    private async void OnGetLocationClicked(object sender, EventArgs e)
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                InfoLabel.Text = "Location permission denied.";
                return;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location == null)
            {
                InfoLabel.Text = "Location not available.";
                return;
            }

            _lat = location.Latitude;
            _lng = location.Longitude;
            _hasLocation = true;

            InfoLabel.Text = $"Location: {_lat:F6}, {_lng:F6}";
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Location error: {ex.Message}";
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            var userId = _auth.CurrentUserId;
            if (userId == null || userId <= 0)
            {
                InfoLabel.Text = "Please login first.";
                return;
            }

            var title = TitleEntry.Text?.Trim() ?? "";
            var desc = DescEditor.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(title))
            {
                InfoLabel.Text = "Please enter a title.";
                return;
            }

            var item = new ReportItem
            {
                UserId = userId.Value,
                Title = title,
                Description = desc,
                PhotoPath = _photoPath ?? "",
                Latitude = _hasLocation ? _lat : (double?)null,
                Longitude = _hasLocation ? _lng : (double?)null,
                CreatedAt = DateTime.UtcNow,
                IsResolved = false,
                ResolvedBy = "",
                ResolvedAt = null
            };

            await _db.AddReportAsync(item);

            ResetForm();
            InfoLabel.Text = "Saved.";

            await ReloadAsync();
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Save error: {ex.Message}";
        }
    }

    private async void OnResolveClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button || button.CommandParameter is not ReportItem item)
                return;

            if (!CanManage(item))
            {
                InfoLabel.Text = "You can only manage your own report unless you are admin.";
                return;
            }

            var resolvedBy = GetResolverName();

            await _db.MarkReportResolvedAsync(item.Id, resolvedBy);
            InfoLabel.Text = $"Marked as resolved by {resolvedBy}.";

            await ReloadAsync();
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Resolve error: {ex.Message}";
        }
    }

    private async void OnUnresolveClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button || button.CommandParameter is not ReportItem item)
                return;

            if (!CanManage(item))
            {
                InfoLabel.Text = "You can only manage your own report unless you are admin.";
                return;
            }

            await _db.MarkReportUnresolvedAsync(item.Id);
            InfoLabel.Text = "Marked as unresolved.";

            await ReloadAsync();
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Undo error: {ex.Message}";
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button || button.CommandParameter is not ReportItem item)
                return;

            if (!CanDelete(item))
            {
                InfoLabel.Text = "You can only delete your own report unless you are admin.";
                return;
            }

            bool confirm = await DisplayAlert(
                "Delete Report",
                $"Are you sure you want to delete \"{item.Title}\"?",
                "Yes",
                "No");

            if (!confirm) return;

            await _db.DeleteReportAsync(item);
            InfoLabel.Text = "Report deleted.";

            await ReloadAsync();
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Delete error: {ex.Message}";
        }
    }

    private bool CanManage(ReportItem item)
    {
        var userId = _auth.CurrentUserId;
        if (userId == null || userId <= 0) return false;

        // admin 可以管理所有人的 report
        if (IsAdmin())
            return true;

        // 普通用户只能管理自己的
        return item.UserId == userId.Value;
    }

    private bool CanDelete(ReportItem item)
    {
        var userId = _auth.CurrentUserId;
        if (userId == null || userId <= 0) return false;

        // admin 可以删除所有人的 report
        if (IsAdmin())
            return true;

        // 普通用户只能删自己的
        return item.UserId == userId.Value;
    }

    private bool IsAdmin()
    {
        var email = (_auth.CurrentEmail ?? "").Trim().ToLowerInvariant();
        return email == "admin@smartreport.com";
    }

    private string GetResolverName()
    {
        if (!string.IsNullOrWhiteSpace(_auth.CurrentName))
            return _auth.CurrentName;

        if (!string.IsNullOrWhiteSpace(_auth.CurrentEmail))
            return _auth.CurrentEmail;

        return $"User {_auth.CurrentUserId}";
    }

    private void ResetForm()
    {
        TitleEntry.Text = "";
        DescEditor.Text = "";

        _photoPath = null;
        _lat = 0;
        _lng = 0;
        _hasLocation = false;

        PreviewImage.Source = null;
        PreviewImage.IsVisible = false;
    }
}