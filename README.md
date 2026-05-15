# GeminiApiChat (WinForms)

這個專案提供一個 **Windows Forms 桌面聊天工具**，可直接設定 Gemini API Key 並與 Gemini 對話，同時支援 **切換 Model**。

## 功能
- 直接輸入 Gemini API Key。
- 下拉選單切換模型（預設提供 `gemini-1.5-flash`、`gemini-1.5-pro`、`gemini-2.0-flash`）。
- 保留多輪對話上下文。
- Enter 送出訊息（Shift+Enter 換行）。
- 一鍵清除對話。

## 如何執行
1. 安裝 .NET 8 SDK（Windows）。
2. 在專案目錄執行：
   ```bash
   dotnet run
   ```
3. 貼上 Gemini API Key。
4. 選擇想使用的 Model。
5. 輸入訊息開始聊天。

## 架構說明
- `MainForm.cs`：UI 與對話流程控制。
- `GeminiClient.cs`：封裝 Gemini API 呼叫。
- `Program.cs`：WinForms 進入點。

## 注意事項
- API Key 僅暫存於執行中的程式記憶體，未做本地持久化。
- 若模型名稱失效，可直接在 `_modelComboBox` 新增可用 model 字串。
