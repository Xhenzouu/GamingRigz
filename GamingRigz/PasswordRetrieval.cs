using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace GamingRigz
{
    public partial class PasswordRetrieval : Form
    {
        SqlConnection con = new SqlConnection(@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = stocks; Integrated Security = True; Connect Timeout = 30; Encrypt=False;");

        public PasswordRetrieval()
        {
            InitializeComponent();
        }

        private void btnRecoverPassword_Click(object sender, EventArgs e)
        {
            {
                string email = tbEmail.Text;
                string selectedRole = cbPosition.SelectedItem?.ToString();

                if (!string.IsNullOrEmpty(selectedRole) && CheckIfAccountExists(email, selectedRole))
                {
                    string password = RetrievePassword(email, selectedRole);
                    MessageBox.Show($"Your {selectedRole} password is: {password}", "Password Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Email not found or incorrect role. Please enter a valid email and role.", "Password Recovery", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        private bool CheckIfAccountExists(string email, string userRole)
        {
            try
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @Email AND UserRole = @UserRole", con);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@UserRole", userRole);

                int count = (int)cmd.ExecuteScalar();

                return count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking email and role: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                con.Close();
            }
        }
        private string RetrievePassword(string email, string userRole)
        {
            try
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT Password FROM Users WHERE Email = @Email AND UserRole = @UserRole", con);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@UserRole", userRole);

                object result = cmd.ExecuteScalar();

                return result != null ? result.ToString() : string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving password: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
            finally
            {
                con.Close();
            }
        }
    }
}
