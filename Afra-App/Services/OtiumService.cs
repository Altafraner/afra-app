using Afra_App.Data;
using Afra_App.Data.Otium;
using Afra_App.Data.People;
using Microsoft.EntityFrameworkCore;

namespace Afra_App;

public class OtiumService(AfraAppContext dbContext)
{
    public void ErstelleOtium(string bezeichnung, string beschreibung, Kategorie kategorie,
        Person erstellendePerson)
    {
        var otium = new Otium
        {
            Bezeichnung = bezeichnung,
            Beschreibung = beschreibung,
            Kategorie = kategorie
        };

        otium.Verantwortliche.Add(erstellendePerson);

        dbContext.Otia.Add(otium);
        dbContext.SaveChanges();
    }

    public void ErstelleKategorie(string designation)
    {
        var kategorie = new Kategorie
        {
            Bezeichnung = designation
        };

        dbContext.OtiaKategorien.Add(kategorie);
        dbContext.SaveChanges();
    }

    public void ErstelleTermin(Otium otium, Person tutor, DateOnly datum, byte block, string ort)
    {
        var termin = new Termin
        {
            Otium = otium,
            Tutor = tutor,
            Datum = datum,
            Block = block,
            Ort = ort,
            Wiederholung = null
        };

        dbContext.OtiaTermine.Add(termin);
        dbContext.SaveChanges();
    }

    public void ErstelleWiederholung(Otium otium, Person tutor, DayOfWeek wochentag, byte block, string location)
    {
        var wiederholung = new Wiederholung
        {
            Otium = otium,
            Tutor = tutor,
            Wochentag = wochentag,
            Block = block,
            Ort = location
        };

        dbContext.OtiaWiederholungen.Add(wiederholung);
        dbContext.SaveChanges();
    }

    public void EinmalEinschreiben(Otium otium, Person student, DateTime start, TimeOnly end)
    {
        otium.EinmalEinschreiben(student, start, end);
        dbContext.SaveChanges();
    }

    public void AktualisiereOtium(Otium update)
    {
        var entry = dbContext.Otia
            .Include(o => o.Kategorie)
            .FirstOrDefault(o => o.Id == update.Id);
        
        if (entry is null) throw new ArgumentException("The given otium could not be found");

        if (entry.Beschreibung != update.Beschreibung)
        {
            entry.Beschreibung = update.Beschreibung;
        }
        
        if (entry.Bezeichnung != update.Bezeichnung)
        {
            entry.Bezeichnung = update.Bezeichnung;
        }
        
        if (entry.Kategorie != update.Kategorie)
        {
            entry.Kategorie = update.Kategorie;
        }
        
        dbContext.SaveChanges();
    }
    
    public void LöscheOtium(Otium otium)
    {
        dbContext.Otia.Remove(otium);
        dbContext.SaveChanges();
    }
    
    public void LöscheKategorie(Kategorie kategory)
    {
        // TODO: Check if there are any otia that are still using this kategory
        dbContext.OtiaKategorien.Remove(kategory);
        dbContext.SaveChanges();
    }
    
    public void LöscheTermin(Termin termin)
    {
        if (termin.Enrollments.Count == 0 && termin.Wiederholung is null)
        {
            dbContext.OtiaTermine.Remove(termin);
        }
        else
        {
            termin.IstAbgesagt = true;
        }
        dbContext.SaveChanges();
        // TODO: Notify the students already enrolled
    }
    
    public void LöscheWiederholung(Wiederholung wiederholung)
    {
        dbContext.OtiaWiederholungen.Remove(wiederholung);
        dbContext.SaveChanges();
    }
    
    public void Austragen(Einschreibung einschreibung)
    {
        dbContext.OtiaEinschreibungen.Remove(einschreibung);
        dbContext.SaveChanges();
    }
    
    public void AktualisiereTermin(Termin update)
    {
        var entry = dbContext.OtiaTermine
            .Find(update.Id);
        
        if (entry is null) throw new ArgumentException("The given installment could not be found");

        if (entry.Block != update.Block)
        {
            entry.Block = update.Block;
        }
        
        if (entry.Ort != update.Ort)
        {
            entry.Ort = update.Ort;
        }
        
        dbContext.SaveChanges();
        // TODO: Notify the students already enrolled
    }

    public void AktualisiereWiederholung(Wiederholung update, bool cascade = true)
    {
        var entry = dbContext.OtiaWiederholungen
            .Include(or => or.Termine)
            .FirstOrDefault(or => or.Id == update.Id);

        if (entry is null) throw new ArgumentException("The given regularity could not be found");

        var termine = cascade ? 
            entry.Termine.Where(t => !t.IstAbgesagt && t.Datum >= DateOnly.FromDateTime(DateTime.Today)).ToList() : 
            [];

        // Update the regularity

        if (entry.Wochentag != update.Wochentag)
        {
            entry.Wochentag = update.Wochentag;
            foreach (var termin in termine)
            {
                // TODO: Notify the students already enrolled
                // This is not as trivial as it seems, because the installment needs to stay in the same week whilst the day changes
                var diff = (int)update.Wochentag - (int)entry.Wochentag;
                termin.Datum = termin.Datum.AddDays(diff);
            }
        }

        if (entry.Ort != update.Ort)
        {
            entry.Ort = update.Ort;
            foreach (var termin in termine)
            {
                // TODO: Notify the students already enrolled
                termin.Ort = update.Ort;
            }
        }

        if (entry.Block != update.Block)
        {
            entry.Block = update.Block;
            foreach (var termin in termine)
            {
                // TODO: Notify the students already enrolled
                termin.Block = update.Block;
            }
        }
        
        dbContext.SaveChanges();
    }
    
    public void AktualisiereEinschreibung(Einschreibung update)
    {
        var entry = dbContext.OtiaEinschreibungen
            .Find(update.Id);
        
        if (entry is null) throw new ArgumentException("The given enrollment could not be found");
        
        if (entry.Ende != update.Ende)
        {
            entry.Ende = update.Ende;
        }
        
        if (entry.Start != update.Start)
        {
            entry.Start = update.Start;
        }
        
        dbContext.SaveChanges();
    }
    
}