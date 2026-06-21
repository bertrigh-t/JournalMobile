using JournalMobile.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using JournalMobile.Models;

namespace JournalMobile.Pages;

public partial class AddSchedulePage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();

    private List<GroupItem> _groups = new();
    private List<SubjectItem> _subjects = new();
    private List<TeacherItem> _teachers = new();
    private List<SemesterItem> _semesters = new();
    public AddSchedulePage()
	{
		InitializeComponent();
        for (int i = 1; i <= 6; i++)
            NumberPicker.Items.Add(i.ToString());
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        DayPicker.ItemsSource = new List<string>
    {
        "Понедельник",
        "Вторник",
        "Среда",
        "Четверг",
        "Пятница",
        "Суббота"
    };

        await LoadGroups();
        await LoadSubjects();
        await LoadTeachers();
        await LoadSemesters();
    }
    private async Task LoadGroups()
    {
        try
        {
            GroupPicker.Items.Clear();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:7070/Admin/groups"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert(
                    "Ошибка групп",
                    $"{response.StatusCode}\n{json}",
                    "OK"
                );

                return;
            }

            _groups = JsonSerializer.Deserialize<List<GroupItem>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new();

            foreach (var group in _groups)
            {
                GroupPicker.Items.Add(group.Name);
            }
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

    private async Task LoadSubjects()
    {
        try
        {
            SubjectPicker.Items.Clear();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:7070/Admin/subjects"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert(
                    "Ошибка предметов",
                    $"{response.StatusCode}\n{json}",
                    "OK"
                );

                return;
            }

            _subjects = JsonSerializer.Deserialize<List<SubjectItem>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new();

            foreach (var subject in _subjects)
            {
                SubjectPicker.Items.Add(subject.Name);
            }
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

    private async Task LoadTeachers()
    {
        try
        {
            TeacherPicker.Items.Clear();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:7070/Admin/teachers"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert(
                    "Ошибка преподавателей",
                    $"{response.StatusCode}\n{json}",
                    "OK"
                );

                return;
            }

            _teachers = JsonSerializer.Deserialize<List<TeacherItem>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new();

            foreach (var teacher in _teachers)
            {
                TeacherPicker.Items.Add(teacher.Name);
            }
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

    private async void OnCreateLessonClicked(object sender, EventArgs e)
    {
        try
        {
            if (
                GroupPicker.SelectedIndex < 0 ||
                SubjectPicker.SelectedIndex < 0 ||
                TeacherPicker.SelectedIndex < 0 ||
                SemesterPicker.SelectedIndex < 0 ||
                NumberPicker.SelectedIndex < 0 ||
                DayPicker.SelectedIndex < 0 ||
                NumberPicker.SelectedIndex < 0
            )
            {
                await DisplayAlert(
                    "Ошибка",
                    "Заполните все поля",
                    "OK"
                );

                return;
            }

            var selectedGroup = _groups[GroupPicker.SelectedIndex];
            var selectedSubject = _subjects[SubjectPicker.SelectedIndex];
            var selectedTeacher = _teachers[TeacherPicker.SelectedIndex];
            var selectedSemester = _semesters[SemesterPicker.SelectedIndex];
            int dayOfWeek = GetDayNumber(DayPicker.SelectedItem!.ToString()!);
            int lessonNumber = int.Parse(NumberPicker.SelectedItem!.ToString()!);
            string lessonTime = GetLessonTime(dayOfWeek, lessonNumber);

            var requestData = new
            {
                groupId = selectedGroup.Id,
                subjectId = selectedSubject.Id,
                teacherId = selectedTeacher.Id,
                semesterId = selectedSemester.Id,
                dayOfWeek = GetDayNumber(DayPicker.SelectedItem!.ToString()),
                number = lessonNumber,
                time = lessonTime

            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://localhost:7070/Schedule"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthService.Token);

            request.Content = new StringContent(
                JsonSerializer.Serialize(requestData),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert(
                    "Ошибка создания",
                    $"{response.StatusCode}\n{json}",
                    "OK"
                );

                return;
            }

            await DisplayAlert(
                "Успешно",
                "Журнал создан",
                "OK"
            );
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
    private string GetLessonTime(int dayOfWeek, int lessonNumber)
    {
        // ПОНЕДЕЛЬНИК — отдельное расписание
        if (dayOfWeek == 1)
        {
            return lessonNumber switch
            {
                1 => "08:45-10:05",
                2 => "10:15-11:35",
                3 => "12:45-14:05",
                4 => "14:15-15:35",
                5 => "15:45-17:05",
                6 => "17:15-18:15",
                _ => ""
            };
        }

        // ВСЕ ОСТАЛЬНЫЕ ДНИ
        return lessonNumber switch
        {
            1 => "08:00-09:20",
            2 => "09:30-10:50",
            3 => "11:10-12:30",
            4 => "13:30-14:50",
            5 => "15:00-16:20",
            6 => "16:30-17:50",
            _ => ""
        };
    }
    private int GetDayNumber(string day)
    {
        return day switch
        {
            "Понедельник" => 1,
            "Вторник" => 2,
            "Среда" => 3,
            "Четверг" => 4,
            "Пятница" => 5,
            "Суббота" => 6,
            _ => 1
        };
    }
}