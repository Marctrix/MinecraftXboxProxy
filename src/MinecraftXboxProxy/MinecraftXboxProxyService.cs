using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

public class MinecraftXboxProxyService
{
    private readonly IPAddress _xBox;
    private readonly IPAddress _server;
    private readonly int _gamePort;

    readonly UdpListener _gameListener;
    readonly Dictionary<int, UdpListener> _clients = new Dictionary<int, UdpListener>();

    public MinecraftXboxProxyService(IPAddress xBox, IPAddress server, int gamePort)
    {
        _xBox = xBox;
        _server = server;
        _gamePort = gamePort;

        _gameListener = new UdpListener(new IPEndPoint(IPAddress.Any, gamePort));
        _clients.Add(gamePort, _gameListener);
    }


    public Task Start()
    {
        //start listening for messages and copy the messages back to the client
        return Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                var received = await _gameListener.Receive();

                if (received.Sender.Address.Equals(_xBox))
                {
                    if (received.Sender.Port != _gamePort)
                        StartListeningForwarder(received.Sender.Port);

                    await _clients[received.Sender.Port].Forward(received.Message, new IPEndPoint(_server, _gamePort));
                }

            }
        });
    }

    void StartListeningForwarder(int port)
    {
        if (_clients.ContainsKey(port))
            return;

        var udpClient = new UdpListener(new IPEndPoint(IPAddress.Any, port));

        _clients.Add(port, udpClient);

        Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                var singleMessage = await udpClient.Receive();
                await _gameListener.Forward(singleMessage.Message, new IPEndPoint(_xBox, port));
            }
        });
    }
}

