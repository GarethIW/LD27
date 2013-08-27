using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    public class Manhack : Enemy
    {
        float bobDir = 1f;

        public Manhack(Vector3 pos, Room room, VoxelSprite sprite)
            : base(pos, room, sprite)
        {
            Type = EnemyType.Manhack;
            
            Position.Z -= 2f;
            animTargetTime = 30f;

            Health = 50f;
        }

        public override void Update(GameTime gameTime, Room currentRoom, Hero gameHero, List<Door> doors)
        {
            if (currentRoom != Room) return;

            if (gameHero.Position.X < Position.X) Speed.X -= 0.01f;
            if (gameHero.Position.X > Position.X) Speed.X += 0.01f;
            if (gameHero.Position.Y < Position.Y) Speed.Y -= 0.01f;
            if (gameHero.Position.Y > Position.Y) Speed.Y += 0.01f;

            CheckCollisions(currentRoom.World, doors, currentRoom, gameHero);

            Position += Speed;

            Speed.Z += (0.01f * bobDir);
            if (Speed.Z > 0.2f) bobDir = -1f;
            if (Speed.Z < -0.2f) bobDir = 1f;

            
            animTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (animTime >= animTargetTime)
            {
                animTime = 0;

                CurrentFrame++;
                if (CurrentFrame == numFrames) CurrentFrame = 0;
            }
            

            for (float z = Position.Z; z < 25f; z += 0.1f)
            {
                if (Room.World.GetVoxel(new Vector3(Position.X, Position.Y, z)).Active) { groundHeight = z; break; }
            }

            boundingSphere = new BoundingSphere(Position, 2f);

            if (knockbackTime > 0) knockbackTime -= gameTime.ElapsedGameTime.TotalMilliseconds;

            if (hitAlpha > 0f) hitAlpha -= 0.1f;

            if (Health <= 0f) Die();

        }

        public override void DoHit(Vector3 attackPos, Vector3 speed, float damage)
        {
            Speed.X = speed.X;
            Speed.Y = speed.Y;

            for (int i = 0; i < 6; i++)
            {
                Color c = new Color(new Vector3(1.0f, (float)Helper.Random.NextDouble(), 0.0f)) * (0.7f + ((float)Helper.Random.NextDouble() * 0.3f));
                ParticleController.Instance.Spawn(Position, new Vector3(-0.1f + ((float)Helper.Random.NextDouble() * 0.2f), -0.1f + ((float)Helper.Random.NextDouble() * 0.2f), -0.1f + ((float)Helper.Random.NextDouble() * 0.2f)), 0.25f, c, 1000, true);                
            }

            knockbackTime = 2000;
            //base.DoHit(attackPos, vector3, p);

            Health -= damage;

            hitAlpha = 1f;
        }

        public override void DoCollide(bool x, bool y, bool z, Vector3 checkPosition, Room currentRoom, Hero gameHero, bool withPlayer)
        {
            if (withPlayer)
            {
                if (!gameHero.DoHit(checkPosition, Speed, 1f))
                {
                    if (x) Speed.X = (Speed.X * -0.9f);
                    if (y) Speed.Y = (Speed.Y * -0.9f);
                    if (z) Speed.Z = (Speed.Z * -0.9f);
                }
                else
                {
                    if (x) Speed.X = 0f;
                    if (y) Speed.Y = 0f;
                    if (z) Speed.Z = 0f;
                }
            }
            else
            {
                if (x) Speed.X = (Speed.X * -0.9f);
                if (y) Speed.Y = (Speed.Y * -0.9f);
                if (z) Speed.Z = (Speed.Z * -0.9f);
            }
            

            if (Speed.Length() > 0.2f)
            {
                Vector3 worldSpace = VoxelWorld.FromScreenSpace(checkPosition);
                Voxel v = Room.World.GetVoxel(checkPosition);

                if (v.Active && Active)
                {
                    if (v.Destructable == 1)
                    {
                        if (Helper.Random.Next(4) == 1)
                        {
                            Room.World.SetVoxelActive((int)worldSpace.X, (int)worldSpace.Y, (int)worldSpace.Z, false);
                            if (Room == currentRoom) ParticleController.Instance.Spawn(Position, new Vector3(-0.05f + ((float)Helper.Random.NextDouble() * 0.1f), -0.05f + ((float)Helper.Random.NextDouble() * 0.1f), -((float)Helper.Random.NextDouble() * 0.5f)), 0.25f, new Color(v.SR, v.SG, v.SB), 1000, true);
                        }
                    }
                    //Active = false;
                }
            }

            //base.DoCollide(x, y, z);
        }
    }
}
