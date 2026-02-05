using System.Windows;
using System.Windows.Controls;
using AIWordHelper.Models;
using AIWordHelper.Services;

namespace AIWordHelper.Views;

public partial class SettingsWindow : Window
{
    private readonly ConfigService _configService;
    private readonly AIService _aiService;

    public SettingsWindow(ConfigService configService, AIService aiService)
    {
        InitializeComponent();
        _configService = configService;
        _aiService = aiService;
        LoadConfig();
    }

    private void LoadConfig()
    {
        var config = _configService.Config;
        TxtApiUrl.Text = config.ApiUrl;
        TxtApiKey.Password = config.ApiKey;
        SliderWaitTime.Value = config.WaitTimeSeconds;
        TxtWaitTime.Text = config.WaitTimeSeconds.ToString("F1");

        RefreshPromptList();

        if (!string.IsNullOrEmpty(config.SelectedModel))
        {
            CmbModels.Items.Add(config.SelectedModel);
            CmbModels.SelectedIndex = 0;
        }
    }

    private void RefreshPromptList()
    {
        LstPrompts.ItemsSource = null;
        LstPrompts.ItemsSource = _configService.Config.Prompts;
    }

    private async void BtnRefreshModels_Click(object sender, RoutedEventArgs e)
    {
        BtnRefreshModels.IsEnabled = false;
        BtnRefreshModels.Content = "加载中...";

        _aiService.ApiUrl = TxtApiUrl.Text;
        _aiService.ApiKey = TxtApiKey.Password;

        var models = await _aiService.GetModelsAsync();

        CmbModels.Items.Clear();
        foreach (var model in models)
        {
            CmbModels.Items.Add(model);
        }

        if (models.Count > 0)
        {
            var currentModel = _configService.Config.SelectedModel;
            var index = models.IndexOf(currentModel);
            CmbModels.SelectedIndex = index >= 0 ? index : 0;
        }

        BtnRefreshModels.Content = "刷新模型";
        BtnRefreshModels.IsEnabled = true;
    }

    private void SliderWaitTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtWaitTime != null)
        {
            TxtWaitTime.Text = SliderWaitTime.Value.ToString("F1");
        }
    }

    private void LstPrompts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LstPrompts.SelectedItem is PromptTemplate prompt)
        {
            TxtPromptName.Text = prompt.Name;
            TxtPromptContent.Text = prompt.Content;
        }
    }

    private void PromptCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        // Auto-save when checkbox changes
    }

    private void BtnNewPrompt_Click(object sender, RoutedEventArgs e)
    {
        var prompt = new PromptTemplate
        {
            Name = TxtPromptName.Text,
            Content = TxtPromptContent.Text,
            IsEnabled = false
        };
        _configService.Config.Prompts.Add(prompt);
        RefreshPromptList();
        LstPrompts.SelectedItem = prompt;
    }

    private void BtnUpdatePrompt_Click(object sender, RoutedEventArgs e)
    {
        if (LstPrompts.SelectedItem is PromptTemplate prompt)
        {
            prompt.Name = TxtPromptName.Text;
            prompt.Content = TxtPromptContent.Text;
            RefreshPromptList();
        }
    }

    private void BtnDeletePrompt_Click(object sender, RoutedEventArgs e)
    {
        if (LstPrompts.SelectedItem is PromptTemplate prompt)
        {
            _configService.Config.Prompts.Remove(prompt);
            RefreshPromptList();
            TxtPromptName.Text = "";
            TxtPromptContent.Text = "";
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var config = _configService.Config;
        config.ApiUrl = TxtApiUrl.Text;
        config.ApiKey = TxtApiKey.Password;
        config.SelectedModel = CmbModels.SelectedItem?.ToString() ?? "";
        config.WaitTimeSeconds = SliderWaitTime.Value;

        _configService.Save();

        _aiService.ApiUrl = config.ApiUrl;
        _aiService.ApiKey = config.ApiKey;

        MessageBox.Show("配置已保存", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}
