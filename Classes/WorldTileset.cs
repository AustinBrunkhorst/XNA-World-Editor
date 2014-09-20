using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace WorldEditor.Classes
{
    public class WorldTileset
    {
        Bitmap image;

        /// <summary>
        /// GDI Image
        /// </summary>
        public Bitmap Image
        {
            get { return image; }
        }

        Texture2D texture;

        /// <summary>
        /// XNA Texture
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
        }

        /// <summary>
        /// Image bounds
        /// </summary>
        public System.Drawing.Rectangle Bounds
        {
            get { return new System.Drawing.Rectangle(0, 0, image.Width, image.Height); }
        }

        string filename;

        /// <summary>
        /// Original image location
        /// </summary>
        public string Filename
        {
            get { return filename; }
        }

        string buildLocation;

        /// <summary>
        /// Asset location for build in XNA
        /// </summary>
        public string BuildLocation
        {
            get { return buildLocation; }
            set { buildLocation = value; }
        }

        bool shouldBuild = true;

        /// <summary>
        /// Should we include the tileset filename in the output file
        /// </summary>
        public bool ShouldBuild
        {
            get { return shouldBuild; }
            set { shouldBuild = value; }
        }

        public WorldTileset(string filename, GraphicsDevice graphicsDevice)
        {
            this.filename = filename;

            // load the image
            using(FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                image = new Bitmap(fileStream);

            // convert to PNG for use with XNA's textures
            using (MemoryStream textureStream = new MemoryStream())
            {
                image.Save(textureStream, ImageFormat.Png);

                texture = Texture2D.FromStream(graphicsDevice, textureStream, image.Width, image.Height, false);
            }
        }
    }
}
