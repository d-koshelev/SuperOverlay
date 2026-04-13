using System.ComponentModel;
using System.Runtime.CompilerServices;

using SuperOverlay.Core.Layouts.Editor;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorWidget : INotifyPropertyChanged
{
    private Guid? _groupId;
    private bool _isSelected;
    private bool _showInRace = true;
    private double _x;
    private double _y;
    private double _width;
    private double _height;
    private int _zIndex;
    private bool _isLocked;
    private bool _isVisibleInCurrentMode = true;
    private string _rawBindingSource = "TelemetryRaw";
    private string _rawBindingFieldPath = "Speed";

    public LayoutEditorWidget()
    {
        TopLeftSlot = CreateSlot(LayoutEditorSlotId.TopLeft, "Add", LayoutEditorTextSizePreset.S, LayoutEditorTextRole.Label);
        TopRightSlot = CreateSlot(LayoutEditorSlotId.TopRight, "Add", LayoutEditorTextSizePreset.S, LayoutEditorTextRole.Label);
        CenterSlot = CreateSlot(LayoutEditorSlotId.Center, "VALUE", LayoutEditorTextSizePreset.M, LayoutEditorTextRole.Primary, isCenter: true);
        BottomLeftSlot = CreateSlot(LayoutEditorSlotId.BottomLeft, "Add", LayoutEditorTextSizePreset.S, LayoutEditorTextRole.Label);
        BottomRightSlot = CreateSlot(LayoutEditorSlotId.BottomRight, "Add", LayoutEditorTextSizePreset.S, LayoutEditorTextRole.Label);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id { get; init; } = Guid.NewGuid();

    public LayoutEditorWidgetSlot TopLeftSlot { get; }
    public LayoutEditorWidgetSlot TopRightSlot { get; }
    public LayoutEditorWidgetSlot CenterSlot { get; }
    public LayoutEditorWidgetSlot BottomLeftSlot { get; }
    public LayoutEditorWidgetSlot BottomRightSlot { get; }

    public double X { get => _x; set => SetField(ref _x, value); }
    public double Y { get => _y; set => SetField(ref _y, value); }
    public double Width { get => _width; set => SetField(ref _width, value); }
    public double Height { get => _height; set => SetField(ref _height, value); }
    public bool IsSelected { get => _isSelected; set => SetField(ref _isSelected, value); }
    public int ZIndex { get => _zIndex; set => SetField(ref _zIndex, value); }
    public bool ShowInRace { get => _showInRace; set => SetField(ref _showInRace, value); }
    public bool IsLocked { get => _isLocked; set => SetField(ref _isLocked, value); }
    public bool IsVisibleInCurrentMode { get => _isVisibleInCurrentMode; set => SetField(ref _isVisibleInCurrentMode, value); }
    public Guid? GroupId
    {
        get => _groupId;
        set
        {
            if (!SetField(ref _groupId, value))
            {
                return;
            }

            RaisePropertyChanged(nameof(IsGrouped));
        }
    }

    public bool IsGrouped => GroupId.HasValue;
    public string RawBindingSource { get => _rawBindingSource; set => SetField(ref _rawBindingSource, string.IsNullOrWhiteSpace(value) ? "TelemetryRaw" : value); }
    public string RawBindingFieldPath { get => _rawBindingFieldPath; set => SetField(ref _rawBindingFieldPath, string.IsNullOrWhiteSpace(value) ? "Speed" : value); }


    public string? TopLeftContent { get => TopLeftSlot.Content; set => TopLeftSlot.Content = value; }
    public string? TopRightContent { get => TopRightSlot.Content; set => TopRightSlot.Content = value; }
    public string? CenterContent { get => CenterSlot.Content; set => CenterSlot.Content = value; }
    public string? BottomLeftContent { get => BottomLeftSlot.Content; set => BottomLeftSlot.Content = value; }
    public string? BottomRightContent { get => BottomRightSlot.Content; set => BottomRightSlot.Content = value; }

    public string TopLeftDisplay => TopLeftSlot.Display;
    public string TopRightDisplay => TopRightSlot.Display;
    public string CenterDisplay => CenterSlot.Display;
    public string BottomLeftDisplay => BottomLeftSlot.Display;
    public string BottomRightDisplay => BottomRightSlot.Display;

    public bool TopLeftHasContent => TopLeftSlot.HasContent;
    public bool TopRightHasContent => TopRightSlot.HasContent;
    public bool CenterHasContent => CenterSlot.HasContent;
    public bool BottomLeftHasContent => BottomLeftSlot.HasContent;
    public bool BottomRightHasContent => BottomRightSlot.HasContent;

    public LayoutEditorTextSizePreset TopLeftTextSizePreset { get => TopLeftSlot.TextSizePreset; set => TopLeftSlot.TextSizePreset = value; }
    public LayoutEditorTextSizePreset TopRightTextSizePreset { get => TopRightSlot.TextSizePreset; set => TopRightSlot.TextSizePreset = value; }
    public LayoutEditorTextSizePreset CenterTextSizePreset { get => CenterSlot.TextSizePreset; set => CenterSlot.TextSizePreset = value; }
    public LayoutEditorTextSizePreset BottomLeftTextSizePreset { get => BottomLeftSlot.TextSizePreset; set => BottomLeftSlot.TextSizePreset = value; }
    public LayoutEditorTextSizePreset BottomRightTextSizePreset { get => BottomRightSlot.TextSizePreset; set => BottomRightSlot.TextSizePreset = value; }

    public LayoutEditorTextRole TopLeftTextRole { get => TopLeftSlot.TextRole; set => TopLeftSlot.TextRole = value; }
    public LayoutEditorTextRole TopRightTextRole { get => TopRightSlot.TextRole; set => TopRightSlot.TextRole = value; }
    public LayoutEditorTextRole CenterTextRole { get => CenterSlot.TextRole; set => CenterSlot.TextRole = value; }
    public LayoutEditorTextRole BottomLeftTextRole { get => BottomLeftSlot.TextRole; set => BottomLeftSlot.TextRole = value; }
    public LayoutEditorTextRole BottomRightTextRole { get => BottomRightSlot.TextRole; set => BottomRightSlot.TextRole = value; }

    public double TopLeftFontSize => TopLeftSlot.FontSize;
    public double TopRightFontSize => TopRightSlot.FontSize;
    public double CenterFontSize => CenterSlot.FontSize;
    public double BottomLeftFontSize => BottomLeftSlot.FontSize;
    public double BottomRightFontSize => BottomRightSlot.FontSize;

    public System.Windows.FontWeight TopLeftFontWeight => TopLeftSlot.FontWeight;
    public System.Windows.FontWeight TopRightFontWeight => TopRightSlot.FontWeight;
    public System.Windows.FontWeight CenterFontWeight => CenterSlot.FontWeight;
    public System.Windows.FontWeight BottomLeftFontWeight => BottomLeftSlot.FontWeight;
    public System.Windows.FontWeight BottomRightFontWeight => BottomRightSlot.FontWeight;

    public System.Windows.Media.Brush TopLeftForegroundBrush => TopLeftSlot.ForegroundBrush;
    public System.Windows.Media.Brush TopRightForegroundBrush => TopRightSlot.ForegroundBrush;
    public System.Windows.Media.Brush CenterForegroundBrush => CenterSlot.ForegroundBrush;
    public System.Windows.Media.Brush BottomLeftForegroundBrush => BottomLeftSlot.ForegroundBrush;
    public System.Windows.Media.Brush BottomRightForegroundBrush => BottomRightSlot.ForegroundBrush;

    public string? GetSlotContent(LayoutEditorSlotId slotId) => GetSlot(slotId).Content;
    public LayoutEditorTextSizePreset GetSlotTextSizePreset(LayoutEditorSlotId slotId) => GetSlot(slotId).TextSizePreset;
    public LayoutEditorTextRole GetSlotTextRole(LayoutEditorSlotId slotId) => GetSlot(slotId).TextRole;
    public void SetSlotTextRole(LayoutEditorSlotId slotId, LayoutEditorTextRole value) => GetSlot(slotId).TextRole = value;
    public void SetSlotTextSizePreset(LayoutEditorSlotId slotId, LayoutEditorTextSizePreset value) => GetSlot(slotId).TextSizePreset = value;
    public void SetSlotContent(LayoutEditorSlotId slotId, string? value) => GetSlot(slotId).Content = value;

    private LayoutEditorWidgetSlot CreateSlot(LayoutEditorSlotId slotId, string placeholder, LayoutEditorTextSizePreset defaultSize, LayoutEditorTextRole defaultRole, bool isCenter = false)
    {
        var slot = new LayoutEditorWidgetSlot(placeholder, defaultSize, defaultRole, isCenter);
        slot.PropertyChanged += (_, __) => RaiseSlotNotifications(slotId);
        return slot;
    }

    private LayoutEditorWidgetSlot GetSlot(LayoutEditorSlotId slotId) => slotId switch
    {
        LayoutEditorSlotId.TopLeft => TopLeftSlot,
        LayoutEditorSlotId.TopRight => TopRightSlot,
        LayoutEditorSlotId.Center => CenterSlot,
        LayoutEditorSlotId.BottomLeft => BottomLeftSlot,
        LayoutEditorSlotId.BottomRight => BottomRightSlot,
        _ => TopLeftSlot,
    };

    private void RaiseSlotNotifications(LayoutEditorSlotId slotId)
    {
        switch (slotId)
        {
            case LayoutEditorSlotId.TopLeft:
                RaisePropertyChanged(nameof(TopLeftContent));
                RaisePropertyChanged(nameof(TopLeftDisplay));
                RaisePropertyChanged(nameof(TopLeftHasContent));
                RaisePropertyChanged(nameof(TopLeftTextSizePreset));
                RaisePropertyChanged(nameof(TopLeftTextRole));
                RaisePropertyChanged(nameof(TopLeftFontSize));
                RaisePropertyChanged(nameof(TopLeftFontWeight));
                RaisePropertyChanged(nameof(TopLeftForegroundBrush));
                break;
            case LayoutEditorSlotId.TopRight:
                RaisePropertyChanged(nameof(TopRightContent));
                RaisePropertyChanged(nameof(TopRightDisplay));
                RaisePropertyChanged(nameof(TopRightHasContent));
                RaisePropertyChanged(nameof(TopRightTextSizePreset));
                RaisePropertyChanged(nameof(TopRightTextRole));
                RaisePropertyChanged(nameof(TopRightFontSize));
                RaisePropertyChanged(nameof(TopRightFontWeight));
                RaisePropertyChanged(nameof(TopRightForegroundBrush));
                break;
            case LayoutEditorSlotId.Center:
                RaisePropertyChanged(nameof(CenterContent));
                RaisePropertyChanged(nameof(CenterDisplay));
                RaisePropertyChanged(nameof(CenterHasContent));
                RaisePropertyChanged(nameof(CenterTextSizePreset));
                RaisePropertyChanged(nameof(CenterTextRole));
                RaisePropertyChanged(nameof(CenterFontSize));
                RaisePropertyChanged(nameof(CenterFontWeight));
                RaisePropertyChanged(nameof(CenterForegroundBrush));
                break;
            case LayoutEditorSlotId.BottomLeft:
                RaisePropertyChanged(nameof(BottomLeftContent));
                RaisePropertyChanged(nameof(BottomLeftDisplay));
                RaisePropertyChanged(nameof(BottomLeftHasContent));
                RaisePropertyChanged(nameof(BottomLeftTextSizePreset));
                RaisePropertyChanged(nameof(BottomLeftTextRole));
                RaisePropertyChanged(nameof(BottomLeftFontSize));
                RaisePropertyChanged(nameof(BottomLeftFontWeight));
                RaisePropertyChanged(nameof(BottomLeftForegroundBrush));
                break;
            case LayoutEditorSlotId.BottomRight:
                RaisePropertyChanged(nameof(BottomRightContent));
                RaisePropertyChanged(nameof(BottomRightDisplay));
                RaisePropertyChanged(nameof(BottomRightHasContent));
                RaisePropertyChanged(nameof(BottomRightTextSizePreset));
                RaisePropertyChanged(nameof(BottomRightTextRole));
                RaisePropertyChanged(nameof(BottomRightFontSize));
                RaisePropertyChanged(nameof(BottomRightFontWeight));
                RaisePropertyChanged(nameof(BottomRightForegroundBrush));
                break;
        }
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        RaisePropertyChanged(propertyName);
        return true;
    }

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
