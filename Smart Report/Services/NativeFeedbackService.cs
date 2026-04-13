using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;

namespace Smart_Report.Services;

public class NativeFeedbackService
{
    // 轻触反馈 / Light tap feedback
    public async Task TapAsync()
    {
        try
        {
            if (HapticFeedback.IsSupported)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    HapticFeedback.Default.Perform(HapticFeedbackType.Click);
                });
            }
        }
        catch
        {
        }
    }

    // 长按反馈 / Long press feedback
    public async Task LongPressAsync()
    {
        try
        {
            if (HapticFeedback.IsSupported)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
                });
            }
        }
        catch
        {
        }
    }

    // 成功时短震动 / Short success vibration
    public Task SuccessVibrateAsync()
    {
        try
        {
            if (Vibration.IsSupported)
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(120));
            }
        }
        catch
        {
        }

        return Task.CompletedTask;
    }

    // 错误时较明显震动 / Stronger error vibration
    public Task ErrorVibrateAsync()
    {
        try
        {
            if (Vibration.IsSupported)
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250));
            }
        }
        catch
        {
        }

        return Task.CompletedTask;
    }
}