using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DungeonTester
{
    public class FloorMap
    {
        //This about defining a value type, such as UInt16 for the FloorMap, so changing data sizes is easier down the road
        //Can't see any reason to go beyond UInt16 for range of indexes, reduces memory footprint in half if done

        public const UInt32 MIN_FLOOR_WIDTH      = 4;
        public const UInt32 MIN_FLOOR_HEIGHT     = 4;
        public const UInt32 DEFAULT_FLOOR_WIDTH  = 1024;
        public const UInt32 DEFAULT_FLOOR_HEIGHT = 1024;
        public const UInt32 MAX_FLOOR_WIDTH      = 4096;
        public const UInt32 MAX_FLOOR_HEIGHT     = 4096;
        public const int DEFAULT_COMPLEXITY      = 4;

        public enum NoiseTypes { Perlin, Simplex, Gradient, Random };

        Boolean initialized = false;
        public Boolean Initialized
        {
            get { return this.initialized; }
            protected set { this.initialized = value; }
        }

        ushort[,] grid;
        public ushort[,] Grid
        {
            get { return this.grid; }
            protected set { this.grid = value; }
        }

        UInt32 width;
        public UInt32 Width
        {
            get { return this.width; }
            protected set { this.width = value; }
        }

        UInt32 height;
        public UInt32 Height
        {
            get { return this.height; }
            protected set { this.height = value; }
        }

        int complexity;
        public int Complexity
        {
            get { return this.complexity; }
            protected set { this.complexity = value; }
        }

        protected Random rand;

        int seed;
        public int Seed
        {
            get { return this.seed; }
            protected set { this.seed = value; }
        }

        public FloorMap(UInt32 floorWidth = FloorMap.DEFAULT_FLOOR_WIDTH, UInt32 floorHeight = FloorMap.DEFAULT_FLOOR_HEIGHT, int complexity = FloorMap.DEFAULT_COMPLEXITY, int seed = 0)
        {
            if (floorWidth < FloorMap.MIN_FLOOR_WIDTH || floorWidth > FloorMap.MAX_FLOOR_WIDTH)
            {
                Console.Out.WriteLine("FloorMap::FloorMap(UInt32 floorWidth, UInt32 floorHeight) -- Invalid floorWidth -- Set to Default");
                floorWidth = FloorMap.DEFAULT_FLOOR_WIDTH;
            }

            if (floorHeight < FloorMap.MIN_FLOOR_HEIGHT || floorHeight > FloorMap.MAX_FLOOR_HEIGHT)
            {
                Console.Out.WriteLine("FloorMap::FloorMap(UInt32 floorWidth, UInt32 floorHeight) -- Invalid floorHeight -- Set to Default");
                floorHeight = FloorMap.DEFAULT_FLOOR_HEIGHT;
            }

            this.Width = floorWidth;
            this.Height = floorHeight;
            
            this.Grid = new ushort[this.Width, this.Height];

            this.Complexity = complexity;
            this.Seed = seed;
            this.rand = new Random(this.Seed);
        }

        /// <summary>
        /// Deep Copy
        /// </summary>
        /// <param name="copyFrom">Object to Copy From</param>
        public FloorMap(FloorMap copyFrom)
        {
            this.Grid = copyFrom.Grid;
        }

        public Boolean Clear(ushort newValue = default(ushort))
        {
            //No need to check for threading issues
            if (newValue == default(ushort))
            {
                this.Grid.Initialize();
                return true;
            }

            //Might want to check for initialization instead of bounds, as one is required for the other and would reduce a step
            //Check Bounds
            if (this.Width > 0 && this.Height > 0)
            {
                //foreach for reading
                //for for writing
                for (UInt32 ix = 0; ix < this.Width; ix++)
                {
                    for (UInt32 iy = 0; iy < this.Height; iy++)
                    {
                        this.Grid[ix, iy] = newValue;
                    }                                    
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Generates a FloorMap object, and initializes it
        /// </summary>
        /// <param name="noise">The type of Noise algorithm to use</param>
        /// <param name="noiseThreshold">The value that the noise has to be above to generate a room</param>
        /// <param name="complexity">The value that controls the parsing of rooms from the noise</param>
        /// <returns>True if the floor is generated completely</returns>
        public Boolean GenerateFloor(NoiseTypes noise, Double noiseThreshold)
        {
            // Add some default values to the parameters

            //can't call if the object is null, so don't need to check

            //Sizes were taken care of previously during creation of object, assume they are safe

            //grid value = floor(noise_value * (Complexity + 1))

            switch (noise)
            {
                case NoiseTypes.Perlin:
                    this.Initialized = this.generatePerlinNoise(noiseThreshold, complexity);
                    break;
                case NoiseTypes.Simplex:
                    this.Initialized = this.generateSimplexNoise(noiseThreshold, complexity);
                    break;
                case NoiseTypes.Gradient:
                    this.Initialized = this.generateGradientNoise(noiseThreshold, complexity);
                    break;
                case NoiseTypes.Random:
                    this.Initialized = this.generateRandomNoise(noiseThreshold);
                    break;
                default:
                    //Response with an error message
                    this.Initialized = false;
                    break;
            }

            return this.Initialized;
        }

        protected Boolean generatePerlinNoise(Double noiseThreshold, int complexity)
        {
            return false;
        }

        protected Boolean generateSimplexNoise(Double noiseThreshold, int complexity)
        {
            return false;
        }

        protected Boolean generateGradientNoise(Double noiseThreshold, int complexity)
        {
            return false;
        }

        protected Boolean generateRandomNoise(Double noiseThreshold)
        {
            //Easiest to implement and see any flaws with the system, so it is made first

            int comp = this.Complexity + 1; //Don't need to use this value any where else in this scope

            Random rand = new Random(this.Seed);

            for (UInt32 ix = 0; ix < this.Width; ix++)
            {
                for (UInt32 iy = 0; iy < this.Height; iy++)
                {
                    this.Grid[ix, iy] = (ushort)rand.Next(comp);
                }
            }

            return true; //Hard to fail this method

            //Could try / catch, but that only accomplish a slower system and no additional functionality
        }

        /// <summary>
        /// Replaces a previous value with a new value
        /// </summary>
        /// <param name="floorX">First Dimension</param>
        /// <param name="floorY">Second Dimension</param>
        /// <param name="newValue">Value to apply</param>
        /// <returns></returns>
        public Boolean SetPoint(UInt32 floorX, UInt32 floorY, ushort newValue)
        {
            //Check Bounds -- Doesn't work when dealing with the scaled floor grid -- check later if || or && is faster
            if (floorX < 0 || floorX > this.Width)
            {
                Console.Out.WriteLine("FloorMap::SetPoint(UInt32 floorX, UInt32 floorY, ushort newValue) -- Invalid floorX -- Out of Bounds");
                return false;
            }

            if (floorY < 0 || floorY > this.Height)
            {
                Console.Out.WriteLine("FloorMap::SetPoint(UInt32 floorX, UInt32 floorY, ushort newValue) -- Invalid floorY -- Out of Bounds");
                return false;
            }

            this.Grid[floorX, floorY] = newValue;

            return true;
        }
    }
}
