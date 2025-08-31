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
using System.Windows.Markup;

namespace GamingRigz
{
    public partial class Stocks : Form
    {
        private bool columnsWidthSet = false;
        

        SqlConnection con = new SqlConnection(@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = stocks; Integrated Security = True; Connect Timeout = 30; Encrypt=False;");

        public Stocks()
        {
            InitializeComponent();

            dataGridView1.CellClick += dataGridView1_CellClick;
            dataGridView1.ScrollBars = ScrollBars.Vertical;
            dataGridView1.ReadOnly = true;
        }

        public void CustomizeDataGridViewAppearance()
        {
            if (dataGridView1 == null)
            {
                MessageBox.Show("DataGridView is not initialized.");
                return;
            }

            if (dataGridView1.Rows.Count == 0)
            {
                return; // No rows to customize
            }

            if (!dataGridView1.Columns.Contains("Category") || !dataGridView1.Columns.Contains("Quantity"))
            {
                MessageBox.Show("Required columns are missing in the DataGridView.");
                return;
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Category"] != null && row.Cells["Quantity"] != null)
                {
                    string category = row.Cells["Category"].Value?.ToString();
                    decimal quantity;  // Change int to decimal here

                    if (decimal.TryParse(row.Cells["Quantity"].Value?.ToString(), out quantity))  // Change int.TryParse to decimal.TryParse
                    {


                    }
                }
            }
        }

        public void disp_data()
        {
            try
            {
                con.Open();
                SqlCommand cmd = con.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "select * from dbo.stocks";

                DataTable dt = new DataTable();

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }

                dataGridView1.DataSource = dt;

                con.Close();

                CustomizeDataGridViewAppearance();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while fetching data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ContainsNonNumeric(string input)
        {
            return input.Any(c => !char.IsDigit(c));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            con.Open();
            bool insertionSuccessful = false;

            try
            {
                if (!IsValidNumeric(textBox3.Text) || !IsValidNumeric(textBox4.Text))
                {
                    MessageBox.Show("Invalid input. Please fill up all fields.");
                }
                else if (string.IsNullOrWhiteSpace(textBox1.Text) || !ContainsNonNumeric(textBox1.Text))
                {
                    MessageBox.Show("Product Name must contain at least one non-numeric character.");
                }
                else if (string.IsNullOrWhiteSpace(comboBox1.Text))
                {
                    MessageBox.Show("Category is required.");
                }
                else
                {
                    SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM dbo.stocks WHERE [Product Name] = @ProductName AND Category = @Category AND Price = @Price AND Quantity = @Quantity", con);
                    checkCmd.Parameters.AddWithValue("@ProductName", textBox1.Text);
                    checkCmd.Parameters.AddWithValue("@Category", comboBox1.Text);
                    checkCmd.Parameters.AddWithValue("@Price", textBox3.Text);
                    checkCmd.Parameters.AddWithValue("@Quantity", textBox4.Text);

                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("Product already exists. You cannot insert duplicate entries.");
                    }
                    else
                    {
                        // Validate quantity
                        if (int.TryParse(textBox4.Text, out int quantity) && quantity > 0 && quantity <= 40)
                        {
                            // Validate price
                            if (decimal.TryParse(textBox3.Text, out decimal price) && price >= 0)
                            {
                                // Insert the record if it doesn't exist
                                SqlCommand insertCmd = new SqlCommand("INSERT INTO dbo.stocks ([Product Name], Category, Price, Quantity) VALUES (@ProductName, @Category, @Price, @Quantity)", con);
                                insertCmd.Parameters.AddWithValue("@ProductName", textBox1.Text);
                                insertCmd.Parameters.AddWithValue("@Category", comboBox1.Text);
                                insertCmd.Parameters.AddWithValue("@Price", textBox3.Text);
                                insertCmd.Parameters.AddWithValue("@Quantity", textBox4.Text);

                                insertCmd.ExecuteNonQuery();
                                MessageBox.Show("Record inserted successfully");
                                insertionSuccessful = true;
                            }
                            else
                            {
                                MessageBox.Show("Invalid price. Please enter a non-negative decimal value.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid quantity. The quantity must be between 1 and 40.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            finally
            {
                if (insertionSuccessful)
                {
                    textBox1.Clear();
                    textBox3.Clear();
                    textBox4.Clear();
                    comboBox1.SelectedIndex = -1;
                }

                con.Close();
                disp_data();
            }
        }

        public void disp_data_custom()
        {
            con.Open();
            SqlCommand cmd = con.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select * from dbo.stocks";

            DataTable dt = new DataTable();

            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }

            dataGridView1.DataSource = dt;

            con.Close();
        }

        private void Stocks_Load(object sender, EventArgs e)
        {
                    
            disp_data();

            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            disp_data();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            con.Open();

            if (!IsValidNumeric(textBox3.Text) || !IsValidNumeric(textBox4.Text))
            {
                MessageBox.Show("Invalid input. Please enter valid numeric information.");
                con.Close();
                return;
            }

            SqlCommand checkCmd = new SqlCommand("SELECT Quantity, Price FROM dbo.stocks WHERE [Product Name] = @ProductName AND Category = @Category", con);
            checkCmd.Parameters.AddWithValue("@ProductName", textBox1.Text);
            checkCmd.Parameters.AddWithValue("@Category", comboBox1.Text);

            SqlDataReader reader = checkCmd.ExecuteReader();

            if (reader.Read())
            {
                int currentQuantity = Convert.ToInt32(reader["Quantity"]);
                decimal currentPrice = Convert.ToDecimal(reader["Price"]);

                reader.Close();

                if (currentQuantity >= 0)  // Check if the current quantity is non-negative
                {
                    bool updateQuantity = !string.IsNullOrEmpty(textBox4.Text);  // Check if textbox is not empty
                    bool updatePrice = !string.IsNullOrEmpty(textBox3.Text);      // Check if textbox is not empty

                    if (updateQuantity)
                    {
                        int increment;
                        if (int.TryParse(textBox4.Text, out increment))
                        {
                            int updatedQuantity = currentQuantity + increment;

                            if (updatedQuantity >= 0 && updatedQuantity <= 50)
                            {
                                SqlCommand updateQuantityCmd = new SqlCommand("UPDATE dbo.stocks SET Quantity = Quantity + @Increment WHERE [Product Name] = @ProductName AND Category = @Category", con);
                                updateQuantityCmd.Parameters.AddWithValue("@ProductName", textBox1.Text);
                                updateQuantityCmd.Parameters.AddWithValue("@Category", comboBox1.Text);
                                updateQuantityCmd.Parameters.AddWithValue("@Increment", increment);

                                updateQuantityCmd.ExecuteNonQuery();

                                MessageBox.Show("Product quantity updated successfully");
                            }
                            else if (updatedQuantity < 0)
                            {
                                MessageBox.Show("Invalid quantity. The resulting quantity cannot be negative.");
                            }
                            else if (updatedQuantity > 50)
                            {
                                MessageBox.Show("Invalid quantity. The resulting quantity cannot exceed 50.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid quantity. Please enter a valid integer value.");
                        }
                    }

                    if (updatePrice)
                    {
                        decimal newPrice;
                        if (decimal.TryParse(textBox3.Text, out newPrice))
                        {
                            SqlCommand updatePriceCmd = new SqlCommand("UPDATE dbo.stocks SET Price = @NewPrice WHERE [Product Name] = @ProductName AND Category = @Category", con);
                            updatePriceCmd.Parameters.AddWithValue("@ProductName", textBox1.Text);
                            updatePriceCmd.Parameters.AddWithValue("@Category", comboBox1.Text);
                            updatePriceCmd.Parameters.AddWithValue("@NewPrice", newPrice);

                            updatePriceCmd.ExecuteNonQuery();

                            
                        }
                        else
                        {
                            MessageBox.Show("Invalid price. Please enter a valid decimal value.");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Product not found or the current quantity is negative. You can only update the quantity or price for existing products with non-negative quantity.");
                }
            }
            else
            {
                MessageBox.Show("Product not found.");
            }

            reader.Close();

            textBox1.Clear();
            textBox3.Clear();
            textBox4.Clear();
            comboBox1.SelectedIndex = -1;

            con.Close();
            disp_data_custom();
           
        }

        private bool IsValidNumeric(string input)
        {
            return double.TryParse(input, out _);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            {
                string productName = textBox1.Text;
                string category = comboBox1.Text;

                decimal price;
                if (!decimal.TryParse(textBox3.Text, out price))
                {
                    MessageBox.Show("Invalid format. Please enter a valid information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return; // exit the method if the conversion fails
                }

                DialogResult result = MessageBox.Show("Are you sure you want to delete this record?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    con.Open();

                    SqlCommand archiveCmd = new SqlCommand("INSERT INTO dbo.archive SELECT * FROM dbo.stocks WHERE [Product Name] = @ProductName AND Category = @Category AND Price = @Price", con);
                    archiveCmd.Parameters.AddWithValue("@ProductName", productName);
                    archiveCmd.Parameters.AddWithValue("@Category", category);
                    archiveCmd.Parameters.AddWithValue("@Price", price);
                    archiveCmd.ExecuteNonQuery();

                    SqlCommand deleteCmd = new SqlCommand("DELETE FROM dbo.stocks WHERE [Product Name] = @ProductName AND Category = @Category AND Price = @Price", con);
                    deleteCmd.Parameters.AddWithValue("@ProductName", productName);
                    deleteCmd.Parameters.AddWithValue("@Category", category);
                    deleteCmd.Parameters.AddWithValue("@Price", price);
                    deleteCmd.ExecuteNonQuery();

                    MessageBox.Show("Record deleted and moved to archive successfully");

                    con.Close();

                    textBox1.Clear();
                    textBox3.Clear();
                    comboBox1.SelectedIndex = -1;
                }
                disp_data();
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Archive archiveForm = new Archive();
            archiveForm.Show();

            this.Hide();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            
        }

        private void SearchData(string keyword)
        {
            using (SqlConnection con = new SqlConnection(@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = stocks; Integrated Security = True; Connect Timeout = 30; Encrypt=False;"))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT * FROM dbo.stocks WHERE [Product Name] LIKE @Keyword OR Category LIKE @Keyword OR Convert(VARCHAR, Price) LIKE @Keyword", con);
                cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                dataGridView1.DataSource = dt;

                con.Close();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                if (row.Cells["Product Name"] != null &&
                    row.Cells["Category"] != null &&
                    row.Cells["Price"] != null &&
                    row.Cells["Quantity"] != null)
                {
                    textBox1.Text = row.Cells["Product Name"].Value?.ToString();
                    comboBox1.Text = row.Cells["Category"].Value?.ToString();
                    textBox3.Text = row.Cells["Price"].Value?.ToString();
                    textBox4.Text = row.Cells["Quantity"].Value?.ToString(); // Update quantity textbox if needed

                    textBox1.Enabled = false;
                    comboBox1.Enabled = false;
                    textBox3.Enabled = false;
                    insertButton.Enabled = false;

                }
                else
                {
                    MessageBox.Show("One or more cells are null in the selected row.");
                }
            }

            textBox4.Clear();

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to exit?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
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

            if (e.ColumnIndex == dataGridView1.Columns["Quantity"].Index)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                decimal quantity;

                if (decimal.TryParse(row.Cells["Quantity"].Value?.ToString(), out quantity))
                {
                    if (quantity >= 1 && quantity <= 10)
                    {
                        e.CellStyle.BackColor = Color.Red;
                    }
                    else if (quantity >= 11 && quantity <= 20)
                    {
                        e.CellStyle.BackColor = Color.Yellow;
                    }
                    else if (quantity >= 21 && quantity <= 50)
                    {
                        e.CellStyle.BackColor = Color.LimeGreen;
                    }
                    else
                    {
                        e.CellStyle.BackColor = Color.White;
                    }
                }
                else
                {
                    e.CellStyle.BackColor = Color.White;
                }
            }
            else if (e.ColumnIndex == dataGridView1.Columns["Category"].Index)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                string category = row.Cells["Category"].Value?.ToString();

                switch (category)
                {
                    case "Chassis":
                        row.DefaultCellStyle.BackColor = Color.LightBlue;
                        break;
                    case "Processor":
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                        break;
                    case "MotherBoard":
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                        break;
                    case "GraphicsCard":
                        row.DefaultCellStyle.BackColor = Color.LightSlateGray;
                        break;
                    case "Memory":
                        row.DefaultCellStyle.BackColor = Color.LightCyan;
                        break;
                    case "PowerSupply":
                        row.DefaultCellStyle.BackColor = Color.LightSalmon;
                        break;
                    case "Storage":
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                        break;
                    case "Monitor":
                        row.DefaultCellStyle.BackColor = Color.LightGray;
                        break;
                    case "CaseFan":
                        row.DefaultCellStyle.BackColor = Color.LightSkyBlue;
                        break;
                    case "CPUCooler":
                        row.DefaultCellStyle.BackColor = Color.LightSeaGreen; // Updated color for CPUCooler
                        break;
                    case "Mouse":
                        row.DefaultCellStyle.BackColor = Color.LightPink; // Updated color for Mouse
                        break;
                    case "Keyboard":
                        row.DefaultCellStyle.BackColor = Color.LightSteelBlue;
                        break;
                    case "Headset":
                        row.DefaultCellStyle.BackColor = Color.Orange;
                        break;
                    case "MODS":
                        row.DefaultCellStyle.BackColor = Color.PaleVioletRed;
                        break;
                    default:
                        row.DefaultCellStyle.BackColor = Color.White; // Default color for unknown categories
                        break;
                }

            }

        }

        private void SetColumnWidths()
        {
            dataGridView1.Columns["Product Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGridView1.Columns["Category"].FillWeight = 20;
            dataGridView1.Columns["Category"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGridView1.Columns["Quantity"].FillWeight = 15;
            dataGridView1.Columns["Quantity"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGridView1.Columns["Price"].FillWeight = 15;
            dataGridView1.Columns["Price"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            string keyword = textBox5.Text.Trim();
            SearchData(keyword);
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to log out?", "Confirm Log Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Login loginForm = new Login();
                loginForm.Show();
                this.Hide();
            }
        }

        private void Stocks_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
            comboBox1.SelectedIndex = -1; // Clear selected index of the combobox
            textBox3.Text = string.Empty;
            textBox4.Text = string.Empty;

            textBox1.Enabled = true;
            comboBox1.Enabled = true;
            textBox3.Enabled = true;
            insertButton.Enabled = true;
        }

        //private void button8_Click_1(object sender, EventArgs e)
        //{
        //    SalesReport salesreportForm = new SalesReport();
        //    salesreportForm.Show();

        //    this.Hide();
        //}

        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            string url = "https://grpricelist.my.canva.site/grpricelist";
            System.Diagnostics.Process.Start(url);
        }

        private void button11_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomGradientPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to exit?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void button8_Click_1(object sender, EventArgs e)
        {

        }
    }


}

    

