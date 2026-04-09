using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using SuperOverlay.Dashboards.Items.Gear;
using SuperOverlay.Dashboards.Items.Speed;
using SuperOverlay.Dashboards.Registry;
using SuperOverlay.iRacing.Hosting;
using SuperOverlay.iRacing.Mapping;
using SuperOverlay.iRacing.Telemetry.Mock;
using SuperOverlay.LayoutBuilder.Runtime;

namespace SuperOverlay.iRacing
{
    public partial class MainWindow : Window
    {
        private readonly MockTelemetryProvider _telemetry = new();
        private readonly IRacingMapper _mapper = new();
        private readonly DashboardRegistry _registry = new();

        private LayoutHost _layoutHost = null!;
        private DispatcherTimer _timer = null!;

        public MainWindow()
        {
            InitializeComponent();

            RegisterDashboards();

            _layoutHost = new LayoutHost(RootGrid);

            var layout = DefaultLayoutFactory.Create();

            foreach (var item in layout.Items)
            {
                var placement = layout.Placements.First(x => x.ItemId == item.Id);
                var definition = _registry.Get(item.TypeId);
                var presenter = definition.CreatePresenter();

                var runtimeItem = new RuntimeLayoutItem(item, placement, presenter);

                _layoutHost.AddItem(runtimeItem);
            }

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };

            _timer.Tick += OnTick;
            _timer.Start();
        }

        private void RegisterDashboards()
        {
            _registry.Register(new GearDashboardDefinition());
            _registry.Register(new SpeedDashboardDefinition());
        }

        private void OnTick(object? sender, EventArgs e)
        {
            var (speed, rpm, gear) = _telemetry.Get();
            var state = _mapper.Map(speed, rpm, gear);

            _layoutHost.Update(state);
        }
    }
}