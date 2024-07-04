using System;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public partial class MainForm : Form
{
    private TcpClient client;
    private NetworkStream stream;
    private const string serverIp = "127.0.0.1"; // Replace with your server's IP address
    private const int serverPort = 12345; // Replace with your server's port

    public MainForm()
    {
        InitializeComponent();
        KeyPreview = true;
        KeyDown += MainForm_KeyDown;
    }

    private async void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        char keyPressed = (char)e.KeyValue;
        Console.WriteLine("Key pressed: " + keyPressed);

        // Send the pressed key to the Python server
        await SendKeyToServerAsync(keyPressed.ToString());
    }

    private async Task SendKeyToServerAsync(string key)
    {
        try
        {
            if (client == null || !client.Connected)
            {
                // Create a TCP client and connect to the Python server
                client = new TcpClient();
                await client.ConnectAsync(serverIp, serverPort);
                stream = client.GetStream();
            }

            // Encode the key as bytes and send it to the server
            byte[] data = Encoding.ASCII.GetBytes(key);
            await stream.WriteAsync(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending key to server: " + ex.Message);
            client?.Close();
            client = null;
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        stream?.Close();
        client?.Close();
        base.OnFormClosed(e);
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}
