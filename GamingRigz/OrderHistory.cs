using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace GamingRigz
{
    public partial class OrderHistory : Form
    {
        private DataTable originalOrderDataTable;
        private DataTable orderDataTable;
        SqlConnection con = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB; Initial Catalog=stocks; Integrated Security=True; Connect Timeout=30; Encrypt=False;");

        public OrderHistory()
        {
            InitializeComponent();
            dataGridViewOrderHistory.ScrollBars = ScrollBars.Vertical;

            // Initialize the DataTable
            orderDataTable = new DataTable();
            orderDataTable.Columns.Add("OrderID", typeof(int));
            orderDataTable.Columns.Add("CustomerName", typeof(string));
            orderDataTable.Columns.Add("TotalAmount", typeof(decimal));
            orderDataTable.Columns.Add("OrderDate", typeof(DateTime));
            dataGridViewOrderHistory.ReadOnly = false;
        }

        public void ReceiveOrderData(int orderID, string customerName, decimal totalAmount, DateTime orderDate)
        {
            // Add the received order data to the DataGridView
            dataGridViewOrderHistory.Rows.Add(orderID, customerName, totalAmount, orderDate);
        }

        private void LoadOriginalOrderData()
        {
            try
            {
                con.Open();

                // Query to retrieve original order history data
                string query = "SELECT OrderID, CustomerName, TotalAmount, OrderDate FROM orderhistory";

                using (SqlCommand cmd = new SqlCommand(query, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    // Create a new DataTable to store the original data
                    originalOrderDataTable = new DataTable();
                    originalOrderDataTable.Load(reader);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading original order data: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        private void InitializeDataGridView()
        {
            // Add columns to the DataGridView with customized widths
            dataGridViewOrderHistory.Columns.Add("OrderID", "Order ID");
            dataGridViewOrderHistory.Columns["OrderID"].Width = 50; // Set the width as needed
            dataGridViewOrderHistory.Columns["OrderID"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridViewOrderHistory.Columns.Add("CustomerName", "Customer Name");
            dataGridViewOrderHistory.Columns["CustomerName"].Width = 125; // Set the width as needed
            dataGridViewOrderHistory.Columns["CustomerName"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridViewOrderHistory.Columns.Add("TotalAmount", "Total Amount");
            dataGridViewOrderHistory.Columns["TotalAmount"].Width = 75; // Set the width as needed
            dataGridViewOrderHistory.Columns["TotalAmount"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridViewOrderHistory.Columns.Add("OrderDate", "Order Date");
            dataGridViewOrderHistory.Columns["OrderDate"].Width = 150; // Set the width as needed
            dataGridViewOrderHistory.Columns["OrderDate"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
           
        }

        private void DisplayOrderHistory()
        {
            try
            {
                con.Open();

                // Query to retrieve order history data
                string query = "SELECT OrderID, CustomerName, TotalAmount, OrderDate FROM orderhistory";

                using (SqlCommand cmd = new SqlCommand(query, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int orderID = Convert.ToInt32(reader["OrderID"]);
                        string customerName = reader["CustomerName"].ToString();
                        decimal totalAmount = Convert.ToDecimal(reader["TotalAmount"]);
                        DateTime orderDate = Convert.ToDateTime(reader["OrderDate"]);

                        dataGridViewOrderHistory.Rows.Add(orderID, customerName, totalAmount, orderDate);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            orderingForm orderformForm = new orderingForm();
            orderformForm.Show();

            // Optionally, close or hide the login form
            this.Hide();
        }

        private void OrderHistory_Load_1(object sender, EventArgs e)
        {
            InitializeDataGridView();
            LoadOriginalOrderData();
            DisplayOrderHistory();
        }

        private void dataGridViewOrderHistory_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            
            
        }

        private void dataGridViewOrderHistory_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                // Handle the sorting based on the selected date
                DateTime selectedDate = dateTimePicker1.Value.Date; // Ensure that only the date is considered without the time

                // Check if the column "OrderDate" exists before proceeding
                if (originalOrderDataTable.Columns.Contains("OrderDate"))
                {
                    // Clear existing rows in the DataGridView
                    dataGridViewOrderHistory.Rows.Clear();

                    // Use LINQ to filter and sort the DataTable
                    var filteredRows = originalOrderDataTable.AsEnumerable()
                        .Where(row => row.Field<DateTime>("OrderDate").Date == selectedDate)
                        .OrderBy(row => row.Field<DateTime>("OrderDate"));

                    // Add filtered and sorted rows to the DataGridView
                    foreach (var row in filteredRows)
                    {
                        int orderID = row.Field<int>("OrderID");
                        string customerName = row.Field<string>("CustomerName");
                        decimal totalAmount = row.Field<decimal>("TotalAmount");
                        DateTime orderDate = row.Field<DateTime>("OrderDate");

                        dataGridViewOrderHistory.Rows.Add(orderID, customerName, totalAmount, orderDate);
                    }

                    // Debugging: Output the filtered and sorted rows to console
                    Console.WriteLine($"Filtered and Sorted Rows Count: {dataGridViewOrderHistory.Rows.Count}");
                    foreach (DataGridViewRow row in dataGridViewOrderHistory.Rows)
                    {
                        Console.WriteLine($"OrderID: {row.Cells["OrderID"].Value}, CustomerName: {row.Cells["CustomerName"].Value}, TotalAmount: {row.Cells["TotalAmount"].Value}, OrderDate: {row.Cells["OrderDate"].Value}");
                    }
                }
                else
                {
                    MessageBox.Show("The 'OrderDate' column does not exist in the DataTable.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the search keyword from the textbox
                string searchKeyword = textBox5.Text.Trim().ToLower();

                // Check if the originalOrderDataTable is not null and has the required columns
                if (originalOrderDataTable != null && originalOrderDataTable.Columns.Contains("CustomerName") && originalOrderDataTable.Columns.Contains("OrderID"))
                {
                    // Clear existing rows in the DataGridView
                    dataGridViewOrderHistory.Rows.Clear();

                    // Use LINQ to filter the DataTable based on CustomerName or OrderID
                    var filteredRows = originalOrderDataTable.AsEnumerable()
                        .Where(row => row.Field<string>("CustomerName").ToLower().Contains(searchKeyword) ||
                                      row.Field<int>("OrderID").ToString().Contains(searchKeyword));

                    // Add filtered rows to the DataGridView
                    foreach (var row in filteredRows)
                    {
                        int orderID = row.Field<int>("OrderID");
                        string customerName = row.Field<string>("CustomerName");
                        decimal totalAmount = row.Field<decimal>("TotalAmount");
                        DateTime orderDate = row.Field<DateTime>("OrderDate");

                        dataGridViewOrderHistory.Rows.Add(orderID, customerName, totalAmount, orderDate);
                    }

                    // Debugging: Output the filtered rows to console
                    Console.WriteLine($"Filtered Rows Count: {dataGridViewOrderHistory.Rows.Count}");
                    foreach (DataGridViewRow row in dataGridViewOrderHistory.Rows)
                    {
                        Console.WriteLine($"OrderID: {row.Cells["OrderID"].Value}, CustomerName: {row.Cells["CustomerName"].Value}, TotalAmount: {row.Cells["TotalAmount"].Value}, OrderDate: {row.Cells["OrderDate"].Value}");
                    }
                }
                else
                {
                    MessageBox.Show("The required columns do not exist in the DataTable.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
