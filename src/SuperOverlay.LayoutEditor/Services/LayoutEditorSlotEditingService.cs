using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SuperOverlay.LayoutEditor;

public sealed class LayoutEditorSlotEditingService
{
    private readonly Window _owner;

    public LayoutEditorSlotEditingService(Window owner)
    {
        _owner = owner;
    }

    public ContextMenu CreateSlotMenu(FrameworkElement element, LayoutEditorWidget widget, LayoutEditorSlotId slotId, Action<LayoutEditorWidget, LayoutEditorSlotId> assignText, Action<LayoutEditorWidget, LayoutEditorSlotId> assignMetric, Action onCleared)
    {
        var menu = new ContextMenu
        {
            PlacementTarget = element,
            Placement = PlacementMode.Bottom,
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#07101C")),
            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2F5FA")),
            BorderThickness = new Thickness(1),
        };

        var textItem = new MenuItem { Header = "Text" };
        textItem.Click += (_, _) => assignText(widget, slotId);
        var metricItem = new MenuItem { Header = "Metric" };
        metricItem.Click += (_, _) => assignMetric(widget, slotId);
        var clearItem = new MenuItem { Header = "Clear", IsEnabled = !string.IsNullOrWhiteSpace(widget.GetSlotContent(slotId)) };
        clearItem.Click += (_, _) =>
        {
            widget.SetSlotContent(slotId, null);
            onCleared();
        };

        menu.Items.Add(textItem);
        menu.Items.Add(metricItem);
        menu.Items.Add(new Separator());
        menu.Items.Add(clearItem);
        return menu;
    }

    public void AssignTextToSlot(LayoutEditorWidget widget, LayoutEditorSlotId slotId, Action onChanged)
    {
        var prompt = new TextValueWindow(widget.GetSlotContent(slotId)) { Owner = _owner };
        if (prompt.ShowDialog() != true)
        {
            return;
        }

        widget.SetSlotContent(slotId, string.IsNullOrWhiteSpace(prompt.ValueText) ? null : prompt.ValueText);
        onChanged();
    }

    public void AssignMetricToSlot(LayoutEditorWidget widget, LayoutEditorSlotId slotId, Action onChanged)
    {
        var picker = new MetricPickerWindow(LayoutEditorMetricsCatalog.AvailableMetrics) { Owner = _owner };
        if (picker.ShowDialog() != true || string.IsNullOrWhiteSpace(picker.SelectedMetric))
        {
            return;
        }

        widget.SetSlotContent(slotId, picker.SelectedMetric);
        onChanged();
    }

    public void ApplyTextSizeChange(LayoutEditorWidget widget, ComboBox comboBox, ComboBox topLeft, ComboBox topRight, ComboBox center, ComboBox bottomLeft, ComboBox bottomRight)
    {
        if (comboBox.SelectedItem is not ComboBoxItem item || !Enum.TryParse<LayoutEditorTextSizePreset>(item.Content?.ToString(), out var preset))
        {
            return;
        }

        if (ReferenceEquals(comboBox, topLeft))
        {
            widget.TopLeftTextSizePreset = preset;
        }
        else if (ReferenceEquals(comboBox, topRight))
        {
            widget.TopRightTextSizePreset = preset;
        }
        else if (ReferenceEquals(comboBox, center))
        {
            widget.CenterTextSizePreset = preset;
        }
        else if (ReferenceEquals(comboBox, bottomLeft))
        {
            widget.BottomLeftTextSizePreset = preset;
        }
        else if (ReferenceEquals(comboBox, bottomRight))
        {
            widget.BottomRightTextSizePreset = preset;
        }
    }

    public void ApplyTextRoleChange(LayoutEditorWidget widget, ComboBox comboBox, ComboBox topLeft, ComboBox topRight, ComboBox center, ComboBox bottomLeft, ComboBox bottomRight)
    {
        if (comboBox.SelectedItem is not ComboBoxItem item || !Enum.TryParse<LayoutEditorTextRole>(item.Content?.ToString(), out var role))
        {
            return;
        }

        if (ReferenceEquals(comboBox, topLeft))
        {
            widget.TopLeftTextRole = role;
        }
        else if (ReferenceEquals(comboBox, topRight))
        {
            widget.TopRightTextRole = role;
        }
        else if (ReferenceEquals(comboBox, center))
        {
            widget.CenterTextRole = role;
        }
        else if (ReferenceEquals(comboBox, bottomLeft))
        {
            widget.BottomLeftTextRole = role;
        }
        else if (ReferenceEquals(comboBox, bottomRight))
        {
            widget.BottomRightTextRole = role;
        }
    }

    public static void ApplyPresetSelectionToUi(ComboBox comboBox, LayoutEditorTextSizePreset preset)
    {
        foreach (var candidate in comboBox.Items.OfType<ComboBoxItem>())
        {
            if (string.Equals(candidate.Content?.ToString(), preset.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedItem = candidate;
                return;
            }
        }
    }

    public static void ApplyRoleSelectionToUi(ComboBox comboBox, LayoutEditorTextRole role)
    {
        foreach (var candidate in comboBox.Items.OfType<ComboBoxItem>())
        {
            if (string.Equals(candidate.Content?.ToString(), role.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedItem = candidate;
                return;
            }
        }
    }
}
