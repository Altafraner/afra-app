using Afra_App.Models;
using Microsoft.AspNetCore.Mvc;

namespace Afra_App.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ClassController (AfraAppContext dbContext) : ControllerBase
{
    /// <summary>
    /// Get all Classes
    /// </summary>
    public IEnumerable<Class> GetClasses()
    {
        return dbContext.Classes;
    }
    
    /// <summary>
    /// Get specific Class
    /// </summary>
    /// <param name="id">ID of the class that will be returned</param>
    [HttpGet("{id}")]
    public ActionResult<Class> GetClass(Guid id)
    {
        var klasse = dbContext.Classes.Find(id);
        return klasse == null ? NotFound() : klasse;
    }
    
    /// <summary>
    /// Creates a new class
    /// </summary>
    /// <param name="class">The Information for the class to be created</param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult<Class> PostClass(Class @class)
    {
        dbContext.Classes.Add(@class);
        dbContext.SaveChanges();
        
        // Refers to the GetClass method
        return CreatedAtAction(nameof(GetClass), new { id = @class.Id }, @class);
    }
    
    /// <summary>
    /// Deletes a class. This will also delete all students in this class.
    /// </summary>
    /// <param name="id">The ID of the Class to be deleted</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public ActionResult<Class> DeleteKlasse(Guid id)
    {
        var klasse = dbContext.Classes.Find(id);
        
        if (klasse == null)
            return NotFound();
        
        dbContext.Classes.Remove(klasse);
        dbContext.SaveChanges();
        return NoContent();
    }
    
    /// <summary>
    /// Gets the tutor of a class
    /// </summary>
    /// <param name="id">The Class of which to return the tutor</param>
    /// <returns></returns>
    [HttpGet("{id}/klassenleiter")]
    public ActionResult<Person> GetTutor(Guid id)
    {
        var klasse = dbContext.Classes
            .Find(id);
        
        if (klasse == null)
        {
            return NotFound();
        }
        
        dbContext.Classes.Entry(klasse)
            .Reference(k => k.Tutor)
            .Load();

        return klasse.Tutor == null ? NotFound() : klasse.Tutor;
    }

    /// <summary>
    /// Sets the tutor of a class
    /// </summary>
    /// <param name="id">Id of the class whose tutor should be set</param>
    /// <param name="tutorId">Id of the tutor</param>
    [HttpPut("{id:guid}/klassenleiter/{tutorId:guid}")]
    public ActionResult<Class> SetTutor(Guid id, Guid tutorId)
    {
        var klasse = dbContext.Classes.Find(id);
        if (klasse == null)
            return NotFound("Klasse nicht gefunden");
        
        var klassenleiter = dbContext.People.Find(tutorId);
        if (klassenleiter == null)
            return NotFound("Klassenleiter nicht gefunden");
        
        klasse.Tutor = klassenleiter;
        dbContext.SaveChanges();
        return klasse;
    }
    
    /// <summary>
    /// Gets all students of a class
    /// </summary>
    /// <param name="id">The class from which the students should be loaded</param>
    [HttpGet("{id:guid}/students")]
    public IEnumerable<Person> GetStudents(Guid id)
    {
        var klasse = dbContext.Classes
            .Find(id);
        
        if (klasse == null)
            return new List<Person>();
        
        dbContext.Classes.Entry(klasse)
            .Collection(k => k.Students)
            .Load();
        
        return klasse.Students;
    }
    
    /// <summary>
    /// Adds a student to a class
    /// </summary>
    /// <param name="id">Id of the class</param>
    /// <param name="studentId">Id of the student</param>
    [HttpPut("{id:guid}/students/{studentId:guid}")]
    public ActionResult<Person> PutStudent(Guid id, Guid studentId)
    {
        var klasse = dbContext.Classes.Find(id);
        if (klasse == null)
            return NotFound("Klasse nicht gefunden");
        
        var schueler = dbContext.People.Find(studentId);
        if (schueler == null)
            return NotFound("Schüler nicht gefunden");
        
        dbContext.People.Entry(schueler)
            .Reference(s => s.Class)
            .Load();
        if (schueler.Class == klasse)
            return BadRequest("Schüler ist bereits in dieser Klasse");
        schueler.Class = klasse;
        dbContext.SaveChanges();
        return schueler;
    }
    
    /// <summary>
    /// Removes a student from a class. This does not delete the people-instance but will remove their student status.
    /// </summary>
    /// <param name="id">Id of the class</param>
    /// <param name="studentId">Id of the student</param>
    [HttpDelete("{id:guid}/students/{studentId:guid}")]
    public ActionResult<Person> DeleteSchueler(Guid id, Guid studentId)
    {
        var klasse = dbContext.Classes.Find(id);
        if (klasse == null)
            return NotFound("Klasse nicht gefunden");
        
        var schueler = dbContext.People.Find(studentId);
        if (schueler == null)
            return NotFound("Schüler nicht gefunden");
        
        dbContext.People.Entry(schueler)
            .Reference(s => s.Class)
            .Load();
        if (schueler.Class != klasse)
            return BadRequest("Schüler ist nicht in dieser Klasse");
        klasse.Students.Remove(schueler);
        dbContext.SaveChanges();
        return schueler;
    }
}