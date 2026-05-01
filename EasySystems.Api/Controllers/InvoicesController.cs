using EasySystems.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EasySystems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class InvoicesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public InvoicesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [HttpGet("request/{requestId:int}")]
    public async Task<IActionResult> GenerateInvoice(int requestId)
    {
        var data = await (
            from storeRequest in _dbContext.StoreRequests
            join user in _dbContext.UserAccounts
                on storeRequest.UserAccountId equals user.Id
            where storeRequest.Id == requestId
            select new
            {
                storeRequest.Id,
                storeRequest.StoreName,
                storeRequest.BusinessType,
                storeRequest.PackageName,
                storeRequest.Status,
                storeRequest.CreatedAtUtc,
                CustomerName = user.FirstName + " " + user.LastName,
                CustomerEmail = user.Email,
                CustomerPhone = user.PhoneNumber
            })
            .FirstOrDefaultAsync();

        if (data is null)
            return NotFound("Request not found.");

        var price = GetPackagePrice(data.PackageName);
        var vat = price * 0.25m;
        var total = price + vat;

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{data.Id:D4}";

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("EasySystems")
                                .FontSize(28)
                                .Bold()
                                .FontColor(Colors.Blue.Medium);

                            left.Item().Text("Professional Online Store Solutions")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(180).Column(right =>
                        {
                            right.Item().AlignRight().Text("INVOICE")
                                .FontSize(26)
                                .Bold();

                            right.Item().AlignRight().Text(invoiceNumber)
                                .FontColor(Colors.Grey.Darken1);
                        });
                    });

                    col.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(25).Column(col =>
                {
                    col.Spacing(20);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("Bill To").Bold().FontSize(14);
                            left.Item().Text(data.CustomerName);
                            left.Item().Text(data.CustomerEmail);
                            left.Item().Text(data.CustomerPhone);
                        });

                        row.RelativeItem().Column(right =>
                        {
                            right.Item().Text("Invoice Details").Bold().FontSize(14);
                            right.Item().Text($"Date: {DateTime.UtcNow:yyyy-MM-dd}");
                            right.Item().Text($"Request ID: #{data.Id}");
                            right.Item().Text($"Status: {data.Status}");
                        });
                    });

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("Description");
                            header.Cell().Element(HeaderCell).Text("Package");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Qty");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Price");
                        });

                        table.Cell().Element(Cell).Text($"{data.StoreName} - {data.BusinessType}");
                        table.Cell().Element(Cell).Text(data.PackageName);
                        table.Cell().Element(Cell).AlignRight().Text("1");
                        table.Cell().Element(Cell).AlignRight().Text($"{price:N0} kr");
                    });

                    col.Item().AlignRight().Width(250).Column(totalCol =>
                    {
                        totalCol.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Subtotal");
                            row.ConstantItem(100).AlignRight().Text($"{price:N0} kr");
                        });

                        totalCol.Item().Row(row =>
                        {
                            row.RelativeItem().Text("VAT 25%");
                            row.ConstantItem(100).AlignRight().Text($"{vat:N0} kr");
                        });

                        totalCol.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        totalCol.Item().PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem().Text("Total").Bold().FontSize(14);
                            row.ConstantItem(100).AlignRight().Text($"{total:N0} kr").Bold().FontSize(14);
                        });
                    });

                    col.Item().PaddingTop(20).Background(Colors.Blue.Lighten5).Padding(16).Column(note =>
                    {
                        note.Item().Text("Payment Information").Bold();
                        note.Item().Text("Payment details can be added here later, for example Bankgiro, Swish, Stripe, or manual invoice payment.");
                    });
                });

                page.Footer().AlignCenter().Text("© EasySystems • Thank you for your business")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf", $"{invoiceNumber}.pdf");
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container
            .Background(Colors.Blue.Medium)
            .DefaultTextStyle(x => x.FontColor(Colors.White).Bold())
            .Padding(8);
    }

    private static IContainer Cell(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(8);
    }

    private static decimal GetPackagePrice(string packageName)
    {
        return packageName switch
        {
            "Starter" => 4900,
            "Business" => 9900,
            "Premium" => 19900,
            _ => 0
        };
    }
}