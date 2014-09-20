using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Windows.Forms;
using WorldEditor.Classes.History;

namespace WorldEditor.Classes.Tools
{
    class WorldSelection : WorldTool
    {
        Point lastDown, lastDownRelative, lastMove;

        Rectangle selection;
        Texture2D texture;

        bool mouseDown;

        public WorldSelection(FormMain main) : base(main) 
        {
            RequiresDraw = true;

            CanvasCursor = Cursors.Cross;
        }

        ~WorldSelection()
        {
            texture.Dispose();
        }

        public void ClearSelection()
        {
            mouseDown = false;

            selection = Rectangle.Empty;
        }

        public override void InitGraphics(GraphicsDevice graphicsDevice)
        {
            texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData<Color>(new Color[] { new Color(47, 154, 255) * 0.15f });
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            if (World.IsDragging || e.Button != MouseButtons.Left)
                return;

            Point relative = RelativePoint(e.Location),
                  location = relative.Minus(World.ViewportOffset).Multiplied(TileSize).Plus(World.ViewportPadding);

            lastDownRelative = relative;
            lastDown = location;
           
            selection.Location = location;
            selection.Width = selection.Height = 0;

            mouseDown = true;

            World.InvalidateCanvas();
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (!mouseDown || e.Button != MouseButtons.Left)
                return;

            Point location = RelativePoint(e.Location);

            if (location == lastMove)
                return;

            Point exact = location.Minus(World.ViewportOffset).Multiplied(TileSize).Plus(World.ViewportPadding);

            if (selection == Rectangle.Empty)
                lastDown = exact;

            selection = createSelectionRectangle(lastDown, exact);

            lastMove = location;

            World.InvalidateCanvas();
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (!mouseDown || e.Button != MouseButtons.Left)
                return;

            mouseDown = false;

            if (selection.Width == 0 || selection.Height == 0)
            {
                if (History.CanRedo) 
                {
                    HistoryItem next = History.NextRedo();

                    if (next.RedoArg is Rectangle && (Rectangle)next.RedoArg != Rectangle.Empty)
                    {
                        World.History.Add(new HistoryItem(World.SelectionRectangle, Rectangle.Empty, applySelectionChange));
                    }
                }

                World.SelectionRectangle = Rectangle.Empty;

                World.InvalidateCanvas();

                return;
            }

            Rectangle relative = createSelectionRectangle(lastDownRelative, RelativePoint(e.Location), true),
                      selectionUndo = World.SelectionRectangle;
            
            World.SelectionRectangle = relative;

            selection = Rectangle.Empty;

            World.History.Add(new HistoryItem(selectionUndo, relative, applySelectionChange));

            World.InvalidateCanvas();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, selection, Color.White);
        }

        void applySelectionChange(HistoryAction action, object rectangle)
        {
            World.SelectionRectangle = (Rectangle)rectangle;
            World.InvalidateCanvas();
        }

        Rectangle createSelectionRectangle(Point p1, Point p2, bool relative = false)
        {
            Rectangle select = Rectangle.Empty;

            p2.X = Math.Max(0, p2.X);
            p2.Y = Math.Max(0, p2.Y);

            if (p1.X < p2.X)
            {
                select.X = p1.X;
                select.Width = p2.X - p1.X;
            }
            else
            {
                select.X = p2.X;
                select.Width = p1.X - p2.X;
            }

            if (p1.Y < p2.Y)
            {
                select.Y = p1.Y;
                select.Height = p2.Y - p1.Y;
            }
            else
            {
                select.Y = p2.Y;
                select.Height = p1.Y - p2.Y;
            }

            if (relative)
            {
                select.Width++;
                select.Height++;
            }
            else
            {
                select.Width += TileSize.Width;
                select.Height += TileSize.Height;
            }

            return select;
        }
    }
}
