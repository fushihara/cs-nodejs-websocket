const WebSocketClient = require('websocket').client;
const client = new WebSocketClient();
const host = "localhost";
const port = 12345;
client.on('connectFailed', (error) => {
    console.log('接続失敗: ' + error.toString());
});

client.on('connect',(connection) => {
  connection.on('error', function(error) {
      console.log("エラー発生: " + error.toString());
  });
  connection.on('close', function() {
      console.log('接続終了');
  });
  connection.on('message', function(message) {
      if (message.type === 'utf8') {
          console.log(`テキストデータ受信: '${message.utf8Data}'`);
      }
  });
  
  function sendNumber() {
      if (connection.connected) {
          var number = Math.round(Math.random() * 1000 ) + 10000;
          let sendMessage = "data "+(number+"")+"end ";
          connection.sendUTF(sendMessage);
          setTimeout(sendNumber, 1000);
      }
  }
  sendNumber();
});
client.connect(`ws://${host}:${port}`);
