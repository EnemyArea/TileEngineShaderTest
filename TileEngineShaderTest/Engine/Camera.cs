using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TileEngineShaderTest.Engine
{
    /// <summary>
    /// </summary>
    public sealed class Camera
    {
        /// <summary>
        /// </summary>
        private int mapWidth;

        /// <summary>
        /// </summary>
        private int mapHeight;

        /// <summary>
        /// </summary>
        private Vector3 positionOffset;

        /// <summary>
        /// </summary>
        private Vector3 positionWithOffset;

        /// <summary>
        ///     Beinhaltet die Matrix mit der gemalt wird
        /// </summary>
        private Matrix invertedViewMatrix;

        /// <summary>
        /// </summary>
        private Matrix invertedOffsetMatrix;

        /// <summary>
        /// </summary>
        private Vector2? focusPosition;

        /// <summary>
        /// </summary>
        private const float MinZoom = float.MinValue;

        /// <summary>
        /// </summary>
        private const float MaxZoom = float.MaxValue;

        /// <summary>
        /// </summary>
        private Matrix offsetMatrix;

        /// <summary>
        /// </summary>
        private Matrix viewMatrix;

        /// <summary>
        /// </summary>
        private Matrix scaleMatrix;

        /// <summary>
        /// </summary>
        private Matrix scaleMatrixWithOffset;


        /// <summary>
        /// </summary>
        public bool LimitZoom { get; set; }

        /// <summary>
        /// </summary>
        public Vector2 Origin { get; private set; }

        /// <summary>
        /// </summary>
        public Viewport Viewport { get; private set; }

        /// <summary>
        /// </summary>
        public float ZoomLevel { get; private set; }

        /// <summary>
        /// </summary>
        public int MinTilePositionX { get; private set; }

        /// <summary>
        /// </summary>
        public int MinTilePositionY { get; private set; }

        /// <summary>
        /// </summary>
        public int MaxTilePositionX { get; private set; }

        /// <summary>
        /// </summary>
        public int MaxTilePositionY { get; private set; }

        /// <summary>
        /// </summary>
        public Rectangle Limits { get; private set; }

        /// <summary>
        /// </summary>
        public Rectangle Bounds { get; private set; }


        /// <summary>
        /// </summary>
        public Vector3 CameraPosition
        {
            get { return this.positionWithOffset - this.positionOffset; }
        }


        /// <summary>
        /// </summary>
        private Vector2 viewOffset;

        /// <summary>
        /// </summary>
        public Vector2 ViewOffset
        {
            get { return this.viewOffset; }
        }


        /// <summary>
        /// </summary>
        private Matrix viewMatrixWithOffset;

        /// <summary>
        /// </summary>
        public Matrix ViewMatrixWithOffset
        {
            get { return this.viewMatrixWithOffset; }
        }


        /// <summary>
        /// </summary>
        public Camera()
        {
            this.SetZoomLevel(1);
            this.LimitZoom = false;
            this.viewOffset = Vector2.Zero;
            this.offsetMatrix = Matrix.Identity;
        }


        /// <summary>
        /// </summary>
        /// <returns></returns>
        private Vector2 GetOffset()
        {
            var mapWidthInPixels = this.Limits.Width;
            var mapHeightInPixels = this.Limits.Height;
            var screenWidthInPixel = this.Viewport.Width;
            var screenHeightInPixel = this.Viewport.Height;

            return new Vector2(
                mapWidthInPixels < screenWidthInPixel ? this.Viewport.Width * 0.5f - mapWidthInPixels * 0.5f : 0,
                mapHeightInPixels < screenHeightInPixel ? this.Viewport.Height * 0.5f - mapHeightInPixels * 0.5f : 0);
        }


        /// <summary>
        /// </summary>
        /// <param name="zoomLevel"></param>
        public void SetZoomLevel(float zoomLevel)
        {
            // Zoomen!
            this.ZoomLevel = 1f * zoomLevel;

            // Scale Matrix
            this.scaleMatrix = Matrix.CreateScale(this.ZoomLevel, this.ZoomLevel, 1f);
        }


        /// <summary>
        /// </summary>
        /// <param name="direction"></param>
        public void Move(Vector2 direction)
        {
            var r = (this.focusPosition ?? new Vector2()) + Vector2.Transform(direction, Matrix.CreateRotationZ(-0));
            this.focusPosition = new Vector2((int)r.X, (int)r.Y);
        }


        /// <summary>
        /// </summary>
        /// <param name="mapWidth"></param>
        /// <param name="mapHeight"></param>
        public void ChangeMapSize(int mapWidth, int mapHeight)
        {
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            this.Limits = new Rectangle(0, 0, mapWidth * GameWorld.TileSize, mapHeight * GameWorld.TileSize);

            // Updaten
            this.viewOffset = this.GetOffset();
            this.offsetMatrix = Matrix.CreateTranslation(new Vector3(this.ViewOffset, 0));
        }


        /// <summary>
        /// </summary>
        private void UpdateMatrix()
        {
            // Den Zielursprung berechnen. Dieser liegt normalerweise in der Mitte des Bildschirms
            // außer, wenn wir ein Ziel Focusieren.
            var targetOrigin = this.focusPosition ?? this.Origin;
            var targetOriginVector = -new Vector3(targetOrigin.X, targetOrigin.Y, 0f);

            // Matrix neuberechnen
            this.viewMatrix = Matrix.CreateTranslation(-this.positionWithOffset) *
                              Matrix.CreateTranslation(targetOriginVector) *
                              this.scaleMatrix *
                              Matrix.CreateTranslation(new Vector3(this.Origin, 0f));

            // Invertieren
            Matrix.Invert(ref this.viewMatrix, out this.invertedViewMatrix);
            Matrix.Invert(ref this.offsetMatrix, out this.invertedOffsetMatrix);

            // Multiplizieren
            Matrix.Multiply(ref this.viewMatrix, ref this.offsetMatrix, out this.viewMatrixWithOffset);
            Matrix.Multiply(ref this.scaleMatrix, ref this.offsetMatrix, out this.scaleMatrixWithOffset);
        }


        /// <summary>
        /// </summary>
        /// <param name="screenPosition"></param>
        /// <returns></returns>
        public Vector2 CalculateScreenPositionToWorldPosition(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition, this.invertedViewMatrix * this.invertedOffsetMatrix);
        }


        /// <summary>
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Point ConvertPositionToTilePosition(Vector2 position)
        {
            return new Point((int)(position.X / GameWorld.TileSize), (int)(position.Y / GameWorld.TileSize));
        }


        /// <summary>
        ///     When using limiting, makes sure the camera position is valid.
        /// </summary>
        public void UpdateCamera()
        {
            // Wir haben dann keinen Offset, weil dieser Zielabhängig berechnet wird
            this.positionWithOffset = new Vector3();

            // Zoom begrenzen?
            if (this.LimitZoom)
            {
                // Zoom validieren
                var minZoomX = (float)this.Viewport.Width / this.Limits.Width;
                var minZoomY = (float)this.Viewport.Height / this.Limits.Height;
                this.ZoomLevel = MathHelper.Clamp(MathHelper.Max(this.ZoomLevel, MathHelper.Max(minZoomX, minZoomY)), MinZoom, MaxZoom);
            }

            // Matrix neuberechnen
            this.UpdateMatrix();

            // Camera Position validieren
            var cameraWorldMin = Vector3.Transform(Vector3.Zero, this.invertedViewMatrix);
            var cameraSize = new Vector3(this.Viewport.Width, this.Viewport.Height, 0) / this.ZoomLevel;
            var limitWorldMin = new Vector3(this.Limits.Left, this.Limits.Top, 0);
            var limitWorldMax = new Vector3(this.Limits.Right, this.Limits.Bottom, 0);

            // Offset speichern
            this.positionOffset = this.positionWithOffset - cameraWorldMin;

            // Neue Position bestimmen
            this.positionWithOffset = Vector3.Clamp(cameraWorldMin, limitWorldMin, limitWorldMax - cameraSize) + this.positionOffset;

            // Matrix neuberechnen, wenn es z.B. korrekturen gab
            this.UpdateMatrix();

            // Sichtbaren Tiles Updaten
            var displayTiles = new Vector3
            {
                X = this.Viewport.Width / this.ZoomLevel + GameWorld.TileSize,
                Y = this.Viewport.Height / this.ZoomLevel + GameWorld.TileSize
            };

            // Neue Position berechnen
            var maxCameraPosition = this.CameraPosition + displayTiles;

            // Min und Max für die For-Schleife berechnen
            this.MinTilePositionX = (int)MathHelper.Max(this.CameraPosition.X / GameWorld.TileSize, 0);
            this.MinTilePositionY = (int)MathHelper.Max(this.CameraPosition.Y / GameWorld.TileSize, 0);
            this.MaxTilePositionX = (int)MathHelper.Min(maxCameraPosition.X / GameWorld.TileSize, this.mapWidth);
            this.MaxTilePositionY = (int)MathHelper.Min(maxCameraPosition.Y / GameWorld.TileSize, this.mapHeight);

            // Bounds updaten
            this.Bounds = new Rectangle(this.MinTilePositionX * GameWorld.TileSize, this.MinTilePositionY * GameWorld.TileSize, this.MaxTilePositionX * GameWorld.TileSize, this.MaxTilePositionY * GameWorld.TileSize);
        }


        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            // Updaten
            this.UpdateCamera();
        }
    }
}