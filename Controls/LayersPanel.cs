using DevComponents.AdvTree;
using System;
using System.Windows.Forms;
using WorldEditor.Forms;

namespace WorldEditor.Controls
{
    public partial class LayersPanel : UserControl
    {
        FormWorld world;

        public FormWorld SelectedWorld
        {
            get { return world; }
            set
            {
                world = value;
                onWorldChange();
            }
        }

        int selected
        {
            get { return layersList.SelectedIndex; }
        }

        int selectedForWorld
        {
            get { return layersList.Nodes.Count - 1 - selected; }
        }

        public LayersPanel()
        {
            InitializeComponent();

            Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Selects a layer
        /// </summary>
        /// <param name="index"> New layer index </param>
        public void SetLayer(int index)
        {
            layersList.SelectedIndex = layersList.Nodes.Count - 1 - index;
        }

        /// <summary>
        /// Called when the selected world changes
        /// </summary>
        void onWorldChange()
        {
            // closed last world open
            if (world == null)
            {
                layersList.Nodes.Clear();
                btnAddLayer.Enabled = false;
                return;
            }

            btnAddLayer.Enabled = true;

            // cache index before it gets changed from
            // modifying the list
            int index = world.SelectedLayer;

            layersList.Nodes.Clear();

            // collision layer
            Node collision = new Node("Collision Layer")
            {
                CheckBoxVisible = true,
                Checked = true,
                ImageIndex = 1,
                Editable = false
            };

            layersList.Nodes.Add(collision);
         
            // add layers from new world
            for (int i = world.LayerCount - 2; i >= 0; i--)
                addLayer(world.GetLayerName(i), i == world.SelectedLayer, world.GetLayerVisibility(i));

            SetLayer(index);
        }

        void btnAddLayer_Click(object sender, EventArgs e)
        {
            string name = "Layer " + layersList.Nodes.Count;
            
            addLayer(name);
            world.AddLayer(name);
        }

        /// <summary>
        /// Adds a layer to current world
        /// </summary>
        /// <param name="name"></param>
        /// <param name="select"></param>
        void addLayer(string name, bool select = false, bool check = true)
        {
            Node layer = new Node("Layer " + layersList.Nodes.Count)
            {
                CheckBoxVisible = true,
                Checked = check,
                ImageIndex = 0,
                ContextMenu = layerMenu
            };

            layersList.Nodes.Insert(1, layer);

            if (select)
                layersList.SelectedNode = layer;
        }

        /// <summary>
        /// Called when a new layer is selected
        /// </summary>
        void selectNewLayer(object sender, EventArgs e)
        {
            if (world == null)
                return;

            world.SelectedLayer = selectedForWorld;
            world.InvalidateCanvas();
        }

        /// <summary>
        /// Called when a layer is renamed via dbl click
        /// </summary>
        void renameLayer(object sender, CellEditEventArgs e)
        {
            world.RenameLayer(selectedForWorld, e.NewText);
        }

        /// <summary>
        /// Removes the selected layer
        /// </summary>
        void removeLayer()
        {
            // reject the deletion with a beep (we don't want to remove the collision layer)
            if (selected == 0)
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            Node layer = layersList.Nodes[selected];

            if (MessageBox.Show("Are you sure you would like to delete \"" + layer.Text + "\"?",
                "Remove Layer", MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning) == DialogResult.Cancel)
            {
                return;
            }

            world.RemoveLayer(selectedForWorld);
            layersList.Nodes.RemoveAt(selected);
        }

        /// <summary>
        /// Called when a layer visibility checkbox state is changed
        /// </summary>
        void toggleLayerVisibility(object sender, AdvTreeCellEventArgs e)
        {
            if (world == null)
                return;

            world.ToggleLayer(selectedForWorld, e.Cell.Checked);
            world.InvalidateCanvas();
        }

        void layerMenu_Opened(object sender, EventArgs e)
        {
            toolBtnMoveUp.Enabled = selected > 1;
            toolBtnMoveDown.Enabled = selected < layersList.Nodes.Count - 1;
        }

        void toolBtnMoveUp_Click(object sender, EventArgs e)
        {
            Node layer = layersList.SelectedNode;

            world.MoveLayerUp(selectedForWorld);

            int index = selected,
                newIndex = index - 1;

            layersList.Nodes.RemoveAt(index);
            layersList.Nodes.Insert(newIndex, layer);
            layersList.SelectedIndex = newIndex;
        }

        void toolBtnMoveDown_Click(object sender, EventArgs e)
        {
            Node layer = layersList.SelectedNode;

            world.MoveLayerDown(selectedForWorld);

            int index = selected,
                newIndex = index + 1;

            layersList.Nodes.RemoveAt(index);
            layersList.Nodes.Insert(newIndex, layer);
            layersList.SelectedIndex = newIndex;
        }

        void toolBtnRemove_Click(object sender, EventArgs e)
        {
            removeLayer();
        }

        void layersList_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Delete)
                removeLayer();
        }

        void layersList_BeforeCheck(object sender, AdvTreeCellBeforeCheckEventArgs e)
        {
            if (Control.MouseButtons != MouseButtons.Left)
                e.Cancel = true;
        }
    }
}

​