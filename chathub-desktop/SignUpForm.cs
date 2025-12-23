using System;
using System.Windows.Forms;

namespace chathub_desktop
{
    public partial class SignUpForm : Form
    {
        private readonly MainForm mainForm;

        public SignUpForm(MainForm parent)
        {
            InitializeComponent();
            mainForm = parent;
        }

        private async void btnCreateAccount_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string pass = txtPassword.Text;
            string confirm = txtConfirm.Text;

            if (name.Length == 0 || pass.Length == 0 || confirm.Length == 0)
            {
                MessageBox.Show("Fill all fields.");
                return;
            }

            if (pass != confirm)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            if (!await mainForm.EnsureConnection())
                return;

            bool ok;
            try
            {
                ok = await mainForm.Register(name, pass);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Registration failed: " + ex.Message);
                return;
            }

            if (!ok)
            {
                MessageBox.Show("Username already exists.");
                return;
            }

            MessageBox.Show("Account created successfully.");
            mainForm.ShowForm(new LoginForm(mainForm));
        }

        private void lblHaveAccount_Click(object sender, EventArgs e)
        {
            mainForm.ShowForm(new LoginForm(mainForm));
        }



      

       
    }

}

