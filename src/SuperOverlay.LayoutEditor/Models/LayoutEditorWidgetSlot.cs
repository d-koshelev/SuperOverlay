using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

using SuperOverlay.Core.Layouts.Editor;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorWidgetSlot : INotifyPropertyChanged
{
    private readonly string _emptyPlaceholder;
    private string? _content;
    private LayoutEditorTextSizePreset _textSizePreset;
    private LayoutEditorTextRole _textRole;
    private readonly bool _isCenter;

    public LayoutEditorWidgetSlot(string emptyPlaceholder, LayoutEditorTextSizePreset defaultSize, LayoutEditorTextRole defaultRole, bool isCenter = false)
    {
        _emptyPlaceholder = emptyPlaceholder;
        _textSizePreset = defaultSize;
        _textRole = defaultRole;
        _isCenter = isCenter;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string? Content
    {
        get => _content;
        set => SetField(ref _content, value, nameof(Content), nameof(Display), nameof(HasContent));
    }

    public LayoutEditorTextSizePreset TextSizePreset
    {
        get => _textSizePreset;
        set => SetField(ref _textSizePreset, value, nameof(TextSizePreset), nameof(FontSize));
    }

    public LayoutEditorTextRole TextRole
    {
        get => _textRole;
        set => SetField(ref _textRole, value, nameof(TextRole), nameof(FontWeight), nameof(ForegroundBrush));
    }

    public string Display => string.IsNullOrWhiteSpace(Content) ? _emptyPlaceholder : Content!;
    public bool HasContent => !string.IsNullOrWhiteSpace(Content);
    public double FontSize => TextSizePreset.ToFontSize(_isCenter);
    public FontWeight FontWeight => TextRole.ToFontWeight();
    public Brush ForegroundBrush => TextRole.ToForegroundBrush();

    public LayoutEditorWidgetSlot Clone()
    {
        return new LayoutEditorWidgetSlot(_emptyPlaceholder, TextSizePreset, TextRole, _isCenter)
        {
            Content = Content,
        };
    }

    private void SetField<T>(ref T field, T value, string propertyName, params string[] derivedPropertyNames)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        foreach (var derivedPropertyName in derivedPropertyNames)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(derivedPropertyName));
        }
    }
}
