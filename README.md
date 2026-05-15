# GeminiApiChat (WinForms)

這個專案提供一個 **Windows Forms 桌面聊天工具**，可直接設定 Gemini API Key 並與 Gemini 對話，同時支援 **切換 Model**。

## 功能
- API Key 儲存在 `appsettings.json`，不必每次貼上。
- 內建較新的 Gemini 預設模型：
  - `gemini-2.5-flash`
  - `gemini-2.5-pro`
  - `gemini-2.5-flash-lite`
  - `gemini-2.0-flash`
- 可按「更新模型」直接向 Gemini API 抓取帳號當下可用模型清單。
- 保留多輪對話上下文。
- Enter 送出訊息（Shift+Enter 換行）。
- 一鍵清除對話。

## 如何執行
1. 安裝 .NET 8 SDK（Windows）。
2. 在專案目錄執行：
   ```bash
   dotnet run
   ```
3. 首次執行會自動建立 `appsettings.json`。
4. 將 API Key 填入後按「儲存設定」。
5. 可按「更新模型」同步可用 model，再開始聊天。

## 架構說明
- `MainForm.cs`：UI 與對話流程控制。
- `GeminiClient.cs`：封裝 Gemini API 呼叫與模型清單查詢。
- `AppConfig.cs`：設定檔讀寫。
- `Program.cs`：WinForms 進入點。
