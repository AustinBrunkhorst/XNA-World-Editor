using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace WorldEditor.Classes.History
{
    public class TileChangesList : Dictionary<Point, WorldTile> 
    {
        public int Layer;

        public TileChangesList(int layer)
        {
            Layer = layer;
        }
    }
}
