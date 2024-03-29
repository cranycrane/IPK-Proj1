using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace IPK_Proj1_Tests;

public class UdpServer
{
    private UdpClient udpClient;
    private bool isListening;

    public UdpServer(int listenPort)
    {
        udpClient = new UdpClient(listenPort);
        isListening = false;
    }

    public async IAsyncEnumerable<byte[]> StartListeningAsync(CancellationToken cancellationToken)
    {
        isListening = true;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                UdpReceiveResult receivedResults = await udpClient.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                byte[] receivedBytes = receivedResults.Buffer;
                yield return receivedBytes;
            }
        }
        finally
        {
            isListening = false;
            Stop(); // Zavoláno pro ukončení naslouchání a uvolnění zdrojů
        }
    }

    public void Stop()
    {
        if (isListening)
        {
            udpClient.Close(); // Ukončí naslouchání a uvolní socket
            udpClient.Dispose(); // Uvolní všechny zdroje spojené s UdpClient
            udpClient = null;
        }
    }
}
