using System;
using System.Drawing;
using System.Windows.Forms;

namespace Activity3
{
    public partial class Login : Form
    {
        private bool databaseReady;

        public Login()
        {
            InitializeComponent();

            this.AcceptButton = button1;
            textBox3.PasswordChar = '*';
            textBox3.UseSystemPasswordChar = false;
            button1.Click += LoginButton_Click;

            linkRegister.LinkClicked += (_, _) =>
            {
                using var register = new Register();
                Hide();
                register.ShowDialog(this);
                Show();
                textBox2.Clear();
                textBox3.Clear();
                textBox2.Focus();
            };

            Shown += (_, _) =>
            {
                try
                {
                    Database.Initialize();
                    databaseReady = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Unable to connect to the Microsoft Access database.\n\n" + ex.Message +
                        "\n\nMake sure Microsoft Access Database Engine (ACE) is installed with the same 64-bit/32-bit architecture as this project.",
                        "Database connection error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }

        private void LoginButton_Click(object? sender, EventArgs e)
        {
            if (!databaseReady)
            {
                MessageBox.Show("The database is not connected. Please close and reopen the application after fixing the database connection.",
                    "Database unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string username = textBox2.Text.Trim();
            string password = textBox3.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter your username and password.",
                    "Required fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!Database.ValidateLogin(username, password))
                {
                    MessageBox.Show("Incorrect username or password.",
                        "Login failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox3.Clear();
                    textBox3.Focus();
                    return;
                }

                using var dashboard = new Dashboard();
                Hide();
                dashboard.ShowDialog(this);
                Show();
                textBox3.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login failed because of a database error:\n\n" + ex.Message,
                    "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
