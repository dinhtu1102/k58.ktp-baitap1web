using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Puzzle15
{
    public partial class Form1 : Form
    {
        private Button[,] tiles = new Button[4, 4];
        // tileIndex holds which piece (1..15) is at position (r,c). 0 = empty.
        private int[,] tileIndex = new int[4, 4];
        private Image[] flatImages = new Image[16]; // 1..15 used, 0 = null
        private int emptyR = 3, emptyC = 3;
        private Random rnd = new Random();

        public Form1()
        {
            InitializeComponent();

            // wire up buttons from designer
            this.btnShuffle.Click += new EventHandler(btnShuffle_Click);
            this.btnLoadImage.Click += new EventHandler(btnLoadImage_Click);

            CreateTiles();
            InitSolvedState();
            // try auto-load puzzle.jpg in exe folder if present
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string tryPath = Path.Combine(exeDir, "puzzle.jpg");
            if (File.Exists(tryPath))
            {
                LoadAndSliceImage(tryPath);
            }
            Render();
        }

        // create 4x4 Button controls inside pnlBoard
        private void CreateTiles()
        {
            int w = this.pnlBoard.Width;
            int h = this.pnlBoard.Height;
            int cellW = w / 4;
            int cellH = h / 4;

            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    Button b = new Button();
                    b.Size = new Size(cellW - 4, cellH - 4); // small spacing
                    b.Location = new Point(c * cellW + 2, r * cellH + 2);
                    b.Tag = new Point(r, c);
                    b.BackColor = Color.LightGray;
                    b.FlatStyle = FlatStyle.Flat;
                    b.Click += new EventHandler(Tile_Click);
                    b.BackgroundImageLayout = ImageLayout.Stretch;
                    // remove any text initially
                    b.Text = "";
                    tiles[r, c] = b;
                    this.pnlBoard.Controls.Add(b);
                }
            }
        }

        // initialize tileIndex to solved state (1..15, 0 empty)
        private void InitSolvedState()
        {
            int v = 1;
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (r == 3 && c == 3)
                    {
                        tileIndex[r, c] = 0;
                        emptyR = r;
                        emptyC = c;
                    }
                    else
                    {
                        tileIndex[r, c] = v;
                        v++;
                    }
                }
            }
            // free any previous images
            for (int i = 0; i < flatImages.Length; i++)
            {
                if (flatImages[i] != null)
                {
                    try { flatImages[i].Dispose(); } catch { }
                    flatImages[i] = null;
                }
            }
        }

        // Render tiles: show image if exists, otherwise blank (empty)
        private void Render()
        {
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    Button b = tiles[r, c];
                    int idx = tileIndex[r, c];
                    if (idx == 0)
                    {
                        b.BackgroundImage = null;
                        b.Text = "";
                        b.BackColor = Color.DarkSlateGray; // empty look
                    }
                    else
                    {
                        if (flatImages[idx] != null)
                        {
                            b.BackgroundImage = flatImages[idx];
                            b.Text = "";
                        }
                        else
                        {
                            // fallback: show number if no image loaded
                            b.BackgroundImage = null;
                            b.Text = idx.ToString();
                            b.Font = new Font("Arial", 20, FontStyle.Bold);
                            b.BackColor = Color.LightBlue;
                        }
                    }
                }
            }
        }

        // When user clicks a tile -> try move if adjacent to empty
        private void Tile_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (b == null) return;
            Point p = (Point)b.Tag;
            int r = p.X;
            int c = p.Y;

            if ((Math.Abs(r - emptyR) == 1 && c == emptyC) || (Math.Abs(c - emptyC) == 1 && r == emptyR))
            {
                // swap indices
                tileIndex[emptyR, emptyC] = tileIndex[r, c];
                tileIndex[r, c] = 0;
                emptyR = r;
                emptyC = c;
                Render();

                if (IsSolved())
                {
                    MessageBox.Show("Chúc mừng! Bạn đã hoàn thành!", "Win");
                }
            }
        }

        // Shuffle by making legal random moves from solved position (ensures solvable)
        private void btnShuffle_Click(object sender, EventArgs e)
        {
            ShuffleRandom(200);
            Render();
        }

        // Load image file and slice into 15 pieces (16th empty) — caller ensures path exists
        private void LoadAndSliceImage(string path)
        {
            if (!File.Exists(path)) return;

            Bitmap src = null;
            try
            {
                // load image safely
                src = new Bitmap(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không mở được ảnh: " + ex.Message);
                return;
            }

            // create square scaled bitmap of panel size to crop evenly
            int target = this.pnlBoard.Width; // assume square
            Bitmap scaled = new Bitmap(target, target);
            using (Graphics g = Graphics.FromImage(scaled))
            {
                g.Clear(Color.White);
                // draw src to fit (keep aspect ratio, center)
                float sx = (float)target / src.Width;
                float sy = (float)target / src.Height;
                float s = (sx < sy) ? sx : sy;
                int newW = (int)(src.Width * s);
                int newH = (int)(src.Height * s);
                int offX = (target - newW) / 2;
                int offY = (target - newH) / 2;
                g.DrawImage(src, new Rectangle(offX, offY, newW, newH));
            }

            // free previous images if any
            for (int i = 0; i < flatImages.Length; i++)
            {
                if (flatImages[i] != null)
                {
                    try { flatImages[i].Dispose(); } catch { }
                    flatImages[i] = null;
                }
            }

            int tileW = target / 4;
            int tileH = target / 4;
            int index = 1;
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (r == 3 && c == 3)
                    {
                        flatImages[0] = null; // empty
                        tileIndex[r, c] = 0;
                    }
                    else
                    {
                        Rectangle srcRect = new Rectangle(c * tileW, r * tileH, tileW, tileH);
                        Bitmap piece = new Bitmap(tileW, tileH);
                        using (Graphics g = Graphics.FromImage(piece))
                        {
                            g.DrawImage(scaled, new Rectangle(0, 0, tileW, tileH), srcRect, GraphicsUnit.Pixel);
                        }
                        flatImages[index] = piece;
                        // place solved order image in tileIndex (keep solved arrangement)
                        // find where that image belongs: we store flatImages index mapping to solved order
                        // tileIndex initially is solved; we only update images displayed via flatImages
                        index++;
                    }
                }
            }

            // always start in solved layout, then shuffle
            InitSolvedState(); // reinit tileIndex to solved positions
            // set empty pos
            emptyR = 3;
            emptyC = 3;
            // dispose source bitmaps
            try { src.Dispose(); } catch { }
            try { scaled.Dispose(); } catch { }
        }

        // Button Load Image click -> open file dialog
        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Choose puzzle image";
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadAndSliceImage(ofd.FileName);
                ShuffleRandom(150);
                Render();
            }
        }

        // Random legal move shuffles
        private void ShuffleRandom(int moves)
        {
            for (int i = 0; i < moves; i++)
            {
                // collect neighbors
                int[] nr = new int[4];
                int[] nc = new int[4];
                int count = 0;
                // up
                if (emptyR - 1 >= 0) { nr[count] = emptyR - 1; nc[count] = emptyC; count++; }
                // down
                if (emptyR + 1 < 4) { nr[count] = emptyR + 1; nc[count] = emptyC; count++; }
                // left
                if (emptyC - 1 >= 0) { nr[count] = emptyR; nc[count] = emptyC - 1; count++; }
                // right
                if (emptyC + 1 < 4) { nr[count] = emptyR; nc[count] = emptyC + 1; count++; }

                if (count == 0) continue;
                int pick = rnd.Next(count);
                int r = nr[pick];
                int c = nc[pick];

                // swap indices
                tileIndex[emptyR, emptyC] = tileIndex[r, c];
                tileIndex[r, c] = 0;
                emptyR = r;
                emptyC = c;
            }
        }

        // Check win: tileIndex in order 1..15 then 0
        private bool IsSolved()
        {
            int expected = 1;
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (r == 3 && c == 3)
                    {
                        if (tileIndex[r, c] != 0) return false;
                    }
                    else
                    {
                        if (tileIndex[r, c] != expected) return false;
                        expected++;
                    }
                }
            }
            return true;
        }
    }
}
