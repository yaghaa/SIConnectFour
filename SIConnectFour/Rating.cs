﻿using System.Collections.Generic;

namespace SIConnectFour
{
    public class Rating
    {
        private readonly sbyte[][] _board;

        public Player Player;

        public int HorizontalsInRow = 0;
        public int VerticalsInRow = 0;
        public int DiagonalsLeftInRow = 0;
        public int DiagonalsRightInRow = 0;
        public int hSolutions, vSolutions, dSolutionsLeft, dSolutionsRight;

        public Rating(sbyte[][] board)
        {
            _board = board;
        }

        public void FeedRating(int row, int col, sbyte color, List<Position> p1Horizontals, List<Position> p1Verticals, List<Position> p1DiagonalsLeft, List<Position> p1DiagonalsRight)
        {
            HorizontalScore(row, col, color, p1Horizontals);
            VerticalScore(row, col, color, p1Verticals);
            DiagonalScoreLeft(row, col, color, p1DiagonalsLeft);
            DiagonalScoreRight(row, col, color, p1DiagonalsRight);
        }

        private void DiagonalScoreLeft(int row, int col, sbyte color,List<Position> previous)
        {
            dSolutionsLeft = 0;
            
            for (int i = Constant.CONNECT - 1; i >= 0; i--)
            {
                int startCol = col - i;
                int startRow = row - i;
                int runningScore = 0;

                for (int ii = 0; ii < Constant.CONNECT; ii++)
                {
                    Position pos = new Position(startRow + ii, startCol + ii);
                    
                    if (!WithinBounds(pos)) break;

                    sbyte selDisc = _board[startRow + ii][startCol + ii];
                    if (!(selDisc == 0 || selDisc == color) || previous.Contains(pos)) break;
                    
                    if (selDisc == color) runningScore++;
                    
                    if (ii == Constant.CONNECT - 1)
                    {
                        previous.Add(new Position(startRow, startCol));
                        if (runningScore == Constant.CONNECT)
                        {
                            dSolutionsLeft++;
                            DiagonalsLeftInRow += 1000;
                            continue;
                        }
                        
                        DiagonalsLeftInRow += 1 ;
                    }
                }
            }
        }

        private void DiagonalScoreRight(int row, int col, sbyte color, List<Position> previous)
        {
            dSolutionsRight = 0;
            
            for (int i = Constant.CONNECT - 1; i >= 0; i--)
            {
                int startCol = col + i;
                int startRow = row - i;
                int runningScore = 0;

                for (int ii = 0; ii < Constant.CONNECT; ii++)
                {
                    Position pos = new Position(startRow + ii, startCol - ii);
                    
                    if (!WithinBounds(pos)) break;

                    sbyte selDisc = _board[startRow + ii][startCol - ii];
                    if (!(selDisc == 0 || selDisc == color) || previous.Contains(pos)) break;
                    
                    if (selDisc == color) runningScore++;
                    
                    if (ii == Constant.CONNECT - 1)
                    {
                        previous.Add(new Position(startRow, startCol));
                        if (runningScore == Constant.CONNECT)
                        {
                            dSolutionsRight++;
                            DiagonalsRightInRow += 1000;
                            continue;
                        }
                        
                        DiagonalsRightInRow += 1;
                    }
                }
            }
        }

        private void HorizontalScore(int row, int col, sbyte color, List<Position> previous)
        {
            hSolutions = 0;
            
            for (int i = Constant.CONNECT - 1; i >= 0; i--)
            {
                int startCol = col - i;
                int runningScore = 0;

                for (int ii = 0; ii < Constant.CONNECT; ii++)
                {
                    Position pos = new Position(row, startCol + ii);
                    
                    if (!WithinBounds(pos)) break;

                    sbyte selDisc = _board[row][startCol + ii];
                    if (!(selDisc == 0 || selDisc == color) || previous.Contains(pos)) break;
                    
                    if (selDisc == color) runningScore++;
                    
                    if (ii == Constant.CONNECT - 1)
                    {
                        previous.Add(new Position(row, startCol));
                        if (runningScore == Constant.CONNECT)
                        {
                            hSolutions++;
                            HorizontalsInRow += 1000;
                            continue;
                        }
                        
                        HorizontalsInRow += 1;
                    }
                }
            }
        }

        private void VerticalScore(int row, int col, sbyte color, List<Position> previous)
        {
            vSolutions = 0;
            
            for (int i = Constant.CONNECT - 1; i >= 0; i--)
            {
                int startRow = row - i;
                int runningScore = 0;

                for (int ii = 0; ii < Constant.CONNECT; ii++)
                {
                    Position pos = new Position(startRow + ii, col);
                    
                    if (!WithinBounds(pos)) break;

                    sbyte selDisc = _board[startRow + ii][col];
                    if (!(selDisc == 0 || selDisc == color) || previous.Contains(pos)) break;
                    
                    if (selDisc == color) runningScore++;
                    
                    if (ii == Constant.CONNECT - 1)
                    {
                        previous.Add(new Position(startRow, col));
                        if (runningScore == Constant.CONNECT)
                        {
                            vSolutions++;
                            VerticalsInRow += 1000;
                            continue;
                        }
                        
                        VerticalsInRow += 1;
                    }
                }
            }
        }

        private bool WithinBounds(Position pos)
        {
            if (pos.Column < 0 || pos.Column >= Constant.COLUMNSIZE
                || pos.Row < 0 || pos.Row >= Constant.ROWSIZE) return false;

            return true;
        }

        public int GetExistingAnswers()
        {
            return vSolutions + hSolutions + dSolutionsLeft + dSolutionsRight;
        }

        public int CalculateRating()
        {
            var rating = 0;
            rating += CalculateValue(HorizontalsInRow);
            rating += CalculateValue(VerticalsInRow);
            rating += CalculateValue(DiagonalsRightInRow);
            rating += CalculateValue(DiagonalsLeftInRow);
            return rating;
        }

        private int CalculateValue(int value)
        {
            if (value == 1)
            {
                return 1;
            }

            if (value == 2)
            {
                return 3;
            }

            if (value == 3)
            {
                return 9;
            }

            if (value == 4)
            {
                return 27;
            }

            return 0;
        }
    }
}