using Afra_App.Profundum.Domain.Models;
using Afra_App.User.Services;
using Microsoft.EntityFrameworkCore;
using Person = Afra_App.User.Domain.Models.Person;

namespace Afra_App.Profundum.Services;

/// <summary>
///     A service for handling enrollments.
/// </summary>
public class EnrollmentService
{
    private readonly AfraAppContext _dbContext;
    private readonly ILogger _logger;
    private readonly UserService _userService;

    /// <summary>
    ///     Constructs the EnrollmentService. Usually called by the DI container.
    /// </summary>
    public EnrollmentService(AfraAppContext dbContext,
        ILogger<EnrollmentService> logger, UserService userService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userService = userService;
    }
}
