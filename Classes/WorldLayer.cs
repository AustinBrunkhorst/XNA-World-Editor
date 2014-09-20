
namespace WorldEditor.Classes
{
    public class WorldLayer
    {
        public string Name;
        public bool Visible;

        WorldRegion tiles;

        public WorldTile this[int x, int y]
        {
            get { return tiles[x, y];  }
            set { tiles[x, y] = value; }
        }

        public WorldLayer(int width, int height, string name, bool visible = true) 
        {
            tiles = new WorldRegion(width, height);

            Name = name;
            Visible = visible;
        }
    }
}
