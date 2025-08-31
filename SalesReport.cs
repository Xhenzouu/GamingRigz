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

namespace GamingRigz
{
    public partial class SalesReport : Form
    {
        private DatabaseHelper dbHelper;
        SqlConnection con = new SqlConnection(@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = stocks; Integrated Security = True; Connect Timeout = 30; Encrypt=False;");

        public SalesReport()
        {
            InitializeComponent();
            this.Load += SalesReport_Load;
            dataGridView1.ScrollBars = ScrollBars.None;
            dataGridView2.ScrollBars = ScrollBars.None;
            // Initialize DatabaseHelper with connection string
            dbHelper = new DatabaseHelper(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=stocks;Integrated Security=True;Connect Timeout=30;Encrypt=False;");
            dataGridView1.ScrollBars = ScrollBars.None;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Archive archiveForm = new Archive();
            archiveForm.Show();
           
            this.Hide();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Stocks stocksForm = new Stocks();
            stocksForm.Show();
            
            this.Hide();
        }

        private void SalesReport_Load(object sender, EventArgs e)
        {


            // Clear existing columns
            dataGridView2.Columns.Clear();

            UpdateTotalProductsLabel();
            UpdateTotalTransactionsLabel();

            // Initialize DataGridView columns
            dataGridView2.AutoGenerateColumns = false;
            dataGridView2.Columns.Add("Date", "Date");
            dataGridView2.Columns.Add("NetSale", "Net Sale");
            dataGridView2.Columns.Add("Profit", "Profit");

            // Set column data types
            dataGridView2.Columns["Date"].DataPropertyName = "Date";
            dataGridView2.Columns["NetSale"].DataPropertyName = "NetSale";
            dataGridView2.Columns["Profit"].DataPropertyName = "Profit";

            // Set DataGridView column formats
            dataGridView2.Columns["NetSale"].DefaultCellStyle.Format = "N2";
            dataGridView2.Columns["Profit"].DefaultCellStyle.Format = "N2";

            // Set DataGridView column alignments
            dataGridView2.Columns["NetSale"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView2.Columns["Profit"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dataGridView2.Columns["Date"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.Columns["NetSale"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.Columns["Profit"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Set column widths
            dataGridView2.Columns["Date"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView2.Columns["NetSale"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView2.Columns["Profit"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            // Set the maximum width for columns (optional)
            dataGridView2.Columns["Date"].Width = 100;
            dataGridView2.Columns["NetSale"].Width = 100;
            dataGridView2.Columns["Profit"].Width = 100;

            // Set the DataGridView to fill the available space
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView1.Columns.Clear();

            UpdateTotalProductsLabel();
            UpdateTotalTransactionsLabel();

            // Initialize DataGridView columns
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.Columns.Add("Date", "Date");
            dataGridView1.Columns.Add("NetSale", "Net Sale");
            dataGridView1.Columns.Add("Profit", "Profit");

            // Set column data types
            dataGridView1.Columns["Date"].DataPropertyName = "Date";
            dataGridView1.Columns["NetSale"].DataPropertyName = "NetSale";
            dataGridView1.Columns["Profit"].DataPropertyName = "Profit";

            // Set DataGridView column formats
            dataGridView1.Columns["NetSale"].DefaultCellStyle.Format = "N2";
            dataGridView1.Columns["Profit"].DefaultCellStyle.Format = "N2";

            // Set DataGridView column alignments
            dataGridView1.Columns["NetSale"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns["Profit"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dataGridView1.Columns["Date"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["NetSale"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Profit"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Set column widths
            dataGridView1.Columns["Date"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns["NetSale"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns["Profit"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            // Set the maximum width for columns (optional)
            dataGridView1.Columns["Date"].Width = 100;
            dataGridView1.Columns["NetSale"].Width = 100;
            dataGridView1.Columns["Profit"].Width = 100;

            // Set the DataGridView to fill the available space
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            GetDailySalesAndProfit();
            GetAllDatesSalesAndProfit();
        }

        private void GetDailySalesAndProfit()
        {
            using (SqlConnection connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=stocks;Integrated Security=True;Connect Timeout=30;Encrypt=False;"))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dbo.GetDailySalesAndProfit", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Pass the selected date from the DateTimePicker as a parameter to the stored procedure
                    command.Parameters.AddWithValue("@SelectedDate", guna2DateTimePicker1.Value);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        // Sort the DataTable based on the 'Date' column
                        DataView dv = dt.DefaultView;
                        dv.Sort = "Date ASC";
                        dt = dv.ToTable();

                        dataGridView1.DataSource = dt;

                    }
                }
            }
        }

        private void GetAllDatesSalesAndProfit()
        {
            using (SqlConnection connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=stocks;Integrated Security=True;Connect Timeout=30;Encrypt=False;"))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dbo.GetDailySalesAndProfitAllDates", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        // Sort the DataTable based on the 'Date' column
                        DataView dv = dt.DefaultView;
                        dv.Sort = "Date ASC";
                        dt = dv.ToTable();

                        dataGridView2.DataSource = dt;
                    }
                }
            }
        }

        private void guna2DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            GetDailySalesAndProfit();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if the upper-left cell is clicked
            if (e.RowIndex == -1 && e.ColumnIndex == -1)
            {
                // Load aggregated data for all dates when the upper-left cell is clicked
                GetAllDatesSalesAndProfit();
            }
        }

        private void UpdateTotalProductsLabel()
        {
            // Use the dbHelper instance to get total products
            int totalProducts = dbHelper.GetTotalProductsInStock();

            // Assuming 'totalProductsLabel' is the name of your label
            totalProductsLabel.Text = totalProducts.ToString();
        }

        private void totalProductsLabel_Click(object sender, EventArgs e)
        {

        }

        private void UpdateTotalTransactionsLabel()
        {
            // Use the dbHelper instance to get total transactions
            int totalTransactions = dbHelper.GetTotalTransactions();

            // Assuming 'totalTransactionsLabel' is the name of your label
            totalTransactionsLabel.Text = totalTransactions.ToString();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
