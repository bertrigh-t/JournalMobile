using System.Text.Json.Serialization;

namespace JournalMobile.Models;

public class ScheduleEditDto
{
    public int Id { get; set; }

    public int GroupId { get; set; }
    public int SubjectId { get; set; }
    public int TeacherId { get; set; }
    public int SemesterId { get; set; }

    public int DayOfWeek { get; set; }
    public int Number { get; set; }
}