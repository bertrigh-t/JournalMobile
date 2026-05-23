using JournalMobile.Services;
using System.Net.Http.Headers;
using System.Text.Json;

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