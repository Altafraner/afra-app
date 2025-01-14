namespace Afra_App.Models;

public class Role
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    
    public Role(string name)
    {
        Name = name;
    }
}