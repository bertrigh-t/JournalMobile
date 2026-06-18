using JournalMobile.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace JournalMobile.Pages;

public partial class StudentPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();

    public StudentPage()
    {
        InitializeComponent();
    }

    private async void OnGradesClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new StudentGradesPage());
    }
    private string FormatDate(string date)
    {
        if (DateTime.TryParse(date, out var parsedDate))
            return parsedDate.ToString("dd.MM.yyyy");

        return date;
    }
    private string TranslateStatus(string status)
    {
        return status switch
        {
            "present" => "Присутствовал",
            "absent" => "Отсутствовал",
            "late" => "Опоздал",
            "excused" => "Уважительная причина",
            _ => status
        };
    }
    private async void OnAttendanceClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new StudentAttendancePage());
    }
    private async void OnScheduleClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new StudentSchedulePage());
    }
    private string TranslateDay(int day)
    {
        return day switch
        {
            1 => "Понедельник",
            2 => "Вторник",
            3 => "Среда",
            4 => "Четверг",
            5 => "Пятница",
            6 => "Суббота",
            _ => "Неизвестно"
        };
    }
}
