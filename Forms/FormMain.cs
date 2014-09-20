using DevComponents.DotNetBar;
using System;
using System.IO;
using System.Media;
using System.Windows.Forms;
using WorldEditor.Classes;
using WorldEditor.Controls;
using WorldEditor.Forms;

namespace WorldEditor
{
    public partial class FormMain : Office2007RibbonForm
    {
        LayersPanel layersPanel;
        TilesetsPanel tilesetsPanel;

        public static Cursor CursorDrag;
        public static Cursor CursorDragging;
        public static Cursor CursorPaint;

        WorldTool selectedTool;

        /// <summary>
        /// Current tool being used in the world
        /// </summary>
        public WorldTool SelectedTool
        {
            get { return selectedTool; }
            set 
            { 
                selectedTool = value;

                if (OpenWorld != null)
                {
                    if(SelectedTool.RequiresDraw)
                        SelectedTool.InitGraphics(OpenWorld.CanvasGraphicsDevice);

                    OpenWorld.SelectedToolChanged();
                }
            }
        }

        /// <summary>
        /// The current world open
        /// </summary>
        public FormWorld OpenWorld
        {
            get { return (FormWorld)ActiveMdiChild; }
        }

        public FormMain()
        {
            InitializeComponent();

            using (var dragStream = new MemoryStream(Properties.Resources.CursorDrag))
                CursorDrag = new Cursor(dragStream);

            using (var draggingStream = new MemoryStream(Properties.Resources.CursorDragging))
                CursorDragging = new Cursor(draggingStream);

            CursorPaint = new Cursor(Properties.Resources.CursorPaint.GetHicon());

            initDockContainers();
        }

        ~FormMain()
        {
            CursorDrag.Dispose();
            CursorDragging.Dispose();
        }

        /// <summary>
        /// Sets the mouse location status bar value
        /// </summary>
        /// <param name="location"></param>
        public void SetMouseLocation(Microsoft.Xna.Framework.Point location)
        {
            mouseLocation.Text = String.Format("({0}, {1})", location.X, location.Y);
        }

        /// <summary>
        /// Selects a new layer
        /// </summary>
        /// <param name="index"></param>
        public void SetSelectedLayer(int index)
        {
            layersPanel.SetLayer(index);
        }

        /// <summary>
        /// Redraws the tileset selector. Used when the draw grid setting changes
        /// </summary>
        public void InvalidateTilesetSelector()
        {
            tilesetsPanel.InvalidateSelector();
        }

        public int SetZoomValue(int zoom)
        {
            int display = zoom.Clamp(zoomSlider.Minimum, zoomSlider.Maximum);

            zoomSlider.Value = display;
            zoomSlider.Text = String.Format("{0}%", display);

            return display;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case (Keys.Control | Keys.Z):
                    if (OpenWorld != null)
                    {
                        if (!OpenWorld.History.CanUndo)
                        {
                            SystemSounds.Beep.Play();
                            return true;
                        }

                        OpenWorld.History.Undo();
                    }
                    break;
                case (Keys.Control | Keys.Y):
                    if (OpenWorld != null)
                    {
                        if (!OpenWorld.History.CanRedo)
                        {
                            SystemSounds.Beep.Play();
                            return true;
                        }

                        OpenWorld.History.Redo();
                    }
                    break;
                case (Keys.Control | Keys.D0):
                    resetZoom();
                    break;
                case (Keys.Control | Keys.Oemplus):
                    zoomSlider.Value += 20;
                    break;
                case (Keys.Control | Keys.OemMinus):
                    zoomSlider.Value -= 20;
                    break;
                case (Keys.Control | Keys.S):
                    // save with the cached save location
                    if (OpenWorld != null && OpenWorld.SaveLocation != null)
                    {
                        SaveAsFile(OpenWorld.SaveLocation);
                    }
                    // open a dialog and save at that location
                    else if (OpenWorld != null && saveAsWorldDialog.ShowDialog() != DialogResult.Cancel)
                    {
                        SaveAsFile(saveAsWorldDialog.FileName);
                    }
                    break;
                case (Keys.Control | Keys.N):
                    btnOpenWorld_Click(null, null);
                    break;
                case (Keys.Control | Keys.H):
                    if (OpenWorld == null)
                        return true;

                    Properties.Settings.Default.DrawGrid = !Properties.Settings.Default.DrawGrid;
                    Properties.Settings.Default.Save();
                    OpenWorld.InvalidateCanvas();
                    InvalidateTilesetSelector();
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        Control focusedControl(Control container)
        {
            foreach (Control childControl in container.Controls)
            {
                if (childControl.Focused)
                {
                    return childControl;
                }
            }

            foreach (Control childControl in container.Controls)
            {
                Control maybeFocusedControl = focusedControl(childControl);

                if (maybeFocusedControl != null)
                {
                    return maybeFocusedControl;
                }
            }

            return null;
        }

        void worldChanged(object sender, EventArgs e)
        {
            layersPanel.SelectedWorld = OpenWorld;
            tilesetsPanel.SelectedWorld = OpenWorld;

            zoomSlider.Enabled = false;

            if (OpenWorld != null)
            {
                zoomSlider.Enabled = true;
            }

            if (SelectedTool != null)
                SelectedTool.OnWorldChange();
        }

        void initDockContainers()
        {
            layersPanel = new LayersPanel();
            tilesetsPanel = new TilesetsPanel();

            layersDock.Controls.Add(layersPanel);
            tilesetsDock.Controls.Add(tilesetsPanel);
        }

        void resetZoom()
        {
            if (OpenWorld == null)
                return;

            zoomSlider.Value = 100;
            zoomSlider.Text = "100%";

            OpenWorld.SetZoom(100);
        }

        void zoomSlider_ValueChanged(object sender, EventArgs e)
        {
            if (OpenWorld == null)
                return;

            OpenWorld.SetZoom(zoomSlider.Value);

            zoomSlider.Text = String.Format("{0}%", zoomSlider.Value);
        }

        void btnWorldSettings_Click(object sender, EventArgs e)
        {
           
        }

        void btnWorldProperties_Click(object sender, EventArgs e)
        {
            if (OpenWorld == null)
                return;

            FormWorldProperties propertiesForm = new FormWorldProperties(OpenWorld.UserProperties);

            propertiesForm.Closed += delegate
            {
                OpenWorld.UserProperties = propertiesForm.EditedProperties;
            };

            propertiesForm.ShowDialog();
        }

        /// <summary>
        /// Called when a button tool is pressed
        /// </summary>
        void selectTool(object sender, EventArgs e)
        {
            if (OpenWorld == null || !(sender is ButtonItem))
                return;

            OpenWorld.FocusCanvas();

            ButtonItem tool = sender as ButtonItem;

            // don't need to re-initialize a tool that's already set
            if (tool.Checked)
                return;

            // uncheck other tools
            foreach (ButtonItem item in worldToolsContainer.Items)
                item.Checked = false;

            tool.Checked = true;

            string toolClassName = ((string)tool.Tag).Split(';')[0];

            // using the button's tag string, create a new instance
            // of that tool, and set it as the selected tool.
            SelectedTool = (WorldTool)Activator.CreateInstance(Type.GetType("WorldEditor.Classes.Tools." + toolClassName), new object[] { this });
        }

        void btnFile_Click(object sender, EventArgs e)
        {
            // is a world open
            bool flag = (OpenWorld != null);

            // buttons to disable
            ButtonItem[] disabled = new ButtonItem[]
            {
                btnSaveWorld
            };

            foreach (var button in disabled)
                button.Enabled = flag;
        }

        void btnNewWorld_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new FormNewWorld())
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    (new FormWorld(settingsForm.Settings, this)).Show();
                }
            }
        }

        void btnSaveAsWorld_Click(object sender, EventArgs e)
        {
            if (saveAsWorldDialog.ShowDialog() == DialogResult.Cancel)
                return;

            SaveAsFile(saveAsWorldDialog.FileName);
        }

        void btnSaveAsImage_Click(object sender, EventArgs e)
        {
            try 
            {
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Filter = "Image|*.png";
                    dialog.OverwritePrompt = true;

                    if (dialog.ShowDialog() == DialogResult.Cancel)
                        return;

                    SaveAsImage(dialog.FileName);

                    MessageBox.Show("Image successfully saved to \"" + dialog.FileName + "\"",
                        "Image Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error creating image..",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                Console.WriteLine(exception.Message);
            }
        }

        void btnOpenWorld_Click(object sender, EventArgs e)
        {
            if (openWorldDialog.ShowDialog() == DialogResult.OK)
            {
                LoadFile(openWorldDialog.FileName);
            }
        }

        void onKeyPress(object sender, KeyPressEventArgs e)
        {
            // don't need to do anything if a world isn't open
            if (OpenWorld == null)
                return;

            if (focusedControl(this) is TextBoxBase)
                return;

            // iterate thorugh all tool button and check if the key pressed 
            // was the one bound to tool
            foreach (ButtonItem tool in worldToolsContainer.Items)
            {
                // not a button item, so not a tool
                if (!(tool is ButtonItem))
                    continue;

                // get the character bound to the tool
                char bind = ((string)tool.Tag).Split(';')[1].ToLower()[0];

                if (e.KeyChar == bind)
                {
                    // already in use
                    if(tool.Checked)
                        return;

                    // uncheck other tools
                    foreach (ButtonItem item in worldToolsContainer.Items)
                        item.Checked = false;

                    tool.Checked = true;

                    string toolClassName = ((string)tool.Tag).Split(';')[0];

                    // using the button's tag string, create a new instance
                    // of that tool, and set it as the selected tool.
                    SelectedTool = (WorldTool)Activator.CreateInstance(Type.GetType("WorldEditor.Classes.Tools." + toolClassName), new object[] { this });

                    OpenWorld.FocusCanvas();

                    return;
                }
            }
        }

        void onFileDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        void onFileDragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string file in files)
                LoadFile(file);
        }
    }
}
