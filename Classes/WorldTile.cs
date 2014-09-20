using Microsoft.Xna.Framework;
using System;
using Size = System.Drawing.Size;

namespace WorldEditor.Classes
{
    /// <summary>
    /// Represents a tile with texture offset and tileset
    /// </summary>
    [Serializable]
    public class WorldTile
    {
        public static WorldTile Collision = new WorldTile(0, 0, 0);

        /// <summary>
        /// Tileset texture offset (in tile units)
        /// </summary>
        public int X, Y;

        /// <summary>
        /// Tileset
        /// </summary>
        public int Tileset;

        public Point Offset
        {
            get { return new Point(X, Y); }
        }

        public WorldTile(int x, int y, int tileset)
        {
            X = x;
            Y = y;
            Tileset = tileset;
        }

        public Rectangle GetRectangleXNA(Size size)
        {
            return new Rectangle(X, Y, size.Width, size.Height);
        }

        public System.Drawing.Rectangle GetRectangleSystem(Size size)
        {
            return new System.Drawing.Rectangle(X, Y, size.Width, size.Height);
        }

        /// <summary>
        /// Gets a one dimensional tile offset
        /// </summary>
        /// <param name="tileSize"> Tile size </param>
        public int GetIndex(Size tileSize, Point tilesetSize)
        {
            int x = X / tileSize.Width,
                y = Y / tileSize.Height;

            return y * (tilesetSize.X / tileSize.Width) + x;
        }

        public WorldTile Clone()
        {
            return new WorldTile(X, Y, Tileset);
        }

        public static bool operator ==(WorldTile a, WorldTile b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            if((object)a != null && (object)b != null)
                return a.Tileset == b.Tileset && a.X == b.X && a.Y == b.Y;

            return (object)a == null && (object)b == null;
        }

        public static bool operator !=(WorldTile a, WorldTile b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
