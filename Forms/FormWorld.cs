using DevComponents.DotNetBar;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using WorldEditor.Classes;
using WorldEditor.Classes.History;
using WorldEditor.Controls;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace WorldEditor.Forms
{
    public partial class FormWorld : Office2007Form
    {
        WorldCanvas worldCanvas;
        List<WorldLayer> layers;

        WorldHistory history;

        /// <summary>
        /// World's change history
        /// </summary>
        public WorldHistory History
        {
            get { return history; }
            set { history = value; }
        }

        WorldSettings settings;

        /// <summary>
        /// World's settings
        /// </summary>
        public WorldSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        Dictionary<string, string> userProperties;

        /// <summary>
        /// World user defined properties
        /// </summary>
        public Dictionary<string, string> UserProperties
        {
            get { return userProperties; }
            set { userProperties = value; }
        }

        string saveLocation;

        public string SaveLocation
        {
            get { return saveLocation; }
            set
            {
                saveLocation = value;

                Text = Path.GetFileName(value);
            }
        }

        int selectedLayer;

        /// <summary>
        /// Selected tile layer
        /// </summary>
        public int SelectedLayer
        {
            get { return selectedLayer; }
            set { selectedLayer = value; }
        }

        int selectionTilesetIndex;

        /// <summary>
        /// Tileset index for the current selection
        /// </summary>
        public int SelectionTilesetIndex
        {
            get { return selectionTilesetIndex; }
            set { selectionTilesetIndex = value; }
        }

        int[] tilesetIndexes;

        /// <summary>
        /// Array of max indexes for each tileset
        /// </summary>
        public int[] TilesetIndexes
        {
            get { return tilesetIndexes; }
            set { tilesetIndexes = value; }
        }

        public int LayerCount
        {
            get { return layers.Count; }
        }

        Rectangle tilesetSelectionRectangle; 

        /// <summary>
        /// Tileset selection rectangle
        /// </summary>
        public Rectangle TilesetSelectionRectangle
        {
            get { return tilesetSelectionRectangle; }
            set { tilesetSelectionRectangle = value; }
        }

        Rectangle selectionRectangle;

        /// <summary>
        /// World selection rectangle
        /// </summary>
        public Rectangle SelectionRectangle
        {
            get { return selectionRectangle; }
            set 
            { 
                selectionRectangle = value;
                worldCanvas.SelectionRectangle = value;
            }
        }

        /// <summary>
        /// Gets the current tool using in the main
        /// form
        /// </summary>
        public WorldTool SelectedTool
        {
            get { return ((FormMain)MdiParent).SelectedTool; }
        }


        List<WorldTileset> tilesets;

        /// <summary>
        /// Tilesets used in this world
        /// </summary>
        public List<WorldTileset> Tilesets
        {
            get { return tilesets; }
        }

        /// <summary>
        /// Graphics device used for the canvas control
        /// </summary>
        public GraphicsDevice CanvasGraphicsDevice
        {
            get { return worldCanvas.GDevice; }
        }

        /// <summary>
        /// Combination of viewport padding and offset
        /// </summary>
        public Point ViewportPosition
        {
            get { return worldCanvas.ViewportPosition; }
        }

        /// <summary>
        /// Scroll offset in the world
        /// </summary>
        public Point ViewportOffset
        {
            get { return worldCanvas.ViewportOffset; }
        }

        /// <summary>
        /// Padding around world if the world size
        /// is smaller than visible viewport
        /// </summary>
        public Point ViewportPadding
        {
            get { return worldCanvas.ViewportPadding; }
        }

        /// <summary>
        /// Determines if the world is dragging with
        /// the space bar (or going to).
        /// </summary>
        public bool IsDragging
        {
            get { return worldCanvas.IsDragging; }
        }

        bool changesMade;

        /// <summary>
        /// Determines if the world has changes to be made
        /// </summary>
        public bool ChangesMade
        {
            get { return changesMade; }
            set 
            {
                if (saveLocation == null)
                    return;

                if (changesMade != value)
                {
                    Text = Path.GetFileName(saveLocation);

                    if (value == true)
                        Text += " *";
                }

                changesMade = value;
            }
        }

        /// <summary>
        /// The canvas' viewport width
        /// </summary>
        public int CanvasWidth
        {
            get { return worldCanvas.Width; }
        }

        /// <summary>
        /// The canvas' viewport width
        /// </summary>
        public int CanvasHeight
        {
            get { return worldCanvas.Height; }
        }

        /// <summary>
        /// Represents a form containing an editable world
        /// </summary>
        /// <param name="settings"> World settings </param>
        public FormWorld(WorldSettings settings, Form parent)
        {
            InitializeComponent();

            MdiParent = parent;

            history = new WorldHistory();
            this.settings = settings;

            userProperties = new Dictionary<string, string>();

            InitWorld();

            scrollBarHorizontal.Scroll += scrollBarScroll;
            scrollBarVertical.Scroll += scrollBarScroll;

            worldCanvas = new WorldCanvas(this)
            {
                ContextMenuStrip = settingsMenu
            };

            canvasPanel.Controls.Add(worldCanvas);

            worldCanvas.Dock = DockStyle.Fill;
        }

        public void FocusCanvas()
        {
            worldCanvas.Focus();
        }

        /// <summary>
        /// Disables the world
        /// </summary>
        public void Disable()
        {
            Enabled = false;
            worldCanvas.Enabled = false;
        }

        /// <summary>
        /// Enables the world
        /// </summary>
        public void Enable()
        {
            Enabled = true;
            worldCanvas.Enabled = true;
        }

        /// <summary>
        /// Deletes all tiles in the selection rectangle
        /// </summary>
        public void DeleteSelection()
        {
            WorldRegion undoRegion = GetLayerRegion(selectionRectangle, SelectedLayer),
                        redoRegion = new WorldRegion(selectionRectangle.Width, selectionRectangle.Height);

            for (int x = selectionRectangle.X; x < selectionRectangle.Right; x++)
            {
                for (int y = selectionRectangle.Y; y < selectionRectangle.Bottom; y++)
                {
                    if (!PointInWorld(x, y))
                        continue;

                    SetTile(x, y, null, SelectedLayer);
                }
            }

            History.Add(new HistoryItem(new TileChangesRegion(undoRegion, selectionRectangle.Location, SelectedLayer),
                new TileChangesRegion(redoRegion, selectionRectangle.Location, SelectedLayer), applyDeletionChanges));

            SelectionRectangle = Rectangle.Empty;

            worldCanvas.Invalidate();
        }

        /// <summary>
        /// Toggles the horiztonal scrollbar's cell when enabled/disabled.
        /// </summary>
        public void toggleHorizontalScroll(bool toggle)
        {
            scrollBarHorizontal.Visible = toggle;
            scrollVerticalPanel.Padding = new System.Windows.Forms.Padding(0, 0, 0, toggle ? 20 : 0);
        }

        /// <summary>
        /// Toggles the vertical scrollbar's cell when enabled/disabled.
        /// </summary>
        public void toggleVerticalScroll(bool toggle)
        {
            scrollVerticalPanel.Visible = toggle;
        }

        #region Tilesets

        /// <summary>
        /// Adds a tileset to the world
        /// </summary>
        /// <param name="tileset"> New tileset </param>
        public void AddTileset(WorldTileset tileset)
        {
            tilesets.Add(tileset);

            UpdateTilesetIndexes();
        }

        /// <summary>
        /// Returns a tileset based from an index
        /// </summary>
        /// <param name="index"> Tileset index </param>
        /// <returns> Null if not found, otherwise a tileset at index </returns>
        public WorldTileset GetTileset(int index)
        {
            return index < 0 || index > tilesets.Count - 1 ? null : tilesets[index];
        }

        public void RemoveTileset(int index)
        {
            tilesets.RemoveAt(index);

            for (int x = 0; x < Settings.WorldSize.Width; x++)
            {
                for (int y = 0; y < Settings.WorldSize.Height; y++)
                {
                    foreach (WorldLayer layer in layers)
                    {
                        WorldTile tile = layer[x, y];

                        if (tile == null)
                            continue;

                        if (tile.Tileset == index)
                            layer[x, y] = null;
                        else if (tile.Tileset > index)
                            layer[x, y].Tileset--;
                    }
                }
            }

            worldCanvas.Invalidate();
            History.Clear();

            UpdateTilesetIndexes();
        }

        /// <summary>
        /// Update tileset indexes when tilesets are added/modified
        /// </summary>
        public void UpdateTilesetIndexes()
        {
            tilesetIndexes = new int[tilesets.Count];

            for (int a = 0; a < tilesets.Count; a++)
            {
                var tileset = tilesets[a];
                var size = tileset.Bounds.Size.Nearest(settings.TileSize);

                // calculate tileset index
                if (a < tilesets.Count - 1)
                {
                    int length = (size.Width / settings.TileSize.Width) *
                                 (size.Height / settings.TileSize.Height);

                    if (a != 0)
                        length += tilesetIndexes[a - 1];

                    tilesetIndexes[a + 1] = length;
                }
            }
        }

        #endregion Tilesets

        #region Zoom

        /// <summary>
        /// Set the world canvas' zoom.
        /// </summary>
        /// <param name="zoom"> zoom level percentage </param>
        public void SetZoom(int zoom)
        {
            worldCanvas.Zoom = zoom;
        }

        /// <summary>
        /// Set the main form's zoom slider value
        /// </summary>
        /// <param name="zoom"> zoom level percentage </param>
        /// <returns> clamped value based off of sliders range </returns>
        public int SetZoomBarValue(int zoom)
        {
            return ((FormMain)MdiParent).SetZoomValue(zoom);
        }

        #endregion Zoom

        #region Tiles

        /// <summary>
        /// Get a tile in world
        /// </summary>
        /// <param name="x"> Tile x position </param>
        /// <param name="y"> Tile y position </param>
        /// <param name="layer"> Layer to get tile from. Default uses current </param>
        /// <returns> Tile if found </returns>
        public WorldTile GetTile(int x, int y, int layer = -1)
        {
            return layers[layer == -1 ? SelectedLayer : layer][x, y];
        }

        /// <summary>
        /// Get a tile in world
        /// </summary>
        /// <param name="point"> Tile location </param>
        /// <param name="layer"> Layer to get tile from. Default uses current </param>
        /// <returns> Tile if found </returns>
        public WorldTile GetTile(Point point, int layer = -1)
        {
            return layers[layer == -1 ? SelectedLayer : layer][point.X, point.Y];
        }

        /// <summary>
        /// Sets a tile in world
        /// </summary>
        /// <param name="x"> Tile x position </param>
        /// <param name="y"> Tile y position </param>
        /// <param name="tile"> New tile </param>
        /// <param name="layerIndex"> Layer to set tile in. Default uses current </param>
        public void SetTile(int x, int y, WorldTile tile, int layerIndex = -1)
        {
            layers[layerIndex == -1 ? SelectedLayer : layerIndex][x, y] = tile;

            ChangesMade = true;
        }

        /// <summary>
        /// Sets a tile in world
        /// </summary>
        /// <param name="point"> Tile location </param>
        /// <param name="tile"> New tile </param>
        /// <param name="layerIndex"> Layer to set tile in. Default uses current </param>
        public void SetTile(Point point, WorldTile tile, int layerIndex = -1)
        {
            layers[layerIndex == -1 ? SelectedLayer : layerIndex][point.X, point.Y] = tile;

            ChangesMade = true;
        }

        #endregion Tiles

        #region Layers

        /// <summary>
        /// Adds a tile layer
        /// </summary>
        /// <param name="name"> Layer name </param>
        public void AddLayer(string name, bool insert = true)
        {
            var layer = new WorldLayer(Settings.WorldSize.Width, Settings.WorldSize.Height, name);

            if (insert)
                layers.Insert(layers.Count - 1, layer);
            else
                layers.Add(layer);
        }

        /// <summary>
        /// Renames a layer
        /// </summary>
        /// <param name="index"> Layer index </param>
        /// <param name="newName"> New layer name </param>
        public void RenameLayer(int index, string newName)
        {
            layers[index].Name = newName;
        }

        /// <summary>
        /// Show or hides a layer
        /// </summary>
        /// <param name="index"> Layer index </param>
        /// <param name="visibility"> New layer visibility </param>
        public void ToggleLayer(int index, bool visibility)
        {
            layers[index].Visible = visibility;
        }

        /// <summary>
        /// Selects a layer
        /// </summary>
        /// <param name="index"> New selected index </param>
        public void SelectLayer(int index)
        {
            ((FormMain)MdiParent).SetSelectedLayer(index);
            worldCanvas.Invalidate();
        }

        /// <summary>
        /// Moves a layer up
        /// </summary>
        /// <param name="index"> Layer index </param>
        public void MoveLayerUp(int index)
        {
            WorldLayer layer = layers[index];

            layers.RemoveAt(index);

            layers.Insert(index + 1, layer);

            History.Clear();
        }

        /// <summary>
        /// Moves a layer down
        /// </summary>
        /// <param name="index"> Layer index </param>
        public void MoveLayerDown(int index)
        {
            WorldLayer layer = layers[index];

            layers.RemoveAt(index);

            layers.Insert(index - 1, layer);

            History.Clear();
        }

        /// <summary>
        /// Returns a layer in the world
        /// </summary>
        /// <param name="index"> Layer index </param>
        /// <returns> World layer </returns>
        public WorldLayer GetLayer(int index)
        {
            return layers[index];
        }

        /// <summary>
        /// Gets the name of a layer
        /// </summary>
        /// <param name="index"> Layer index </param>
        /// <returns> Layer name </returns>
        public string GetLayerName(int index)
        {
            return layers[index].Name;
        }

        /// <summary>
        /// Gets the visibility of a layer
        /// </summary>
        /// <param name="index"> Layer index </param>
        /// <returns> layer visibility </returns>
        public bool GetLayerVisibility(int index)
        {
            return layers[index].Visible;
        }

        /// <summary>
        /// Removes a layer
        /// </summary>
        /// <param name="index"> Layer index </param>
        public void RemoveLayer(int index)
        {
            layers.RemoveAt(index);

            History.Clear();
        }

        /// <summary>
        /// Clears all layers
        /// </summary>
        public void ClearLayers()
        {
            History.Clear();

            layers.Clear();
        }

        /// <summary>
        /// Get a region of tiles from a layer
        /// </summary>
        /// <param name="region"> Tile region </param>
        /// <param name="layer"> Layer in context </param>
        /// <returns> A region based from zero extracted from the layer </returns>
        public WorldRegion GetLayerRegion(Rectangle region, int layer)
        {
            WorldLayer layerIn = GetLayer(layer);
            WorldRegion regionOut = new WorldRegion(region.Width, region.Height);

            for (int x = region.X, ox = 0; x < region.Right; x++, ox++)
            {
                for (int y = region.Y, oy = 0; y < region.Bottom; y++, oy++)
                {
                    if (!PointInWorld(x, y))
                    {
                        regionOut[ox, oy] = null;
                        continue;
                    }

                    WorldTile i = layerIn[x, y];

                    regionOut[ox, oy] = i == null ? null : i.Clone();
                }
            }

            return regionOut;
        }

        #endregion Layers

        /// <summary>
        /// Flags world canvas for redraw
        /// </summary>
        public void InvalidateCanvas()
        {
            worldCanvas.Invalidate();
        }

        /// <summary>
        /// Called when the selected tool has changed
        /// </summary>
        public void SelectedToolChanged()
        {
            worldCanvas.OnSelectedToolChanged();
        }

        /// <summary>
        /// Determines if the location is contained
        /// in the world. Based in tile units
        /// </summary>
        /// <param name="x"> X position </param>
        /// <param name="y"> Y position </param>
        /// <returns> True if location is valid in world </returns>
        public bool PointInWorld(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Settings.WorldSize.Width && y < Settings.WorldSize.Height;
        }

        /// <summary>
        /// Inits the world with constructed settings
        /// </summary>
        void InitWorld()
        {
            layers = new List<WorldLayer>();

            layers.Add(new WorldLayer(Settings.WorldSize.Width, Settings.WorldSize.Height, "Collision Layer"));

            AddLayer("Layer 1");
           
            SelectedLayer = 0;

            tilesets = new List<WorldTileset>();
        }

        /// <summary>
        /// Update the world canvas's viewport when a scrollbar is scrolled.
        /// </summary>
        void scrollBarScroll(object sender, ScrollEventArgs e)
        {
            worldCanvas.ScrollViewport();
        }

        void applyDeletionChanges(HistoryAction action, object obj)
        {
            TileChangesRegion changes = obj as TileChangesRegion;

            if (action == HistoryAction.Undo)
            {
                SelectionRectangle = new Rectangle(changes.Offset.X, changes.Offset.Y, 
                    changes.Region.Width, changes.Region.Height);
            }
            else
            {
                SelectionRectangle = Rectangle.Empty;
            }

            Rectangle offsetRect = new Rectangle(0, 0, changes.Region.Width, changes.Region.Height),
                      changeRect = offsetRect;

            changeRect.X = Math.Max(0, changes.Offset.X);
            changeRect.Y = Math.Max(0, changes.Offset.Y);

            for (int x = changes.Offset.X, ox = 0; x < changes.Offset.X + changes.Region.Width; x++, ox++)
            {
                for (int y = changes.Offset.Y, oy = 0; y < changes.Offset.Y + changes.Region.Height; y++, oy++)
                {
                    if (PointInWorld(x, y) && changeRect.Contains(x, y) && offsetRect.Contains(ox, oy))
                        SetTile(x, y, changes.Region[ox, oy], changes.Layer);
                }
            }

            SelectLayer(changes.Layer);
        }

        void settingsMenu_Opened(object sender, EventArgs e)
        {
            toolBtnShowGrid.Checked = Properties.Settings.Default.DrawGrid;
            toolBtnLayerOpacity.Checked = Properties.Settings.Default.LayerOpacity;
        }

        void toolBtnShowGrid_Click(object sender, EventArgs e)
        {
            // toggle setting
            Properties.Settings.Default.DrawGrid = !Properties.Settings.Default.DrawGrid;
            Properties.Settings.Default.Save();
            worldCanvas.Invalidate();
            ((FormMain)MdiParent).InvalidateTilesetSelector();
        }

        void toolBtnLayerOpacity_Click(object sender, EventArgs e)
        {
            // toggle setting
            Properties.Settings.Default.LayerOpacity = !Properties.Settings.Default.LayerOpacity;
            Properties.Settings.Default.Save();
            worldCanvas.Invalidate();
        }
    }
}
