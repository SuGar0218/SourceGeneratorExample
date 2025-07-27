using CustomControlExample.Demo.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CustomControlExample.Demo;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Frame_Loaded(object sender, RoutedEventArgs e)
    {
        Frame frame = (Frame) sender;
        frame.Navigate(typeof(MainPage));
    }
}
