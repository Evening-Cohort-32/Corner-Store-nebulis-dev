using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models;

public class Order
{
    public int Id { get; set; }
    [Required]
    public int CashierId { get; set; }
    public DateTime? PaidOnDate { get; set; }
    
    private List<OrderProduct> ProductsOrdered { get; set; } = new List<OrderProduct>();
    public decimal Total => ProductsOrdered.Sum(op => op.OrderTotal);
}