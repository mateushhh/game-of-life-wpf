using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GameOfLife
{
    public partial class MainWindow : Window
    {
        public SimulationParameters CurrentParameters { get; private set; } = new SimulationParameters();
        private readonly SimulationEngine _engine = new SimulationEngine();
        public int BoardWidth { get; set; } = 30;
        public int BoardHeight { get; set; } = 30;
        private const int CellSize = 20;

        private double zoom = 1.0;
        private Cell[,] cells;

        private DispatcherTimer _simulationTimer;
        private bool _isRunning;
        private int _ticks = 0;

        private int _alivePlants = 0;
        private int _bornPlants = 0;
        private int _deadPlants = 0;
        private Brush _plantBrush = Brushes.Green;

        private int _aliveAnimals = 0;
        private int _bornAnimals = 0;
        private int _deadAnimals = 0;
        private Brush _animalBrush = Brushes.Red;

        public MainWindow()
        {
            InitializeComponent();
            ReadParametersFromUI();
            UpdateDimensionLabels();

            cells = new Cell[BoardHeight, BoardWidth];

            for (int y = 0; y < BoardHeight; y++)
                for (int x = 0; x < BoardWidth; x++)
                    cells[y, x] = new Cell();

            BoardCanvas.Width = BoardWidth * CellSize;
            BoardCanvas.Height = BoardHeight * CellSize;

            DrawBoard();

            _simulationTimer = new DispatcherTimer();
            _simulationTimer.Tick += SimulationTimer_Tick;

            UpdateTimerInterval();
            UpdateControlStates();
            UpdateStatistics();
        }

        private void StepSimulation()
        {
            cells = _engine.Step(cells, CurrentParameters);

            SimulationStatistics stats = _engine.Statistics;

            _ticks = stats.Ticks;

            _bornPlants = stats.BornPlants;
            _deadPlants = stats.DeadPlants;

            _bornAnimals = stats.BornAnimals;
            _deadAnimals = stats.DeadAnimals;

            UpdateStatistics();
            DrawBoard();
        }

        private void DrawBoard()
        {
            if (cells == null || BoardCanvas == null)
                return;

            BoardCanvas.Children.Clear();

            BoardCanvas.Width = BoardWidth * CellSize;
            BoardCanvas.Height = BoardHeight * CellSize;

            for (int y = 0; y < BoardHeight; y++)
            {
                for (int x = 0; x < BoardWidth; x++)
                {
                    Cell cell = cells[y, x];

                    if (cell.Plant != null)
                    {
                        var plantShape = new Rectangle
                        {
                            Width = CellSize,
                            Height = CellSize,
                            Fill = _plantBrush
                        };

                        Canvas.SetLeft(plantShape, x * CellSize);
                        Canvas.SetTop(plantShape, y * CellSize);

                        BoardCanvas.Children.Add(plantShape);
                    }

                    if (cell.Animal != null)
                    {
                        var animalShape = new Ellipse
                        {
                            Width = CellSize * 0.70,
                            Height = CellSize * 0.70,
                            Fill = _animalBrush
                        };

                        Canvas.SetLeft(animalShape, x * CellSize + CellSize * 0.15);
                        Canvas.SetTop(animalShape, y * CellSize + CellSize * 0.15);

                        BoardCanvas.Children.Add(animalShape);
                    }
                }
            }

            for (int x = 0; x <= BoardWidth; x++)
            {
                BoardCanvas.Children.Add(new Line
                {
                    X1 = x * CellSize,
                    Y1 = 0,
                    X2 = x * CellSize,
                    Y2 = BoardHeight * CellSize,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                });
            }

            for (int y = 0; y <= BoardHeight; y++)
            {
                BoardCanvas.Children.Add(new Line
                {
                    X1 = 0,
                    Y1 = y * CellSize,
                    X2 = BoardWidth * CellSize,
                    Y2 = y * CellSize,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                });
            }
        }

        private void SimulationTimer_Tick(object sender, EventArgs e)
        {
            StepSimulation();
        }

        private void UpdateTimerInterval()
        {
            if (_simulationTimer == null || SpeedSlider == null)
                return;

            double speed = Math.Max(1, SpeedSlider.Value);

            _simulationTimer.Interval =
                TimeSpan.FromMilliseconds(1000.0 / speed);
        }

        private void Board_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(BoardCanvas);

            int x = (int)(pos.X / CellSize);
            int y = (int)(pos.Y / CellSize);

            if (x < 0 || x >= BoardWidth || y < 0 || y >= BoardHeight)
                return;

            if (cells[y, x].Plant == null)
                cells[y, x].Plant = new Plant();
            else
                cells[y, x].Plant = null;
            
            DrawBoard();
        }

        private void Board_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(BoardCanvas);

            int x = (int)(pos.X / CellSize);
            int y = (int)(pos.Y / CellSize);

            if (x < 0 || x >= BoardWidth || y < 0 || y >= BoardHeight)
                return;

            if (cells[y, x].Animal == null)
                cells[y, x].Animal = new Animal();
            else
                cells[y, x].Animal = null;

            DrawBoard();
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            zoom = ZoomSlider.Value / 100.0;

            BoardScale.ScaleX = zoom;
            BoardScale.ScaleY = zoom;

            if (ZoomValueText != null)
                ZoomValueText.Text = $"Zoom: {ZoomSlider.Value:F0}%";
        }

        private void BoardScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            double factor = e.Delta > 0 ? 1.1 : 1.0 / 1.1;

            ZoomSlider.Value = Math.Clamp(
                ZoomSlider.Value * factor, 20, 400);

            e.Handled = true;
        }

        private void DimensionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateDimensionLabels();
        }

        private void UpdateDimensionLabels()
        {
            if (WidthValueText != null)
                WidthValueText.Text = ((int)WidthSlider.Value).ToString();
            if (HeightValueText != null)
                HeightValueText.Text = ((int)HeightSlider.Value).ToString();
        }

        private void SpeedSlider_ValueChanged(
    object sender,
    RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateSpeedLabel();

            if (SpeedSlider.Value <= 0 && _isRunning)
            {
                _simulationTimer.Stop();
                _isRunning = false;
                StartStopButton.Content = "START";
            }

            UpdateTimerInterval();
            UpdateControlStates();
        }

        private void UpdateSpeedLabel()
        {
            if (SpeedValueText != null)
                SpeedValueText.Text = ((int)SpeedSlider.Value).ToString();
        }

        private bool ReadParametersFromUI()
        {
            try
            {
                CurrentParameters = new SimulationParameters
                {
                    R = ParseSet(RTextBox.Text, nameof(RTextBox)),
                    W = ParseSet(WTextBox.Text, nameof(WTextBox)),
                    B = ParseSet(BTextBox.Text, nameof(BTextBox)),
                    P = ParseSet(PTextBox.Text, nameof(PTextBox)),
                    Z = ParseSet(ZTextBox.Text, nameof(ZTextBox)),
                    M = ParseNonNegativeInt(MTextBox.Text, "M"),
                    L = ParseNonNegativeInt(LTextBox.Text, "L")
                };

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Wrong Parameters:\n" + ex.Message,
                    "Wrong Parameters",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }
        }

        private void ApplyParametersButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ReadParametersFromUI())
                return;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BoardWidth = (int)WidthSlider.Value;
                BoardHeight = (int)HeightSlider.Value;

                cells = new Cell[BoardHeight, BoardWidth];

                for (int y = 0; y < BoardHeight; y++)
                {
                    for (int x = 0; x < BoardWidth; x++)
                    {
                        cells[y, x] = new Cell();
                    }
                }

                ResetStatistics();
                DrawBoard();
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, "Format Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static HashSet<int> ParseSet(string input, string parameterName)
        {
            var values = new HashSet<int>();
            foreach (string part in input.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!int.TryParse(part.Trim(), out int value) || value < 0)
                    throw new FormatException($"Field {parameterName}: Type in numbers divided with ',', ';' or ' '");
                values.Add(value);
            }

            if (values.Count == 0)
                throw new FormatException($"Field {parameterName} cannot be empty.");
            return values;
        }

        private static int ParseNonNegativeInt(string input, string parameterName)
        {
            if (!int.TryParse(input, out int value) || value < 0)
                throw new FormatException($"Field {parameterName} cannot be a negative integer.");
            return value;
        }


        private Storyboard _pulseStoryboard;

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_pulseStoryboard == null)
            {
                _pulseStoryboard = (Storyboard)FindResource("PulseAnimation");
            }

            if (_isRunning)
            {
                _simulationTimer.Stop();
                _isRunning = false;

                StartStopButton.Content = "START";
                StartStopButton.Background = Brushes.CornflowerBlue;

                _pulseStoryboard.Stop(StartStopButton);
                StartStopScale.ScaleX = 1.0;
                StartStopScale.ScaleY = 1.0;
            }
            else
            {
                UpdateTimerInterval();
                _simulationTimer.Start();
                _isRunning = true;

                StartStopButton.Content = "STOP";
                StartStopButton.Background = Brushes.Crimson;

                _pulseStoryboard.Begin(StartStopButton, true);
            }

            UpdateControlStates();
        }


        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning)
                return;

            StepSimulation();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                for (int x = 0; x < BoardWidth; x++)
                {
                    cells[y, x] = new Cell();
                }
            }

            ResetStatistics();
            DrawBoard();
        }

        private void RandomizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning)
                return;

            Random random = new Random();

            for (int y = 0; y < BoardHeight; y++)
            {
                for (int x = 0; x < BoardWidth; x++)
                {
                    int result = random.Next(100);

                    cells[y, x].Plant = null;
                    cells[y, x].Animal = null;

                    if (result < 35)
                    {
                        cells[y, x].Plant = new Plant();
                    }
                    else if (result < 40)
                    {
                        cells[y, x].Animal = new Animal();
                    }
                }
            }

            ResetStatistics();
            DrawBoard();
        }

        private void UpdateControlStates()
        {
            bool hasSpeed = SpeedSlider.Value > 0;

            StartStopButton.IsEnabled = _isRunning || hasSpeed;

            StepButton.IsEnabled = !_isRunning;

            RandomizeButton.IsEnabled = !_isRunning;
            GenerateButton.IsEnabled = !_isRunning;
            ClearButton.IsEnabled = !_isRunning;
        }

        private void UpdateStatistics()
        {
            int plants = 0;
            int animals = 0;

            for (int y = 0; y < BoardHeight; y++)
            {
                for (int x = 0; x < BoardWidth; x++)
                {
                    if (cells[y, x].Plant != null)
                        plants++;

                    if (cells[y, x].Animal != null)
                        animals++;
                }
            }

            _alivePlants = plants;
            _aliveAnimals = animals;

            TicksText.Text = $"Ticks: {_ticks}";
            AlivePText.Text = $"Alive: {plants}";
            BornPText.Text = $"Born: {_bornPlants}";
            DeathPText.Text = $"Deaths: {_deadPlants}";

            AliveAText.Text = $"Alive: {animals}";
            BornAText.Text = $"Born: {_bornAnimals}";
            DeathAText.Text = $"Deaths: {_deadAnimals}";
        }

        private void ResetStatistics()
        {
            _engine.ResetStatistics();

            _ticks = 0;

            _bornPlants = 0;
            _deadPlants = 0;

            _bornAnimals = 0;
            _deadAnimals = 0;

            UpdateStatistics();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = ".txt"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"R:{string.Join(",", CurrentParameters.R)}");
                    sb.AppendLine($"W:{string.Join(",", CurrentParameters.W)}");
                    sb.AppendLine($"B:{string.Join(",", CurrentParameters.B)}");
                    sb.AppendLine($"P:{string.Join(",", CurrentParameters.P)}");
                    sb.AppendLine($"Z:{string.Join(",", CurrentParameters.Z)}");
                    sb.AppendLine($"M:{CurrentParameters.M}");
                    sb.AppendLine($"L:{CurrentParameters.L}");
                    sb.AppendLine($"Dimensions:{BoardWidth},{BoardHeight}");

                    for (int y = 0; y < BoardHeight; y++)
                    {
                        for (int x = 0; x < BoardWidth; x++)
                        {
                            var cell = cells[y, x];
                            int p = cell.Plant != null ? 1 : 0;
                            int a = cell.Animal != null ? 1 : 0;
                            int age = cell.Animal != null ? cell.Animal.Age : 0;

                            if (p == 1 || a == 1)
                            {
                                sb.AppendLine($"{x},{y},{p},{a},{age}");
                            }
                        }
                    }

                    System.IO.File.WriteAllText(dlg.FileName, sb.ToString());
                    MessageBox.Show($"Saved simulation state", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error when saving file: \n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning)
            {
                MessageBox.Show("Stop simulation before loading simulation state.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var lines = System.IO.File.ReadAllLines(dlg.FileName);
                    if (lines.Length < 8)
                        throw new FormatException("Wrong file format.");

                    CurrentParameters.R = ParseSetLine(lines[0]);
                    CurrentParameters.W = ParseSetLine(lines[1]);
                    CurrentParameters.B = ParseSetLine(lines[2]);
                    CurrentParameters.P = ParseSetLine(lines[3]);
                    CurrentParameters.Z = ParseSetLine(lines[4]);
                    CurrentParameters.M = int.Parse(lines[5].Split(':')[1]);
                    CurrentParameters.L = int.Parse(lines[6].Split(':')[1]);

                    var dimParts = lines[7].Split(':')[1].Split(',');
                    BoardWidth = int.Parse(dimParts[0]);
                    BoardHeight = int.Parse(dimParts[1]);

                    WidthSlider.Value = BoardWidth;
                    HeightSlider.Value = BoardHeight;

                    RTextBox.Text = string.Join(", ", CurrentParameters.R);
                    WTextBox.Text = string.Join(", ", CurrentParameters.W);
                    BTextBox.Text = string.Join(", ", CurrentParameters.B);
                    PTextBox.Text = string.Join(", ", CurrentParameters.P);
                    ZTextBox.Text = string.Join(", ", CurrentParameters.Z);
                    MTextBox.Text = CurrentParameters.M.ToString();
                    LTextBox.Text = CurrentParameters.L.ToString();

                    cells = new Cell[BoardHeight, BoardWidth];
                    for (int y = 0; y < BoardHeight; y++)
                        for (int x = 0; x < BoardWidth; x++)
                            cells[y, x] = new Cell();

                    for (int i = 8; i < lines.Length; i++)
                    {
                        var parts = lines[i].Split(',');
                        if (parts.Length >= 5)
                        {
                            int x = int.Parse(parts[0]);
                            int y = int.Parse(parts[1]);
                            int p = int.Parse(parts[2]);
                            int a = int.Parse(parts[3]);
                            int age = int.Parse(parts[4]);

                            if (x < BoardWidth && y < BoardHeight)
                            {
                                if (p == 1) cells[y, x].Plant = new Plant();
                                if (a == 1) cells[y, x].Animal = new Animal { Age = age };
                            }
                        }
                    }

                    ResetStatistics();
                    DrawBoard();
                    MessageBox.Show("Loaded simulation state.", "Load", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error when loading the save file: \n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PlantColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlantColorComboBox == null || PlantColorComboBox.SelectedItem == null)
                return;

            if (PlantColorComboBox?.SelectedItem is ComboBoxItem item && item.Tag is string colorName)
            {
                var converter = new BrushConverter();
                _plantBrush = (Brush)(converter.ConvertFromString(colorName) ?? Brushes.Green);
                DrawBoard();
            }
        }

        private void AnimalColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AnimalColorComboBox == null || AnimalColorComboBox.SelectedItem == null)
                return;

            if (AnimalColorComboBox?.SelectedItem is ComboBoxItem item && item.Tag is string colorName)
            {
                var converter = new BrushConverter();
                _animalBrush = (Brush)(converter.ConvertFromString(colorName) ?? Brushes.Red);
                DrawBoard();
            }
        }

        private static HashSet<int> ParseSetLine(string line)
        {
            var parts = line.Split(':');
            if (parts.Length < 2) return new HashSet<int>();
            return ParseSet(parts[1], "File");
        }
    }
}
