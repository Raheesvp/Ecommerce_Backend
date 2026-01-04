using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Profile
{
    public  class UserProfile
    {
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }

        public string? LastName { get; set; }


        [Required]
        public string? MobileNumber { get; set; }

      
        public string? ProfileImageUrl { get; set; }



    }
}
