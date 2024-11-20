using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class CompetitionBot : Opponent
{
    static readonly List<List<List<ulong>>> winCheckBoard = ConnectFour.PrecomputeWinChecks();

    static bool WinCheck(ulong pieces, int col, int row)
    {
        bool value = winCheckBoard[row][col].Any(board => (board & pieces) == board);
        return value;
    }
    int BestMove(List<int> heights, bool myTurn, ulong myPieces, ulong enemyPieces, int move, int depth)
    {
        if (depth == 0)
            return 0;
        heights[move]++;

        if (myTurn)
        {
            myPieces += ((ulong)1) << ((heights[move]-1) * 7 + 6 - move);
            if (WinCheck(myPieces, move, heights[move] - 1))
            {
                return -1 * depth;
            }
        }
        else
        {
            enemyPieces += ((ulong)1) << ((heights[move]-1) * 7 + 6 - move);
            if (WinCheck(enemyPieces, move, heights[move] - 1))
            {
                return -1 * depth;
            }
        }

        int bestValue = int.MinValue;
        for (int i = 0; i < 7; i++)
        {
            if (heights[i] + 1 > 6)
                continue;
            int value = BestMove(new List<int>(heights), !myTurn, myPieces, enemyPieces, i, depth - 1);
            value = -1 * value;
            if (value > bestValue)
                bestValue = value;
        }
        return bestValue;
    }

    public int BestMove(List<int> heights, bool myTurn, ulong myPieces, ulong enemyPieces)
    {
        List<int> bestMoves = new List<int>();
        int max = int.MinValue;
        int depth = 2;
       
        for (int i = 0; i < 7; i++)
        {
            if (heights[i] + 1 > 6)
                continue;
            int value = -1 * BestMove(new List<int>(heights), myTurn, myPieces, enemyPieces, i, depth);

            if (value > max)
            {
                max = value;
                bestMoves.Clear();
                bestMoves.Add(i);
            }
            else if (value == max)
            {
                bestMoves.Add(i);
            }
        }
        int bestMove;
        if (bestMoves.Count != 0)
            bestMove = bestMoves.MinBy(x => Math.Abs(x - 3));
        else
            bestMove = 0;

        return bestMove;
    }

    override public int GetMove(ConnectFour game, int player)
    {
        ulong myPieces;
        ulong enemyPieces;
        if (player == 1)
        {
            myPieces = game.P1Board;
            enemyPieces = game.P2Board;
        }
        else
        {
            myPieces = game.P2Board;
            enemyPieces = game.P1Board;
        }
        return BestMove(game.Heights, game.CurrentPlayer == player, myPieces, enemyPieces);
    }
}
