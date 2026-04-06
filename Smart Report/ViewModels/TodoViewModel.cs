using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Smart_Report.Data;
using Smart_Report.Models;
using Smart_Report.Services;

namespace Smart_Report.ViewModels;

public partial class TodoViewModel : BaseViewModel
{
    private readonly AppDb _db;
    private readonly AuthService _auth;

    public ObservableCollection<TodoItem> Todos { get; } = new();
    public ObservableCollection<TodoItem> FilteredTodos { get; } = new();

    [ObservableProperty] private string newTitle = "";
    [ObservableProperty] private string query = "";

    private int _userId;

    public TodoViewModel(AppDb db, AuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    public async Task EnsureAuthAndLoadAsync()
    {
        var uid = await _auth.GetCurrentUserIdAsync();
        if (uid is null)
        {
            await Shell.Current.GoToAsync("//login");
            return;
        }
        _userId = uid.Value;
        await LoadAsync();
    }

    partial void OnQueryChanged(string value) => ApplyFilter();

    private async Task LoadAsync()
    {
        Todos.Clear();
        var items = await _db.GetTodosAsync(_userId);
        foreach (var t in items) Todos.Add(t);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        FilteredTodos.Clear();
        var q = (Query ?? "").Trim().ToLowerInvariant();

        foreach (var t in Todos)
        {
            if (string.IsNullOrEmpty(q) || t.Title.ToLowerInvariant().Contains(q))
                FilteredTodos.Add(t);
        }
    }

    [RelayCommand]
    private async Task AddTodoAsync()
    {
        var title = (NewTitle ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title)) return;

        var todo = new TodoItem { UserId = _userId, Title = title, IsDone = false, CreatedAt = DateTime.UtcNow };
        await _db.InsertTodoAsync(todo);

        NewTitle = "";
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ToggleDoneAsync(TodoItem item)
    {
        item.IsDone = !item.IsDone;
        await _db.UpdateTodoAsync(item);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteAsync(TodoItem item)
    {
        await _db.DeleteTodoAsync(item);
        await LoadAsync();
    }
}
