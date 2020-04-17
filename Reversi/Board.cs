namespace Reversi
{
    public class Board
    {
        public enum Color
        {
            Black = -1,
            Empty = 0,
            White = 1
        }

        private readonly bool[,] safeDiscs;

        private readonly Color[,] squares;

        public Board()
        {
            squares = new Color[8, 8];
            safeDiscs = new bool[8, 8];

            int i, j;
            for (i = 0; i < 8; i++)
            for (j = 0; j < 8; j++)
            {
                squares[i, j] = Color.Empty;
                safeDiscs[i, j] = false;
            }

            UpdateCounts();
        }

        public int BlackCount { get; private set; }

        public int WhiteCount { get; private set; }

        public int EmptyCount { get; private set; }

        public int BlackFrontierCount { get; private set; }

        public int WhiteFrontierCount { get; private set; }

        public int BlackSafeCount { get; private set; }

        public int WhiteSafeCount { get; private set; }

        public static Color Invert(Color color) => (Color) (-(int) color);

        public void SetForNewGame()
        {
            int i, j;
            for (i = 0; i < 8; i++)
            for (j = 0; j < 8; j++)
            {
                squares[i, j] = Color.Empty;
                safeDiscs[i, j] = false;
            }
            squares[3, 3] = Color.White;
            squares[3, 4] = Color.Black;
            squares[4, 3] = Color.Black;
            squares[4, 4] = Color.White;
            UpdateCounts();
        }

        public Color GetSquareContents(int row, int col) => squares[row, col];

        public void MakeMove(Color color, int row, int col)
        {
            squares[row, col] = color;
            int dr, dc;
            for (dr = -1; dr <= 1; dr++)
            for (dc = -1; dc <= 1; dc++)
                if (!(dr == 0 && dc == 0) && IsOutflanking(color, row, col, dr, dc))
                {
                    int r = row + dr;
                    int c = col + dc;
                    while (squares[r, c] == Invert(color))
                    {
                        squares[r, c] = color;
                        r += dr;
                        c += dc;
                    }
                }
            UpdateCounts();
        }

        public bool HasAnyValidMove(Color color)
        {
            int r, c;
            for (r = 0; r < 8; r++)
            for (c = 0; c < 8; c++)
                if (IsValidMove(color, r, c))
                    return true;
            return false;
        }

        public bool IsValidMove(Color color, int row, int col)
        {
            if (squares[row, col] != Color.Empty)
                return false;
            int dr, dc;
            for (dr = -1; dr <= 1; dr++)
            for (dc = -1; dc <= 1; dc++)
                if (!(dr == 0 && dc == 0) && IsOutflanking(color, row, col, dr, dc))
                    return true;
            return false;
        }

        public int GetValidMoveCount(Color color)
        {
            int n = 0;
            int i, j;
            for (i = 0; i < 8; i++)
            for (j = 0; j < 8; j++)
                if (IsValidMove(color, i, j))
                    n++;
            return n;
        }

        private bool IsOutflanking(Color color, int row, int col, int dr, int dc)
        {
            int r = row + dr;
            int c = col + dc;
            while (r >= 0 && r < 8 && c >= 0 && c < 8 && squares[r, c] == Invert(color))
            {
                r += dr;
                c += dc;
            }
            if (r < 0 || r > 7 || c < 0 || c > 7 || r - dr == row && c - dc == col || squares[r, c] != color)
                return false;
            return true;
        }

        private void UpdateCounts()
        {
            BlackCount = 0;
            WhiteCount = 0;
            EmptyCount = 0;
            BlackFrontierCount = 0;
            WhiteFrontierCount = 0;
            WhiteSafeCount = 0;
            BlackSafeCount = 0;
            int i, j;
            bool statusChanged = true;
            while (statusChanged)
            {
                statusChanged = false;
                for (i = 0; i < 8; i++)
                for (j = 0; j < 8; j++)
                    if (squares[i, j] != Color.Empty && !safeDiscs[i, j] && !IsOutflankable(i, j))
                    {
                        safeDiscs[i, j] = true;
                        statusChanged = true;
                    }
            }
            for (i = 0; i < 8; i++)
            for (j = 0; j < 8; j++)
            {
                bool isFrontier = false;
                if (squares[i, j] != Color.Empty)
                {
                    int dr;
                    for (dr = -1; dr <= 1; dr++)
                    {
                        int dc;
                        for (dc = -1; dc <= 1; dc++)
                            if (!(dr == 0 && dc == 0) && i + dr >= 0 && i + dr < 8 && j + dc >= 0 && j + dc < 8 &&
                                squares[i + dr, j + dc] == Color.Empty)
                                isFrontier = true;
                    }
                }
                if (squares[i, j] == Color.Black)
                {
                    BlackCount++;
                    if (isFrontier)
                        BlackFrontierCount++;
                    if (safeDiscs[i, j])
                        BlackSafeCount++;
                }
                else if (squares[i, j] == Color.White)
                {
                    WhiteCount++;
                    if (isFrontier)
                        WhiteFrontierCount++;
                    if (safeDiscs[i, j])
                        WhiteSafeCount++;
                }
                else
                    EmptyCount++;
            }
        }

        private bool IsOutflankable(int row, int col)
        {
            Color color = squares[row, col];
            int i, j;
            bool hasSpaceSide1 = false;
            bool hasUnsafeSide1 = false;
            bool hasSpaceSide2 = false;
            bool hasUnsafeSide2 = false;
            for (j = 0; j < col && !hasSpaceSide1; j++)
                if (squares[row, j] == Color.Empty)
                    hasSpaceSide1 = true;
                else if (squares[row, j] != color || !safeDiscs[row, j])
                    hasUnsafeSide1 = true;
            for (j = col + 1; j < 8 && !hasSpaceSide2; j++)
                if (squares[row, j] == Color.Empty)
                    hasSpaceSide2 = true;
                else if (squares[row, j] != color || !safeDiscs[row, j])
                    hasUnsafeSide2 = true;
            if (hasSpaceSide1 && hasSpaceSide2 ||
                hasSpaceSide1 && hasUnsafeSide2 ||
                hasUnsafeSide1 && hasSpaceSide2)
                return true;
            hasSpaceSide1 = false;
            hasSpaceSide2 = false;
            hasUnsafeSide1 = false;
            hasUnsafeSide2 = false;
            for (i = 0; i < row && !hasSpaceSide1; i++)
                if (squares[i, col] == Color.Empty)
                    hasSpaceSide1 = true;
                else if (squares[i, col] != color || !safeDiscs[i, col])
                    hasUnsafeSide1 = true;
            for (i = row + 1; i < 8 && !hasSpaceSide2; i++)
                if (squares[i, col] == Color.Empty)
                    hasSpaceSide2 = true;
                else if (squares[i, col] != color || !safeDiscs[i, col])
                    hasUnsafeSide2 = true;
            if (hasSpaceSide1 && hasSpaceSide2 ||
                hasSpaceSide1 && hasUnsafeSide2 ||
                hasUnsafeSide1 && hasSpaceSide2)
                return true;
            hasSpaceSide1 = false;
            hasSpaceSide2 = false;
            hasUnsafeSide1 = false;
            hasUnsafeSide2 = false;
            i = row - 1;
            j = col - 1;
            while (i >= 0 && j >= 0 && !hasSpaceSide1)
            {
                if (squares[i, j] == Color.Empty)
                    hasSpaceSide1 = true;
                else if (squares[i, j] != color || !safeDiscs[i, j])
                    hasUnsafeSide1 = true;
                i--;
                j--;
            }
            i = row + 1;
            j = col + 1;
            while (i < 8 && j < 8 && !hasSpaceSide2)
            {
                if (squares[i, j] == Color.Empty)
                    hasSpaceSide2 = true;
                else if (squares[i, j] != color || !safeDiscs[i, j])
                    hasUnsafeSide2 = true;
                i++;
                j++;
            }
            if (hasSpaceSide1 && hasSpaceSide2 ||
                hasSpaceSide1 && hasUnsafeSide2 ||
                hasUnsafeSide1 && hasSpaceSide2)
                return true;
            hasSpaceSide1 = false;
            hasSpaceSide2 = false;
            hasUnsafeSide1 = false;
            hasUnsafeSide2 = false;
            i = row - 1;
            j = col + 1;
            while (i >= 0 && j < 8 && !hasSpaceSide1)
            {
                if (squares[i, j] == Color.Empty)
                    hasSpaceSide1 = true;
                else if (squares[i, j] != color || !safeDiscs[i, j])
                    hasUnsafeSide1 = true;
                i--;
                j++;
            }
            i = row + 1;
            j = col - 1;
            while (i < 8 && j >= 0 && !hasSpaceSide2)
            {
                if (squares[i, j] == Color.Empty)
                    hasSpaceSide2 = true;
                else if (squares[i, j] != color || !safeDiscs[i, j])
                    hasUnsafeSide2 = true;
                i++;
                j--;
            }
            if (hasSpaceSide1 && hasSpaceSide2 ||
                hasSpaceSide1 && hasUnsafeSide2 ||
                hasUnsafeSide1 && hasSpaceSide2)
                return true;
            return false;
        }
    }
}