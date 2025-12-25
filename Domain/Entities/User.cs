using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class User : BaseEntity
    {
        // For EF Core

        protected User() { }

        public User( string firstName,string lastName,string email, string passwordHash, Roles role = Roles.User)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("FirstName cannot be empty");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("SecondName Cannot be empty");

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty");

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("PasswordHash cannot be empty");

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Email = email.Trim().ToLowerInvariant();
            PasswordHash = passwordHash;
            Role = role;
            IsBlocked = false;
        }

        public string FirstName { get;  set; }

        public string LastName { get;  set; }

        public string FullName => $"{FirstName}{LastName}";
        public string Email { get; private set; }
        public string PasswordHash { get;  set; }

        public string? MobileNumber { get;  set; }

        public string?  ProfileImageUrl { get; set; }
        public Roles Role { get; private set; }
        public bool IsBlocked { get;  set; }

        public string? RefreshToken { get; private set; }
        public DateTime? RefreshTokenExpiryTime { get; private set; }


        public void Block() => IsBlocked = true;
        public void Unblock() => IsBlocked = false;

        public void ChangeRole(Roles role) => Role = role;

        public void UpdatePassword(string passwordHash)
        {
            PasswordHash = passwordHash;
        }

        public void SetRefreshToken(string token, DateTime expiry)
        {
            RefreshToken = token;
            RefreshTokenExpiryTime = expiry;
        }
    }

   
}
