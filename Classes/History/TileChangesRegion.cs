using Microsoft.Xna.Framework;

namespace WorldEditor.Classes.History
{
    public class TileChangesRegion
    {
        public WorldRegion Region;
        public Point Offset;
        public int Layer;

        public TileChangesRegion(WorldRegion region, Point offset, int layer)
        {
            Region = region;
            Offset = offset;
            Layer = layer;
        }
    }
}
