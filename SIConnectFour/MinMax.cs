using System.Collections.Generic;
using System.Linq;

namespace SIConnectFour
{
    public class MinMax
    {
        public static List<GameBoard> GetNextStates(GameBoard bs, bool maxPlayer)
        {
            List<int> moves = bs.GetAvailableMoves();
            List<GameBoard> states = new List<GameBoard>();
            
            foreach (int move in moves)
            {
                GameBoard nextState = new GameBoard(bs);
                nextState.MakeMove(move, bs.Player);

                states.Add(nextState);
            }
            
            if (maxPlayer)
            {
                states.Sort((a, b) => b.Estimation - a.Estimation);
            }
            else
            {
                states.Sort((a, b) => a.Estimation - b.Estimation);
            }

            return states;
        }

        public static int FindBestMove(GameBoard bs, int lookAhead, bool player)
        {
            int bestMoveIndex;


            AlphaBeta2(bs, lookAhead, int.MinValue, int.MaxValue, !player, out bestMoveIndex);

            return bestMoveIndex;
        }

        private static int AlphaBeta2(GameBoard bs, int depth, int alpha, int beta, bool maxPlayer, out int bestMove)
        {
            List<int> moves = bs.GetAvailableMoves();
            bestMove = moves.FirstOrDefault();

            
            if (depth == 0 || bs.Player1ExistingAnswers > 0
                || bs.Player2ExistingAnswers > 0 || moves.Count == 0)
            {
                return bs.Estimation;
            }

            List<GameBoard> nextStates = GetNextStates(bs, maxPlayer);

            if (maxPlayer)
            {
                foreach (GameBoard nextState in nextStates)
                {
                    int moveIdx;
                    int rating = AlphaBeta2(nextState, depth - 1, alpha, beta, false, out moveIdx);
                    
                    if (rating > alpha)
                    {
                        alpha = rating;
                        bestMove = nextState.LastMove;
                    }

                    if (beta <= alpha) // Beta cuts off
                    {
                        break;
                    }
                }

                return alpha;
            }
            else
            {
                foreach (GameBoard nextState in nextStates)
                {
                    int moveIdx;
                    int rating = AlphaBeta2(nextState, depth - 1, alpha, beta, true, out moveIdx);
                    
                    if (rating < beta)
                    {
                        beta = rating;
                        bestMove = nextState.LastMove;
                    }

                    if (beta <= alpha) // Alpha cuts off
                    {
                        break;
                    }
                }

                return beta;
            }
        }
    }
}