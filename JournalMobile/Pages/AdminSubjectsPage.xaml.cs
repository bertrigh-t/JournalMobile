using JournalMobile.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using JournalMobile.Models;


namespace JournalMobile.Pages;

public partial class AdminSubjectsPage : ContentPage
{
    private readonly HttpClient _httpClient = new();
    private List<SubjectItem> _subjects = new();
    private List<SubjectItem> _teachers = new();
    private List<TeacherSubjectItem> _teacherSubjects = new();
    public AdminSubjectsPage()
    {
        InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadTeachers();
        await LoadSubjects();
        await LoadAssignSubjects();
    }
    private async Task LoadSubjects()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get,"https://localhost:7070/Subject");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",AuthService.Token);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Response JSON: {json}");

            if (string.IsNullOrWhiteSpace(json))
            {
                await DisplayAlert("Ошибка", "Сервер вернул пустой ответ", "OK");
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Ошибка", $"{response.StatusCode}\n{json}", "OK");
                return;
            }

            _subjects = JsonSerializer.Deserialize<List<SubjectItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Ошибка",json,"OK");
                return;
            }

            _subjects = JsonSerializer.Deserialize<List<SubjectItem>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new();

            SubjectsCollection.ItemsSource = _subjects;
        }

        catch (Exception ex)
        {
            await DisplayAlert("Ошибка",ex.Message,"OK");
        }
    }
    private async Task LoadTeachers()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7070/Admin/teachers");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            _teachers = JsonSerializer.Deserialize<List<SubjectItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            TeacherPicker.ItemsSource = _teachers.Select(t => t.Name).ToList();
            if (_teachers.Count > 0) TeacherPicker.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async Task LoadAssignSubjects()
    {
        AssignSubjectPicker.ItemsSource = _subjects.Select(s => s.Name).ToList();
    }
    private async Task LoadTeacherSubjects(int teacherId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7070/TeacherSubjects/{teacherId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            _teacherSubjects = JsonSerializer.Deserialize<List<TeacherSubjectItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            TeacherSubjectsCollection.ItemsSource = _teacherSubjects;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
    private async void OnTeacherChanged(object sender, EventArgs e)
    {
        if (TeacherPicker.SelectedIndex < 0) return;
        var teacherId = _teachers[TeacherPicker.SelectedIndex].Id;
        await LoadTeacherSubjects(teacherId);
    }

    private async void OnAssignSubjectClicked(object sender, EventArgs e)
    {
        if (TeacherPicker.SelectedIndex < 0 || AssignSubjectPicker.SelectedIndex < 0) return;

        var teacherId = _teachers[TeacherPicker.SelectedIndex].Id;
        var subjectId = _subjects[AssignSubjectPicker.SelectedIndex].Id;

        var requestData = new { UserId = teacherId, SubjectId = subjectId };
        var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7070/TeacherSubjects");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.Token);
        request.Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            await DisplayAlert("Ошибка", "Не удалось назначить предмет", "OK");
            return;
        }

        await LoadTeacherSubjects(teacherId);
    }

    private async void OnTeacherSubjectSelected(object sender, SelectionChangedEventArgs e)
    {
        if (TeacherSubjectsCollection.SelectedItem is not TeacherSubjectItem ts) return;

        bool confirm = await DisplayAlert("Удалить", $"Удалить предмет {ts.Name}?", "Да", "Нет");
        if (!confirm) return;

        var request = new HttpRequestMessage(HttpMethod.Delete, $"https://localhost:7070/TeacherSubjects/{ts.Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.Token);
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            await DisplayAlert("Ошибка", "Не удалось удалить предмет", "OK");
            return;
        }

        var teacherId = _teachers[TeacherPicker.SelectedIndex].Id;
        await LoadTeacherSubjects(teacherId);
    }
    private async void OnAddSubjectClicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync(
            "Новый предмет",
            "Введите название"
        );

        if (string.IsNullOrWhiteSpace(name))
            return;

        await AddSubject(name);
    }
    private async void OnSubjectTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Border border)
            return;

        if (border.BindingContext is not SubjectItem subject)
            return;

        var action = await DisplayActionSheet(
            subject.Name,
            "Отмена",
            null,
            "Изменить",
            "Удалить"
        );

        switch (action)
        {
            case "Изменить":
                await EditSubject(subject);
                break;

            case "Удалить":
                await DeleteSubject(subject);
                break;
        }
    }
    private async Task AddSubject(string name)
    {
        try
        {
            var requestData = new
            {
                name
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://localhost:7070/Subject"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    AuthService.Token
                );

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
                    "Ошибка",
                    json,
                    "OK"
                );

                return;
            }

            await LoadSubjects();
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
    private async Task EditSubject(SubjectItem subject)
    {
        var name = await DisplayPromptAsync(
            "Изменение названия",
            "Название",
            initialValue: subject.Name
        );

        if (string.IsNullOrWhiteSpace(name))
            return;

        await UpdateSubject(subject.Id, name);
    }
    private async Task UpdateSubject(int id, string name)
    {
        var requestData = new
        {
            name
        };

        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"https://localhost:7070/Subject/{id}"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                AuthService.Token
            );

        request.Content = new StringContent(
            JsonSerializer.Serialize(requestData),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
            await LoadSubjects();
    }
    private async Task DeleteSubject(SubjectItem subject)
    {
        bool confirm = await DisplayAlert(
            "Удаление",
            $"Удалить предмет? {subject.Name}?",
            "Да",
            "Нет"
        );

        if (!confirm)
            return;

        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"https://localhost:7070/Subject/{subject.Id}"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                AuthService.Token
            );

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
            await LoadSubjects();
    }
}

