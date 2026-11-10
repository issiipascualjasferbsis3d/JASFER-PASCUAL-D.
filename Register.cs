using System;
using System.Windows.Forms;

namespace Activity3
{
    public partial class Register : Form
    {
        public Register()
        {
            InitializeComponent();

            this.AcceptButton = button1;
            textBox3.PasswordChar = '*';
            textBox3.UseSystemPasswordChar = false;
            textBox4.PasswordChar = '*';
            textBox4.UseSystemPasswordChar = false;
            button1.Click += RegisterButton_Click;

            linkLogin.LinkClicked += (_, _) => Close();
        }

        private void Register_Load(object sender, EventArgs e)
        {
            try
            {
                Database.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to connect to the Microsoft Access database.\n\n" + ex.Message +
                    "\n\nMake sure Microsoft Access Database Engine (ACE) is installed with the same 64-bit/32-bit architecture as this project.",
                    "Database connection error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button1.Enabled = false;
            }
        }

        private void RegisterButton_Click(object? sender, EventArgs e)
        {
            string fullName = textBox1.Text.Trim();
            string username = textBox2.Text.Trim();
            string password = textBox3.Text;
            string confirmPassword = textBox4.Text;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Please complete all fields.",
                    "Required fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (fullName.Length > 100 || username.Length > 50)
            {
                MessageBox.Show("Full name must be 100 characters or fewer and username must be 50 characters or fewer.",
                    "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Password must contain at least 6 characters.",
                    "Weak password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("The passwords do not match.",
                    "Password mismatch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox4.Clear();
                textBox4.Focus();
                return;
            }

            try
            {
                if (Database.UsernameExists(username))
                {
                    MessageBox.Show("That username is already registered. Please choose another.",
                        "Username already exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox2.Focus();
                    return;
                }

                Database.RegisterUser(fullName, username, password);
                MessageBox.Show("Account created successfully!","Registration successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not create the account:\n\n" + ex.Message,
                    "Registration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
