namespace JournalMobile.Models;

public class TeacherAttendanceItem
{
    public int Id { get; set; }

    public string Student { get; set; } = "";

    public string Date { get; set; } = "";

    public string Status { get; set; } = "";

    public Microsoft.Maui.Graphics.Color StatusColor => Status switch
    {
        "absent" => Microsoft.Maui.Graphics.Color.FromArgb("#E57373"),
        "late" => Microsoft.Maui.Graphics.Color.FromArgb("#FFB74D"),
        "excused" => Microsoft.Maui.Graphics.Color.FromArgb("#90CAF9"),
        "present" => Microsoft.Maui.Graphics.Color.FromArgb("#8BC34A"),
        _ => Microsoft.Maui.Graphics.Colors.Gray
    };
}
