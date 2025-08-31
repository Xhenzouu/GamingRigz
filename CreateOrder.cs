    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Drawing.Printing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Data.SqlClient;
    using System.Windows.Forms;
    using System.DirectoryServices;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
    using System.Globalization;
    using System.IO;
    using System.Drawing.Imaging;
    using System.Diagnostics;

namespace GamingRigz
{
    public partial class orderingForm : Form
    {
        SqlConnection con = new SqlConnection(@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = stocks; Integrated Security = True; Connect Timeout = 30; Encrypt=False;");
        private DataTable cartDataTable;
        private List<CartItem> selectedItems;
        private PrintDocument printDocument;
        private bool isFirstPage = true;
        private bool isPrintingInProgress = false;
        private bool columnsWidthSet = false;
        private bool showedMessageBox = false;

        public orderingForm()
        {
            InitializeComponent();
            selectedItems = new List<CartItem>();
            cbCat.Items.AddRange(new string[] { "Chassis", "Processor", "Motherboard", "GraphicsCard", "Memory", "Storage", "PowerSupply" });
            btClear.Click += btClear_Click;
            pRnt.Click += pRnt_Click;
            printDocument = new PrintDocument();
            dataGridView1.ScrollBars = ScrollBars.None;
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
            tbAmountPaid.TextChanged += tbAmountPaid_TextChanged;                    
            printDocument.PrintPage += new PrintPageEventHandler(OnPrintPage);
        }

        private void AddSelectedItemToCart()
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    string selectedProductName = dataGridView1.SelectedRows[0].Cells["Product Name"].Value.ToString();
                    decimal selectedPrice = Convert.ToDecimal(dataGridView1.SelectedRows[0].Cells["Price"].Value);
                    CartItem existingItem = selectedItems.Find(item => item.ProductName == selectedProductName);

                    if (existingItem != null)
                    {
                        // Check if adding one more item exceeds the quantity limit
                        if (existingItem.Quantity < 40)
                        {
                            existingItem.Quantity++;
                            existingItem.UpdateDateOfPurchase();
                            UpdateQuantityTextBox();
                            UpdateListBox();
                        }
                        else
                        {
                            MessageBox.Show($"Quantity limit (40) reached for {selectedProductName}. You cannot add more.");
                        }
                    }
                    else
                    {
                        // Check if adding a new item exceeds the quantity limit
                        if (selectedItems.Sum(item => item.Quantity) < 40)
                        {
                            CartItem newItem = new CartItem
                            {
                                ProductName = selectedProductName,
                                Price = selectedPrice,
                                DateOfPurchase = DateTime.Now
                            };

                            selectedItems.Add(newItem);

                            // Update TotalPrice for the selected items
                            foreach (var item in selectedItems)
                            {
                                item.UpdateTotalPrice();
                            }

                            UpdateQuantityTextBox();
                            UpdateListBox();
                        }
                        else
                        {
                            MessageBox.Show($"Quantity limit (40) reached. You cannot add more items.");
                        }
                    }
                }
            }
            catch (NullReferenceException )
            {
                MessageBox.Show("Cannot execute action.",
                                          "Warning!",
                                          MessageBoxButtons.OK,
                                          MessageBoxIcon.Warning);
            }
        }

        private void Form_Load(object sender, EventArgs e)
        {
            {
                disp_data();

                // Set the initial quantity based on the selected product
                UpdateQuantityTextBox();
            }

        }
        private void UpdateQuantityTextBoxAutomatically()
        {
            // Check if there is a selected row in the dataGridView1
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Get the selected product name from the DataGridView
                string selectedProductName = dataGridView1.SelectedRows[0].Cells["Product Name"].Value.ToString();

                // Find the existing item in the selected items
                CartItem existingItem = selectedItems.Find(item => item.ProductName == selectedProductName);

                // Update the TextBox with the quantity of the selected product
                tbQT.Text = existingItem != null ? existingItem.Quantity.ToString() : "0";

                // Update TotalPrice for the existing item
                existingItem?.UpdateTotalPrice();
            }
            else
            {
                // No row is selected, set tbQT to 0
                tbQT.Text = "0";
            }
        }
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            // Update tbQT automatically when the selected row changes
            UpdateQuantityTextBoxAutomatically();
        }
        private void UpdateQuantityTextBox()
        {
            // Check if there is a selected row in the dataGridView1
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Get the selected product name from the DataGridView
                string selectedProductName = dataGridView1.SelectedRows[0].Cells["Product Name"].Value.ToString();

                // Find the existing item in the selected items
                CartItem existingItem = selectedItems.Find(item => item.ProductName == selectedProductName);

                // Update the TextBox with the quantity of the selected product
                tbQT.Text = existingItem != null ? existingItem.Quantity.ToString() : "0";

                // Calculate the total price for the existing item
                existingItem?.UpdateTotalPrice();
            }
            else
            {
                // No row is selected, set tbQT to 0
                tbQT.Text = "0";
            }
        }


        public void disp_data()
        {
            // Set up connection string
            string connectionString = @"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = stocks; Integrated Security = True; Connect Timeout = 30; Encrypt=False;";

            // Create a SqlConnection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Open the connection
                connection.Open();

                // Create a SqlCommand to select data from the Stocks table
                string selectQuery = "SELECT [Product Name], Category, Price, Quantity FROM dbo.Stocks";
                using (SqlCommand command = new SqlCommand(selectQuery, connection))
                {
                    // Create a DataTable to hold the results
                    cartDataTable = new DataTable();

                    // Create a SqlDataAdapter to fill the DataTable
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        // Fill the DataTable with the data from the Stocks table
                        adapter.Fill(cartDataTable);
                    }

                    // Bind the DataTable to the DataGridView for displaying stock data
                    dataGridView1.DataSource = cartDataTable;
                }

                // Create a SqlCommand to delete existing data from Cart
                string deleteQuery = "DELETE FROM dbo.Cart";
                using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                {
                    // Execute the delete query
                    deleteCommand.ExecuteNonQuery();
                }

                // Create a SqlCommand to insert data from Stocks to Cart only for available stocks
                string insertQuery = "INSERT INTO dbo.Cart ([Product Name], Category, Price, Quantity) SELECT [Product Name], Category, Price, Quantity FROM dbo.Stocks";
                using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                {
                    // Execute the insert query
                    insertCommand.ExecuteNonQuery();
                }

                // Create a SqlCommand to select data from the Cart table
                string selectCartQuery = "SELECT [Product Name], Category, Price, Quantity FROM dbo.Cart";
                using (SqlCommand cartCommand = new SqlCommand(selectCartQuery, connection))
                {
                    // Create a DataTable to hold the results
                    cartDataTable = new DataTable();

                    // Create a SqlDataAdapter to fill the DataTable
                    using (SqlDataAdapter cartAdapter = new SqlDataAdapter(cartCommand))
                    {
                        // Fill the DataTable with the data from the Cart table
                        cartAdapter.Fill(cartDataTable);
                    }

                    // Bind the DataTable to the DataGridView for displaying cart data
                    dataGridView1.DataSource = cartDataTable;
                }
            }

            con.Close();
            dataGridView1.Columns[0].Width = 215;
            dataGridView1.Columns[1].Width = 110;  // or whatever width works well for abbrev
            dataGridView1.Columns[2].Width = 80;

            // Populate the ComboBox with distinct categories
            PopulateComboBox();
        }

        private void PopulateComboBox()
        {
            // Get distinct categories from the DataTable
            var distinctCategories = cartDataTable.AsEnumerable()
                .Select(row => row.Field<string>("Category"))
                .Distinct()
                .OrderBy(category => category) // Sort the categories
                .ToList();

            // Clear existing items and add distinct categories to the ComboBox
            cbCat.Items.Clear();
            cbCat.Items.AddRange(distinctCategories.ToArray());
        }

        private async void pRnt_Click(object sender, EventArgs e)
        {
            if (isPrintingInProgress ||
                string.IsNullOrEmpty(tbName.Text) ||
                string.IsNullOrEmpty(tbContactNumber.Text) ||
                string.IsNullOrEmpty(tbAddress.Text) ||
                string.IsNullOrEmpty(tbEmail.Text) ||
                string.IsNullOrEmpty(tbAmountPaid.Text))
            {
                return;
            }

            isPrintingInProgress = true;

            try
            {
                
                string name = tbName.Text;
                string contactNumber = tbContactNumber.Text;
                string address = tbAddress.Text;
                string email = tbEmail.Text;

                if (!IsNumeric (contactNumber))
                {
                    MessageBox.Show("Invalid Contact Number. Please enter numeric values only.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!IsValidEmail(email))
                {
                    MessageBox.Show("Invalid Email. Please enter a valid email address.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Add user information to the ListBox
                lbList.Items.Add($"{"Name:",-25} {name}");
                lbList.Items.Add($"{"Contact Number:",-25} {contactNumber}");
                lbList.Items.Add($"{"Address:",-25} {address}");
                lbList.Items.Add($"{"Email:",-25} {email}");

                // Add a separator line
                lbList.Items.Add(new string('-', 60));

                // Update the ListBox with selected items
                UpdateListBox();

                List<CartItem> orderedItems = GetOrderedItems();

                // Validate the Amount Paid
                decimal amountPaid;
                if (decimal.TryParse(tbAmountPaid.Text, out amountPaid))
                {
                    decimal totalPrices = orderedItems.Sum(item => item.CalculateTotalPrice());

                    if (amountPaid >= totalPrices)
                    {
                        // User entered a valid amount, proceed with printing

                        // Update stock quantities in the database
                        UpdateStockQuantities(orderedItems);

                        await RecordOrderHistoryAsync(name, orderedItems);

                        isFirstPage = false;

                        // Ensure that the ListBox is updated before printing
                        await Task.Run(() => { });

                        // Use await to asynchronously show the print preview on the UI thread
                        await Task.Run(() =>
                        {
                            BeginInvoke((Action)(() =>
                            {
                                using (PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog())
                                {
                                    printPreviewDialog.Document = printDocument;
                                    printPreviewDialog.ShowDialog();
                                }
                            }));
                        });
                    }
                    else
                    {
                        if (!showedMessageBox)
                        {
                            MessageBox.Show("Amount Paid must be equal or greater than Total Price.", "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            showedMessageBox = true;
                        }
                    }
                }
                else
                {
                    if (!showedMessageBox)
                    {
                        MessageBox.Show("Invalid Amount Paid. Please enter a valid numeric value.", "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        showedMessageBox = true;
                    }
                }
            }


            finally
            {
                if (showedMessageBox)
                {
                    showedMessageBox = false;
                }
                
                // Reset the flag for the next print operation
                isFirstPage = true;

                isPrintingInProgress = false;

                // Reset the showedMessageBox flag
                showedMessageBox = false;
            }


        }

        private bool IsNumeric(string input)
        {
            return input.All (char.IsDigit);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        private async Task RecordOrderHistoryAsync(string customerName, List<CartItem> orderedItems)
        {
            // Insert a new record into the order history table
            using (SqlConnection connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=stocks;Integrated Security=True;Connect Timeout=30;Encrypt=False;"))
            {
                await connection.OpenAsync();

                decimal totalAmount = orderedItems.Sum(item => item.CalculateTotalPrice());

                string insertQuery = $"INSERT INTO dbo.orderhistory (CustomerName, TotalAmount, OrderDate) VALUES ('{customerName}', {totalAmount}, GETDATE())";

                using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                {
                    await insertCommand.ExecuteNonQueryAsync();
                }
            }
        }


        private void UpdateStockQuantities(List<CartItem> orderedItems)
        {
            using (SqlConnection connection = new SqlConnection(@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = stocks; Integrated Security = True; Connect Timeout = 30; Encrypt=False;"))
            {
                connection.Open();

                foreach (CartItem orderedItem in orderedItems)
                {
                    // Assuming there's a column named 'Quantity' in dbo.Stocks table
                    string updateQuery = $"UPDATE dbo.Stocks SET Quantity = Quantity - {orderedItem.Quantity} WHERE [Product Name] = '{orderedItem.ProductName}'";

                    using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.ExecuteNonQuery();
                    }
                }

            }
        }

        private List<CartItem> GetOrderedItems()
        {
            List<CartItem> orderedItems = new List<CartItem>();

            foreach (CartItem item in selectedItems)
            {
                if (item.Quantity > 0)
                {
                    orderedItems.Add(item);
                }
            }

            return selectedItems.Where(item => item.Quantity > 0).ToList();
        }

        private void guna2CustomGradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void orderingForm_Load(object sender, EventArgs e)
        {
            disp_data();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void UpdateListBox()
        {
            // Clear the ListBox
            lbList.Items.Clear();
            decimal totalPrices = 0;

            // Add user information to the ListBox
            string name = tbName.Text;
            string contactNumber = tbContactNumber.Text;
            string address = tbAddress.Text;
            string email = tbEmail.Text;

            lbList.Items.Add($"{"Name:",-25} {name}");
            lbList.Items.Add($"{"Contact Number:",-25} {contactNumber}");
            lbList.Items.Add($"{"Address:",-25} {address}");
            lbList.Items.Add($"{"Email:",-25} {email}");

            // Add a separator line
            lbList.Items.Add(new string('-', 60));

            // Check if selectedItems is not empty
            if (selectedItems.Any())
            {
                foreach (CartItem item in selectedItems)
                {
                    // Update the TotalPrice
                    item.UpdateTotalPrice();

                    // Only add items with Quantity greater than 0
                    if (item.Quantity > 0)
                    {
                        string itemText = $"{item.ProductName,-25} - {item.Currency}{item.Price,10:F2} - Qty: {item.Quantity} - Date: {item.DateOfPurchase.ToString("yyyy-MM-dd HH:mm:ss")}";
                        totalPrices += item.CalculateTotalPrice();

                        // Add the item to the ListBox based on quantity
                        for (int i = 0; i < item.Quantity; i++)
                        {
                            lbList.Items.Add($"{item.ProductName,-25} - {item.Currency}{item.Price,10:F2}");
                        }
                    }
                }
            }

            // Display the total price with proper spacing
            lbList.Items.Add(new string('-', 60));

            // Use FirstOrDefault to get the first item or null if the list is empty
            CartItem firstItem = selectedItems.FirstOrDefault();
            if (firstItem != null)
            {
                lbList.Items.Add($"{"Total Price:",-25} {firstItem.Currency}{totalPrices,10:F2}");
            }

            // Set the TopIndex to the maximum index
            lbList.TopIndex = lbList.Items.Count - 1;
        }

        private class CartItem
        {
            public string ProductName { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public string Currency { get; } = "₱"; // Set the currency to pesos
            public decimal TotalPrice { get; private set; } // Allow private setting
            public DateTime DateOfPurchase { get; set; }

            // Update the constructor to initialize Quantity to 1
            public CartItem()
            {
                Quantity = 1;
            }

            // Add a method to update the TotalPrice property
            public void UpdateTotalPrice()
            {
                TotalPrice = CalculateTotalPrice();
            }

            // Update the CalculateTotalPrice method to return the calculated total price
            public decimal CalculateTotalPrice()
            {
                return Price * Quantity;
            }

            // Override ToString to provide a formatted string for ListBox display
            public override string ToString()
            {
                return $"{ProductName,-25} - {Currency}{CalculateTotalPrice(),10:F2}";
            }
            public void UpdateDateOfPurchase()
            {
                DateOfPurchase = DateTime.Now;
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Check if cartDataTable is initialized
            if (cartDataTable != null)
            {
                // Filter the DataTable based on the entered ProductName and selected category
                DataView dv = cartDataTable.DefaultView;
                string filterExpression = $"[Product Name] LIKE '%{tbPN.Text}%'";

                // Check if a category is selected
                if (cbCat.SelectedItem != null)
                {
                    filterExpression += $" AND Category = '{cbCat.SelectedItem.ToString()}'";
                }

                dv.RowFilter = filterExpression;

                // Sort the DataView by ProductName
                dv.Sort = "[Product Name] ASC";

                // Update the DataGridView with the filtered and sorted DataView
                dataGridView1.DataSource = dv.ToTable();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Login loginForm = new Login();
            loginForm.Show();

            this.Hide();
        }

        private void cbCat_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1_TextChanged(sender, e);
        }

        private void btPlus_Click(object sender, EventArgs e)
        {

        }

        private void btMinus_Click(object sender, EventArgs e)
        {

        }

        private void btPlus_Click_1(object sender, EventArgs e)
        {
            // Call the method to add the selected item to the cart
            AddSelectedItemToCart();
        }

        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {
            // Get the selected row from the DataGridView
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Get the selected product details
                string productName = dataGridView1.SelectedRows[0].Cells["Product Name"].Value.ToString();

                // Find the existing item in the selected items
                CartItem existingItem = selectedItems.Find(item => item.ProductName == productName);

                if (existingItem != null)
                {
                    // Decrement the quantity for the existing item
                    if (existingItem.Quantity > 1)
                    {
                        existingItem.Quantity--;

                        // Update the TextBox with the new quantity
                        UpdateQuantityTextBox();

                        // Update TotalPrice for the existing item
                        existingItem.UpdateTotalPrice();

                        // Update the ListBox
                        UpdateListBox();
                    }
                    else
                    {
                        // If quantity is 1, remove the item from the selected items
                        selectedItems.Remove(existingItem);

                        // Update the TextBox with the new quantity
                        UpdateQuantityTextBox();

                        // Update the ListBox
                        UpdateListBox();
                    }
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

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

                // Check if quantity is a valid decimal
                if (decimal.TryParse(row.Cells["Quantity"].Value?.ToString(), out quantity))
                {
                    // Apply color coding based on quantity
                    if (quantity >= 1 && quantity <= 10)
                    {
                        e.CellStyle.BackColor = Color.Red;
                    }
                    else if (quantity >= 11 && quantity <= 20)
                    {
                        e.CellStyle.BackColor = Color.Yellow;
                    }
                    else if (quantity >= 21 && quantity <= 40)
                    {
                        e.CellStyle.BackColor = Color.LimeGreen;
                    }
                    // Add more conditions if needed
                    else
                    {
                        // Default color for quantities not in the specified ranges
                        e.CellStyle.BackColor = Color.White;
                    }
                }
                else
                {
                    // Default color for invalid quantity
                    e.CellStyle.BackColor = Color.White;
                }
            }
            else if (e.ColumnIndex == dataGridView1.Columns["Category"].Index)
            {
                // Apply color coding based on category for the "Category" column
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
            // Fill the "Product Name" column
            dataGridView1.Columns["Product Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // Set other columns to a fixed width of 50
            dataGridView1.Columns["Category"].Width = 50;
            dataGridView1.Columns["Quantity"].Width = 50;
            dataGridView1.Columns["Price"].Width = 50;

            // Center-align the column headers
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Prompt the user to confirm logging out
            DialogResult result = MessageBox.Show("Are you sure you want to log out?", "Confirm Log Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Open the login form and close the current form
                Login loginForm = new Login();
                loginForm.Show();
                this.Hide();
            }
            // If the user clicks 'No', do nothing
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // Prompt the user to confirm exiting the application
            DialogResult result = MessageBox.Show("Are you sure you want to exit?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Close the application
                Application.Exit();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            OrderHistory orderhistoryForm = new OrderHistory();
            orderhistoryForm.Show();

            // Optionally, close or hide the login form
            this.Hide();
        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {

        }

        private void btClear_Click(object sender, EventArgs e)
        {

            tbName.Text = string.Empty;
            tbContactNumber.Text = string.Empty;
            tbAddress.Text = string.Empty;
            tbEmail.Text = string.Empty;

            // Clear the ListBox and reset selectedItems
            lbList.Items.Clear();
            selectedItems.Clear();

            // Optionally, update other UI elements or perform additional actions as needed

            // Update the ListBox
            UpdateListBox();

        }

        private void lbList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void OnPrintPage(object sender, PrintPageEventArgs e)
        {
            // Set up the font and brush for drawing text
            Font font = new Font("Arial", 10);
            Brush brush = Brushes.Black;

            Image logo = Image.FromFile("D:\\Download\\gr-modified.png");

            // Calculate the center position for drawing the image
            float xCenter = e.MarginBounds.Left + (e.MarginBounds.Width - 340) / 2;
            float y = e.MarginBounds.Top;  // Set y to the top

            // Draw user information
            DrawText(e.Graphics, font, brush, $"Name: {tbName.Text}", e, ref y);
            DrawText(e.Graphics, font, brush, $"Contact Number: {tbContactNumber.Text}", e, ref y);
            DrawText(e.Graphics, font, brush, $"Address: {tbAddress.Text}", e, ref y);
            DrawText(e.Graphics, font, brush, $"Email: {tbEmail.Text}", e, ref y);

            // Draw a separator line
            DrawLine(e.Graphics, xCenter, e, ref y);

            // Draw selected items
            foreach (CartItem item in selectedItems)
            {
                item.UpdateTotalPrice();
                if (item.Quantity > 0)
                {
                    string itemText = $"{item.ProductName} - {item.Currency}{item.Price * item.Quantity:F2}";
                    DrawText(e.Graphics, font, brush, itemText, e, ref y);
                }
            }

            // Draw a separator line
            DrawLine(e.Graphics, xCenter, e, ref y);

            // Draw total price
            decimal totalPrices = selectedItems.Sum(item => item.CalculateTotalPrice());
            string totalPriceText = $"Total Price: {selectedItems.First().Currency}{totalPrices:F2}";
            DrawText(e.Graphics, font, brush, totalPriceText, e, ref y);

            // Draw a separator line
            DrawLine(e.Graphics, xCenter, e, ref y);

            // Draw Amount Paid
            decimal amountPaid;
            if (decimal.TryParse(tbAmountPaid.Text, out amountPaid))
            {
                string amountPaidText = $"Amount Paid: {selectedItems.First().Currency}{amountPaid:F2}";
                DrawText(e.Graphics, font, brush, amountPaidText, e, ref y);
            }
            else
            {
                DrawText(e.Graphics, font, brush, "Amount Paid: Invalid input", e, ref y);
            }

            // Calculate and draw Change
            decimal change = amountPaid - totalPrices;
            string changeText = $"Change: {selectedItems.First().Currency}{change:F2}";
            DrawText(e.Graphics, font, brush, changeText, e, ref y);

            // Draw Date
            string dateText = $"Date: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
            DrawText(e.Graphics, font, brush, dateText, e, ref y);

            // Adjust the y position before drawing the logo
            y += 10;

            // Draw the shop logo
            DrawImage(e.Graphics, logo, e, ref y, 340, 340, xCenter);

            // Indicate that there are no more pages to print
            e.HasMorePages = false;
        }


        private void DrawText(Graphics graphics, Font font, Brush brush, string text, PrintPageEventArgs e, ref float y)
        {
            float lineHeight = 20; // Set the desired line height

            graphics.DrawString(text, font, brush, e.MarginBounds.Left, y);
            y += lineHeight; // Use a consistent line height

            // Check if the y position is near the bottom, and if so, reset it to the top
            if (y + lineHeight > e.MarginBounds.Bottom)
            {
                y = e.MarginBounds.Top;
            }
        }

        private void DrawLine(Graphics graphics, float x, PrintPageEventArgs e, ref float y)
        {
            graphics.DrawLine(Pens.Black, x, y, e.MarginBounds.Right, y);
            y += 10;
        }

        private void DrawImage(Graphics graphics, Image image, PrintPageEventArgs e, ref float y, int width, int height, float x)
        {
            // Set the opacity to 50%
            float opacity = 0.5f;
            ColorMatrix colorMatrix = new ColorMatrix { Matrix33 = opacity };
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            // Draw the image with adjusted opacity and size
            graphics.DrawImage(image, new Rectangle((int)x, (int)y, width, height),
                               0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);

            // Adjust the y position after drawing the image
            y += height + 10; // You can adjust the spacing as needed
        }


        private void tbQT_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void tbAmountPaid_TextChanged(object sender, EventArgs e)
        {
            bool isAmountPaidValid = !string.IsNullOrEmpty(tbAmountPaid.Text);
            pRnt.Enabled = isAmountPaidValid;
        }

        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {           
            string url = "https://grpricelist.my.canva.site/grpricelist";           
            System.Diagnostics.Process.Start(url);
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {

        }
    }
}