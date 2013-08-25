using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Threading;
using System.IO;

namespace LD27
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class LD27Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Room[,] Rooms = new Room[4,4];
        List<Door> Doors = new List<Door>();

        Hero gameHero;
        Camera gameCamera;

        EnemyController enemyController;
        ProjectileController projectileController;
        ParticleController particleController;
        BombController bombController;

        BasicEffect drawEffect;

        SpriteFont font;
        Texture2D roomIcon;

        MouseState lms;
        KeyboardState lks;
        GamePadState lgs;

        VoxelSprite tileSheet;
        VoxelSprite doorSheet;
        VoxelSprite objectSheet;

        int exitRoomX;
        int exitRoomY;

        Room currentRoom;

        int generatedPercent = 0;

        double doorCountdown = 0;
        double doorCountdownTarget = 10000; // Ten Seconds!

        int roomMovesLeft = 0;
        RoomShift roomShift = null;

        RoomState roomState = RoomState.RoomsShifting;

        public LD27Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            IsMouseVisible = true;
            graphics.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            AudioController.LoadContent(Content);

            tileSheet = new VoxelSprite(16, 16, 16);
            LoadVoxels.LoadSprite(Path.Combine(Content.RootDirectory, "tiles.vxs"), ref tileSheet);
            doorSheet = new VoxelSprite(16,16,16);
            LoadVoxels.LoadSprite(Path.Combine(Content.RootDirectory, "door.vxs"), ref doorSheet);
            objectSheet = new VoxelSprite(16, 16, 16);
            LoadVoxels.LoadSprite(Path.Combine(Content.RootDirectory, "dynamic.vxs"), ref objectSheet);

            gameCamera = new Camera(GraphicsDevice, GraphicsDevice.Viewport);
            particleController = new ParticleController(GraphicsDevice);
            projectileController = new ProjectileController(GraphicsDevice);
            bombController = new BombController(GraphicsDevice, objectSheet);
            enemyController = new EnemyController(GraphicsDevice);

            projectileController.LoadContent(Content);
            enemyController.LoadContent(Content);

            drawEffect = new BasicEffect(GraphicsDevice)
            {
                World = gameCamera.worldMatrix,
                View = gameCamera.viewMatrix,
                Projection = gameCamera.projectionMatrix,
                VertexColorEnabled = true,
            };

            gameHero = new Hero(0, 0, Vector3.Zero);
            gameHero.LoadContent(Content, GraphicsDevice);

            ThreadPool.QueueUserWorkItem(delegate { CreateRoomsAsync(); });

            Doors.Add(new Door(VoxelWorld.ToScreenSpace((7 * 16) + 7, 7, 21) + new Vector3(Voxel.HALF_SIZE,Voxel.HALF_SIZE,Voxel.HALF_SIZE), 0, doorSheet));
            Doors.Add(new Door(VoxelWorld.ToScreenSpace((14 * 16) + 7, (4 * 16) + 7, 21) + new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), 1, doorSheet));
            Doors.Add(new Door(VoxelWorld.ToScreenSpace((7 * 16) + 7, (8 * 16) + 7, 21) + new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), 2, doorSheet));
            Doors.Add(new Door(VoxelWorld.ToScreenSpace(7, (4 * 16) + 7, 21) + new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), 3, doorSheet));

            roomIcon = Content.Load<Texture2D>("roomicon");
            font = Content.Load<SpriteFont>("font");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            if (generatedPercent >= 100)
            {
                MouseState cms = Mouse.GetState();
                KeyboardState cks = Keyboard.GetState();
                GamePadState cgs = GamePad.GetState(PlayerIndex.One);

                Vector2 mp2D = Vector2.Clamp(new Vector2(cms.X, cms.Y), Vector2.Zero, new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
                Vector3 mousePos = Helper.ProjectMousePosition(mp2D, GraphicsDevice.Viewport, gameCamera.worldMatrix, gameCamera.viewMatrix, gameCamera.projectionMatrix, 0f);

                Vector2 virtualJoystick = Vector2.Zero;
                if (cks.IsKeyDown(Keys.W) || cks.IsKeyDown(Keys.Up)) virtualJoystick.Y = -1;
                if (cks.IsKeyDown(Keys.S) || cks.IsKeyDown(Keys.Left)) virtualJoystick.X = -1;
                if (cks.IsKeyDown(Keys.A) || cks.IsKeyDown(Keys.Down)) virtualJoystick.Y = 1;
                if (cks.IsKeyDown(Keys.D) || cks.IsKeyDown(Keys.Right)) virtualJoystick.X = 1;
                if(virtualJoystick.Length()>0f) virtualJoystick.Normalize();
                if (cgs.ThumbSticks.Left.Length() > 0.1f)
                {
                    virtualJoystick = cgs.ThumbSticks.Left;
                    virtualJoystick.Y = -virtualJoystick.Y;
                }

                gameHero.Move(virtualJoystick);

                if ((cks.IsKeyDown(Keys.Space) && !lks.IsKeyDown(Keys.Space)) || (cgs.Buttons.B==ButtonState.Pressed && lgs.Buttons.B!=ButtonState.Pressed)) gameHero.TryPlantBomb(currentRoom);
                if (cks.IsKeyDown(Keys.Z) || cks.IsKeyDown(Keys.Enter) || cgs.Buttons.A == ButtonState.Pressed) gameHero.DoAttack();

                if (cks.IsKeyDown(Keys.X) || cks.IsKeyDown(Keys.RightShift) || cgs.Buttons.X == ButtonState.Pressed) gameHero.DoDefend(true, virtualJoystick); else gameHero.DoDefend(false, virtualJoystick);
                


                int openCount = 0;
                foreach (Door d in Doors) if (d.IsOpen) openCount++;

#region ROOM STATE SHIT
                switch (roomState)
                {
                    case RoomState.DoorsOpening:
                        OpenDoors();
                        if (openCount > 0) roomState = RoomState.DoorsOpen;
                        doorCountdown = doorCountdownTarget;
                        break;
                    case RoomState.DoorsOpen:
                        if (doorCountdown > 0)
                        {
                            doorCountdown -= gameTime.ElapsedGameTime.TotalMilliseconds;

                            if (doorCountdown <= 0)
                            {
                                roomState = RoomState.DoorsClosing;
                            }
                        }
                        break;
                    case RoomState.DoorsClosing:
                        foreach (Door d in Doors) d.Close(false);
                        if (openCount == 0)
                        {
                            roomMovesLeft = 3 + Helper.Random.Next(5);
                            DoRoomShift();
                            roomState = RoomState.RoomsShifting;
                        }
                        break;
                    case RoomState.RoomsShifting:
                        foreach (Door d in Doors) d.Close(true);
                        if (roomShift != null)
                        {
                            roomShift.Update(gameTime, gameHero, ref Rooms);
                            if (roomShift.Complete)
                            {
                                if (roomMovesLeft > 0) DoRoomShift();
                                else roomShift = null;
                            }
                        }
                        if (roomShift == null && roomMovesLeft == 0)
                        {
                            roomState = RoomState.DoorsOpening;
                        }
                        break;
                }
#endregion

                if (roomShift!=null)
                    gameCamera.Update(gameTime, currentRoom.World, roomShift.cameraShake);
                else
                    gameCamera.Update(gameTime, currentRoom.World, Vector3.Zero);

                foreach(Room r in Rooms)
                    if(r.World!=null) r.World.Update(gameTime, gameCamera, currentRoom==r);
                //currentRoom.World.Update(gameTime, gameCamera);

                gameHero.Update(gameTime, gameCamera, currentRoom, Doors, ref Rooms);
                currentRoom = Rooms[gameHero.RoomX, gameHero.RoomY];
                currentRoom.Update(gameTime);

                enemyController.Update(gameTime, gameCamera, currentRoom, gameHero, Doors);
                particleController.Update(gameTime, gameCamera, currentRoom.World);
                projectileController.Update(gameTime, gameCamera, currentRoom);
                bombController.Update(gameTime, currentRoom, gameHero);

                foreach (Door d in Doors) d.Update(gameTime);

                drawEffect.View = gameCamera.viewMatrix;
                drawEffect.World = gameCamera.worldMatrix;

                lms = cms;
                lks = cks;
                lgs = cgs;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (generatedPercent < 100)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(font, "generating: " + generatedPercent, new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) / 2, Color.DarkGray, 0f, font.MeasureString("generating: " + generatedPercent) / 2, 1f, SpriteEffects.None, 1);
                spriteBatch.End();

                base.Draw(gameTime);
                return;                
            }

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            foreach (EffectPass pass in drawEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                for (int y = 0; y < currentRoom.World.Y_CHUNKS; y++)
                {
                    for (int x = 0; x < currentRoom.World.X_CHUNKS; x++)
                    {
                        Chunk c = currentRoom.World.Chunks[x, y, 0];
                        if (!c.Visible) continue;

                        if (c == null || c.VertexArray.Length == 0) continue;
                        if (!gameCamera.boundingFrustum.Intersects(c.boundingSphere)) continue;
                        GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, c.VertexArray, 0, c.VertexArray.Length, c.IndexArray, 0, c.VertexArray.Length / 2);
                    }
                }
            }

            currentRoom.Draw(GraphicsDevice, gameCamera, drawEffect);

            foreach (Door d in Doors) d.Draw(GraphicsDevice, gameCamera, drawEffect);
            enemyController.Draw(gameCamera, currentRoom);
            projectileController.Draw(gameCamera, currentRoom);
            bombController.Draw(gameCamera, currentRoom);

            gameHero.Draw(GraphicsDevice, gameCamera);

            particleController.Draw();

            spriteBatch.Begin();
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                {
                    if (!Rooms[x, y].IsGap) spriteBatch.Draw(roomIcon, new Vector2(5+(x*25), 5+(y*25)), null, (gameHero.RoomX==x && gameHero.RoomY==y)?Color.Magenta:Color.Gray);
                }
            spriteBatch.DrawString(font, ((int)(doorCountdown / 1000)).ToString("00"), new Vector2(GraphicsDevice.Viewport.Width-100, 5), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None,1);
            double ms = doorCountdown - ((int)(doorCountdown / 1000) * 1000);
            spriteBatch.DrawString(font, (ms/10).ToString("00"), new Vector2(GraphicsDevice.Viewport.Width-35, 40), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

#region DOOR SHIT
        void OpenDoors()
        {
            if (gameHero.RoomX > 0 && !Rooms[gameHero.RoomX - 1, gameHero.RoomY].IsGap) Doors[3].Open(false);
            if (gameHero.RoomX < 3 && !Rooms[gameHero.RoomX + 1, gameHero.RoomY].IsGap) Doors[1].Open(false);
            if (gameHero.RoomY > 0 && !Rooms[gameHero.RoomX, gameHero.RoomY-1].IsGap) Doors[0].Open(false);
            if (gameHero.RoomY < 3 && !Rooms[gameHero.RoomX, gameHero.RoomY+1].IsGap) Doors[2].Open(false);

        }

        int prevRoomX = -1;
        int prevRoomY = -1;
        void DoRoomShift()
        {
            roomMovesLeft--;
            
            int gapX = 0;
            int gapY = 0;
            for(int x=0;x<4;x++)
                for(int y=0;y<4;y++)
                    if(Rooms[x,y].IsGap) { gapX = x; gapY = y; }

            int rx = 0;
            int ry = 0;
            bool ok = false;
            while(!ok)
            {
                rx = gapX-1 + Helper.Random.Next(3);
                ry = gapY-1 + Helper.Random.Next(3);
                if(!(rx==gapX&&ry==gapY) && (rx==gapX || ry==gapY) && !(rx==prevRoomX && ry==prevRoomY) && rx>=0 && rx<4 && ry>=0 && ry<4) ok=true;
            }

            roomShift = new RoomShift(rx, ry, gapX, gapY);
            prevRoomX = gapX;
            prevRoomY = gapY;
        }
#endregion

#region MAKIN ROOMS
        void CreateRoomsAsync()
        {
            int gapRoomX = Helper.Random.Next(4);
            int gapRoomY = Helper.Random.Next(4);

            float roomPercent = 100f / 16f;

            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                {
                    if (!(x == gapRoomX && y == gapRoomY))
                    {
                        Rooms[x, y] = new Room(tileSheet, objectSheet, false);
                    }
                    else Rooms[x, y] = new Room(tileSheet, objectSheet, true);
                    generatedPercent += (int)roomPercent;
                }

            // make exit
            bool made = false;
            while (!made)
            {
                int side = Helper.Random.Next(4);

                switch (side)
                {
                    case 0:
                        exitRoomX = Helper.Random.Next(4);
                        exitRoomY = -1;
                        if (!Rooms[exitRoomX, exitRoomY + 1].IsGap) made = true;
                        break;
                    case 1:
                        exitRoomY = Helper.Random.Next(4);
                        exitRoomX = 4;
                        if (!Rooms[exitRoomX - 1, exitRoomY].IsGap) made = true;
                        break;
                    case 2:
                        exitRoomX = Helper.Random.Next(4);
                        exitRoomY = 4;
                        if (!Rooms[exitRoomX, exitRoomY - 1].IsGap) made = true;
                        break;
                    case 3:
                        exitRoomY = Helper.Random.Next(4);
                        exitRoomX = -1;
                        if (!Rooms[exitRoomX + 1, exitRoomY].IsGap) made = true;
                        break;
                }
            }



            gameHero.RoomX = exitRoomX;// = new Hero(exitRoomX, exitRoomY, new Vector3(0, 0, 0));
            gameHero.RoomY = exitRoomY;

            //VoxelWorld.ToScreenSpace((7 * 16) + 7, 7, 21) + new Vector3(Voxel.HALF_SIZE,Voxel.HALF_SIZE,Voxel.HALF_SIZE), 0, doorSheet));
            ///VoxelWorld.ToScreenSpace((14 * 16) + 7, (4 * 16) + 7, 21) + new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), 1, doorSheet));
            //VoxelWorld.ToScreenSpace((7 * 16) + 7, (8 * 16) + 7, 21) + new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), 2, doorSheet));
            //VoxelWorld.ToScreenSpace(7, (4 * 16) + 7, 21) + new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), 3, doorSheet));

            if (gameHero.RoomX == -1)
            {
                gameHero.RoomX = 0;
                gameHero.Position = Doors[3].Position + new Vector3(7f, 0f, 4f);
                gameHero.Rotation = 0f;
            }
            if (gameHero.RoomX == 4)
            {
                gameHero.RoomX = 3;
                gameHero.Position = Doors[1].Position + new Vector3(-7f, 0f, 4f);
                gameHero.Rotation = -MathHelper.Pi;
            }
            if (gameHero.RoomY == -1)
            {
                gameHero.RoomY = 0;
                gameHero.Position = Doors[0].Position + new Vector3(0f, 7f, 4f);
                gameHero.Rotation = MathHelper.PiOver2;
            }
            if (gameHero.RoomY == 4) 
            { 
                gameHero.RoomY = 3;
                gameHero.Position = Doors[2].Position + new Vector3(0f, -7f, 4f);
                gameHero.Rotation = -MathHelper.PiOver2;
            }

            currentRoom = Rooms[gameHero.RoomX, gameHero.RoomY];

           

            gameCamera.Target = new Vector3((currentRoom.World.X_SIZE * Voxel.SIZE) / 2, (currentRoom.World.Y_SIZE * Voxel.SIZE) / 2, 0f);

            generatedPercent = 100;
        }
#endregion
    }

    enum RoomState
    {
        DoorsOpen,
        DoorsClosing,
        RoomsShifting,
        DoorsOpening
    }
}
