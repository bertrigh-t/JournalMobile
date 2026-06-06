using JournalMobile.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace JournalMobile.Pages;

public partial class AdminSubjectsPage : ContentPage
{
    private readonly HttpClient _httpClient = new();

    private List<SubjectItem> _subjects = new();
    public AdminSubjectsPage()
    {
        InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadSubjects();
    }
    private async Task LoadSubjects()
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:7070/Subject"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    AuthService.Token
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

            _subjects = JsonSerializer.Deserialize<List<SubjectItem>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new();

            SubjectsCollection.ItemsSource = _subjects;
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
    private async void OnAddSubjectClicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync(
            "Новый предмет",
            "Введите название:"
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
            "Изменение предмета",
            "Предмет:",
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
            $"Удалить предмет {subject.Name}?",
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
public class SubjectItem
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

}
