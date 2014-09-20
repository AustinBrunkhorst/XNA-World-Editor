using DevComponents.DotNetBar;
using Microsoft.Xna.Framework;
using System;
using System.Windows.Forms;
using WorldEditor.Classes;
using WorldEditor.Forms;
using Point = System.Drawing.Point;

namespace WorldEditor.Controls
{
    public partial class TilesetsPanel : UserControl
    {
        FormWorld world;
        TilesetSelector selector;

        public FormWorld SelectedWorld
        {
            get { return world; }
            set
            {
                world = value;
                onWorldChange();
            }
        }

        public TilesetsPanel()
        {
            InitializeComponent();

            Dock = DockStyle.Fill;

            selector = new TilesetSelector();
            selector.ContextMenuStrip = tilesetMenu;

            tilesetContainer.Controls.Add(selector);
        }

        /// <summary>
        /// Redraws the selector
        /// </summary>
        public void InvalidateSelector()
        {
            selector.Invalidate();
        }

        void btnAddTileset_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Add Tileset";
                dialog.Filter = "Images|*.png;*.jpg;*.jpeg;*.gif;*.bmp";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        foreach (WorldTileset t in world.Tilesets)
                        {
                            if (dialog.FileName == t.Filename)
                            {
                                MessageBox.Show("The following tileset already exists\n\n" + dialog.FileName, 
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        world.AddTileset(new WorldTileset(dialog.FileName, world.CanvasGraphicsDevice));

                        ButtonItem item = new ButtonItem(String.Empty, dialog.SafeFileName);

                        tilesetsList.Items.Add(item);
                        tilesetsList.SelectedItem = item;
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message, "Error Loading Tileset", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        void refreshTileset(object sender, EventArgs e)
        {
            try
            {
                int index = tilesetsList.SelectedIndex;

                WorldTileset refresh = new WorldTileset(world.Tilesets[index].Filename,
                    world.CanvasGraphicsDevice);

                world.Tilesets[index] = refresh;
                world.InvalidateCanvas();
                world.UpdateTilesetIndexes();

                selector.Tileset = refresh.Image;
                selector.Selection = Rectangle.Empty;
                selector.Invalidate();
            }
            catch (Exception exception)
            {
                MessageBox.Show("An error occured while refreshing the tileset\n" + exception.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void removeTileset(object sender, EventArgs e)
        {
            if (MessageBox.Show("All tiles referencing of this tileset in the world will be reset.",
                "Remove Tileset",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning) == DialogResult.OK)
            {
                world.RemoveTileset(tilesetsList.SelectedIndex);
                tilesetsList.Items.RemoveAt(tilesetsList.SelectedIndex);
                tilesetsList_SelectedIndexChanged(null, null);
            }

        }

        void onWorldChange()
        {
            tilesetsList.Items.Clear();

            if (world == null)
            {
                btnAddTileset.Enabled = false;
                selector.Tileset = null;
                return;
            }

            btnAddTileset.Enabled = true;

            Rectangle selection = world.TilesetSelectionRectangle;

            foreach (WorldTileset tileset in world.Tilesets)
            {
                tilesetsList.Items.Add(new ButtonItem(String.Empty, System.IO.Path.GetFileName(tileset.Filename)));
            }

            if (world.Tilesets.Count > 0)
            {
                int selectedTileset = world.SelectionTilesetIndex;

                tilesetsList.SelectedIndex = selectedTileset;
                selector.Tileset = world.GetTileset(selectedTileset).Image;
                selector.Selection = selection;

                tilesetContainer.AutoScrollPosition = new Point(selection.X - ((tilesetContainer.Width - selection.Width) / 2), selection.Y - ((tilesetContainer.Height - selection.Height) / 2));
            }
            else
            {
                selector.Tileset = null;
                selector.Selection = Rectangle.Empty;
                tilesetContainer.AutoScrollPosition = Point.Empty;
            }

            selector.UpdateGrid();
        }

        void tilesetsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = tilesetsList.SelectedIndex;

            selector.Tileset = index == -1 ? null : world.GetTileset(index).Image;
            selector.Selection = Rectangle.Empty;
            world.SelectionTilesetIndex = index;
        }
    }
}
