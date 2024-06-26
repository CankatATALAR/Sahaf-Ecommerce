using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Cannot Be Empty")]
        [Display(Name = "Name")]
        [StringLength(50, ErrorMessage = "Can Be Max 50 Characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Cannot Be Empty")]
        [Display(Name = "SurName")]
        [StringLength(50, ErrorMessage = "Can Be Max 50 Characters")]
        public string SurName { get; set; }

        
        public string Email { get; set; }

        [Required(ErrorMessage = "Cannot Be Empty")]
        [Display(Name = "Username")]
        [StringLength(50, ErrorMessage = "Can Be Max 50 Characters")]
        public string UserName { get; set; }

        
        public string Password { get; set; }

        
        public string RePassword { get; set; }

        [StringLength(10, ErrorMessage = "Can Be Max 10 Characters")]
        public string Role { get; set; }
    }
}
