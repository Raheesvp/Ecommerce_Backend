using Application.Contracts.Services;
using Application.DTOs.Order;
using DinkToPdf;
using DinkToPdf.Contracts;

public class PdfService : IPdfService
{
    private readonly IConverter _converter;
    public PdfService(
                IConverter converter
        ) 
    
    { _converter = converter; }

   

    public async Task<byte[]> GenerateInvoicePdf(OrderResponse order)
    {
        var htmlContent = GetInvoiceHtml(order);

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 }
            },
            Objects = {
                new ObjectSettings() {
                    HtmlContent = htmlContent,
                    WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
        };

        var pdfBytes = _converter.Convert(doc);
        return await Task.FromResult(pdfBytes);
    }

    private string GetInvoiceHtml(OrderResponse order)
    {
        decimal gstAmount = order.TotalAmount * 0.08m;
        decimal grandTotal = order.TotalAmount + gstAmount;

        string paymentStatus = order.PaymentMethod == "COD" ? "UNPAID - DUE ON DELIVERY" : "PAID IN FULL";
        string statusColor = order.PaymentMethod == "COD" ? "#fbbf24" : "#22c55e";

        var itemsHtml = "";
        foreach (var item in order.OrderItems)
        {
            itemsHtml += $@"
                <tr style='border-bottom: 1px solid #eee;'>
                    <td style='padding: 12px;'>{item.ProductName}</td>
                    <td style='padding: 12px; text-align: center;'>{item.Quantity}</td>
                    <td style='padding: 12px; text-align: right;'>₹{item.Price}</td>
                    <td style='padding: 12px; text-align: right;'>₹{item.Price * item.Quantity}</td>
                </tr>";
        }

        return $@"
        <html>
        <body style='font-family: Arial, sans-serif; margin: 40px;'>
            <div style='display: flex; justify-content: space-between;'>
                <h1 style='color: #dc2626;'>WOLF ATHLETIX</h1>
                <div style='text-align: right;'>
                    <h2>INVOICE #{order.Id}</h2>
                    <p>Date: {order.OrderDate:dd MMM yyyy}</p>
                </div>
            </div>
            <div style='margin-top: 20px; padding: 10px; background: #f9f9f9; border-left: 5px solid {statusColor};'>
                <strong>Status: {paymentStatus}</strong>
            </div>
            <table style='width: 100%; margin-top: 30px; border-collapse: collapse;'>
                <tr style='background: #111; color: white;'>
                    <th style='padding: 10px; text-align: left;'>Item</th>
                    <th>Qty</th>
                    <th style='text-align: right;'>Price</th>
                    <th style='text-align: right;'>Total</th>
                </tr>
                {itemsHtml}
            </table>
            <div style='text-align: right; margin-top: 20px;'>
                <p>Subtotal: ₹{order.TotalAmount}</p>
                <p>GST (8%): ₹{gstAmount}</p>
                <h2 style='color: #dc2626;'>Grand Total: ₹{grandTotal}</h2>
            </div>
        </body>
        </html>";
    }
}