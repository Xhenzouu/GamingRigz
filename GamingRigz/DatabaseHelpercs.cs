using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

public class DatabaseHelper
{
    private string connectionString;

    public DatabaseHelper(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public int GetTotalProductsInStock()
    {
        int totalProducts = 0;

        using (SqlConnection connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=stocks;Integrated Security=True;Connect Timeout=30;Encrypt=False;"))
        {
            connection.Open();

            string query = "SELECT COUNT(*) FROM stocks";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                totalProducts = (int)command.ExecuteScalar();
            }
        }

        return totalProducts;
    }

    public int GetTotalTransactions()
    {
        int totalTransactions = 0;

        using (SqlConnection connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=stocks;Integrated Security=True;Connect Timeout=30;Encrypt=False;"))
        {
            connection.Open();

            string query = "SELECT COUNT(*) FROM orderhistory";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                totalTransactions = (int)command.ExecuteScalar();
            }
        }

        return totalTransactions;
    }
}
