using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    public class Sentinel : Enemy
    {
        Vector3 Target;

        public Sentinel(Vector3 pos, Room room, VoxelSprite sprite)
            : base(pos, room, sprite)
        {
            Type = EnemyType.Sentinel;

            animTargetTime = 100;
            numFrames = 5;
            offsetFrame = 1;
            Target = Position;
            Rotation = (float)(Helper.Random.NextDouble() * MathHelper.TwoPi);

            numAttackFrames = 6;
        }

        public override void Update(GameTime gameTime, Room currentRoom, Hero gameHero, List<Door> doors)
        {
            if (currentRoom != Room) return;

            if (Vector3.Distance(Position, gameHero.Position) < 30f) attacking = true;

            if (attacking)
            {
                Speed = Vector3.Zero;
                Rotation = Helper.TurnToFace(new Vector2(Position.X, Position.Y), new Vector2(gameHero.Position.X, gameHero.Position.Y), Rotation, 1f, 0.5f);

                attackTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (attackTime >= attackTargetTime)
                {
                    attackTime = 0;
                    attackFrame += attackDir;

                    if (attackFrame == numAttackFrames-1 && attackDir == 1)
                    {
                        ProjectileController.Instance.Spawn(ProjectileType.Laserbolt, Room, Position, Matrix.CreateRotationZ(Rotation), new Vector3(Helper.AngleToVector(Rotation, 0.5f),0f), 5000, false);
                    }

                    if (attackFrame == numAttackFrames-1) { attackDir = -1; attackFrame = numAttackFrames-2; }
                    if (attackFrame == -1) { attackFrame = 0; if (Vector3.Distance(Position, gameHero.Position) >= 30f) attacking = false; attackDir = 1; }
                }
            }
            else
            {
                Vector3 dir = Target - Position;
                if (dir.Length() > 0f)
                    dir.Normalize();
                Speed = dir * 0.2f;

                if (Vector3.Distance(Position, Target) <= 1f) Target = Position + (new Vector3(Helper.AngleToVector(((Rotation + MathHelper.Pi) - MathHelper.PiOver2) + ((float)Helper.Random.NextDouble() * MathHelper.Pi), 100f), 0f));

                Rotation = Helper.TurnToFace(new Vector2(Position.X, Position.Y), new Vector2(Position.X, Position.Y) + (new Vector2(Speed.X, Speed.Y) * 50f), Rotation, 1f, 0.5f);

                base.Update(gameTime, currentRoom, gameHero, doors);
            }

            boundingSphere = new BoundingSphere(Position, 3f);

            for (float z = Position.Z; z < 25f; z += 0.1f)
            {
                if (Room.World.GetVoxel(new Vector3(Position.X, Position.Y, z)).Active) { groundHeight = z; break; }
            }

            if (hitAlpha > 0f) hitAlpha -= 0.1f;

            if (Health <= 0f) Die();
        }

        public override void DoHit(Vector3 attackPos, Vector3 speed, float damage)
        {
            for (int i = 0; i < 6; i++)
            {
                Color c = new Color(new Vector3(1.0f, (float)Helper.Random.NextDouble(), 0.0f)) * (0.7f + ((float)Helper.Random.NextDouble() * 0.3f));
                ParticleController.Instance.Spawn(Position, new Vector3(-0.1f + ((float)Helper.Random.NextDouble() * 0.2f), -0.1f + ((float)Helper.Random.NextDouble() * 0.2f), -0.1f + ((float)Helper.Random.NextDouble() * 0.2f)), 0.25f, c, 1000, true);
            }

            hitAlpha = 1f;

            Health -= damage;

        }

        public override void DoCollide(bool x, bool y, bool z, Vector3 checkPosition, Room currentRoom, Hero gameHero, bool withPlayer)
        {
           // Target = new Vector3(Helper.Random.Next(Room.World.X_SIZE) * Voxel.SIZE, Helper.Random.Next(Room.World.Y_SIZE) * Voxel.SIZE, Position.Z);

            Target = Position + (new Vector3(Helper.AngleToVector(((Rotation+MathHelper.Pi) - MathHelper.PiOver2) + ((float)Helper.Random.NextDouble()*MathHelper.Pi), 100f), 0f)); //new Vector3(Helper.Random.Next(Room.World.X_SIZE) * Voxel.SIZE, Helper.Random.Next(Room.World.Y_SIZE) * Voxel.SIZE, Position.Z);  //Position + (-Speed * 100f);
            Vector3 dir = Target - Position;
            if (dir.Length() > 0f)
                dir.Normalize();
            Speed = dir * 0.2f;

            base.DoCollide(x, y, z, checkPosition, currentRoom, gameHero, withPlayer);
        }
    }
}
