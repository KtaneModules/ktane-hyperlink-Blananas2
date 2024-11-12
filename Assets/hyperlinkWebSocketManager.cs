using System.Threading;
using WebSocketSharp;

public class hyperlinkWebSocketManager
{
	private readonly hyperlinkScript _module;
	private readonly string _data;
	private WebSocket _ws;

	public hyperlinkWebSocketManager(hyperlinkScript module, string data)
	{
		_module = module;
		_data = data;
		StartThread();
	}

	private static readonly string WebSocketURL = "ws://hyperlink.eltrick.uk:2999";
	private bool shouldBeRunning = true;

	private void StartThread()
	{
		var t = new Thread(WebSocketThread)
		{
			IsBackground = true
		};
		t.Start();
	}

	private void WebSocketThread()
	{
		_ws = new WebSocket(WebSocketURL);

		using (_ws)
		{
			_ws.OnOpen += OnOpen;
			_ws.OnMessage += OnMessageReceived;
			_ws.OnError += OnError;
			_ws.OnClose += OnClose;
			_ws.Connect();
			while(shouldBeRunning)
				Thread.Sleep(100);
		}

		_ws = null;
	}

	public void Stop()
	{
		shouldBeRunning = false;
	}

	private void Send(string data)
	{
		_ws.Send(data);
	}

	private void OnOpen(object sender, object e)
	{
		Send(_data);
	}

	private void OnMessageReceived(object sender, MessageEventArgs e)
	{
		var message = e.Data.Split('|');
		if (message.Length > 1)
		{
			if (message[0] == "Connection")
			{
				switch (message[1])
				{
					case "Ready":
						UnityMainThreadDispatcher.Instance().Enqueue(_module.ConnectionReady());
						break;
					case "Error":
						UnityMainThreadDispatcher.Instance().Enqueue(_module.ConnectionError());
						break;
					case "Ping":
						Send("Connection|Pong");
						break;
				}
			}
		}
			
	}

	private void ConnectionLost()
	{
		UnityMainThreadDispatcher.Instance().Enqueue(_module.ConnectionLost());
	}

	private void OnError(object sender, ErrorEventArgs e)
	{
		if(!_module.moduleSolved) 
			ConnectionLost();
	}

	private void OnClose(object sender, CloseEventArgs e)
	{
		if(!_module.moduleSolved) 
			ConnectionLost();
	}

	public void Solve()
	{
		Send("Connection|Closesocket");
	}
}
