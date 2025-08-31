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
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Media;
using System.Globalization;

namespace GamingRigz
{
    public partial class SalesReport : Form
    {
        private DatabaseHelper dbHelper;
        private Chart chart1;
        string connStr = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=stocks;Integrated Security=True;Connect Timeout=30;Encrypt=False;";


        public SalesReport()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=stocks;Integrated Security=True;Connect Timeout=30;Encrypt=False;");

            PopulateChart();

            
            this.Load += SalesReport_Load;
        }
        public class DatabaseHelper
        {
            private string connectionString;

            public DatabaseHelper(string connectionString)
            {
                this.connectionString = connectionString;
            }

            public decimal GetTotalTransactions()
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT COUNT(*) FROM OrderHistory";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        object result = command.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            if (decimal.TryParse(result.ToString(), out decimal totalTransactions))
                            {
                                return totalTransactions;
                            }
                            else
                            {
                                // Handle the case where the conversion to decimal fails
                                // You can log an error, throw an exception, or return a default value
                                return 0;
                            }
                        }
                        else
                        {
                            // Handle the case where the result is null or DBNull.Value
                            // You can log an error, throw an exception, or return a default value
                            return 0;
                        }
                    }
                }
            }


            public decimal GetTotalSales()
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT SUM(TotalAmount) FROM OrderHistory";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        object result = command.ExecuteScalar();
                        return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                    }
                }
            }

            public int GetTotalProductsSold()
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Update the table name if needed, assuming OrderDetails is the correct table
                        string query = "SELECT SUM(Quantity) FROM OrderDetails";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            object result = command.ExecuteScalar();

                            if (result != null && result != DBNull.Value)
                            {
                                return Convert.ToInt32(result);
                            }

                            // Log an error or throw an exception if the result is null or DBNull.Value
                            throw new InvalidOperationException("Query returned null or DBNull.Value.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as appropriate for your application
                    Console.WriteLine($"Error in GetTotalProductsSold: {ex.Message}");
                    return 0; // Return a default value or handle the error accordingly
                }
            }
        }
        private void PopulateChart()
        {

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT DATEPART(month, OrderDate) AS Month, SUM(TotalAmount) AS Sales
                  FROM OrderHistory 
                  GROUP BY DATEPART(month, OrderDate)";

                // Execute query and store results
                var monthlySales = new List<decimal>();
                var months = new List<string>();

                using (SqlConnection connection = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        months.Add(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(reader.GetInt32(0)));
                        // Add sales for month
                        monthlySales.Add(reader.GetDecimal(1));
                    }

                    var series = new Series("Sales");
                    series.Points.DataBindXY(monthlySales, months);
                    series.ChartType = SeriesChartType.Column;
                    series.BorderWidth = 2;
                    series.Color = System.Drawing.Color.DodgerBlue;

                    UpdateTotalTransactionsLabel();
                    UpdateTotalSalesLabel();
                    UpdateTotalProductsSoldLabel();


                }
            }
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


                    }
                }
            }
        }

        private void guna2DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
        }
        private void UpdateChart(DateTime selectedDate)
        {
            using (SqlConnection connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=stocks;Integrated Security=True;Connect Timeout=30;Encrypt=False;"))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("dbo.GetDailySalesAndProfit", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@SelectedDate", selectedDate);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {


                    }
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex == -1)
            {
                GetAllDatesSalesAndProfit();
            }
        }

        private void UpdateTotalSalesLabel()
        {
            decimal totalSales = dbHelper.GetTotalSales();
            txtEarn.Text = totalSales.ToString("C", CultureInfo.GetCultureInfo("en-PH")); // "en-PH" for Philippine Peso
        }

        private void UpdateTotalProductsSoldLabel()
        {
            int totalProductsSold = dbHelper.GetTotalProductsSold();

        }

        private void UpdateTotalTransactionsLabel()
        {
            decimal totalTransactions = dbHelper.GetTotalTransactions();
            txtTrans.Text = totalTransactions.ToString(); // No currency format
        }
    

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2CustomGradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
