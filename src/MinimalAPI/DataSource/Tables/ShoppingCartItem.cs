using System.ComponentModel.DataAnnotations;

namespace MinimalAPI.DataSource.Tables
{
    public class ShoppingCartItem
    {
        [Key]
        public Guid Id { get; set; }
        public string ItemName { get; set; }
        public double Quantity { get; set; }
        public bool IsPickedUp { get; set; }
    }
}
