using JournalMobile.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using JournalMobile.Models;

namespace JournalMobile.Pages;

public partial class StudentAttendancePage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();

    public StudentAttendancePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAttendance();
    }

    private async Task LoadAttendance()
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:7070/Student/attendance"
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

            var attendance = JsonSerializer.Deserialize<List<AttendanceItem>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (attendance == null || attendance.Count == 0)
            {
                await DisplayAlert("Посещаемость", "Записей пока нет", "OK");
                return;
            }

            AttendanceCollection.ItemsSource = attendance;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка подключения", ex.Message, "OK");
        }
    }
}
