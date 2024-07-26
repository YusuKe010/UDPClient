using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;

public class UDPClient : MonoBehaviour
{
	[SerializeField] private string _serverIPAddress;

	private readonly int _port = 7000;

	public UDPClient(int port) => _port = port;
	private UdpClient _udpClient;


	private async void Start()
	{
		await SendServerAddressRequest("こんにちは");
	}

	public async Task<string> SendServerAddressRequest(object obj, int timeOutLimit = 3)
	{
		_udpClient = new UdpClient() { EnableBroadcast = true };
		
		var data = ObjectToByte(obj);
		await _udpClient.SendAsync(data, data.Length, new IPEndPoint(IPAddress.Broadcast, _port));
		Debug.Log("Waiting...");

		Task<UdpReceiveResult> receive = _udpClient.ReceiveAsync();
		if (await Task.WhenAny(receive, Task.Delay(timeOutLimit * 1000)) == receive)
		{
			var result = await receive;
			var receivedData = result.Buffer;
			string response = Encoding.UTF8.GetString(receivedData);
			Debug.Log("Server response: " + response);

			if (IPAddress.TryParse(response, out var ipAddress))
			{
				_serverIPAddress = ipAddress.ToString();
			}
		}
		else
		{
			Debug.Log($"TimeOut : ローカルリクエストに失敗しました");
		}

		_udpClient.Close();

		return _serverIPAddress;
	}


	byte[] ObjectToByte(object objects)
	{
		var json = JsonUtility.ToJson(objects);
		return Encoding.UTF8.GetBytes(json);
	}
}
