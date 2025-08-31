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

namespace GamingRigz
{
    public partial class Archive : Form
    {

        private bool columnsWidthSet = false;

        SqlConnection con = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=stocks;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

        public Archive()
        {
            InitializeComponent();
            dataGridView1.ScrollBars = ScrollBars.None;
        }

        public void disp_data()
        {
            con.Open();
            SqlCommand cmd = con.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select * from dbo.archive";

            // Use a DataTable to hold the data
            DataTable dt = new DataTable();

            // Use a SqlDataAdapter to fill the DataTable
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }

            // Set the DataGridView's DataSource to the filled DataTable
            dataGridView1.DataSource = dt;

            con.Close();
        }

        private void Archive_Load(object sender, EventArgs e)
        {
            disp_data();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            Stocks stocksForm = new Stocks();
            stocksForm.Show();

            // Optionally, close or hide the login form
            this.Hide();
        }

        private void button8_Click(object sender, EventArgs e)
        {
           
        }

        private void button6_Click(object sender, EventArgs e)
        {
            
        }

        private void SearchData(string keyword)
        {
            using (SqlConnection con = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=stocks;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
            {
                con.Open();

                // Use OR to search in multiple columns
                SqlCommand cmd = new SqlCommand("SELECT * FROM dbo.stocks WHERE [Product Name] LIKE @Keyword OR Category LIKE @Keyword OR Convert(VARCHAR, Price) LIKE @Keyword", con);
                cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                dataGridView1.DataSource = dt;

                con.Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Check if there are any items in the archive
            if (HasItemsInArchive())
            {
                // Prompt the user for confirmation
                DialogResult result = MessageBox.Show("Are you sure you want to empty the archive? This action cannot be undone.", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    // User confirmed, proceed to empty the archive
                    EmptyArchive();
                }
            }
            else
            {
                MessageBox.Show("There are no items to empty from the archive.", "No Items", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void EmptyArchive()
        {
            using (SqlConnection con = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=stocks;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
            {
                con.Open();

                // Execute the SQL command to delete all records in the archive table
                SqlCommand cmd = new SqlCommand("DELETE FROM dbo.archive", con);
                cmd.ExecuteNonQuery();

                con.Close();

                // Refresh the display or take any other necessary actions
                disp_data();

                MessageBox.Show("Archive emptied successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Check if there are any items in the archive
            if (HasItemsInArchive())
            {
                // Prompt the user to confirm the restoration
                DialogResult result = MessageBox.Show("Are you sure you want to restore all items from the archive?", "Confirm Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Loop through all rows in the DataGridView
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        // Check if the row is not empty
                        if (row.Cells["Product Name"].Value != DBNull.Value)
                        {
                            // Extract values from the row
                            string productName = row.Cells["Product Name"].Value?.ToString();
                            string category = row.Cells["Category"].Value?.ToString();
                            decimal? price = row.Cells["Price"].Value as decimal?;

                            // Provide a default value for quantity, or retrieve it from the row if available
                            int quantity = 0; // Replace with an appropriate default value
                            if (row.Cells["Quantity"].Value != null)
                            {
                                quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                            }

                            // Call the RestoreRow method for each row
                            RestoreItem(productName, category, price, quantity);
                        }
                    }

                    // Optionally, refresh the DataGridView or perform any other actions
                    // For example, you might want to reload the data from the database

                    // dataGridView1.DataSource = ... ; // Reload data

                    // Inform the user that restoration is complete
                    MessageBox.Show("Items restored successfully.", "Restore Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("There are no items to restore from the archive.", "No Items", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool HasItemsInArchive()
        {
            // Create a DataTable to check if there are any rows in the archive
            DataTable dt = (DataTable)dataGridView1.DataSource;
            return dt != null && dt.Rows.Count > 0 && dt.Rows[0]["Product Name"] != DBNull.Value;
        }

        private void RestoreItem(string productName, string category, decimal? price, int? quantity)
        {
            if (!string.IsNullOrEmpty(productName))
            {
                // Perform the restoration logic here
                // Example: Insert the row into the stocks table
                using (SqlConnection con = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=stocks;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO dbo.stocks ([Product Name], Category, Price, Quantity) VALUES (@ProductName, @Category, @Price, @Quantity)", con);
                    cmd.Parameters.AddWithValue("@ProductName", productName);
                    cmd.Parameters.AddWithValue("@Category", category);
                    cmd.Parameters.AddWithValue("@Price", price ?? (object)DBNull.Value); // Handle nullable decimal
                    cmd.Parameters.AddWithValue("@Quantity", quantity ?? (object)DBNull.Value); // Handle nullable decimal
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                // Clear the row from the archive table
                using (SqlConnection con = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=stocks;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM dbo.archive WHERE [Product Name] = @ProductName", con);
                    cmd.Parameters.AddWithValue("@ProductName", productName);
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                // Refresh the display of the archive table
                disp_data();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            disp_data();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2CirclePictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            Login loginForm = new Login();
            loginForm.Show();

            this.Hide();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomGradientPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2CustomGradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {

        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            Stocks stockForm = new Stocks();
            stockForm.Show();

            // Optionally, close or hide the login form
            this.Hide();
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (!columnsWidthSet)
            {
                SetColumnWidths();
                columnsWidthSet = true;
            }

            if (e.ColumnIndex == dataGridView1.Columns["Category"].Index ||
                e.ColumnIndex == dataGridView1.Columns["Price"].Index ||
                e.ColumnIndex == dataGridView1.Columns["Quantity"].Index)
            {
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private void SetColumnWidths()
        {
            dataGridView1.Columns["Product Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGridView1.Columns["Category"].FillWeight = 25;
            dataGridView1.Columns["Category"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGridView1.Columns["Quantity"].FillWeight = 15;
            dataGridView1.Columns["Quantity"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGridView1.Columns["Price"].FillWeight = 15;
            dataGridView1.Columns["Price"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            SalesReport salesreportForm = new SalesReport();
            salesreportForm.Show();

            // Optionally, close or hide the login form
            this.Hide();
        }
    }
    
}
