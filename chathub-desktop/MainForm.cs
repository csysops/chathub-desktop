using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chathub_desktop
{
    public partial class MainForm : Form
    {
        private const string SERVER_URL = "http://datpt.somee.com/chat";

        public HubConnection Connection { get; private set; }

        public event Action<string, string> PrivateMessageReceived;

        public MainForm()
        {
            InitializeComponent();
            ShowForm(new LoginForm(this));
        }

        // ---------------- Navigation ----------------
        public void ShowForm(Form childForm)
        {
            foreach (Control c in Controls)
            {
                if (c is Form)
                {
                    ((Form)c).Close();
                    Controls.Remove(c);
                }
            }

            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            Controls.Add(childForm);
            childForm.Show();
        }

        // ---------------- Connection ----------------
        public async Task<bool> EnsureConnection()
        {
            if (Connection != null &&
                Connection.State == HubConnectionState.Connected)
                return true;

            Connection = new HubConnectionBuilder()
                .WithUrl(SERVER_URL)
                .WithAutomaticReconnect()
                .Build();

            Connection.On<string, string>(
                "ReceivePrivate",
                OnReceivePrivate
            );

            try
            {
                await Connection.StartAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message);
                return false;
            }
        }

        private void OnReceivePrivate(string fromUser, string message)
        {
            if (PrivateMessageReceived != null)
                PrivateMessageReceived(fromUser, message);
        }

        // ---------------- Hub API Wrappers ----------------
        public Task<bool> Login(string user, string pass)
        {
            return Connection.InvokeAsync<bool>("LoginUser", user, pass);
        }

        public Task<bool> Register(string user, string pass)
        {
            return Connection.InvokeAsync<bool>("RegisterUser", user, pass);
        }

        public Task<List<string>> GetAllUsers()
        {
            return Connection.InvokeAsync<List<string>>("GetAllUsers");
        }

        public Task SendPrivate(string from, string to, string message)
        {
            return Connection.InvokeAsync("SendPrivate", from, to, message);
        }

        public Task<List<ChatMessageDto>> LoadHistory(string a, string b)
        {
            return Connection.InvokeAsync<List<ChatMessageDto>>(
                "LoadHistory", a, b);
        }
    }

    public class ChatMessageDto
    {
        public string Sender { get; set; }
        public string Message { get; set; }
        public string Timestamp { get; set; }

        public ChatMessageDto()
        {
            Sender = "";
            Message = "";
            Timestamp = "";
        }
    }
}

