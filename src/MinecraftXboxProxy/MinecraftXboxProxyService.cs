using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class MinecraftXboxProxyService
{
    private IPAddress _xBox;
    private IPAddress _server;
    private int _gamePort;

    UdpListener _gameListener;
    readonly Dictionary<int, UdpListener> _clients = new Dictionary<int, UdpListener>();

    private CancellationTokenSource _cancellationToken;
    private List<Task> runningTasks = new List<Task>();

    public void Start(IPAddress xBox, IPAddress server, int gamePort)
    {
        if (runningTasks.Any())
            throw new InvalidOperationException("Proxy Service already started. Please stop it first!");

        _xBox = xBox;
        _server = server;
        _gamePort = gamePort;

        _gameListener = new UdpListener(new IPEndPoint(IPAddress.Any, gamePort));
        _clients.Add(gamePort, _gameListener);

        _cancellationToken = new CancellationTokenSource();

        //start listening for messages and copy the messages back to the client
        var task = Task.Factory.StartNew(async () =>
        {
            while (!_cancellationToken.Token.IsCancellationRequested)
            {
                var received = await _gameListener.Receive();

                if (received.Sender.Address.Equals(_xBox))
                {
                    if (received.Sender.Port != _gamePort)
                        StartListeningForwarder(received.Sender.Port, _cancellationToken.Token);

                    await _clients[received.Sender.Port].Forward(received.Message, new IPEndPoint(_server, _gamePort));
                }
            }

            _clients.Remove(_gamePort);
        }, _cancellationToken.Token);

        runningTasks.Add(task);
    }

    void StartListeningForwarder(int port, CancellationToken token)
    {
        if (_clients.ContainsKey(port))
            return;

        var udpClient = new UdpListener(new IPEndPoint(IPAddress.Any, port));

        _clients.Add(port, udpClient);

        var task = Task.Factory.StartNew(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                var singleMessage = await udpClient.Receive();
                await _gameListener.Forward(singleMessage.Message, new IPEndPoint(_xBox, port));
            }
            CleanUpClientOnPort(port);
            
        }, token);

        runningTasks.Add(task);
    }

    private void CleanUpClientOnPort(int port)
    {
        if (!_clients.ContainsKey(port))
            return;

        var client = _clients[port];
        client.Dispose();

        _clients.Remove(port);
    }

    public void Stop() {
        _cancellationToken.Cancel();

        Task.WaitAll(runningTasks.ToArray());

        _cancellationToken.Dispose();
        runningTasks.Clear();
    }
}

