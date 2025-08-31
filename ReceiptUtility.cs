using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;

public static class ReceiptUtility
{
    public static void DisplayReceipt(int orderID, List<CartItem> orderedItems)
    {
        PrintDocument printDocument = new PrintDocument();
        printDocument.PrintPage += (sender, e) => OnPrintPage(sender, e, orderID, orderedItems);
        PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
        printPreviewDialog.Document = printDocument;
        printPreviewDialog.ShowDialog();
    }

    private static void OnPrintPage(object sender, PrintPageEventArgs e, int orderID, List<CartItem> orderedItems)
    {
        // Use the order details to display the receipt
        // ... (rest of the OnPrintPage method)

        // You can use the 'orderedItems' list to display the items in the receipt
        foreach (CartItem item in orderedItems)
        {
            // Display item details in the receipt
            // ...
        }
    }
}