using CustomControlExample.Controls.Attributes;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
namespace CustomControlExample.Controls;

public sealed partial class FloatingActionButton : Control
{
    public FloatingActionButton()
    {
        DefaultStyleKey = typeof(FloatingActionButton);
    }

    [DependencyProperty]
    public partial double Length { get; set; }
}
