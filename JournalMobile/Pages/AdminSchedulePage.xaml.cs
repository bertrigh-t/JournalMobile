using JournalMobile.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using JournalMobile.Models;

namespace JournalMobile.Pages;

public partial class AdminSchedulePage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();

    private List<GroupItem> _groups = new();
    private List<SemesterItem> _semesters = new();

    private List<ScheduleItem> _schedule = new();

    public AdminSchedulePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadGroups();
        await LoadSemesters();
    }

    private async Task LoadGroups()
    {
        var req = new HttpRequestMessage(HttpMethod.Get,
            "https://localhost:7070/Admin/groups");

        req.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", AuthService.Token);

        var res = await _httpClient.SendAsync(req);
        var json = await res.Content.ReadAsStringAsync();

        _groups = JsonSerializer.Deserialize<List<GroupItem>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        GroupPicker.ItemsSource = _groups;
        GroupPicker.ItemDisplayBinding = new Binding("Name");
    }

    private async Task LoadSemesters()
    {
        var req = new HttpRequestMessage(HttpMethod.Get,
            "https://localhost:7070/Semesters");

        req.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", AuthService.Token);

        var res = await _httpClient.SendAsync(req);
        var json = await res.Content.ReadAsStringAsync();

        _semesters = JsonSerializer.Deserialize<List<SemesterItem>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        SemesterPicker.ItemsSource = _semesters
            .Select(x => $"{x.Start_date} - {x.End_date}")
            .ToList();
    }

    private async void OnLoadClicked(object sender, EventArgs e)
    {
        if (GroupPicker.SelectedIndex < 0 ||
            SemesterPicker.SelectedIndex < 0)
        {
            await DisplayAlert("Ошибка", "Выбери группу и семестр", "OK");
            return;
        }

        var groupId = _groups[GroupPicker.SelectedIndex].Id;
        var semesterId = _semesters[SemesterPicker.SelectedIndex].Id;

        await LoadSchedule(groupId, semesterId);
    }

    private async Task LoadSchedule(int groupId, int semesterId)
    {
        var req = new HttpRequestMessage(HttpMethod.Get,
            $"https://localhost:7070/Schedule?groupId={groupId}&semesterId={semesterId}");

        req.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", AuthService.Token);

        var res = await _httpClient.SendAsync(req);
        var json = await res.Content.ReadAsStringAsync();

        _schedule = JsonSerializer.Deserialize<List<ScheduleItem>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        var items = _schedule.Select(s => new ScheduleGroupItem
        {
            DayText = TranslateDay(s.DayOfWeek),
            TimeSubjectTeacher = $"{s.Time} | {s.Subject} | {s.Teacher}"
        }).ToList();

        ScheduleCollection.ItemsSource = items;
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
            _ => "?"
        };
    }
    private async void OnAddClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddSchedulePage());
    }
}
public class ScheduleGroupItem
{
    public string DayText { get; set; } = "";
    public string TimeSubjectTeacher { get; set; } = "";
}