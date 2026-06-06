namespace JournalMobile.Pages;

public partial class AdminPage : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();

    public AdminPage()
	{
		InitializeComponent();
	}
    private async void OnJournalsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AdminJournalsPage());
    }
    private async void OnUsersClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AdminUsersPage());
    }
    private async void OnSubjectsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AdminSubjectsPage());
    }


}