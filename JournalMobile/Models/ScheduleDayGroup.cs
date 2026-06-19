namespace JournalMobile.Models;
public class ScheduleDayGroup : List<ScheduleItem>
{
    public string DayName { get; set; }
    public int DayOfWeek { get; set; }

    public int Number { get; set; }
    public ScheduleDayGroup(int dayOfWeek, IEnumerable<ScheduleItem> items) : base(items)
    {
        DayOfWeek = dayOfWeek;
        DayName = TranslateDay(dayOfWeek);
    }

    private static string TranslateDay(int day) => day switch
    {
        1 => "Понедельник",
        2 => "Вторник",
        3 => "Среда",
        4 => "Четверг",
        5 => "Пятница",
        6 => "Суббота",
        _ => "?"
    };
}