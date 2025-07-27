using CustomControlExample.Controls.Attributes;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace CustomControlExample.Controls;

public sealed partial class AutoValidateBox : TextBox
{
    public AutoValidateBox()
    {
        DefaultStyleKey = typeof(AutoValidateBox);
    }

    [DependencyProperty<bool>(DefaultValue = true)]
    public partial bool IsValid { get; set; }

    [DependencyProperty]
    public partial Brush InvalidForeground { get; set; }

    [DependencyProperty]
    public partial Brush InvalidBackground { get; set; }

    [DependencyProperty]
    public partial Brush InvalidBorderBrush { get; set; }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();


    }
}
