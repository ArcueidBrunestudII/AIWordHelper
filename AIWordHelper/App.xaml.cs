using System.Windows;
using System.Windows.Threading;
using AIWordHelper.Services;
using AIWordHelper.Views;
using Forms = System.Windows.Forms;
using DrawingIcon = System.Drawing.Icon;
using DrawingSystemIcons = System.Drawing.SystemIcons;

namespace AIWordHelper;

public partial class App : Application
{
    private Forms.NotifyIcon? _notifyIcon;
    private ConfigService _configService = null!;
    private AIService _aiService = null!;
    private MouseHookService _mouseHookService = null!;
    private ClipboardService _clipboardService = null!;
    private SettingsWindow? _settingsWindow;
    private DispatcherTimer? _selectionTimer;
    private System.Windows.Point _lastMousePosition;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize services
        _configService = new ConfigService();
        _configService.Load();

        _aiService = new AIService
        {
            ApiUrl = _configService.Config.ApiUrl,
            ApiKey = _configService.Config.ApiKey
        };

        _clipboardService = new ClipboardService();

        // Setup mouse hook
        _mouseHookService = new MouseHookService();
        _mouseHookService.OnSelectionComplete += OnSelectionComplete;
        _mouseHookService.Start();

        // Setup selection timer
        _selectionTimer = new DispatcherTimer();
        _selectionTimer.Tick += SelectionTimer_Tick;

        // Create system tray icon
        CreateTrayIcon();
    }

    private void CreateTrayIcon()
    {
        _notifyIcon = new Forms.NotifyIcon
        {
            Visible = true,
            Text = "AI划词助手"
        };

        // Try to load icon, use default if not found
        try
        {
            var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "tray_icon.ico");
            if (System.IO.File.Exists(iconPath))
            {
                _notifyIcon.Icon = new DrawingIcon(iconPath);
            }
            else
            {
                _notifyIcon.Icon = DrawingSystemIcons.Application;
            }
        }
        catch
        {
            _notifyIcon.Icon = SystemIcons.Application;
        }

        // Create context menu
        var contextMenu = new Forms.ContextMenuStrip();
        contextMenu.Items.Add("控制台", null, (s, e) => ShowSettingsWindow());
        contextMenu.Items.Add(new Forms.ToolStripSeparator());
        contextMenu.Items.Add("退出", null, (s, e) => ExitApplication());

        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    private void ShowSettingsWindow()
    {
        if (_settingsWindow == null)
        {
            _settingsWindow = new SettingsWindow(_configService, _aiService);
        }
        _settingsWindow.Show();
        _settingsWindow.Activate();
    }

    private void OnSelectionComplete(System.Windows.Point mousePosition)
    {
        _lastMousePosition = mousePosition;

        // Reset and start timer
        _selectionTimer?.Stop();
        _selectionTimer!.Interval = TimeSpan.FromSeconds(_configService.Config.WaitTimeSeconds);
        _selectionTimer.Start();
    }

    private async void SelectionTimer_Tick(object? sender, EventArgs e)
    {
        _selectionTimer?.Stop();

        // Temporarily disable mouse hook to avoid triggering on our own actions
        _mouseHookService.IsEnabled = false;

        try
        {
            var selectedText = await _clipboardService.GetSelectedTextAsync();

            if (!string.IsNullOrWhiteSpace(selectedText))
            {
                // Get enabled prompts
                var enabledPrompts = _configService.Config.Prompts
                    .Where(p => p.IsEnabled)
                    .ToList();

                if (enabledPrompts.Count > 0)
                {
                    // Combine all enabled prompts
                    var promptPrefix = string.Join("\n", enabledPrompts.Select(p => p.Content));
                    var fullPrompt = promptPrefix + selectedText;

                    // Show AI response window
                    await Dispatcher.InvokeAsync(() =>
                    {
                        var responseWindow = new AIResponseWindow(
                            _aiService,
                            _configService.Config.SelectedModel,
                            fullPrompt,
                            _lastMousePosition);
                        responseWindow.Show();
                    });
                }
            }
        }
        catch
        {
            // Ignore errors
        }
        finally
        {
            _mouseHookService.IsEnabled = true;
        }
    }

    private void ExitApplication()
    {
        _mouseHookService?.Dispose();
        _notifyIcon?.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mouseHookService?.Dispose();
        _notifyIcon?.Dispose();
        base.OnExit(e);
    }
}
