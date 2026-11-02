using ScottPlot.WinForms;
using System.Globalization;


namespace vieta_method_and_discriminant
{
  public partial class Form1 : Form
  {

    // Поля для коэффициентов
    private TextBox _textBoxA;
    private TextBox _textBoxB;
    private TextBox _textBoxC;

    // Выбор метода
    private RadioButton _radioDisc;
    private RadioButton _radioVieta;

    private FormsPlot _formsPlot;
    private TrackBar _yZoomTrackBar;

    private Label _labelResult;
    private Label _labelWarning;

    // Поля для результатов
    private Label _labelX1Name;
    private Label _valueX1;
    private Label _labelX2Name;
    private Label _valueX2;
    private Label _labelDetailsName;
    private Label _valueDetails;

    private TableLayoutPanel _resultTable;
    private GroupBox _groupParams;
    private GroupBox _groupResult;

    private MenuStrip _menuStrip;

    private System.Windows.Forms.Timer _autoCalcTimer;

    private double _a, _b, _c;
    private Solver _solverResult;

    public Form1()
    {
      BuildUserInterface();

      _autoCalcTimer = new System.Windows.Forms.Timer { Interval = 300 };
      _autoCalcTimer.Tick += (sender, args) =>
      {
        _autoCalcTimer.Stop();
        TryAutoCalculate();
      };

      _textBoxA.TextChanged += (sender, args) => ScheduleAutoCalculate();
      _textBoxB.TextChanged += (sender, args) => ScheduleAutoCalculate();
      _textBoxC.TextChanged += (sender, args) => ScheduleAutoCalculate();

      _radioDisc.CheckedChanged += (sender, args) => ScheduleAutoCalculate();
      _radioVieta.CheckedChanged += (sender, args) => ScheduleAutoCalculate();
    }

    // =========
    // ИНТЕРФЕЙС 
    // =========

    private void BuildUserInterface()
    {
      Text = "Решение квадратных уравнений";
      Width = 1150;
      Height = 700;
      MinimumSize = new Size(950, 600);
      StartPosition = FormStartPosition.CenterScreen;
      Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
      BackColor = Color.FromArgb(245, 245, 245);

      _menuStrip = new MenuStrip();
      var calculateItem = new ToolStripMenuItem("Рассчитать");
      var clearItem = new ToolStripMenuItem("Очистить");
      calculateItem.Click += (s, e) => Calculate();
      clearItem.Click += (s, e) => ClearAll();
      _menuStrip.Items.Add(calculateItem);
      _menuStrip.Items.Add(clearItem);
      MainMenuStrip = _menuStrip;
      Controls.Add(_menuStrip);

      var rootLayout = new TableLayoutPanel
      {
        Dock = DockStyle.Fill,
        ColumnCount = 2,
        RowCount = 1,
        Padding = new Padding(15)
      };
      rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400));
      rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
      Controls.Add(rootLayout);

      var leftContainer = new TableLayoutPanel
      {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 2,
        Margin = new Padding(10)
      };
      leftContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 235));
      leftContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

      _groupParams = new GroupBox
      {
        Text = " Уравнение ax² + bx + c = 0 ",
        Dock = DockStyle.Fill,
        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
        ForeColor = Color.FromArgb(60, 60, 60),
        Padding = new Padding(10, 15, 10, 10)
      };

      var paramsLayout = new TableLayoutPanel
      {
        Dock = DockStyle.Fill,
        ColumnCount = 2,
        RowCount = 4
      };
      paramsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 65));
      paramsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

      for (int i = 0; i < 4; i++) {
        paramsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
      }

      // a
      paramsLayout.Controls.Add(MakeLabel("a ="), 0, 0);
      _textBoxA = MakeTextBox();
      _textBoxA.Text = "1";
      paramsLayout.Controls.Add(_textBoxA, 1, 0);

      // b
      paramsLayout.Controls.Add(MakeLabel("b ="), 0, 1);
      _textBoxB = MakeTextBox();
      _textBoxB.Text = "2";
      paramsLayout.Controls.Add(_textBoxB, 1, 1);

      // c
      paramsLayout.Controls.Add(MakeLabel("c ="), 0, 2);
      _textBoxC = MakeTextBox();
      _textBoxC.Text = "-6";
      paramsLayout.Controls.Add(_textBoxC, 1, 2);

      // Метод
      paramsLayout.Controls.Add(MakeLabel("Метод:"), 0, 3);

      var modePanel = new FlowLayoutPanel
      {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.LeftToRight,
        WrapContents = false,
        Padding = new Padding(0, 6, 0, 0)
      };

      _radioDisc = new RadioButton { Text = "Дискриминант", Checked = true, AutoSize = true, Margin = new Padding(0, 17, 15, 0) };
      _radioVieta = new RadioButton { Text = "Теорема Виета", AutoSize = true, Margin = new Padding(0, 17, 15, 0) };

      modePanel.Controls.Add(_radioDisc);
      modePanel.Controls.Add(_radioVieta);
      paramsLayout.Controls.Add(modePanel, 1, 3);

      _groupParams.Controls.Add(paramsLayout);
      leftContainer.Controls.Add(_groupParams, 0, 0);

      // --- ГРУППА: РЕЗУЛЬТАТ ---
      _groupResult = new GroupBox
      {
        Text = " Результат ",
        Dock = DockStyle.Fill,
        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
        ForeColor = Color.FromArgb(60, 60, 60),
        Padding = new Padding(10, 15, 10, 10),
        Margin = new Padding(0, 15, 0, 0)
      };

      _labelResult = new Label
      {
        Text = "Ожидание данных",
        Dock = DockStyle.Fill,
        AutoSize = false,
        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
        ForeColor = Color.Gray,
        TextAlign = ContentAlignment.MiddleLeft
      };

      _resultTable = CreateResultTable();
      _resultTable.Visible = false;

      _labelWarning = new Label
      {
        Text = "",
        Dock = DockStyle.Fill,
        AutoSize = false,
        Font = new Font("Segoe UI", 9F, FontStyle.Italic),
        ForeColor = Color.DarkOrange,
        TextAlign = ContentAlignment.TopLeft,
        Padding = new Padding(0, 10, 0, 0)
      };

      var resLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
      resLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
      resLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
      resLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

      resLayout.Controls.Add(_labelResult, 0, 0);
      resLayout.Controls.Add(_resultTable, 0, 1);
      resLayout.Controls.Add(_labelWarning, 0, 2);

      _groupResult.Controls.Add(resLayout);
      leftContainer.Controls.Add(_groupResult, 0, 1);

      rootLayout.Controls.Add(leftContainer, 0, 0);

      // --- ПРАВАЯ ПАНЕЛЬ (ГРАФИК) ---
      var plotContainer = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Margin = new Padding(15, 0, 0, 0) };
      plotContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
      plotContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

      _formsPlot = new FormsPlot { Dock = DockStyle.Fill };
      plotContainer.Controls.Add(_formsPlot, 0, 0);

      // Ползунок масштаба Y
      var zoomPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 5, 0, 0) };
      zoomPanel.Controls.Add(new Label { Text = "Масштаб Y:", AutoSize = true, Margin = new Padding(0, 5, 10, 0), ForeColor = Color.DimGray });

      _yZoomTrackBar = new TrackBar { Minimum = 10, Maximum = 300, Value = 100, TickFrequency = 20, Width = 200, AutoSize = false, Height = 30 };
      _yZoomTrackBar.Scroll += (s, e) => UpdatePlot(false);
      zoomPanel.Controls.Add(_yZoomTrackBar);

      var lblZoom = new Label { Text = "1.0x", AutoSize = true, Margin = new Padding(10, 5, 0, 0), ForeColor = Color.DimGray };
      _yZoomTrackBar.ValueChanged += (s, e) => {
        lblZoom.Text = (_yZoomTrackBar.Value / 100.0).ToString("0.0") + "x";
      };
      zoomPanel.Controls.Add(lblZoom);

      plotContainer.Controls.Add(zoomPanel, 0, 1);
      rootLayout.Controls.Add(plotContainer, 1, 0);
    }

    private TableLayoutPanel CreateResultTable()
    {
      var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3 };
      table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
      table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

      for (int i = 0; i < 3; i++) {
      table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
    }

      _labelX1Name = CreateResultNameLabel("Корень x1:");
      _valueX1 = CreateResultValueLabel();
      _labelX2Name = CreateResultNameLabel("Корень x2:");
      _valueX2 = CreateResultValueLabel();
      _labelDetailsName = CreateResultNameLabel("Вычисления:");
      _valueDetails = CreateResultValueLabel();

      table.Controls.Add(_labelX1Name, 0, 0); table.Controls.Add(_valueX1, 1, 0);
      table.Controls.Add(_labelX2Name, 0, 1); table.Controls.Add(_valueX2, 1, 1);
      table.Controls.Add(_labelDetailsName, 0, 2); table.Controls.Add(_valueDetails, 1, 2);

      return table;
    }

    private Label CreateResultNameLabel(string text)
    {
      return new Label { Text = text, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F), TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(80, 80, 80) };
    }

    private Label CreateResultValueLabel()
    {
      return new Label { Text = "-", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
    }

    private Label MakeLabel(string caption)
    {
      return new Label { Text = caption, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9.5F), Padding = new Padding(0, 0, 8, 0) };
    }

    private TextBox MakeTextBox()
    {
      return new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F), Margin = new Padding(0, 4, 0, 4) };
    }

    // ============================================================
    // ЛОГИКА
    // ============================================================

    private void ScheduleAutoCalculate()
    {
      _autoCalcTimer.Stop();
      _autoCalcTimer.Start();
    }

    private void TryAutoCalculate()
    {
      if (string.IsNullOrWhiteSpace(_textBoxA.Text) || string.IsNullOrWhiteSpace(_textBoxB.Text) || string.IsNullOrWhiteSpace(_textBoxC.Text)) {
        ClearResult();
        return;
      }
      Calculate();
    }

    private void Calculate()
    {
      _textBoxA.BackColor = SystemColors.Window;
      _textBoxB.BackColor = SystemColors.Window;
      _textBoxC.BackColor = SystemColors.Window;

      if (!TryParseDouble(_textBoxA.Text, out _a)) { _textBoxA.BackColor = Color.MistyRose; ShowError("некорректное a"); return; }
      if (!TryParseDouble(_textBoxB.Text, out _b)) { _textBoxB.BackColor = Color.MistyRose; ShowError("некорректное b"); return; }
      if (!TryParseDouble(_textBoxC.Text, out _c)) { _textBoxC.BackColor = Color.MistyRose; ShowError("некорректное c"); return; }

      _solverResult = QuadraticSolver.Solve(_a, _b, _c, _radioVieta.Checked);

      ShowResult();
      UpdatePlot(true);
    }

    private void ShowResult()
    {
      if (!_solverResult.HasRoots) {
        _labelResult.Text = "Корней нет";
        _labelResult.ForeColor = Color.FromArgb(220, 50, 50);
        _valueDetails.Text = _solverResult.Details;

        _labelX1Name.Visible = false; _valueX1.Visible = false;
        _labelX2Name.Visible = false; _valueX2.Visible = false;
        _labelDetailsName.Visible = true; _valueDetails.Visible = true;
        _resultTable.Visible = true;
        return;
      }

      _labelResult.Text = "Успешно решено";
      _labelResult.ForeColor = Color.FromArgb(0, 120, 215);

      _valueX1.Text = FormatNum(_solverResult.X1);
      _valueX2.Text = FormatNum(_solverResult.X2);
      _valueDetails.Text = _solverResult.Details;

      bool isSingleRoot = Math.Abs(_solverResult.X1 - _solverResult.X2) < 1e-9;

      _labelX1Name.Visible = true; _valueX1.Visible = true;
      _labelX2Name.Visible = !isSingleRoot; _valueX2.Visible = !isSingleRoot;
      _labelDetailsName.Visible = true; _valueDetails.Visible = true;

      _resultTable.Visible = true;
    }

    private void ShowError(string msg)
    {
      _labelResult.Text = "Ошибка: " + msg;
      _labelResult.ForeColor = Color.FromArgb(220, 50, 50);
      _resultTable.Visible = false;
      _solverResult = null;
      UpdatePlot(false);
    }

    private void ClearResult()
    {
      _labelResult.Text = "Ожидание данных";
      _labelResult.ForeColor = Color.Gray;
      _resultTable.Visible = false;
      _solverResult = null;
      UpdatePlot(false);
    }

    private void ClearAll()
    {
      _textBoxA.Text = ""; _textBoxB.Text = ""; _textBoxC.Text = "";
      ClearResult();
    }

    private static string FormatNum(double val)
    {
      string s = Math.Round(val, 4).ToString(CultureInfo.InvariantCulture);
      return s;
    }

    private static bool TryParseDouble(string text, out double value)
    {
      text = (text ?? "").Trim().Replace(',', '.');
      return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    // ======
    // ГРАФИК
    // ======

    private void UpdatePlot(bool recalcBaseLimits = true)
    {
      _formsPlot.Plot.Clear();

      if (Math.Abs(_a) < 1e-12) {
      return; // Если это не парабола
    }

      double xCenter = -_b / (2 * _a);
      if (_solverResult != null && _solverResult.HasRoots) {
        xCenter = (_solverResult.X1 + _solverResult.X2) / 2;
      }

      double xMin = xCenter - 10;
      double xMax = xCenter + 10;

      int samples = 400;
      double[] xs = new double[samples];
      double[] ys = new double[samples];

      for (int i = 0; i < samples; i++) {
        double x = xMin + (xMax - xMin) * i / (samples - 1);
        xs[i] = x;
        ys[i] = _a * x * x + _b * x + _c;
      }

      var scatter = _formsPlot.Plot.Add.Scatter(xs, ys);
      scatter.Color = ScottPlot.Color.FromSDColor(Color.FromArgb(0, 120, 215));
      scatter.LineWidth = 2;
      scatter.MarkerSize = 0;

      var zeroLine = _formsPlot.Plot.Add.HorizontalLine(0);
      zeroLine.Color = ScottPlot.Color.FromSDColor(Color.Gray).WithAlpha(0.5);

      if (_solverResult != null && _solverResult.HasRoots) {
        _formsPlot.Plot.Add.Marker(_solverResult.X1, 0).Color = ScottPlot.Color.FromSDColor(Color.Red);
        if (Math.Abs(_solverResult.X1 - _solverResult.X2) > 1e-5) {
          _formsPlot.Plot.Add.Marker(_solverResult.X2, 0).Color = ScottPlot.Color.FromSDColor(Color.Red);
        }
      }

      _formsPlot.Plot.Title($"y = {_a}x² + {_b}x + {_c}");
      _formsPlot.Plot.Axes.SetLimitsX(xMin, xMax);

      double zoomFactor = _yZoomTrackBar.Value / 100.0;
      double halfRange = Math.Max(Math.Abs(_a) * 10, 10);
      halfRange /= zoomFactor;

      _formsPlot.Plot.Axes.SetLimitsY(-halfRange, halfRange);

      _formsPlot.Refresh();
    }
  }
}