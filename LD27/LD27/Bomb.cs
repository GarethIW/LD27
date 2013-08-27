using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    public class Bomb
    {

        public Vector3 Position;

        public Room Room;

        public bool Active = true;

        public int CurrentFrame = 0;

        double explodeTime = 3000;
        double animTime = 0;
        double animTargetTime = 500;

        public Bomb(Vector3 pos, Room room)
        {
            Position = pos;
            Room = room;
        }

        public void Update(GameTime gameTime, Room currentRoom, Hero gameHero)
        {
            explodeTime -= gameTime.ElapsedGameTime.TotalMilliseconds;
            animTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (animTime >= animTargetTime)
            {
                animTargetTime = animTargetTime / 1.3;
                animTime = 0;

                CurrentFrame = 1 - CurrentFrame;
            }

            if (explodeTime <= 0)
            {
                Active = false;
                Room.World.Explode(Position, 8f, (currentRoom == Room));
                if (Room == currentRoom) ParticleController.Instance.SpawnExplosion(Position);

                foreach (Enemy e in EnemyController.Instance.Enemies.Where(en => en.Room == currentRoom))
                {
                    if (Vector3.Distance(Position, e.Position) < 10f)
                    {
                        float dam = (100f / 10f) * Vector3.Distance(Position, e.Position);
                        Vector3 speed = (Position-e.Position);
                        speed.Normalize();
                        e.DoHit(e.Position, speed * 0.5f, dam); 
                    }
                }
                gameHero.DoExplosionHit(Position, 10f);
                AudioController.PlaySFX("explosion1", 1f, -0.1f, 0.1f);
            }
        }
    }
}
