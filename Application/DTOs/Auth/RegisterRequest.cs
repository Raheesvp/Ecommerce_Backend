using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Auth
{
    public  class RegisterRequest
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ]+([ '-][a-zA-ZÀ-ÿ]+)*$", ErrorMessage = "Names cannot be just spaces.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
        @"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$",ErrorMessage = "Enter a valid email address")]


        public string Email { get; set; } = string.Empty;

    

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "Passwords Do Not Match")]

        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
