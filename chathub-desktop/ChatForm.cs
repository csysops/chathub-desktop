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
        private readonly MainForm mainForm;
        private readonly string currentUser;

        private string selectedPeer = "";

        public ChatForm(MainForm parent, string username)
        {
            InitializeComponent();
            mainForm = parent;
            currentUser = username;

            label2.Text = "Logged as: " + currentUser;

            mainForm.PrivateMessageReceived += OnPrivateMessageReceived;

            Load += ChatForm_Load;
        }

        private async void ChatForm_Load(object sender, EventArgs e)
        {
            await ReloadUsers();
        }

        private async void OnPrivateMessageReceived(string from, string message)
        {
            if (from != selectedPeer)
            {
                BeginInvoke(new Action(() => MarkUserAsNew(from)));
                return;
            }

            await ReloadCurrentChat();
        }

        // ---------------- Users ----------------
        private async Task ReloadUsers()
        {
            List<string> users = await mainForm.GetAllUsers();

            listBox1.Items.Clear();
            foreach (string u in users)
            {
                if (u != currentUser)
                    listBox1.Items.Add(u);
            }
        }

        private async void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
                return;

            selectedPeer = listBox1.SelectedItem.ToString()
                .Replace("(new) ", "");

            await ReloadCurrentChat();
        }

        // ---------------- Chat ----------------
        private async Task ReloadCurrentChat()
        {
            if (selectedPeer.Length == 0)
                return;

            List<ChatMessageDto> history =
                await mainForm.LoadHistory(currentUser, selectedPeer);

            BeginInvoke(new Action(() =>
            {
                richTextBox1.Clear();
                foreach (ChatMessageDto m in history)
                    AppendMessage(m.Sender, m.Message, m.Timestamp);
            }));
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            if (selectedPeer.Length == 0)
                return;

            string msg = textBox1.Text.Trim();
            if (msg.Length == 0)
                return;

            await mainForm.SendPrivate(currentUser, selectedPeer, msg);
            textBox1.Clear();

            await ReloadCurrentChat();
        }

        // ---------------- UI Helpers ----------------
        private void AppendMessage(string sender, string message, string ts)
        {
            bool isMe = sender == currentUser;

            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.SelectionAlignment =
                isMe ? HorizontalAlignment.Right : HorizontalAlignment.Left;

            richTextBox1.AppendText(sender + "\n");
            richTextBox1.AppendText(message + "\n");
            richTextBox1.AppendText(ts + "\n\n");

            richTextBox1.ScrollToCaret();
        }

        private void MarkUserAsNew(string username)
        {
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                string name = listBox1.Items[i].ToString();
                if (name == username)
                {
                    listBox1.Items[i] = "(new) " + name;
                    break;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mainForm.ShowForm(new LoginForm(mainForm));
        }

        
        private async void button2_Click(object sender, EventArgs e)
        {
            await ReloadUsers();
        }
    }
}
