using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class CompetitionBot : Opponent
{
    static readonly List<List<List<ulong>>> winCheckBoard = ConnectFour.precomputeWinChecks();

    bool winCheck(ulong pieces, int col, int row)
    {
        bool value = winCheckBoard[row][col].Any(board => (board & pieces) == board);
        return value;
    }
    int _bestMove(List<int> heights, bool myTurn, ulong myPieces, ulong enemyPieces, int move, int depth)
    {
        if (depth == 0)
            return 0;
        heights[move]++;

        if (myTurn)
        {
            myPieces += ((ulong)1) << ((heights[move]-1) * 7 + 6 - move);
            if (winCheck(myPieces, move, heights[move] - 1))
            {
                return -1 * depth;
            }
        }
        else
        {
            enemyPieces += ((ulong)1) << ((heights[move]-1) * 7 + 6 - move);
            if (winCheck(enemyPieces, move, heights[move] - 1))
            {
                return -1 * depth;
            }
        }

        int bestValue = int.MinValue;
        for (int i = 0; i < 7; i++)
        {
            if (heights[i] + 1 > 6)
                continue;
            int value = _bestMove(new List<int>(heights), !myTurn, myPieces, enemyPieces, i, depth - 1);
            value = -1 * value;
            if (value > bestValue)
                bestValue = value;
        }
        return bestValue;
    }

    public int bestMove(List<int> heights, bool myTurn, ulong myPieces, ulong enemyPieces)
    {
        List<int> bestMoves = new List<int>();
        int max = int.MinValue;
        int depth = 2;
       
        for (int i = 0; i < 7; i++)
        {
            if (heights[i] + 1 > 6)
                continue;
            int value = -1 * _bestMove(new List<int>(heights), myTurn, myPieces, enemyPieces, i, depth);

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
            myPieces = game.p1board;
            enemyPieces = game.p2board;
        }
        else
        {
            myPieces = game.p2board;
            enemyPieces = game.p1board;
        }
        return bestMove(game.heights, game.currentPlayer == player, myPieces, enemyPieces);
    }
}
