using JournalMobile.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using JournalMobile.Models;

namespace JournalMobile.Pages;

public partial class StudentSchedulePage : ContentPage
{
    private readonly HttpClient _httpClient = new();

    public StudentSchedulePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadStudentSchedule();
    }

    private async Task LoadStudentSchedule()
    {
        try
        {
            ScheduleCollection.ItemsSource = null;

            // 1. Получаем текущего студента
            var studentRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:7070/Students/me"
            );

            studentRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var studentResponse = await _httpClient.SendAsync(studentRequest);
            var studentJson = await studentResponse.Content.ReadAsStringAsync();

            if (!studentResponse.IsSuccessStatusCode)
            {
                await DisplayAlert(
                    "Ошибка студента",
                    $"{studentResponse.StatusCode}\n{studentJson}",
                    "OK"
                );
                return;
            }

            var student = JsonSerializer.Deserialize<StudentItem>(
                studentJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            if (student == null)
            {
                await DisplayAlert(
                    "Ошибка",
                    "Не удалось загрузить данные студента",
                    "OK"
                );
                return;
            }

            if (student.GroupId == 0)
            {
                await DisplayAlert(
                    "Ошибка",
                    "У студента не указана группа",
                    "OK"
                );
                return;
            }

            // 2. Получаем расписание по groupId
            var scheduleRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://localhost:7070/Schedule/group/{student.GroupId}"
            );

            scheduleRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var scheduleResponse = await _httpClient.SendAsync(scheduleRequest);
            var scheduleJson = await scheduleResponse.Content.ReadAsStringAsync();

            if (!scheduleResponse.IsSuccessStatusCode)
            {
                await DisplayAlert(
                    "Ошибка расписания",
                    $"{scheduleResponse.StatusCode}\n{scheduleJson}",
                    "OK"
                );
                return;
            }

            var schedule = JsonSerializer.Deserialize<List<ScheduleItem>>(
                scheduleJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new List<ScheduleItem>();

            if (schedule.Count == 0)
            {
                await DisplayAlert(
                    "Расписание",
                    "Для вашей группы пока нет расписания",
                    "OK"
                );
                return;
            }

            ScheduleCollection.ItemsSource = schedule
                .OrderBy(x => x.DayOfWeek)
                .ThenBy(x => x.Number)
                .ToList();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Ошибка",
                ex.Message,
                "OK"
            );
        }
    }
}
public class StudentItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int GroupId { get; set; }
    public int UserId { get; set; }
}
