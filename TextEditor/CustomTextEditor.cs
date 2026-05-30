using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MajdataEdit.TextEditor;
public partial class CustomTextEditor : Canvas
{
    private TextBox _internalTextBox;
    private Canvas _canvas;
    private bool _isUpdating;

    #region DependencyProperties

    public static readonly DependencyProperty HighlightColorProperty =
        DependencyProperty.Register("HighlightColor", typeof(Brush), typeof(CustomTextEditor),
            new FrameworkPropertyMetadata(Brushes.Red, FrameworkPropertyMetadataOptions.None));

    public static readonly DependencyProperty ShouldHighlightProperty =
        DependencyProperty.Register("ShouldHighlight", typeof(Func<string, bool>), typeof(CustomTextEditor),
            new FrameworkPropertyMetadata(new Func<string, bool>(s => false), FrameworkPropertyMetadataOptions.None));

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string), typeof(CustomTextEditor),
            new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextPropertyChanged));

    public static readonly DependencyProperty CaretIndexProperty =
        DependencyProperty.Register("CaretIndex", typeof(int), typeof(CustomTextEditor),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCaretIndexPropertyChanged));

    public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register("Foreground", typeof(Brush), typeof(CustomTextEditor),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.None,
                (d, e) => ((CustomTextEditor)d)._internalTextBox.Foreground = (Brush)e.NewValue));

    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register("FontSize", typeof(double), typeof(CustomTextEditor),
            new FrameworkPropertyMetadata(12d, FrameworkPropertyMetadataOptions.None,
                (d, e) => ((CustomTextEditor)d)._internalTextBox.FontSize = (double)e.NewValue));

    public static readonly DependencyProperty PaddingProperty =
        DependencyProperty.Register("Padding", typeof(Thickness), typeof(CustomTextEditor),
            new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.None,
                (d, e) => ((CustomTextEditor)d)._internalTextBox.Padding = (Thickness)e.NewValue));

    public static readonly DependencyProperty FontFamilyProperty =
        DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(CustomTextEditor),
            new FrameworkPropertyMetadata(default(FontFamily), FrameworkPropertyMetadataOptions.None,
                (d, e) => ((CustomTextEditor)d)._internalTextBox.FontFamily = (FontFamily)e.NewValue));

    public static readonly DependencyProperty BorderThicknessProperty =
    DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(CustomTextEditor),
            new FrameworkPropertyMetadata(new Thickness(1), FrameworkPropertyMetadataOptions.None,
                (d, e) => ((CustomTextEditor)d)._internalTextBox.BorderThickness = (Thickness)e.NewValue));

    public static readonly DependencyProperty AcceptsReturnProperty =
    DependencyProperty.Register("AcceptsReturn", typeof(bool), typeof(CustomTextEditor),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None,
            (d, e) => ((CustomTextEditor)d)._internalTextBox.AcceptsReturn = (bool)e.NewValue));

    public static readonly DependencyProperty AcceptsTabProperty =
    DependencyProperty.Register("AcceptsTab", typeof(bool), typeof(CustomTextEditor),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None,
            (d, e) => ((CustomTextEditor)d)._internalTextBox.AcceptsTab = (bool)e.NewValue));

    public static readonly DependencyProperty TextWrappingProperty =
    DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(CustomTextEditor),
        new FrameworkPropertyMetadata(TextWrapping.NoWrap, FrameworkPropertyMetadataOptions.None,
            (d, e) => ((CustomTextEditor)d)._internalTextBox.TextWrapping = (TextWrapping)e.NewValue));

    public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
    DependencyProperty.Register("VerticalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(CustomTextEditor),
        new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden, FrameworkPropertyMetadataOptions.None,
            (d, e) => ((CustomTextEditor)d)._internalTextBox.VerticalScrollBarVisibility = (ScrollBarVisibility)e.NewValue));

    public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty =
    DependencyProperty.Register("HorizontalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(CustomTextEditor),
        new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden, FrameworkPropertyMetadataOptions.None,
            (d, e) => ((CustomTextEditor)d)._internalTextBox.HorizontalScrollBarVisibility = (ScrollBarVisibility)e.NewValue));

    public static readonly DependencyProperty AutoWordSelectionProperty =
    DependencyProperty.Register("AutoWordSelection", typeof(bool), typeof(CustomTextEditor),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None,
            (d, e) => ((CustomTextEditor)d)._internalTextBox.AutoWordSelection = (bool)e.NewValue));

    public static readonly DependencyProperty TextChangedProperty =
    DependencyProperty.Register("TextChanged", typeof(TextChangedEventHandler), typeof(CustomTextEditor),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None,
            (d, e) => ((CustomTextEditor)d)._internalTextBox.TextChanged += (TextChangedEventHandler)e.NewValue));

    public static readonly DependencyProperty SelectionChangedProperty =
    DependencyProperty.Register("SelectionChanged", typeof(RoutedEventHandler), typeof(CustomTextEditor),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None,
            (d, e) => ((CustomTextEditor)d)._internalTextBox.SelectionChanged += (RoutedEventHandler)e.NewValue));

    public static readonly DependencyProperty ResourcesProperty =
    DependencyProperty.Register("Resources", typeof(ResourceDictionary), typeof(CustomTextEditor),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None,
            (d, e) => ((CustomTextEditor)d)._internalTextBox.Resources = (ResourceDictionary)e.NewValue));

    #endregion

    public CustomTextEditor()
    {
        InitializeInternalTextBox();
        Focusable = true;
    }

    #region Properties

    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    public string SelectedText
    {
        get { return _internalTextBox.SelectedText; }
        set { _internalTextBox.SelectedText = value; }
    }

    public int SelectionStart
    {
        get { return _internalTextBox.SelectionStart; }
    }

    public double VerticalOffset
    {
        get { return _internalTextBox.VerticalOffset; }
    }

    public bool IsUndoEnabled
    {
        get { return _internalTextBox.IsUndoEnabled; }
        set { _internalTextBox.IsUndoEnabled = value; }
    }

    public Brush HighlightColor
    {
        get { return (Brush)GetValue(HighlightColorProperty); }
        set { SetValue(HighlightColorProperty, value); }
    }

    public Func<string, bool> ShouldHighlight
    {
        get { return (Func<string, bool>)GetValue(ShouldHighlightProperty); }
        set { SetValue(ShouldHighlightProperty, value); }
    }

    public Brush Foreground
    {
        get { return (Brush)GetValue(ForegroundProperty); }
        set { SetValue(ForegroundProperty, value); }
    }

    public double FontSize
    {
        get { return (double)GetValue(FontSizeProperty); }
        set { SetValue(FontSizeProperty, value); }
    }

    public FontFamily FontFamily
    {
        get { return (FontFamily)GetValue(FontFamilyProperty); }
        set { SetValue(FontFamilyProperty, value); }
    }

    public Thickness Padding
    {
        get { return (Thickness)GetValue(PaddingProperty); }
        set { SetValue(PaddingProperty, value); }
    }

    public Thickness BorderThickness
    {
        get { return (Thickness)GetValue(BorderThicknessProperty); }
        set { SetValue(BorderThicknessProperty, value); }
    }

    public int CaretIndex
    {
        get { return (int)GetValue(CaretIndexProperty); }
        set { SetValue(CaretIndexProperty, value); }
    }

    public bool AcceptsReturn
    {
        get { return (bool)GetValue(AcceptsReturnProperty); }
        set { SetValue(AcceptsReturnProperty, value); }
    }

    public bool AcceptsTab
    {
        get { return (bool)GetValue(AcceptsTabProperty); }
        set { SetValue(AcceptsTabProperty, value); }
    }

    public TextWrapping TextWrapping
    {
        get { return (TextWrapping)GetValue(TextWrappingProperty); }
        set { SetValue(TextWrappingProperty, value); }
    }

    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
        set { SetValue(VerticalScrollBarVisibilityProperty, value); }
    }
    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
        set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
    }
    public bool AutoWordSelection
    {
        get { return (bool)GetValue(AutoWordSelectionProperty); }
        set { SetValue(AutoWordSelectionProperty, value); }
    }
    public TextChangedEventHandler TextChanged
    {
        get { return (TextChangedEventHandler)GetValue(TextChangedProperty); }
        set { SetValue(TextChangedProperty, value); }
    }
    public RoutedEventHandler SelectionChanged
    {
        get { return (RoutedEventHandler)GetValue(SelectionChangedProperty); }
        set { SetValue(SelectionChangedProperty, value); }
    }

    protected override void OnInitialized(EventArgs e)
    {
        _internalTextBox.TextChanged += OnInternalTextChanged;
        _internalTextBox.SelectionChanged += OnInternalSelectionChanged;
        _internalTextBox.AddHandler(ScrollViewer.ScrollChangedEvent, (ScrollChangedEventHandler)OnScrollChanged);
    }

    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);
        _internalTextBox.Focus();
        Keyboard.Focus(_internalTextBox);
    }

    private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var highlightedTextBox = (CustomTextEditor)d;
        highlightedTextBox._internalTextBox.Text = highlightedTextBox.Text;
    }

    private static void OnCaretIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (((CustomTextEditor)d)._internalTextBox.CaretIndex != (int)e.NewValue)
        {
            ((CustomTextEditor)d)._internalTextBox.CaretIndex = (int)e.NewValue;
        }
    }

    #endregion

    #region Passing down to internal textbox
    public void Select(int start, int length)
    {
        _internalTextBox.Select(start, length);
    }

    public void Clear()
    {
        _internalTextBox.Clear();
    }

    public Rect GetRectFromCharacterIndex(int charIndex)
    {
        return _internalTextBox.GetRectFromCharacterIndex(charIndex);
    }

    public Rect GetRectFromCharacterIndex(int charIndex, bool trailingEdge)
    {
        return _internalTextBox.GetRectFromCharacterIndex(charIndex, trailingEdge);
    }

    public void ScrollToVerticalOffset(double offset)
    {
        _internalTextBox.ScrollToVerticalOffset(offset);
    }
    #endregion

    private void InitializeInternalTextBox()
    {
        _internalTextBox = new TextBox { Background = new SolidColorBrush() {
            Color = new Color() { R = 0xff, G = 0x05, B = 0x05, A = 0x05},
            Opacity = 0.3
        } };

        var widthBinding = new Binding("ActualWidth") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(CustomTextEditor), 1) };
        BindingOperations.SetBinding(_internalTextBox, WidthProperty, widthBinding);
        var heightBinding = new Binding("ActualHeight") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(CustomTextEditor), 1) };
        BindingOperations.SetBinding(_internalTextBox, HeightProperty, heightBinding);

        Children.Add(_internalTextBox);

        SetTop(_internalTextBox, 0);
        SetLeft(_internalTextBox, 0);
        SetZIndex(_internalTextBox, 255);
    }

    private void OnInternalTextChanged(object sender, TextChangedEventArgs e)
    {
        var newText = ((TextBox)sender).Text;

        if (Text != newText)
        {
            Text = newText;
        }

        UpdateMarkup();
    }

    private void OnInternalSelectionChanged(object sender, RoutedEventArgs e)
    {
        CaretIndex = _internalTextBox.CaretIndex;
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        UpdateMarkup();
    }
}