using CommunityToolkit.Mvvm.ComponentModel;

namespace Smart_Report.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string errorMessage = "";
}
