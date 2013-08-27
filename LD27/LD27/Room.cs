using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD27
{
    public class Room
    {
        public VoxelWorld World;

        public bool IsGap = false;
        public bool IsComplete = false;

        VoxelSprite objectSheet;

        float[,] wallRots = new float[15, 9];

        Color wallColor = Color.Red;
        float wallGlow = 0f;
        float wallGlowTarget = -0.3f;

        public Room(VoxelSprite tileSheet, VoxelSprite objects, bool isGap)
        {
            IsGap = isGap;

            if (!isGap)
            {
                World = new VoxelWorld(15, 9, 1, true);

                CreateMap(tileSheet);
            }
            else IsComplete = true;

            //IsComplete = true;

            objectSheet = objects;
        }

        public void Update(GameTime gameTime)
        {
            wallGlow = MathHelper.Lerp(wallGlow, wallGlowTarget, 0.01f);
            if (wallGlow < -0.29f) wallGlowTarget = 0.3f;
            if (wallGlow > 0.29f) wallGlowTarget = -0.3f;

            if(!IsComplete)
                wallColor = new Color(0.7f + wallGlow, 0f, 0f);
            else
                wallColor = new Color(0f, 0.7f + wallGlow, 0f);


            if (EnemyController.Instance.Enemies.Count(en => en.Room == this) == 0) IsComplete = true;
        }

        public void Draw(GraphicsDevice gd, Camera gameCamera, BasicEffect drawEffect)
        {
            drawEffect.DiffuseColor = wallColor.ToVector3();

             for (int x = 0; x < 15; x++)
                for (int y = 0; y < 9; y++)
                    if (x == 0 || x == 14 || y == 0 || y == 8)
                        if (x != 7 && y != 4)
                        {
                            drawEffect.World = Matrix.CreateRotationX(MathHelper.PiOver2) *
                                               Matrix.CreateRotationZ(wallRots[x,y]) * 
                                               Matrix.CreateTranslation(VoxelWorld.ToScreenSpace((x * 16) + 7, (y * 16) + 7, 21) + (Vector3.One * Voxel.HALF_SIZE)) * 
                                               gameCamera.worldMatrix;
                            foreach (EffectPass pass in drawEffect.CurrentTechnique.Passes)
                            {
                                pass.Apply();

                                AnimChunk c = objectSheet.AnimChunks[7];
                                gd.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, c.VertexArray, 0, c.VertexArray.Length, c.IndexArray, 0, c.VertexArray.Length / 2);
                            }
                        }

             drawEffect.DiffuseColor = Color.White.ToVector3();
        }

        void CreateMap(VoxelSprite tileSheet)
        {

            for (int x = 0; x < 15; x++)
                for (int y = 0; y < 9; y++)
                {
                    wallRots[x, y] = MathHelper.PiOver2 * (float)Helper.Random.Next(4);
                    if (x == 0 || x == 14 || y == 0 || y == 8)
                        if (x != 7 && y != 4)
                            World.CopySprite(x * Chunk.X_SIZE, y * Chunk.X_SIZE, 14, tileSheet.AnimChunks[0], Helper.Random.Next(4), 0);
                        else
                            World.CopySprite(x * Chunk.X_SIZE, y * Chunk.X_SIZE, 14, tileSheet.AnimChunks[(x == 7 ? 2 : 1)], 0, 0);

                    
                }



            for (int i = 0; i < 3; i++)
            {
                int rx = 1 + Helper.Random.Next(6);
                int ry = 1 + Helper.Random.Next(3);

                int t = 6 + Helper.Random.Next(1);

                World.CopySprite(rx * Chunk.X_SIZE, ry * Chunk.X_SIZE, 14, tileSheet.AnimChunks[t], 0, 1);
                World.CopySprite((14 - rx) * Chunk.X_SIZE, ry * Chunk.X_SIZE, 14, tileSheet.AnimChunks[t], 0, 1);
                World.CopySprite(rx * Chunk.X_SIZE, (8 - ry) * Chunk.X_SIZE, 14, tileSheet.AnimChunks[t], 0, 1);
                World.CopySprite((14 - rx) * Chunk.X_SIZE, (8 - ry) * Chunk.X_SIZE, 14, tileSheet.AnimChunks[t], 0, 1);

                if(Helper.Random.Next(5)==1) PickupController.Instance.Spawn(PickupType.Health, this, VoxelWorld.ToScreenSpace((rx * Chunk.X_SIZE) + (Chunk.X_SIZE / 2), (ry * Chunk.Y_SIZE) + (Chunk.Y_SIZE / 2), 21));
                if (Helper.Random.Next(5) == 1) PickupController.Instance.Spawn(PickupType.Health, this, VoxelWorld.ToScreenSpace(((14 - rx) * Chunk.X_SIZE) + (Chunk.X_SIZE / 2), (ry * Chunk.Y_SIZE) + (Chunk.Y_SIZE / 2), 21));
                if (Helper.Random.Next(5) == 1) PickupController.Instance.Spawn(PickupType.Health, this, VoxelWorld.ToScreenSpace((rx * Chunk.X_SIZE) + (Chunk.X_SIZE / 2), ((8 - ry) * Chunk.Y_SIZE) + (Chunk.Y_SIZE / 2), 21));
                if (Helper.Random.Next(5) == 1) PickupController.Instance.Spawn(PickupType.Health, this, VoxelWorld.ToScreenSpace(((14 - rx) * Chunk.X_SIZE) + (Chunk.X_SIZE / 2), ((8 - ry) * Chunk.Y_SIZE) + (Chunk.Y_SIZE / 2), 21));
            }

            for (int i = 0; i < 1; i++)
            {
                int rx = 2 + Helper.Random.Next(5);
                int t = 4 + Helper.Random.Next(3);

                World.CopySprite(rx * Chunk.X_SIZE, 4 * Chunk.X_SIZE, 14, tileSheet.AnimChunks[t], 0, 1);
                World.CopySprite((14 - rx) * Chunk.X_SIZE, 4 * Chunk.X_SIZE, 14, tileSheet.AnimChunks[t], 0, 1);
            }
            for (int i = 0; i < 1; i++)
            {
                int ry = 2 + Helper.Random.Next(2);
                int t = 4 + Helper.Random.Next(3);

                World.CopySprite(7 * Chunk.X_SIZE, ry * Chunk.X_SIZE, 14, tileSheet.AnimChunks[t], 0, 1);
                World.CopySprite(7 * Chunk.X_SIZE, (8 - ry) * Chunk.X_SIZE, 14, tileSheet.AnimChunks[t], 0, 1);
            }

            //if (!((rx == 1 || rx == 14) && ry == 4) && !((ry == 1 || ry == 8) && rx == 7))
            //{

            //    World.CopySprite(rx * Chunk.X_SIZE, ry * Chunk.X_SIZE, 14, tileSheet.AnimChunks[3], 0);

            //}

            int enemiesSpawned = 0;
            bool headSpawned = false;
            bool oozeSpawned = false;
            int numEnemies = 0 + Helper.Random.Next(6);
            while (enemiesSpawned < numEnemies)
            {
                for (int x = 1; x < 14; x++)
                    for (int y = 1; y < 8; y++)
                    {
                        if (!World.GetVoxel((x * Chunk.X_SIZE) + (Chunk.X_SIZE / 2), (y * Chunk.Y_SIZE) + (Chunk.Y_SIZE / 2), 23).Active)
                        {
                            // Create an enemy?
                            if (!(x == 7 && y == 4))
                            {
                                if (enemiesSpawned < numEnemies && Helper.Random.Next(50) == 1)
                                {
                                    

                                    EnemyType type = (EnemyType)Helper.Random.Next(Enum.GetValues(typeof(EnemyType)).Length);
                                    if (type != EnemyType.Head)
                                    {
                                        if (type == EnemyType.Ooze && oozeSpawned) continue;
                                        if(type== EnemyType.Ooze) oozeSpawned = true;

                                        EnemyController.Instance.Spawn(type, VoxelWorld.ToScreenSpace((x * Chunk.X_SIZE) + (Chunk.X_SIZE / 2), (y * Chunk.Y_SIZE) + (Chunk.Y_SIZE / 2), 21), this);
                                        enemiesSpawned++;
                                    }
                                }
                            }
                            if (x == 7 && y == 4)
                            {
                                if (enemiesSpawned < numEnemies && !headSpawned && Helper.Random.Next(5) == 1)
                                {
                                    enemiesSpawned++;
                                    EnemyController.Instance.Spawn(EnemyType.Head, VoxelWorld.ToScreenSpace((x * Chunk.X_SIZE) + (Chunk.X_SIZE / 2), (y * Chunk.Y_SIZE) + (Chunk.Y_SIZE / 2), 21), this);
                                    headSpawned = true;
                                }
                            }
                        }
                    }
            }

            World.UpdateWorldMeshes();
        }
    }
}
