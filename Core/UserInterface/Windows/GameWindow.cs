﻿using Emberpoint.Core.GameObjects.Interfaces;
using Emberpoint.Core.Resources;
using SadConsole;

namespace Emberpoint.Core.UserInterface.Windows
{
    public class GameWindow : Console, IUserInterface
    {
        public GameWindow(int width, int height) : base(width, height)
        {
            // Set the XNA container's title
            Settings.WindowTitle = Strings.GameTitle;

            // Print the game title at the  top
            Surface.Print((int)System.Math.Round((Width / 2) / 1.5f) - Resources.Strings.GameTitle.Length / 2, 1, Resources.Strings.GameTitle);

            // Set the current screen to the game window
            GameHost.Instance.Screen = this;
        }

        public Console Console
        {
            get { return this; }
        }

        public void Refresh()
        {
            Surface.Clear();
            Settings.WindowTitle = Resources.Strings.GameTitle;
            Surface.Print((int)System.Math.Round((Width / 2) / 1.5f) - Resources.Strings.GameTitle.Length / 2, 1, Resources.Strings.GameTitle);
        }
    }
}
