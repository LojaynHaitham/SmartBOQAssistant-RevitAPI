using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using NGU.Lab13.Models;
using NGU.Lab13.Helpers;
using NGU.Lab13.Views;

namespace NGU.Lab13.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly Document _doc;
        private readonly string _settingsDirectory;
        private readonly string _settingsFilePath;

        private string _projectName = "No Active Project";
        private string _filePath = "N/A";
        private string _statusMessage = "Ready. Click 'Scan Model' to begin.";
        private string _geminiApiKey = string.Empty;
        private int _totalRooms;
        private double _totalArea;
        private bool _isBusy;
        private ObservableCollection<BoqItem> _boqItems = new ObservableCollection<BoqItem>();

        public DashboardViewModel(Document doc)
        {
            _doc = doc ?? throw new ArgumentNullException(nameof(doc));

            _settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SmartBoqAssistant");
            _settingsFilePath = Path.Combine(_settingsDirectory, "settings.json");

            ProjectName = _doc.Title;
            FilePath = string.IsNullOrEmpty(_doc.PathName) ? "Unsaved Project" : _doc.PathName;

            ScanModelCommand = new RelayCommand(ScanModel, _ => !IsBusy);
            GenerateBoqCommand = new RelayCommand(GenerateBoq, _ => !IsBusy);
            GenerateAiReportCommand = new RelayCommand(GenerateAiReport, _ => !IsBusy);
            ExportCsvCommand = new RelayCommand(ExportCsv, _ => !IsBusy);
            SaveApiKeyCommand = new RelayCommand(SaveApiKey, _ => !IsBusy);

            LoadApiKey();
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string GeminiApiKey
        {
            get => _geminiApiKey;
            set
            {
                if (_geminiApiKey != value)
                {
                    _geminiApiKey = value.Trim();
                    OnPropertyChanged();
                }
            }
        }

        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (_projectName != value)
                {
                    _projectName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TotalRooms
        {
            get => _totalRooms;
            set
            {
                if (_totalRooms != value)
                {
                    _totalRooms = value;
                    OnPropertyChanged();
                }
            }
        }

        public double TotalArea
        {
            get => _totalArea;
            set
            {
                if (_totalArea != value)
                {
                    _totalArea = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalAreaFormatted));
                }
            }
        }

        public string TotalAreaFormatted => $"{TotalArea:N2} m²";

        public ObservableCollection<BoqItem> BoqItems
        {
            get => _boqItems;
            set
            {
                _boqItems = value;
                OnPropertyChanged();
            }
        }

        public ICommand ScanModelCommand { get; }
        public ICommand GenerateBoqCommand { get; }
        public ICommand GenerateAiReportCommand { get; }
        public ICommand ExportCsvCommand { get; }
        public ICommand SaveApiKeyCommand { get; }

        private void LoadApiKey()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string jsonContent = File.ReadAllText(_settingsFilePath);
                    var config = JsonSerializer.Deserialize<ApiConfig>(jsonContent);

                    if (config != null)
                    {
                        GeminiApiKey = config.GeminiApiKey;
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load settings: {ex.Message}";
            }
        }

        private void SaveApiKey()
        {
            try
            {
                if (!Directory.Exists(_settingsDirectory))
                {
                    Directory.CreateDirectory(_settingsDirectory);
                }

                var config = new ApiConfig { GeminiApiKey = GeminiApiKey };
                string jsonContent = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, jsonContent);

                StatusMessage = "Gemini API Key saved successfully!";
                MessageBox.Show("Gemini API Key has been saved locally.", "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to save key: {ex.Message}";
                MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ScanModel()
        {
            try
            {
                StatusMessage = "Scanning Revit model for rooms...";
                BoqItems.Clear();

                var roomCollector = new FilteredElementCollector(_doc)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .Cast<Room>()
                    .ToList();

                double totalAreaSum = 0;
                int count = 0;

                foreach (Room room in roomCollector)
                {
                    if (room.Area <= 0)
                    {
                        continue;
                    }

                    string name = room.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString() ?? "Unnamed Room";
                    string number = room.get_Parameter(BuiltInParameter.ROOM_NUMBER)?.AsString() ?? "No Number";
                    double areaInSqm = room.Area * 0.092903;

                    Level? level = _doc.GetElement(room.LevelId) as Level;
                    string levelName = level?.Name ?? "Unknown Level";

                    BoqItems.Add(new BoqItem
                    {
                        RoomName = name,
                        RoomNumber = number,
                        Area = areaInSqm,
                        Level = levelName
                    });

                    totalAreaSum += areaInSqm;
                    count++;
                }

                TotalRooms = count;
                TotalArea = totalAreaSum;

                StatusMessage = $"Scan complete. Found {count} valid rooms. Total Area: {TotalAreaFormatted}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Scan failed: {ex.Message}";
                MessageBox.Show($"An error occurred while scanning: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateBoq()
        {
            if (!BoqItems.Any())
            {
                StatusMessage = "No rooms scanned. Please click 'Scan Model' first.";
                MessageBox.Show("Please scan the Revit model before generating a BOQ report.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                StatusMessage = "Generating Bill of Quantities...";

                var groupedRooms = BoqItems
                    .GroupBy(r => r.RoomName)
                    .Select(g => new
                    {
                        RoomName = g.Key,
                        Count = g.Count(),
                        TotalArea = g.Sum(r => r.Area)
                    })
                    .OrderBy(g => g.RoomName);

                string report = "BILL OF QUANTITIES SUMMARY\n";
                report += $"Project: {ProjectName}\n";
                report += $"Generated On: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
                report += new string('-', 45) + "\n";
                report += string.Format("{0,-20} | {1,-6} | {2,-12}\n", "Room Type", "Count", "Total Area");
                report += new string('-', 45) + "\n";

                foreach (var group in groupedRooms)
                {
                    report += string.Format(
                        "{0,-20} | {1,6} | {2,9:F2} m²\n",
                        group.RoomName.Length > 20 ? group.RoomName.Substring(0, 17) + "..." : group.RoomName,
                        group.Count,
                        group.TotalArea);
                }

                report += new string('-', 45) + "\n";
                report += $"Total Quantities: {TotalRooms} rooms | Total Floor Area: {TotalAreaFormatted}\n";

                MessageBox.Show(report, "Bill of Quantities Report", MessageBoxButton.OK, MessageBoxImage.Information);
                StatusMessage = "BOQ report generated successfully!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"BOQ generation failed: {ex.Message}";
            }
        }

        private async void GenerateAiReport()
        {
            if (!BoqItems.Any())
            {
                StatusMessage = "No rooms scanned. Please click 'Scan Model' first.";
                MessageBox.Show("Please scan the Revit model before running AI analysis.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(GeminiApiKey))
            {
                StatusMessage = "Gemini API Key missing. Please expand configuration and paste it.";
                MessageBox.Show(
                    "Google Gemini API Key is missing.\n\nPaste your API key in the Gemini API Configuration panel, then click Save API Key.",
                    "API Key Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Preparing room database and AI prompt...";

                var top10Rooms = BoqItems
                    .OrderByDescending(r => r.Area)
                    .Take(10)
                    .ToList();

                var groupedSummary = BoqItems
                    .GroupBy(r => r.RoomName)
                    .Select(g => new
                    {
                        RoomName = g.Key,
                        Count = g.Count(),
                        TotalArea = g.Sum(r => r.Area)
                    })
                    .OrderByDescending(g => g.TotalArea)
                    .ToList();

                var promptBuilder = new StringBuilder();

                promptBuilder.AppendLine("You are an expert BIM Manager, Architect, and Quantity Surveyor.");
                promptBuilder.AppendLine("Analyze this Revit room BOQ summary and generate a professional report.");
                promptBuilder.AppendLine();
                promptBuilder.AppendLine($"Project Name: {ProjectName}");
                promptBuilder.AppendLine($"Total Room Count: {TotalRooms}");
                promptBuilder.AppendLine($"Total Floor Area: {TotalAreaFormatted}");
                promptBuilder.AppendLine();

                promptBuilder.AppendLine("Top 10 Largest Rooms:");
                foreach (var item in top10Rooms)
                {
                    promptBuilder.AppendLine($"- {item.RoomName}, Number: {item.RoomNumber}, Area: {item.Area:F2} m², Level: {item.Level}");
                }

                promptBuilder.AppendLine();
                promptBuilder.AppendLine("Room Type and Quantity Summary:");
                foreach (var group in groupedSummary)
                {
                    promptBuilder.AppendLine($"- {group.RoomName}: Count {group.Count}, Total Area {group.TotalArea:F2} m²");
                }

                promptBuilder.AppendLine("You are a senior BIM Manager, Architect, Quantity Surveyor and Construction Cost Consultant.");

                promptBuilder.AppendLine("Analyze the provided Revit room database.");

                promptBuilder.AppendLine("Do not write generic BIM explanations.");

                promptBuilder.AppendLine("Base every observation on the provided room data.");

                promptBuilder.AppendLine("");

                promptBuilder.AppendLine("Generate:");

                promptBuilder.AppendLine("1. Executive Summary");
                promptBuilder.AppendLine("2. Floors with Highest Area");
                promptBuilder.AppendLine("3. Occupancy Estimate");
                promptBuilder.AppendLine("4. Space Optimization Analysis");
                promptBuilder.AppendLine("5. Architectural Recommendations");
                promptBuilder.AppendLine("6. Cost Optimization Opportunities");
                promptBuilder.AppendLine("7. BIM Data Quality Review");
                promptBuilder.AppendLine("8. Potential Design Risks");
                promptBuilder.AppendLine("9. Sustainability Recommendations");
                promptBuilder.AppendLine("10. Final Professional Conclusion");

                promptBuilder.AppendLine("");
                promptBuilder.AppendLine("Use bullet points.");
                promptBuilder.AppendLine("Reference actual room names.");
                promptBuilder.AppendLine("Reference actual room areas.");
                promptBuilder.AppendLine("Point out unusually large or unusually small rooms.");
                promptBuilder.AppendLine("Identify possible duplicated spaces.");
                promptBuilder.AppendLine("Keep recommendations practical.");

                string promptText = promptBuilder.ToString();

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = promptText
                                }
                            }
                        }
                    }
                };

                string jsonRequest = JsonSerializer.Serialize(requestBody);

                StatusMessage = "Connecting to Gemini API and generating AI report...";

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(120);

                    var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={GeminiApiKey}";

                    HttpResponseMessage response = await client.PostAsync(url, httpContent);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorResponseContent = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Gemini API returned status code {response.StatusCode}. Details: {errorResponseContent}");
                    }

                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    using (var responseDoc = JsonDocument.Parse(jsonResponse))
                    {
                        var root = responseDoc.RootElement;

                        if (root.TryGetProperty("candidates", out var candidates) &&
                            candidates.GetArrayLength() > 0 &&
                            candidates[0].TryGetProperty("content", out var responseContent) &&
                            responseContent.TryGetProperty("parts", out var parts) &&
                            parts.GetArrayLength() > 0 &&
                            parts[0].TryGetProperty("text", out var textElement))
                        {
                            string aiReport = textElement.GetString() ?? "Received empty response from Gemini.";

                            StatusMessage = "AI Report generated successfully!";

                            var reportWindow = new AiReportWindow(aiReport);

                            foreach (Window win in Application.Current.Windows)
                            {
                                if (win.IsActive)
                                {
                                    reportWindow.Owner = win;
                                    break;
                                }
                            }

                            reportWindow.ShowDialog();
                        }
                        else
                        {
                            throw new Exception("Gemini API request succeeded, but the response text could not be parsed.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Gemini AI error: {ex.Message}";
                MessageBox.Show(
                    $"Failed to generate AI report.\n\nError details:\n{ex.Message}",
                    "Gemini API Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ExportCsv()
        {
            if (!BoqItems.Any())
            {
                StatusMessage = "No rooms scanned. Please click 'Scan Model' first.";
                MessageBox.Show("Please scan the Revit model before exporting to CSV.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    DefaultExt = ".csv",
                    FileName = $"{ProjectName}_BOQ_Rooms.csv",
                    Title = "Export BOQ Rooms to CSV"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    StatusMessage = $"Exporting BOQ to CSV: {Path.GetFileName(filePath)}...";

                    using (var writer = new StreamWriter(filePath))
                    {
                        writer.WriteLine("Room Name,Room Number,Area (sqm),Level");

                        foreach (var item in BoqItems)
                        {
                            string escapedName = item.RoomName.Contains(",") ? $"\"{item.RoomName}\"" : item.RoomName;
                            string escapedLevel = item.Level.Contains(",") ? $"\"{item.Level}\"" : item.Level;

                            writer.WriteLine($"{escapedName},{item.RoomNumber},{item.Area:F4},{escapedLevel}");
                        }
                    }

                    StatusMessage = $"Successfully exported to: {Path.GetFileName(filePath)}";
                    MessageBox.Show($"Room BOQ successfully exported to:\n{filePath}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"CSV Export failed: {ex.Message}";
                MessageBox.Show($"Failed to export CSV: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ApiConfig
    {
        public string GeminiApiKey { get; set; } = string.Empty;
    }
}