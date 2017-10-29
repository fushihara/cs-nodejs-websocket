using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
// http://kimux.net/?p=956
// https://stackoverflow.com/questions/41911072/choosing-a-buffer-size-for-a-websocket-response

namespace websocket {
    class WebSocketManager {
        public delegate void LogDelegate(String message);
        public delegate void MessageReceiveDelegate(WebSocket client, String tag, String message);
        public delegate void ConnectDelegate(WebSocket client, HttpListenerRequest httpRequest);
        public delegate void DisconnectDelegate(WebSocket client, Exception err);
        public event LogDelegate OnLog;
        public event MessageReceiveDelegate OnMessage;
        public event ConnectDelegate OnConnect;
        public event DisconnectDelegate OnDisconnect;
        private String host;
        private int port;
        private HttpListener httpListener;
        private List<WebSocket> clients = new List<WebSocket>();
        public WebSocketManager(String host, int port) {
            this.host = host;
            this.port = port;
        }
        public async void exec() {
            this.httpListener = new HttpListener();
            this.httpListener.Prefixes.Add($"http://{host}:{port}/");
            this.httpListener.Start();
            while (true) {
                /// 接続待機
                var listenerContext = await httpListener.GetContextAsync();
                if (listenerContext.Request.IsWebSocketRequest) {
                    /// httpのハンドシェイクがWebSocketならWebSocket接続開始
                    ProcessRequest(listenerContext);
                } else {
                    /// httpレスポンスを返す
                    listenerContext.Response.StatusCode = 400;
                    listenerContext.Response.Close();
                }
            }

        }
        public void sendAllClient(String tag, String message) {
            String sendMessage = $"{tag}:{message}";
            byte[] sendByte = Encoding.UTF8.GetBytes(sendMessage);
            Parallel.ForEach(this.clients,
                p => p.SendAsync(new ArraySegment<byte>(sendByte),
                WebSocketMessageType.Text,
                true,
                System.Threading.CancellationToken.None));
        }
        private void log(String message) {
            OnLog?.Invoke(message);
        }
        /// <summary>
        /// WebSocket接続毎の処理
        /// </summary>
        /// <param name="listenerContext"></param>
        private async void ProcessRequest(HttpListenerContext listenerContext) {
            this.log($"New Session:{listenerContext.Request.RemoteEndPoint.Address.ToString()}");

            /// WebSocketの接続完了を待機してWebSocketオブジェクトを取得する
            var ws = (await listenerContext.AcceptWebSocketAsync(subProtocol: null)).WebSocket;
            OnConnect?.Invoke(ws, listenerContext.Request);
            this.clients.Add(ws);
            while (ws.State == WebSocketState.Open) {
                try {
                    byte[] buffer = new byte[20];
                    int offset = 0;
                    int free = buffer.Length;
                    WebSocketReceiveResult receive;
                    while (true) {
                        receive = await ws.ReceiveAsync(new ArraySegment<byte>(buffer, offset, free), System.Threading.CancellationToken.None);
                        offset += receive.Count;
                        free -= receive.Count;
                        if (receive.EndOfMessage) { break; }
                        if (free == 0) {
                            var newSize = buffer.Length + 4;
                            var newBuffer = new byte[newSize];
                            Array.Copy(buffer, 0, newBuffer, 0, offset);
                            buffer = newBuffer;
                            free = buffer.Length - offset;
                        }
                    }
                    byte[] result = new byte[offset];
                    Buffer.BlockCopy(buffer, 0, result, 0, offset);
                    /// テキスト
                    if (receive.MessageType == WebSocketMessageType.Text) {
                        String message = Encoding.UTF8.GetString(result.ToArray());
                        this.log($"String Received:{listenerContext.Request.RemoteEndPoint.Address.ToString()}");
                        this.log($"Message={message}");
                        String tag = "";
                        String body = "";
                        if (message.Contains(":")) {
                            int tagIndex = message.IndexOf(':');
                            tag = message.Substring(0, tagIndex);
                            body = message.Substring(tagIndex);
                        } else {
                            tag = "";
                            body = message;
                        }
                        OnMessage?.Invoke(ws, tag, body);
                    } else if (receive.MessageType == WebSocketMessageType.Close) {
                        OnDisconnect?.Invoke(ws, null);
                        this.log($"Session Close:{listenerContext.Request.RemoteEndPoint.Address.ToString()}");
                        break;
                    }
                } catch (Exception e) {
                    /// 例外 クライアントが異常終了しやがった
                    OnDisconnect?.Invoke(ws, e);
                    this.log($"Session Abort:{listenerContext.Request.RemoteEndPoint.Address.ToString()}");
                    break;
                }
            }
            /// クライアントを除外する
            this.clients.Remove(ws);
            ws.Dispose();

        }
    }
}
