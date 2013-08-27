using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    public class Door
    {
        public Vector3 Position;
        public BoundingBox CollisionBox;

        public Vector3 ParticlePosition;
        public Vector3 ParticleDir;

        public bool IsOpen = false;
        public bool IsBlocked = true;
        public int Dir = 0;

        bool opening;
        bool closing;

        double frameTime = 0;
        double frameTargetTime = 100;
        int currentFrame = 0;
        
        VoxelSprite spriteSheet;

        public Door(Vector3 pos, int dir, VoxelSprite sheet)
        {
            Position = pos;
            Dir = dir;

            if(Dir==0||Dir==2)
                CollisionBox = new BoundingBox(pos - new Vector3(4f, 1f, 4f), pos + new Vector3(4f, 1f, 4f));
            else
                CollisionBox = new BoundingBox(pos - new Vector3(1f, 4f, 4f), pos + new Vector3(1f, 4f, 4f));

            switch (Dir)
            {
                case 0:
                    ParticlePosition = Position + new Vector3(0f, -15f, 0f);
                    ParticleDir = new Vector3(0f, 1f, 0f);
                    break;
                case 1:
                    ParticlePosition = Position + new Vector3(15f, 0f, 0f);
                    ParticleDir = new Vector3(-1f, 0f, 0f);
                    break;
                case 2:
                    ParticlePosition = Position + new Vector3(0f, 15f, 0f);
                    ParticleDir = new Vector3(0f, -1f, 0f);
                    break;
                case 3:
                    ParticlePosition = Position + new Vector3(-15f, 0f, 0f);
                    ParticleDir = new Vector3(1f, 0f, 0f);
                    break;
            }

            spriteSheet = sheet;
        }

        public void Update(GameTime gameTime)
        {
            if (opening || closing)
            {
                frameTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (frameTime >= frameTargetTime)
                {
                    frameTime = 0;
                    if (opening && currentFrame < spriteSheet.AnimChunks.Count - 1)
                    {
                        currentFrame++;
                        if (currentFrame == spriteSheet.AnimChunks.Count - 1)
                        {
                            opening = false;
                            IsOpen = true;
                        }
                    }
                    if (closing && currentFrame > 0)
                    {
                        currentFrame--;
                        if (currentFrame == 0)
                        {
                            closing = false;
                            IsOpen = false;
                        }
                    }


                }
            }

            if (IsOpen) IsBlocked = false;
            if (!IsOpen || opening || closing) IsBlocked = true;
        }

        public void Draw(GraphicsDevice gd, Camera gameCamera, BasicEffect drawEffect)
        {
            drawEffect.World = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationZ((Dir == 1 || Dir == 3) ? MathHelper.PiOver2 : 0f) * Matrix.CreateTranslation(Position) * gameCamera.worldMatrix;

            foreach (EffectPass pass in drawEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                AnimChunk c = spriteSheet.AnimChunks[currentFrame];
                gd.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, c.VertexArray, 0, c.VertexArray.Length, c.IndexArray, 0, c.VertexArray.Length / 2);
            }
        }

        internal void Close(bool immediate)
        {
            if (IsOpen)
            {

                if (immediate) { IsOpen = false; currentFrame = 0; opening = false; closing = false; }
                else
                {
                    if(!closing) AudioController.PlaySFX("door", 0.3f, 0f, 0f);

                    closing = true;
                }
            }
        }

        internal void Open(bool immediate)
        {
            if (!IsOpen)
            {

                if (immediate) { IsOpen = true; currentFrame = spriteSheet.AnimChunks.Count - 1; closing = false; opening = false; }
                else
                {
                    if(!opening) AudioController.PlaySFX("door", 0.3f, 0f, 0f);
                 
                    opening = true;
                }
            }
        }

        
    }
}
