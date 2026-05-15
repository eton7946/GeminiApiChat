using System.Text;

namespace GeminiApiChat;

public sealed class MainForm : Form
{
    private readonly TextBox _apiKeyTextBox = new() { UseSystemPasswordChar = true, PlaceholderText = "API Key 會從 appsettings.json 載入" };
    private readonly ComboBox _modelComboBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Button _reloadModelsButton = new() { Text = "更新模型" };
    private readonly Button _saveConfigButton = new() { Text = "儲存設定" };
    private readonly Button _clearButton = new() { Text = "清除對話" };
    private readonly RichTextBox _chatWindow = new() { ReadOnly = true, Dock = DockStyle.Fill };
    private readonly TextBox _inputTextBox = new() { Multiline = true, Height = 80 };
    private readonly Button _sendButton = new() { Text = "送出", Width = 100 };
    private readonly GeminiClient _geminiClient = new(new HttpClient());
    private readonly List<ChatMessage> _history = [];
    private AppConfig _config = AppConfig.Load();

    public MainForm()
    {
        Text = "Gemini API Chat (WinForms)";
        Width = 1020;
        Height = 720;
        StartPosition = FormStartPosition.CenterScreen;

        _apiKeyTextBox.Text = _config.ApiKey;
        SetModelList(_config.PreferredModels);

        var topPanel = BuildTopPanel();
        var bottomPanel = BuildBottomPanel();

        Controls.Add(_chatWindow);
        Controls.Add(bottomPanel);
        Controls.Add(topPanel);

        _sendButton.Click += async (_, _) => await SendMessageAsync();
        _clearButton.Click += (_, _) => ClearConversation();
        _saveConfigButton.Click += (_, _) => SaveConfig();
        _reloadModelsButton.Click += async (_, _) => await ReloadModelsAsync();

        _inputTextBox.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                await SendMessageAsync();
            }
        };
    }

    private Panel BuildTopPanel()
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 95, Padding = new Padding(10) };

        var apiKeyLabel = new Label { Text = "API Key", AutoSize = true, Top = 14, Left = 10 };
        _apiKeyTextBox.SetBounds(80, 10, 650, 30);

        var modelLabel = new Label { Text = "Model", AutoSize = true, Top = 54, Left = 10 };
        _modelComboBox.SetBounds(80, 50, 300, 30);

        _reloadModelsButton.SetBounds(390, 50, 100, 30);
        _saveConfigButton.SetBounds(760, 10, 110, 30);
        _clearButton.SetBounds(880, 10, 110, 30);

        panel.Controls.Add(apiKeyLabel);
        panel.Controls.Add(_apiKeyTextBox);
        panel.Controls.Add(modelLabel);
        panel.Controls.Add(_modelComboBox);
        panel.Controls.Add(_reloadModelsButton);
        panel.Controls.Add(_saveConfigButton);
        panel.Controls.Add(_clearButton);
        return panel;
    }

    private Panel BuildBottomPanel()
    {
        var panel = new Panel { Dock = DockStyle.Bottom, Height = 140, Padding = new Padding(10) };

        _inputTextBox.SetBounds(10, 10, 850, 95);
        _sendButton.SetBounds(875, 35, 100, 40);

        panel.Controls.Add(_inputTextBox);
        panel.Controls.Add(_sendButton);
        return panel;
    }

    private void SaveConfig()
    {
        _config.ApiKey = _apiKeyTextBox.Text.Trim();
        _config.DefaultModel = _modelComboBox.SelectedItem?.ToString() ?? _config.DefaultModel;
        _config.PreferredModels = _modelComboBox.Items.Cast<string>().ToList();
        AppConfig.Save(_config);
        AppendChat("System", $"設定已寫入 {AppConfig.ConfigPath}");
    }

    private async Task ReloadModelsAsync()
    {
        try
        {
            ToggleInput(false);
            var models = await _geminiClient.ListModelsAsync(_apiKeyTextBox.Text.Trim(), CancellationToken.None);
            if (models.Count == 0)
            {
                AppendChat("System", "API 未回傳可用 Gemini 模型，保留原本清單。");
                return;
            }

            SetModelList(models);
            AppendChat("System", $"已更新模型清單，共 {models.Count} 個。");
        }
        catch (Exception ex)
        {
            AppendChat("System", $"更新模型失敗：{ex.Message}");
        }
        finally
        {
            ToggleInput(true);
        }
    }

    private void SetModelList(IEnumerable<string> models)
    {
        var list = models.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        _modelComboBox.Items.Clear();
        _modelComboBox.Items.AddRange(list.Cast<object>().ToArray());

        var pick = list.FirstOrDefault(m => m.Equals(_config.DefaultModel, StringComparison.OrdinalIgnoreCase))
            ?? list.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(pick))
        {
            _modelComboBox.SelectedItem = pick;
        }
    }

    private async Task SendMessageAsync()
    {
        var question = _inputTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(question)) return;

        ToggleInput(false);
        try
        {
            AppendChat("User", question);
            _history.Add(new ChatMessage { Role = "user", Text = question });
            _inputTextBox.Clear();

            var answer = await _geminiClient.SendChatAsync(_apiKeyTextBox.Text.Trim(), _modelComboBox.SelectedItem?.ToString() ?? string.Empty, _history, CancellationToken.None);

            AppendChat("Gemini", answer);
            _history.Add(new ChatMessage { Role = "model", Text = answer });
        }
        catch (Exception ex)
        {
            AppendChat("System", $"錯誤：{ex.Message}");
        }
        finally
        {
            ToggleInput(true);
            _inputTextBox.Focus();
        }
    }

    private void ClearConversation()
    {
        _history.Clear();
        _chatWindow.Clear();
    }

    private void ToggleInput(bool enabled)
    {
        _sendButton.Enabled = enabled;
        _inputTextBox.Enabled = enabled;
        _modelComboBox.Enabled = enabled;
        _reloadModelsButton.Enabled = enabled;
        _saveConfigButton.Enabled = enabled;
    }

    private void AppendChat(string role, string text)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[{DateTime.Now:HH:mm:ss}] {role}:");
        sb.AppendLine(text);
        sb.AppendLine(new string('-', 50));
        _chatWindow.AppendText(sb.ToString());
        _chatWindow.SelectionStart = _chatWindow.TextLength;
        _chatWindow.ScrollToCaret();
    }
}
