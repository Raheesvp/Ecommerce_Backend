namespace Domain.Common
{
    public abstract class BaseEntity
    {
        // 1. ID: Protected set is fine, EF Core can handle it.
        public int Id { get; set; }

        // --- CREATION AUDIT ---
        // Changed to DateTime.UtcNow (never use string for dates)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Renamed 'CreatedOn' to 'CreatedBy' to store the User ID/Name
        public string? CreatedBy { get; set; }

        // --- UPDATE AUDIT ---
        // Changed string -> DateTime?
        public DateTime? LastModifiedAt { get; set; }

        // Changed 'ModifiedOn' (ambiguous) to 'LastModifiedBy' (clear)
        public string? LastModifiedBy { get; set; }

        // --- DELETION AUDIT (Soft Delete) ---
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }



        public void SetUpdated()
        {
            LastModifiedAt = DateTime.UtcNow;
        }
    }
}
