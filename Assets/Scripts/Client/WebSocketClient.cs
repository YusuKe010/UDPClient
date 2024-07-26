using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WebSocketClient : MonoBehaviour
{
    ClientWebSocket ws = new();
    
    private async void Start()
    {
        await GetWeb();
    }

    async Task GetWeb()
    {
        var uri = new Uri("ws://10.40.15.121:8000/ws/");

        await ws.ConnectAsync(uri, CancellationToken.None);
        var buffer = new byte[1024];

        while (true)
        {
            //所得情報確保用の配列を準備
            var segment = new ArraySegment<byte>(buffer);

            //サーバからのレスポンス情報を取得
            var result = await ws.ReceiveAsync(segment, CancellationToken.None);

            //エンドポイントCloseの場合、処理を中断
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK",
                    CancellationToken.None);
                return;
            }

            //バイナリの場合は、当処理では扱えないため、処理を中断
            if (result.MessageType == WebSocketMessageType.Binary)
            {
                await ws.CloseAsync(WebSocketCloseStatus.InvalidMessageType,
                    "I don't do binary", CancellationToken.None);
                return;
            }

            //メッセージの最後まで取得
            int count = result.Count;
            while (!result.EndOfMessage)
            {
                if (count >= buffer.Length)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.InvalidPayloadData,
                        "That's too long", CancellationToken.None);
                    return;
                }
                segment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                result = await ws.ReceiveAsync(segment, CancellationToken.None);

                count += result.Count;
            }

            //メッセージを取得
            var message = Encoding.UTF8.GetString(buffer, 0, count);
            Debug.Log(message);
        }
    }

    private void OnDestroy()
    {
    }
}
