using System.Drawing;
using WorldEditor.Classes.History;
using WorldEditor.Forms;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace WorldEditor.Classes
{
    public abstract class WorldAccessible
    {
        FormMain mainForm;

        protected FormWorld World
        {
            get { return (FormWorld)mainForm.ActiveMdiChild; }
        }

        protected WorldHistory History
        {
            get
            {
                return World == null ? null : World.History;
            }
        }

        protected Size CanvasSize
        {
            get
            {
                return World == null ? Size.Empty : World.Settings.CanvasSize;
            }
        }

        protected Size WorldSize
        {
            get
            {
                return World == null ? Size.Empty : World.Settings.WorldSize;
            }
        }

        protected Size TileSize
        {
            get
            {
                return World == null ? Size.Empty : World.Settings.TileSize;
            }
        }

        protected Rectangle TilesetSelection
        {
            get
            {
                return World == null ? Rectangle.Empty : World.TilesetSelectionRectangle;
            }
        }

        protected int TilesetSelectionIndex
        {
            get
            {
                return World == null ? -1 : World.SelectionTilesetIndex;
            }
        }

        protected int SelectedLayer
        {
            get
            {
                return World == null ? -1 : World.SelectedLayer;
            }
        }

        public WorldAccessible(FormMain main)
        {
            mainForm = main;
        }
    }
}
