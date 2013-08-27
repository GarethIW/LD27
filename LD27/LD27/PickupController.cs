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
    public class PickupController
    {
        public static PickupController Instance;
        GraphicsDevice graphicsDevice;

        List<Pickup> Pickups; 

        BasicEffect drawEffect;

        VoxelSprite pickupStrip;

        public PickupController(GraphicsDevice gd)
        {
            Instance = this;
            graphicsDevice = gd;
            Pickups = new List<Pickup>();

            drawEffect = new BasicEffect(gd)
            {
                VertexColorEnabled = true
            };

        }

        public void LoadContent(ContentManager content)
        {
            pickupStrip = new VoxelSprite(16, 16, 16);
            LoadVoxels.LoadSprite(Path.Combine(content.RootDirectory, "dynamic.vxs"), ref pickupStrip);
        }

        public void Update(GameTime gameTime, Camera gameCamera, Hero gameHero, Room currentRoom)
        {
            foreach (Pickup p in Pickups.Where(proj => proj.Active))
            {
                p.Update(gameTime, currentRoom, gameHero);
                if (gameHero.boundingSphere.Intersects(p.boundingSphere) && p.Room==currentRoom) p.Collect(gameHero);
            }

            Pickups.RemoveAll(proj => !proj.Active);

            drawEffect.World = gameCamera.worldMatrix;
            drawEffect.View = gameCamera.viewMatrix;
            drawEffect.Projection = gameCamera.projectionMatrix;
        }

        public void Draw(Camera gameCamera, Room currentRoom)
        {
            foreach (Pickup p in Pickups.Where(proj => proj.Type == PickupType.Health && proj.Room == currentRoom))
            {
                drawEffect.World = gameCamera.worldMatrix *
                                   Matrix.CreateRotationX(MathHelper.PiOver2) *
                                   Matrix.CreateRotationZ(p.Rotation - MathHelper.PiOver2) *
                                   Matrix.CreateScale(1f) *
                                   Matrix.CreateTranslation(p.Position);
                foreach (EffectPass pass in drawEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    graphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, pickupStrip.AnimChunks[p.currentFrame + p.frameOffset].VertexArray, 0, pickupStrip.AnimChunks[p.currentFrame + p.frameOffset].VertexArray.Length, pickupStrip.AnimChunks[p.currentFrame + p.frameOffset].IndexArray, 0, pickupStrip.AnimChunks[p.currentFrame + p.frameOffset].VertexArray.Length / 2);

                }
            }
        }

        public void Reset()
        {
            Pickups.Clear();
        }

        public void Spawn(PickupType type, Room room, Vector3 pos)
        {
        
            Pickups.Add(new Pickup(type, room, pos));
        }




        
    }
}
