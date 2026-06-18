using JournalMobile.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using JournalMobile.Models;

namespace JournalMobile.Pages;

public partial class AdminSemestersPage : ContentPage
{
    private readonly HttpClient _httpClient = new();

    private List<SemesterItem> _semesters = new();

    public AdminSemestersPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadSemesters();
    }

    private async Task LoadSemesters()
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:7070/Semesters"
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

            _semesters = JsonSerializer.Deserialize<List<SemesterItem>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new();

            SemestersCollection.ItemsSource = _semesters;
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

    private async void OnAddSemesterClicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync(
            "Новый семестр",
            "Введите название:"
        );

        if (string.IsNullOrWhiteSpace(name))
            return;

        var startDate = await DisplayPromptAsync(
            "Дата начала",
            "Введите дату начала (ГГГГ-ММ-ДД):"
        );

        if (string.IsNullOrWhiteSpace(startDate))
            return;

        var endDate = await DisplayPromptAsync(
            "Дата окончания",
            "Введите дату окончания (ГГГГ-ММ-ДД):"
        );

        if (string.IsNullOrWhiteSpace(endDate))
            return;

        await AddSemester(name, startDate, endDate);
    }

    private async void OnSemesterTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Border border)
            return;

        if (border.BindingContext is not SemesterItem semester)
            return;

        var action = await DisplayActionSheet(
            semester.Name,
            "Отмена",
            null,
            "Изменить",
            "Удалить"
        );

        switch (action)
        {
            case "Изменить":
                await EditSemester(semester);
                break;

            case "Удалить":
                await DeleteSemester(semester);
                break;
        }
    }

    private async Task AddSemester(string name, string startDate, string endDate)
    {
        try
        {
            var requestData = new
            {
                name,
                start_date = startDate,
                end_date = endDate
            };

            var request = new HttpRequestMessage(HttpMethod.Post,"https://localhost:7070/Semesters");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.Token);

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

            await LoadSemesters();
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

    private async Task EditSemester(SemesterItem semester)
    {
        var name = await DisplayPromptAsync(
            "Изменение семестра",
            "Название:",
            initialValue: semester.Name
        );

        if (string.IsNullOrWhiteSpace(name))
            return;

        var startDate = await DisplayPromptAsync(
            "Дата начала",
            "Введите дату начала:",
            initialValue: semester.Start_date
        );

        if (string.IsNullOrWhiteSpace(startDate))
            return;

        var endDate = await DisplayPromptAsync(
            "Дата окончания",
            "Введите дату окончания:",
            initialValue: semester.End_date
        );

        if (string.IsNullOrWhiteSpace(endDate))
            return;

        await UpdateSemester(
            semester.Id,
            name,
            startDate,
            endDate
        );
    }

    private async Task UpdateSemester(int id, string name, string startDate, string endDate)
    {
        var requestData = new
        {
            name,
            start_date = startDate,
            end_date = endDate
        };

        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"https://localhost:7070/Semesters/{id}"
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
            await LoadSemesters();
    }

    private async Task DeleteSemester(SemesterItem semester)
    {
        bool confirm = await DisplayAlert(
            "Удаление",
            $"Удалить семестр {semester.Name}?",
            "Да",
            "Нет"
        );

        if (!confirm)
            return;

        var request = new HttpRequestMessage(HttpMethod.Delete, $"https://localhost:7070/Semesters/{semester.Id}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                AuthService.Token
            );

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
            await LoadSemesters();
    }
}