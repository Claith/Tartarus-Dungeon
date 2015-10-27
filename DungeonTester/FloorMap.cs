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

        //Need to implement two different data structures.
        //1. A hard data structure to handle the information in a 32bit or 64bit sized system, instead of shorts
        //Will likely require unsafe data manipulation. Could be fun. Save as binary.
        //2. A form for exporting / importing the data to / from a sql database.
        //The database will need extra information, which isn't used here. I'd rather make it "complete" by default than 
        //make an adapter down the road.

        //Move a copy of this into documentation
        //Each tile will take 8 bits for type
        //  - 1bit to enable / disable tile (shown as black "unexplored" tile)
        //  - 2bits to show "room edges" / "cliffs" (4 options)
        //      - 00 = Normal Floor
        //      - 01 = No Floor (hole in ground)
        //      - 10 = Fake Wall (Illusionary Wall?)
        //      - 11 = Wall
        //  - 5bits to show pallete selection (32 options - 23 mapped + 9 extras)
        //      - 00000 = Stone
        //      - 00001 = Fire
        //      - 00010 = Steam
        //      ...
        //      - 10111 = Skylane (last mapped)
        //      ...
        //      - 11111 = Extra 9
        //Each tile will take 16 (8x2) bits for the tile content
        //  - 4bits for object parity (Type)
        //      - 0001 = Ground Decoration
        //      - 0010 = Constant Effect (i.e. On Fire)
        //      - 0100 = Monster Spawner
        //      - 1000 = Item Object (Vase, Chest, Stack of dropped items, Corpse, etc)
        //  - 4bits for wire placement - 1bit per wire type (Terraria Style)
        //      - 0001 = Red Wire
        //      - 0010 = Blue Wire
        //      - 0100 = Green Wire
        //      - 1000 = Yellow Wire

        //Objects need to possess their own properties and aren't fixed.
        //A corpse would have items, but the items wouldn't be part of the corpse.
        //256 object ids wouldn't be enough. If I include the left over 6 bits here, I can support 16384 object ids. Enough
        //Do I need those extra bits for anything else? Tenatively "no".
        //  - 6bits + 8bits for object ids
        //Since this is already at 24 bits, wouldn't it be easier to just define a tile as 32 bits, 
        //save on processing the bits into multiple tiles and maybe find a use for those bits later on? lighting levels?

        //Question. Is it possible to use the coordinates as the object id? Should be as long as they are quick to search.
        //Only need 10 bits?
        //Inventories determined on use / death.
        //Mobs are saved as a "monster spawner" to new location.
        //Need to care for overlapping objects, such as a monster + decorative rocks + fire on ground. 2 bits for parity
        //Need layer for wires. last 4 bits.
        //each tile takes 16bits total then.

        public const UInt32 MIN_FLOOR_WIDTH      = 4;
        public const UInt32 MIN_FLOOR_HEIGHT     = 4;
        public const UInt32 DEFAULT_FLOOR_WIDTH  = 1024;
        public const UInt32 DEFAULT_FLOOR_HEIGHT = 1024;
        public const UInt32 MAX_FLOOR_WIDTH      = 4096;
        public const UInt32 MAX_FLOOR_HEIGHT     = 4096;
        public const int DEFAULT_COMPLEXITY      = 4;
        public const int DEFAULT_SEED            = 0;

        public enum NoiseTypes { Perlin, Simplex, Gradient, Random };

        Boolean initialized = false;
        public Boolean Initialized
        {
            get { return this.initialized; }
            protected set { this.initialized = value; }
        }

        FloorTile[,] floor;
        public FloorTile[,] Floor
        {
            get { return this.floor; }
            protected set { this.floor = value; }
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
            
            this.Floor = new FloorTile[this.Width, this.Height];

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
            this.Floor = copyFrom.Floor;

            //Add more fields obviously
        }

        public Boolean Clear(Int32 newValue = default(Int32))
        {
            //No need to check for threading issues -- might be able to move to a foreach, then clear()
            if (newValue == default(Int32))
            {
                this.Floor.Initialize();
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
                        this.Floor[ix, iy] = newValue;
                    }                                    
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts Floor Data into a serialized form for storage.
        /// </summary>
        /// <param name="readFrom">FloorMap to serialize</param>
        /// <returns>Serialized Grid representing the floor</returns>
        public static Int16[,] SerializeDungeon(FloorMap readFrom)
        {
            Int16[,] tempGrid = new Int16[readFrom.Width, readFrom.Height];

            foreach(

            return tempGrid;
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
                    this.Floor[ix, iy] = (Int16)rand.Next(comp);
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
        public Boolean SetPoint(UInt32 floorX, UInt32 floorY, Int16 newValue)
        {
            //Check Bounds -- Doesn't work when dealing with the scaled floor grid -- check later if || or && is faster
            if (floorX < 0 || floorX > this.Width)
            {
                Console.Out.WriteLine("FloorMap::SetPoint(UInt32 floorX, UInt32 floorY, Int32 newValue) -- Invalid floorX -- Out of Bounds");
                return false;
            }

            if (floorY < 0 || floorY > this.Height)
            {
                Console.Out.WriteLine("FloorMap::SetPoint(UInt32 floorX, UInt32 floorY, Int32 newValue) -- Invalid floorY -- Out of Bounds");
                return false;
            }

            this.Floor[floorX, floorY] = newValue;

            return true;
        }
    }
}
