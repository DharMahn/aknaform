using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aknaform
{
    internal class Zoomer
    {
        Rectangle zoomRect = new Rectangle(0, 0, 20, 20);
        public Rectangle ZoomRect { get { return zoomRect; } }
        Rectangle zoomBounds = new Rectangle(750, 0, 500, 500);
        public Rectangle ZoomBounds { get { return zoomBounds; } set { zoomBounds = value; } }
        Bitmap bmp = new Bitmap(100, 100);
        public Bitmap Bmp { get { return bmp; } set { bmp = value; } }
        float scale = 100f;
        float targetscale = 100f;
        public float Scale { get { return scale; } set { targetscale = value; scale = value; } }
        float minScale = 5f;
        System.Windows.Forms.Timer timer;
        Point zoomLocation = new Point();
        private bool instant;
        public Zoomer(bool instant = false)
        {
            bmp = new Bitmap(100, 100);
            this.instant = instant;
            if (instant)
            {
                timer = new System.Windows.Forms.Timer();
                timer.Tick += timer_Tick;
                timer.Interval = 16;
                timer.Start();
            }
        }
        float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }
        private void timer_Tick(object? sender, EventArgs e)
        {
            if (Math.Abs(scale - targetscale) > 0.01)
            {
                scale = Lerp(scale, TargetScale, 0.2f);
                Zoom(zoomLocation);
            }
            else
            {
                scale = targetscale;
            }
        }

        public float TargetScale
        {
            get
            {
                return targetscale;
            }
            set
            {
                targetscale = value < minScale ? minScale : value;
                if (instant)
                {
                    scale = value;
                }
            }
        }

        public void DrawZoomedRegion(Graphics g)
        {
            g.DrawImage(bmp, zoomBounds, zoomRect, GraphicsUnit.Pixel);
        }

        public void Zoom(Point zoom)
        {
            zoomLocation = zoom;
            int thing = (int)scale;
            zoomRect.X = zoomLocation.X - thing;
            zoomRect.Y = zoomLocation.Y - thing;
            zoomRect.Width = thing * 2;
            zoomRect.Height = thing * 2;
            if (zoomRect.Height > bmp.Height)
            {
                zoomRect.Height = bmp.Height;
                zoomRect.Width = bmp.Height;
                zoomRect.X = zoomLocation.X - bmp.Height / 2;
                zoomRect.Y = 0;
                TargetScale = bmp.Height / 2f;
            }
            if (zoomRect.Width > bmp.Width)
            {
                zoomRect.X = 0;
                zoomRect.Y = zoomLocation.Y - bmp.Width / 2;
                zoomRect.Width = bmp.Width;
                zoomRect.Height = bmp.Width;
                TargetScale = bmp.Width / 2f;
            }
            if (zoomRect.X < 0)
            {
                zoomRect.X = 0;
            }
            if (zoomRect.Y < 0)
            {
                zoomRect.Y = 0;
            }
            if (zoomRect.X + zoomRect.Width > bmp.Width)
            {
                zoomRect.X = bmp.Width - zoomRect.Width;
            }
            if (zoomRect.Y + zoomRect.Height > bmp.Height)
            {
                zoomRect.Y = bmp.Height - zoomRect.Height;
            }
        }
    }
}
