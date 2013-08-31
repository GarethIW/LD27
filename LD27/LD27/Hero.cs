using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LD27
{
    public class Hero
    {
        public Vector3 Position;
        public float Rotation;
        public Vector3 Speed;

        public float Health = 0;
        public float MaxHealth = 1000f;
        public bool Dead = false;

        public int RoomX;
        public int RoomY;

        public int numBombs = 3;
        public double bombRespawnTime = 0;

        public VoxelSprite spriteSheet;

        BasicEffect drawEffect;

        float moveSpeed = 0.5f;

        double frameTime = 0;
        double frameTargetTime = 100;
        int currentFrame = 0;

        public BoundingSphere boundingSphere = new BoundingSphere();

        double timeSinceLastHit = 0;

        bool attacking = false;
        double attackTime = 0;
        double attackTargetTime = 500;
        float attackRotation = 0f;
        int attackDir;
        int attackFrame = 0;


        bool defending = false;

        public float hitAlpha = 0f;

        public Vector3 IntroTarget;
        public bool introTargetReached = false;

        public bool exitReached = false;

        public Hero(int startX, int startY, Vector3 pos, Vector3 target)
        {
            Health = MaxHealth;
            RoomX = startX;
            RoomY = startY;

            Position = pos;
            IntroTarget = target;
        }

        public void LoadContent(ContentManager content, GraphicsDevice gd)
        {
            spriteSheet = new VoxelSprite(16, 16, 16);
            LoadVoxels.LoadSprite(Path.Combine(content.RootDirectory, "dude.vxs"), ref spriteSheet);

            drawEffect = new BasicEffect(gd)
            {
                VertexColorEnabled = true
            };
        }

        public void Update(GameTime gameTime, Camera gameCamera, Room currentRoom, List<Door> doors, ref Room[,] rooms, bool allRoomsComplete, int exitRoomX, int exitRoomY, Door exitDoor)
        {
            if (Dead) return;

            Health = MathHelper.Clamp(Health, 0f, MaxHealth);

            //Health = MaxHealth;

            Vector2 v2pos = new Vector2(Position.X, Position.Y);
            Vector2 v2speed = new Vector2(Speed.X, Speed.Y);
            if (Speed.Length() > 0f)
            {
                //if (!defending)
                Rotation = Helper.TurnToFace(v2pos, v2pos + (v2speed * 50f), Rotation, 1f, 0.5f);
            }
            if(introTargetReached) CheckCollisions(currentRoom.World, doors, currentRoom);
            Position += Speed;

            v2speed = new Vector2(Speed.X, Speed.Y);
            if (Speed.Length() > 0f)
            {
                frameTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (frameTime >= frameTargetTime)
                {
                    frameTime = 0;
                    currentFrame++;
                    if (currentFrame == 4) currentFrame = 0;
                }
            }

            if (RoomX == exitRoomX && RoomY == exitRoomY && introTargetReached)
            {
                if (Position.X < doors[3].Position.X - 5f && doors[3] == exitDoor) { exitReached = true; }
                if (Position.X > doors[1].Position.X + 5f && doors[1] == exitDoor) { exitReached = true; }
                if (Position.Y < doors[0].Position.Y - 5f && doors[0] == exitDoor) { exitReached = true; }
                if (Position.Y > doors[2].Position.Y + 5f && doors[2] == exitDoor) { exitReached = true; }
            }

            if (introTargetReached)
            {
                if (Position.X < doors[3].Position.X && !(doors[3] == exitDoor && RoomX == exitRoomX && RoomY == exitRoomY)) { RoomX--; Position = doors[1].Position + new Vector3(0f, 0f, 4f); ResetDoors(doors, ref rooms, allRoomsComplete, exitRoomX, exitRoomY, exitDoor); }
                if (Position.X > doors[1].Position.X && !(doors[1] == exitDoor && RoomX == exitRoomX && RoomY == exitRoomY)) { RoomX++; Position = doors[3].Position + new Vector3(0f, 0f, 4f); ResetDoors(doors, ref rooms, allRoomsComplete, exitRoomX, exitRoomY, exitDoor); }
                if (Position.Y < doors[0].Position.Y && !(doors[0] == exitDoor && RoomX == exitRoomX && RoomY == exitRoomY)) { RoomY--; Position = doors[2].Position + new Vector3(0f, 0f, 4f); ResetDoors(doors, ref rooms, allRoomsComplete, exitRoomX, exitRoomY, exitDoor); }
                if (Position.Y > doors[2].Position.Y && !(doors[2] == exitDoor && RoomX == exitRoomX && RoomY == exitRoomY)) { RoomY++; Position = doors[0].Position + new Vector3(0f, 0f, 4f); ResetDoors(doors, ref rooms, allRoomsComplete, exitRoomX, exitRoomY, exitDoor); }

                
            }

            

            Vector2 p = Helper.RandomPointInCircle(Helper.PointOnCircle(ref v2pos, 1f, (Rotation - MathHelper.Pi) + 0.1f), 0f, 2f);
            ParticleController.Instance.Spawn(new Vector3(p, Position.Z-1f), new Vector3(0f, 0f, -0.01f - ((float)Helper.Random.NextDouble() * 0.01f)), 0.5f, Color.Black*0.2f, 2000, false);

            drawEffect.Projection = gameCamera.projectionMatrix;
            drawEffect.View = gameCamera.viewMatrix;
            drawEffect.World = gameCamera.worldMatrix *
                               Matrix.CreateRotationX(MathHelper.PiOver2) *
                               Matrix.CreateRotationZ(Rotation - MathHelper.PiOver2) *
                               Matrix.CreateTranslation(new Vector3(0, 0, (-(spriteSheet.Z_SIZE * SpriteVoxel.HALF_SIZE)) + SpriteVoxel.HALF_SIZE)) *
                               Matrix.CreateScale(0.9f) *
                               Matrix.CreateTranslation(Position);

            boundingSphere = new BoundingSphere(Position + new Vector3(0f,0f,-4f), 3f);

            timeSinceLastHit -= gameTime.ElapsedGameTime.TotalMilliseconds;

            if (attacking)
            {
                attackTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (attackTime >= attackTargetTime)
                {
                    attackTime = 0;
                    attackFrame+=attackDir;

                    if (attackFrame == 1 && attackDir == 1)
                    {
                        bool hit = false;
                        float radiusSweep = 1f;
                        foreach (Enemy e in EnemyController.Instance.Enemies.Where(en => en.Room == currentRoom))
                        {
                            for (float az = 0f; az > -8f; az -= 1f)
                            {
                                for (float a = Rotation - radiusSweep; a < Rotation + radiusSweep; a += 0.02f)
                                {
                                    for (float dist = 0f; dist < 5f; dist += 0.2f)
                                    {
                                        Vector3 attackPos = new Vector3(Helper.PointOnCircle(ref v2pos, dist, Rotation), Position.Z + az);

                                        if (e.boundingSphere.Contains(attackPos) == ContainmentType.Contains && !hit)
                                        {
                                            e.DoHit(attackPos, new Vector3(Helper.AngleToVector(Rotation, 0.01f), 0f), 10f);
                                            hit = true;
                                        }
                                    }
                                }
                            }
                        }

                    }

                    if (attackFrame == 3) { attackDir = -1; attackFrame = 1; }
                    if (attackFrame == -1) { attackFrame = 0; attacking = false; }

                }
            }

            if (hitAlpha > 0f) hitAlpha -= 0.1f;

            if (Health <= 0f) Die();

            bombRespawnTime -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (bombRespawnTime <= 0 && numBombs < 3)
            {
                numBombs++;
                bombRespawnTime = 5000;
                if (numBombs == 3) bombRespawnTime = 0;
            }

            if (!introTargetReached)
            {
                if (IntroTarget.X < Position.X) Move(new Vector2(-0.3f, 0f));
                if (IntroTarget.X > Position.X) Move(new Vector2(0.3f, 0f));
                if (IntroTarget.Y < Position.Y) Move(new Vector2(0f,-0.3f));
                if (IntroTarget.Y > Position.Y) Move(new Vector2(0f,0.3f));

                if(Vector3.Distance(IntroTarget, Position)<1f) introTargetReached = true;
            }
        }

        private void ResetDoors(List<Door> doors, ref Room[,] rooms, bool allRoomsComplete, int exitRoomX, int exitRoomY, Door exitDoor)
        {
            if (RoomX <= -1) { RoomX = 0; exitReached = true; exitDoor.Close(false); }
            if (RoomY <= -1) { RoomY = 0; exitReached = true; exitDoor.Close(false); }
            if (RoomX >= 4) { RoomX = 3; exitReached = true; exitDoor.Close(false); }
            if (RoomY >= 4) { RoomX = 3; exitReached = true; exitDoor.Close(false); }

            if (!exitReached)
            {
                if (RoomX > 0 && !rooms[RoomX - 1, RoomY].IsGap) doors[3].Open(true); else doors[3].Close(true);
                if (RoomX < 3 && !rooms[RoomX + 1, RoomY].IsGap) doors[1].Open(true); else doors[1].Close(true);
                if (RoomY > 0 && !rooms[RoomX, RoomY - 1].IsGap) doors[0].Open(true); else doors[0].Close(true);
                if (RoomY < 3 && !rooms[RoomX, RoomY + 1].IsGap) doors[2].Open(true); else doors[2].Close(true);

                if (allRoomsComplete && RoomX == exitRoomX && RoomY == exitRoomY) exitDoor.Open(true);


                ParticleController.Instance.Reset();
                ProjectileController.Instance.Reset();
            }
        }

        public void Draw(GraphicsDevice gd, Camera gameCamera)
        {
            if (Dead) return;

            drawEffect.DiffuseColor = new Vector3(1f, 1f - hitAlpha, 1f - hitAlpha);
            foreach (EffectPass pass in drawEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                AnimChunk c = spriteSheet.AnimChunks[currentFrame];
                gd.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, c.VertexArray, 0, c.VertexArray.Length, c.IndexArray, 0, c.VertexArray.Length / 2);

                if (!attacking && !defending)
                {
                    c = spriteSheet.AnimChunks[currentFrame + 4];
                    gd.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, c.VertexArray, 0, c.VertexArray.Length, c.IndexArray, 0, c.VertexArray.Length / 2);
                }

                if (attacking)
                {
                    c = spriteSheet.AnimChunks[attackFrame + 8];
                    gd.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, c.VertexArray, 0, c.VertexArray.Length, c.IndexArray, 0, c.VertexArray.Length / 2);
                }

                if(defending)
                {
                    c = spriteSheet.AnimChunks[12];
                    gd.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, c.VertexArray, 0, c.VertexArray.Length, c.IndexArray, 0, c.VertexArray.Length / 2);
                }
            }
        }

        public void DoAttack()
        {
            if (!introTargetReached || Dead || attacking || defending) return;

            attacking = true;
            attackFrame = 0;
            attackTime = 0;
            attackDir = 1;

            AudioController.PlaySFX("sword", 0.2f, -0.2f, 0.2f);
        }

        public void DoDefend(bool def, Vector2 virtualJoystick)
        {
            if (Dead || !introTargetReached) return;

            if (!def) defending = false;
            if (!attacking && def)
            {
                if (!defending) AudioController.PlaySFX("defend", 0.6f, -0.1f, 0.1f);
                defending = true;
            }
        }

        public virtual void Die()
        {
            if (Dead || !introTargetReached) return;

            for (int x = 0; x < spriteSheet.X_SIZE; x++)
                for (int y = 0; y < spriteSheet.Y_SIZE; y++)
                    for (int z = 0; z < spriteSheet.Z_SIZE; z++)
                    {
                        if (Helper.Random.Next(5) == 1)
                        {
                            SpriteVoxel v = spriteSheet.AnimChunks[currentFrame].Voxels[x, y, z];
                            Vector3 pos;
                            if (!v.Active)
                            {
                                pos = (-new Vector3(spriteSheet.X_SIZE * Voxel.HALF_SIZE, spriteSheet.Y_SIZE * Voxel.HALF_SIZE, spriteSheet.Z_SIZE * Voxel.HALF_SIZE) * 0.9f) + (new Vector3(x * Voxel.SIZE, y * Voxel.SIZE, z * Voxel.SIZE) * 0.9f);
                                pos = Position + new Vector3(0f, 0f, -7f) + Vector3.Transform(pos, Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationZ(Rotation));
                                ParticleController.Instance.Spawn(pos, -(Vector3.One * 0.1f) + (Vector3.One * ((float)Helper.Random.NextDouble() * 0.2f)), 0.3f, v.Color, 3000, true);
                            }

                            v = spriteSheet.AnimChunks[currentFrame+4].Voxels[x, y, z];
                            if (!v.Active) continue;
                            pos = (-new Vector3(spriteSheet.X_SIZE * Voxel.HALF_SIZE, spriteSheet.Y_SIZE * Voxel.HALF_SIZE, spriteSheet.Z_SIZE * Voxel.HALF_SIZE) * 0.9f) + (new Vector3(x * Voxel.SIZE, y * Voxel.SIZE, z * Voxel.SIZE) * 0.9f);
                            pos = Position + new Vector3(0f, 0f, -7f) + Vector3.Transform(pos, Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationZ(Rotation));
                            ParticleController.Instance.Spawn(pos, -(Vector3.One * 0.1f) + (Vector3.One * ((float)Helper.Random.NextDouble() * 0.2f)), 0.3f, v.Color, 3000, true);
                        }
                    }

            for (int i = 0; i < 16; i++)
            {
                ParticleController.Instance.Spawn(Position + new Vector3(0f, 0f, -7f), new Vector3(-0.05f + ((float)Helper.Random.NextDouble() * 0.1f), -0.05f + ((float)Helper.Random.NextDouble() * 0.1f), -0.05f + ((float)Helper.Random.NextDouble() * 0.1f)), 0.5f, new Color(0.5f + ((float)Helper.Random.NextDouble() * 0.5f), 0f, 0f), 1000, true);
            }

            Dead = true;
        }

        public void Move(Vector2 virtualJoystick)
        {
            if (Dead) return;
            if (exitReached) return;

            Vector3 dir = new Vector3(virtualJoystick, 0f);

            if (!defending)
                Speed = dir * moveSpeed;
            else
                Speed = dir * (moveSpeed * 0.1f);
        }


        internal void TryPlantBomb(Room currentRoom)
        {
            if (Dead || !introTargetReached) return;

            if (numBombs > 0)
            {
                BombController.Instance.Spawn(Position + new Vector3(0f, 0f, -3f), currentRoom);
                numBombs--;
                if (bombRespawnTime <= 0) bombRespawnTime = 5000;
                AudioController.PlaySFX("bomb_place", 1f, 0f, 0f);
            }
        }

        internal void DoExplosionHit(Vector3 pos, float r)
        {
            if (Dead || !introTargetReached) return;

            if (Vector3.Distance(Position, pos) < r)
            {
                float dam = (200f / r) * Vector3.Distance(Position, pos);
                Vector3 speed = (pos - Position);
                speed.Normalize();
                DoHit(Position, speed * 0.5f, dam);
            }
        }

        internal bool DoHit(Vector3 pos, Vector3 speed, float damage)
        {
            if (Dead || !introTargetReached) return true;

            if (defending)
            {
                Vector2 v2pos = new Vector2(Position.X, Position.Y);
                BoundingSphere shieldSphere = new BoundingSphere(new Vector3(Helper.PointOnCircle(ref v2pos, 3f, Rotation), Position.Z-5f), 4f);

                if(shieldSphere.Contains(pos)== ContainmentType.Contains) return false;
            }

            if (timeSinceLastHit <= 0)
            {
                
                for (int i = 0; i < 4; i++)
                {
                    ParticleController.Instance.Spawn(pos, speed + new Vector3(-0.05f + ((float)Helper.Random.NextDouble() * 0.1f), -0.05f + ((float)Helper.Random.NextDouble() * 0.1f), -0.05f + ((float)Helper.Random.NextDouble() * 0.1f)), 0.5f, new Color(0.5f + ((float)Helper.Random.NextDouble() * 0.5f), 0f, 0f), 1000, true);
                }
                timeSinceLastHit = 100;

                AudioController.PlaySFX("player_hit", 0.5f, -0.2f, 0.2f);
            }

            hitAlpha = 1f;

            Health -= damage;

            return true;
        }

        void CheckCollisions(VoxelWorld world, List<Door> doors, Room currentRoom)
        {
            float checkRadius = 3.5f;
            float radiusSweep = 0.75f;
            Vector2 v2Pos = new Vector2(Position.X, Position.Y);
            float checkHeight = Position.Z - 1f;
            Voxel checkVoxel;
            Vector3 checkPos;

            if (Speed.Y < 0f)
            {
                for (float a = -MathHelper.PiOver2 - radiusSweep; a < -MathHelper.PiOver2 + radiusSweep; a += 0.02f)
                {
                    checkPos = new Vector3(Helper.PointOnCircle(ref v2Pos, checkRadius, a), checkHeight);
                    checkVoxel = world.GetVoxel(checkPos);
                    if ((checkVoxel.Active && world.CanCollideWith(checkVoxel.Type)))
                    {
                        Speed.Y = 0f;
                    }
                    foreach (Door d in doors) { if (d.IsBlocked && d.CollisionBox.Contains(checkPos)==ContainmentType.Contains) Speed.Y = 0f; }
                    foreach (Enemy e in EnemyController.Instance.Enemies.Where(en => en.Room == currentRoom)) { if (e.boundingSphere.Contains(checkPos + new Vector3(0f,0f,-5f)) == ContainmentType.Contains) Speed.Y = 0f; }

                }
            }
            if (Speed.Y > 0f)
            {
                for (float a = MathHelper.PiOver2 - radiusSweep; a < MathHelper.PiOver2 + radiusSweep; a += 0.02f)
                {
                    checkPos = new Vector3(Helper.PointOnCircle(ref v2Pos, checkRadius, a), checkHeight);
                    checkVoxel = world.GetVoxel(checkPos);
                    if ((checkVoxel.Active && world.CanCollideWith(checkVoxel.Type)))
                    {
                        Speed.Y = 0f;
                    }
                    foreach (Door d in doors) { if (d.IsBlocked && d.CollisionBox.Contains(checkPos) == ContainmentType.Contains) Speed.Y = 0f; }
                    foreach (Enemy e in EnemyController.Instance.Enemies.Where(en => en.Room == currentRoom)) { if (e.boundingSphere.Contains(checkPos + new Vector3(0f, 0f, -5f)) == ContainmentType.Contains) Speed.Y = 0f; }

                }
            }
            if (Speed.X < 0f)
            {
                for (float a = -MathHelper.Pi - radiusSweep; a < -MathHelper.Pi + radiusSweep; a += 0.02f)
                {
                    checkPos = new Vector3(Helper.PointOnCircle(ref v2Pos, checkRadius, a), checkHeight);
                    checkVoxel = world.GetVoxel(checkPos);
                    if ((checkVoxel.Active && world.CanCollideWith(checkVoxel.Type)))
                    {
                        Speed.X = 0f;
                    }
                    foreach (Door d in doors) { if (d.IsBlocked && d.CollisionBox.Contains(checkPos) == ContainmentType.Contains) Speed.X = 0f; }
                    foreach (Enemy e in EnemyController.Instance.Enemies.Where(en => en.Room == currentRoom)) { if (e.boundingSphere.Contains(checkPos + new Vector3(0f, 0f, -5f)) == ContainmentType.Contains) Speed.X = 0f; }

                }
            }
            if (Speed.X > 0f)
            {
                for (float a = -radiusSweep; a < radiusSweep; a += 0.02f)
                {
                    checkPos = new Vector3(Helper.PointOnCircle(ref v2Pos, checkRadius, a), checkHeight);
                    checkVoxel = world.GetVoxel(checkPos);
                    if ((checkVoxel.Active && world.CanCollideWith(checkVoxel.Type)))
                    {
                        Speed.X = 0f;
                    }
                    foreach (Door d in doors) { if (d.IsBlocked && d.CollisionBox.Contains(checkPos) == ContainmentType.Contains) Speed.X = 0f; }
                    foreach (Enemy e in EnemyController.Instance.Enemies.Where(en => en.Room == currentRoom)) { if (e.boundingSphere.Contains(checkPos + new Vector3(0f, 0f, -5f)) == ContainmentType.Contains) Speed.X = 0f; }

                }
            }
        }

    }
}
