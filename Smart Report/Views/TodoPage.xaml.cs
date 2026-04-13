using Microsoft.Extensions.DependencyInjection;
using Smart_Report.Data;
using Smart_Report.Models;
using Smart_Report.Services;

namespace Smart_Report.Views;

public partial class TodoPage : ContentPage
{
    private readonly AppDb _db;
    private readonly AuthService _auth;
    private readonly NativeFeedbackService _feedback;
    private List<TodoItem> _items = new();

    public TodoPage()
    {
        InitializeComponent();

        _db = MauiProgram.Services.GetRequiredService<AppDb>();
        _auth = MauiProgram.Services.GetRequiredService<AuthService>();
        _feedback = MauiProgram.Services.GetRequiredService<NativeFeedbackService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            await _feedback.ErrorVibrateAsync();
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task ReloadAsync()
    {
        var uid = _auth.CurrentUserId;

        if (uid == null)
        {
            await Shell.Current.GoToAsync("login");
            return;
        }

        _items = await _db.GetTodosAsync(uid.Value);
        TodoList.ItemsSource = _items;
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        try
        {
            var text = (TodoEntry.Text ?? "").Trim();
            if (text.Length == 0)
            {
                await _feedback.ErrorVibrateAsync();
                return;
            }

            var uid = _auth.CurrentUserId;
            if (uid == null)
            {
                await Shell.Current.GoToAsync("login");
                return;
            }

            await _db.InsertTodoAsync(new TodoItem
            {
                UserId = uid.Value,
                Title = text,
                IsDone = false
            });

            TodoEntry.Text = "";

            await _feedback.TapAsync();
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            await _feedback.ErrorVibrateAsync();
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        try
        {
            if (sender is not CheckBox cb || cb.BindingContext is not TodoItem item)
                return;

            item.IsDone = e.Value;
            await _db.UpdateTodoAsync(item);

            await _feedback.TapAsync();
        }
        catch (Exception ex)
        {
            await _feedback.ErrorVibrateAsync();
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnDeleteInvoked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not SwipeItem si || si.CommandParameter is not TodoItem item)
                return;

            await _db.DeleteTodoAsync(item);

            await _feedback.LongPressAsync();
            await _feedback.SuccessVibrateAsync();

            await ReloadAsync();
        }
        catch (Exception ex)
        {
            await _feedback.ErrorVibrateAsync();
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}