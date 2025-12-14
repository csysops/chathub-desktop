using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chathub_desktop
{

    public partial class MainForm : Form
    {

        
        public MainForm()
        {
            InitializeComponent();
            ShowForm(new LoginForm(this)); // Show LoginForm on startup
        }

        // This method will display any child form inside MainForm
        public void ShowForm(Form childForm)
        {
            // Remove any existing child forms
            foreach (Control c in this.Controls)
            {
                if (c is Form)
                {
                    ((Form)c).Close();
                    this.Controls.Remove(c);
                }
            }

            childForm.TopLevel = false;                 // Important
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            this.Controls.Add(childForm);
            childForm.Show();
        }

        public HubConnection Connection { get; private set; }

        public async Task<bool> EnsureConnection()
        {
            if (Connection != null && Connection.State == HubConnectionState.Connected)
                return true;

            Connection = new HubConnectionBuilder()
                .WithUrl("http://datpt.somee.com/chat")
                .WithAutomaticReconnect()
                .Build();

            try
            {
                await Connection.StartAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}");
                return false;
            }
        }


    }
}