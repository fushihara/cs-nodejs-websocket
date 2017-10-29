using System;
using System.Windows;

namespace websocket {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {
        private WebSocketManager wsm;
        public MainWindow() {
            InitializeComponent();
            this.wsm = new WebSocketManager("localhost", 12345);
            this.wsm.OnConnect += (client, request) => {
                this.addLog($"デバイスが接続しました");
            };
            this.wsm.OnMessage += (client, tag, message) => {
                this.addLog($"メッセージ受信:{tag}/{message}");
            };
            this.wsm.OnDisconnect += (client, error) => {
                this.addLog($"デバイス切断");
            };
            this.addLog("コンストラクタ");
            this.addLog(JsonTest.Serialize());
        }

        private void sendButtonCliek(object sender, RoutedEventArgs e) {
            String text = this.sendText.Text.Trim();
            this.sendText.Text = "";
            this.addLog($"送信ボタン押下[{text}]");
            this.wsm.sendAllClient("message", text);
        }
        private void logClearButtonCliek(object sender, RoutedEventArgs e) {
            this.logText.Text = "";
        }
        private void addLog(String message) {
            String time = DateTime.Now.ToString("HH:mm:ss.fff");
            this.logText.Text += $"{time} {message}\n";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            this.addLog("onload");
            this.wsm.exec();
        }
   }
}