
namespace WorldEditor.Classes
{
    public class WorldRegion
    {
        WorldTile[][] tiles;

        public int Width, Height;

        public WorldTile this[int x, int y] 
        {
            get { return tiles[x][y]; }
            set { tiles[x][y] = value; }
        }

        public WorldRegion(int width, int height)
        {
            tiles = new WorldTile[width][];

            for (int i = 0; i < width; i++)
                tiles[i] = new WorldTile[height];

            Width = width;
            Height = height;
        }
    }
}
