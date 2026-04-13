using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Smart_Report.Data;
using Smart_Report.Models;
using Smart_Report.Services;

namespace Smart_Report.Views;

public partial class ReportPage : ContentPage
{
    private readonly ObservableCollection<ReportItem> _reports = new();
    private readonly AppDb _db;
    private readonly AuthService _auth;
    private readonly NativeFeedbackService _feedback;

    // 这里填你自己的 Google Static Maps API Key
    // Put your own Google Static Maps API Key here
    private const string GoogleStaticMapsApiKey = "";

    private string? _photoPath;
    private string? _locationPreviewPath;
    private double _lat;
    private double _lng;
    private bool _hasLocation;

    public ReportPage()
    {
        InitializeComponent();

        _db = MauiProgram.Services.GetRequiredService<AppDb>();
        _auth = MauiProgram.Services.GetRequiredService<AuthService>();
        _feedback = MauiProgram.Services.GetRequiredService<NativeFeedbackService>();

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
                await _feedback.ErrorVibrateAsync();
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
            await _feedback.TapAsync();
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Photo error: {ex.Message}";
            await _feedback.ErrorVibrateAsync();
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
                await _feedback.ErrorVibrateAsync();
                return;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location == null)
            {
                InfoLabel.Text = "Location not available.";
                await _feedback.ErrorVibrateAsync();
                return;
            }

            _lat = location.Latitude;
            _lng = location.Longitude;
            _hasLocation = true;

            await GenerateLocationPreviewAsync();

            if (!string.IsNullOrWhiteSpace(_locationPreviewPath))
            {
                InfoLabel.Text = $"Location captured: {_lat:F6}, {_lng:F6}";
            }
            else
            {
                InfoLabel.Text =
                    $"Location captured. Preview image unavailable, but you can open Google Maps. Lat: {_lat:F6}, Lng: {_lng:F6}";
            }

            await _feedback.TapAsync();
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Location error: {ex.Message}";
            await _feedback.ErrorVibrateAsync();
        }
    }

    private async Task GenerateLocationPreviewAsync()
    {
        try
        {
            _locationPreviewPath = await SaveStaticMapPreviewAsync(_lat, _lng);

            OpenMapButton.IsVisible = _hasLocation;

            if (!string.IsNullOrWhiteSpace(_locationPreviewPath) && File.Exists(_locationPreviewPath))
            {
                LocationPreviewImage.Source = ImageSource.FromFile(_locationPreviewPath);
                LocationPreviewImage.IsVisible = true;
                LocationPreviewTitle.IsVisible = true;
            }
            else
            {
                LocationPreviewImage.Source = null;
                LocationPreviewImage.IsVisible = false;
                LocationPreviewTitle.IsVisible = false;
            }
        }
        catch
        {
            OpenMapButton.IsVisible = _hasLocation;
            LocationPreviewImage.Source = null;
            LocationPreviewImage.IsVisible = false;
            LocationPreviewTitle.IsVisible = false;
        }
    }

    private async Task<string?> SaveStaticMapPreviewAsync(double lat, double lng)
    {
        try
        {
            // 没填 API key 时，不生成地图预览图，但程序不会报错
            if (string.IsNullOrWhiteSpace(GoogleStaticMapsApiKey))
                return null;

            var latText = lat.ToString(CultureInfo.InvariantCulture);
            var lngText = lng.ToString(CultureInfo.InvariantCulture);

            var url =
                $"https://maps.googleapis.com/maps/api/staticmap" +
                $"?center={latText},{lngText}" +
                $"&zoom=16" +
                $"&size=600x300" +
                $"&scale=2" +
                $"&maptype=roadmap" +
                $"&markers=color:red|label:R|{latText},{lngText}" +
                $"&key={GoogleStaticMapsApiKey}";

            using var http = new HttpClient();
            var bytes = await http.GetByteArrayAsync(url);

            if (bytes == null || bytes.Length == 0)
                return null;

            var fileName = $"location_map_{Guid.NewGuid():N}.png";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            await File.WriteAllBytesAsync(filePath, bytes);
            return filePath;
        }
        catch
        {
            return null;
        }
    }

    private async void OnOpenCurrentLocationInGoogleMapsClicked(object sender, EventArgs e)
    {
        try
        {
            if (!_hasLocation)
            {
                InfoLabel.Text = "Please get location first.";
                await _feedback.ErrorVibrateAsync();
                return;
            }

            await OpenGoogleMapsAsync(_lat, _lng);
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Open map error: {ex.Message}";
            await _feedback.ErrorVibrateAsync();
        }
    }

    private async void OnOpenSavedLocationInGoogleMapsClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button || button.CommandParameter is not ReportItem item)
                return;

            if (item.Latitude == null || item.Longitude == null)
            {
                InfoLabel.Text = "This report has no saved location.";
                await _feedback.ErrorVibrateAsync();
                return;
            }

            await OpenGoogleMapsAsync(item.Latitude.Value, item.Longitude.Value);
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Open saved map error: {ex.Message}";
            await _feedback.ErrorVibrateAsync();
        }
    }

    private async Task OpenGoogleMapsAsync(double lat, double lng)
    {
        var latText = lat.ToString(CultureInfo.InvariantCulture);
        var lngText = lng.ToString(CultureInfo.InvariantCulture);

        var url = $"https://www.google.com/maps/search/?api=1&query={latText},{lngText}";
        await Launcher.Default.OpenAsync(new Uri(url));
        await _feedback.TapAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            var userId = _auth.CurrentUserId;
            if (userId == null || userId <= 0)
            {
                InfoLabel.Text = "Please login first.";
                await _feedback.ErrorVibrateAsync();
                return;
            }

            var title = TitleEntry.Text?.Trim() ?? "";
            var desc = DescEditor.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(title))
            {
                InfoLabel.Text = "Please enter a title.";
                await _feedback.ErrorVibrateAsync();
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
                LocationPreviewPath = _locationPreviewPath ?? "",
                CreatedAt = DateTime.UtcNow,
                IsResolved = false,
                ResolvedBy = "",
                ResolvedAt = null
            };

            await _db.AddReportAsync(item);

            ResetForm();
            InfoLabel.Text = "Saved.";

            await _feedback.TapAsync();
            await _feedback.SuccessVibrateAsync();

            await ReloadAsync();
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Save error: {ex.Message}";
            await _feedback.ErrorVibrateAsync();
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
                await _feedback.ErrorVibrateAsync();
                return;
            }

            var resolvedBy = GetResolverName();

            await _db.MarkReportResolvedAsync(item.Id, resolvedBy);
            InfoLabel.Text = $"Marked as resolved by {resolvedBy}.";

            await _feedback.TapAsync();
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Resolve error: {ex.Message}";
            await _feedback.ErrorVibrateAsync();
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
                await _feedback.ErrorVibrateAsync();
                return;
            }

            await _db.MarkReportUnresolvedAsync(item.Id);
            InfoLabel.Text = "Marked as unresolved.";

            await _feedback.TapAsync();
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Undo error: {ex.Message}";
            await _feedback.ErrorVibrateAsync();
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
                await _feedback.ErrorVibrateAsync();
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

            await _feedback.LongPressAsync();
            await _feedback.SuccessVibrateAsync();

            await ReloadAsync();
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Delete error: {ex.Message}";
            await _feedback.ErrorVibrateAsync();
        }
    }

    private bool CanManage(ReportItem item)
    {
        var userId = _auth.CurrentUserId;
        if (userId == null || userId <= 0) return false;

        if (IsAdmin())
            return true;

        return item.UserId == userId.Value;
    }

    private bool CanDelete(ReportItem item)
    {
        var userId = _auth.CurrentUserId;
        if (userId == null || userId <= 0) return false;

        if (IsAdmin())
            return true;

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
        _locationPreviewPath = null;
        _lat = 0;
        _lng = 0;
        _hasLocation = false;

        PreviewImage.Source = null;
        PreviewImage.IsVisible = false;

        LocationPreviewImage.Source = null;
        LocationPreviewImage.IsVisible = false;
        LocationPreviewTitle.IsVisible = false;

        OpenMapButton.IsVisible = false;
    }
}