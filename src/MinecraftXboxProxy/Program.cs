using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public struct Received
{
    public IPEndPoint Sender;
    public byte[] Message;
}

abstract class UdpBase
{
    protected UdpClient Client;

    protected UdpBase()
    {
        Client = new UdpClient();
    }

    public async Task<Received> Receive()
    {
        var result = await Client.ReceiveAsync();
        return new Received
        {
            Message = result.Buffer,
            Sender = result.RemoteEndPoint
        };        
    }
}

//Server
class UdpListener : UdpBase
{
    private IPEndPoint _listenOn;

    public UdpListener() : this(new IPEndPoint(IPAddress.Any, 19132))
    {
    }

    public UdpListener(IPEndPoint endpoint)
    {
        _listenOn = endpoint;
        Client = new UdpClient(_listenOn);        
    }

    public void Forward(byte[] message, IPEndPoint endpoint)
    {
        Client.Send(message, message.Length, endpoint);
    }

}

//Client
class UdpUser : UdpBase
{
    private UdpUser() { }

    public static UdpUser ConnectTo(string hostname, int port)
    {
        var connection = new UdpUser();
        connection.Client.Connect(hostname, port);
        return connection;
    }

    public void Send(string message)
    {
        var datagram = Encoding.ASCII.GetBytes(message);
        Client.Send(datagram, datagram.Length);
    }

}

class Program
{
    const int gamePort = 19132;

    static IPAddress xBoxIp = IPAddress.Parse("192.168.1.196");
    static IPAddress serverIp = IPAddress.Parse("187.78.541.454");

    static UdpListener gameEndpoint = new UdpListener(new IPEndPoint(IPAddress.Any, gamePort));

    static void Main(string[] args)
    {
        if (args.Length >= 1)
            xBoxIp = IPAddress.Parse(args[0]);

        if (args.Length >= 2)
            xBoxIp = IPAddress.Parse(args[0]);
                
        var minecraft = new IPEndPoint(serverIp, gamePort);
        clients.Add(gamePort, gameEndpoint);

        //start listening for messages and copy the messages back to the client
        Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                var received = await gameEndpoint.Receive();

                if (received.Sender.Address.Equals(xBoxIp))
                {
                    if (received.Sender.Port != gamePort)
                        StartListeningForwarder(received.Sender.Port);

                    clients[received.Sender.Port].Forward(received.Message, minecraft);
                }

            }
        });

        //create a new client
        //type ahead :-)
        string read;
        do
        {
            read = Console.ReadLine();
        } while (read != "quit");
    }

    static Dictionary<int, UdpListener> clients = new Dictionary<int, UdpListener>();

    static void StartListeningForwarder(int port)
    {
        if (clients.ContainsKey(port))
            return;
            
        var udpClient = new UdpListener(new IPEndPoint(IPAddress.Any, port));

        clients.Add(port, udpClient);

        Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                var singleMessage = await udpClient.Receive();
                gameEndpoint.Forward(singleMessage.Message, new IPEndPoint(xBoxIp, port));
            }
        });        
    }
}

