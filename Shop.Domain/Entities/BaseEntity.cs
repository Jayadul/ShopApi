namespace Shop.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? CreationDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public bool IsArchived { get; set; } = false;
}
