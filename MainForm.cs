using System.Text;

namespace GeminiApiChat;

public sealed class MainForm : Form
{
    private readonly TextBox _apiKeyTextBox = new() { UseSystemPasswordChar = true, PlaceholderText = "貼上 Gemini API Key" };
    private readonly ComboBox _modelComboBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Button _clearButton = new() { Text = "清除對話" };
    private readonly RichTextBox _chatWindow = new() { ReadOnly = true, Dock = DockStyle.Fill };
    private readonly TextBox _inputTextBox = new() { Multiline = true, Height = 80 };
    private readonly Button _sendButton = new() { Text = "送出", Width = 100 };
    private readonly GeminiClient _geminiClient = new(new HttpClient());
    private readonly List<ChatMessage> _history = [];

    public MainForm()
    {
        Text = "Gemini API Chat (WinForms)";
        Width = 980;
        Height = 720;
        StartPosition = FormStartPosition.CenterScreen;

        _modelComboBox.Items.AddRange([
            "gemini-1.5-flash",
            "gemini-1.5-pro",
            "gemini-2.0-flash"
        ]);
        _modelComboBox.SelectedIndex = 0;

        var topPanel = BuildTopPanel();
        var bottomPanel = BuildBottomPanel();

        Controls.Add(_chatWindow);
        Controls.Add(bottomPanel);
        Controls.Add(topPanel);

        _sendButton.Click += async (_, _) => await SendMessageAsync();
        _clearButton.Click += (_, _) => ClearConversation();

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
        var panel = new Panel { Dock = DockStyle.Top, Height = 90, Padding = new Padding(10) };

        var apiKeyLabel = new Label { Text = "API Key", AutoSize = true, Top = 14, Left = 10 };
        _apiKeyTextBox.SetBounds(80, 10, 650, 30);

        var modelLabel = new Label { Text = "Model", AutoSize = true, Top = 54, Left = 10 };
        _modelComboBox.SetBounds(80, 50, 250, 30);

        _clearButton.SetBounds(760, 25, 160, 35);

        panel.Controls.Add(apiKeyLabel);
        panel.Controls.Add(_apiKeyTextBox);
        panel.Controls.Add(modelLabel);
        panel.Controls.Add(_modelComboBox);
        panel.Controls.Add(_clearButton);
        return panel;
    }

    private Panel BuildBottomPanel()
    {
        var panel = new Panel { Dock = DockStyle.Bottom, Height = 140, Padding = new Padding(10) };

        _inputTextBox.SetBounds(10, 10, 820, 95);
        _sendButton.SetBounds(845, 35, 100, 40);

        panel.Controls.Add(_inputTextBox);
        panel.Controls.Add(_sendButton);
        return panel;
    }

    private async Task SendMessageAsync()
    {
        var question = _inputTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(question))
        {
            return;
        }

        ToggleInput(false);
        try
        {
            AppendChat("User", question);
            _history.Add(new ChatMessage { Role = "user", Text = question });
            _inputTextBox.Clear();

            var answer = await _geminiClient.SendChatAsync(
                _apiKeyTextBox.Text.Trim(),
                _modelComboBox.SelectedItem?.ToString() ?? string.Empty,
                _history,
                CancellationToken.None);

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
