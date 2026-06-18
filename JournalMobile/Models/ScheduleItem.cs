using System.Text.Json.Serialization;

namespace JournalMobile.Models;

public class ScheduleItem
{
    public int Id { get; set; }
    public int DayOfWeek { get; set; }
    public string DayName =>
    DayOfWeek switch
    {
        1 => "Понедельник",
        2 => "Вторник",
        3 => "Среда",
        4 => "Четверг",
        5 => "Пятница",
        6 => "Суббота",
        _ => "Неизвестно"
    };
    public string Time { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Group { get; set; } = "";
    public string Teacher { get; set; } = "";
}
