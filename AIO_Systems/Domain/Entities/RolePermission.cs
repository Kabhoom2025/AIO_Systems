namespace AIO_Systems.Domain.Entities;

public class RolePermission
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string Feature { get; set; } = string.Empty;

    public virtual Role Role { get; set; } = null!;
}
