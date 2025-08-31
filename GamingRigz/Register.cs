using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace GamingRigz
{
    public partial class RoleSelectionForm : Form
    {
        public string SelectedUsername { get; private set; }
        public string SelectedPassword { get; private set; }
        public string SelectedEmail { get; private set; }
        public string SelectedRole { get; private set; }


        public RoleSelectionForm()
        {
            InitializeComponent();
            InitializeRoleComboBox();

        }
        private void InitializeRoleComboBox()
        {
            // Add role options to the combo box
            cbPosition.Items.Add("Staff");
            cbPosition.Items.Add("Manager");
        }

        private void cbRole_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string username = tbUser.Text;
            string password = tbPass.Text;
            string email = tbEmail.Text;

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(email))
            {
                SelectedUsername = username;
                SelectedPassword = password;
                SelectedEmail = email;

                // Check if a role is selected
                if (cbPosition.SelectedItem != null)
                {
                    SelectedRole = cbPosition.SelectedItem.ToString();


                    // Save the user to the 'Users' table with their role
                    RegisterUser(username, password, email, SelectedRole);

                    // Set DialogResult to OK to indicate a successful registration
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("Please select a user role.");
                }
            }
            else
            {
                MessageBox.Show("Please fill in all the required fields.");
            }
        }
        private void RegisterUser(string username, string password, string email, string userRole)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = stocks; Integrated Security = True; Connect Timeout = 30; Encrypt=False;"))
                {
                    con.Open();

                    // Check if the username already exists
                    SqlCommand cmdCheckUser = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username", con);
                    cmdCheckUser.Parameters.AddWithValue("@Username", username);
                    int userCount = (int)cmdCheckUser.ExecuteScalar();

                    if (userCount == 0)
                    {
                        // Insert the new user into the 'Users' table
                        SqlCommand cmdRegister = new SqlCommand("INSERT INTO Users (Username, Password, Email, UserRole) VALUES (@Username, @Password, @Email, @UserRole)", con);
                        cmdRegister.Parameters.AddWithValue("@Username", username);
                        cmdRegister.Parameters.AddWithValue("@Password", password); // Store the password as plain text
                        cmdRegister.Parameters.AddWithValue("@Email", email);
                        cmdRegister.Parameters.AddWithValue("@UserRole", userRole);
                        cmdRegister.ExecuteNonQuery();

                        // Display a MessageBox indicating successful registration
                        MessageBox.Show("Registration successful! You can now log in with your credentials.");

                        // Set DialogResult to OK to indicate a successful registration
                        DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        MessageBox.Show("Username already exists. Please choose a different username.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error registering user: " + ex.Message);
            }
        }

        private void tbPass_TextChanged(object sender, EventArgs e)
        {

             if (!guna2ToggleSwitch1.Checked)
             {
                    textBox2.UseSystemPasswordChar = true;
                    UpdateStatusLabel();
             }
        }
        private void guna2ToggleSwitch2_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.UseSystemPasswordChar = !guna2ToggleSwitch1.Checked;
            UpdateStatusLabel();
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
    }

}
