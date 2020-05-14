using System;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Fraamo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int ExtraMaximizedMargin = 6;
        private bool comboBoxItemPressed = false;
        private Timer visibilityTimer = new Timer(3000);

        public MainWindow()
        {
            // Compensate for the extra space WPF adds by increasing the max width and height here
            this.MaxHeight = SystemParameters.WorkArea.Height + 2*ExtraMaximizedMargin;
            this.MaxWidth = SystemParameters.WorkArea.Width + 2*ExtraMaximizedMargin;

            InitializeComponent();

            // Setup drag on title bar
            titleBar.MouseLeftButtonDown += (o, e) => DragMove();
            new WindowResizer(this,
                new WindowBorder(BorderPosition.TopLeft, topLeft),
                new WindowBorder(BorderPosition.Top, top),
                new WindowBorder(BorderPosition.TopRight, topRight),
                new WindowBorder(BorderPosition.Right, right),
                new WindowBorder(BorderPosition.BottomRight, bottomRight),
                new WindowBorder(BorderPosition.Bottom, bottom),
                new WindowBorder(BorderPosition.BottomLeft, bottomLeft),
                new WindowBorder(BorderPosition.Left, left)
                );

            // Title bar visibility timer
            visibilityTimer.Elapsed += OnTimedEvent;
            visibilityTimer.AutoReset = false;
            visibilityTimer.Start();
        }

        private void CommandBinding_CanExecute_1(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_1(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void CommandBinding_Executed_2(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                SystemCommands.MaximizeWindow(this);
            }
            else 
            {
                SystemCommands.RestoreWindow(this);
            }
        }

        private void CommandBinding_Executed_3(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }
        
        private void SetWindowSize(int width, int height)
        {
            MainWindow1.WindowState = WindowState.Normal;
            this.Width = (double)width;
            this.Height = (double)height;
        }

        private void UpdateSizeText()
        {
            comboBox.Text = ((int)this.ActualWidth).ToString() + "x" + ((int)this.ActualHeight).ToString();
        }
        
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSizeText();
        }

        private void titleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState == WindowState.Normal)
                {
                    SystemCommands.MaximizeWindow(this);
                }
                else
                {
                    SystemCommands.RestoreWindow(this);
                }
            }
        }

        private void MainWindow1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.DefaultHeight = this.Height;
            Properties.Settings.Default.DefaultWidth = this.Width;
            Properties.Settings.Default.DefaultLeft = this.Left;
            Properties.Settings.Default.DefaultTop = this.Top;

            Properties.Settings.Default.Save();
        }

        private void comboBox_ItemPressed(ComboBox cmb)
        {
            ComboBoxItem selectedItem = (cmb.SelectedItem as ComboBoxItem);

            if (selectedItem != null)
            {
                string selectedString = selectedItem.Content.ToString();

                Regex splitString = new Regex(@"(\d+)x(\d+)", RegexOptions.IgnoreCase);
                MatchCollection matches = splitString.Matches(selectedString);

                if (matches.Count > 0)
                {
                    int width = Int32.Parse(matches[0].Groups[1].Value);
                    int height = Int32.Parse(matches[0].Groups[2].Value);
                    SetWindowSize(width, height);
                    Keyboard.ClearFocus();
                }
            }
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox cmb = sender as ComboBox;
                comboBoxItemPressed = !cmb.IsDropDownOpen;

                comboBox_ItemPressed(cmb);
            }
            catch (Exception)
            {
                // DO NOTHING
            }
        }

        private void comboBox_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                ComboBox cmb = sender as ComboBox;
                if (comboBoxItemPressed)
                {
                    comboBox_ItemPressed(cmb);
                }
                comboBoxItemPressed = true;
            }
            catch (Exception)
            {
                // DO NOTHING
            }
        }

        private void comboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string selectedString = comboBox.Text;
                    comboBox.SelectedItem = null;

                    Regex splitString = new Regex(@"(\d+)x(\d+)", RegexOptions.IgnoreCase);
                    MatchCollection matches = splitString.Matches(selectedString);

                    if (matches.Count > 0)
                    {
                        int width = Int32.Parse(matches[0].Groups[1].Value);
                        int height = Int32.Parse(matches[0].Groups[2].Value);
                        SetWindowSize(width, height);
                        Keyboard.ClearFocus();
                    }
                    else comboBox.Focus();
                }
                catch (Exception)
                {
                    comboBox.Focus();
                }
            }
        }

        private void titleBar_Rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            showTitleBar();
        }

        private void titleBar_MouseEnter(object sender, MouseEventArgs e)
        {
            showTitleBar();
        }

        private void titleBar_MouseLeave(object sender, MouseEventArgs e)
        {
            visibilityTimer.Stop();
            visibilityTimer.Start();
        }

        private void showTitleBar()
        {
            visibilityTimer.Start();
            titleBar.Visibility = Visibility.Visible;
        }

        private void hideTitleBar()
        {
            visibilityTimer.Stop();
            titleBar.Visibility = Visibility.Collapsed;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if(titleBar.IsMouseOver)
                {
                    visibilityTimer.Start();
                }
                else
                {
                    hideTitleBar();
                }
            });
        }

        private void MainWindow1_Initialized(object sender, EventArgs e)
        {
            // Set position and size as when it closed
            this.Height = Properties.Settings.Default.DefaultHeight;
            this.Width = Properties.Settings.Default.DefaultWidth;
            this.Left = Properties.Settings.Default.DefaultLeft;
            this.Top = Properties.Settings.Default.DefaultTop;

            if (this.Top < SystemParameters.VirtualScreenTop - ExtraMaximizedMargin)
            {
                this.Top = SystemParameters.VirtualScreenTop;
            }

            if (this.Left < SystemParameters.VirtualScreenLeft - ExtraMaximizedMargin)
            {
                this.Left = SystemParameters.VirtualScreenLeft;
            }

            if (this.Left + this.Width > SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth + ExtraMaximizedMargin)
            {
                this.Left = SystemParameters.VirtualScreenWidth + SystemParameters.VirtualScreenLeft - this.Width;
            }

            if (this.Top + this.Height > SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight + ExtraMaximizedMargin)
            {
                this.Top = SystemParameters.VirtualScreenHeight + SystemParameters.VirtualScreenTop - this.Height;
            }
        }
    }
}
