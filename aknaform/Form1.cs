using System.Diagnostics;

namespace aknaform
{
    public partial class Form1 : Form
    {
        public static int mapwidth = 1;
        public static int mapheight = 1;
        Aknakereso aknakereso;
        GameMenu menu;
        Font font;
        DateTime? lastClickTime = null;
        Graphics g;
        Bitmap render;
        float zoomRatio = 1f;
        Point zoomPosition = new Point(0, 0);
        public Form1()
        {
            InitializeComponent();

            FormBorderStyle = FormBorderStyle.None;
            Width = 451;
            Height = 451;
            FormBorderStyle = FormBorderStyle.Sizable;
            NewGame();
            DoubleBuffered = true;
            MouseWheel += Form1_MouseWheel;
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            const float zoomFactor = 0.1f; // Amount of zoom for each wheel tick
            if (e.Delta > 0) // Zoom in
            {
                scale += zoomFactor;
            }
            else if (e.Delta < 0 && scale > zoomFactor) // Zoom out
            {
                scale -= zoomFactor;
            }
            RecalculateSizes(true);
            Draw();
        }

        void NewGame()
        {
            menu ??= new GameMenu() { Left = Left + 20, Top = Top + 20 };
            menu.ShowDialog();

            if (menu.DialogResult == DialogResult.OK)
            {
                aknakereso = new Aknakereso();
                firstClick = true;
                mapwidth = menu.W;
                mapheight = menu.H;
            }
            else if (menu.DialogResult == DialogResult.Abort)
            {
                aknakereso = new Aknakereso();
                aknakereso.LoadGame();
                mapwidth = aknakereso.GetMapWidth;
                mapheight = aknakereso.GetMapHeight;
            }
            else if (menu.DialogResult == DialogResult.Ignore)
            {
                if (aknakereso != null)
                {
                    aknakereso.SaveGame();
                }
                else
                {
                    MessageBox.Show("Nincs aktív játék");
                }
            }
            RecalculateSizes();
            font = new Font(new FontFamily("Arial"), fontSize);
        }
        float fontSize;
        float cellWidth;
        float cellHeight;
        private void RecalculateSizes(bool zoomOnly = false)
        {
            if (ClientRectangle.Width == 0 || ClientRectangle.Height == 0)
                return;

            cellWidth = (float)ClientRectangle.Width / mapwidth;
            cellHeight = (float)ClientRectangle.Height / mapheight;

            AdjustScale();
            AdjustOffsets();

            render = new Bitmap(ClientSize.Width, ClientSize.Height);

            UpdateFontSize();
            font = new Font("Arial", fontSize);
            g = Graphics.FromImage(render);
            Draw();
        }

        private void AdjustScale()
        {
            float minScaleX = this.ClientSize.Width / (mapwidth * cellWidth);
            float minScaleY = this.ClientSize.Height / (mapheight * cellHeight);
            float minScale = Math.Max(minScaleX, minScaleY);

            if (scale < minScale)
                scale = minScale;
        }

        private void AdjustOffsets()
        {
            if (scale * mapwidth * cellWidth <= this.ClientSize.Width)
                offsetX = (this.ClientSize.Width - scale * mapwidth * cellWidth) / 2.0f;
            else
                offsetX = Clamp(offsetX, (this.ClientSize.Width - mapwidth * cellWidth * scale) / scale, 0);

            if (scale * mapheight * cellHeight <= this.ClientSize.Height)
                offsetY = (this.ClientSize.Height - scale * mapheight * cellHeight) / 2.0f;
            else
                offsetY = Clamp(offsetY, (this.ClientSize.Height - mapheight * cellHeight * scale) / scale, 0);
        }

        private float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private void UpdateFontSize()
        {
            if (ClientRectangle.Width > ClientRectangle.Height)
                fontSize = ((float)ClientRectangle.Height / mapheight) * 0.75f * scale;
            else
                fontSize = ((float)ClientRectangle.Width / mapwidth) * 0.75f * scale;
        }
        private float scale = 1.0f; // default scale
        private float offsetX = 0.0f, offsetY = 0.0f; // default offset values for panning

        void Draw()
        {
            if (aknakereso != null)
            {
                int startX = (int)Math.Max(0, -offsetX / cellWidth);
                int startY = (int)Math.Max(0, -offsetY / cellHeight);

                int endX = (int)Math.Min(mapwidth, startX + (this.ClientSize.Width / (cellWidth * scale)) + 1);
                int endY = (int)Math.Min(mapheight, startY + (this.ClientSize.Height / (cellHeight * scale)) + 1);

                for (int y = startY; y < endY; y++)
                {
                    for (int x = startX; x < endX; x++)
                    {
                        float adjustedX = (x * cellWidth + offsetX) * scale;
                        float adjustedY = (y * cellHeight + offsetY) * scale;

                        if (aknakereso.GetMaskValue(x, y) != -1 && !aknakereso.CheckLoss())
                        {
                            g.FillRectangle(Brushes.Gray, adjustedX, adjustedY, cellWidth * scale, cellHeight * scale);
                        }
                        else
                        {
                            g.FillRectangle(Brushes.White, adjustedX, adjustedY, cellWidth * scale, cellHeight * scale);
                        }
                        g.DrawRectangle(Pens.Black, adjustedX, adjustedY, cellWidth * scale, cellHeight * scale);

                        adjustedX += cellWidth * scale / 2;
                        adjustedY += cellHeight * scale / 2 + 1;

                        if (aknakereso.GetMaskValue(x, y) == -1 || aknakereso.CheckLoss())
                        {
                            int cellValue = aknakereso.GetCellValue(x, y);
                            if (cellValue > 0 && cellValue <= 9)
                            {
                                string text = cellValue != 9 ? cellValue.ToString() : "B";
                                Brush textBrush = cellValue switch
                                {
                                    1 => Brushes.Blue,
                                    2 => Brushes.Green,
                                    3 => Brushes.Red,
                                    4 => Brushes.Purple,
                                    5 => Brushes.Maroon,
                                    6 => Brushes.Turquoise,
                                    7 => Brushes.Black,
                                    8 => Brushes.Gray,
                                    9 => Brushes.Gray,
                                    _ => Brushes.Black
                                };

                                g.DrawString(text, font, textBrush, adjustedX, adjustedY, format);
                            }
                        }
                        else if (aknakereso.GetMaskValue(x, y) == 1)
                        {
                            g.DrawString("F", font, Brushes.Black, adjustedX, adjustedY, format);
                        }
                    }
                }
            }
            Invalidate();
        }

        bool firstClick = true;
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            Point ptc = new Point(e.X, e.Y);

            // Adjusting the position with the offset and scale
            ptc.X = (int)((ptc.X - offsetX * scale) / (cellWidth * scale));
            ptc.Y = (int)((ptc.Y - offsetY * scale) / (cellHeight * scale));

            Text = ptc.ToString();
        }
        static StringFormat format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(render, new RectangleF(0, 0, ClientSize.Width, ClientSize.Height), new RectangleF(zoomPosition.X, zoomPosition.Y, render.Width / zoomRatio, render.Height / zoomRatio), GraphicsUnit.Pixel);
        }
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {

            Point ptc = PointToClient(Cursor.Position);
            ptc.X = (int)(ptc.X / cellWidth);
            ptc.Y = (int)(ptc.Y / cellHeight);
            if (firstClick)
            {
                aknakereso.GenerateNewMap(menu.W, menu.H, menu.B, ptc.X, ptc.Y);
                firstClick = false;
            }
            if (e.Button == MouseButtons.Left)
            {
                aknakereso.ClickOnCell(ptc.X, ptc.Y);
                if (lastClickTime == null || (DateTime.Now - lastClickTime).Value.TotalMilliseconds >= 300)
                {
                    Debug.WriteLine("first click");
                    lastClickTime = DateTime.Now;
                }
                else
                {
                    Debug.WriteLine("second click");
                    if ((DateTime.Now - lastClickTime).Value.TotalMilliseconds < 300)
                    {
                        aknakereso.ClickOnCell(ptc.X, ptc.Y, true);
                    }
                    Debug.WriteLine("difference is: " + (DateTime.Now - lastClickTime).Value.TotalMilliseconds);
                    lastClickTime = null;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                aknakereso.FlagCell(ptc.X, ptc.Y);
            }
            Draw();
            if (aknakereso.CheckLoss())
            {
                Draw();
                Application.DoEvents();
                if (MessageBox.Show("Vesztettél!\nÚj játék?", "Game over", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    NewGame();
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            else if (aknakereso.CheckWin())
            {
                if (MessageBox.Show("Nyertél!\nÚj játék?", "Game over", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    NewGame();
                }
                else
                {
                    Environment.Exit(0);
                }
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            RecalculateSizes();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
                NewGame();

            float panAmount = 10.0f * scale;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    offsetY += panAmount;
                    break;
                case Keys.Down:
                    offsetY -= panAmount;
                    break;
                case Keys.Left:
                    offsetX += panAmount;
                    break;
                case Keys.Right:
                    offsetX -= panAmount;
                    break;
            }

            AdjustOffsets(); // Ensure offsets stay within bounds
            Draw();
        }
    }
}