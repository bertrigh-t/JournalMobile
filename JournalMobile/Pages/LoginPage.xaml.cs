using System.Text;
using System.Text.Json;
using JournalMobile.Services;

namespace JournalMobile.Pages;

public partial class LoginPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();

    public LoginPage()
    {
        InitializeComponent();
    }
    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public string Role { get; set; } = "";
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        MessageLabel.Text = "";

        var login = LoginEntry.Text;
        var password = PasswordEntry.Text;

        var data = new
        {
            login = login,
            password = password
        };

        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(
                "https://localhost:7070/Auth/login",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                MessageLabel.Text = "Неверный логин или пароль";
                return;
            }

            var responseText = await response.Content.ReadAsStringAsync();

            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(
                responseText,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            if (loginResponse == null)
            {
                MessageLabel.Text = "Ошибка чтения ответа сервера";
                return;
            }

            MessageLabel.TextColor = Colors.Green;
            MessageLabel.Text = "Вход выполнен";

            AuthService.Token = loginResponse.Token;
            AuthService.Role = loginResponse.Role;

            if (loginResponse.Role == "student")
            {
                await Navigation.PushAsync(new StudentPage());
            }
            else if (loginResponse.Role == "teacher")
            {
                await Navigation.PushAsync(new TeacherPage());
            }
            else
            {
                await DisplayAlert("Ошибка", "Неизвестная роль: " + loginResponse.Role, "OK");
            }
        }
        catch (Exception ex)
        {
            MessageLabel.Text = "Ошибка подключения: " + ex.Message;
        }
    }
}