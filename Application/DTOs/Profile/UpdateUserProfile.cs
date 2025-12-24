using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Profile
{
    public  class UpdateUserProfile
    {
        public string  FirstName { get; set; }

        public string LastName { get; set; }
        public string MobileNumber { get; set; }

        public IFormFile? ProfileImage { get; set; }

        public string? CurrentPassword { get; set; }

        public string? NewPassword { get; set; } 

    }
}
