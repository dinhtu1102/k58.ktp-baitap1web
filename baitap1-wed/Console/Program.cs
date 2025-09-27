// Program.cs
using System;
using TuPuzzleLib;

class Program
{
    static void PrintBoard(int[] b, int rows, int cols)
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int v = b[r * cols + c];
                if (v == 0) Console.Write("  . ");
                else Console.Write(v.ToString().PadLeft(3) + " ");
            }
            Console.WriteLine();
        }
    }

    static string DirName(int d)
    {
        if (d == 0) return "Up";
        if (d == 1) return "Down";
        if (d == 2) return "Left";
        if (d == 3) return "Right";
        return "?";
    }

    static void Main()
    {
        SlidingPuzzle sp = new SlidingPuzzle(4, 4);
        Console.WriteLine("=== TuPuzzle Console demo ===");
        Console.WriteLine("Author: " + sp.AuthorSignature);
        Console.WriteLine("Nhập 16 số (0 là ô trống) cách nhau bởi dấu cách và Enter, hoặc nhấn Enter để xáo ngẫu nhiên:");
        string line = Console.ReadLine();
        if (line.Trim().Length == 0)
        {
            // personal shuffle
            Console.Write("Nhập tên để tạo 'dấu ấn' (Enter để mặc định): ");
            string name = Console.ReadLine();
            if (name.Trim().Length == 0) sp.Shuffle(40);
            else sp.SignatureShuffle(name);
        }
        else
        {
            string[] parts = line.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 16)
            {
                Console.WriteLine("Sai định dạng, sẽ xáo ngẫu nhiên.");
                sp.Shuffle(40);
            }
            else
            {
                int[] b = new int[16];
                for (int i = 0; i < 16; i++) b[i] = int.Parse(parts[i]);
                sp.Board = b;
            }
        }

        int[] current = sp.Board;
        PrintBoard(current, 4, 4);
        Console.WriteLine("Solvable? " + (sp.IsSolvable() ? "Yes" : "No"));
        if (!sp.IsSolvable()) { Console.WriteLine("Không thể giải, kết thúc."); return; }

        Console.WriteLine("Bắt đầu giải (với IDA*), tối đa 80 bước...");
        int[] solution;
        bool ok = sp.Solve(out solution, 80);
        if (ok)
        {
            Console.WriteLine("Tìm được giải, số bước: " + solution.Length);
            for (int i = 0; i < solution.Length; i++)
            {
                Console.Write((i + 1) + ":" + DirName(solution[i]) + " ");
                if ((i + 1) % 8 == 0) Console.WriteLine();
            }
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("Không tìm được giải trong giới hạn: " + sp.LastError);
        }
        Console.WriteLine("-- Kết thúc. Author: " + sp.AuthorSignature);
    }
}
