using JournalMobile.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using JournalMobile.Models;

namespace JournalMobile.Pages;

public partial class StudentSchedulePage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();

    public StudentSchedulePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSchedule();
    }

    private async Task LoadSchedule()
    {
        ScheduleCollection.ItemsSource = null;

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
                await DisplayAlert("Ошибка", $"{response.StatusCode}\n{json}", "OK");
                return;
            }

            var schedule = JsonSerializer.Deserialize<List<ScheduleItem>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (schedule == null || schedule.Count == 0)
            {
                await DisplayAlert("Расписание", "Расписание отсутствует", "OK");
                return;
            }

            var items = schedule.Select(s => new ScheduleDisplayItem
            {
                DayOfWeekText = TranslateDay(s.DayOfWeek),
                TimeSubjectClassroom = $"{s.Time} | {s.Subject}"
            }).ToList();

            ScheduleCollection.ItemsSource = items;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка подключения", ex.Message, "OK");
        }
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

public class ScheduleDisplayItem
{
    public string DayOfWeekText { get; set; } = "";
    public string TimeSubjectClassroom { get; set; } = "";
}