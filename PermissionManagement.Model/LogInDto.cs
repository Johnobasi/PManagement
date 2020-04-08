using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PermissionManagement.Model
{
   public class LogInDto
    {
       [Required(AllowEmptyStrings = false, ErrorMessage = "User name must be supplied")]
       public string Username { get; set; }

       [Required(AllowEmptyStrings = false, ErrorMessage = "Password must be supplied")]
       public string Password { get; set; }
       public string TokenCode { get; set; }

       public bool RememberMe { get; set; }

       public string Message { get; set; }
       
       public string ReturnUrl { get; set; }
    }
}
