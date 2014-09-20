using DevComponents.AdvTree;
using System;
using System.IO;
using System.Windows.Forms;

namespace WorldEditor.Forms
{
    public partial class FormSaveSettings : Form
    {
        string saveLocation;

        FormWorld world;

        public FormSaveSettings(string saveLocation, FormWorld world)
        {
            InitializeComponent();

            this.saveLocation = saveLocation;
            this.world = world;

            var saveUri = new Uri(saveLocation);

            foreach (var tileset in world.Tilesets)
            {
                string relative = saveUri.MakeRelativeUri(new Uri(tileset.Filename)).ToString();

                Node node = new Node(Path.GetFileName(tileset.Filename))
                {
                    CheckBoxVisible = true,
                    Checked = tileset.ShouldBuild
                };

                Cell cell = new Cell()
                {
                    Text = tileset.BuildLocation != null ? tileset.BuildLocation : 
                           Path.Combine(Path.GetDirectoryName(relative), Path.GetFileNameWithoutExtension(relative)),
                    Editable = true
                };

                node.Cells[0].Editable = false;

                node.Cells.Add(cell);

                tilesetFilesList.Nodes.Add(node);
            }
        }

        void btnSave_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < tilesetFilesList.Nodes.Count; i++)
            {
                var node = tilesetFilesList.Nodes[i];
                var tileset = world.Tilesets[i];

                tileset.BuildLocation = node.Cells[1].Text;
                tileset.ShouldBuild = node.Cells[0].Checked;
            }
        }
    }
}
