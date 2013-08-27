using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    public enum PickupType
    {
        Health
    }

    public class Pickup
    {
        public PickupType Type;

        public Room Room;

        public bool Active;
        public Vector3 Position;
        public Vector3 Speed;
        public float Rotation;

        float bobDir = 1f;

        public BoundingSphere boundingSphere;

        double frameTime = 0;
        double frameTargetTime = 200;
        public int currentFrame = 0;
        int numFrames = 3;
        int frameDir = 1;
        public int frameOffset = 0;

        public Pickup(PickupType type, Room room, Vector3 pos)
        {
            Type = type;
            Room = room;
            Position = pos;
            Rotation = (float)Helper.Random.NextDouble() * MathHelper.TwoPi;

            Active = true;

            switch (Type)
            {
                case PickupType.Health:
                    numFrames = 3;
                    frameOffset = 8;
                    break;
            }
        }

        public void Update(GameTime gameTime, Room currentRoom, Hero gameHero)
        {
            if (currentRoom != Room) return;

            Speed.Z += (0.001f * bobDir);
            if (Speed.Z > 0.05f) bobDir = -1f;
            if (Speed.Z < -0.05f) bobDir = 1f;

            frameTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (frameTime >= frameTargetTime)
            {
                frameTime = 0;
                currentFrame+=frameDir;
                if (currentFrame == numFrames) { currentFrame -= 2; frameDir = -1; }
                if (currentFrame == -1) { currentFrame = 1; frameDir = 1; }
            }

            Rotation += 0.01f;

            Position += Speed;

            boundingSphere = new BoundingSphere(Position, 1f);
            
        }

        public void Collect(Hero gameHero)
        {
            switch (Type)
            {
                case PickupType.Health:
                    gameHero.Health += 100f;
                    AudioController.PlaySFX("collect_health", 1f, 0f, 0f);
                    break;
            }
            Active = false;
        }

    }
}
