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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTodaySchedule();
    }
    private async Task LoadTodaySchedule()
    {
        TodayScheduleLabel.Text = "Загрузка...";

        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:7070/Schedule/student"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                TodayScheduleLabel.Text = $"Ошибка: {response.StatusCode}";
                return;
            }

            var schedule = JsonSerializer.Deserialize<List<ScheduleItem>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (schedule == null || schedule.Count == 0)
            {
                TodayScheduleLabel.Text = "Расписание на сегодня отсутствует";
                return;
            }

            var today = (int)DateTime.Now.DayOfWeek;
            if (today == 0) today = 7; // воскресенье = 7

            var todaySchedule = schedule.Where(s => s.DayOfWeek == today).ToList();

            if (todaySchedule.Count == 0)
            {
                TodayScheduleLabel.Text = "Пар на сегодня нет";
                return;
            }

            TodayScheduleLabel.Text = string.Join("\n",
                todaySchedule.Select(s =>
                    $"{s.StartTime}-{s.EndTime} | {s.Subject} | каб. {s.Classroom}")
            );
        }
        catch (Exception ex)
        {
            TodayScheduleLabel.Text = "Ошибка подключения:\n" + ex.Message;
        }
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

public class ScheduleItem
{
    public int DayOfWeek { get; set; }
    public string Subject { get; set; } = "";
    public string StartTime { get; set; } = "";
    public string EndTime { get; set; } = "";
    public string Classroom { get; set; } = "";
}