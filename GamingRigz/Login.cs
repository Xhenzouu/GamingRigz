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

            if (AuthenticateManager(username, password))
            {
                Stocks stockForm = new Stocks();
                stockForm.Show();

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
        private void btRegister_Click(object sender, EventArgs e)
        {

        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert the hashed bytes to a string for storage
                return BitConverter.ToString(hashedBytes).Replace("-", "");
            }
        }
        private bool AuthenticateStaff(string username, string password)
        {
            try
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password AND UserRole = 'Staff'", con);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);

                int count = (int)cmd.ExecuteScalar();

                return count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error authenticating user: " + ex.Message);
                return false;
            }
            finally
            {
                con.Close();
            }
        }

        private bool AuthenticateManager(string username, string password)
        {
            try
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password AND UserRole = 'Manager'", con);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);

                int count = (int)cmd.ExecuteScalar();

                return count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error authenticating user: " + ex.Message);
                return false;
            }
            finally
            {
                con.Close();
            }
        }


        private void Login_Load(object sender, EventArgs e)
        {

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


        private void btRegister_Click_1(object sender, EventArgs e)
        {
            RoleSelectionForm reg = new RoleSelectionForm();
            reg.Show();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            PasswordRetrieval recover = new PasswordRetrieval();
            recover.Show();
        }
    } 
}