using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SchoolBookingApp.MVVM.View.CustomControls;

/// <summary>
/// Interaction logic for DataBox.xaml
/// </summary>
public partial class DataBox : UserControl
{
    public DataBox()
    {
        InitializeComponent();

        this.MouseEnter += (s, e) =>
            VisualStateManager.GoToState(this, "MouseOver", true);
        this.MouseLeave += (s, e) => 
            VisualStateManager.GoToState(this, "Normal", true);
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(DataBox), new PropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty NumberProperty =
        DependencyProperty.Register(nameof(Number), typeof(string), typeof(DataBox), new PropertyMetadata(string.Empty));

    public string Number
    {
        get => (string)GetValue(NumberProperty);
        set => SetValue(NumberProperty, value);
    }

    public static readonly DependencyProperty StartColorProperty =
        DependencyProperty.Register(nameof(StartColor), typeof(Color), typeof(DataBox), new PropertyMetadata(Colors.Transparent));

    public Color StartColor
    {
        get => (Color)GetValue(StartColorProperty);
        set => SetValue(StartColorProperty, value);
    }

    public static readonly DependencyProperty StopColorProperty =
        DependencyProperty.Register(nameof(StopColor), typeof(Color), typeof(DataBox), new PropertyMetadata(Colors.Transparent));

    public Color StopColor
    {
        get => (Color)GetValue(StopColorProperty);
        set => SetValue(StopColorProperty, value);
    }

    public static readonly new DependencyProperty ForegroundProperty = 
        DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(DataBox), new PropertyMetadata(null));

    public new Brush Foreground
    {
        get => (Brush)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }
}
