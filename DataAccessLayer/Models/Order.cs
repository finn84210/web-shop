using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        [Required]
        public string Status { get; set; } = "Nieuw";

        public string Source { get; set; } = "Webshop";

        public string? ExternalReference { get; set; }

        public string? DeliveryPerson { get; set; }

        public DateTime? SentToDeliveryAt { get; set; }

        public int CustomerId { get; set; }
        
        public Customer Customer { get; set; } = null!;

        public ICollection<Product> Products { get; } = new List<Product>();

        [NotMapped]
        public bool IsActive => Status != "Afgerond" && Status != "Geannuleerd";
    }
}
