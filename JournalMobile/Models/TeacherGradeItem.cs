namespace JournalMobile.Models;

public class TeacherGradeItem
{
    public int Id { get; set; }
    public string Student { get; set; } = "";
    public int Grade { get; set; }
    public string Date { get; set; } = "";

    public Microsoft.Maui.Graphics.Color GradeColor => Grade switch
    {
        5 => Microsoft.Maui.Graphics.Color.FromArgb("#8BC34A"),
        4 => Microsoft.Maui.Graphics.Color.FromArgb("#CDDC39"),
        3 => Microsoft.Maui.Graphics.Color.FromArgb("#FFB74D"),
        2 => Microsoft.Maui.Graphics.Color.FromArgb("#E57373"),
        _ => Microsoft.Maui.Graphics.Colors.Gray
    };
}