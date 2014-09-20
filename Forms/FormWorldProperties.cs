using DevComponents.AdvTree;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WorldEditor.Forms
{
    public partial class FormWorldProperties : Form
    {
        const string newPropertyText = "<new property>";

        Dictionary<string, string> worldProperties;

        public Dictionary<string, string> EditedProperties
        {
            get { return worldProperties; }
        }

        public FormWorldProperties(Dictionary<string, string> properties)
        {
            InitializeComponent();

            worldProperties = properties;

            foreach (var property in properties)
                addProperty(property.Key, property.Value, true);

            addProperty();
        }

        /// <summary>
        /// Adds a temporary property to the list
        /// </summary>
        void addProperty(string name = newPropertyText, string value = "", bool valueEditable = false)
        {
            Node node = new Node(name)
            {
                ContextMenu = propertyMenu
            };

            Cell cell = new Cell(value)
            {
                Editable = valueEditable
            };

            node.Cells.Add(cell);

            propertiesList.Nodes.Add(node);
        }

        void propertiesTree_BeforeCellEdit(object sender, CellEditEventArgs e)
        {
            e.Cell.TagString = e.Cell.Text;

            if (e.Cell.Text == newPropertyText)
                e.Cell.Text = String.Empty;
        }

        void propertiesTree_AfterCellEditComplete(object sender, CellEditEventArgs e)
        {
            if (e.Cell.TagString == newPropertyText)
            {
                if (e.Cell.Text.Length == 0)
                {
                    e.Cell.Text = newPropertyText;
                }
                else
                {
                    propertiesList.SelectedNode.Cells[1].Editable = true;
                    addProperty();
                }
            }
        }

        void propertyMenu_Opened(object sender, EventArgs e)
        {
            // disable the remove buttonif the selected 
            // node is a temporary node
            toolBtnRemove.Enabled = propertiesList.SelectedNode.Cells[1].Editable;
        }

        void toolBtnRemove_Click(object sender, EventArgs e)
        {
            propertiesList.Nodes.RemoveAt(propertiesList.SelectedIndex);
        }

        void btnOk_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();

            foreach (Node node in propertiesList.Nodes)
            {
                // don't add the temporary node
                if(node.Cells[1].Editable)
                    properties.Add(node.Cells[0].Text, node.Cells[1].Text);
            }

            worldProperties = properties;

            Close();
        }
    }
}
