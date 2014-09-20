using DevComponents.AdvTree;
using System;
using System.Windows.Forms;
using World2D;

namespace WorldEditor.Forms
{
    public partial class FormLoadSettings : Form
    {
        Tileset[] tilesets;

        public FormLoadSettings(Tileset[] tilesets)
        {
            InitializeComponent();

            this.tilesets = tilesets;

            foreach (var tileset in tilesets)
            {
                Node node = new Node(tileset.File);

                if (String.IsNullOrEmpty(tileset.File))
                {
                    node.Text = String.Format("Not Specified [w: {0}, h: {1}]", tileset.Size.X, tileset.Size.Y);
                }

                node.Cells.Add(new Cell("<browse for file>"));

                node.Cells[0].Editable = false;
                node.Cells[1].Editable = false;

                node.NodeDoubleClick += node_NodeDoubleClick;

                tilesetFilesList.Nodes.Add(node);
            }
        }

        void node_NodeDoubleClick(object sender, EventArgs e)
        {
            if (tilesetBrowseDialog.ShowDialog() == DialogResult.OK)
            {
                var node = sender as Node;

                node.Cells[1].Text = tilesetBrowseDialog.FileName;
                node.Tag = true;

                bool enable = true;

                foreach (var tileset in tilesetFilesList.Nodes)
                {
                    if (!(bool)node.Tag)
                    {
                        enable = false;
                        break;
                    }
                }

                btnLoad.Enabled = enable;
            }
        }

        void btnLoad_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < tilesets.Length; i++)
            {
                tilesets[i].File = tilesetFilesList.Nodes[i].Cells[1].Text;
            }
        }
    }
}
