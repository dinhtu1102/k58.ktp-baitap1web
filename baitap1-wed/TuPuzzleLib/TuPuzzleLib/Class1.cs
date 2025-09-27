// SlidingPuzzle.cs
using System;

namespace TuPuzzleLib
{
    public class SlidingPuzzle
    {
        // public properties (classic property syntax - compatible C#2.0)
        private int rowsField;
        private int colsField;
        private int[] boardField;
        private string lastErrorField;
        private string authorField;

        // movement vectors: up, down, left, right
        private int[] dx;
        private int[] dy;

        public SlidingPuzzle(int rows, int cols)
        {
            rowsField = rows;
            colsField = cols;
            boardField = new int[rowsField * colsField];
            dx = new int[4];
            dy = new int[4];
            dx[0] = -1; dy[0] = 0; // up
            dx[1] = 1; dy[1] = 0; // down
            dx[2] = 0; dy[2] = -1;// left
            dx[3] = 0; dy[3] = 1; // right

            authorField = "Tú Nguyễn Đình - TuPuzzleLib";
            InitGoal();
        }

        public int Rows { get { return rowsField; } }
        public int Cols { get { return colsField; } }
        public int[] Board { get { return (int[])boardField.Clone(); } set { if (value != null && value.Length == rowsField * colsField) boardField = (int[])value.Clone(); else lastErrorField = "Invalid board size."; } }
        public string LastError { get { return lastErrorField; } }
        public string AuthorSignature { get { return authorField; } }

        // Initialize to goal state 1..N-1, 0 as blank at the end
        public void InitGoal()
        {
            int n = rowsField * colsField;
            for (int i = 0; i < n; i++)
            {
                if (i < n - 1) boardField[i] = i + 1;
                else boardField[i] = 0;
            }
        }

        // Find index of zero (blank)
        private int FindZero(int[] b)
        {
            for (int i = 0; i < b.Length; i++) if (b[i] == 0) return i;
            return -1;
        }

        // Swap helper
        private void Swap(int[] b, int i, int j)
        {
            int t = b[i]; b[i] = b[j]; b[j] = t;
        }

        // Manhattan distance
        private int ComputeManhattan(int[] b)
        {
            int sum = 0;
            for (int i = 0; i < b.Length; i++)
            {
                int v = b[i];
                if (v == 0) continue;
                int goalR = (v - 1) / colsField;
                int goalC = (v - 1) % colsField;
                int r = i / colsField;
                int c = i % colsField;
                int dr = r - goalR; if (dr < 0) dr = -dr;
                int dc = c - goalC; if (dc < 0) dc = -dc;
                sum += dr + dc;
            }
            return sum;
        }

        // Check solvability for general NxM (standard 15-puzzle rules)
        public bool IsSolvable()
        {
            int[] b = boardField;
            int inversions = 0;
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i] == 0) continue;
                for (int j = i + 1; j < b.Length; j++)
                {
                    if (b[j] == 0) continue;
                    if (b[i] > b[j]) inversions++;
                }
            }
            if (colsField % 2 == 1)
            {
                // odd grid width: inversions must be even
                return (inversions % 2) == 0;
            }
            else
            {
                // even grid width: blank row counting from bottom matters
                int zeroIndex = FindZero(b);
                int zeroRowFromTop = zeroIndex / colsField;
                int zeroRowFromBottom = rowsField - zeroRowFromTop;
                bool cond = ((zeroRowFromBottom % 2 == 0) && (inversions % 2 == 1)) || ((zeroRowFromBottom % 2 == 1) && (inversions % 2 == 0));
                return cond;
            }
        }

        // Shuffle randomly (in-place)
        public void Shuffle(int moves)
        {
            Random r = new Random();
            int zero = FindZero(boardField);
            for (int k = 0; k < moves; k++)
            {
                int zr = zero / colsField;
                int zc = zero % colsField;
                int[] cand = new int[4];
                int cnt = 0;
                for (int d = 0; d < 4; d++)
                {
                    int nr = zr + dx[d]; int nc = zc + dy[d];
                    if (nr >= 0 && nr < rowsField && nc >= 0 && nc < colsField)
                    {
                        cand[cnt++] = d;
                    }
                }
                if (cnt == 0) break;
                int choice = cand[r.Next(cnt)];
                int newZero = (zr + dx[choice]) * colsField + (zc + dy[choice]);
                Swap(boardField, zero, newZero);
                zero = newZero;
            }
        }

        // Deterministic "personal" shuffle using a signature string (dấu ấn cá nhân)
        public void SignatureShuffle(string signature)
        {
            if (signature == null) signature = "Anonymous";
            int seed = 0;
            for (int i = 0; i < signature.Length; i++)
            {
                seed = seed * 31 + (int)signature[i];
            }
            if (seed < 0) seed = -seed;
            Random r = new Random(seed & 0x7fffffff);
            int moves = 30 + (Math.Abs(seed) % 50);
            int zero = FindZero(boardField);
            for (int k = 0; k < moves; k++)
            {
                int zr = zero / colsField;
                int zc = zero % colsField;
                int[] cand = new int[4];
                int cnt = 0;
                for (int d = 0; d < 4; d++)
                {
                    int nr = zr + dx[d]; int nc = zc + dy[d];
                    if (nr >= 0 && nr < rowsField && nc >= 0 && nc < colsField)
                    {
                        cand[cnt++] = d;
                    }
                }
                if (cnt == 0) break;
                int choice = cand[r.Next(cnt)];
                int newZero = (zr + dx[choice]) * colsField + (zc + dy[choice]);
                Swap(boardField, zero, newZero);
                zero = newZero;
            }
        }

        // Solve using IDA*; returns true and out moves (array of directions 0:up,1:down,2:left,3:right)
        // maxDepth: safety limit to avoid infinite/very-long runs
        public bool Solve(out int[] solution, int maxDepth)
        {
            solution = null;
            if (!IsSolvable())
            {
                lastErrorField = "Unsovable configuration.";
                return false;
            }
            int n = boardField.Length;
            int[] boardCopy = (int[])boardField.Clone();
            int zero = FindZero(boardCopy);
            int h = ComputeManhattan(boardCopy);
            if (h == 0)
            {
                solution = new int[0];
                return true;
            }
            int threshold = h;
            int[] path = new int[maxDepth + 1];
            int pathLen = 0;
            while (threshold <= maxDepth)
            {
                int t = Search(boardCopy, zero, 0, threshold, -1, path, ref pathLen);
                if (t == -1)
                {
                    solution = new int[pathLen];
                    for (int i = 0; i < pathLen; i++) solution[i] = path[i];
                    return true;
                }
                if (t == int.MaxValue) break;
                threshold = t;
            }
            lastErrorField = "No solution within maxDepth.";
            return false;
        }

        // Opposite direction helper
        private int Opposite(int dir)
        {
            if (dir == 0) return 1;
            if (dir == 1) return 0;
            if (dir == 2) return 3;
            if (dir == 3) return 2;
            return -1;
        }

        // IDA* recursive search: return -1 if found; otherwise return minimal threshold exceeded.
        private int Search(int[] board, int zeroPos, int g, int threshold, int lastMove, int[] path, ref int pathLen)
        {
            int h = ComputeManhattan(board);
            int f = g + h;
            if (f > threshold) return f;
            if (h == 0)
            {
                pathLen = g;
                return -1; // found
            }
            int min = int.MaxValue;
            int zr = zeroPos / colsField;
            int zc = zeroPos % colsField;
            for (int dir = 0; dir < 4; dir++)
            {
                if (Opposite(dir) == lastMove) continue;
                int nr = zr + dx[dir];
                int nc = zc + dy[dir];
                if (nr < 0 || nr >= rowsField || nc < 0 || nc >= colsField) continue;
                int newZero = nr * colsField + nc;
                Swap(board, zeroPos, newZero);
                path[g] = dir;
                int t = Search(board, newZero, g + 1, threshold, dir, path, ref pathLen);
                if (t == -1) return -1;
                if (t < min) min = t;
                Swap(board, zeroPos, newZero); // backtrack
            }
            return min;
        }
    }
}
