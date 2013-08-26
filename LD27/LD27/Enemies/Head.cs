using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    /// <summary>
    /// The 'head' of @MattDrivenDev
    /// </summary>
    public class Head : Enemy
    {
        Vector3 Target;

        float bobDir = 1f;
        int attackMode = 0;

        int missilesLaunched = 0;
        double missileDelay = 0;

        public Head(Vector3 pos, Room room, VoxelSprite sprite)
            : base(pos, room, sprite)
        {
            Type = EnemyType.Head;

            Position.Z -= 5f;

            numFrames = 1;
            offsetFrame = 0;

            numAttackFrames = 7;

            Health = 500f;
        }

        public override void Update(GameTime gameTime, Room currentRoom, Hero gameHero, List<Door> doors)
        {
            if (currentRoom != Room) return;

            Rotation = Helper.TurnToFace(new Vector2(Position.X, Position.Y), new Vector2(gameHero.Position.X, gameHero.Position.Y), Rotation, 1f, 0.5f);

            Speed.Z += (0.001f * bobDir);
            if (Speed.Z > 0.05f) bobDir = -1f;
            if (Speed.Z < -0.05f) bobDir = 1f;

            Position += Speed;

            for (float z = Position.Z; z < 25f; z += 0.1f)
            {
                if (Room.World.GetVoxel(new Vector3(Position.X, Position.Y, z)).Active) { groundHeight = z; break; }
            }
            //base.Update(gameTime, currentRoom, gameHero, doors);

            boundingSphere = new BoundingSphere(Position, 4f);


            if (!attacking && Helper.Random.Next(300) == 1)
            {
                attacking = true;
                attackDir = 1;
                attackMode = Helper.Random.Next(2);
            }

            if (attacking)
            {
                if (attackMode == 0) offsetFrame = 0;
                if (attackMode == 1) offsetFrame = 7;

                attackTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (attackTime >= attackTargetTime)
                {
                    attackTime = 0;
                    if((attackDir==1 && attackFrame<numAttackFrames-1) || attackDir==-1) attackFrame += attackDir;
                }
                if (attackFrame == numAttackFrames-1 && attackDir == 1)
                {
                    switch (attackMode)
                    {
                        case 0:
                            
                            missileDelay += gameTime.ElapsedGameTime.TotalMilliseconds;
                            if (missileDelay >= 500)
                            {
                                missileDelay = 0;
                                float rot = (Rotation - 0.5f) + ((float)Helper.Random.NextDouble() * 1f);
                                ProjectileController.Instance.Spawn(ProjectileType.Rocket, Room, Position + new Vector3(0f,0f,-2f), Matrix.CreateRotationZ(rot), new Vector3(Helper.AngleToVector(rot, 0.5f), 0.2f), 7000, false);
                                missilesLaunched++;
                            }
                            if (missilesLaunched == 3) { attackDir = -1; missilesLaunched = 0; }
                            break;
                        case 1:
                            missileDelay += gameTime.ElapsedGameTime.TotalMilliseconds;
                            if (missileDelay >= 50)
                            {
                                missileDelay = 0;
                                float rot = (Rotation - 0.1f) + ((float)Helper.Random.NextDouble() * 0.2f);
                                ProjectileController.Instance.Spawn(ProjectileType.Gatling, Room, Position + new Vector3(0f, 0f, 2f), Matrix.CreateRotationZ(rot), new Vector3(Helper.AngleToVector(rot, 1f), 0.1f), 5000, false);
                                missilesLaunched++;
                            }
                            if (missilesLaunched == 20) { attackDir = -1; missilesLaunched = 0; }
                            break;
                    }
                }

                if (attackFrame == -1) { attackFrame = 0; attacking = false; offsetFrame = 0; attackDir = 1; }

            }

            if (hitAlpha > 0f) hitAlpha -= 0.1f;

            if (Health <= 0f) Die();
        }

        public override void DoHit(Vector3 attackPos, Vector3 speed, float damage)
        {

            for (int i = 0; i < 4; i++)
            {
                ParticleController.Instance.Spawn(attackPos, speed + new Vector3(-0.1f + ((float)Helper.Random.NextDouble() * 0.2f), -0.1f + ((float)Helper.Random.NextDouble() * 0.2f), -0.1f + ((float)Helper.Random.NextDouble() * 0.2f)), 0.5f, new Color(0.5f + ((float)Helper.Random.NextDouble() * 0.5f), 0f, 0f), 1000, true);
            }

            hitAlpha = 1f;


            ///knockbackTime = 500;
            //base.DoHit(attackPos, vector3, p);

            Health -= damage;


        }
    }
}
