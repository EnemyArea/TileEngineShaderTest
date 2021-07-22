#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using TileEngineShaderTest.Engine;

#endregion

namespace TileEngineShaderTest
{
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private FrameRateCounter frameRateCounter;
        private SpriteBatch spriteBatch;
        private Texture2D tilesetTexture;
        private IndexBuffer indexBuffer;
        private VertexBuffer vertexBuffer;
        private Camera camera;
        private Texture2D textureFromMap1;
        private Texture2D textureFromMap2;
        private Texture2D textureFromMap3;
        private Texture2D textureFromMap4;
        private Effect mapShader;
        private int mapWidth;
        private int mapHeight;
        private List<WorldmapTile[][]> mapTiles;
        private Dictionary<int, int[,]> floorAutotileTable;

        public Game1()
        {
            this.graphics = new GraphicsDeviceManager(this);
            var currentDir = Path.Combine(Directory.GetCurrentDirectory(), "..\\GameResources");
            this.Content.RootDirectory = currentDir;
            this.IsMouseVisible = true;
            this.graphics.SynchronizeWithVerticalRetrace = true;
            this.graphics.PreferredBackBufferWidth = 1280;
            this.graphics.PreferredBackBufferHeight = 768;
            this.graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }


        /// <summary>
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private static IEnumerable<int> CreateIndicies(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var offset = 4 * i;

                yield return (ushort)(offset + 0);
                yield return (ushort)(offset + 1);
                yield return (ushort)(offset + 2);
                yield return (ushort)(offset + 1);
                yield return (ushort)(offset + 3);
                yield return (ushort)(offset + 2);
            }
        }


        /// <summary>
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        private static IEnumerable<VertexPositionColorTexture> RenderTextureFragmentRect(Rectangle destination)
        {
            var fragmentSizeX = destination.Width;
            var fragmentSizeY = destination.Height;
            var offsetX = destination.X;
            var offsetY = destination.Y;

            var vertexPositionTopLeft = new Vector3(offsetX, offsetY, 0);
            var vertexPositionTopRight = new Vector3(offsetX + fragmentSizeX, offsetY, 0);
            var vertexPositionBottomLeft = new Vector3(offsetX, offsetY + fragmentSizeY, 0);
            var vertexPositionBottomRight = new Vector3(offsetX + fragmentSizeX, offsetY + fragmentSizeY, 0);

            yield return new VertexPositionColorTexture(vertexPositionTopLeft, Color.White, new Vector2(0, 0));
            yield return new VertexPositionColorTexture(vertexPositionTopRight, Color.White, new Vector2(1, 0));
            yield return new VertexPositionColorTexture(vertexPositionBottomLeft, Color.White, new Vector2(0, 1));
            yield return new VertexPositionColorTexture(vertexPositionBottomRight, Color.White, new Vector2(1, 1));
        }


        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);

            this.frameRateCounter = new FrameRateCounter(this);
            this.frameRateCounter.LoadContent();

            this.mapTiles = JsonConvert.DeserializeObject<List<WorldmapTile[][]>>(File.ReadAllText("Maps/adelsviertel_main.json"));
            this.tilesetTexture = this.Content.Load<Texture2D>("PandoraAdelsv");

            //this.mapTiles = JsonConvert.DeserializeObject<List<WorldmapTile[][]>>(File.ReadAllText("Maps/valfs_haus.json"));
            //this.tilesetTexture = this.Content.Load<Texture2D>("ValfsHaus");

            this.camera = new Camera();
            this.mapShader = this.Content.Load<Effect>("MapShader");

            this.mapWidth = this.mapTiles[0].Length;
            this.mapHeight = this.mapTiles[0][0].Length;

            this.floorAutotileTable = new Dictionary<int, int[,]>
            {
                { 0, new[,] { { 2, 4 }, { 1, 4 }, { 2, 3 }, { 1, 3 } } },
                { 1, new[,] { { 2, 0 }, { 1, 4 }, { 2, 3 }, { 1, 3 } } },
                { 2, new[,] { { 2, 4 }, { 3, 0 }, { 2, 3 }, { 1, 3 } } },
                { 3, new[,] { { 2, 0 }, { 3, 0 }, { 2, 3 }, { 1, 3 } } },
                { 4, new[,] { { 2, 4 }, { 1, 4 }, { 2, 3 }, { 3, 1 } } },
                { 5, new[,] { { 2, 0 }, { 1, 4 }, { 2, 3 }, { 3, 1 } } },
                { 6, new[,] { { 2, 4 }, { 3, 0 }, { 2, 3 }, { 3, 1 } } },
                { 7, new[,] { { 2, 0 }, { 3, 0 }, { 2, 3 }, { 3, 1 } } },
                { 8, new[,] { { 2, 4 }, { 1, 4 }, { 2, 1 }, { 1, 3 } } },
                { 9, new[,] { { 2, 0 }, { 1, 4 }, { 2, 1 }, { 1, 3 } } },
                { 10, new[,] { { 2, 4 }, { 3, 0 }, { 2, 1 }, { 1, 3 } } },
                { 11, new[,] { { 2, 0 }, { 3, 0 }, { 2, 1 }, { 1, 3 } } },
                { 12, new[,] { { 2, 4 }, { 1, 4 }, { 2, 1 }, { 3, 1 } } },
                { 13, new[,] { { 2, 0 }, { 1, 4 }, { 2, 1 }, { 3, 1 } } },
                { 14, new[,] { { 2, 4 }, { 3, 0 }, { 2, 1 }, { 3, 1 } } },
                { 15, new[,] { { 2, 0 }, { 3, 0 }, { 2, 1 }, { 3, 1 } } },
                { 16, new[,] { { 0, 4 }, { 1, 4 }, { 0, 3 }, { 1, 3 } } },
                { 17, new[,] { { 0, 4 }, { 3, 0 }, { 0, 3 }, { 1, 3 } } },
                { 18, new[,] { { 0, 4 }, { 1, 4 }, { 0, 3 }, { 3, 1 } } },
                { 19, new[,] { { 0, 4 }, { 3, 0 }, { 0, 3 }, { 3, 1 } } },
                { 20, new[,] { { 2, 2 }, { 1, 2 }, { 2, 3 }, { 1, 3 } } },
                { 21, new[,] { { 2, 2 }, { 1, 2 }, { 2, 3 }, { 3, 1 } } },
                { 22, new[,] { { 2, 2 }, { 1, 2 }, { 2, 1 }, { 1, 3 } } },
                { 23, new[,] { { 2, 2 }, { 1, 2 }, { 2, 1 }, { 3, 1 } } },
                { 24, new[,] { { 2, 4 }, { 3, 4 }, { 2, 3 }, { 3, 3 } } },
                { 25, new[,] { { 2, 4 }, { 3, 4 }, { 2, 1 }, { 3, 3 } } },
                { 26, new[,] { { 2, 0 }, { 3, 4 }, { 2, 3 }, { 3, 3 } } },
                { 27, new[,] { { 2, 0 }, { 3, 4 }, { 2, 1 }, { 3, 3 } } },
                { 28, new[,] { { 2, 4 }, { 1, 4 }, { 2, 5 }, { 1, 5 } } },
                { 29, new[,] { { 2, 0 }, { 1, 4 }, { 2, 5 }, { 1, 5 } } },
                { 30, new[,] { { 2, 4 }, { 3, 0 }, { 2, 5 }, { 1, 5 } } },
                { 31, new[,] { { 2, 0 }, { 3, 0 }, { 2, 5 }, { 1, 5 } } },
                { 32, new[,] { { 0, 4 }, { 3, 4 }, { 0, 3 }, { 3, 3 } } },
                { 33, new[,] { { 2, 2 }, { 1, 2 }, { 2, 5 }, { 1, 5 } } },
                { 34, new[,] { { 0, 2 }, { 1, 2 }, { 0, 3 }, { 1, 3 } } },
                { 35, new[,] { { 0, 2 }, { 1, 2 }, { 0, 3 }, { 3, 1 } } },
                { 36, new[,] { { 2, 2 }, { 3, 2 }, { 2, 3 }, { 3, 3 } } },
                { 37, new[,] { { 2, 2 }, { 3, 2 }, { 2, 1 }, { 3, 3 } } },
                { 38, new[,] { { 2, 4 }, { 3, 4 }, { 2, 5 }, { 3, 5 } } },
                { 39, new[,] { { 2, 0 }, { 3, 4 }, { 2, 5 }, { 3, 5 } } },
                { 40, new[,] { { 0, 4 }, { 1, 4 }, { 0, 5 }, { 1, 5 } } },
                { 41, new[,] { { 0, 4 }, { 3, 0 }, { 0, 5 }, { 1, 5 } } },
                { 42, new[,] { { 0, 2 }, { 3, 2 }, { 0, 3 }, { 3, 3 } } },
                { 43, new[,] { { 0, 2 }, { 1, 2 }, { 0, 5 }, { 1, 5 } } },
                { 44, new[,] { { 0, 4 }, { 3, 4 }, { 0, 5 }, { 3, 5 } } },
                { 45, new[,] { { 2, 2 }, { 3, 2 }, { 2, 5 }, { 3, 5 } } },
                { 46, new[,] { { 0, 2 }, { 3, 2 }, { 0, 5 }, { 3, 5 } } },
                { 47, new[,] { { 0, 0 }, { 1, 0 }, { 0, 1 }, { 1, 1 } } }
            };

            this.textureFromMap1 = this.CreateTexture(0);
            this.textureFromMap2 = this.CreateTexture(1);
            this.textureFromMap3 = this.CreateTexture(2);
            this.textureFromMap4 = this.CreateTexture(3);

            var allVertices = RenderTextureFragmentRect(new Rectangle(0, 0, this.mapWidth * GameWorld.TileSize, this.mapHeight * GameWorld.TileSize)).ToArray();
            var indices = CreateIndicies(allVertices.Length / 4).ToArray();
            this.indexBuffer = new IndexBuffer(this.tilesetTexture.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
            this.indexBuffer.SetData(indices);

            this.vertexBuffer = new VertexBuffer(this.tilesetTexture.GraphicsDevice, typeof(VertexPositionColorTexture), allVertices.Length, BufferUsage.WriteOnly);
            this.vertexBuffer.SetData(allVertices);

            // Camera auf Karte begrenzen
            this.camera.ChangeMapSize(this.mapWidth * GameWorld.TileSize, this.mapHeight * GameWorld.TileSize);
        }

        private static int Concat(int a, int b)
        {
            return Convert.ToInt32((a+""+b));
        }

        private Texture2D CreateTexture(int layerIndex)
        {
            var textureData = new Color[this.mapWidth * 2 * this.mapHeight * 2];
            var textureFromMap = new Texture2D(this.GraphicsDevice, this.mapWidth * 2, this.mapHeight * 2);

            var tiles = this.mapTiles[layerIndex];
            for (var mY = 0; mY < this.mapHeight; mY++)
            {
                for (var mX = 0; mX < this.mapWidth; mX++)
                {
                    var worldmapTile = tiles[mX][mY];
                    if (worldmapTile.TextureIndex != -1) //  && mX == 19 && mY == 11
                    {
                        switch (worldmapTile.TileTextureType)
                        {
                            case TileTextureType.TileA1:
                            case TileTextureType.TileA1Waterfall:
                            case TileTextureType.TileA2:
                            case TileTextureType.TileA3:
                            case TileTextureType.TileA4_1:
                            case TileTextureType.TileA4_2:
                            case TileTextureType.TileA4_3:
                            case TileTextureType.TileA4Wall_1:
                            case TileTextureType.TileA4Wall_2:
                            case TileTextureType.TileA4Wall_3:
                                {
                                    //textureData[mY * 2 * this.mapWidth * 2 + mX * 2] = Color.Pink;
                                    //textureData[mY * 2 * this.mapWidth * 2 + mX * 2 + 1] = Color.Pink;
                                    //textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2] = Color.Pink;
                                    //textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2 + 1] = Color.Pink;

                                    //textureData[mY * 2 * this.mapWidth * 2 + mX * 2] = Color.Transparent;
                                    //textureData[mY * 2 * this.mapWidth * 2 + mX * 2 + 1] = Color.Transparent;
                                    //textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2] = Color.Transparent;
                                    //textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2 + 1] = Color.Transparent;


                                    // Zeichnen
                                    var textureMap = this.floorAutotileTable[worldmapTile.AutotileBitMask];

                                    var sourceRectAutotileX1 = textureMap[0, 0];
                                    var sourceRectAutotileY1 = textureMap[0, 1];

                                    var sourceRectAutotileX2 = textureMap[1, 0];
                                    var sourceRectAutotileY2 = textureMap[1, 1];

                                    var sourceRectAutotileX3 = textureMap[2, 0];
                                    var sourceRectAutotileY3 = textureMap[2, 1];

                                    var sourceRectAutotileX4 = textureMap[3, 0];
                                    var sourceRectAutotileY4 = textureMap[3, 1];

                                    var a1 = 100 + Concat(sourceRectAutotileX1, sourceRectAutotileY1);
                                    var a2 = 100 + Concat(sourceRectAutotileX2, sourceRectAutotileY2);
                                    var a3 = 100 + Concat(sourceRectAutotileX3, sourceRectAutotileY3);
                                    var a4 = 100 + Concat(sourceRectAutotileX4, sourceRectAutotileY4);

                                    var deX = worldmapTile.TextureIndexModMulti / 32;
                                    var deY = worldmapTile.TextureIndexDivMulti / 32;
                                  
                                    textureData[mY * 2 * this.mapWidth * 2 + mX * 2] = new Color(deX, deY, a1);
                                    textureData[mY * 2 * this.mapWidth * 2 + mX * 2 + 1] = new Color(deX, deY, a2);
                                    textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2] = new Color(deX, deY, a3);
                                    textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2 + 1] = new Color(deX, deY, a4);


                                    //textureData[mY * 2 * this.mapWidth * 2 + mX * 2] = new Color(worldmapTile.TextureIndexModMulti / 32, worldmapTile.TextureIndexDivMulti / 32, sourceRectAutotileX1, sourceRectAutotileY1);
                                    //textureData[mY * 2 * this.mapWidth * 2 + mX * 2 + 1] = new Color(worldmapTile.TextureIndexModMulti / 32, worldmapTile.TextureIndexDivMulti / 32, sourceRectAutotileX2, sourceRectAutotileY2);
                                    //textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2] = new Color(worldmapTile.TextureIndexModMulti / 32, worldmapTile.TextureIndexDivMulti / 32, sourceRectAutotileX3, sourceRectAutotileY3);
                                    //textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2 + 1] = new Color(worldmapTile.TextureIndexModMulti / 32, worldmapTile.TextureIndexDivMulti / 32, sourceRectAutotileX4, sourceRectAutotileY4);
                                }
                                break;

                            case TileTextureType.TileA5:
                            case TileTextureType.TileB:
                            case TileTextureType.TileC:
                            case TileTextureType.TileD:
                            case TileTextureType.TileE:
                                {
                                    //textureData[mY * 2 * this.mapWidth * 2 + mX * 2] = Color.Pink;
                                    //textureData[mY * 2 * this.mapWidth * 2 + mX * 2 + 1] = Color.Pink;
                                    //textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2] = Color.Pink;
                                    //textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2 + 1] = Color.Pink;

                                    //textureData[mY * 2 * this.mapWidth * 2 + mX * 2] = Color.Transparent;
                                    //textureData[mY * 2 * this.mapWidth * 2 + mX * 2 + 1] = Color.Transparent;
                                    //textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2] = Color.Transparent;
                                    //textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2 + 1] = Color.Transparent;
                                    
                                    var deX = worldmapTile.TextureIndexModMulti / 32;
                                    var deY = worldmapTile.TextureIndexDivMulti / 32;

                                    textureData[mY * 2 * this.mapWidth * 2 + mX * 2] = new Color(deX, deY, 0);
                                    textureData[mY * 2 * this.mapWidth * 2 + mX * 2 + 1] = new Color(deX, deY, 1);
                                    textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2] = new Color(deX, deY, 2);
                                    textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2 + 1] = new Color(deX, deY, 4);

                                    //textureData[mY * 2 * this.mapWidth * 2 + mX * 2] = new Color(worldmapTile.TextureIndexModMulti / 32, worldmapTile.TextureIndexDivMulti / 32, 0, 0);
                                    //textureData[mY * 2 * this.mapWidth * 2 + mX * 2 + 1] = new Color(worldmapTile.TextureIndexModMulti / 32, worldmapTile.TextureIndexDivMulti / 32, 16, 0);
                                    //textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2] = new Color(worldmapTile.TextureIndexModMulti / 32, worldmapTile.TextureIndexDivMulti / 32, 0, 16);
                                    //textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2 + 1] = new Color(worldmapTile.TextureIndexModMulti / 32, worldmapTile.TextureIndexDivMulti / 32, 16, 16);
                                }
                                break;
                        }
                    }
                    else
                    {
                        //textureData[mY * 2 * this.mapWidth * 2 + mX * 2] = Color.Pink;
                        //textureData[mY * 2 * this.mapWidth * 2 + mX * 2 + 1] = Color.Pink;
                        //textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2] = Color.Pink;
                        //textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2 + 1] = Color.Pink;

                        textureData[mY * 2 * this.mapWidth * 2 + mX * 2] = Color.Transparent;
                        textureData[mY * 2 * this.mapWidth * 2 + mX * 2 + 1] = Color.Transparent;
                        textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2] = Color.Transparent;
                        textureData[(mY * 2 + 1) * this.mapWidth * 2 + mX * 2 + 1] = Color.Transparent;
                    }
                }
            }

            textureFromMap.SetData(textureData);

            using (var t = File.Create($"map{layerIndex}.png"))
            {
                textureFromMap.SaveAsPng(t, textureFromMap.Width, textureFromMap.Height);
            }

            return textureFromMap;
        }
        

        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            this.indexBuffer.Dispose();
            this.vertexBuffer.Dispose();
            this.mapShader.Dispose();
            this.textureFromMap1?.Dispose();
            this.textureFromMap2?.Dispose();
            this.textureFromMap3?.Dispose();
            this.tilesetTexture.Dispose();
            this.spriteBatch.Dispose();
        }


        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            this.frameRateCounter.StartUpdateTimer();

            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyboardState = Keyboard.GetState();

            var moveDirection = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
                moveDirection -= Vector2.UnitY;

            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                moveDirection -= Vector2.UnitX;

            if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
                moveDirection += Vector2.UnitY;

            if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                moveDirection += Vector2.UnitX;

            // need to normalize the direction vector incase moving diagonally, but can't normalize the zero vector
            // however, the zero vector means we didn't want to move this frame anyways so all good
            var isCameraMoving = moveDirection != Vector2.Zero;
            if (isCameraMoving)
            {
                moveDirection.Normalize();
                this.camera.Move(moveDirection * 500f * deltaSeconds);
            }

            this.camera.Update(gameTime);

            base.Update(gameTime);
            this.frameRateCounter.EndUpdateTimer(gameTime);
        }


        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            this.frameRateCounter.StartDrawTimer();
            this.GraphicsDevice.Clear(Color.CornflowerBlue);

            this.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            this.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            this.GraphicsDevice.SetVertexBuffer(this.vertexBuffer);
            this.GraphicsDevice.Indices = this.indexBuffer;

            this.mapShader.Parameters["MapWidthInTiles"].SetValue(this.mapWidth);
            this.mapShader.Parameters["MapHeightInTiles"].SetValue(this.mapHeight);
            this.mapShader.Parameters["TilesetTexture"]?.SetValue(this.tilesetTexture);
            this.mapShader.Parameters["View"].SetValue(this.camera.ViewMatrixWithOffset);
            this.mapShader.Parameters["World"].SetValue(Matrix.Identity);
            this.mapShader.Parameters["ColorKey"].SetValue(Color.Pink.ToVector4());
            this.mapShader.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(0, this.GraphicsDevice.Viewport.Width, this.GraphicsDevice.Viewport.Height, 0, 0, -1));

            if (this.textureFromMap1 != null)
            {
                this.mapShader.Parameters["MapTexture"]?.SetValue(this.textureFromMap1);
                foreach (var pass in this.mapShader.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    this.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, this.vertexBuffer.VertexCount);
                }
            }

            if (this.textureFromMap2 != null)
            {
                this.mapShader.Parameters["MapTexture"]?.SetValue(this.textureFromMap2);
                foreach (var pass in this.mapShader.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    this.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, this.vertexBuffer.VertexCount);
                }
            }

            if (this.textureFromMap3 != null)
            {
                this.mapShader.Parameters["MapTexture"]?.SetValue(this.textureFromMap3);
                foreach (var pass in this.mapShader.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    this.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, this.vertexBuffer.VertexCount);
                }
            }

            if (this.textureFromMap4 != null)
            {
                this.mapShader.Parameters["MapTexture"]?.SetValue(this.textureFromMap4);
                foreach (var pass in this.mapShader.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    this.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, this.vertexBuffer.VertexCount);
                }
            }

            base.Draw(gameTime);
            this.frameRateCounter.EndDrawTimer(gameTime);
        }
    }
}