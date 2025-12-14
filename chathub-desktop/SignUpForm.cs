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
using System.Xml.Linq;

namespace chathub_desktop
{
    public partial class SignUpForm : Form
    {
        private MainForm mainForm;
        public SignUpForm(MainForm parentForm)
        {
            InitializeComponent();
            mainForm = parentForm;
        }

        private async void btnCreateAccount_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string pass = txtPassword.Text;
            string confirm = txtConfirm.Text;

            if (string.IsNullOrEmpty(name) ||
                string.IsNullOrEmpty(pass) ||
                string.IsNullOrEmpty(confirm))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (pass != confirm)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            // 1. Ensure SignalR connection
            if (!await mainForm.EnsureConnection())
                return;

            try
            {
                // 2. Call backend RegisterUser (same as reference ChatForm)
                bool ok = await mainForm.Connection
                    .InvokeAsync<bool>("RegisterUser", name, pass);

                if (!ok)
                {
                    MessageBox.Show("Username already exists.");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration failed: {ex.Message}");
                return;
            }

            // 3. Success
            MessageBox.Show("Account created successfully. You can now log in.");
            mainForm.ShowForm(new LoginForm(mainForm));
        }




        private void txtConfirm_TextChanged(object sender, EventArgs e)
        {

        }

        private void lblHaveAccount_Click(object sender, EventArgs e)
        {
            mainForm.ShowForm(new LoginForm(mainForm));
        }
    }

}
