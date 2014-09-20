using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using World2D;
using WorldEditor.Classes;
using WorldEditor.Forms;
using Point = Microsoft.Xna.Framework.Point;

namespace WorldEditor
{
    partial class FormMain
    {
        /// <summary>
        /// Saves the open world as a file
        /// </summary>
        void SaveAsFile(string saveLocation)
        {
           using (var saveSettings = new FormSaveSettings(saveLocation, OpenWorld))
           {
               if (saveSettings.ShowDialog() == DialogResult.OK)
               {
                   try
                   {
                       // TODO: different file types
                       var settings = OpenWorld.Settings;

                       var save = new WorldFile()
                       {
                           Size = new Point(settings.WorldSize.Width, settings.WorldSize.Height),
                           TileSize = new Point(settings.TileSize.Width, settings.TileSize.Height),
                           Properties = OpenWorld.UserProperties,
                           Tilesets = new Tileset[OpenWorld.Tilesets.Count],
                           Layers = new RawLayer[OpenWorld.LayerCount - 1],
                           CollisionData = new int[settings.WorldSize.Width * settings.WorldSize.Height]
                       };

                       for (int a = 0; a < OpenWorld.Tilesets.Count; a++)
                       {
                           var tileset = OpenWorld.Tilesets[a];
                           var size = tileset.Bounds.Size.Nearest(settings.TileSize);

                           save.Tilesets[a] = new Tileset()
                           {
                               File = tileset.ShouldBuild ? tileset.BuildLocation : null,
                               Size = new Point(size.Width, size.Height)
                           };
                       }

                       // convert layers to raw layers
                       for (int b = 0; b < OpenWorld.LayerCount - 1; b++)
                       {
                           var rawLayer = new RawLayer()
                           {
                               Name = OpenWorld.GetLayerName(b),
                               Data = new int[settings.WorldSize.Width * settings.WorldSize.Height]
                           };

                           for (int x = 0; x < settings.WorldSize.Width; x++)
                           {
                               for (int y = 0; y < settings.WorldSize.Height; y++)
                               {
                                   WorldTile tile = OpenWorld.GetTile(x, y, b);

                                   int rawValue = (tile == null) ? 0 :
                                       tile.GetIndex(settings.TileSize, save.Tilesets[tile.Tileset].Size) + OpenWorld.TilesetIndexes[tile.Tileset] + 1;

                                   rawLayer.Data[y * settings.WorldSize.Width + x] = rawValue;
                               }
                           }

                           save.Layers[b] = rawLayer;
                       }

                       for (int x = 0; x < settings.WorldSize.Width; x++)
                       {
                           for (int y = 0; y < settings.WorldSize.Height; y++)
                           {
                               WorldTile tile = OpenWorld.GetTile(x, y, OpenWorld.LayerCount - 1);

                               int rawValue = (tile == null) ? 0 :
                                   tile.GetIndex(settings.TileSize, save.Tilesets[tile.Tileset].Size) + OpenWorld.TilesetIndexes[tile.Tileset] + 1;

                               save.CollisionData[y * settings.WorldSize.Width + x] = rawValue;
                           }
                       }

                       XmlWriterSettings xmlSettings = new XmlWriterSettings() { Indent = true };

                       // serialize and save xml data to the save location
                       using (XmlWriter writer = XmlWriter.Create(saveLocation, xmlSettings))
                       {
                           IntermediateSerializer.Serialize(writer, save, null);
                       }

                       OpenWorld.SaveLocation = saveLocation;
                       OpenWorld.ChangesMade = false;

                       MessageBox.Show("The world has successfully saved.", "World Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                   }
                   catch (Exception exception)
                   {
                       MessageBox.Show("Error saving world\n\n" + exception.Message,
                           "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                   }
               }
           }
        }

        /// <summary>
        /// Saves the open world as an image
        /// </summary>
        void SaveAsImage(string saveLocation)
        {
            var settings = OpenWorld.Settings;

            using (var image = new Bitmap(settings.CanvasSize.Width, settings.CanvasSize.Height))
            using (var graphics = Graphics.FromImage(image))
            {
                // optimize the drawing
                graphics.InterpolationMode =
                    System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                graphics.SmoothingMode =
                    System.Drawing.Drawing2D.SmoothingMode.None;
                graphics.PixelOffsetMode =
                    System.Drawing.Drawing2D.PixelOffsetMode.None;
                graphics.CompositingQuality =
                    System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

                Rectangle drawRect = new Rectangle(0, 0,
                    settings.TileSize.Width, settings.TileSize.Height);

                for (int x = 0; x < settings.WorldSize.Width; x++, drawRect.X += settings.TileSize.Width)
                {
                    // reset the y position after each x iteration
                    drawRect.Y = 0;
                    for (int y = 0; y < settings.WorldSize.Height; y++, drawRect.Y += settings.TileSize.Height)
                    {
                        for (int i = 0; i < OpenWorld.LayerCount; i++)
                        {
                            WorldTile tile = OpenWorld.GetTile(x, y, i);

                            // don't need to draw it
                            if (tile == null)
                                continue;

                            graphics.DrawImage(OpenWorld.GetTileset(tile.Tileset).Image,
                                drawRect, tile.GetRectangleSystem(settings.TileSize), GraphicsUnit.Pixel);
                        }
                    }
                }

                image.Save(saveLocation);
            }
        }
    }
}
