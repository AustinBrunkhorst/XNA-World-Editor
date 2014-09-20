using System;
using System.Windows.Forms;

namespace WorldEditor.Forms
{
    public partial class FormSettings : Form
    {
        public FormSettings(string title, object settings)
        {
            InitializeComponent();

            Text = title;

            propertiesGrid.SelectedObject = settings;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
