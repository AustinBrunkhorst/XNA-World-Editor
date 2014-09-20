using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WorldEditor.Forms
{
    public partial class FormOpenWorld : Form
    {
        public FormOpenWorld()
        {
            InitializeComponent();
        }

        void LoadTilesets()
        {
            string derp = @"C:\Users\Austin Brunkhorst\Desktop\AustinBrunkhorst";
            string derpName = Path.GetFileNameWithoutExtension(derp);

            string[] files = Directory.GetFiles(Path.GetDirectoryName(derp));
            string[] extensions = new string[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };

            foreach (string file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file),
                       extension = Path.GetExtension(file);

                if (name == derpName && extensions.Contains(extension.ToLower()))
                {
                    Console.WriteLine(name);
                }
            }
        }
    }
}
