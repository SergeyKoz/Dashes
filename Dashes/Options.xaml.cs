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
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for options.xaml
    /// </summary>
    public partial class Options : Window
    {

        private int[] sizeFieldOptions = new int[] { 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20};
        private string[] gameModeOptions = new string[] { "Player 1 Palyer 2", "Player 1 Computer" };

        private VirtualizingStackPanel PanelX, PanelY;

        private int InitX, InitY;

        private GameMode InitGameMode;

        public Options()
        {
            InitializeComponent();
            FieldSizeX.ItemsSource = sizeFieldOptions;
            FieldSizeY.ItemsSource = sizeFieldOptions;
            GameModeList.ItemsSource = gameModeOptions;

            GameField Game=((App)(Application.Current)).Game;
            InitX = Game.x;
            InitY = Game.y;
            InitGameMode = Game.GameMode;
        }
        
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            bool OptionsChanged=false;
            GameField Game = ((App)(Application.Current)).Game;

            int NewX = FieldSizeX.SelectedItem == null ? InitX : (int)FieldSizeX.SelectedItem;
            int NewY = FieldSizeY.SelectedItem == null ? InitY : (int)FieldSizeY.SelectedItem;



            GameMode NewGameMode;
            switch (GameModeList.SelectedIndex)
            {
                case 0:
                    NewGameMode = GameMode.Player1Player2;
                    break;
                case 1:
                    NewGameMode = GameMode.Player1Computer;
                    break;
                default:
                    NewGameMode = GameMode.Player1Player2;
                    break;
            }

            if (NewX != InitX || NewY != InitY || NewGameMode != InitGameMode)
            {
                OptionsChanged = true;
            }

            if (OptionsChanged)
            {
                MessageBoxResult Result = MessageBox.Show("Do you want to save changes and start new game?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (Result == MessageBoxResult.Yes)
                {
                    Game.x = NewX;
                    Game.y = NewY;
                    Game.GameMode = NewGameMode;
                    Game.NewGame();
                }
            }

            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {   
            GameField Game = ((App)(Application.Current)).Game;

            int offset, gameModeIndex;
            
            PanelX = FindVisualChild<VirtualizingStackPanel>(FieldSizeX);
            PanelY = FindVisualChild<VirtualizingStackPanel>(FieldSizeY);

            offset = Game.x - sizeFieldOptions[0];
            PanelX.SetVerticalOffset(offset);

            offset = Game.y - sizeFieldOptions[0];
            PanelY.SetVerticalOffset(offset);

            switch (Game.GameMode)
            {
                case GameMode.Player1Player2:
                    gameModeIndex = 0;
                    break;
                case GameMode.Player1Computer:
                    gameModeIndex = 1;
                    break;
                default:
                    gameModeIndex = 0;
                    break;
            }

            GameModeList.SelectedIndex = gameModeIndex;
        }

        private void FieldSizeX_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                FieldSizeX.SelectedIndex = (int)PanelX.VerticalOffset;
            }
        }

        private void FieldSizeY_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {   
            if (this.IsLoaded)
            {
                FieldSizeY.SelectedIndex = (int)PanelY.VerticalOffset;
            }
        }

        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child is T)
                {
                    return (T)child;
                }
                else
                {
                    child = FindVisualChild<T>(child);
                    if (child != null)
                    {
                        return (T)child;
                    }
                }
            }
            return null;
        }

    }
}
