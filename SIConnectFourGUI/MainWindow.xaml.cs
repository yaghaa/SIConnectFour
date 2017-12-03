using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;
using SIConnectFour;

namespace SIConnectFourGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int LookAhead = 3;
        private SolidColorBrush Color_Background;
        private SolidColorBrush Color_Player1;
        private SolidColorBrush Color_Player2;
        private SolidColorBrush Color_Empty;
        private GameBoard GameBoard;

        public MainWindow()
        {
            InitializeComponent();

            Color_Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD1D136")); // Yellow
            Color_Empty = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF")); // White
            Color_Player1 = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0000")); // Red
            Color_Player2 = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0000FF")); // Blue

            GameBoard = new GameBoard(Constant.ROWSIZE, Constant.COLUMNSIZE);
            GameBoard.PlayerMoved += GameBoard_PlayerMoved;

        }

        private void GameBoard_PlayerMoved(Position pos, bool player)
        {
            // Updates game board
            Ellipse disc = GetDisc(pos);
            if (disc == null) return;

            // Sets color and removes events
            SetColor(disc, (player ? 2 : 1));
            RemoveEvents(disc);

            if (GameBoard.Player1ExistingAnswers > 0)
            {
                // Player 1 wins!
                MessageBox.Show("Player 1 wins!");

                foreach (var child in Grid_GameBoard.Children)
                {
                    // Removes events from all discs
                    Ellipse remove = child as Ellipse;
                    RemoveEvents(remove);
                }

                return;
            }
            else if (GameBoard.Player2ExistingAnswers > 0)
            {
                // Player 2 wins!
                MessageBox.Show("Computer wins!");

                foreach (var child in Grid_GameBoard.Children)
                {
                    // Removes events from all discs
                    Ellipse remove = child as Ellipse;
                    RemoveEvents(remove);
                }

                return;
            }

            if (GameBoard.Player != Player.P1) // Computer's turn
                GameBoard.MakeMove(MinMax.FindBestMove(GameBoard, LookAhead, GameBoard.Player == Player.P1), GameBoard.Player);
        }

        private Ellipse GetDisc(Position pos)
        {
            if (pos == null) return null;

            // Calculates child index
            int index = Constant.COLUMNSIZE * pos.Row + pos.Column;
            if (index >= Grid_GameBoard.Children.Count) return null;

            return Grid_GameBoard.Children[index] as Ellipse;
        }

        private void Grid_GameBoard_Loaded(object sender, RoutedEventArgs e)
        {
            GameBoard.Reset();
            
            Grid_GameBoard.RowDefinitions.Clear();
            Grid_GameBoard.ColumnDefinitions.Clear();
            Grid_GameBoard.Children.Clear();
            
            for (int row = 0; row < Constant.ROWSIZE; row++)
            {
                Grid_GameBoard.RowDefinitions.Add(new RowDefinition());

                for (int col = 0; col < Constant.COLUMNSIZE; col++)
                {
                    if (row == 0)
                        Grid_GameBoard.ColumnDefinitions.Add(new ColumnDefinition()); // Sets column defintion
                    
                    Ellipse disc = new Ellipse();
                    disc.Fill = Color_Empty; // Sets color to empty
                    disc.Tag = new Position(row, col);
                    
                    Grid.SetRow(disc, row);
                    Grid.SetColumn(disc, col);
                    
                    disc.MouseEnter += Disc_MouseEnter;
                    disc.MouseLeave += Disc_MouseLeave;
                    disc.MouseLeftButtonUp += Disc_MouseLeftButtonUp;
                    
                    Grid_GameBoard.Children.Add(disc);
                }
            }
            
            Grid_GameBoard.ContextMenu = Grid_GameBoard.Resources["ContextMenu_Options"] as ContextMenu;
            
        }

        private void Disc_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Ellipse)) return;

            Ellipse disc = sender as Ellipse;
            Position pos = disc.Tag as Position;

            GameBoard.MakeMove(pos.Column, Player.P1);
        }

        private void SetColor(Ellipse disc, int color)
        {
            if (disc == null) return;

            switch (color)
            {
                default:
                    disc.Fill = Color_Empty;
                    break;
                case 1:
                    disc.Fill = Color_Player1;
                    break;
                case 2:
                    disc.Fill = Color_Player2;
                    break;
            }
        }

        private void RemoveEvents(Ellipse disc)
        {
            if (disc == null) return;

            // Removes events
            disc.MouseEnter -= Disc_MouseEnter;
            disc.MouseLeave -= Disc_MouseLeave;
            disc.MouseLeftButtonUp -= Disc_MouseLeftButtonUp;
        }

        private void Disc_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!(sender is Ellipse)) return;

            // Sets to empty color
            SetColor(sender as Ellipse, 0);
        }

        private void Disc_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!(sender is Ellipse)) return;

            // Sets to player color
            SetColor(sender as Ellipse, 1);
        }

        private void Button_Reset_Click(object sender, RoutedEventArgs e)
        {
            Grid_GameBoard_Loaded(sender, e);
        }

        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.R:
                    // Resets board
                    Grid_GameBoard_Loaded(sender, e);
                    break;
                case Key.X:
                case Key.Q:
                case Key.Escape:
                    // Quits application
                    this.Close();
                    break;
            }
        }
    }
}
