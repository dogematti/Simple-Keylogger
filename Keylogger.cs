using System;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Text;

public partial class MainForm : Form
{
    private TcpClient client;
    private NetworkStream stream;

    public MainForm()
    {
        InitializeComponent();
        KeyPreview = true;
        KeyDown += MainForm_KeyDown;
    }

    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        char keyPressed = (char)e.KeyValue;
        Console.WriteLine("Key pressed: " + keyPressed);

        // Send the pressed key to the Python server
        SendKeyToServer(keyPressed.ToString());
    }

    private void SendKeyToServer(string key)
    {
        try
        {
            if (client == null)
            {
                // Create a TCP client and connect to the Python server
                client = new TcpClient("127.0.0.1", 12345); // Replace with your server's IP and port
                stream = client.GetStream();
            }

            // Encode the key as bytes and send it to the server
            byte[] data = Encoding.ASCII.GetBytes(key);
            stream.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending key to server: " + ex.Message);
        }
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}
