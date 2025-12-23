using System;
using System.Windows.Forms;

namespace chathub_desktop
{
    public partial class LoginForm : Form
    {
        private readonly MainForm mainForm;

        public LoginForm(MainForm parent)
        {
            InitializeComponent();
            mainForm = parent;
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

            if (!await mainForm.EnsureConnection())
                return;

            bool ok;
            try
            {
                ok = await mainForm.Login(username, password);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login failed: " + ex.Message);
                return;
            }

            if (!ok)
            {
                MessageBox.Show("Invalid username or password.");
                return;
            }

            mainForm.ShowForm(new ChatForm(mainForm, username));
        }

        private void btnSignup_Click(object sender, EventArgs e)
        {
            mainForm.ShowForm(new SignUpForm(mainForm));
        }



        
    }
}

