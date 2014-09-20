using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WorldEditor.Classes.History;

namespace WorldEditor.Classes.Tools
{
    class TileFill : TileTool
    {
        readonly Point cursorHotspot = new Point(8, 8);

        public TileFill(FormMain main) : base(main) 
        {
            CanvasCursor = FormMain.CursorPaint;
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            if (TilesetSelection == Rectangle.Empty || World.IsDragging || e.Button != MouseButtons.Left)
                return;

            Point location = RelativePoint(new System.Drawing.Point(e.X + cursorHotspot.X, e.Y + cursorHotspot.Y));

            if (!World.PointInWorld(location.X, location.Y) ||
                (World.SelectionRectangle != Rectangle.Empty && !World.SelectionRectangle.Contains(location)))
                return;

            fillFromOrigin(location);
           
            World.InvalidateCanvas();
        }

        protected override void ApplyTileChanges(HistoryAction action, object obj)
        {
            TileChangesRegion changes = obj as TileChangesRegion;

            //iterate through the changed region and apply it to the current layer
            for (int x = changes.Offset.X, ox = 0; x < changes.Offset.X + changes.Region.Width; x++, ox++)
            {
                for (int y = changes.Offset.Y, oy = 0; y < changes.Offset.Y + changes.Region.Height; y++, oy++)
                {
                    World.SetTile(x, y, changes.Region[ox, oy], changes.Layer);
                }
            }

            World.SelectLayer(changes.Layer);
        }

        void fillFromOrigin(Point location)
        {
            renderFillInfo(computeFillInfo(location));
        }

        /// <summary>
        /// Renders fill changes
        /// </summary>
        /// <param name="info"> Fill information </param>
        void renderFillInfo(FillInfo info)
        {
            WorldRegion undoRegion = World.GetLayerRegion(info.Bounds, World.SelectedLayer),
                        redoRegion = World.GetLayerRegion(info.Bounds, World.SelectedLayer);

            int selectionWidth = TilesetSelection.Width / TileSize.Width,
                selectionHeight = TilesetSelection.Height / TileSize.Height;

            // <bounds loop>
            for (int boundsX = info.Bounds.X; boundsX < info.Bounds.Right; boundsX += selectionWidth)
            {
                for (int boundsY = info.Bounds.Y; boundsY < info.Bounds.Bottom; boundsY += selectionHeight)
                {
                    // <render loop>
                    for (int renderX = boundsX, sx = 0; renderX < boundsX + selectionWidth; renderX++, sx++)
                    {
                        for (int renderY = boundsY, sy = 0; renderY < boundsY + selectionHeight; renderY++, sy++)
                        {
                            int offsetX = renderX - info.Bounds.X,
                                offsetY = renderY - info.Bounds.Y;

                            // determine if the point from the current selection iteration should be rendered.
                            // must be inside the region bounds and an existing change
                            if (offsetX >= info.Bounds.Width || offsetY >= info.Bounds.Height || !info.Changes[offsetX][offsetY])
                                continue;

                            WorldTile tile = TileFromSelectionOffset(sx, sy);

                            World.SetTile(renderX, renderY, tile);
                            redoRegion[offsetX, offsetY] = tile;
                        }
                    }
                    // </render loop>
                }
            }
            // </bounds loop>

            ChangesUndo = new TileChangesRegion(undoRegion, info.Bounds.Location, World.SelectedLayer);
            ChangesRedo = new TileChangesRegion(redoRegion, info.Bounds.Location, World.SelectedLayer);

            History.Add(PackagedHistoryItem());
        }

        /// <summary>
        /// Computes a fill region and points contained in it based
        /// on the origin
        /// </summary>
        /// <param name="origin"> Initial fill point </param>
        /// <returns> Fill information generated from origin </returns>
        FillInfo computeFillInfo(Point origin)
        {
            FillInfo fillInfo = new FillInfo();
            
            fillInfo.Bounds = new Rectangle(origin.X, origin.Y, origin.X, origin.Y);

            WorldTile targetTile = World.GetTile(origin);

            //queue containing tiles to check
            List<Point> fillQ = new List<Point>();
            fillQ.Add(origin);

            //list that will be pushed with points valid
            //for change. This is used later to populate
            //our flag array
            List<Point> changedPoints = new List<Point>();

            //cache this condition for later optimization
            bool checkRect = World.SelectionRectangle != Rectangle.Empty;

            //iteration bounds. used for optimization
            int boundLeft, boundRight, boundTop, boundBottom;

            //set the bounds depending on the selection rectangle
            if (checkRect)
            {
                boundLeft = World.SelectionRectangle.X.Clamp(0, WorldSize.Width);
                boundRight = World.SelectionRectangle.Right.Clamp(0, WorldSize.Width);
                boundTop = World.SelectionRectangle.Y.Clamp(0, WorldSize.Height);
                boundBottom = World.SelectionRectangle.Bottom.Clamp(0, WorldSize.Height);
            }
            else
            {
                boundLeft = boundTop = 0;
                boundRight = WorldSize.Width;
                boundBottom = WorldSize.Height;
            }

            //array of flags containing tiles already processed for optimization
            bool[][] processedTiles = new bool[WorldSize.Width][];

            for (int i = 0; i < WorldSize.Width; i++)
                processedTiles[i] = new bool[WorldSize.Height];

            //process fill queue until there's no remaining options
            while (fillQ.Count > 0) {
                //take this point from the queue
                Point currentPoint = fillQ[0];
                fillQ.RemoveAt(0);

                //set the current point as a valid change
                changedPoints.Add(currentPoint);
                processedTiles[currentPoint.X][currentPoint.Y] = true;

                //seek left
                int left = currentPoint.X;
                while (--left >= boundLeft && World.GetTile(left, currentPoint.Y) == targetTile); left++;
                    
                //seek right
                int right = currentPoint.X;
                while (++right < boundRight && World.GetTile(right, currentPoint.Y) == targetTile); right--;

                //adjust fill region bounds
                fillInfo.Bounds.X = Math.Min(left, fillInfo.Bounds.X);
                fillInfo.Bounds.Y = Math.Min(currentPoint.Y, fillInfo.Bounds.Y);
                fillInfo.Bounds.Width = Math.Max(right, fillInfo.Bounds.Width);
                fillInfo.Bounds.Height = Math.Max(currentPoint.Y, fillInfo.Bounds.Height);

                //used for optimizing left-right iterations
                bool lAbove = false;
                bool lBelow = false;

                //iterate through row based on calculation and
                //add other potential rows
                for (int x = left; x <= right; x++) {
                    Point fillPoint = new Point(x, currentPoint.Y);

                    changedPoints.Add(fillPoint);

                    //check tile above
                    if (fillPoint.Y > boundTop) {
                        Point pointAbove = new Point(fillPoint.X, fillPoint.Y - 1);

                        if (!processedTiles[pointAbove.X][pointAbove.Y] && World.GetTile(pointAbove) == targetTile)
                        {
                            if (!lAbove)
                                fillQ.Add(pointAbove);

                            lAbove = true;
                        } 
                        else 
                        {
                            lAbove = false;
                        }

                        //declare this tile as processed
                        processedTiles[pointAbove.X][pointAbove.Y] = true;
                    }

                    //check tile below
                    if (fillPoint.Y + 1 < boundBottom)
                    {
                        Point pointBelow = new Point(fillPoint.X, fillPoint.Y + 1);
                        if (!processedTiles[pointBelow.X][pointBelow.Y] && World.GetTile(pointBelow) == targetTile)
                        {
                            if (!lBelow)
                                fillQ.Add(pointBelow);

                            lBelow = true;
                        }
                        else 
                        { 
                            lBelow = false; 
                        }

                        //declare this tile as processed
                        processedTiles[pointBelow.X][pointBelow.Y] = true;
                    }
                }
            }

            //normalize fill region size
            fillInfo.Bounds.Width = fillInfo.Bounds.Width - fillInfo.Bounds.X + 1;
            fillInfo.Bounds.Height = fillInfo.Bounds.Height - fillInfo.Bounds.Y + 1;

            //if there's a selection in the world, determine if we should
            //use it, or the bounds of the changes
            if (checkRect && !World.SelectionRectangle.Contains(fillInfo.Bounds))
            {
                fillInfo.Bounds = World.SelectionRectangle;
            }

            fillInfo.Changes = new bool[fillInfo.Bounds.Width][];

            for (int i = 0; i < fillInfo.Bounds.Width; i++)
                fillInfo.Changes[i] = new bool[fillInfo.Bounds.Height];

            //convert changed tiles into flags for optimized
            //use in applying changes
            foreach (Point change in changedPoints)
            {
                fillInfo.Changes[change.X - fillInfo.Bounds.X]
                                [change.Y - fillInfo.Bounds.Y] = true;
            }

            return fillInfo;
        }

        /// <summary>
        /// Represents fill information when calculating from an origin
        /// </summary>
        struct FillInfo
        {
            public Rectangle Bounds;
            public bool[][] Changes;
        }
    }
}
