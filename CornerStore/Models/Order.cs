using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models;

public class Order
{
    public int Id { get; set; }
    [Required]
    public int CashierId { get; set; }
    public DateTime? PaidOnDate { get; set; }
    
    public List<OrderProduct> ProductsOrdered { get; set; }

    public decimal Total
    {
        get
        {
            decimal OrderCost = ProductsOrdered.Sum(po => po.Quantity * po.Product.Price);
            return OrderCost;
        }
    }
}