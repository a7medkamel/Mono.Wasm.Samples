using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebAssembly;
using ClientWebSocket = WebAssembly.Net.WebSockets.ClientWebSocket;

namespace WasmClientWebSocketTest {
	public class Client {
		static ArraySegment<byte> clientBuffer = WebSocket.CreateClientBuffer (100, 100);

		static JSObject messageArea;

		static void Main (JSObject output)
		{
			messageArea = output;
			Console.WriteLine ("WebSocket Client ready.");

		}

		static async void checkWebSocket (Uri server, string protocols = "")
		{
			if (cws == null) {
				await UpdateMessageArea ($"Create  WebSocket: {server}");

				cws = new ClientWebSocket ();

				if (!string.IsNullOrEmpty(protocols)) {
					//cws.Options.AddSubProtocol("dumb-increment-protocol");
					//cws.Options.AddSubProtocol("dumb-increment-protocol2");
					//cws.Options.AddSubProtocol ("echo-protocol");

					foreach (var p in protocols.Split(';')) {
						cws.Options.AddSubProtocol (p);
					}

				}

			}

		}

		static ClientWebSocket cws;
		static CancellationTokenSource _cancellation;

		public async Task<WebSocketState> OnConnectWebSocket (JSObject protocol, JSObject hostname, JSObject port, JSObject endpoint, JSObject protocols)
		{
			var connect = new TaskCompletionSource<WebSocketState> ();
			var ws_protocol = protocol.GetObjectProperty ("value").ToString ();
			var ws_hostname = hostname.GetObjectProperty ("value").ToString ();
			var ws_port = port.GetObjectProperty ("value").ToString () ?? string.Empty;
			var ws_endpoint = endpoint.GetObjectProperty ("value").ToString ();
			var ws_protocols = protocols.GetObjectProperty ("value").ToString ();

			if (!string.IsNullOrEmpty (ws_port))
				ws_port = $":{ws_port}";

			var webSocketURL = ws_protocol + "://" + ws_hostname + ws_port + ws_endpoint;
			try {
				var server = new Uri (webSocketURL);
				await UpdateMessageArea ($"Connecting WebSocket: {server}");
				await ConnectWebSocket (server, ws_protocols);
			}
			catch (Exception ue) {
				await UpdateMessageArea (ue.Message, true);
			}
			finally {
				await UpdateMessageArea ($"WebSocket State: {cws?.State}");
				connect.SetResult (cws?.State ?? WebSocketState.None);
			}

			return await connect.Task;
		}

		private async Task ConnectWebSocket (Uri server, string protocols)
		{
			_cancellation = new CancellationTokenSource ();

			checkWebSocket (server, protocols);


			using (var cts2 = new CancellationTokenSource (4000)) {

				try {
					Task taskConnect = cws.ConnectAsync (server, cts2.Token);
					await taskConnect;
				} catch (Exception exc) {
					await UpdateMessageArea ($"ConnectWebSocket connect exception: {cws.CloseStatus} - {cws.CloseStatusDescription} / {exc.Message}", true);
					await UpdateMessageArea ($"ConnectWebSocket connect inner exception: {cws.CloseStatus} - {cws.CloseStatusDescription} / {exc.InnerException?.Message}", true);
				}
			}

	    		if (cws.State == WebSocketState.Open)
				RunReceiveLoop ();

		}

		public async void CloseWebSocket (string closeReason = null)
		{

			try {
				Task taskClose = cws.CloseAsync (WebSocketCloseStatus.NormalClosure, closeReason, _cancellation.Token);
				await taskClose;
			} catch (Exception excC) {
				await UpdateMessageArea ($"CloseWebSocket Exception: {cws.CloseStatus} - {cws.CloseStatusDescription} / {excC.Message}", true);
			} finally {
				await UpdateMessageArea ($"CloseWebSocket: {cws.CloseStatus} - {cws.CloseStatusDescription}", true);
			}

		}


		public async void SendWebSocketMessage (JSObject htmlMessage, string type)
		{

			try {
				var message = htmlMessage.GetObjectProperty ("value").ToString ();
				var buffer = Encoding.UTF8.GetBytes (message);
				var msgType = (type == "binary") ? WebSocketMessageType.Binary : WebSocketMessageType.Text;
				await cws.SendAsync (new ArraySegment<byte> (buffer), msgType, true, _cancellation.Token);
			} catch (Exception excC) {
				await UpdateMessageArea ($"SendWebSocketMessage Exception: {cws.CloseStatus} - {cws.CloseStatusDescription} / {excC.Message}");
			}

		}

		private async void RunReceiveLoop ()
		{
			var buffer = new ArraySegment<byte> (new byte [4096]);

			while (!_cancellation?.IsCancellationRequested ?? true) {
				try {
					var r = await cws.ReceiveAsync (buffer, _cancellation.Token);
					if (r.MessageType == WebSocketMessageType.Close) {
						await UpdateMessageArea ($"Received {r.MessageType}: Close Status: [{cws.CloseStatus} Description: [{cws.CloseStatusDescription}]");
					} else {

						await UpdateMessageArea ($"Received {r.MessageType}: [{Encoding.UTF8.GetString (buffer.Array, buffer.Offset, r.Count)}]");
					}
				} catch (Exception e) {
					if (!_cancellation.IsCancellationRequested) {
						await UpdateMessageArea ($"Failed to receive: {e.Message}");
						break;
					}
				}
			}
		}

		private static async Task UpdateMessageArea (string message, bool mirrorOnConsole = false)
		{
			var update = new TaskCompletionSource<bool> ();
			messageArea.SetObjectProperty ("value", $"{DateTime.Now}: {message}\r\n" + messageArea.GetObjectProperty ("value"));
			if (mirrorOnConsole)
				Console.WriteLine (message);
			update.SetResult (true);
			await update.Task;
		}
	}
}
