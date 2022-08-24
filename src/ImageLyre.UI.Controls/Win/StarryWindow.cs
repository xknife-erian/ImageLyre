using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace ImageLyre.UI.Controls.Win;

public class StarryWindow : Window
{
    public StarryWindow()
    {
        DefaultStyleKey = typeof(StarryWindow);
        CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));
        CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindow, CanResizeWindow));
        CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow, CanMinimizeWindow));
        CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindow, CanResizeWindow));
        CommandBindings.Add(new CommandBinding(SystemCommands.ShowSystemMenuCommand, ShowSystemMenu));

        Loaded += delegate
        {
            var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source?.AddHook(WndProc);
        };
    }

    public static readonly DependencyProperty WindowCommandVisibilityProperty = DependencyProperty.Register(
        nameof(WindowCommandVisibility), typeof(Visibility), typeof(StarryWindow), 
        new PropertyMetadata(System.Windows.Visibility.Visible));

    public Visibility WindowCommandVisibility
    {
        get => (Visibility) GetValue(WindowCommandVisibilityProperty);
        set => SetValue(WindowCommandVisibilityProperty, value);
    }

    public static readonly DependencyProperty ResizeVisibilityProperty = DependencyProperty.Register(
        nameof(ResizeVisibility), typeof(Visibility), typeof(StarryWindow),
        new PropertyMetadata(System.Windows.Visibility.Visible,OnResizeModeChanged));

    private static void OnResizeModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window)
            return;
        var v = (Visibility) e.NewValue;
        switch (v)
        {
            case Visibility.Collapsed:
            case Visibility.Hidden:
                window.ResizeMode = ResizeMode.NoResize;
                break;
            case Visibility.Visible:
                window.ResizeMode = ResizeMode.CanResize;
                break;
        }
    }

    public Visibility ResizeVisibility
    {
        get { return (Visibility) GetValue(ResizeVisibilityProperty); }
        set { SetValue(ResizeVisibilityProperty, value); }
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        if (SizeToContent == SizeToContent.WidthAndHeight)
            InvalidateMeasure();
    }

    #region Menus

    public static readonly DependencyProperty MenusProperty = DependencyProperty.Register(
        nameof(Menus), typeof(IEnumerable<MenuItemModel>), typeof(StarryWindow), 
        new PropertyMetadata(default(IEnumerable<MenuItemModel>), OnMenusAdded));

    private static void OnMenusAdded(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // if (d is not StarryWindow window) return;
        // var menu = Util.GetChildObjects<Menu>(d);
        // if (menu != null) menu[0].ItemsSource = e.NewValue as IEnumerable<MenuItemModel>;
    }

    private static object FindControl(DependencyObject obj, Type targetType)
    {
        if (obj.DependencyObjectType.SystemType == targetType)
            return obj;
        return FindControl(VisualTreeHelper.GetParent(obj), targetType);
    }

    public IEnumerable<MenuItemModel> Menus
    {
        get => (IEnumerable<MenuItemModel>) GetValue(MenusProperty);
        set => SetValue(MenusProperty, value);
    }

    #endregion

    #region Window Commands

    private void CanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
    }

    private void CanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = ResizeMode != ResizeMode.NoResize;
    }

    private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }

    private void MaximizeWindow(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.MaximizeWindow(this);
    }

    private void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.MinimizeWindow(this);
    }

    private void RestoreWindow(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.RestoreWindow(this);
    }

    private void ShowSystemMenu(object sender, ExecutedRoutedEventArgs e)
    {
        var element = e.OriginalSource as FrameworkElement;
        if (element == null)
            return;

        var point = WindowState == WindowState.Maximized
            ? new Point(0, element.ActualHeight)
            : new Point(Left + BorderThickness.Left, element.ActualHeight + Top + BorderThickness.Top);
        point = element.TransformToAncestor(this).Transform(point);
        SystemCommands.ShowSystemMenu(this, point);
    }

    #endregion

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        return IntPtr.Zero;
    }
}