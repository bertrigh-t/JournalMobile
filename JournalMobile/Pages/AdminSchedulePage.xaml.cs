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
    private int _currentGroupId;
    private int _currentSemesterId;
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
        _currentGroupId = groupId;
        _currentSemesterId = semesterId;

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
            NumberTimeSubjectTeacher = $"№{s.Number} {s.Time} | {s.Subject} | {s.Teacher}"
        }).ToList();
        var groups = _schedule
        .GroupBy(s => s.DayOfWeek)
        .OrderBy(g => g.Key)
        .Select(g => new ScheduleDayGroup(g.Key, g.OrderBy(s => s.Number)))
        .ToList();

        ScheduleCollection.ItemsSource = groups;
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
    private async void OnLessonTapped (object sender, TappedEventArgs e)
    {
        if (sender is not Frame frame)
            return;

        if (frame.BindingContext is not ScheduleItem item)
            return;

        var action = await DisplayActionSheet(
            $"{item.Subject} ({item.Time})",
            "Отмена",
            null,
            "Изменить",
            "Удалить"
        );

        switch (action)
        {
            case "Удалить":
                await DeleteLesson(item);
                break;
        }
    }
    private async Task DeleteLesson(ScheduleItem item)
    {
        bool confirm = await DisplayAlert(
            "Удаление",
            $"Удалить предмет? {item.Subject}?",
            "Да",
            "Нет"
        );

        if (!confirm)
            return;

        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"https://localhost:7070/Schedule/{item.Id}"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                AuthService.Token
            );

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
            await LoadSchedule(_currentGroupId, _currentSemesterId);
    }
}
public class ScheduleGroupItem
{
    public string DayText { get; set; } = "";
    public string NumberTimeSubjectTeacher { get; set; } = "";
}