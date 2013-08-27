#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace LD27.Mono.Linux
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        private static LD27Game game;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            game = new LD27Game();
            game.Run();
        }
    }
}
