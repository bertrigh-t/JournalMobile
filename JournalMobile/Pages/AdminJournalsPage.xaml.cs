using JournalMobile.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace JournalMobile.Pages;

public partial class AdminJournalsPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();

    private List<AdminGroupItem> _groups = new();
    private List<AdminSubjectItem> _subjects = new();
    private List<AdminTeacherItem> _teachers = new();

    public AdminJournalsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadGroups();
        await LoadSubjects();
        await LoadTeachers();
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

            _groups = JsonSerializer.Deserialize<List<AdminGroupItem>>(
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

            _subjects = JsonSerializer.Deserialize<List<AdminSubjectItem>>(
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

            _teachers = JsonSerializer.Deserialize<List<AdminTeacherItem>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new();

            foreach (var teacher in _teachers)
            {
                TeacherPicker.Items.Add(teacher.Login);
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

    private async void OnCreateJournalClicked(object sender, EventArgs e)
    {
        try
        {
            if (
                GroupPicker.SelectedIndex < 0 ||
                SubjectPicker.SelectedIndex < 0 ||
                TeacherPicker.SelectedIndex < 0
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

            var requestData = new
            {
                groupId = selectedGroup.Id,
                subjectId = selectedSubject.Id,
                userId = selectedTeacher.Id
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://localhost:7070/Admin/journals"
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
}

public class AdminGroupItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class AdminSubjectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class AdminTeacherItem
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
}