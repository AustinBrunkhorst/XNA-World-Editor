using DevComponents.DotNetBar;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Windows.Forms;
using WorldEditor.Classes;
using WorldEditor.Classes.Tools;
using WorldEditor.Classes.XNA;
using WorldEditor.Forms;
using Padding = System.Windows.Forms.Padding;
using Size = System.Drawing.Size;

namespace WorldEditor.Controls
{
    class WorldCanvas : GraphicsDeviceControl
    {
        FormWorld parent;

        ContentManager content;

        Point viewportPosition;

        Rectangle renderBounds, drawRect, tileRect, backgroundRect, 
                  gridRect, selectionRect, selectionRectRendered;

        Rectangle[] canvasBorders, selectionBorders;

        Texture2D colorTexture, gridTexture;

        SpriteFont idFont;

        readonly Color controlBGColor = new Color(208, 215, 226);
        readonly Color canvasBGColor = new Color(235, 242, 255);
        readonly Color canvasBorderColor = new Color(43, 60, 89);
        readonly Color selectionBorderColor = new Color(47, 154, 255);
        readonly Color selectionColor = new Color(47, 154, 255) * 0.35f;
        readonly Color collisionTileColor = new Color(200, 80, 80) * .85f;
        readonly Color collisionTileColorOff = new Color(200, 80, 80) * 0.45f;
        readonly Color offLayerTint = new Color(190, 190, 190) * 0.35f;
       
        Vector2 gridPos = Vector2.Zero;

        SpriteBatch spriteBatch;

        Point mouselastDown, mouselastMoved;

        float zoom = 1.0f;
        Matrix renderMatrix = Matrix.CreateScale(1.0f);

        ScrollBarAdv scrollHorizontal
        {
            get { return parent.scrollBarHorizontal; }
        }

        ScrollBarAdv scrollVertical
        {
            get { return parent.scrollBarVertical; }
        }

        WorldSettings settings
        {
            get { return parent.Settings; }
        }

        public bool IsDragging = false,
                    DrawBrushPreview = false;

        public GraphicsDevice GDevice
        {
            get { return GraphicsDevice; }
        }

        public Point ViewportPosition
        {
            get { return new Point(viewportPosition.X - backgroundRect.X, viewportPosition.Y - backgroundRect.Y); }
        }

        public Point ViewportOffset
        {
            get { return renderBounds.Location; }
        }

        public Point ViewportPadding
        {
            get { return drawRect.Location; }
        }

        public Rectangle SelectionRectangle
        {
            set 
            {
                selectionRect = value;
                selectionRectRendered = value;
                selectionRectRendered.Width *= settings.TileSize.Width;
                selectionRectRendered.Height *= settings.TileSize.Height;

                updateSelectionRect();
            }
        }

        public int Zoom
        {
            get
            {
                return (int)Math.Round(zoom * 100.0f);
            }
            set
            {
                zoom = (float)value / 100.0f;
            }
        }

        public WorldCanvas(FormWorld form)
        {
            Padding = Margin = new Padding(0);

            parent = form;
        }

        ~WorldCanvas()
        {
            gridTexture.Dispose();
            colorTexture.Dispose();
            content.Unload();
        }

        /// <summary>
        /// Called when the viewport position changes
        /// </summary>
        public void ScrollViewport()
        {
            if (Width > settings.CanvasSize.Width)
            {
                scrollHorizontal.Value = 0;
                parent.toggleHorizontalScroll(false);
            }
            else
            {
                scrollHorizontal.Value = Math.Min(scrollHorizontal.Maximum, scrollHorizontal.Value);
                parent.toggleHorizontalScroll(true);
            }

            if (Height > settings.CanvasSize.Height)
            {
                scrollVertical.Value = 0;
                parent.toggleVerticalScroll(false);
            }
            else
            {
                scrollVertical.Value = Math.Min(scrollVertical.Maximum, scrollVertical.Value);
                parent.toggleVerticalScroll(true);
            }

            viewportPosition.X = scrollHorizontal.Value;
            viewportPosition.Y = scrollVertical.Value;

            updateRenderBounds();
        }

        /// <summary>
        /// Called when the tile size changes
        /// </summary>
        public void OnWorldUpdate()
        {
            updateGridTexture();
            updateRenderBounds();

            tileRect = new Rectangle(0, 0, settings.TileSize.Width, settings.TileSize.Height);
        }

        /// <summary>
        /// Called when the selected tool is changed
        /// </summary>
        public void OnSelectedToolChanged()
        {
            if (parent.SelectedTool != null)
            {
                Cursor = parent.SelectedTool.CanvasCursor;

                Invalidate();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                Zoom = parent.SetZoomBarValue(Zoom + (Math.Sign(e.Delta) * 20));
            }
            else
            {
                scrollVertical.Value = (scrollVertical.Value - e.Delta).Clamp(0, scrollVertical.Maximum);
            }

            ScrollViewport();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    if (parent.SelectionRectangle != Rectangle.Empty)
                    {
                        parent.DeleteSelection();
                    }
                    break;
                case Keys.Space:
                    if (!IsDragging)
                    {
                        WorldTool tool = parent.SelectedTool;

                        if (tool is WorldSelection)
                        {
                            ((WorldSelection)tool).ClearSelection();
                        }

                        IsDragging = true;
                        Cursor = FormMain.CursorDrag;
                        mouselastDown = mouselastMoved;

                        Invalidate();
                    }
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                IsDragging = false;

                if (parent.SelectedTool != null)
                    Cursor = parent.SelectedTool.CanvasCursor;
                else
                    Cursor = Cursors.Default;

                Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (parent.SelectedTool != null)
            {
                Cursor = parent.SelectedTool.CanvasCursor;
                parent.SelectedTool.OnMouseEnter(e);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();

            if (e.Button == MouseButtons.Left)
            {
                mouselastDown = new Point(e.X, e.Y);

                if (IsDragging)
                    Cursor = FormMain.CursorDragging;
            }

            if (parent.SelectedTool != null)
                parent.SelectedTool.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && IsDragging)
            {
                Cursor = FormMain.CursorDrag;
            }

            if (parent.SelectedTool != null)
                parent.SelectedTool.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point location = new Point(e.X, e.Y),
                  status = new Point(ViewportPosition.X + e.X, ViewportPosition.Y + e.Y).Nearest(settings.TileSize).Divided(settings.TileSize);

            ((FormMain)parent.MdiParent).SetMouseLocation(status);

            if (IsDragging && e.Button == MouseButtons.Left)
            {
                scrollHorizontal.Value = (scrollHorizontal.Value + mouselastDown.X - e.X).Clamp(0, scrollHorizontal.Maximum);
                scrollVertical.Value = (scrollVertical.Value + mouselastDown.Y - e.Y).Clamp(0, scrollVertical.Maximum);

                mouselastDown = location;

                ScrollViewport();

                return;
            }

            mouselastMoved = location;

            if (parent.SelectedTool != null)
                parent.SelectedTool.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (parent.SelectedTool != null)
            {
                Cursor = Cursors.Default;
                parent.SelectedTool.OnMouseLeave(e);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            scrollHorizontal.Maximum = (Width < settings.CanvasSize.Width) ?
                                        settings.CanvasSize.Width - Size.Width : 0;
            scrollVertical.Maximum = (Height < settings.CanvasSize.Height) ?
                                      settings.CanvasSize.Height - Size.Height : 0;
            ScrollViewport();
        }

        protected override void Initialize()
        {
            content = new ResourceContentManager(Services, Properties.Resources.ResourceManager);

            idFont = content.Load<SpriteFont>("IDFont");

            spriteBatch = new SpriteBatch(GraphicsDevice);

            colorTexture = new Texture2D(GraphicsDevice, 1, 1);
            colorTexture.SetData<Color>(new Color[] { Color.White });

            OnWorldUpdate();

            if(parent.SelectedTool != null)
                parent.SelectedTool.InitGraphics(GraphicsDevice);
        }

        protected override void Draw()
        {
            GraphicsDevice.Clear(controlBGColor);

            // TODO: zooming.
            spriteBatch.Begin();

            // draw the rendered background
            spriteBatch.Draw(colorTexture, backgroundRect, canvasBGColor);

            Rectangle draw = drawRect;

            int yInitial = draw.Y;

            // draw tiles need be rendered in viewport.
            for (int x = renderBounds.X; x < renderBounds.Width; x++, draw.X += settings.TileSize.Width)
            {
                draw.Y = yInitial;

                for (int y = renderBounds.Y; y < renderBounds.Height; y++, draw.Y += settings.TileSize.Height)
                {
                    if (!parent.PointInWorld(x, y))
                        continue;

                    // draw layers in reverse to honor z-index
                    for (int i = 0; i < parent.LayerCount; i++) 
                    {
                        // don't need to draw it if it's hidden
                        if (parent.GetLayerVisibility(i) == false)
                            continue;

                        WorldTile tile = parent.GetTile(x, y, i);

                        if (tile == null)
                            continue;

                        tileRect.X = tile.X;
                        tileRect.Y = tile.Y;

                        bool useOpacity = (i != parent.SelectedLayer) && 
                            Properties.Settings.Default.LayerOpacity;

                        Color tint;

                        // TODO: yeah...
                        //// collision layer
                        //if (i == parent.LayerCount - 1)
                        //{
                        //    tint = (useOpacity ? collisionTileColorOff : collisionTileColor);

                        //    spriteBatch.Draw(
                        //        texture: colorTexture,
                        //        destinationRectangle: draw,
                        //        sourceRectangle: tileRect,
                        //        color: tint);

                        //    // draw the collision ID
                        //    if (i == parent.SelectedLayer)
                        //    {
                        //        Point size = new Point() {
                        //            X = parent.Tilesets[tile.Tileset].Bounds.Width,
                        //            Y = parent.Tilesets[tile.Tileset].Bounds.Height
                        //        };

                        //        string id = ((tile == null) ? 0 :
                        //            tile.GetIndex(settings.TileSize, size) + parent.TilesetIndexes[tile.Tileset] + 1).ToString();

                        //        Vector2 idSize = idFont.MeasureString(id),
                        //                tileSize = new Vector2()
                        //                {
                        //                    X = settings.TileSize.Width,
                        //                    Y = settings.TileSize.Height
                        //                };

                        //        spriteBatch.DrawString(idFont, id, new Vector2(draw.X, draw.Y) + ((tileSize - idSize) / 2), Color.White);
                        //    }
                        //}
                        //else
                        {
                            tint = (useOpacity ? offLayerTint : Color.White);

                            var tileset = parent.GetTileset(tile.Tileset);

                            if (tileset == null)
                                continue;

                            spriteBatch.Draw(
                                texture: tileset.Texture,
                                destinationRectangle: draw,
                                sourceRectangle: tileRect,
                                color: tint);
                        }
                    }
                }
            }

            spriteBatch.End();

            // draw tile grid if enabled
            if (Properties.Settings.Default.DrawGrid)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, null, null);

                spriteBatch.Draw(gridTexture, gridPos, gridRect, Color.White);

                //draw borders
                foreach(Rectangle border in canvasBorders)
                    spriteBatch.Draw(colorTexture, border, canvasBorderColor);

                spriteBatch.End();
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // draw the selection rectangle if it exists
            if (selectionRect != Rectangle.Empty)
            {
                spriteBatch.Draw(colorTexture, selectionRectRendered, selectionColor);

                foreach (Rectangle border in selectionBorders)
                    spriteBatch.Draw(colorTexture, border, selectionBorderColor); 
            }

            // allow the selected tool to draw
            if (parent.SelectedTool != null && parent.SelectedTool.RequiresDraw)
            {
                parent.SelectedTool.Draw(spriteBatch);
            }

            spriteBatch.End();
        }

        /// <summary>
        /// Updates locations/sizes with new viewport
        /// </summary>
        void updateRenderBounds()
        {
            Size size = new Size(Size.Width, Size.Height).Nearest(settings.TileSize, true).Divided(settings.TileSize);
            Point location = viewportPosition.Nearest(settings.TileSize).Divided(settings.TileSize);

            renderBounds = new Rectangle(location.X,
                location.Y,
                location.X + size.Width + 1,
                location.Y + size.Height + 1);

            int drawX = renderBounds.X * settings.TileSize.Width - viewportPosition.X,
                drawY = renderBounds.Y * settings.TileSize.Height - viewportPosition.Y;

            if (Size.Width > settings.CanvasSize.Width)
            {
                drawX = (Size.Width - settings.CanvasSize.Width) / 2;
            }

            if (Size.Height > settings.CanvasSize.Height)
            {
                drawY = (Size.Height - settings.CanvasSize.Height) / 2;
            }

            drawRect = new Rectangle(drawX, drawY, settings.TileSize.Width,
                settings.TileSize.Height);

            backgroundRect = new Rectangle(drawX, drawY,
                Math.Min((size.Width + 1) * settings.TileSize.Width, settings.CanvasSize.Width),
                Math.Min((size.Height + 1) * settings.TileSize.Height, settings.CanvasSize.Height));

            gridRect.Width = backgroundRect.Width - 1;
            gridRect.Height = backgroundRect.Height - 1;

            gridPos.X = backgroundRect.X;
            gridPos.Y = backgroundRect.Y;

            canvasBorders = bordersFromRectangle(backgroundRect);

            updateSelectionRect();

            Invalidate();
        }

        /// <summary>
        /// Updates the draw region for the selection rectangle
        /// </summary>
        void updateSelectionRect()
        {
            selectionRectRendered.X = (selectionRect.X - renderBounds.X) * settings.TileSize.Width + drawRect.X;
            selectionRectRendered.Y = (selectionRect.Y - renderBounds.Y) * settings.TileSize.Height + drawRect.Y;

            selectionBorders = bordersFromRectangle(selectionRectRendered);
        }

        /// <summary>
        /// Generates a dashed tile grid texture with given tile size. (bottom & right lines)
        /// </summary>
        void updateGridTexture()
        {
            gridTexture = new Texture2D(GraphicsDevice, settings.TileSize.Width, settings.TileSize.Height);
            gridRect = new Rectangle(0, 0, settings.TileSize.Width, settings.TileSize.Height);

            int w = settings.TileSize.Width,
                h = settings.TileSize.Height,
                s = w * h;

            Color[] gridData = new Color[s];
            Color gridColor = Color.Black * 0.90f;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < (i == 0 ? w : h); j += 7)
                {
                    for (int k = j; k < j + 3; k++)
                    {
                        int l = settings.TileSize.Width * (i == 0 ? h - 1 : k) + (i == 0 ? k : w - 1);

                        if(l < s)
                            gridData[l] = gridColor;
                    }
                }
            }

            gridTexture.SetData<Color>(gridData);
        }

        /// <summary>
        /// Generates 4 rectangles representing the borders of the given rectangle
        /// </summary>
        /// <param name="rectangle"> Rectangle to generate borders from </param>
        /// <returns> Border rectangles </returns>
        Rectangle[] bordersFromRectangle(Rectangle rectangle)
        {
            return new Rectangle[4] {
                new Rectangle(rectangle.X, rectangle.Y, 1, rectangle.Height),
                new Rectangle(rectangle.Right, rectangle.Y, 1, rectangle.Height + 1),
                new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, 1),
                new Rectangle(rectangle.X, rectangle.Bottom, rectangle.Width + 1, 1)
            };
        }
    }
}
