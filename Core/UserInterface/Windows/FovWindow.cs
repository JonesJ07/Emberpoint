﻿using Emberpoint.Core.Extensions;
using Emberpoint.Core.GameObjects.Abstracts;
using Emberpoint.Core.GameObjects.Entities;
using Emberpoint.Core.GameObjects.Interfaces;
using Emberpoint.Core.GameObjects.Managers;
using Emberpoint.Core.Resources;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using Console = SadConsole.Console;

namespace Emberpoint.Core.UserInterface.Windows
{
    public class FovWindow : Console, IUserInterface
    {
        public Console Console => this;

        private readonly int _maxLineRows;
        private readonly Console _textConsole;
        //Recreated with ReinitializeCharObjects()
        private Dictionary<char, CharObj> _charObjects;
        private readonly Dictionary<char, BlueprintTile> _blueprintTiles;

        public FovWindow(int width, int height) : base(width, height)
        {
            this.DrawBorders(width, height, "O", "|", "-", Color.Gray);
            Surface.Print(3, 0, Strings.ObjectsInView, Color.Orange);
            _charObjects = new Dictionary<char, CharObj>();
            _blueprintTiles = Blueprint.GetTilesFromConfig();
            _maxLineRows = Height - 2;

            _textConsole = new Console(Width - 2, Height - 2)
            {
                Position = new Point(2, 1),
            };

            Position = new Point(Constants.Map.Width + 7, 3 + 25);

            Children.Add(_textConsole);
            GameHost.Instance.Screen.Children.Add(this);
        }
       
        private void ReinitializeCharObjects(IEnumerable<char> characters, bool updateText = true)
        {
            _charObjects = new Dictionary<char, CharObj>(GetCharObjectPairs(characters));

            if (updateText)
                UpdateText();
        }

        private IEnumerable<KeyValuePair<char, CharObj>> GetCharObjectPairs(IEnumerable<char> characters)
        {
            foreach (var character in characters)
            {
                if (!_blueprintTiles.TryGetValue(character, out var tile) || tile.Name == null) continue;
                var glyphColor = MonoGameExtensions.GetColorByString(tile.Foreground);
                if (glyphColor.A == 0) continue; // Don't render transparent tiles on the fov window
                yield return new KeyValuePair<char, CharObj>(character, new CharObj(tile.Glyph, glyphColor, () => Constants.ResourceHelper.ReadProperty(tile.Name, tile.Name ?? "")));
            }
        }

        private void UpdateText()
        {
            _textConsole.Clear();
            _textConsole.Cursor.Position = new Point(0, 0);

            var orderedValues = _charObjects.OrderBy(x => x.Key).Select(pair => pair.Value);

            foreach (var charObj in orderedValues.Take(_maxLineRows - 1))
                DrawCharObj(charObj);

            if (_charObjects.Count > _maxLineRows)
                _textConsole.Cursor.Print("<More Objects..>");
        }

        private void DrawCharObj(CharObj charObj)
        {
            _textConsole.Cursor.Print(new ColoredString("[" + charObj.Glyph + "]:", charObj.GlyphColor, Color.Transparent));
            _textConsole.Cursor.Print(' ' + charObj.Name());
            _textConsole.Cursor.CarriageReturn();
            _textConsole.Cursor.LineFeed();
        }

        public void Update(IEntity entity)
        {
            int radius = entity is Player ? Constants.Player.FieldOfViewRadius : entity.FieldOfViewRadius;
            var farBrightCells = GetBrightCellsInFov(entity, radius + 3);

            // Gets cells player can see after FOV refresh.
            var cells = GridManager.Grid.GetExploredCellsInFov(entity)
                .Select(a => (char)a.Glyph)
                //Merge in bright cells before FOV refresh.
                .Union(farBrightCells)
                //Take only unique cells as an array.
                .Distinct();

            // Draw visible cells to the FOV window
            ReinitializeCharObjects(characters: cells, updateText: false);
            UpdateText();
        }

        public void Refresh()
        {
            Surface.Clear();
            this.DrawBorders(Width, Height, "O", "|", "-", Color.Gray);
            Surface.Print(3, 0, Strings.ObjectsInView, Color.Orange);

            if (Game.Player != null)
                Update(Game.Player);
        }

        private IEnumerable<char> GetBrightCellsInFov(IEntity entity, int fovRadius)
        {
            return GridManager.Grid.GetExploredCellsInFov(entity, fovRadius)
                .Where(a => a.LightProperties.Brightness > 0f)
                .Select(a => (char)a.Glyph);
        }

        private readonly struct CharObj
        {
            public readonly char Glyph;
            public readonly Color GlyphColor;
            public readonly Func<string> Name;

            public CharObj(char glyph, Color glyphColor, Func<string> name)
            {
                Glyph = glyph;
                GlyphColor = glyphColor;
                Name = name;
            }
        }

    }
}
