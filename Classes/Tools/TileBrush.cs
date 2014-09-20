using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WorldEditor.Classes.History;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace WorldEditor.Classes.Tools
{
    public class TileBrush : TileTool
    {
        Rectangle brushRect = Rectangle.Empty;

        Point lastMouseMove;

        bool mouseDown = false,
             drawPreview = true;

        readonly Color tilesetTint = Color.White * 0.75f;
        readonly Color previewOverlayColor = new Color(47, 154, 255) * .25f;
        readonly Color collisionTileColor = new Color(200, 80, 80) * .45f;

        Texture2D colorTexture;

        public TileBrush(FormMain form) : base(form) 
        {
            RequiresDraw = true;
        }

        ~TileBrush()
        {
            colorTexture.Dispose();
        }

        public override void InitGraphics(GraphicsDevice graphicsDevice)
        {
            colorTexture = new Texture2D(graphicsDevice, 1, 1);
            colorTexture.SetData<Color>(new Color[] { Color.White });
        }

        public override void OnMouseEnter(EventArgs e)
        {
            drawPreview = true;

            // update the brush rectangle size if
            // any changes were made to the selection
            brushRect.Width = TilesetSelection.Width;
            brushRect.Height = TilesetSelection.Height;
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            if (TilesetSelection == Rectangle.Empty || World.IsDragging || e.Button != MouseButtons.Left)
                return;

            ChangesUndo = new TileChangesList(World.SelectedLayer);
            ChangesRedo = new TileChangesList(World.SelectedLayer);

            Point location = RelativePoint(e.Location);

            // draw a uniform line if shift is pressed when clicked
            if (lastRenderedPoint != PointNull && (Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                renderUniformLine(lastRenderedPoint, location);
            }
            else
            {
                renderPointCentered(location, true);
            }

            lastMouseMove = location;

            mouseDown = true;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (TilesetSelection == Rectangle.Empty)
                return;

            Point location = RelativePoint(e.Location);

            // don't need to do anything if the position hasn't changed
            if (lastMouseMove == location)
                return;

            // calculated the center offset of the selection.
            Point selectionOffset = new Point(
                (int)Math.Floor((float)(TilesetSelection.Width / TileSize.Width) / 2.0f),
                (int)Math.Floor((float)(TilesetSelection.Height / TileSize.Height) / 2.0f)
            );

            // set the brush preview location relative to the world
            brushRect.Location = location.Minus(selectionOffset).Minus(World.ViewportOffset).Multiplied(TileSize).Plus(World.ViewportPadding);

            // paint onto the world if the left button is down.
            // we render a line to ensure there are no
            // skips when moving the mouse quickly
            if (e.Button == MouseButtons.Left)
            {
                renderLine(lastMouseMove, location);
            }

            // redraw the canvas to show changes
            World.InvalidateCanvas();

            lastMouseMove = location;
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            // add changes to history assuming the conditions are right
            if (mouseDown && TilesetSelection != Rectangle.Empty && e.Button == MouseButtons.Left)
            {
                History.Add(PackagedHistoryItem());

                //clear temporary changes
                ChangesUndo = null;
                ChangesRedo = null;

                mouseDown = false;
            }
        }

        public override void OnMouseLeave(EventArgs e)
        {
            // hide the selection preview
            drawPreview = false;

            World.InvalidateCanvas();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // we don't want to draw if conditions aren't
            // right to do so
            if (!drawPreview || World.IsDragging)
                return;

            // draw a selection preview
            if (SelectedLayer == CollisionLayer)
            {
                spriteBatch.Draw(colorTexture, brushRect, collisionTileColor);

                return;
            }

            WorldTileset tileset = World.GetTileset(TilesetSelectionIndex);

            if (tileset == null)
                return;

            spriteBatch.Draw(tileset.Texture, brushRect, TilesetSelection, tilesetTint);
            spriteBatch.Draw(colorTexture, brushRect, previewOverlayColor);
        }

        public override void OnWorldChange()
        {
            // reset positions
            lastMouseMove = default(Point);
            lastRenderedPoint = PointNull;
        }

        void renderLine(Point p1, Point p2)
        {
            List<Point> points = pointsOnLine(p1, p2);

            // render all our points on the line
            for (int i = 0; i < points.Count; i++)
                renderPointCentered(points[i]);
        }

        void renderUniformLine(Point p1, Point p2)
        {
            List<Point> points = pointsOnLine(p1, p2);

            // offset to render points based on dominant direction
            int inc = Math.Abs(p2.X - p1.X) > Math.Abs(p2.Y - p1.Y) ?
                TilesetSelection.Width / TileSize.Width :
                TilesetSelection.Height / TileSize.Height,
                //determines our last point that is drawn
                last = ((points.Count - 1) / inc) * inc;

            // draw each point on the line with the offset
            // invalidates the canvas on the last point
            for (int i = 0; i <= last; i += inc)
                renderPointCentered(points[i], i == last);
        }

        List<Point> pointsOnLine(Point p1, Point p2)
        {
            int directionX = Math.Abs(p2.X - p1.X),
                directionY = Math.Abs(p2.Y - p1.Y),
                slopeX = p1.X < p2.X ? 1 : -1,
                slopeY = p1.Y < p2.Y ? 1 : -1,
                error = directionX - directionY;

            List<Point> points = new List<Point>();

            while (true)
            {
                // add point to points contained
                // in this line
                points.Add(p1);

                // reached the end of our line
                if (p1.X == p2.X && p1.Y == p2.Y)
                    break;

                int e2 = 2 * error;

                if (e2 > -directionY)
                {
                    error -= directionY;
                    p1.X += slopeX;
                }

                if (e2 < directionX)
                {
                    error += directionX;
                    p1.Y += slopeY;
                }
            }

            return points;
        }
    }
}

