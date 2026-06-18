namespace JournalMobile.Models;
public class ScheduleDayGroup
{
    public string DayName { get; set; } = "";

    public List<ScheduleItem> Lessons { get; set; } = new();
}