﻿using Emberpoint.Core.Extensions;
using Emberpoint.Core.GameObjects.Interfaces;
using Emberpoint.Core.GameObjects.Managers;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Emberpoint.Core.GameObjects.Map
{
    public class EmberCell : ColoredGlyph, ILightable, IEquatable<EmberCell>
    {
        public EmberCellProperties CellProperties { get; set; }
        public LightEngineProperties LightProperties { get; set; }
        public EmberEffectProperties EffectProperties { get; set; }

        public Point Position { get; set; }

        public EmberCell()
        {
            CellProperties = new EmberCellProperties
            {
                NormalForeground = Color.White,
                NormalBackground = Color.Black,
                ForegroundFov = Color.Lerp(Color.White, Color.Black, .5f),
                BackgroundFov = Color.Black,
                Walkable = true,
                Interactable = false,
                BlocksFov = false,
                IsExplored = false
            };

            Glyph = ' ';
            LightProperties = new LightEngineProperties();
            EffectProperties = new EmberEffectProperties();
            Foreground = CellProperties.NormalForeground;
            Background = CellProperties.NormalBackground;
        }

        public EmberCell(Point position, int glyph, Color foreground, Color fov)
        {
            CellProperties = new EmberCellProperties
            {
                NormalForeground = foreground,
                NormalBackground = Color.Black,
                ForegroundFov = fov,
                BackgroundFov = Color.Black,
                Walkable = true,
                Interactable = false,
                BlocksFov = false,
                IsExplored = false
            };
            
            Position = position;
            Glyph = glyph;
            Foreground = foreground;
            Background = Color.Black;
            LightProperties = new LightEngineProperties();
            EffectProperties = new EmberEffectProperties();
        }

        public EmberCell(Point position, int glyph, Color foreground, Color foregroundFov, Color background, Color backgroundFov)
        {
            CellProperties = new EmberCellProperties
            {
                NormalForeground = foreground,
                NormalBackground = background,
                ForegroundFov = foregroundFov,
                BackgroundFov = backgroundFov,
                Walkable = true,
                Interactable = false,
                BlocksFov = false,
                IsExplored = false
            };

            Position = position;
            Glyph = glyph;
            Foreground = foreground;
            Background = background;
            LightProperties = new LightEngineProperties();
            EffectProperties = new EmberEffectProperties();
        }

        public void CopyFrom(EmberCell cell)
        {
            // If we passed the same reference, we don't need to do anything
            if (cell == this) return;

            // Does foreground, background, glyph, mirror, decorators
            CopyAppearanceFrom(cell);

            // Base properties
            Position = cell.Position;
            // Ember cell properties
            CellProperties.Name = cell.CellProperties.LocalizedName;
            CellProperties.NormalForeground = cell.CellProperties.NormalForeground;
            CellProperties.NormalBackground = cell.CellProperties.NormalBackground;
            CellProperties.ForegroundFov = cell.CellProperties.ForegroundFov;
            CellProperties.Interactable = cell.CellProperties.Interactable;
            CellProperties.BackgroundFov = cell.CellProperties.BackgroundFov;
            CellProperties.Walkable = cell.CellProperties.Walkable;
            CellProperties.BlocksFov = cell.CellProperties.BlocksFov;
            CellProperties.IsExplored = cell.CellProperties.IsExplored;

            // Light engine properties
            LightProperties.Brightness = cell.LightProperties.Brightness;
            LightProperties.LightRadius = cell.LightProperties.LightRadius;
            LightProperties.EmitsLight = cell.LightProperties.EmitsLight;
            LightProperties.LightColor = cell.LightProperties.LightColor;
            LightProperties.LightSources = cell.LightProperties.LightSources;

            // Ember Effect Properties
            if (cell.EffectProperties.EntityMovementEffects != null)
            {
                EffectProperties.ClearMovementEffects();
                EffectProperties.AddMovementEffects(cell.EffectProperties.EntityMovementEffects);
            }
        }

        public new EmberCell Clone()
        {
            var cell = new EmberCell()
            {
                CellProperties = new EmberCellProperties()
                {
                    Name = this.CellProperties.LocalizedName,
                    NormalForeground = this.CellProperties.NormalForeground,
                    NormalBackground = this.CellProperties.NormalBackground,
                    ForegroundFov = this.CellProperties.ForegroundFov,
                    BackgroundFov = this.CellProperties.BackgroundFov,
                    Walkable = this.CellProperties.Walkable,
                    Interactable = this.CellProperties.Interactable,
                    BlocksFov = this.CellProperties.BlocksFov,
                    IsExplored = this.CellProperties.IsExplored
                },

                Position = this.Position,

                LightProperties = new LightEngineProperties
                {
                    Brightness = this.LightProperties.Brightness,
                    LightRadius = this.LightProperties.LightRadius,
                    EmitsLight = this.LightProperties.EmitsLight,
                    LightColor = this.LightProperties.LightColor,
                    LightSources = this.LightProperties.LightSources
                },

                EffectProperties = new EmberEffectProperties()
            };

            // Add effects
            if (EffectProperties.EntityMovementEffects != null)
            {
                cell.EffectProperties.AddMovementEffects(EffectProperties.EntityMovementEffects);
            }

            // Does foreground, background, glyph, mirror, decorators
            CopyAppearanceTo(cell);
            return cell;
        }

        public EmberCell GetClosestLightSource()
        {
            // Return itself when this is a light source.
            if (LightProperties.EmitsLight) return this;
            if (LightProperties.LightSources == null) return null;
            if (LightProperties.LightSources.Count == 1) return LightProperties.LightSources[0];
            EmberCell closest = null;
            float smallestDistance = float.MaxValue;
            foreach (var source in LightProperties.LightSources)
            {
                var sqdistance = source.Position.SquaredDistance(Position);
                if (smallestDistance > sqdistance)
                {
                    smallestDistance = sqdistance;
                    closest = source;
                }
            }

            return closest;
        }

        public bool ContainsEntity(int blueprintId)
        {
            return EntityManager.EntityExistsAt(Position, blueprintId);
        }

        public bool Equals(EmberCell other)
        {
            return other.Position.Equals(Position);
        }

        public override bool Equals(object obj)
        {
            return Equals((EmberCell)obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[EmberCell Information]");
            sb.AppendLine($"<Position>: [X] {Position.X} [Y] {Position.Y}");
            sb.AppendLine();
            sb.AppendLine($"<Glyph>: {Glyph}");
            sb.AppendLine($"<Foreground>: {Foreground}");
            sb.AppendLine($"<Background>: {Background}");
            sb.AppendLine("[CellProperties]");
            sb.AppendLine($"<Name>: {CellProperties.Name}");
            sb.AppendLine($"<Walkable>: {CellProperties.Walkable}");
            sb.AppendLine($"<Interactable>: {CellProperties.Interactable}");
            sb.AppendLine($"<BlocksFov>: {CellProperties.BlocksFov}");
            sb.AppendLine($"<IsExplored>: {CellProperties.IsExplored}");
            sb.AppendLine();
            sb.AppendLine("[LightProperties]");
            sb.AppendLine($"<EmitsLight>: {LightProperties.EmitsLight}");
            sb.AppendLine($"<Brightness>: {LightProperties.Brightness}");
            sb.AppendLine($"<LightRadius>: {LightProperties.LightRadius}");
            sb.AppendLine($"<LightColor>: {LightProperties.LightColor}");
            if (LightProperties.LightSources != null)
            {
                sb.AppendLine("<<LightSources>>");
                foreach (var lightSource in LightProperties.LightSources)
                {
                    sb.AppendLine($"<LightSource>: [X] {lightSource.Position.X} [Y] {lightSource.Position.Y}");
                }
            }
            sb.AppendLine();
            sb.AppendLine("[EffectProperties]");
            sb.AppendLine($"<MovementEffects>: {(EffectProperties.EntityMovementEffects != null ? EffectProperties.EntityMovementEffects.Count : 0)}");
            sb.AppendLine("[---------------------]");
            return sb.ToString();
        }

        public class LightEngineProperties
        {
            public List<EmberCell> LightSources { get; set; }
            public float Brightness { get; set; }
            public int LightRadius { get; set; }
            public bool EmitsLight { get; set; }
            public Color LightColor { get; set; }
        }

        public class EmberCellProperties
        {
            public bool Walkable { get; set; }
            public bool Interactable { get; set; }

            public string LocalizedName { get; private set; }
            public string Name
            {
                get 
                {
                    // Check if this name can be found within the localization resources
                    return Constants.ResourceHelper.ReadProperty(LocalizedName, LocalizedName ?? ""); 
                }
                set 
                {
                    LocalizedName = value; 
                }
            }

            public Color NormalForeground { get; set; }
            public Color ForegroundFov { get; set; }
            public Color NormalBackground { get; set; }
            public Color BackgroundFov { get; set; }
            public bool BlocksFov { get; set; }
            public bool IsExplored { get; set; }
        }

        public class EmberEffectProperties
        {
            private List<Action<IEntity>> _entityMovementEffects;
            public IReadOnlyList<Action<IEntity>> EntityMovementEffects
            {
                get { return _entityMovementEffects; }
            }

            public void AddMovementEffect(Action<IEntity> action, int? insertIndex = null)
            {
                if (_entityMovementEffects == null) 
                    _entityMovementEffects = new List<Action<IEntity>>();

                if (insertIndex != null)
                    _entityMovementEffects.Insert(insertIndex.Value, action);
                else
                    _entityMovementEffects.Add(action);
            }

            public void AddMovementEffects(IEnumerable<Action<IEntity>> actions)
            {
                if (_entityMovementEffects == null)
                    _entityMovementEffects = new List<Action<IEntity>>();
                _entityMovementEffects.AddRange(actions);
            }

            public void ClearMovementEffects()
            {
                _entityMovementEffects = null;
            }
        }
    }
}
