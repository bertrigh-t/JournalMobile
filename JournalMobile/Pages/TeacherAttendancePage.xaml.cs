using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using JournalMobile.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace JournalMobile.Pages;

public partial class TeacherAttendancePage : ContentPage
{
    private readonly HttpClient _httpClient = new();

    private List<GroupAttendanceItem> _groups = new();
    private List<JournalAttendanceItem> _journals = new();
    private List<SemesterAttendanceItem> _semesters = new();
    private List<StudentItem> _students = new();
    private List<string> _dates = new();

    private int _selectedGroupId;
    private int _selectedJournalId;
    private int _selectedSemesterId;

    public TeacherAttendancePage()
    {
        InitializeComponent();

        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSemesters();
        await LoadGroups();
    }

    private async Task LoadGroups()
    {
        try
        {
            GroupPicker.Items.Clear();
            JournalPicker.Items.Clear();
            AttendanceTable.Children.Clear();

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

            _groups = JsonSerializer.Deserialize<List<GroupAttendanceItem>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<GroupAttendanceItem>();

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
    private async Task LoadSemesters()
    {
        try
        {
            SemesterPicker.Items.Clear();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:7070/Semesters"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert(
                    "Ошибка семестров",
                    json,
                    "OK"
                );

                return;
            }

            _semesters = JsonSerializer.Deserialize<List<SemesterAttendanceItem>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new();

            foreach (var semester in _semesters)
            {
                SemesterPicker.Items.Add(
                    $"{FormatDateShort(semester.Start_date)} - {FormatDateShort(semester.End_date)}"
                );
            }
            if (_semesters.Count > 0)
                SemesterPicker.SelectedIndex = 0;
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

    private async void OnGroupChanged(object sender, EventArgs e)
    {
        if (GroupPicker.SelectedIndex < 0 || GroupPicker.SelectedIndex >= _groups.Count)
            return;

        var group = _groups[GroupPicker.SelectedIndex];
        _selectedGroupId = group.Id;

        await LoadStudents(group.Id);
        await LoadJournals(group.Id);
    }
    private async void OnSemesterChanged(object sender, EventArgs e)
    {
        if (SemesterPicker.SelectedIndex < 0)
            return;

        var semester = _semesters[SemesterPicker.SelectedIndex];

        _selectedSemesterId = semester.Id;

        if (_selectedGroupId > 0)
            await LoadJournals(_selectedGroupId);
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
            AttendanceTable.Children.Clear();
            _journals.Clear();
            _selectedJournalId = 0;

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://localhost:7070/Groups/{groupId}/journals?semesterId={_selectedSemesterId}"
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

            _journals = JsonSerializer.Deserialize<List<JournalAttendanceItem>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<JournalAttendanceItem>();

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

        await LoadAttendance();
    }

    private async Task LoadAttendance()
    {
        if (_selectedJournalId <= 0)
            return;

        try
        {
            AttendanceTable.Children.Clear();
            AttendanceTable.RowDefinitions.Clear();
            AttendanceTable.ColumnDefinitions.Clear();

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://localhost:7070/Attendance/journal/{_selectedJournalId}"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Ошибка посещаемости", $"{response.StatusCode}\n{json}", "OK");
                return;
            }

            var attendance = JsonSerializer.Deserialize<List<TeacherAttendanceItem>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<TeacherAttendanceItem>();

            BuildAttendanceTable(attendance);
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
            var attendanceRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://localhost:7070/attendance/journal/{_selectedJournalId}"
            );

            attendanceRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthService.Token);

            var response = await _httpClient.SendAsync(attendanceRequest);
            var json = await response.Content.ReadAsStringAsync();

            var attendances = JsonSerializer.Deserialize<List<TeacherAttendanceItem>>(
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
                        .Text("Журнал посещаемости")
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
                                    var attendance = attendances.FirstOrDefault(g =>
                                        g.Student == student.Name &&
                                        g.Date == date);

                                    table.Cell().Border(1).Padding(5)
                                        .AlignCenter()
                                        .Text(
                                            attendance == null
                                                ? "-"
                                                : GetStatusText(attendance.Status)
                                        );
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

    private void BuildAttendanceTable(List<TeacherAttendanceItem> attendances)
    {
        AttendanceTable.Children.Clear();
        AttendanceTable.RowDefinitions.Clear();
        AttendanceTable.ColumnDefinitions.Clear();

        var students = _students
            .OrderBy(s => s.Name)
            .ToList();

        if (_dates.Count == 0)
        {
            _dates = attendances
                .Select(g => g.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }
        if (students.Count == 0)
        {
            AttendanceTable.RowDefinitions.Add(new RowDefinition { Height = 45 });
            AttendanceTable.ColumnDefinitions.Add(new ColumnDefinition { Width = 260 });
            AddCell("Студенты не найдены", 0, 0, true);
            return;
        }

        if (_dates.Count == 0)
        {
            _dates.Add(DateTime.Now.ToString("yyyy-MM-dd"));
        }

        AttendanceTable.ColumnDefinitions.Add(new ColumnDefinition { Width = 150 });

        foreach (var date in _dates)
            AttendanceTable.ColumnDefinitions.Add(new ColumnDefinition { Width = 80 });

        AttendanceTable.RowDefinitions.Add(new RowDefinition { Height = 48 });

        AddCell("Студент", 0, 0, true);

        for (int i = 0; i < _dates.Count; i++)
            AddCell(FormatDateShort(_dates[i]), i + 1, 0, true);

        for (int row = 0; row < students.Count; row++)
        {
            AttendanceTable.RowDefinitions.Add(new RowDefinition { Height = 48 });

            var student = students[row];

            AddCell(student.Name, 0, row + 1, true);

            for (int col = 0; col < _dates.Count; col++)
            {
                var date = _dates[col];

                var attendance = attendances.FirstOrDefault(g =>
                    g.Student == student.Name &&
                    g.Date == date);

                if (attendance == null)
                    AddEmptyAttendanceCell(student.Id, date, col + 1, row + 1);
                else
                    AddAttendanceCell(attendance, col + 1, row + 1);
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

        AttendanceTable.Children.Add(border);
    }

    private void AddEmptyAttendanceCell(int studentId, string date, int column, int row)
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
            await AddAttendance(studentId, date);
        };

        border.GestureRecognizers.Add(tap);

        Grid.SetColumn(border, column);
        Grid.SetRow(border, row);

        AttendanceTable.Children.Add(border);
    }

    private void AddAttendanceCell(TeacherAttendanceItem attendance, int column, int row)
    {
        var border = new Border
        {
            BackgroundColor = attendance.StatusColor,
            Stroke = Microsoft.Maui.Graphics.Color.FromArgb("#B7C9DD"),
            StrokeThickness = 1,
            Padding = 8,
            Content = new Label
            {
                Text = GetStatusText(attendance.Status).ToString(),
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) =>
        {
            await EditAttendance(attendance);
        };

        border.GestureRecognizers.Add(tap);

        Grid.SetColumn(border, column);
        Grid.SetRow(border, row);

        AttendanceTable.Children.Add(border);
    }

    private async Task AddAttendance(int studentId, string date)
    {
        var selected = await DisplayActionSheet(
            "Выберите статус",
            "Отмена",
            null,
            "Присутствовал",
            "Отсутствовал",
            "Опоздал",
            "Уважительная причина"
        );

        if (string.IsNullOrWhiteSpace(selected) || selected == "Отмена")
            return;

        string status = selected switch
        {
            "Присутствовал" => "present",
            "Отсутствовал" => "absent",
            "Опоздал" => "late",
            "Уважительная причина" => "excused",
            _ => ""
        };
        var requestData = new
        {
            studentId,
            journalId = _selectedJournalId,
            status,
            date
        };

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://localhost:7070/Attendance"
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
                "Ошибка добавления",
                $"{response.StatusCode}\n{json}",
                "OK"
            );
            return;
        }

        await LoadAttendance();
    }
    private async Task EditAttendance(TeacherAttendanceItem attendance)
    {
        var selected = await DisplayActionSheet(
            $"Статус для {attendance.Student}",
            "Отмена",
            null,
            "Присутствовал",
            "Отсутствовал",
            "Опоздал",
            "Уважительная причина"
        );

        if (string.IsNullOrWhiteSpace(selected) || selected == "Отмена")
            return;

        string status = selected switch
        {
            "Присутствовал" => "present",
            "Отсутствовал" => "absent",
            "Опоздал" => "late",
            "Уважительная причина" => "excused",
            _ => ""
        };

        await UpdateAttendance(attendance.Id, status);
    }
    private async Task UpdateAttendance(int id, string status)
    {
        try
        {
            var requestData = new
            {
                status,
                date = DateTime.Now.ToString("yyyy-MM-dd")
            };

            var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"https://localhost:7070/attendance/{id}"
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

            await LoadAttendance();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка изменения оценки", ex.Message, "OK");
        }
    }
    private string FormatDateShort(string date)
    {
        if (DateTime.TryParse(date, out var parsedDate))
            return parsedDate.ToString("dd.MM.yy");

        return date;
    }
    private string GetStatusText(string status)
    {
        return status switch
        {
            "present" => "Присутствовал",
            "absent" => "Отсутствовал",
            "late" => "Опоздал",
            "excused" => "Уважительная причина",
            _ => status
        };
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

        await LoadAttendance();
    }
}

public class GroupAttendanceItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class JournalAttendanceItem
{
    public int Id { get; set; }
    public string Subject { get; set; } = "";
}
public class SemesterAttendanceItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    public string Start_date { get; set; } = "";
    public string End_date { get; set; } = "";
}
public class TeacherAttendanceItem
{
    public int Id { get; set; }

    public string Student { get; set; } = "";

    public string Date { get; set; } = "";

    public string Status { get; set; } = "";

    public Microsoft.Maui.Graphics.Color StatusColor => Status switch
    {
        "absent" => Microsoft.Maui.Graphics.Color.FromArgb("#E57373"),
        "late" => Microsoft.Maui.Graphics.Color.FromArgb("#FFB74D"),
        "excused" => Microsoft.Maui.Graphics.Color.FromArgb("#90CAF9"),
        "present" => Microsoft.Maui.Graphics.Color.FromArgb("#8BC34A"),
        _ => Microsoft.Maui.Graphics.Colors.Gray
    };
}
