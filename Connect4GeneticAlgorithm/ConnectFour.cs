using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class ConnectFour
{
    public static readonly List<List<List<ulong>>> winCheckBoard = PrecomputeWinChecks();

    public double[] Board { get; private set; }
    public List<int> AvailableMoves { get; private set; }
    public List<int> PlayedMoves { get; private set; }
    public List<int> Heights { get; private set; }
    public int CurrentPlayer { get; private set; }
    public ulong P1Board { get; private set; }
    public ulong P2Board { get; private set; }

    public ConnectFour() 
    {
        Board = new double[42];
        AvailableMoves = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
        PlayedMoves = new List<int>();
        Heights = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
        CurrentPlayer = 1;
        P1Board = 0;
        P2Board = 0;
    }
    public bool MakeMove(int position)
    {
        if (!AvailableMoves.Contains(position))
        {
            return false;
        }
        PlayedMoves.Add(position);
        Board[Heights[position] * 7 + position] = CurrentPlayer;
        Heights[position]++;
        if (Heights[position] > 5)
        {
            AvailableMoves.Remove(position);
        }
        if (CurrentPlayer == 1)
        {
            P1Board += ((ulong) 1) << ((Heights[position] - 1) * 7 + 6 - position);
        }
        else
        {
            P2Board += ((ulong) 1) << ((Heights[position] - 1) * 7 + 6 - position);
        }
        CurrentPlayer = CurrentPlayer == 1 ? -1 : 1;
        return true;
    }

    public Status CheckWin(int position, int playerToCheck)
    {
        if (playerToCheck == 1 &&
            winCheckBoard[Heights[position]-1][position].Any(winCondition => (P1Board & winCondition) == winCondition))
        {
            return Status.P1Win;
        }
        if (playerToCheck == -1 &&
            winCheckBoard[Heights[position]-1][position].Any(winCondition => (P2Board & winCondition) == winCondition))
        {
            return Status.P2Win;
        }

        if (PlayedMoves.Count < 42)
        {
            return Status.Ongoing;
        }
            

        return Status.Draw;
    }

    static public List<List<List<ulong>>> PrecomputeWinChecks()
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
