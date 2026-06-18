namespace JournalMobile.Models;

public class SemesterItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Start_date { get; set; } = "";
    public string End_date { get; set; } = "";

    public string PeriodText =>
        $"{Start_date} - {End_date}";
}