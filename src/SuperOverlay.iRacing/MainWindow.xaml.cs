using System;
using System.Windows;
using System.Windows.Threading;
using SuperOverlay.Dashboards.Gear;
using SuperOverlay.Dashboards.Speed;
using SuperOverlay.iRacing.Telemetry;
using SuperOverlay.iRacing.Mapping;
using SuperOverlay.LayoutBuilder.Contracts;

namespace SuperOverlay.iRacing
{
    public partial class MainWindow : Window
    {
        private readonly GearDashboardDefinition _gearDef = new();
        private readonly SpeedDashboardDefinition _speedDef = new();

        private readonly MockTelemetryProvider _telemetry = new();
        private readonly IRacingMapper _mapper = new();

        private ILayoutItemPresenter _gearPresenter;
        private ILayoutItemPresenter _speedPresenter;

        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            // создаём dashboards
            _gearPresenter = _gearDef.CreatePresenter();
            _speedPresenter = _speedDef.CreatePresenter();

            // добавляем в UI
            RootGrid.Children.Add((UIElement)_gearPresenter.View);
            RootGrid.Children.Add((UIElement)_speedPresenter.View);

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };

            _timer.Tick += OnTick;
            _timer.Start();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            var (speed, rpm, gear) = _telemetry.Get();
            var state = _mapper.Map(speed, rpm, gear);

            _gearPresenter.Update(state);
            _speedPresenter.Update(state);
        }
    }
}