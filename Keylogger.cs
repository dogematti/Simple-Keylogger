using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm : Form
{
    private TcpClient _client;
    private NetworkStream _stream;
    private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

    private const string ServerIp = "127.0.0.1";
    private const int ServerPort = 12345;

    public MainForm()
    {
        InitializeComponent();

        // KeyPress gives you the actual character (respects Shift, etc.)
        KeyPreview = true;
        this.KeyPress += MainForm_KeyPress;

        // Optional: if you want arrow keys / function keys too, keep KeyDown:
        this.KeyDown += MainForm_KeyDown;
    }

    private async void MainForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        // e.KeyChar is the real character typed
        string msg = e.KeyChar.ToString();

        // Add newline so the server can read per-key as a line
        await SendToServerAsync(msg + "\n");
    }

    private async void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        // KeyPress won't fire for these; send them as tokens
        // (Python side can parse strings like "<LEFT>")
        string token = e.KeyCode switch
        {
            Keys.Left => "<LEFT>\n",
            Keys.Right => "<RIGHT>\n",
            Keys.Up => "<UP>\n",
            Keys.Down => "<DOWN>\n",
            Keys.Escape => "<ESC>\n",
            Keys.Enter => "<ENTER>\n",
            Keys.Back => "<BACKSPACE>\n",
            Keys.Tab => "<TAB>\n",
            _ => null
        };

        if (token != null)
        {
            e.SuppressKeyPress = true; // avoids ding/beep in some controls
            await SendToServerAsync(token);
        }
    }

    private async Task EnsureConnectedAsync()
    {
        if (_client != null && _client.Connected && _stream != null)
            return;

        CleanupConnection();

        _client = new TcpClient();
        await _client.ConnectAsync(ServerIp, ServerPort);
        _stream = _client.GetStream();
    }

    private async Task SendToServerAsync(string message)
    {
        await _sendLock.WaitAsync();
        try
        {
            await EnsureConnectedAsync();

            byte[] data = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(data, 0, data.Length);
            await _stream.FlushAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Send error: " + ex.Message);
            CleanupConnection();
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private void CleanupConnection()
    {
        try { _stream?.Close(); } catch { }
        try { _client?.Close(); } catch { }
        _stream = null;
        _client = null;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        CleanupConnection();
        _sendLock.Dispose();
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
