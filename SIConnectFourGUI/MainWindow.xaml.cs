using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        private List<TimeSpan> MoveTimesP1;
        private List<TimeSpan> MoveTimesP2;
        private int LookAheadP1 = 6;
        private int LookAheadP2 = 6;
        private SolidColorBrush Color_Background;
        private SolidColorBrush Color_Player1;
        private SolidColorBrush Color_Player2;
        private SolidColorBrush Color_Empty;
        private SolidColorBrush Color_Hover;
        private GameBoard GameBoard;

        public MainWindow()
        {
            MoveTimesP2 = new List<TimeSpan>();
            MoveTimesP1 = new List<TimeSpan>();
            InitializeComponent();

            Color_Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD1D136")); // Yellow
            Color_Empty = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF")); // White
            Color_Player1 = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0000")); // Red
            Color_Player2 = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0000FF")); // Blue
            Color_Hover = (SolidColorBrush)(new BrushConverter().ConvertFrom("#C0C0C0")); // Silver

            GameBoard = new GameBoard(Constant.ROWSIZE, Constant.COLUMNSIZE)
            {
                _p1H = cbP1H.SelectedIndex,
                _p2H = cbP2H.SelectedIndex
            };
            GameBoard.PlayerMoved += GameBoard_PlayerMoved;

        }

        private void GameBoard_PlayerMoved(Position pos, bool player)
        {
            // Updates game board
            Ellipse disc = GetDisc(pos);
            if (disc == null) return;

            // Sets color and removes events
            SetColor(disc, (player ? 1 : 2));
            RemoveEvents(disc);

            if (cbPlayType.SelectedIndex == 1)
            {
                Grid_GameBoard.Measure(new Size(505,254));
            }

            if (GameBoard.Player1ExistingAnswers > 0)
            {
                var P1Median = Median(MoveTimesP1);
                var P2Median = Median(MoveTimesP2);
                // Player 1 wins!
                MessageBox.Show($"Player 1 wins! \nŚredni czas ruchu P1: {P1Median}\nŚredni czas ruchu P2: {P2Median}");

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
                var P1Median = Median(MoveTimesP1);
                var P2Median = Median(MoveTimesP2);
                // Player 2 wins!
                MessageBox.Show($"Player 2 wins! \nŚredni czas ruchu P1: {P1Median}\nŚredni czas ruchu P2: {P2Median}");

                foreach (var child in Grid_GameBoard.Children)
                {
                    // Removes events from all discs
                    Ellipse remove = child as Ellipse;
                    RemoveEvents(remove);
                }

                return;
            }

            if (GameBoard.GetAvailableMoves().Count == 0)
            {
                var P1Median = Median(MoveTimesP1);
                var P2Median = Median(MoveTimesP2);
                // Player 2 wins!
                MessageBox.Show($"Draw! \nŚredni czas ruchu P1: {P1Median}\nŚredni czas ruchu P2: {P2Median}");
            }

            if ( GameBoard.Player != Player.P1) // Computer's turn
            {
                var sw = new Stopwatch();
                sw.Start();
                GameBoard.MakeMove(MinMax.FindBestMove(GameBoard, GameBoard.Player == Player.P1 ? LookAheadP1 : LookAheadP2, GameBoard.Player == Player.P2),
                    GameBoard.Player);
                sw.Stop();
                
                MoveTimesP2.Add(sw.Elapsed);
            }
        }

        public double Median(List<TimeSpan> ts)
        {
            ts.Sort();

            var n = ts.Count;

            double median;

            var isOdd = n % 2 != 0;
            if (isOdd)
            {
                median = ts[(n + 1) / 2 - 1].TotalMilliseconds;
            }
            else
            {
                median = (ts[n / 2 - 1].TotalMilliseconds + ts[n / 2].TotalMilliseconds) / 2.0d;
            }

            return median;
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
                    disc.Height = 40;
                    disc.Width = 40;
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
        }

        private void Disc_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Ellipse)) return;

            Ellipse disc = sender as Ellipse;
            Position pos = disc.Tag as Position;
            if(cbPlayType.SelectedIndex == 0)
                GameBoard.MakeMove(pos.Column, Player.P1);
            else
            {
                var sw2 = new Stopwatch();
                sw2.Start();
                GameBoard.MakeMove(MinMax.FindBestMove(GameBoard, GameBoard.Player == Player.P1 ? LookAheadP1 : LookAheadP2, GameBoard.Player == Player.P2),
                    GameBoard.Player);
                sw2.Stop();

                MoveTimesP1.Add(sw2.Elapsed);
            }
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
                case 3:
                    disc.Fill = Color_Hover;
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
            SetColor(sender as Ellipse, 3);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Grid_GameBoard_Loaded(sender, e);
            if (cbPlayType.SelectedIndex == 0)
            {
                Grid_GameBoard.IsEnabled = true;
                GameBoard._p2H = cbP2H.SelectedIndex;
            }
            else
            {
                Grid_GameBoard.IsEnabled = true;
                GameBoard._p1H = cbP1H.SelectedIndex;
                GameBoard._p2H = cbP2H.SelectedIndex;

                var sw2 = new Stopwatch();
                sw2.Start();
                GameBoard.MakeMove(MinMax.FindBestMove(GameBoard, GameBoard.Player == Player.P1 ? LookAheadP1 : LookAheadP2, GameBoard.Player == Player.P2),
                    GameBoard.Player);
                sw2.Stop();

                MoveTimesP1.Add(sw2.Elapsed);
            }
        }
    }
}
