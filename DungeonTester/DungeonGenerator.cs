using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DungeonTester
{
    /// <summary>
    /// Creates and manages data regarding the dungeon as a whole.
    /// Passes size and noise properties to create a dungeon floor to FloorMap.
    /// Handles assigning rooms and reading generated noise.
    /// </summary>
    class DungeonGenerator
    {
        // Constants
        private const int PROCESS_COUNT = 4; // I really should lock down size and other factors into powers of 2 to make things easily

        // Too much power, might not have enough responsibility otherwise.

        Boolean initialized = false;

        UInt32 width = 1024;
        public UInt32 Width
        {
            get { return width; }
            protected set { width = value; }
        }
        
        UInt32 height = 1024;
        public UInt32 Height
        {
            get { return height; }
            protected set { height = value; }
        }

        /// <summary>
        /// A metric for describing the number of rooms.
        /// A higher value generates many small rooms, and a smaller value generates more larger rooms.
        /// When applied to a FloorMap, it changes the noise value to the ceiling of the (noise_range / complexity) multiple.
        /// </summary>
        int complexity = 4;
        public int Complexity
        {
            get { return complexity; }
            protected set { complexity = value; }
        }

        /// <summary>
        /// The value by which smaller noise values will be culled from being considered rooms (set to 0)
        /// This value is not superceded by Complexity, and this value can cull into the Complexity algorithm if not low enough.
        /// </summary>
        Double roomNoiseThreshold;
        public Double RoomNoiseThreshold
        {
            get { return this.roomNoiseThreshold; }
            protected set { this.roomNoiseThreshold = value; }
        }

        /// <summary>
        /// Base seed to use to generate the whole dungeon.
        /// Used to generate floor seeds
        /// </summary>
        int seed;
        public int Seed
        {
            get { return this.seed; }
            protected set { this.seed = value; }
        }

        Random rand;
        protected Random Rand
        {
            get { return this.rand; }
            set { this.rand = value; }
        }

        //There is a lot more boundary checking I can and should do for those 4 below
        //Check is min is bigger than max, and vice versa
        //a negative will just throw a compile error, so shouldn't be a problem
        //Attached default values are arbitrary
        UInt32 minWidth = 4;
        public UInt32 MinWidth
        {
            get { return minWidth; }
            protected set { minWidth = ( value < 3 ) ? 3 : value; }
        }

        UInt32 minHeight = 4;
        public UInt32 MinHeight
        {
            get { return minHeight; }
            protected set { minHeight = ( value < 3 ) ? 3 : value; }
        }

        UInt32 maxWidth = 32;
        public UInt32 MaxWidth
        {
            get { return maxWidth; }
            protected set { maxWidth = (value > width ) ? width : value; }
        }

        UInt32 maxHeight = 32;
        public UInt32 MaxHeight
        {
            get { return maxHeight; }
            protected set { maxHeight = ( value > height ) ? height : value; }
        }

        /// <summary>
        /// Cache for the next FloorMap to be generated from
        /// </summary>
        short[,] noiseMap;
        protected short[,] NoiseMap
        {
            get { return this.noiseMap; }
            set { this.noiseMap = value; }
        }

        // Might want to look into having a previous floor and next floor NoiseMap structure

        List<FloorMap> floors;
        public List<FloorMap> Floors
        {
            get { return this.floors; }
            protected set { this.floors = value; } //may want to think about overloading this to simply add the value instead of copying
        }

        public Boolean setupDungeon(UInt32 DungeonHeight = 1024, UInt32 DungeonWidth = 1024, int RoomComplexity = 4, 
            UInt32 RoomMinWidth = 4, UInt32 RoomMinHeight = 4, int seed = 0)
        {
            this.Complexity = RoomComplexity;
            this.MinWidth = RoomMinWidth;
            this.MinHeight = RoomMinHeight;
            this.Width = DungeonWidth;
            this.Height = DungeonHeight;

            this.Seed = seed;

            this.floors = new List<FloorMap>(); //think about pre-defining capacity

            //Need to properly initialize here

            initialized = true;
            return initialized;
        }

        public Boolean nextFloor(UInt32 FloorWidth = 1024, UInt32 FloorHeight = 1024, int Seed = -1)
        {
            //This current setup only supports one floor size. Allow setup to have nextWidth and such, and add floor dynamically
            return true;
        }

        /// <summary>
        /// Generates a new Dungeon Floor, adding it to the end of the list.
        /// </summary>
        public void Generate()
        {
            //empty floor map
            FloorMap tempFloor;// = new FloorMap(this.Width, this.Height);

            UInt32 scaledWidth = this.Width / this.MinWidth;
            UInt32 scaledHeight = this.Height / this.MinHeight;
            FloorMap scaledFloor = new FloorMap(scaledWidth, scaledHeight, this.Complexity);
            //get noise type from MainWindow

            //Using Random for testing purposes
            FloorMap.NoiseTypes noiseType = FloorMap.NoiseTypes.Random;

            scaledFloor.GenerateFloor(noiseType, 0.5);

            //Fill out the tempFloor by multiplying the coords by the scaling factor
            tempFloor = ExpandFloor(scaledFloor);

            floors.Add(tempFloor);
        }

        /// <summary>
        /// Read a Noise Map and interpret into a FloorMap
        /// </summary>
        /// <param name="sourceFloor"></param>
        /// <returns></returns>
        protected FloorMap ExpandFloor(short[,] sourceNoiseMap, int sourceComplexity = FloorMap.DEFAULT_COMPLEXITY, int sourceSeed = FloorMap.DEFAULT_SEED)
        {
            //Tempted to make a constructor to do this for me automatically. Might not be a bad idea.
            //Think about making use of global scaling instead of per axis
            FloorMap tempFloor = new FloorMap((UInt32)sourceNoiseMap.GetLength(0) * this.MinWidth, (UInt32)sourceNoiseMap.GetLength(1) * this.MinHeight, sourceComplexity, sourceSeed);

            //how do I want to do this? It would likely be best to simply loop through tempFloor, and read the value from (ushort)floor(ix / scale) and (ushort)floor(iy / scale)

            // The current idea of how to generate a floor, is to generate a noiseMap, and then extrapolate that into a FloorMap.
            // On loading a map, the map gets populated, which entails placing default objects, and then applying the delta (if exists)
            // I will likely require a method to determine if a FloorTile is "active" so that mobs and events are rendered.

            // If I didn't need to track the position with an index, I'd love to do a simple foreach. Is it possible?
            // If I can't, is it possible to break the problem into 4 threads to run independently? Will need a lock.

            for (UInt32 ix = 0; ix < tempFloor.Width; ix++)
            {
                for (UInt32 iy = 0; iy < tempFloor.Height; iy++)
                {
                    //Read the value -- Don't like this multiple casting. Look into a redesign.
                    //Might be better to read the sourceFloor once, then apply the value using another set of for loops.
                    //If the scaling factor was set, could hard code it in. Think about doing that for small standard scaling (4 or 8).
                    tempFloor.SetPoint(ix, iy, sourceFloor.Floor[(UInt32)Math.Floor((double)ix / this.MinWidth), (UInt32)Math.Floor((double)iy / this.MinHeight)]);

                    //This will work for a rough start
                }
            }

            return tempFloor;
        }
    }
}
