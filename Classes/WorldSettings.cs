using System;
using System.ComponentModel;
using System.Drawing;

namespace WorldEditor.Classes
{
    public class WorldSettings : INotifyPropertyChanged
    {
        Size worldSize, tileSize;

        [Browsable(true), Description("World size in tiles"), Category("General")]
        public Size WorldSize
        {
            get { return worldSize; }
            set
            {
                worldSize = new Size(Math.Abs(value.Width), Math.Abs(value.Height));

                OnPropertyChanged(new PropertyChangedEventArgs("World Size"));
            }
        }

        [Browsable(true), Description("Size of each tile in pixels"), Category("General")]
        public Size TileSize
        {
            get { return tileSize; }
            set
            {
                tileSize = new Size(Math.Abs(value.Width), Math.Abs(value.Height));
                
                OnPropertyChanged(new PropertyChangedEventArgs("Tile Size"));
            }
        }

        [Browsable(false)]
        public Size CanvasSize
        {
            get { return new Size(worldSize.Width * tileSize.Width, worldSize.Height * tileSize.Height); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }
    }
}
