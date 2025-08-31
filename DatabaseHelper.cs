using System;

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

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM stocks", connection))
            {
                totalProducts = (int)command.ExecuteScalar();
            }
        }

        return totalProducts;
    }

    // Add more methods for other database operations as needed
}
