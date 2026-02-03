using Altafraner.AfraApp.Profundum.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services;

internal class ProfundumFachbereicheService
{
    private readonly AfraAppContext _dbContext;

    public ProfundumFachbereicheService(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> CreateFachbereichAsync(string label)
    {
        var entity = _dbContext.ProfundaFachbereiche.Add(
            new ProfundumFachbereich { Label = label }
        );
        await _dbContext.SaveChangesAsync();
        return entity.Entity.Id;
    }

    public async Task UpdateFachbereichAsync(Guid id, string label)
    {
        var entity = _dbContext.ProfundaFachbereiche.FirstOrDefault(x => x.Id == id);
        if (entity is null)
            throw new ArgumentException("Kategorie not found", nameof(id));
        entity.Label = label;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteFachbereichAsync(Guid id)
    {
        var entity = _dbContext.ProfundaFachbereiche.FirstOrDefault(x => x.Id == id);
        if (entity is null)
            throw new ArgumentException("Kategorie not found", nameof(id));
        _dbContext.ProfundaFachbereiche.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<ProfundumFachbereich>> GetFachbereicheAsync()
    {
        return await _dbContext.ProfundaFachbereiche.ToListAsync();
    }
}
