using System.Security.Claims;
using System.Text.Json.Serialization;
using Afra_App.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Afra_App.Models;

public class Person
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    
    [JsonIgnore]
    public Person? Mentor { get; set; }
    public Class? Class { get; set; }

    [JsonIgnore]
    public ICollection<Person> Mentees { get; set; } = new List<Person>();
    [JsonIgnore]
    public ICollection<Class> TutoredClasses { get; set; } = new List<Class>();
    [JsonIgnore]
    public ICollection<Role> Roles { get; set; } = new List<Role>();
    [JsonIgnore]
    public ICollection<Otium> ManagedOtia { get; set; } = new List<Otium>();
    [JsonIgnore]
    public ICollection<OtiumEnrollment> OtiaEnrollments { get; set; } = new List<OtiumEnrollment>();
    
    public bool IsStudent => Class != null;
    
    [JsonIgnore]
    public IEnumerable<Permission> Permissions => Roles.SelectMany(r => r.Permissions).Distinct();

    public override string ToString() => $"{FirstName} {LastName}";
    
    
    public ClaimsPrincipal ToClaimsPrincipalAsync()
    {
        var claims = new List<Claim>
        {
            new(AfraAppClaimTypes.Id, Id.ToString()),
            new(AfraAppClaimTypes.GivenName, FirstName),
            new(AfraAppClaimTypes.LastName, LastName)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identity);
    }
}