using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Cannot Be Empty")]
        [Display(Name = "Name")]
        [StringLength(50, ErrorMessage = "Can Be Max 50 Characters")]
        public string Name { get; set; }    

        [Required(ErrorMessage = "Cannot Be Empty")]
        [Display(Name = "Description")]
        [StringLength(100, ErrorMessage = "Can Be Max 50 Characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Cannot Be Empty")]
        [Display(Name = "Price")]
        public Decimal Price { get; set; }

        [Required(ErrorMessage = "Cannot Be Empty")]
        [Display(Name = "Stock")]
        //[StringLength(50, ErrorMessage = "Can Be Max 50 Characters")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Cannot Be Empty")]
        [Display(Name = "Popular")]
        public bool Popular { get; set; }

        [Required(ErrorMessage = "Cannot Be Empty")]
        [Display(Name = "Image")]
        public string Image { get; set; }

        [Required(ErrorMessage = "Cannot Be Empty")]
        [Display(Name = "CategoryId")]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public virtual List<Cart> Cart { get; set;}
        public virtual List<Sales> Sales { get; set;}


    }
}
