using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Windows.Forms;

namespace WorldEditor.Classes
{
    public abstract class WorldTool : WorldAccessible
    {
        public bool RequiresDraw = false;

        public Cursor CanvasCursor = Cursors.Default;

        protected int CollisionLayer
        {
            get { return World.LayerCount - 1; }
        }

        public WorldTool(FormMain main) : base(main) { }

        public virtual void OnMouseEnter(EventArgs e) { }
        public virtual void OnMouseDown(MouseEventArgs e) { }
        public virtual void OnMouseMove(MouseEventArgs e) { }
        public virtual void OnMouseUp(MouseEventArgs e) { }
        public virtual void OnMouseLeave(EventArgs e) { }
        public virtual void InitGraphics(GraphicsDevice graphicsDevice) { }
        public virtual void Draw(SpriteBatch spriteBatch) { }
        public virtual void OnWorldChange() { }

        /// <summary>
        /// Get a point in world from a mouse location measured in tiles
        /// </summary>
        /// <param name="location"> Mouse Location </param>
        /// <returns> Relative tile point </returns>
        protected Point RelativePoint(System.Drawing.Point location)
        {
            Point offset = World.ViewportPadding;

            if (CanvasSize.Width < World.CanvasWidth)
                offset.X = 0;
            if (CanvasSize.Height < World.CanvasHeight)
                offset.Y = 0;

            return new Point(World.ViewportPosition.X + location.X + offset.X, World.ViewportPosition.Y + location.Y + offset.Y).Nearest(TileSize).Divided(TileSize);
        }
    }
}
