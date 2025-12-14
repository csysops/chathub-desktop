using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chathub_desktop
{

    public partial class ChatForm : Form
    {
        private const string SERVER_URL = "http://datpt.somee.com/chat";

        private readonly MainForm mainForm;
        private readonly string currentUser;

        private HubConnection connection;
        private string selectedPeer = "";

        public ChatForm(MainForm parentForm, string username)
        {
            InitializeComponent();
            mainForm = parentForm;
            currentUser = username;

            listBox1.DrawMode = DrawMode.OwnerDrawFixed;
            listBox1.ItemHeight = 36;
            listBox1.DrawItem += listBox1_DrawItem;

            label2.Text = "Logged as: " + currentUser;

            Load += ChatForm_Load;
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            string text = listBox1.Items[e.Index].ToString();
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool isNew = text.StartsWith("(new)");

            // Background
            Color bgColor = isSelected
                ? Color.FromArgb(220, 235, 252)
                : Color.White;

            using (SolidBrush bgBrush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            // New-message indicator (small dot)
            if (isNew)
            {
                using (SolidBrush indicatorBrush = new SolidBrush(Color.DodgerBlue))
                {
                    e.Graphics.FillRectangle(
                        indicatorBrush,
                        e.Bounds.Left + 4,
                        e.Bounds.Top + 10,
                        6,
                        6
                    );
                }
            }

            // Text
            FontStyle style = isNew ? FontStyle.Bold : FontStyle.Regular;
            using (Font font = new Font("Segoe UI", 10, style))
            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                Rectangle textRect = new Rectangle(
                    e.Bounds.Left + 18,
                    e.Bounds.Top + 8,
                    e.Bounds.Width - 20,
                    e.Bounds.Height - 8
                );

                e.Graphics.DrawString(
                    text.Replace("(new) ", ""),
                    font,
                    textBrush,
                    textRect
                );
            }

            e.DrawFocusRectangle();
        }



        // ---------------- Form Load ----------------
        private async void ChatForm_Load(object sender, EventArgs e)
        {
            if (!await EnsureConnection())
                return;

            await LoadUsers();
        }

        // ---------------- SignalR ----------------
        private async Task<bool> EnsureConnection()
        {
            if (connection != null &&
                connection.State == HubConnectionState.Connected)
                return true;

            connection = new HubConnectionBuilder()
                .WithUrl(SERVER_URL) // shared config
                .WithAutomaticReconnect()
                .Build();

            connection.On<string, string>("ReceivePrivate", (fromUser, message) =>
            {
                BeginInvoke(new MethodInvoker(() =>
                {
                    if (fromUser == selectedPeer)
                    {
                        AppendMessage(fromUser, message);
                    }
                    else
                    {
                        MarkUserAsNew(fromUser);
                    }
                }));
            });

            try
            {
                await connection.StartAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}");
                return false;
            }
        }

        // ---------------- Load Users ----------------
        private async Task LoadUsers()
        {
            try
            {
                var users = await connection
                    .InvokeAsync<List<string>>("GetAllUsers");

                listBox1.Items.Clear();
                foreach (var u in users.Where(u => u != currentUser))
                    listBox1.Items.Add(u);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load users: {ex.Message}");
            }
        }

        // ---------------- Select User ----------------
        private async void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null) return;

            selectedPeer = listBox1.SelectedItem.ToString()
                .Replace("(new) ", "");

            richTextBox1.Clear();

            try
            {
                var history = await connection
                    .InvokeAsync<List<ChatMessageDto>>(
                        "LoadHistory", currentUser, selectedPeer);

                foreach (var m in history)
                    AppendMessage(m.Sender, m.Message, m.Timestamp);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load history: {ex.Message}");
            }
        }

        // ---------------- Send Message ----------------
        private async void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedPeer))
                return;

            string msg = textBox1.Text.Trim();
            if (msg.Length == 0) return;

            try
            {
                await connection
                    .InvokeAsync("SendPrivate",
                        currentUser, selectedPeer, msg);

                AppendMessage(currentUser, msg);
                textBox1.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Send failed: {ex.Message}");
            }
        }

        // ---------------- Helpers ----------------
        private void AppendMessage(string sender, string message, string ts = null)
        {
            ts = DateTime.UtcNow.ToString("HH:mm");

            bool isMe = sender == currentUser;

            // Move caret to end
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.SelectionLength = 0;

            // Alignment
            richTextBox1.SelectionAlignment =
                isMe ? HorizontalAlignment.Right : HorizontalAlignment.Left;

            // Sender line
            richTextBox1.SelectionFont = new Font("Segoe UI", 9, FontStyle.Bold);
            richTextBox1.AppendText(sender + "\n");

            // Message line
            richTextBox1.SelectionFont = new Font("Segoe UI", 10);
            richTextBox1.AppendText(message + "\n");

            // Timestamp
            richTextBox1.SelectionFont = new Font("Consolas", 8);
            richTextBox1.SelectionColor = Color.Gray;
            richTextBox1.AppendText(ts + "\n\n");

            // Reset formatting
            richTextBox1.SelectionAlignment = HorizontalAlignment.Left;
            richTextBox1.SelectionColor = Color.Black;

            richTextBox1.ScrollToCaret();
        }

        private void MarkUserAsNew(string username)
        {
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                var name = listBox1.Items[i].ToString();
                if (name == username && !name.StartsWith("(new)"))
                {
                    listBox1.Items[i] = "(new) " + name;
                    break;
                }
            }
        }

        // ---------------- Navigation ----------------
        private void button1_Click(object sender, EventArgs e)
        {
            mainForm.ShowForm(new LoginForm(mainForm));
        }

        // --------- Unused auto-generated handlers ---------
        private void pnlBottom_Paint(object sender, PaintEventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void fileToolStripMenuItem_Click(object sender, EventArgs e) { }
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e) { }
        private void richTextBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (connection == null ||
                connection.State != HubConnectionState.Connected)
            {
                MessageBox.Show("Not connected to server.");
                return;
            }

            await LoadUsers();
        }
    }

    // ---------------- DTO ----------------
    public class ChatMessageDto
    {
        public string Sender { get; set; } = "";
        public string Message { get; set; } = "";
        public string Timestamp { get; set; } = "";
    }
}
