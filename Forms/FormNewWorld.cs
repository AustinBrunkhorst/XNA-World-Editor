using System;
using System.Drawing;
using System.Windows.Forms;
using WorldEditor.Classes;

namespace WorldEditor.Forms
{
    public partial class FormNewWorld : Form
    {
        WorldSettings settings;

        /// <summary>
        /// Settings created
        /// </summary>
        public WorldSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        public FormNewWorld()
        {
            InitializeComponent();
        }

        void valueChanged(object sender, EventArgs e)
        {
            lblWorldSize.Text = String.Format("{0} x {1}",
                worldWidth.Value * tileWidth.Value,
                worldHeight.Value * tileHeight.Value);
        }

        void btnOk_Click(object sender, EventArgs e)
        {
            settings = new WorldSettings()
            {
                WorldSize = new Size(worldWidth.Value, worldHeight.Value),
                TileSize = new Size(tileWidth.Value, tileHeight.Value)
            };
        }
    }
}
