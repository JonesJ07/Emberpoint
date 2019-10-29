﻿using Emberpoint.Core.Objects.Abstracts;
using Emberpoint.Core.Objects.Interfaces;
using GoRogue.MapViews;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Console = SadConsole.Console;

namespace Emberpoint.Core.Objects
{
    public class EmberGrid : IRenderable
    {
        private EmberCell[] _cells;
        private EmberCell[] Cells
        {
            get
            {
                return _cells ?? (_cells = Blueprint.GetCells());
            }
        }

        private ArrayMap<bool> _fieldOfView;
        public ArrayMap<bool> FieldOfView
        {
            get
            {
                if (_fieldOfView != null) return _fieldOfView;
                _fieldOfView = new ArrayMap<bool>(GridSizeX, GridSizeY);
                for (int x = 0; x < GridSizeX; x++)
                {
                    for (int y = 0; y < GridSizeY; y++)
                    {
                        _fieldOfView[x, y] = GetCell(x, y).Walkable;
                    }
                }
                return _fieldOfView;
            }
        }

        public int GridSizeX { get; }
        public int GridSizeY { get; }

        public Blueprint<EmberCell> Blueprint { get; }

        public EmberGrid(Blueprint<EmberCell> blueprint)
        {
            GridSizeX = blueprint.GridSizeX;
            GridSizeY = blueprint.GridSizeY;
            Blueprint = blueprint;
        }

        public EmberGrid(int gridSizeX, int gridSizeY, EmberCell[] cells)
        {
            GridSizeX = gridSizeX;
            GridSizeY = gridSizeY;
            _cells = cells;

            _fieldOfView = new ArrayMap<bool>(gridSizeX, gridSizeY);
            for (int x = 0; x < GridSizeX; x++)
            {
                for (int y = 0; y < GridSizeY; y++)
                {
                    _fieldOfView[x, y] = GetCell(x, y).Walkable;
                }
            }
        }

        public EmberCell GetCell(Point position)
        {
            return Cells[position.Y * GridSizeX + position.X].Clone();
        }

        public EmberCell GetCell(int x, int y)
        {
            return Cells[y * GridSizeX + x].Clone();
        }

        /// <summary>
        /// Use this when updating multiple cells at a time for performance.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private EmberCell GetNonClonedCell(int x, int y)
        {
            return Cells[y * GridSizeX + x];
        }

        public void SetCell(EmberCell cell)
        {
            var originalCell = Cells[cell.Position.Y * GridSizeX + cell.Position.X];

            // Update the map fov values if the walkable is changed
            bool updateFieldOfView = originalCell.Walkable != cell.Walkable;

            // Copy the new cell data
            originalCell.CopyFrom(cell);
            if (updateFieldOfView)
            {
                UpdateFieldOfView(cell.Position.X, cell.Position.Y);

                // Recalculate the fov of all entities
                var entities = EntityManager.GetEntities<IEntity>();
                foreach (var entity in entities.Where(a => a.FieldOfViewRadius > 0))
                {
                    entity.FieldOfView.Calculate(entity.Position, entity.FieldOfViewRadius);
                    if (entity is Player)
                        DrawFieldOfView(entity);
                }
            }
        }

        public void RenderObject(Console console)
        {
            console.SetSurface(Cells, GridSizeX, GridSizeY);
        }

        /// <summary>
        /// Call this to re-render the map
        /// </summary>
        /// <param name="cell"></param>
        public void RedrawMap()
        {
            Game.Map.IsDirty = true;
        }

        /// <summary>
        /// Updates the FieldOfView data for this cell.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void UpdateFieldOfView(int x, int y)
        {
            FieldOfView[x, y] = GetNonClonedCell(x, y).Walkable;
        }

        /// <summary>
        /// Updates the FieldOfView data for the entire grid.
        /// </summary>
        public void UpdateFieldOfView()
        {
            for (int x = 0; x < GridSizeX; x++)
            {
                for (int y = 0; y < GridSizeY; y++)
                {
                    FieldOfView[x, y] = GetNonClonedCell(x, y).Walkable;
                }
            }
        }

        public void DrawFieldOfView(IEntity entity)
        {
            for (int x = 0; x < GridSizeX; x++)
            {
                for (int y = 0; y < GridSizeY; y++)
                {
                    var cell = GetNonClonedCell(x, y);
                    if (entity.FieldOfView.BooleanFOV[x,y])
                    {
                        cell.Foreground = cell.NormalForeground;                       
                    }
                    else
                    {
                        cell.Foreground = cell.ForegroundFov;
                    }
                    SetCell(cell);
                }
            }
            RedrawMap();
        }

        public EmberCell[] GetNeighbors(EmberCell cell)
        {
            int x = cell.Position.X;
            int y = cell.Position.Y;

            var points = new List<Point>();
            if (!InBounds(x, y)) return Array.Empty<EmberCell>();

            if (InBounds(x + 1, y)) points.Add(new Point(x + 1, y));
            if (InBounds(x - 1, y)) points.Add(new Point(x - 1, y));
            if (InBounds(x, y + 1)) points.Add(new Point(x, y + 1));
            if (InBounds(x, y - 1)) points.Add(new Point(x, y - 1));
            if (InBounds(x + 1, y + 1)) points.Add(new Point(x + 1, y + 1));
            if (InBounds(x - 1, y - 1)) points.Add(new Point(x - 1, y - 1));
            if (InBounds(x + 1, y - 1)) points.Add(new Point(x + 1, y - 1));
            if (InBounds(x - 1, y + 1)) points.Add(new Point(x - 1, y + 1));

            return points.Select(a => GetCell(a)).ToArray();
        }

        public bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < GridSizeX && y < GridSizeY;
        }

        public bool InBounds(Point position)
        {
            return position.X >= 0 && position.Y >= 0 && position.X < GridSizeX && position.Y < GridSizeY;
        }
    }
}
