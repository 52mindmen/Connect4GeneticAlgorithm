using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class ConnectFour
{
    public static readonly List<List<List<ulong>>> winCheckBoard = precomputeWinChecks();

    public double[] board;
    public List<int> availableMoves;
    public List<int> playedMoves;
    public List<int> heights;
    public int currentPlayer { get; private set; }
    public ulong p1board { get; private set; }
    public ulong p2board { get; private set; }

    public ConnectFour() 
    {
        board = new double[42];
        availableMoves = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
        playedMoves = new List<int>();
        heights = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
        currentPlayer = 1;
        p1board = 0;
        p2board = 0;
    }
    public bool MakeMove(int position)
    {
        if (!availableMoves.Contains(position))
        {
            return false;
        }
        playedMoves.Add(position);
        board[heights[position] * 7 + position] = currentPlayer;
        heights[position]++;
        if (heights[position] > 5)
        {
            availableMoves.Remove(position);
        }
        if (currentPlayer == 1)
        {
            p1board += ((ulong) 1) << ((heights[position] - 1) * 7 + 6 - position);
        }
        else
        {
            p2board += ((ulong) 1) << ((heights[position] - 1) * 7 + 6 - position);
        }
        currentPlayer = currentPlayer == 1 ? -1 : 1;
        return true;
    }

    public Status CheckWin(int position, int playerToCheck)
    {
        if (playerToCheck == 1 &&
            winCheckBoard[heights[position]-1][position].Any(winCondition => (p1board & winCondition) == winCondition))
        {
            return Status.P1Win;
        }
        if (playerToCheck == -1 &&
            winCheckBoard[heights[position]-1][position].Any(winCondition => (p2board & winCondition) == winCondition))
        {
            return Status.P2Win;
        }

        if (playedMoves.Count < 42)
        {
            return Status.Ongoing;
        }
            

        return Status.Draw;
    }

    static public List<List<List<ulong>>> precomputeWinChecks()
    {
        {
            List<List<List<ulong>>> board = new(6);
            for (int i = 0; i < 6; i++)
            {
                List<List<ulong>> col = new(7);
                for (int j = 0; j < 7; j++)
                    col.Add(new List<ulong>());
                board.Add(col);
            }

            ulong horizontal = 15;
            ulong vertical = 2113665;
            ulong diagonalA = 16843009; //left top to right bottom
            ulong diagonalB = 2130440; //right top to left bottom
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    ulong modifiedHorizontal = horizontal << j + i * 7;

                    for (int k = 0; k < 4; k++)
                        board[i][6 - (j + k)].Add(modifiedHorizontal);
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    ulong modifiedVertical = vertical << j + i * 7;

                    for (int k = 0; k < 4; k++)
                        board[(i + k)][6 - j].Add(modifiedVertical);
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    ulong modifiedDiagonalA = diagonalA << j + i * 7;

                    for (int k = 0; k < 4; k++)
                        board[(i + k)][6 - (j + k)].Add(modifiedDiagonalA);

                    ulong modifiedDiagonalB = diagonalB << j + i * 7;

                    for (int k = 0; k < 4; k++)
                        board[(i + k)][3 - j + k].Add(modifiedDiagonalB);
                }
            }

            return board;
        }
    }
}
