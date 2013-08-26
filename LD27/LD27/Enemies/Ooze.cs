using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    public class Ooze : Enemy
    {
        Vector3 Target;
        public int Iteration = 0;

        

        public Ooze(Vector3 pos, Room room, VoxelSprite sprite)
            : base(pos, room, sprite)
        {
            Type = EnemyType.Ooze;

            animTargetTime = 500;
            numFrames = 3;
            offsetFrame = 1;
            Target = Position;
            Health = 150;
            Rotation = (float)(Helper.Random.NextDouble() * MathHelper.TwoPi);
        }

        public override void Update(GameTime gameTime, Room currentRoom, Hero gameHero, List<Door> doors)
        {
            if (currentRoom != Room) return;

            Vector3 dir = Target - Position;
            if (dir.Length() > 0f)
                dir.Normalize();
            Speed = dir * 0.05f;

            if (Vector3.Distance(Position, Target) <= 1f) Target = Position + (new Vector3(Helper.AngleToVector(((Rotation + MathHelper.Pi) - MathHelper.PiOver2) + ((float)Helper.Random.NextDouble() * MathHelper.Pi), 100f), 0f));

            Rotation = Helper.TurnToFace(new Vector2(Position.X, Position.Y), new Vector2(Position.X, Position.Y) + (new Vector2(Speed.X, Speed.Y) * 50f), Rotation, 1f, 0.5f);

            boundingSphere = new BoundingSphere(Position, 3f - (1f * Iteration));

            if (Helper.Random.Next(300) == 1)
            {
                dir = gameHero.Position - Position;
                dir.Normalize();

                ProjectileController.Instance.Spawn(ProjectileType.Acid, Room, Position, Matrix.Identity, new Vector3(dir.X * 0.3f, dir.Y * 0.3f, -(float)Helper.Random.NextDouble()), 5000, true);
            }

            Scale = 1f - (0.2f * (float)Iteration);

            if (hitAlpha > 0f) hitAlpha -= 0.1f;

            base.Update(gameTime, currentRoom, gameHero, doors);
        }

        public override void DoCollide(bool x, bool y, bool z, Vector3 checkPosition, Room currentRoom, Hero gameHero, bool withPlayer)
        {
            // Target = new Vector3(Helper.Random.Next(Room.World.X_SIZE) * Voxel.SIZE, Helper.Random.Next(Room.World.Y_SIZE) * Voxel.SIZE, Position.Z);

            Target = Position + (new Vector3(Helper.AngleToVector(((Rotation + MathHelper.Pi) - MathHelper.PiOver2) + ((float)Helper.Random.NextDouble() * MathHelper.Pi), 100f), 0f)); //new Vector3(Helper.Random.Next(Room.World.X_SIZE) * Voxel.SIZE, Helper.Random.Next(Room.World.Y_SIZE) * Voxel.SIZE, Position.Z);  //Position + (-Speed * 100f);
            Vector3 dir = Target - Position;
            if (dir.Length() > 0f)
                dir.Normalize();
            Speed = dir * 0.2f;

            base.DoCollide(x, y, z, checkPosition, currentRoom, gameHero, withPlayer);
        }

        public override void DoHit(Vector3 attackPos, Vector3 speed, float damage)
        {

            for (int i = 0; i < 4; i++)
            {
                ParticleController.Instance.Spawn(attackPos, speed + new Vector3(-0.1f + ((float)Helper.Random.NextDouble() * 0.2f), -0.1f + ((float)Helper.Random.NextDouble() * 0.2f), -0.1f + ((float)Helper.Random.NextDouble() * 0.2f)), 0.5f, new Color(0f, 0.5f + ((float)Helper.Random.NextDouble() * 0.5f), 0f), 1000, true);
            }

            hitAlpha = 1f;


            Health -= damage;


        }

        public override void Die()
        {
            if (Iteration < 2)
            {
                for (int i = 0; i <3; i++)
                {
                    float height = spriteSheet.Z_SIZE * Voxel.HALF_SIZE;
                    EnemyController.Instance.SpawnOoze(Position + new Vector3(0f,0f, height*(0.2f * (Iteration+1))), Room, Iteration + 1);
                }
            }
            base.Die();
        }
    }
}
