using JournalMobile.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using JournalMobile.Models;

namespace JournalMobile.Pages;

public partial class AdminUsersPage : ContentPage
{
    private readonly HttpClient _httpClient = new();

    private List<UserItem> _users = new();
    private List<UserItem> _filteredUsers = new();
    public AdminUsersPage()
	{
		InitializeComponent();
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadUsers();
        RoleFilterPicker.ItemsSource = new List<string>
            {
                "Все",
                "admin",
                "teacher",
                "student"
            };

        SortPicker.ItemsSource = new List<string>
            {
                "По логину A-Z",
                "По логину Z-A"
            };

        RoleFilterPicker.SelectedIndex = 0;
        SortPicker.SelectedIndex = 0;
    }
    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void OnRoleFilterChanged(object sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void OnSortChanged(object sender, EventArgs e)
    {
        ApplyFilters();
    }
    private void ApplyFilters()
    {
        IEnumerable<UserItem> result = _users;

        var search = SearchEntry.Text?.ToLower();

        if (!string.IsNullOrWhiteSpace(search))
        {
            result = result.Where(x =>
                x.Login.ToLower().Contains(search)
            );
        }
        if (RoleFilterPicker.SelectedIndex > 0)
        {
            var role = RoleFilterPicker.SelectedItem.ToString();

            result = result.Where(x => x.Role == role);
        }
        if (SortPicker.SelectedIndex == 0)
            result = result.OrderBy(x => x.Login);

        else if (SortPicker.SelectedIndex == 1)
            result = result.OrderByDescending(x => x.Login);
        _filteredUsers = result.ToList();

        UsersCollection.ItemsSource = _filteredUsers;
    }
    private async Task LoadUsers()
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:7070/Users"
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

            _users = JsonSerializer.Deserialize<List<UserItem>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new();

            ApplyFilters();
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
    private async void OnAddUserClicked(object sender, EventArgs e)
    {
        var login = await DisplayPromptAsync(
            "Новый пользователь",
            "Введите логин:"
        );

        if (string.IsNullOrWhiteSpace(login))
            return;

        var password = await DisplayPromptAsync(
            "Новый пользователь",
            "Введите пароль:"
        );

        if (string.IsNullOrWhiteSpace(password))
            return;

        var role = await DisplayActionSheet(
            "Выберите роль",
            "Отмена",
            null,
            "admin",
            "teacher",
            "student"
        );

        if (role == "Отмена")
            return;
        var name = await DisplayPromptAsync(
            "Новое имя",
            "Введите фамилию и имя:"
            );
        if (string.IsNullOrWhiteSpace(name))
            return;

        await AddUser(login, password, role, name);
    }
    private async void OnUserTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Border border)
            return;

        if (border.BindingContext is not UserItem user)
            return;

        var action = await DisplayActionSheet(
            user.Login,
            "Отмена",
            null,
            "Изменить",
            "Удалить"
        );

        switch (action)
        {
            case "Изменить":
                await EditUser(user);
                break;

            case "Удалить":
                await DeleteUser(user);
                break;
        }
    }
    private async Task AddUser(string login, string password, string role, string name)
    {
        try
        {
            var requestData = new
            {
                login,
                password,
                role,
                name
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://localhost:7070/Users"
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

            await LoadUsers();
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
    private async Task EditUser(UserItem user)
    {
        var login = await DisplayPromptAsync(
            "Изменение пользователя",
            "Логин:",
            initialValue: user.Login
        );

        if (string.IsNullOrWhiteSpace(login))
            return;

        var password = await DisplayPromptAsync(
            "Изменение пользователя",
            "Новый пароль:"
        );

        if (string.IsNullOrWhiteSpace(password))
            return;

        var role = await DisplayActionSheet(
            "Роль",
            "Отмена",
            null,
            "admin",
            "teacher",
            "student"
        );

        if (role == "Отмена")
            return;

        var name = await DisplayPromptAsync(
            "Изменение пользователя",
            "Имя (ФИО):"
        );

        if (string.IsNullOrWhiteSpace(name))
            return;

        await UpdateUser(user.Id, login, password, role, name);
    }
    private async Task UpdateUser(int id, string login, string password, string role, string name)
    {
        var requestData = new
        {
            login,
            password,
            role,
            name
        };

        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"https://localhost:7070/Users/{id}"
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
            await DisplayAlert("Ошибка", json, "OK");
            return;
        }

        await LoadUsers();
    }
    private async Task DeleteUser(UserItem user)
    {
        bool confirm = await DisplayAlert(
            "Удаление",
            $"Удалить пользователя {user.Login}?",
            "Да",
            "Нет"
        );

        if (!confirm)
            return;

        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"https://localhost:7070/Users/{user.Id}"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                AuthService.Token
            );

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
            await LoadUsers();
    }
}
