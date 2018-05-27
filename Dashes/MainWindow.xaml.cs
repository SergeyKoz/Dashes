using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();            
        }


        public Canvas GameFieldCanvas
        {
            get { return gameFieldCanvas; }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {   
            MessageBoxResult Result = MessageBox.Show("Do you want to start new game?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (Result == MessageBoxResult.Yes)
            {
                ((App)(Application.Current)).Game.NewGame();
            }
        }
               
        private void gameFieldCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            ((App)(Application.Current)).Game.GameCursor.CursorMove(sender, e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ((App)(Application.Current)).Game.InitGameField();
        }

        private void gameFieldCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((App)(Application.Current)).Game.SetStep(sender, e);
        }

        private void gameFieldCanvas_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((App)(Application.Current)).Game.GameCursor.ChangeLineModeField(sender, e);
        }

        private void gameFieldCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            ((App)(Application.Current)).Game.GameCursor.CursorLeave(sender, e);
        }

        private void Option_Click(object sender, RoutedEventArgs e)
        {
            Window OptionsWindow = new Options();
            OptionsWindow.ShowDialog();
        }
    }
}
