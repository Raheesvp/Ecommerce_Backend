using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Admin
{
    public class UserResponse
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
