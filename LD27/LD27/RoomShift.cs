using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    public class RoomShift
    {
        //public static SoundEffectInstance scrapeSound = AudioController.effects["roomscrape"].CreateInstance();

        public int RoomX;
        public int RoomY;
        public int RoomTargetX;
        public int RoomTargetY;
        public bool Complete;

        public Vector3 cameraShake;

        double shiftTime = 0;
        double targetShiftTime = 1000;

        public RoomShift(int rx, int ry, int targx, int targy)
        {
            RoomX = rx;
            RoomY = ry;
            RoomTargetX = targx;
            RoomTargetY = targy;

            //scrapeSound = AudioController.effects["roomscrape"].CreateInstance();
            //if(!scrapeSound.IsLooped)
            //    scrapeSound.IsLooped = true;
            AudioController.instances["roomscrape"].Volume = 0f;
            AudioController.instances["roomscrape"].Resume();

        }

        public void Update(GameTime gameTime, Hero gameHero, ref Room[,] Rooms)
        {
            float dist = 4f - Vector3.Distance(new Vector3(gameHero.RoomX, gameHero.RoomY, 0f), new Vector3(RoomX, RoomY, 0f));
            dist = MathHelper.Clamp(dist, 0f, 3f);
            Vector3 dir = new Vector3(RoomX, RoomY, 0f) - new Vector3(gameHero.RoomX, gameHero.RoomY, 0f);

            AudioController.instances["roomscrape"].Pan = (1f / 3f) * dir.X;
            AudioController.instances["roomscrape"].Volume = (1f / 3f) * dist;

            shiftTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (shiftTime >= targetShiftTime)
            {
                AudioController.instances["roomscrape"].Pause();
                AudioController.PlaySFX("roomclunk", (1f / 3f) * dist, 0f, (1f / 3f) * dir.X);
                Complete = true;
                Room tempRoom = Rooms[RoomX, RoomY];
                Rooms[RoomX, RoomY] = Rooms[RoomTargetX, RoomTargetY];
                Rooms[RoomTargetX, RoomTargetY] = tempRoom;

                if (gameHero.RoomX == RoomX && gameHero.RoomY == RoomY)
                {
                    gameHero.RoomX = RoomTargetX;
                    gameHero.RoomY = RoomTargetY;
                }
            }

            dist = 3f - Vector3.Distance(new Vector3(gameHero.RoomX, gameHero.RoomY, 0f), new Vector3(RoomX, RoomY, 0f));
            dist = MathHelper.Clamp(dist, 0f, 3f);
            if (Helper.Random.Next(2) == 1) dist = -dist;
            cameraShake = new Vector3(((float)Helper.Random.NextDouble() * dist), 0f, ((float)Helper.Random.NextDouble() * dist)) * 0.1f;
        }
    }
}
