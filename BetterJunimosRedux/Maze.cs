namespace BetterJunimosRedux
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using StardewModdingAPI;
    using StardewValley;
    using StardewValley.Buildings;
    using StardewValley.TerrainFeatures;

    /// <summary>
    /// A maze made of crops.
    /// </summary>
    public class Maze
    {
        private MazeTileType[,] mazeTiles;

        private Maze(int radius, MazeTileType[,] maze = null)
        {
            if (maze is null)
            {
                int size = (2 * radius) + 1;
                maze = new MazeTileType[size, size];
            }

            if (radius <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }

            this.mazeTiles = this.GetMaze(radius, maze);
        }

        private enum MazeTileType
        {
            Wall,
            FixedWall,
            Path,
            Hut,
            HutDoor,
        }

        /// <summary>
        /// Makes a maze for the hut with the given guid.
        /// </summary>
        /// <param name="hutGuid">The hut's guid.</param>
        public static void MakeMazeForHut(Guid hutGuid)
        {
            JunimoHut hut = Utils.GetHutFromGuid(hutGuid);
            if (hut == null)
            {
                ModEntry.SMonitor.Log("Hut is null", LogLevel.Error);
                return;
            }

            Maze maze = new Maze(ModEntry.BetterJunimosApi.GetJunimoHutMaxRadius(), GetFixedWallsForHut(hut));
            string[,] cropTypes = new string[maze.mazeTiles.GetLength(0), maze.mazeTiles.GetLength(1)];
            for (int x = 0; x < maze.mazeTiles.GetLength(0); x += 1)
            {
                for (int y = 0; y < maze.mazeTiles.GetLength(1); y += 1)
                {
                    cropTypes[x, y] = maze.mazeTiles[x, y] == MazeTileType.Wall ? BetterJunimos.CropTypes.Trellis : BetterJunimos.CropTypes.Ground;
                }
            }

            BetterJunimos.CropMap cropMap = new BetterJunimos.CropMap()
            {
                Map = cropTypes,
            };

            ModEntry.BetterJunimosApi.SetCropMapForHut(hutGuid, cropMap);
        }

        /// <summary>
        /// Clears a maze for the hut with the given guid.
        /// </summary>
        /// <param name="hutGuid">The hut's guid.</param>
        public static void ClearMazeForHut(Guid hutGuid)
        {
            ModEntry.BetterJunimosApi.ClearCropMapForHut(hutGuid);
        }

        private static MazeTileType[,] GetFixedWallsForHut(JunimoHut hut)
        {
            int radius = ModEntry.BetterJunimosApi.GetJunimoHutMaxRadius();
            int sx = hut.tileX.Value - radius + 1;
            int sy = hut.tileY.Value - radius + 1;
            int size = (2 * radius) + 1;
            MazeTileType[,] maze = new MazeTileType[size, size];
            GameLocation location = hut.GetParentLocation();

            for (int x = 0; x < size; x += 1)
            {
                for (int y = 0; y < size; y += 1)
                {
                    Vector2 pos = new Vector2(x + sx, y + sy);

                    // trellis crop check
                    if (location.terrainFeatures.ContainsKey(pos))
                    {
                        HoeDirt hoeDirt = location.terrainFeatures[pos] as HoeDirt;
                        if (hoeDirt != null && hoeDirt.crop != null)
                        {
                            if (!hoeDirt.crop.dead.Value && hoeDirt.crop.raisedSeeds.Value)
                            {
                                maze[x, y] = MazeTileType.FixedWall;
                                continue;
                            }
                        }
                    }

                    // general obstruction check
                    if (Utils.GetIsOccupied(location, pos))
                    {
                        maze[x, y] = MazeTileType.FixedWall;
                    }
                }
            }

            return maze;
        }

        private MazeTileType[,] GetMaze(int radius, MazeTileType[,] maze)
        {
            int size = (2 * radius) + 1;

            this.PlaceHut(maze);

            // start at the hut door and visit all rooms in the maze
            int endX = radius + 1;
            int endY = radius + 1;
            this.Visit(maze, endX, endY);

            // pick a random spot at the bottom of the grid as the maze entry
            int exitIndex = Game1.random.Next(size);
            for (int x = 0; x < size; x += 1)
            {
                int checkX = (x + exitIndex) % size;
                if (maze[checkX, maze.GetLength(1) - 1] == MazeTileType.FixedWall || maze[checkX, maze.GetLength(1) - 2] != MazeTileType.Path)
                {
                    continue;
                }

                maze[checkX, maze.GetLength(1) - 1] = MazeTileType.Path;
                break;
            }

            return maze;
        }

        private void PlaceHut(MazeTileType[,] maze)
        {
            int width = (maze.GetLength(0) / 2) - 1;
            int height = (maze.GetLength(1) / 2) - 1;
            for (int x = width; x < width + 3; x += 1)
            {
                for (int y = height; y < height + 2; y += 1)
                {
                    maze[x, y] = MazeTileType.Hut;
                }
            }

            maze[width + 1, height + 2] = MazeTileType.HutDoor;
        }

        private void Visit(MazeTileType[,] maze, int visitX, int visitY)
        {
            maze[visitX, visitY] = MazeTileType.Path;

            IOrderedEnumerable<Vector2> directions = Directions.All.OrderBy((d) => Game1.random.Next());
            foreach ((float directionX, float directionY) in directions)
            {
                int tileX = visitX + (int)directionX;
                int tileY = visitY + (int)directionY;
                int tile2X = visitX + ((int)directionX * 2);
                int tile2Y = visitY + ((int)directionY * 2);

                if (!this.GetIsUnvisitedRoom(maze, tileX, tileY) || maze[tile2X, tile2Y] == MazeTileType.FixedWall)
                {
                    continue;
                }

                maze[tileX, tileY] = MazeTileType.Path;
                this.Visit(maze, tile2X, tile2Y);
            }
        }

        private bool GetIsRoom(MazeTileType[,] maze, int x, int y)
        {
            if (maze[x, y] == MazeTileType.HutDoor)
            {
                return true;
            }

            return (x % 2) == 1 && (y % 2) == 1;
        }

        private bool GetIsUnvisitedRoom(MazeTileType[,] maze, int x, int y)
        {
            bool wall = false;

            try
            {
                if (this.GetIsRoom(maze, x, y) && maze[x, y] == MazeTileType.Wall)
                {
                    wall = true;
                }

                if (this.GetIsRoom(maze, x, y) && maze[x, y] == MazeTileType.HutDoor)
                {
                    wall = true;
                }
            }
            catch (IndexOutOfRangeException)
            {
                wall = false;
            }

            return wall;
        }

        private static class Directions
        {
            public static readonly Vector2 Up = new Vector2(0, 1);

            public static readonly Vector2 Right = new Vector2(1, 0);

            public static readonly Vector2 Down = new Vector2(0, -1);

            public static readonly Vector2 Left = new Vector2(-1, 0);

            public static readonly List<Vector2> All = new List<Vector2>()
            {
                Up,
                Right,
                Down,
                Left,
            };
        }
    }
}