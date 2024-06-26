using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Cannot Be Empty")]
        [Display(Name = "Name")]
        [StringLength(50, ErrorMessage = "Can Be Max 50 Characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Cannot Be Empty")]
        [Display(Name = "Description")]
        [StringLength(50, ErrorMessage = "Can Be Max 50 Characters")]
        public string Description { get; set; }

        public virtual List<Product> Products { get; set; }
    }
}
