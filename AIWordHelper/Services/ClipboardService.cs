using System.Runtime.InteropServices;
using System.Windows;

namespace AIWordHelper.Services;

public class ClipboardService
{
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    private const byte VK_CONTROL = 0x11;
    private const byte VK_C = 0x43;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    public async Task<string?> GetSelectedTextAsync()
    {
        string? originalClipboard = null;
        string? selectedText = null;

        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                // Save original clipboard
                if (System.Windows.Clipboard.ContainsText())
                {
                    originalClipboard = System.Windows.Clipboard.GetText();
                }

                // Clear clipboard
                System.Windows.Clipboard.Clear();
            }
            catch { }
        });

        // Small delay to ensure clipboard is cleared
        await Task.Delay(50);

        // Simulate Ctrl+C
        keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
        keybd_event(VK_C, 0, 0, UIntPtr.Zero);
        keybd_event(VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

        // Wait for clipboard to be updated
        await Task.Delay(100);

        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                // Get selected text
                if (System.Windows.Clipboard.ContainsText())
                {
                    selectedText = System.Windows.Clipboard.GetText();
                }

                // Restore original clipboard
                if (originalClipboard != null)
                {
                    System.Windows.Clipboard.SetText(originalClipboard);
                }
            }
            catch { }
        });

        return selectedText;
    }
}
