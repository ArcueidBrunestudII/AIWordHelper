using System.Windows;
using System.Windows.Input;
using AIWordHelper.Services;

namespace AIWordHelper.Views;

public partial class AIResponseWindow : Window
{
    private readonly AIService _aiService;
    private readonly string _model;
    private readonly string _prompt;
    private CancellationTokenSource? _cts;

    public AIResponseWindow(AIService aiService, string model, string prompt, Point position)
    {
        InitializeComponent();
        _aiService = aiService;
        _model = model;
        _prompt = prompt;

        // Position window near mouse
        Left = position.X + 10;
        Top = position.Y + 10;

        // Ensure window stays on screen
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;

        if (Left + 500 > screenWidth)
            Left = screenWidth - 510;
        if (Top + 400 > screenHeight)
            Top = position.Y - 410;

        Loaded += AIResponseWindow_Loaded;
    }

    private async void AIResponseWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _cts = new CancellationTokenSource();
        TxtStatus.Text = "生成中...";
        TxtContent.Text = "";

        try
        {
            await foreach (var chunk in _aiService.StreamChatAsync(_model, _prompt))
            {
                if (_cts.Token.IsCancellationRequested) break;
                TxtContent.Text += chunk;
            }
            TxtStatus.Text = "完成";
        }
        catch (Exception ex)
        {
            TxtContent.Text = $"错误: {ex.Message}";
            TxtStatus.Text = "失败";
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            DragMove();
        }
    }

    private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        _cts?.Cancel();
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        base.OnClosed(e);
    }
}
