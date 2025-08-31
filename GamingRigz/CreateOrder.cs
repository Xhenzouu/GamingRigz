    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Drawing.Printing;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Data.SqlClient;
    using System.Windows.Forms;
    using System.Drawing.Imaging;
using System.Windows.Controls;

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
        private bool showMessagebox = true;
        private bool showedNameErrorMessage = false;
        private bool showMessageboxName = true;
        private bool showMessageboxAmountPaid = true;
        private bool showMessageboxContactNumber = true;
        private bool hasErrors = false;
        private bool showMessageboxLocal = true;
        private bool isNameErrorShown = false;
        private bool isContactNumberErrorShown = false;
        private bool isEmailErrorShown = false;
        private bool isAmountPaidErrorShown = false;
        private bool isPrintConfirmationShown = false;
        private bool hasPrintConfirmationBeenShown = false;
        public orderingForm()
        {
            InitializeComponent();
            selectedItems = new List<CartItem>();
            cbCat.Items.AddRange(new string[] { "Chassis", "Processor", "Motherboard", "GraphicsCard", "Memory", "Storage", "PowerSupply" });
            btClear.Click += btClear_Click;
            pRnt.Click += pRnt_Click;
            printDocument = new PrintDocument();
            dataGridView1.ScrollBars = ScrollBars.Vertical;
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
            tbAmountPaid.TextChanged += tbAmountPaid_TextChanged;
            printDocument.PrintPage += new PrintPageEventHandler(OnPrintPage);
            dataGridView1.ReadOnly = true;
            tbContactNumber.TextChanged += tbContactNumber_TextChanged;
            tbContactNumber.KeyPress += tbContactNumber_KeyPress;
            tbContactNumber.KeyDown += tbContactNumber_KeyDown;
            dataGridView1.CellClick += dataGridView1_CellClick;

        }

        private void AddSelectedItemToCart()
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    string selectedProductName = dataGridView1.SelectedRows[0].Cells["Product Name"].Value.ToString();
                    decimal selectedPrice = Convert.ToDecimal(dataGridView1.SelectedRows[0].Cells["Price"].Value);

                    // Parse the quantity from the tbQT textbox
                    if (int.TryParse(tbQT.Text, out int enteredQuantity))
                    {
                        string query = "SELECT [Quantity] FROM [dbo].[stocks] WHERE [Product Name] = @ProductName";

                        using (SqlCommand command = new SqlCommand(query, con))
                        {
                            command.Parameters.AddWithValue("@ProductName", selectedProductName);

                            con.Open();
                            decimal availableQuantity = (decimal)command.ExecuteScalar();
                            con.Close();

                            if (enteredQuantity > 0 && enteredQuantity <= availableQuantity)
                            {
                                CartItem existingItem = selectedItems.Find(item => item.ProductName == selectedProductName);

                                if (existingItem != null)
                                {
                                    existingItem.Quantity = enteredQuantity;
                                    existingItem.UpdateDateOfPurchase();
                                }
                                else
                                {
                                    CartItem newItem = new CartItem
                                    {
                                        ProductName = selectedProductName,
                                        Price = selectedPrice,
                                        Quantity = enteredQuantity,
                                        DateOfPurchase = DateTime.Now
                                    };

                                    selectedItems.Add(newItem);
                                }

                                UpdateQuantityTextBox();
                                UpdateListBox();
                            }
                            else
                            {
                                MessageBox.Show($"Invalid quantity. Please enter a quantity between 1 and {availableQuantity}.");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid quantity. Please enter a valid number.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void Form_Load(object sender, EventArgs e)
        {
            {
                disp_data();

                UpdateQuantityTextBox();
            }

        }
        private void UpdateQuantityTextBoxAutomatically()
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                string selectedProductName = dataGridView1.SelectedRows[0].Cells["Product Name"].Value.ToString();

                CartItem existingItem = selectedItems.Find(item => item.ProductName == selectedProductName);

                tbQT.Text = existingItem != null ? existingItem.Quantity.ToString() : "0";

                existingItem?.UpdateTotalPrice();
            }
            else
            {
                tbQT.Text = "0";
            }
        }
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            UpdateQuantityTextBoxAutomatically();
        }
        private void UpdateQuantityTextBox()
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                string selectedProductName = dataGridView1.SelectedRows[0].Cells["Product Name"].Value.ToString();

                CartItem existingItem = selectedItems.Find(item => item.ProductName == selectedProductName);

                tbQT.Text = existingItem != null ? existingItem.Quantity.ToString() : "0";

                existingItem?.UpdateTotalPrice();
            }
            else
            {
                tbQT.Text = "0";
            }
        }


        public void disp_data()
        {
            string connectionString = @"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = stocks; Integrated Security = True; Connect Timeout = 30; Encrypt=False;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string selectQuery = "SELECT [Product Name], Category, Price, Quantity FROM dbo.Stocks";
                using (SqlCommand command = new SqlCommand(selectQuery, connection))
                {
                    cartDataTable = new DataTable();

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(cartDataTable);
                    }

                    dataGridView1.DataSource = cartDataTable;
                }

                string deleteQuery = "DELETE FROM dbo.Cart";
                using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                {
                    deleteCommand.ExecuteNonQuery();
                }

                string insertQuery = "INSERT INTO dbo.Cart ([Product Name], Category, Price, Quantity) SELECT [Product Name], Category, Price, Quantity FROM dbo.Stocks";
                using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                {
                    insertCommand.ExecuteNonQuery();
                }

                string selectCartQuery = "SELECT [Product Name], Category, Price, Quantity FROM dbo.Cart";
                using (SqlCommand cartCommand = new SqlCommand(selectCartQuery, connection))
                {
                    cartDataTable = new DataTable();

                    using (SqlDataAdapter cartAdapter = new SqlDataAdapter(cartCommand))
                    {
                        cartAdapter.Fill(cartDataTable);
                    }

                    dataGridView1.DataSource = cartDataTable;
                }
            }

            con.Close();
            dataGridView1.Columns[0].Width = 215;
            dataGridView1.Columns[1].Width = 110;  // or whatever width works well for abbrev
            dataGridView1.Columns[2].Width = 80;

            PopulateComboBox();
        }

        private void PopulateComboBox()
        {
            var distinctCategories = cartDataTable.AsEnumerable()
                .Select(row => row.Field<string>("Category"))
                .Distinct()
                .OrderBy(category => category) // Sort the categories
                .ToList();

            cbCat.Items.Clear();
            cbCat.Items.AddRange(distinctCategories.ToArray());
        }

        private async void pRnt_Click(object sender, EventArgs e)
        {
            if (isPrintingInProgress)
            {
                return;
            }

            showMessageboxName = true;
            showMessageboxContactNumber = true;
            showMessagebox = true;
            hasErrors = false;

            if (string.IsNullOrEmpty(tbName.Text) ||
                string.IsNullOrEmpty(tbContactNumber.Text) ||
                string.IsNullOrEmpty(tbAddress.Text) ||
                string.IsNullOrEmpty(tbEmail.Text) ||
                string.IsNullOrEmpty(tbAmountPaid.Text))
            {
                if (showMessageboxLocal)
                {
                    MessageBox.Show("All fields must be filled out.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    showMessageboxLocal = false;
                }
                return;
            }

            ValidateName();
            ValidateContactNumber();
            ValidateEmail();
            ValidateAddress();
            ValidateAmountPaid();

            if (hasErrors)
            {
                showMessageboxName = true;
                showMessageboxContactNumber = true;
                showMessagebox = true;
                return;
            }

            isPrintingInProgress = true;

            try
            {
                string name = tbName.Text;
                string contactNumber = tbContactNumber.Text;
                string address = tbAddress.Text;
                string email = tbEmail.Text;

                lbList.Items.Add($"{"Name:",-25} {name}");
                lbList.Items.Add($"{"Contact Number:",-25} {contactNumber}");
                lbList.Items.Add($"{"Address:",-25} {address}");
                lbList.Items.Add($"{"Email:",-25} {email}");

                lbList.Items.Add(new string('-', 60));

                UpdateListBox();

                List<CartItem> orderedItems = GetOrderedItems();

                decimal amountPaid;
                if (decimal.TryParse(tbAmountPaid.Text, out amountPaid))
                {
                    decimal totalPrices = orderedItems.Sum(item => item.CalculateTotalPrice());

                    if (amountPaid >= totalPrices)
                    {
                        UpdateStockQuantities(orderedItems);

                        await RecordOrderHistoryAsync(name, orderedItems);

                        isFirstPage = false;

                        await Task.Run(() => { });

                        DialogResult confirmationResult = MessageBox.Show("Print order?", "Confirm Print", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (confirmationResult == DialogResult.Yes)
                        {
                            // Show preview if user clicked Yes
                            using (PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog())
                            {
                                printPreviewDialog.Document = printDocument;
                                printPreviewDialog.ShowDialog();

                                // Print after preview closes
                                printDocument.Print();
                            }
                        }
                        else
                        {
                            return;
                        }

                    }
                    else
                    {
                        if (showMessageboxLocal)
                        {
                            MessageBox.Show("Amount Paid must be equal or greater than Total Price.", "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            showMessageboxLocal = false;
                        }
                    }
                }
                else
                {
                    if (showMessageboxLocal)
                    {
                        MessageBox.Show("Invalid Amount Paid. Please enter a valid numeric value.", "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        showMessageboxLocal = false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (showMessageboxLocal)
                {
                    ShowErrorMessage(ex.Message);
                    showMessageboxLocal = false;
                }
            }
            finally
            {
                isPrintingInProgress = false;

                tbName.Clear();
                tbContactNumber.Clear();
                tbAddress.Clear();
                tbEmail.Clear();
                tbAmountPaid.Clear();

                lbList.Items.Clear();

                // Reset the flag in case the user decides not to print
                isPrintConfirmationShown = false;
                hasPrintConfirmationBeenShown = false;
            }
        }

        private void ValidateName()
        {
            string[] words = tbName.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2 && !isNameErrorShown)
            {
                MessageBox.Show("Name must have at least two words.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                isNameErrorShown = true;
                hasErrors = true;
                showMessagebox = false;
            }
        }
        private void ValidateContactNumber()
        {
            string contactNumber = tbContactNumber.Text;
            if ((!IsNumeric(contactNumber) || contactNumber.Length != 11) && !isContactNumberErrorShown)
            {
                MessageBox.Show("Invalid Contact Number. Please enter a numeric value with exactly 11 digits.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                isContactNumberErrorShown = true;
                hasErrors = true;
                showMessagebox = false;
            }
        }
        private void ValidateEmail()
        {
            string email = tbEmail.Text;
            if (!IsValidEmail(email) && !isEmailErrorShown)
            {
                MessageBox.Show("Invalid Email. Please enter a valid email address.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                isEmailErrorShown = true;
                hasErrors = true;
                showMessagebox = false;
            }
        }
        private void ValidateAddress()
        {

        }
        private void ValidateAmountPaid()
        {
            if (hasErrors || isAmountPaidErrorShown) return;

            decimal amountPaid;
            if (!decimal.TryParse(tbAmountPaid.Text, out amountPaid) || amountPaid < 0)
            {
                ShowErrorMessage("Invalid Amount Paid. Please enter a valid non-negative numeric value.");
                isAmountPaidErrorShown = true;
                return;
            }

            List<CartItem> orderedItems = GetOrderedItems();
            decimal totalPrices = orderedItems.Sum(item => item.CalculateTotalPrice());

            if (amountPaid < totalPrices)
            {
                ShowErrorMessage("Amount Paid must be equal or greater than Total Price.");
                isAmountPaidErrorShown = true;
            }
        }
        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            hasErrors = true;
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
            lbList.Items.Clear();
            decimal totalPrices = 0;

            string name = tbName.Text;
            string contactNumber = tbContactNumber.Text;
            string address = tbAddress.Text;
            string email = tbEmail.Text;

            lbList.Items.Add($"{"Name:",-25} {name}");
            lbList.Items.Add($"{"Contact Number:",-25} {contactNumber}");
            lbList.Items.Add($"{"Address:",-25} {address}");
            lbList.Items.Add($"{"Email:",-25} {email}");

            lbList.Items.Add(new string('-', 60));

            if (selectedItems.Any())
            {
                foreach (CartItem item in selectedItems)
                {
                    item.UpdateTotalPrice();

                    if (item.Quantity > 0)
                    {
                        string truncatedName = TruncateProductName(item.ProductName, 25); // Adjust the length as needed
                        lbList.Items.Add($"{truncatedName,-30} Qty: {item.Quantity}");

                        totalPrices += item.CalculateTotalPrice();
                    }
                }
            }

            lbList.Items.Add(new string('-', 60));

            CartItem firstItem = selectedItems.FirstOrDefault();
            if (firstItem != null)
            {
                lbList.Items.Add($"{"Total Price:",-25} {firstItem.Currency}{totalPrices,10:F2}");
            }

            lbList.TopIndex = lbList.Items.Count - 1;
        }

        private string TruncateProductName(string productName, int maxLength)
        {
            if (productName.Length > maxLength)
            {
                return productName.Substring(0, maxLength - 3) + "...";
            }
            return productName;
        }

        private string RemoveVowels(string input)
        {
            string result = new string(input.Where(c => !IsVowel(c)).ToArray());
            return result;
        }

        private bool IsVowel(char c)
        {
            return "aeiouAEIOU".Contains(c);
        }

        private class CartItem
        {
            public string ProductName { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public string Currency { get; } = "₱"; // Set the currency to pesos
            public decimal TotalPrice { get; private set; } // Allow private setting
            public DateTime DateOfPurchase { get; set; }

            public CartItem()
            {
                Quantity = 1;
            }

            public void UpdateTotalPrice()
            {
                TotalPrice = CalculateTotalPrice();
            }

            public decimal CalculateTotalPrice()
            {
                return Price * Quantity;
            }

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
            if (cartDataTable != null)
            {
                DataView dv = cartDataTable.DefaultView;
                string filterExpression = $"[Product Name] LIKE '%{tbPN.Text}%'";

                if (cbCat.SelectedItem != null)
                {
                    filterExpression += $" AND Category = '{cbCat.SelectedItem.ToString()}'";
                }

                dv.RowFilter = filterExpression;

                dv.Sort = "[Product Name] ASC";

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
            AddSelectedItemToCart();
        }

        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    DataGridViewCell productNameCell = dataGridView1.SelectedRows[0].Cells["Product Name"];

                    if (productNameCell != null && productNameCell.Value != null && !string.IsNullOrEmpty(productNameCell.Value.ToString()))
                    {
                        string productName = productNameCell.Value.ToString();

                        CartItem existingItem = selectedItems.Find(item => item.ProductName == productName);

                        if (existingItem != null)
                        {
                            if (existingItem.Quantity > 1)
                            {
                                existingItem.Quantity--;

                                UpdateQuantityTextBox();

                                existingItem.UpdateTotalPrice();

                                UpdateListBox();
                            }
                            else
                            {
                                selectedItems.Remove(existingItem);

                                UpdateQuantityTextBox();

                                UpdateListBox();
                            }
                        }
                        else
                        {
                            MessageBox.Show("The selected product is not in the cart.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("The selected product name is not valid.");
                    }
                }
            }
            catch (NullReferenceException)
            {
                // Handle a specific NullReferenceException
                MessageBox.Show("An error occurred: NullReferenceException");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                MessageBox.Show($"An error occurred: {ex.Message}");
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
                    else if (quantity >= 21 && quantity <= 40)
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

            dataGridView1.Columns["Category"].Width = 50;
            dataGridView1.Columns["Quantity"].Width = 50;
            dataGridView1.Columns["Price"].Width = 50;

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to log out?", "Confirm Log Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Login loginForm = new Login();
                loginForm.Show();
                this.Hide();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to exit?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
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

            this.Hide();
        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {

        }

        private void btClear_Click(object sender, EventArgs e)
        {
            tbQT.Text = string.Empty;
            tbAmountPaid.Text = string.Empty; 
            tbName.Text = string.Empty;
            tbContactNumber.Text = string.Empty;
            tbAddress.Text = string.Empty;
            tbEmail.Text = string.Empty;

            lbList.Items.Clear();
            selectedItems.Clear();


            UpdateListBox();

        }

        private void lbList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private decimal CalculateTotalPrices()
        {
            return selectedItems.Sum(item => item.CalculateTotalPrice());
        }
        private void OnPrintPage(object sender, PrintPageEventArgs e)
        {
            Font font = new Font("Arial", 10);
            Brush brush = Brushes.Black;

            System.Drawing.Image logo = System.Drawing.Image.FromFile("C:/Users/bj/Downloads/gr-modified.png");

            float xCenter = e.MarginBounds.Left + (e.MarginBounds.Width - 340) / 2;
            float y = e.MarginBounds.Top;

            DrawUserInformation(e.Graphics, font, brush, ref y, e);
            DrawSeparatorLine(e.Graphics, xCenter, ref y, e);
            DrawSelectedItems(e.Graphics, font, brush, ref y, e);

            DrawSeparatorLine(e.Graphics, xCenter, ref y, e);

            decimal totalPrices = CalculateTotalPrices(); // Calculate total prices
            DrawTotalPrice(e.Graphics, font, brush, totalPrices, ref y, e);

            DrawSeparatorLine(e.Graphics, xCenter, ref y, e);

            decimal amountPaid;
            bool isAmountPaidValid = decimal.TryParse(tbAmountPaid.Text, out amountPaid);
            DrawAmountPaid(e.Graphics, font, brush, isAmountPaidValid, amountPaid, ref y, e);

            DrawChange(e.Graphics, font, brush, totalPrices, amountPaid, ref y, e);
            DrawDate(e.Graphics, font, brush, ref y, e);

            y += 10;

            DrawShopLogo(e.Graphics, logo, ref y, 340, 340, xCenter, e);

            e.HasMorePages = false;
        }
        private void tbContactNumber_TextChanged(object sender, EventArgs e)
        {
            if (tbContactNumber.Text.Length > 11)
            {
                tbContactNumber.Text = tbContactNumber.Text.Substring(0, 11);
            }
        }

        private void tbContactNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != 8)
            {
                e.Handled = true;
            }

            if (tbContactNumber.Text.Length >= 11 && e.KeyChar != 8) // Check for Backspace
            {
                e.Handled = true;
            }
        }
        private void tbContactNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back)
            {
                e.Handled = false;
            }
        }
        private void DrawUserInformation(Graphics graphics, Font font, Brush brush, ref float y, PrintPageEventArgs e)
        {
            DrawText(graphics, font, brush, $"Name: {tbName.Text}", ref y, e);
            DrawText(graphics, font, brush, $"Contact Number: {tbContactNumber.Text}", ref y, e);
            DrawText(graphics, font, brush, $"Address: {tbAddress.Text}", ref y, e);
            DrawText(graphics, font, brush, $"Email: {tbEmail.Text}", ref y, e);
        }

        private void DrawSeparatorLine(Graphics graphics, float x, ref float y, PrintPageEventArgs e)
        {
            float lineWidth = 2; // Set the desired line width

            graphics.DrawLine(Pens.Black, e.MarginBounds.Left, y, e.MarginBounds.Right, y);
            y += lineWidth; // Increase the vertical position by the line width

            if (y + lineWidth > e.MarginBounds.Bottom)
            {
                y = e.MarginBounds.Top;
            }
        }

        private void DrawSeparatorLine(Graphics graphics, PrintPageEventArgs e, ref float y)
        {
            float lineWidth = 2; // Set the desired line width

            graphics.DrawLine(Pens.Black, e.MarginBounds.Left, y, e.MarginBounds.Right, y);
            y += lineWidth; // Increase the vertical position by the line width

            if (y + lineWidth > e.MarginBounds.Bottom)
            {
                y = e.MarginBounds.Top;
            }
        }


        private void DrawSelectedItems(Graphics graphics, Font font, Brush brush, ref float y, PrintPageEventArgs e)
        {
            foreach (CartItem item in selectedItems)
            {
                item.UpdateTotalPrice();
                if (item.Quantity > 0)
                {
                    string itemNameWithoutVowels = RemoveVowels(item.ProductName);
                    string itemText = $"{itemNameWithoutVowels} - Qty: {item.Quantity} - {item.Currency}{item.Price * item.Quantity:F2}";
                    DrawText(graphics, font, brush, itemText, ref y, e);
                }
            }
        }

        private void DrawTotalPrice(Graphics graphics, Font font, Brush brush, decimal totalPrices, ref float y, PrintPageEventArgs e)
        {
            string totalPriceText = $"Total Price: {selectedItems.First().Currency}{totalPrices:F2}";
            DrawText(graphics, font, brush, totalPriceText, ref y, e);
        }

        private void DrawAmountPaid(Graphics graphics, Font font, Brush brush, bool isAmountPaidValid, decimal amountPaid, ref float y, PrintPageEventArgs e)
        {
            if (isAmountPaidValid)
            {
                string amountPaidText = $"Amount Paid: {selectedItems.First().Currency}{amountPaid:F2}";
                DrawText(graphics, font, brush, amountPaidText, ref y, e);
            }
            else
            {
                DrawText(graphics, font, brush, "Amount Paid: Invalid input", ref y, e);
            }
        }

        private void DrawChange(Graphics graphics, Font font, Brush brush, decimal totalPrices, decimal amountPaid, ref float y, PrintPageEventArgs e)
        {
            decimal change = amountPaid - totalPrices;
            string changeText = $"Change: {selectedItems.First().Currency}{change:F2}";
            DrawText(graphics, font, brush, changeText, ref y, e);
        }



        private void DrawDate(Graphics graphics, Font font, Brush brush, ref float y, PrintPageEventArgs e)
        {
            string dateText = $"Date: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
            DrawText(graphics, font, brush, dateText, ref y, e);
        }

        private void DrawShopLogo(Graphics graphics, System.Drawing.Image logo, ref float y, int width, int height, float xCenter, PrintPageEventArgs e)
        {
            float opacity = 0.20f;
            ColorMatrix colorMatrix = new ColorMatrix { Matrix33 = opacity };
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            y = e.MarginBounds.Top + (e.MarginBounds.Height - height) / 2;

            graphics.DrawImage(logo, new Rectangle((int)xCenter, (int)y, width, height),
                                0, 0, logo.Width, logo.Height, GraphicsUnit.Pixel, imageAttributes);
        }


        private void DrawText(Graphics graphics, Font font, Brush brush, string text, ref float y, PrintPageEventArgs e)
        {
            float lineHeight = 20; // Set the desired line height

            graphics.DrawString(text, font, brush, e.MarginBounds.Left, y);
            y += lineHeight; // Use a consistent line height

            if (y + lineHeight > e.MarginBounds.Bottom)
            {
                y = e.MarginBounds.Top;
            }
        }

        private int GetQuantityFromText(string text)
        {
            int startIndex = text.IndexOf("Qty:") + 5;

            if (startIndex >= 5)
            {
                int endIndex = text.IndexOf("-", startIndex);

                if (endIndex > startIndex)
                {
                    string quantityString = text.Substring(startIndex, endIndex - startIndex).Trim();

                    if (int.TryParse(quantityString, out int quantity))
                    {
                        return quantity;
                    }
                }
            }

            return 0;
        }

        private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            float y = e.MarginBounds.Top;
            Font font = new Font("Arial", 12); // Adjust the font as needed
            Brush brush = Brushes.Black; // Adjust the color as needed

            // Draw user information
            DrawText(e.Graphics, font, brush, "Name: John Doe", ref y, e);
            DrawText(e.Graphics, font, brush, "Contact Number: 123-456-7890", ref y, e);
            DrawText(e.Graphics, font, brush, "Address: 123 Main St", ref y, e);
            DrawText(e.Graphics, font, brush, "Email: john@example.com", ref y, e);

            y += 20; // Add some spacing

            // Draw separator line
            DrawText(e.Graphics, font, brush, new string('-', 60), ref y, e);

            y += 20; // Add some spacing

            // Draw product information
            DrawText(e.Graphics, font, brush, "Product Name 1 - Qty: 5", ref y, e);
            DrawText(e.Graphics, font, brush, "Product Name 2 - Qty: 3", ref y, e);

            y += 20; // Add some spacing

            // Draw separator line
            DrawText(e.Graphics, font, brush, new string('-', 60), ref y, e);

            y += 20; // Add some spacing

            // Draw total price
            DrawText(e.Graphics, font, brush, "Total Price: ₱150.00", ref y, e);
        }

        private void DrawLine(Graphics graphics, float x, PrintPageEventArgs e, ref float y)
        {
            graphics.DrawLine(Pens.Black, x, y, e.MarginBounds.Right, y);
            y += 10;
        }
        private void CenteredText(Graphics graphics, Font font, Brush brush, string text, ref float y, PrintPageEventArgs e)
        {
            float textWidth = graphics.MeasureString(text, font).Width;
            float xCentered = e.MarginBounds.Left + (e.MarginBounds.Width - textWidth) / 2;

            graphics.DrawString(text, font, brush, xCentered, y);
            y += 20; // Use a consistent line height
        }
        private void DrawImage(Graphics graphics, System.Drawing.Image image, ref float y, int width, int height, float x, PrintPageEventArgs e)
        {
            float opacity = 0.20f;
            ColorMatrix colorMatrix = new ColorMatrix { Matrix33 = opacity };
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            float xCenter = e.MarginBounds.Left + (e.MarginBounds.Width - width) / 2;

            y = e.MarginBounds.Top + (e.MarginBounds.Height - height) / 2;

            graphics.DrawImage(image, new Rectangle((int)xCenter, (int)y, width, height),
                               0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
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

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex == -1)
            {
                // Disable further processing for the upper-left corner cell
                return;
            }
        }
    }
}