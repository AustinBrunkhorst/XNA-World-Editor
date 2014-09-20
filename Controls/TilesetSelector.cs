using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using WorldEditor.Classes;
using WorldEditor.Forms;

namespace WorldEditor.Controls
{
    public partial class TilesetSelector : UserControl
    {
        Bitmap tileset, grid;
        Rectangle selection;
        Color selectionColor = Color.FromArgb(47, 154, 255);

        const long SCROLL_TIMEOUT = 25;
        long lastResizeScroll;

        Point lastMouseDown, lastSelectionPosition;
        Stopwatch scrollWatch;

        public Bitmap Tileset
        {
            get { return tileset; }
            set
            {
                tileset = value;

                Size = value == null ? Size.Empty : value.Size.Nearest(world.Settings.TileSize);

                Refresh();
            }
        }

        public Microsoft.Xna.Framework.Rectangle Selection
        {
            get { return new Microsoft.Xna.Framework.Rectangle(selection.X, selection.Y, selection.Width, selection.Height); }
            set 
            {
                selection = new Rectangle(value.X, value.Y, value.Width, value.Height);
                world.TilesetSelectionRectangle = value;

                Refresh();
            }
        }

        TilesetsPanel parent
        {
            get { return (TilesetsPanel)Parent.Parent; }
        }

        FormWorld world
        {
            get { return parent.SelectedWorld; }
        }

        public TilesetSelector()
        {
            DoubleBuffered = true;

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);

            Size = Size.Empty;
            selection = Rectangle.Empty;

            scrollWatch = new Stopwatch();
        }

        /// <summary>
        /// Updates the tile grid
        /// </summary>
        public void UpdateGrid()
        {
            grid = new Bitmap(world.Settings.TileSize.Width, world.Settings.TileSize.Height);

            using (Graphics graphics = Graphics.FromImage(grid))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb((int)(255 * 0.90f), Color.Black)))
            {
                graphics.DrawRectangle(new Pen(brush), 0, 0, world.Settings.TileSize.Width, world.Settings.TileSize.Height);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                scrollWatch.Start();

                Point point = new Point(e.X, e.Y).Nearest(world.Settings.TileSize);

                lastMouseDown = point;
                selection.Location = point;
                selection.Size = world.Settings.TileSize;

                Refresh();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Rectangle select = createSelectionRectangle(lastMouseDown, e.Location);

                if (select == selection)
                    return;

                selection = select;

                Panel container = (Panel)Parent;

                Point newScroll = new Point(-container.AutoScrollPosition.X, -container.AutoScrollPosition.Y);

                long now = scrollWatch.ElapsedMilliseconds;

                if (now - lastResizeScroll > SCROLL_TIMEOUT)
                {
                    int s = 0;

                    if (selection.X < lastSelectionPosition.X  && selection.X < newScroll.X)
                    {
                        newScroll.X = selection.X;
                        s++;
                    }
                    else if (selection.Right > newScroll.X + container.ClientSize.Width)
                    {
                        newScroll.X = selection.Right - container.ClientSize.Width;
                        s++;
                    }

                    if (selection.Y < lastSelectionPosition.Y && selection.Y < newScroll.Y)
                    {
                        newScroll.Y = selection.Y;
                        s++;
                    }
                    else if (selection.Bottom > newScroll.Y + container.ClientSize.Height)
                    {
                        newScroll.Y = selection.Bottom - container.ClientSize.Height;
                        s++;
                    }

                    if (s > 0)
                        lastResizeScroll = now;
                }

                lastSelectionPosition = selection.Location;
                container.AutoScrollPosition = newScroll;

                Refresh();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                world.TilesetSelectionRectangle = new Microsoft.Xna.Framework.Rectangle(selection.X, selection.Y, selection.Width, selection.Height);

                scrollWatch.Stop();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.White, e.ClipRectangle);

            if (tileset == null)
                return;

            e.Graphics.DrawImage(tileset, 0, 0, tileset.Width, tileset.Height);

            // draw tile grid if enabled
            if (Properties.Settings.Default.DrawGrid)
            {
                using (TextureBrush brush = new TextureBrush(grid))
                    e.Graphics.FillRectangle(brush, e.ClipRectangle);
            }

            // draw the selection
            using (SolidBrush selectionFill = new SolidBrush(Color.FromArgb(128, selectionColor)))
            {
                using (Pen borderPen = new Pen(selectionColor))
                {
                    e.Graphics.FillRectangle(selectionFill, selection);
                    e.Graphics.DrawRectangle(borderPen, selection);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        Rectangle createSelectionRectangle(Point p1, Point p2)
        {
            Rectangle select = Rectangle.Empty;

            p2.X = p2.X.Clamp(0, Width);
            p2.Y = p2.Y.Clamp(0, Height);

            if (p1.X < p2.X)
            {
                select.X = p1.X;
                select.Width = p2.X - p1.X;
            }
            else
            {
                select.X = p2.X;
                select.Width = (p1.X - p2.X) + world.Settings.TileSize.Width;
            }

            if (p1.Y < p2.Y)
            {
                select.Y = p1.Y;
                select.Height = p2.Y - p1.Y;
            }
            else
            {
                select.Y = p2.Y;
                select.Height = (p1.Y - p2.Y) + world.Settings.TileSize.Height;
            }

            select.Location = select.Location.Nearest(world.Settings.TileSize);
            select.Size = select.Size.Nearest(world.Settings.TileSize, true);

            return select;
        }
    }
}
