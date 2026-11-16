using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models;

public class Product
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public decimal Price { get; set; }
    [Required]
    public string Brand { get; set; }
    [Required]
    public int CategoryId { get; set; }
    [Required]
    public Category Category { get; set; }
}