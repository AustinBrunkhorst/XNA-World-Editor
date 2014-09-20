using Microsoft.Xna.Framework;
using System;
using System.Windows.Forms;
using WorldEditor.Classes.History;

namespace WorldEditor.Classes.Tools
{
    /// <summary>
    /// Represents a generic tile tool
    /// </summary>
    public abstract class TileTool : WorldTool
    {
        static readonly protected Point PointNull = new Point(-1, -1);

        protected object ChangesUndo, ChangesRedo;

        protected Point lastRenderedPoint = PointNull;

        /// <summary>
        /// Represents a generic tile tool
        /// including basic tile operations
        /// </summary>
        /// <param name="main"> Main form </param>
        public TileTool(FormMain main) : base(main) { }

        /// <summary>
        /// Helper method for getting a tile from a selection offset.
        /// </summary>
        /// <param name="x"> x offset </param>
        /// <param name="y"> y offset </param>
        protected WorldTile TileFromSelectionOffset(int x, int y)
        {
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                return null;

            return new WorldTile(TilesetSelection.X + (x * TileSize.Width),
                TilesetSelection.Y + (y * TileSize.Height), TilesetSelectionIndex);
        }

        /// <summary>
        /// Renders the tileset selection to the world centered
        /// on given point
        /// </summary>
        /// <param name="point"> Point origin </param>
        /// <param name="invalidate"> Flag for invalidating the world canvas after done calculating changes </param>
        protected void renderPointCentered(Point point, bool invalidate = false)
        {
            //cast changes as changes list
            TileChangesList undo = ChangesUndo as TileChangesList,
                            redo = ChangesRedo as TileChangesList;

            int width = TilesetSelection.Width / TileSize.Width,
                height = TilesetSelection.Height / TileSize.Height,
                offsetX = (int)Math.Floor((float)width / 2),
                offsetY = (int)Math.Floor((float)height / 2);

            bool checkRectangle = World.SelectionRectangle != Rectangle.Empty;

            for (int x = point.X, sx = 0; x < point.X + width; x++, sx++)
            {
                for (int y = point.Y, sy = 0; y < point.Y + height; y++, sy++)
                {
                    Point change = new Point(x - offsetX, y - offsetY);

                    if (!World.PointInWorld(change.X, change.Y))
                        continue;

                    if (checkRectangle && !World.SelectionRectangle.Contains(change))
                        continue;

                    if (undo != null && !undo.ContainsKey(change))
                        undo[change] = World.GetTile(change);

                    WorldTile tile = TileFromSelectionOffset(sx, sy);

                    World.SetTile(change.X, change.Y, tile, World.SelectedLayer);

                    if(redo != null)
                        redo[change] = tile;
                }
            }

            lastRenderedPoint = point;

            if (invalidate)
                World.InvalidateCanvas();
        }

        /// <summary>
        /// Generic history callback delegate specifically for tile based tools
        /// </summary>
        /// <param name="obj"> History arguement</param>
        protected virtual void ApplyTileChanges(HistoryAction action, object obj)
        {
            lastRenderedPoint = PointNull;

            TileChangesList changes = (TileChangesList)obj;

            foreach (var change in changes)
                World.SetTile(change.Key.X, change.Key.Y, change.Value, changes.Layer);

            World.SelectLayer(changes.Layer);
        }

        /// <summary>
        /// Packages changes into a history item
        /// </summary>
        /// <returns> Packaged history item </returns>
        protected HistoryItem PackagedHistoryItem()
        {
            return new HistoryItem(ChangesUndo, ChangesRedo, ApplyTileChanges);
        }
    }
}
