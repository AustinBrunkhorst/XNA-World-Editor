using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using World2D;
using WorldEditor.Classes;
using WorldEditor.Forms;

namespace WorldEditor
{
    partial class FormMain
    {
        void LoadFile(string location)
        {
           // determine if the world is already being edited
           foreach (FormWorld world in MdiChildren)
           {
               if (world.SaveLocation == location)
               {
                   world.Activate();

                   return;
               }
           }

           try
           {
               using(var stream = File.Open(location, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
               using (var reader = XmlReader.Create(stream, null))
               {
                   WorldFile worldFile = IntermediateSerializer.Deserialize<WorldFile>(reader, null);

                   WorldSettings settings = new WorldSettings()
                   {
                       WorldSize = new Size(worldFile.Size.X, worldFile.Size.Y),
                       TileSize = new Size(worldFile.TileSize.X, worldFile.TileSize.Y)
                   };

                   var world = new FormWorld(settings, this);

                   world.SaveLocation = location;
                   world.ClearLayers();

                   stream.Position = 0;

                   Dictionary<string, string> properties = new Dictionary<string, string>();

                   // parse properties from xml
                   using (var propertiesReader = XmlReader.Create(stream, new XmlReaderSettings() { IgnoreWhitespace = true }))
                   {
                       while (propertiesReader.Read())
                       {
                           if (propertiesReader.IsStartElement("Item"))
                           {
                               propertiesReader.ReadStartElement("Item");

                               propertiesReader.Read();

                               string key = propertiesReader.ReadString();

                               propertiesReader.Read();

                               string value = propertiesReader.ReadString();

                               propertiesReader.Read();

                               properties.Add(key, value);
                           }
                       }
                   }

                   world.UserProperties = properties;
                    
                   using (var settingsForm = new FormLoadSettings(worldFile.Tilesets))
                   {
                       if (settingsForm.ShowDialog() == DialogResult.OK)
                       {
                           RawLayer[] layers = worldFile.Layers;

                           Array.Resize<RawLayer>(ref layers, worldFile.Layers.Length + 1);

                           worldFile.Layers = layers;

                           worldFile.Layers[worldFile.Layers.Length - 1] = new RawLayer()
                           {
                               Name = "Collision Layer",
                               Data = worldFile.CollisionData
                           };

                           parseLayers(world, worldFile.Tilesets, worldFile.Layers);

                           world.ChangesMade = false;

                           // need to show for the graphics device in the world canvas to be created
                           world.Show();

                           foreach(var tileset in worldFile.Tilesets)
                           {
                               world.AddTileset(new WorldTileset(tileset.File, world.CanvasGraphicsDevice));
                           }

                           worldChanged(null, null);
                       }
                       else
                       {
                           world.Close();
                       }
                   }
               }
           }
           catch (Exception e)
           {
               MessageBox.Show("Error while attempting to load " + Path.GetFileName(location) +"\n\n"+ e.Message, "Error Loading File", MessageBoxButtons.OK, MessageBoxIcon.Error);

               MdiChildren[MdiChildren.Length - 1].Close();
           }
        }

        void parseLayers(FormWorld world, Tileset[] tilesets, RawLayer[] layers)
        {
            var tilesetIndexes = new int[tilesets.Length];

            int tileWidth = world.Settings.TileSize.Width,
                tileHeight = world.Settings.TileSize.Height;

            // calculate the tileset indexes
            for (var a = 0; a < tilesets.Length - 1; a++)
            {
                Tileset set = tilesets[a];

                int length = (set.Size.X / tileWidth) *
                             (set.Size.Y / tileHeight);

                if (a != 0)
                    length += tilesetIndexes[a - 1];

                tilesetIndexes[a + 1] = length;
            }

            for (int b = 0; b < layers.Length; b++)
            {
                RawLayer raw = layers[b];

                world.AddLayer(raw.Name, false);

                for (int c = 0; c < raw.Data.Length; c++)
                {
                    int rawTile = raw.Data[c];

                    // tile is empty
                    if (rawTile == 0)
                        continue;

                    // subtract one because one is added when
                    // saving the file (allow for null tiles to be 0)
                    rawTile--;

                    int setIndex = 0;

                    // calculate the tileset index for the tile
                    for (int d = 0; d < tilesets.Length; d++)
                    {
                        // the last tileset, or in range tile tileset index [d]
                        if (d == tilesets.Length - 1 ||
                           (rawTile >= tilesetIndexes[d] && rawTile < tilesetIndexes[d + 1]))
                        {
                            setIndex = d;
                            break;
                        }
                    }

                    // subtract the tileset index to offset
                    // the source offset
                    rawTile -= tilesetIndexes[setIndex];

                    Tileset set = tilesets[setIndex];

                    int width = set.Size.X / tileWidth,
                        worldWidth = world.Settings.WorldSize.Width;

                    int x = rawTile % width,
                        y = rawTile / width;

                    world.SetTile(c % worldWidth, c / worldWidth, new WorldTile(x * tileWidth, y * tileHeight, setIndex), b);
                }
            }
        }
    }
}
