using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using JournalMobile.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace JournalMobile.Pages;

public partial class TeacherGradesPage : ContentPage
{
    private readonly HttpClient _httpClient = new();

    private List<GroupItem> _groups = new();
    private List<JournalItem> _journals = new();
    private List<StudentItem> _students = new();
    private List<string> _dates = new();

    private int _selectedGroupId;
    private int _selectedJournalId;

    public TeacherGradesPage()
    {
        InitializeComponent();

        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadGroups();
    }

    private async Task LoadGroups()
    {
        try
        {
            GroupPicker.Items.Clear();
            JournalPicker.Items.Clear();
            GradesTable.Children.Clear();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:7070/Groups"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Ошибка групп", $"{response.StatusCode}\n{json}", "OK");
                return;
            }

            _groups = JsonSerializer.Deserialize<List<GroupItem>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<GroupItem>();

            foreach (var group in _groups)
                GroupPicker.Items.Add(group.Name);

            if (_groups.Count > 0)
                GroupPicker.SelectedIndex = 0;
            else
                await DisplayAlert("Группы", "Группы не найдены", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка загрузки групп", ex.Message, "OK");
        }
    }

    private async void OnGroupChanged(object sender, EventArgs e)
    {
        if (GroupPicker.SelectedIndex < 0 || GroupPicker.SelectedIndex >= _groups.Count)
            return;

        var group = _groups[GroupPicker.SelectedIndex];
        _selectedGroupId = group.Id;

        await LoadStudents(group.Id);
        await LoadJournals(group.Id);
    }

    private async Task LoadStudents(int groupId)
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://localhost:7070/Groups/{groupId}/students"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Ошибка студентов", $"{response.StatusCode}\n{json}", "OK");
                return;
            }

            _students = JsonSerializer.Deserialize<List<StudentItem>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<StudentItem>();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка загрузки студентов", ex.Message, "OK");
        }
    }

    private async Task LoadJournals(int groupId)
    {
        try
        {
            JournalPicker.Items.Clear();
            GradesTable.Children.Clear();
            _journals.Clear();
            _selectedJournalId = 0;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://localhost:7070/Groups/{groupId}/journals"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Ошибка предметов", $"{response.StatusCode}\n{json}", "OK");
                return;
            }

            _journals = JsonSerializer.Deserialize<List<JournalItem>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<JournalItem>();

            foreach (var journal in _journals)
                JournalPicker.Items.Add(journal.Subject);

            if (_journals.Count > 0)
                JournalPicker.SelectedIndex = 0;
            else
                await DisplayAlert("Предметы", "Предметы для группы не найдены", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка загрузки предметов", ex.Message, "OK");
        }
    }

    private async void OnJournalChanged(object sender, EventArgs e)
    {
        if (JournalPicker.SelectedIndex < 0 || JournalPicker.SelectedIndex >= _journals.Count)
            return;

        var journal = _journals[JournalPicker.SelectedIndex];
        _selectedJournalId = journal.Id;

        await LoadGrades();
    }

    private async Task LoadGrades()
    {
        if (_selectedJournalId <= 0)
            return;

        try
        {
            GradesTable.Children.Clear();
            GradesTable.RowDefinitions.Clear();
            GradesTable.ColumnDefinitions.Clear();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://localhost:7070/Grades/journal/{_selectedJournalId}"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Ошибка оценок", $"{response.StatusCode}\n{json}", "OK");
                return;
            }

            var grades = JsonSerializer.Deserialize<List<TeacherGradeItem>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<TeacherGradeItem>();

            BuildGradesTable(grades);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка загрузки оценок", ex.Message, "OK");
        }
    }
    private async void OnExportPdfClicked(object sender, EventArgs e)
    {
        try
        {
            var gradesRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://localhost:7070/Grades/journal/{_selectedJournalId}"
            );

            gradesRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var response = await _httpClient.SendAsync(gradesRequest);
            var json = await response.Content.ReadAsStringAsync();

            var grades = JsonSerializer.Deserialize<List<TeacherGradeItem>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new();

            var students = _students
                .OrderBy(s => s.Name)
                .ToList();

            var dates = _dates
                .OrderBy(d => d)
                .ToList();

            string downloadsPath;

#if ANDROID
downloadsPath = Android.OS.Environment
    .GetExternalStoragePublicDirectory(
        Android.OS.Environment.DirectoryDownloads
    )!
    .AbsolutePath;
#else
            downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads"
            );
            #endif

            string filePath = Path.Combine(downloadsPath, "journal.pdf");
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    page.Header()
                        .Text("Журнал оценок")
                        .FontSize(24)
                        .Bold();

                    page.Content().Column(column =>
                    {
                        column.Item().Text($"Группа: {GroupPicker.SelectedItem}");
                        column.Item().Text($"Предмет: {JournalPicker.SelectedItem}");

                        column.Item().PaddingTop(15);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(140);

                                foreach (var _ in dates)
                                    columns.ConstantColumn(55);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Border(1).Padding(5)
                                    .Text("Студент").Bold();

                                foreach (var date in dates)
                                {
                                    header.Cell().Border(1).Padding(5)
                                        .Text(FormatDateShort(date))
                                        .Bold();
                                }
                            });

                            foreach (var student in students)
                            {
                                table.Cell().Border(1).Padding(5)
                                    .Text(student.Name);

                                foreach (var date in dates)
                                {
                                    var grade = grades.FirstOrDefault(g =>
                                        g.Student == student.Name &&
                                        g.Date == date);

                                    table.Cell().Border(1).Padding(5)
                                        .AlignCenter()
                                        .Text(grade?.Grade.ToString() ?? "-");
                                }
                            }
                        });
                    });
                });
            })
            .GeneratePdf(filePath);

            await DisplayAlert(
                "PDF сохранён",
                $"Файл сохранён:\n{filePath}",
                "OK"
            );

            await Launcher.Default.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка PDF", ex.Message, "OK");
        }
    }

    private void BuildGradesTable(List<TeacherGradeItem> grades)
    {
        GradesTable.Children.Clear();
        GradesTable.RowDefinitions.Clear();
        GradesTable.ColumnDefinitions.Clear();

        var students = _students
            .OrderBy(s => s.Name)
            .ToList();

        if (_dates.Count == 0)
        {
            _dates = grades
                .Select(g => g.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }
        if (students.Count == 0)
        {
            GradesTable.RowDefinitions.Add(new RowDefinition { Height = 45 });
            GradesTable.ColumnDefinitions.Add(new ColumnDefinition { Width = 260 });
            AddCell("Студенты не найдены", 0, 0, true);
            return;
        }

        if (_dates.Count == 0)
        {
            _dates.Add(DateTime.Now.ToString("yyyy-MM-dd"));
        }

        GradesTable.ColumnDefinitions.Add(new ColumnDefinition { Width = 150 });

        foreach (var date in _dates)
            GradesTable.ColumnDefinitions.Add(new ColumnDefinition { Width = 80 });

        GradesTable.RowDefinitions.Add(new RowDefinition { Height = 48 });

        AddCell("Студент", 0, 0, true);

        for (int i = 0; i < _dates.Count; i++)
            AddCell(FormatDateShort(_dates[i]), i + 1, 0, true);

        for (int row = 0; row < students.Count; row++)
        {
            GradesTable.RowDefinitions.Add(new RowDefinition { Height = 48 });

            var student = students[row];

            AddCell(student.Name, 0, row + 1, true);

            for (int col = 0; col < _dates.Count; col++)
            {
                var date = _dates[col];

                var grade = grades.FirstOrDefault(g =>
                    g.Student == student.Name &&
                    g.Date == date);

                if (grade == null)
                    AddEmptyGradeCell(student.Id, date, col + 1, row + 1);
                else
                    AddGradeCell(grade, col + 1, row + 1);
            }
        }
    }

    private void AddCell(string text, int column, int row, bool isHeader)
    {
        var border = new Border
        {
            BackgroundColor = isHeader
                ? Microsoft.Maui.Graphics.Color.FromArgb("#AFC7FF")
                : Microsoft.Maui.Graphics.Color.FromArgb("#EAF3FF"),
            Stroke = Microsoft.Maui.Graphics.Color.FromArgb("#B7C9DD"),
            StrokeThickness = 1,
            Padding = 8,
            Content = new Label
            {
                Text = text,
                TextColor = Colors.Black,
                FontAttributes = isHeader ? FontAttributes.Bold : FontAttributes.None,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        Grid.SetColumn(border, column);
        Grid.SetRow(border, row);

        GradesTable.Children.Add(border);
    }

    private void AddEmptyGradeCell(int studentId, string date, int column, int row)
    {
        var border = new Border
        {
            BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#EAF3FF"),
            Stroke = Microsoft.Maui.Graphics.Color.FromArgb("#B7C9DD"),
            StrokeThickness = 1,
            Padding = 8,
            Content = new Label
            {
                Text = "+",
                TextColor = Colors.Gray,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) =>
        {
            await AddGrade(studentId, date);
        };

        border.GestureRecognizers.Add(tap);

        Grid.SetColumn(border, column);
        Grid.SetRow(border, row);

        GradesTable.Children.Add(border);
    }

    private void AddGradeCell(TeacherGradeItem grade, int column, int row)
    {
        var border = new Border
        {
            BackgroundColor = grade.GradeColor,
            Stroke = Microsoft.Maui.Graphics.Color.FromArgb("#B7C9DD"),
            StrokeThickness = 1,
            Padding = 8,
            Content = new Label
            {
                Text = grade.Grade.ToString(),
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) =>
        {
            await EditGrade(grade);
        };

        border.GestureRecognizers.Add(tap);

        Grid.SetColumn(border, column);
        Grid.SetRow(border, row);

        GradesTable.Children.Add(border);
    }

    private async Task AddGrade(int studentId, string date)
    {
        var result = await DisplayPromptAsync(
            "Добавление оценки",
            "Введите оценку:",
            maxLength: 1,
            keyboard: Keyboard.Numeric
        );

        if (string.IsNullOrWhiteSpace(result))
            return;

        if (!int.TryParse(result, out int grade) || grade < 2 || grade > 5)
        {
            await DisplayAlert("Ошибка", "Введите оценку от 2 до 5", "OK");
            return;
        }

        var requestData = new
        {
            studentId,
            journalId = _selectedJournalId,
            grade,
            date
        };

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://localhost:7070/Grades"
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
            await DisplayAlert("Ошибка добавления", $"{response.StatusCode}\n{json}", "OK");
            return;
        }

        await LoadGrades();
    }

    private async Task EditGrade(TeacherGradeItem grade)
    {
        var result = await DisplayPromptAsync(
            "Изменение оценки",
            $"{grade.Student}\nТекущая оценка: {grade.Grade}\nВведите новую оценку:",
            initialValue: grade.Grade.ToString(),
            maxLength: 1,
            keyboard: Keyboard.Numeric
        );

        if (string.IsNullOrWhiteSpace(result))
            return;

        if (!int.TryParse(result, out int newGrade) || newGrade < 2 || newGrade > 5)
        {
            await DisplayAlert("Ошибка", "Введите оценку от 2 до 5", "OK");
            return;
        }

        await UpdateGrade(grade.Id, newGrade);
    }

    private async Task UpdateGrade(int id, int grade)
    {
        try
        {
            var requestData = new
            {
                grade,
                date = DateTime.Now.ToString("yyyy-MM-dd")
            };

            var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"https://localhost:7070/Grades/{id}"
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
                await DisplayAlert("Ошибка изменения", $"{response.StatusCode}\n{json}", "OK");
                return;
            }

            await LoadGrades();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка изменения оценки", ex.Message, "OK");
        }
    }

    private string FormatDateShort(string date)
    {
        if (DateTime.TryParse(date, out var parsedDate))
            return parsedDate.ToString("dd.MM");

        return date;
    }

    private async void OnAddDateClicked(object sender, EventArgs e)
    {
        var result = await DisplayPromptAsync(
            "Новая дата",
            "Введите дату в формате ГГГГ-ММ-ДД",
            initialValue: DateTime.Now.ToString("yyyy-MM-dd")
        );

        if (string.IsNullOrWhiteSpace(result))
            return;

        if (!DateTime.TryParse(result, out _))
        {
            await DisplayAlert("Ошибка", "Неверный формат даты", "OK");
            return;
        }

        if (_dates.Contains(result))
        {
            await DisplayAlert("Ошибка", "Такая дата уже есть", "OK");
            return;
        }

        _dates.Add(result);

        _dates = _dates
            .OrderBy(d => d)
            .ToList();

        await LoadGrades();
    }
}

public class GroupItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class JournalItem
{
    public int Id { get; set; }
    public string Subject { get; set; } = "";
}
public class TeacherGradeItem
{
    public int Id { get; set; }
    public string Student { get; set; } = "";
    public int Grade { get; set; }
    public string Date { get; set; } = "";

    public Microsoft.Maui.Graphics.Color GradeColor => Grade switch
    {
        5 => Microsoft.Maui.Graphics.Color.FromArgb("#8BC34A"),
        4 => Microsoft.Maui.Graphics.Color.FromArgb("#CDDC39"),
        3 => Microsoft.Maui.Graphics.Color.FromArgb("#FFB74D"),
        2 => Microsoft.Maui.Graphics.Color.FromArgb("#E57373"),
        _ => Microsoft.Maui.Graphics.Colors.Gray
    };
}