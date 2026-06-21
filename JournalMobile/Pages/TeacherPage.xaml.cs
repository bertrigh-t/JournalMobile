using JournalMobile.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using JournalMobile.Models;


namespace JournalMobile.Pages;

public partial class TeacherPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();

    public TeacherPage()
    {
        InitializeComponent();
    }

    private async void OnGroupStudentsClicked(object sender, EventArgs e)
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:7070/Groups/1/students"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            var students = JsonSerializer.Deserialize<List<StudentItem>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Ошибка подключения",
                ex.Message,
                "OK"
            );
        }
    }
    private async void OnGradesJournalClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TeacherGradesPage());
    }

    private async void OnAttendanceStudentsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TeacherAttendancePage());
    }


    private string FormatDate(string date)
    {
        if (DateTime.TryParse(date, out var parsedDate))
            return parsedDate.ToString("dd.MM.yyyy");

        return date;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {

    }
}

public class TeacherScheduleItem
{
    public string Subject { get; set; } = "";
    public string Group { get; set; } = "";
    public string Time { get; set; } = "";
}