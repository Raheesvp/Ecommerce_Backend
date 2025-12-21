using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Settings
            const string adminEmail = "rahees12@gmail.com";
            const string adminPassword = "rahees123";

            // 1. Check if Admin exists (Ignore Filter to find deleted ones)
            var existingAdmin = await context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == adminEmail);

            if (existingAdmin == null)
            {
                // CASE A: Create New Admin
                var newAdmin = new User(
                    firstName: "Admin",
                    lastName: "User",
                    email: adminEmail,
                    passwordHash: BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    role: Roles.Admin
                );

                // Manual Audit Fields (Since no user is logged in)
                newAdmin.CreatedAt = DateTime.UtcNow;
                newAdmin.CreatedBy = "System";
                newAdmin.IsBlocked = false;
                newAdmin.IsDeleted = false;

                await context.Users.AddAsync(newAdmin);
                await context.SaveChangesAsync();
                Console.WriteLine("--- SEEDING: Created new System Admin ---");
            }
            else if (existingAdmin.IsDeleted)
            {
                // CASE B: Restore Deleted Admin
                existingAdmin.IsDeleted = false;
                existingAdmin.DeletedAt = null;
                existingAdmin.DeletedBy = null;

                // Optional: Reset password if you want to be sure
                // existingAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);

                context.Users.Update(existingAdmin);
                await context.SaveChangesAsync();
                Console.WriteLine("--- SEEDING: Restored deleted System Admin ---");
            }
        }
    }
}
