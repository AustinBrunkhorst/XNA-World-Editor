using System;
using System.Drawing;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace WorldEditor.Classes
{
    //TODO: organize this crap
    public static class MathHelper
    {
        public static float Nearest(this float number, float near, bool ceil = false)
        {
            float div = (float)number / near;

            return ceil ? (float)Math.Ceiling(div) : (float)Math.Floor(div) * near;
        }

        public static int Nearest(this int number, int near, bool ceil = false)
        {
            float div = (float)number / near;

            return (int)(ceil ? Math.Ceiling(div) : Math.Floor(div)) * near;
        }

        public static Point Nearest(this Point point, Size near, bool ceil = false)
        {
            return new Point(point.X.Nearest(near.Width, ceil), point.Y.Nearest(near.Height, ceil));
        }

        public static System.Drawing.Point Nearest(this System.Drawing.Point point, Size near, bool ceil = false)
        {
            return new System.Drawing.Point(point.X.Nearest(near.Width, ceil), point.Y.Nearest(near.Height, ceil));
        }

        public static Size Nearest(this Size size, Size near, bool ceil = false)
        {
            return new Size(size.Width.Nearest(near.Width, ceil), size.Height.Nearest(near.Height, ceil));
        }

        public static float Clamp(this float number, float min, float max)
        {
            return (number < min) ? min : (number > max) ? max : number;
        }

        public static int Clamp(this int number, int min, int max)
        {
            return (number < min) ? min : (number > max) ? max : number;
        }

        public static Point Plus(this Point location, Point size)
        {
            return new Point(location.X + size.X, location.Y + size.Y);
        }

        public static Point Minus(this Point location, Point size)
        {
            return new Point(location.X - size.X, location.Y - size.Y);
        }

        public static Point Multiplied(this Point location, Size scale)
        {
            return new Point(location.X * scale.Width, location.Y * scale.Height);
        }

        public static Point Multiplied(this Point location, float scale)
        {
            return new Point((int)Math.Round(location.X * scale), (int)Math.Round(location.Y * scale));
        }

        public static Point Divided(this Point location, Size size)
        {
            return new Point(location.X / size.Width, location.Y / size.Height);
        }

        public static Point Divided(this Point location, float size)
        {
            return new Point((int)Math.Round(location.X / size), (int)Math.Round(location.Y / size));
        }

        public static Size Plus(this Size size, Size add)
        {
            return new Size(size.Width + add.Width, size.Height + add.Height);
        }

        public static Size Minus(this Size size, Size subtract)
        {
            return new Size(size.Width - subtract.Width, size.Height - subtract.Height);
        }

        public static Size Multiplied(this Size size, Size scale)
        {
            return new Size(size.Width * scale.Width, size.Height * scale.Height);
        }

        public static Size Multiplied(this Size size, int scale)
        {
            return new Size(size.Width * scale, size.Height * scale);
        }

        public static Size Multiplied(this Size size, float scale)
        {
            return new Size((int)Math.Round(size.Width * scale), (int)Math.Round(size.Height * scale));
        }

        public static Size Divided(this Size size, Size scale)
        {
            return new Size(size.Width / scale.Width, size.Height / scale.Height);
        }

        public static Size Divided(this Size size, int scale)
        {
            return new Size(size.Width / scale, size.Height / scale);
        }

        public static Size Divided(this Size size, float scale)
        {
            return new Size((int)Math.Round(size.Width / scale), (int)Math.Round(size.Height / scale));
        }

        public static Rectangle Inflated(this Rectangle rect, Size size)
        {
            Rectangle copy = rect;

            copy.Inflate(size.Width, size.Height);

            return copy;
        }

        public static Rectangle Inflated(this Rectangle rect, int width, int height)
        {
            Rectangle copy = rect;

            copy.Inflate(width, height);

            return copy;
        }

        public static Rectangle Deflated(this Rectangle rect, Size size)
        {
            Rectangle copy = rect;

            copy.Inflate(-size.Width, -size.Height);

            return copy;
        }

        public static Rectangle Deflated(this Rectangle rect, int width, int height)
        {
            Rectangle copy = rect;

            copy.Inflate(-width, -height);

            return copy;
        }
    }
}
