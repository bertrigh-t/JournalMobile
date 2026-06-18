namespace JournalMobile.Models;

public class AttendanceItem
{
    public string Subject { get; set; } = "";
    public string Date { get; set; } = "";
    public string Status { get; set; } = "";

    public string DateText
    {
        get
        {
            if (DateTime.TryParse(Date, out var parsedDate))
                return parsedDate.ToString("dd.MM.yyyy");

            return Date;
        }
    }

    public string StatusText => Status switch
    {
        "present" => "Присутствовал",
        "absent" => "Отсутствовал",
        "late" => "Опоздал",
        "excused" => "Уважительная причина",
        _ => Status
    };

    public Color StatusColor => Status switch
    {
        "present" => Color.FromArgb("#8BC34A"),
        "absent" => Color.FromArgb("#E57373"),
        "late" => Color.FromArgb("#FFB74D"),
        "excused" => Color.FromArgb("#64B5F6"),
        _ => Colors.Gray
    };
}