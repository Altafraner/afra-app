using Afra_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Afra_App;

public class OtiumService(AfraAppContext dbContext)
{
    public void Create(string designation, string description, OtiumsKategory kategory, bool isCataloged,
        Person creator)
    {
        var otium = new Otium
        {
            Designation = designation,
            Description = description,
            Kategory = kategory,
            IsCataloged = isCataloged
        };

        otium.Managers.Add(creator);

        dbContext.Otia.Add(otium);
        dbContext.SaveChanges();
    }

    public void CreateKategory(string designation)
    {
        var kategory = new OtiumsKategory
        {
            Designation = designation
        };

        dbContext.OtiaKategories.Add(kategory);
        dbContext.SaveChanges();
    }

    public void CreateInstallment(Otium otium, Person tutor, DateTime start, TimeOnly end, string location)
    {
        var installment = new OstiumInstallment
        {
            Otium = otium,
            Tutor = tutor,
            Start = start,
            End = end,
            Location = location,
            Regularity = null
        };

        dbContext.OtiaInstallments.Add(installment);
        dbContext.SaveChanges();
    }

    public void CreateRegularity(Otium otium, Person tutor, DayOfWeek day, TimeOnly start, TimeOnly end,
        string location)
    {
        var regularity = new OtiumRegularity
        {
            Otium = otium,
            Tutor = tutor,
            Day = day,
            Start = start,
            End = end,
            Location = location
        };

        dbContext.OtiaRegularities.Add(regularity);
        dbContext.SaveChanges();
    }

    public void Enroll(Otium otium, Person student, DateTime start, TimeOnly end, bool mayEdit)
    {
        otium.EnrollOnce(student, start, end, mayEdit);
        dbContext.SaveChanges();
    }

    public void Update(Otium update)
    {
        var entry = dbContext.Otia
            .Include(o => o.Kategory)
            .FirstOrDefault(o => o.Id == update.Id);
        
        if (entry is null) throw new ArgumentException("The given otium could not be found");

        if (entry.Description != update.Description)
        {
            entry.Description = update.Description;
        }
        
        if (entry.Designation != update.Designation)
        {
            entry.Designation = update.Designation;
        }
        
        if (entry.IsCataloged != update.IsCataloged)
        {
            entry.IsCataloged = update.IsCataloged;
        }
        
        if (entry.Kategory != update.Kategory)
        {
            entry.Kategory = update.Kategory;
        }
        
        dbContext.SaveChanges();
    }
    
    public void Delete(Otium otium)
    {
        dbContext.Otia.Remove(otium);
        dbContext.SaveChanges();
    }
    
    public void DeleteKategory(OtiumsKategory kategory)
    {
        // TODO: Check if there are any otia that are still using this kategory
        dbContext.OtiaKategories.Remove(kategory);
        dbContext.SaveChanges();
    }
    
    public void DeleteInstallment(OstiumInstallment installment)
    {
        if (installment.Enrollments.Count == 0 && installment.Regularity is null)
        {
            dbContext.OtiaInstallments.Remove(installment);
        }
        else
        {
            installment.IsCanceled = true;
        }
        dbContext.SaveChanges();
        // TODO: Notify the students already enrolled
    }
    
    public void DeleteRegularity(OtiumRegularity regularity)
    {
        dbContext.OtiaRegularities.Remove(regularity);
        dbContext.SaveChanges();
    }
    
    public void Unenroll(OtiumEnrollment enrollment)
    {
        dbContext.OtiaEnrollments.Remove(enrollment);
        dbContext.SaveChanges();
    }
    
    public void UpdateInstallment(OstiumInstallment update)
    {
        var entry = dbContext.OtiaInstallments
            .Find(update.Id);
        
        if (entry is null) throw new ArgumentException("The given installment could not be found");

        if (entry.End != update.End)
        {
            entry.End = update.End;
        }
        
        if (entry.Location != update.Location)
        {
            entry.Location = update.Location;
        }
        
        if (entry.Start != update.Start)
        {
            entry.Start = update.Start;
        }
        
        dbContext.SaveChanges();
        // TODO: Notify the students already enrolled
    }

    public void UpdateRegularity(OtiumRegularity update, bool cascade = true)
    {
        var entry = dbContext.OtiaRegularities
            .Include(or => or.Installments)
            .FirstOrDefault(or => or.Id == update.Id);

        if (entry is null) throw new ArgumentException("The given regularity could not be found");

        var installments = cascade ? entry.Installments.Where(ei => !ei.IsCanceled && ei.Start >= DateTime.Now) : [];

        // Update the regularity

        if (entry.Day != update.Day)
        {
            entry.Day = update.Day;
            foreach (var installment in installments)
            {
                // TODO: Notify the students already enrolled
                // This is not as trivial as it seems, because the installment needs to stay in the same week whilst the day changes
                var diff = (int)update.Day - (int)entry.Day;
                installment.Start = installment.Start.AddDays(diff);
            }
        }

        if (entry.End != update.End)
        {
            entry.End = update.End;
            foreach (var installment in installments)
            {
                // TODO: Notify the students already enrolled
                installment.End = update.End;
            }
        }

        if (entry.Location != update.Location)
        {
            entry.Location = update.Location;
            foreach (var installment in installments)
            {
                // TODO: Notify the students already enrolled
                installment.Location = update.Location;
            }
        }

        if (entry.Start != update.Start)
        {
            entry.Start = update.Start;
            foreach (var installment in installments)
            {
                // TODO: Notify the students already enrolled
                installment.Start = installment.Start.Date + update.Start.ToTimeSpan();
            }
        }
        
        dbContext.SaveChanges();
    }
    
    public void UpdateEnrollment(OtiumEnrollment update)
    {
        var entry = dbContext.OtiaEnrollments
            .Find(update.Id);
        
        if (entry is null) throw new ArgumentException("The given enrollment could not be found");
        
        if (entry.End != update.End)
        {
            entry.End = update.End;
        }
        
        if (entry.Start != update.Start)
        {
            entry.Start = update.Start;
        }
        
        dbContext.SaveChanges();
    }
    
}