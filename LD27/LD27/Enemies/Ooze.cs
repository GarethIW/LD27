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

        public Ooze(Vector3 pos, Room room, VoxelSprite sprite)
            : base(pos, room, sprite)
        {
            Target = new Vector3(room.World.X_SIZE * Voxel.SIZE, room.World.Y_SIZE * Voxel.SIZE, pos.Z);
        }

        public override void Update(GameTime gameTime, Room currentRoom, Hero gameHero, List<Door> doors)
        {
            if (currentRoom != Room) return;

            if (Target.X < Position.X) Speed.X -= 0.01f;
            if (Target.X > Position.X) Speed.X += 0.01f;
            if (Target.Y < Position.Y) Speed.Y -= 0.01f;
            if (Target.Y > Position.Y) Speed.Y += 0.01f;

            base.Update(gameTime, currentRoom, gameHero, doors);
        }
    }
}
