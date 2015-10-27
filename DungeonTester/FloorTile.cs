// FloorTile.cs
// By: Christopher "Claith" Smith
// Description:
// Untested
// Should be feature complete, aside TileTheme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DungeonTester
{
    public class FloorTile
    {
        /// <summary>
        /// Class of tile to use.
        /// </summary>
        public static enum TileType { FLOOR = 0, HOLE, ILLUSION_WALL, WALL };

        //Implement the rest 23 + 9 special themes = 32
        public static enum TileTheme { STONE = 0, FIRE, STEAM, LAVA, FIRE_STORM, BURNED, FIRE_PIT, SMOKE,
            WATER, SWAMP, RAIN, ICE, FLOODED, CRYSTAL,
            EARTH, FLOATING_ROCKS, DESERT, MIASMA,
            AIR, SNOW, SAND_STORM, SKY_LANE,
            FROZEN_FLAME, SHADOW, OASIS}; // 25/32

        // Need to decide how I want to store the floor itself. I have two choices.
        // 1. I can store all the data raw as it happens. This is massive in size. Compression may help, but could have issues with performance.
        // 2. I can store the delta of the floor compared to the generated floor tiles. This will have a smaller data size. This is the way to go.
        // Still need to make the serialize for each tile. Just not sure if I should figure out if a delta is needed or not first.
        // The first step to determine if a delta is needed is if the tile is enabled (thus created), if so then it needs to check if there is any difference.
        // Checking the differences with the serialized data will likely be the fastest method instead of a series of if / elses, but the if chain
        // will be able to short circuit and save a few steps if I can decide which aspects are the most likely to change between the generated and the current.
        // If a tile was "saved" before, it should always be saved as part of the delta as well. Need to include a flag for this.

        Boolean forceSave;
        public Boolean ForceSave
        {
            get { return this.forceSave; }
            protected set { this.forceSave = value; }
        }

        Boolean enabled;
        public Boolean Enabled
        {
            get { return this.enabled; }
            protected set {
                this.enabled = value;
            }
        }

        TileType type;
        public TileType Type
        {
            get { return this.type; }
            protected set { this.type = value; }
        }

        TileTheme theme;
        public TileTheme Theme
        {
            get { return this.theme; }
            protected set { this.theme = value; }
        }

        Boolean itemExists;
        public Boolean ItemExists
        {
            get { return this.itemExists; }
            protected set { this.itemExists = value; }
        }

        Boolean monsterExists;
        public Boolean MonsterExists
        {
            get { return this.monsterExists; }
            protected set { this.monsterExists = value; }
        }

        Boolean constantEffectExists;
        public Boolean ConstantEffectExists
        {
            get { return this.constantEffectExists; }
            protected set { this.constantEffectExists = value; }
        }

        Boolean decorationExists;
        public Boolean DecorationExists
        {
            get { return this.decorationExists; }
            protected set { this.decorationExists = value; }
        }

        Boolean redWire;
        public Boolean RedWire
        {
            get { return this.redWire; }
            protected set { this.redWire = value; }
        }

        Boolean blueWire;
        public Boolean BlueWire
        {
            get { return this.blueWire; }
            protected set { this.blueWire = value; }
        }

        Boolean greenWire;
        public Boolean GreenWire
        {
            get { return this.greenWire; }
            protected set { this.greenWire = value; }
        }

        Boolean yellowWire;
        public Boolean YellowWire
        {
            get { return this.yellowWire; }
            protected set { this.yellowWire = value; }
        }

        //Think about switching to "Seen" instead of "Known"
        Boolean known;
        /// <summary>
        /// True if the tile is enabled AND has light cast on it.
        /// </summary>
        public Boolean Known
        {
            get {
                if (!this.Enabled) //Untested - Not sure this is needed
                    return this.Enabled;
                else
                    return this.known;
            }
            protected set { this.known = value; }
        }

        public FloorTile()
        {
            this.Clear();
        }

        public void SaveOn() // Not sure these functions are needed
        {
            this.ForceSave = true;
        }

        public void SaveOff()
        {
            this.ForceSave = false;
        }

        public void Clear()
        {
            this.Enabled = false;
            this.Type = TileType.FLOOR;
            this.ItemExists = false;
            this.MonsterExists = false;
            this.ConstantEffectExists = false;
            this.DecorationExists = false;
            this.YellowWire = false;
            this.GreenWire = false;
            this.BlueWire = false;
            this.RedWire = false;
            this.Theme = TileTheme.STONE;
        }

        #region Serialize Region

        /// <summary>
        /// Converts all needed information for saving the FloorTile Object into a condensed form (16 bits).
        /// </summary>
        /// <returns>A 16-bit representation of the FloorTile Object. If Negative, the tile is disabled.</returns>
        public Int16 Serialize()
        {
            return FloorTile.Serialize(this);
        }

        /// <summary>
        /// Converts all needed information for saving a tile into a condensed form (16 bits).
        /// </summary>
        /// <param name="readFrom">A Single FloorTile to be serialized</param>
        /// <returns>A 16-bit representation of the FloorTile. If Negative, the tile is disabled.</returns>
        public static Int16 Serialize(ref FloorTile readFrom) //Untested
        {
            //Need to list order and usage of bits here.

            // [10000000 00000000] - Enabled                                                    0x80 = 10000000
            // [01100000 00000000] - Tile Type (Floor, Hole, Fake Wall, Wall)                   0x40 = 01000000
            // [00010000 00000000] - Item Object (Vase, Chest, Dropped Items, Corpse, Etc)
            // [00001000 00000000] - Monster Spawn
            // [00000100 00000000] - Constant Floor Effect (Fire, etc.)
            // [00000010 00000000] - Floor Decoration (Rocks, Debris, Bones, Etc)
            // [00000001 00000000] - Yellow Wire
            // [00000000 10000000] - Green Wire
            // [00000000 01000000] - Blue Wire
            // [00000000 00100000] - Red Wire
            // [00000000 00011111] - Tile Theme (Placed at end for simple addition)


            UInt16 temp = 0;

            if (readFrom.Enabled)
            {
                temp = (UInt16)(temp | 0x8000);     // 1000 0000
            }

            switch (readFrom.Type)
            {
                case TileType.FLOOR:
                    // Keep Default Value of 0      // 0000 0000
                    break;
                case TileType.HOLE:
                    temp = (UInt16)(temp | 0x2000); // 0010 0000
                    break;
                case TileType.ILLUSION_WALL:
                    temp = (UInt16)(temp | 0x4000); // 0100 0000
                    break;
                case TileType.WALL:
                    temp = (UInt16)(temp | 0x6000); // 0110 0000
                    break;
                default:
                    //Default to WALL
                    temp = (UInt16)(temp | 0x6000); // 0110 0000
                    break;
            }

            if (readFrom.ItemExists)
            {
                temp = (UInt16)(temp | 0x1000);     // 0001 0000
            }

            if (readFrom.MonsterExists)
            {
                temp = (UInt16)(temp | 0x0800);     // 0000 1000
            }

            if (readFrom.ConstantEffectExists)
            {
                temp = (UInt16)(temp | 0x0400);     // 0000 0100
            }

            if (readFrom.DecorationExists)
            {
                temp = (UInt16)(temp | 0x0200);     // 0000 0010
            }

            if (readFrom.YellowWire)
            {
                temp = (UInt16)(temp | 0x0100);
            }

            if (readFrom.GreenWire)
            {
                temp = (UInt16)(temp | 0x0080);
            }

            if (readFrom.BlueWire)
            {
                temp = (UInt16)(temp | 0x0040);
            }

            if (readFrom.RedWire)
            {
                temp = (UInt16)(temp | 0x0020);
            }

            // Should just add the Theme value to the variable, no need for a long switch statement or bit shifting
            temp = (UInt16)(temp + (UInt16)readFrom.Theme);

            return (Int16)temp; //Using a signed variable to assist determining if the tile is enabled or not
        }

        #endregion Serialize Regions

        public static FloorTile Deserialize(Int16 readFrom) //Untested
        {
            // [10000000 00000000] - Enabled                                                    0x80 = 10000000
            // [01100000 00000000] - Tile Type (Floor, Hole, Fake Wall, Wall)                   0x40 = 01000000
            // [00010000 00000000] - Item Object (Vase, Chest, Dropped Items, Corpse, Etc)
            // [00001000 00000000] - Monster Spawn
            // [00000100 00000000] - Constant Floor Effect (Fire, etc.)
            // [00000010 00000000] - Floor Decoration (Rocks, Debris, Bones, Etc)
            // [00000001 00000000] - Yellow Wire
            // [00000000 10000000] - Green Wire
            // [00000000 01000000] - Blue Wire
            // [00000000 00100000] - Red Wire
            // [00000000 00011111] - Tile Theme (Placed at end for simple addition)

            FloorTile temp = new FloorTile();
            Int16 tempRead;

            // If Negative - thankfully casting doesn't change the sign bit
            if (readFrom < 0)
            {
                temp.Enabled = true;
            }

            tempRead = (Int16)(readFrom & 0x6000);
            switch (tempRead)
            {
                case 0x0000:
                    temp.Type = TileType.FLOOR;
                    break;
                case 0x2000:
                    temp.Type = TileType.HOLE;
                    break;
                case 0x4000:
                    temp.Type = TileType.ILLUSION_WALL;
                    break;
                case 0x6000:
                    temp.Type = TileType.WALL;
                    break;
                default:
                    temp.Type = TileType.WALL;
                    // Throw a console error
                    Console.Out.WriteLine("FloorTile.cs::Deserialize(Int16 readFrom) -- Impossible Error, TileType doesn't exist");
                    break;
            }

            // From this point on, I'm trying to decide if just bitshifting would be quicker or not.
            // Possible performance gain in this function.
            temp.ItemExists = (readFrom & 0x1000) != 0 ? true : false;
            temp.MonsterExists = (readFrom & 0x0800) != 0 ? true : false;
            temp.ConstantEffectExists = (readFrom & 0x0400) != 0 ? true : false;
            temp.DecorationExists = (readFrom & 0x0200) != 0 ? true : false;

            temp.YellowWire = (readFrom & 0x0100) != 0 ? true : false;
            temp.GreenWire = (readFrom & 0x0080) != 0 ? true : false;
            temp.BlueWire = (readFrom & 0x0040) != 0 ? true : false;
            temp.RedWire = (readFrom & 0x0020) != 0 ? true : false;

            tempRead = (Int16)(readFrom & 0x001F);
            temp.Theme = (TileTheme)tempRead;

            return temp;
        }
    }
}
