using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIConnectFour
{
    public class GameBoard
    {
        private sbyte[][] _board;
        private Player _player;
        private int _lastMove;
        

        private int _estimation;
        private int _p1ExistingAnswers;
        private int _p2ExistingAnswers;
        public int _p1H;
        public int _p2H;

        public delegate void MoveHandler(Position pos, bool player);
        public event MoveHandler PlayerMoved;
        
        public Player Player => _player;

        public int Player1ExistingAnswers => _p1ExistingAnswers;

        public int Player2ExistingAnswers => _p2ExistingAnswers;

        public int Estimation => _estimation;

        public int LastMove => _lastMove;

        public List<int> GetAvailableMoves()
        {
            List<int> moves = new List<int>();

            for (int i = 0; i < Constant.COLUMNSIZE; i++)
            {
                if (_board[0][i] == 0) moves.Add(i);
            }

            return moves;
        }

        public bool IsCorrectMove(int column, Player player)
        {
            if (player != _player || column < 0 || column >= Constant.COLUMNSIZE) return false;

            for (int row = 0; row < Constant.ROWSIZE; row++)
            {
                if (_board[row][column] == 0) return true;
            }

            return false;
        }

        public int MakeMove(int column, Player player)
        {
            if (!IsCorrectMove(column, player)) return -1;
            int row = 0;
            for (var i = 0; i < Constant.ROWSIZE; i++)
            {
                if (_board[i][column] == 0)
                {
                    row = i;
                }

            }
            
            _board[row][column] = (sbyte)(player == Player.P1 ? 1 : 2);
            _player = (_player == Player.P1)?Player.P2:Player.P1;
            
            _estimation = CalculateRating(out _p1ExistingAnswers, out _p2ExistingAnswers);
            OnPlayerMove(new Position(row, column), _player);

            return row;
        }
        
        private int CalculateRating(out int p1ExistingAnswers, out int p2ExistingAnswers)
        {
            int p1Score = 0, p2Score = 0;
            p1ExistingAnswers = 0;
            p2ExistingAnswers = 0;

            List<Position> p1Horizontals = new List<Position>();
            List<Position> p1Verticals = new List<Position>();
            List<Position> p1DiagonalsLeft = new List<Position>();
            List<Position> p1DiagonalsRight = new List<Position>();

            List<Position> p2Horizontals = new List<Position>();
            List<Position> p2Verticals = new List<Position>();
            List<Position> p2DiagonalsLeft = new List<Position>();
            List<Position> p2DiagonalsRight = new List<Position>();

            for (int col = 0; col < Constant.COLUMNSIZE; col++)
            {
                for (int row = Constant.ROWSIZE - 1; row >= 0; row--)
                {
                    #region FirstHeuristic
                    if((Player == Player.P1 && _p1H==0) || (Player == Player.P2 && _p2H == 0))
                    {
                        sbyte color = _board[row][col];
                        if (color == 0) break;
                        else if (color == 1)
                        {
                            var rating = new Rating(_board);
                            rating.FeedRating(row, col, color, ref p1Horizontals, ref p1Verticals, ref p1DiagonalsLeft, ref p1DiagonalsRight);

                            p1Score += rating.CalculatePionts();
                            p1ExistingAnswers += rating.GetExistingAnswers();
                        }
                        else if (color == 2)
                        {
                            var rating = new Rating(_board);
                            rating.FeedRating(row, col, color, ref p2Horizontals, ref p2Verticals, ref p2DiagonalsLeft, ref p2DiagonalsRight);

                            p2Score += rating.CalculatePionts();
                            p2ExistingAnswers += rating.GetExistingAnswers();
                        }
                        
                    }
                    #endregion
                    #region SecondHeuristic
                    else if ((Player == Player.P1 && _p1H == 1) || (Player == Player.P2 && _p2H == 1))
                    {
                        
                        sbyte color = _board[row][col];
                        if (color == 0) break;
                        else if (color == 1)
                        {
                            var rating = new Rating(_board);
                            rating.FeedRating(row, col, color, ref p1Horizontals, ref p1Verticals, ref p1DiagonalsLeft, ref p1DiagonalsRight);

                            p1Score += rating.CalculatePoints2();
                            p1ExistingAnswers += rating.GetExistingAnswers();
                        }
                        else if (color == 2)
                        {
                            var rating = new Rating(_board);
                            rating.FeedRating(row, col, color, ref p2Horizontals, ref p2Verticals, ref p2DiagonalsLeft, ref p2DiagonalsRight);

                            p2Score += rating.CalculatePoints2();
                            p2ExistingAnswers += rating.GetExistingAnswers();
                        }
                    }
                    #endregion

                    #region ThirdHeuristic
                    else if ((Player == Player.P1 && _p1H == 2) || (Player == Player.P2 && _p2H == 2))
                    {

                        sbyte color = _board[row][col];
                        if (color == 0) break;
                        else if (color == 1)
                        {
                            var rating = new Rating(_board);
                            rating.FeedRating(row, col, color, ref p1Horizontals, ref p1Verticals, ref p1DiagonalsLeft, ref p1DiagonalsRight);
                            if (Player == Player.P1)
                            {

                                p1Score += rating.CalculatePoints2();
                            }
                            else
                            {
                                p1Score += rating.CalculatePionts();
                            }
                            p1ExistingAnswers += rating.GetExistingAnswers();
                        }
                        else if (color == 2)
                        {
                            var rating = new Rating(_board);
                            rating.FeedRating(row, col, color, ref p2Horizontals, ref p2Verticals, ref p2DiagonalsLeft, ref p2DiagonalsRight);

                            if (Player == Player.P2)
                            {

                                p2Score += rating.CalculatePoints2();
                            }
                            else
                            {
                                p2Score += rating.CalculatePionts();
                            }
                            p2ExistingAnswers += rating.GetExistingAnswers();
                        }
                    }
                    #endregion
                }
            }

            if (p1ExistingAnswers > 0)
                return int.MaxValue;
            else if (p2ExistingAnswers > 0)
                return int.MinValue;
            else
                return p1Score - p2Score;

        }

        private void OnPlayerMove(Position position, Player player)
        {
            _lastMove = position.Column;

            MoveHandler handler = PlayerMoved;
            if (handler == null) return; 

            handler(position, player == Player.P1);
        }

        public GameBoard(int rowSize, int columnSize)
        {
            Constant.ROWSIZE = rowSize;
            Constant.COLUMNSIZE = columnSize;

            InitializeBoard(Constant.ROWSIZE, Constant.COLUMNSIZE);
        }

        public GameBoard(GameBoard bs)
        {
            _p1H = bs._p1H;
            _p2H = bs._p2H;
            _board = bs._board.Select(a => a.ToArray()).ToArray(); // Deep copy

            _player = bs._player;
            _estimation = bs._estimation;

            _p1ExistingAnswers = bs._p1ExistingAnswers;
            _p2ExistingAnswers = bs._p2ExistingAnswers;
        }

        private void InitializeBoard(int rowSize, int columnSize)
        {
            _player = Player.P1;
            _lastMove = -1;
            
            _board = new sbyte[rowSize][];

            for (int i = 0; i < rowSize; i++)
                _board[i] = new sbyte[columnSize];
            
            _estimation = 0;
            _p1ExistingAnswers = 0;
            _p2ExistingAnswers = 0;
        }

        public void Reset()
        {
            InitializeBoard(Constant.ROWSIZE, Constant.COLUMNSIZE);
        }
    }
}
