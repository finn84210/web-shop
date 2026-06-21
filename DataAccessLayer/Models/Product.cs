using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class Product
    {        
        public int Id { get; set; }

        [Required(ErrorMessage = "Productnaam is verplicht.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Omschrijving is verplicht.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Categorie is verplicht.")]
        public string Category { get; set; } = "Algemeen";

        [Range(0.01, 999999.99, ErrorMessage = "Prijs moet groter zijn dan 0.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Voorraad mag niet negatief zijn.")]
        public int Stock { get; set; }

        public ICollection<Order> Orders { get; } = new List<Order>();

        public ICollection<Part> Parts { get; } = new List<Part>();
    }
}
