using Microsoft.Xna.Framework.Graphics;

namespace TileEngineShaderTest.Engine
{
    /// <summary>
    /// </summary>
    public sealed class Tile
    {
        /// <summary>
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// </summary>
        public VertexPositionTexture[] Vertices { get; set; }

        public int Row { get; set; }
        public int Column { get; set; }


        /// <summary>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Tile(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}