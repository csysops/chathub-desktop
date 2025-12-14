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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace chathub_desktop
{
    public partial class LoginForm : Form
    {
        private MainForm mainForm;
        public LoginForm(MainForm parentForm)
        {
            InitializeComponent();
            mainForm = parentForm;
        }

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }

        private void btnSignup_Click(object sender, EventArgs e)
        {
            mainForm.ShowForm(new SignUpForm(mainForm));
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (username.Length == 0 || password.Length == 0)
            {
                MessageBox.Show("Enter username and password.");
                return;
            }

            // 1. Ensure SignalR connection
            if (!await mainForm.EnsureConnection())
                return;

            try
            {
                // 2. Call backend LoginUser (same as reference)
                bool ok = await mainForm.Connection
                    .InvokeAsync<bool>("LoginUser", username, password);

                if (!ok)
                {
                    MessageBox.Show("Invalid username or password.");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}");
                return;
            }

            // 3. Navigate to ChatForm on success
            mainForm.ShowForm(new ChatForm(mainForm, username));
        }


        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
