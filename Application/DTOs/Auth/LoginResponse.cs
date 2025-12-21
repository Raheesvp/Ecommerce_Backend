using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Auth
{
    public  class LoginResponse
    {
       
        public string AccessToken { get; set; } 

        public string RefreshToken { get; set; } 

        public required string Role { get; set; }

        
    }
}
