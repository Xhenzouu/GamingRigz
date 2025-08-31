using System;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Drawing;



namespace GamingRigz
{
    public partial class Login : Form
    {

        private string originalPassword = string.Empty;

        SqlConnection con = new SqlConnection(@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = stocks; Integrated Security = True; Connect Timeout = 30; Encrypt=False;");

        public Login()
        {
            InitializeComponent();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text;
            string password = textBox2.Text;

            // Perform authentication logic here...
            if (AuthenticateManager(username, password))
            {
                // If authentication is successful, open the overview window
                Stocks stockForm = new Stocks();
                stockForm.Show();

                // Optionally, close or hide the login form
                this.Hide();

                MessageBox.Show("Welcome, Manager!");
            }
            else if (AuthenticateStaff(username, password))
            {
                orderingForm orderingForm = new orderingForm();
                orderingForm.Show();

                this.Hide();

                MessageBox.Show("Welcome, Staff!");
            }
            else
            {
                MessageBox.Show("Invalid username or password. Please try again.");
            }
        }

        private bool AuthenticateStaff(string username, string password)
        {
            const string hardcodedUsername = "Staff";
            const string hardcodedPassword = "123";

            return (username == hardcodedUsername && password == hardcodedPassword);
        }

        private bool AuthenticateManager(string username, string password)
        {
            // Hardcoded username and password
            const string hardcodedUsername = "Manager";
            const string hardcodedPassword = "123";

            // Compare entered credentials with hardcoded values
            return (username == hardcodedUsername && password == hardcodedPassword);
        }

        private void Login_Load(object sender, EventArgs e)
        {
            // Initialization code, if needed
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void guna2ToggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            
        }

        private void guna2ToggleSwitch1_CheckedChanged_1(object sender, EventArgs e)
        {
            textBox2.UseSystemPasswordChar = !guna2ToggleSwitch1.Checked;
            UpdateStatusLabel();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!guna2ToggleSwitch1.Checked)
            {
                textBox2.UseSystemPasswordChar = true;
                UpdateStatusLabel();
            }
        }

        private void UpdateStatusLabel()
        {
            labelStatus.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            // Assuming you have a label named labelStatus
            if (guna2ToggleSwitch1.Checked)
            {
                labelStatus.Text = "Visible";
                labelStatus.ForeColor = Color.LightGray; // Set color to green when visible
            }
            else
            {
                labelStatus.Text = "Hidden";
                labelStatus.ForeColor = Color.LightGray; // Set color to red when hidden
            }
        }
    } 
}