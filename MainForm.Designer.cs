// Copyright (c) 2026 SecurMe — EDR detection testing ONLY.

namespace SecurMe.Tools.KeyloggerSim;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private Label lblStatus;
    private Label lblTarget;
    private Label lblKeysLogged;
    private TextBox txtPreview;
    private Button btnConnect;
    private Button btnDisconnect;
    private TextBox txtServerIp;
    private TextBox txtServerPort;
    private Label lblIp;
    private Label lblPort;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();

        // ── Labels ──
        lblIp = new Label { Text = "C2 IP:", Location = new Point(12, 17), AutoSize = true };
        lblPort = new Label { Text = "Port:", Location = new Point(220, 17), AutoSize = true };

        txtServerIp = new TextBox
        {
            Text = "127.0.0.1",
            Location = new Point(60, 14),
            Size = new Size(150, 23)
        };

        txtServerPort = new TextBox
        {
            Text = "12345",
            Location = new Point(260, 14),
            Size = new Size(60, 23)
        };

        btnConnect = new Button
        {
            Text = "Connect",
            Location = new Point(340, 12),
            Size = new Size(90, 27)
        };
        btnConnect.Click += BtnConnect_Click;

        btnDisconnect = new Button
        {
            Text = "Disconnect",
            Location = new Point(440, 12),
            Size = new Size(90, 27),
            Enabled = false
        };
        btnDisconnect.Click += BtnDisconnect_Click;

        lblStatus = new Label
        {
            Text = "Status: Disconnected",
            Location = new Point(12, 50),
            AutoSize = true,
            ForeColor = Color.Red,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        lblTarget = new Label
        {
            Text = "Target: —",
            Location = new Point(220, 50),
            AutoSize = true
        };

        lblKeysLogged = new Label
        {
            Text = "Keys sent: 0",
            Location = new Point(420, 50),
            AutoSize = true
        };

        txtPreview = new TextBox
        {
            Location = new Point(12, 78),
            Size = new Size(560, 240),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 10)
        };

        // ── Form ──
        this.Text = "SecurMe Keylogger Simulator — EDR Test Tool";
        this.ClientSize = new Size(584, 330);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.KeyPreview = true;

        this.Controls.AddRange(new Control[]
        {
            lblIp, txtServerIp, lblPort, txtServerPort,
            btnConnect, btnDisconnect,
            lblStatus, lblTarget, lblKeysLogged,
            txtPreview
        });

        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
