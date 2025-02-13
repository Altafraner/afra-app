namespace Afra_App.Data.People;

public class Rolle
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public ICollection<Berechtigung> Permissions { get; set; } = new List<Berechtigung>();
    
    public Rolle(string name)
    {
        Name = name;
    }
}