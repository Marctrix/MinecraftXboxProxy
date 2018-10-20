using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
//Server
class UdpListener
{
    protected UdpClient Client;

    public UdpListener() : this(new IPEndPoint(IPAddress.Any, 19132))
    {
    }

    public UdpListener(IPEndPoint endpoint)
    {        
        Client = new UdpClient(endpoint);        
    }

    public async Task Forward(byte[] message, IPEndPoint endpoint)
    {
        await Client.SendAsync(message, message.Length, endpoint);
    }

    public async Task<UdpPackage> Receive()
    {
        var result = await Client.ReceiveAsync();
        return new UdpPackage
        {
            Message = result.Buffer,
            Sender = result.RemoteEndPoint
        };
    }
}

