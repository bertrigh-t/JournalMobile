using JournalMobile.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace JournalMobile.Pages;

public partial class StudentGradesPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();

    public StudentGradesPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadGrades();
    }

    private async Task LoadGrades()
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:7070/Student/grades"
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

            var grades = JsonSerializer.Deserialize<List<GradeItem>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (grades == null || grades.Count == 0)
            {
                await DisplayAlert("Оценки", "Оценок пока нет", "OK");
                return;
            }

            var grouped = grades
                .GroupBy(g => g.Subject)
                .Select(g => new SubjectGradesGroup
                {
                    Subject = g.Key,
                    Grades = g.ToList()
                })
                .ToList();

            SubjectsCollection.ItemsSource = grouped;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка подключения", ex.Message, "OK");
        }
    }

    private async void OnGradeTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not GradeItem grade)
            return;

        await DisplayAlert(
            "Информация об оценке",
            $"Предмет: {grade.Subject}\n" +
            $"Оценка: {grade.Grade}\n" +
            $"Дата: {FormatDate(grade.Date)}",
            "OK"
        );
    }
    public class SubjectGradesGroup
    {
        public string Subject { get; set; } = "";
        public List<GradeItem> Grades { get; set; } = new();
    }
    private string FormatDate(string date)
    {
        if (DateTime.TryParse(date, out var parsedDate))
            return parsedDate.ToString("dd.MM.yyyy");

        return date;
    }
}
public class GradeItem
{
    public int Id { get; set; }
    public string Subject { get; set; } = "";
    public int Grade { get; set; }
    public string Date { get; set; } = "";
    public Color GradeColor => Grade switch
    {
        5 => Color.FromArgb("#F4B5FF"),
        4 => Color.FromArgb("#C9B6CC"),
        3 => Color.FromArgb("#926A99"),
        2 => Color.FromArgb("#5E3266"),
        _ => Colors.Gray
    };

}