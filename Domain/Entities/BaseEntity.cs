namespace Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  
        public string? CreatedBy { get; set; }
     
        public DateTime? LastModifiedAt { get; set; }
  
        public string? LastModifiedBy { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }



        public void SetUpdated()
        {
            LastModifiedAt = DateTime.UtcNow;
        }
    }
}
