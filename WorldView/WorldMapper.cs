﻿namespace MoreTerra
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Text;
	using System.IO;
    using System.Windows.Forms;
	using MoreTerra.Utilities;
	using MoreTerra.Structures;

    public class WorldMapper
    {
        // It should be Tiletype enum but my hand hurts too much to fix.
        public static Dictionary<int, TileProperties> tileTypeDefs;

        private List<Chest> chests;
        private Dictionary<TileType, List<SymbolLoc>> tileSymbolsToAdd;
        private TileType[,] tiles;
        private Boolean[,] processed;
		private List<String> alreadyDenied;

        private WorldHeader worldHeader;
        private WorldReader reader;

        int maxX, maxY;

        public int progress = 0;
		
        public WorldMapper()
        {
            tileTypeDefs = new Dictionary<int, TileProperties>(255);
            chests = new List<Chest>();
			alreadyDenied = new List<String>();
        }

        public WorldHeader Header
        {
            get
            {
                return this.worldHeader;
            }
        }

        public void Initialize()
        {
            // :OHGOD:
            tileTypeDefs[0] = new TileProperties(TileType.Dirt, false, Constants.Colors.DIRT);
            tileTypeDefs[1] = new TileProperties(TileType.Stone, false, Constants.Colors.STONE);
            tileTypeDefs[2] = new TileProperties(TileType.Grass, false, Constants.Colors.GRASS);
            tileTypeDefs[3] = new TileProperties(TileType.Plants, true, Constants.Colors.PLANTS);
            tileTypeDefs[4] = new TileProperties(TileType.Torches, false, Constants.Colors.LIGHT_SOURCE);
            tileTypeDefs[5] = new TileProperties(TileType.Trees, true, Constants.Colors.WOOD);
            tileTypeDefs[6] = new TileProperties(TileType.Iron, false, Constants.Colors.IRON, true);
            tileTypeDefs[7] = new TileProperties(TileType.Copper, false, Constants.Colors.COPPER, true);
			tileTypeDefs[8] = new TileProperties(TileType.Gold, false, Constants.Colors.GOLD, true);
			tileTypeDefs[9] = new TileProperties(TileType.Silver, false, Constants.Colors.SILVER, true);

            tileTypeDefs[10] = new TileProperties(TileType.Door, true, Constants.Colors.DECORATIVE);
            tileTypeDefs[11] = new TileProperties(TileType.DoorOpen, true, Constants.Colors.DECORATIVE);
            tileTypeDefs[12] = new TileProperties(TileType.Heart, true, Constants.Colors.IMPORTANT, true);
            tileTypeDefs[13] = new TileProperties(TileType.Bottles, true, Constants.Colors.DECORATIVE);
            tileTypeDefs[14] = new TileProperties(TileType.Table, true, Constants.Colors.DECORATIVE);
            tileTypeDefs[15] = new TileProperties(TileType.Chair, true, Constants.Colors.DECORATIVE);
            tileTypeDefs[16] = new TileProperties(TileType.Anvil, true, Constants.Colors.DECORATIVE);
            tileTypeDefs[17] = new TileProperties(TileType.Furnance, true, Constants.Colors.DECORATIVE);
            tileTypeDefs[18] = new TileProperties(TileType.CraftingTable, true, Constants.Colors.DECORATIVE);
            tileTypeDefs[19] = new TileProperties(TileType.WoodenPlatform, false, Constants.Colors.WOOD);

			tileTypeDefs[20] = new TileProperties(TileType.PlantsDecorative, true, Constants.Colors.PLANTS);
            tileTypeDefs[21] = new TileProperties(TileType.Chest, true, Constants.Colors.IMPORTANT, true);
			tileTypeDefs[22] = new TileProperties(TileType.Demonite, false, Constants.Colors.DEMONITE, true);
            tileTypeDefs[23] = new TileProperties(TileType.CorruptionGrass, false, Constants.Colors.CORRUPTION_GRASS);
            tileTypeDefs[24] = new TileProperties(TileType.CorruptionPlants, true, Constants.Colors.CORRUPTION_GRASS);
            tileTypeDefs[25] = new TileProperties(TileType.Ebonstone, false, Constants.Colors.EBONSTONE);
            tileTypeDefs[26] = new TileProperties(TileType.Altar, true, Constants.Colors.IMPORTANT, true);
            tileTypeDefs[27] = new TileProperties(TileType.Sunflower, true, Constants.Colors.PLANTS);
            tileTypeDefs[28] = new TileProperties(TileType.Pot, true, Constants.Colors.IMPORTANT);
            tileTypeDefs[29] = new TileProperties(TileType.PiggyBank, true, Constants.Colors.DECORATIVE);

			tileTypeDefs[30] = new TileProperties(TileType.BlockWood, false, Constants.Colors.WOOD_BLOCK);
            tileTypeDefs[31] = new TileProperties(TileType.ShadowOrb, true, Constants.Colors.IMPORTANT, true);
            tileTypeDefs[32] = new TileProperties(TileType.CorruptionVines, false, Constants.Colors.CORRUPTION_VINES);
            tileTypeDefs[33] = new TileProperties(TileType.Candle, true, Constants.Colors.LIGHT_SOURCE);
            tileTypeDefs[34] = new TileProperties(TileType.ChandlerCopper, true, Constants.Colors.LIGHT_SOURCE);
            tileTypeDefs[35] = new TileProperties(TileType.ChandlerSilver, true, Constants.Colors.LIGHT_SOURCE);
            tileTypeDefs[36] = new TileProperties(TileType.ChandlerGold, true, Constants.Colors.LIGHT_SOURCE);
			tileTypeDefs[37] = new TileProperties(TileType.Meteorite, false, Constants.Colors.METEORITE, true);
            tileTypeDefs[38] = new TileProperties(TileType.BlockStone, false, Constants.Colors.BLOCK);
            tileTypeDefs[39] = new TileProperties(TileType.BlockRedStone, false, Constants.Colors.BLOCK);

			tileTypeDefs[40] = new TileProperties(TileType.Clay, false, Constants.Colors.CLAY);
            tileTypeDefs[41] = new TileProperties(TileType.BlockBlueStone, false, Constants.Colors.DUNGEON_BLUE);
            tileTypeDefs[42] = new TileProperties(TileType.LightGlobe, true, Constants.Colors.LIGHT_SOURCE);
            tileTypeDefs[43] = new TileProperties(TileType.BlockGreenStone, false, Constants.Colors.DUNGEON_GREEN);
            tileTypeDefs[44] = new TileProperties(TileType.BlockPinkStone, false, Constants.Colors.DUNGEON_PINK);
            tileTypeDefs[45] = new TileProperties(TileType.BlockGold, false, Constants.Colors.BLOCK);
            tileTypeDefs[46] = new TileProperties(TileType.BlockSilver, false, Constants.Colors.BLOCK);
            tileTypeDefs[47] = new TileProperties(TileType.BlockCopper, false, Constants.Colors.BLOCK);
            tileTypeDefs[48] = new TileProperties(TileType.Spikes, false, Constants.Colors.SPIKES);
            tileTypeDefs[49] = new TileProperties(TileType.CandleBlue, false, Constants.Colors.LIGHT_SOURCE);

			tileTypeDefs[50] = new TileProperties(TileType.Books, true, Constants.Colors.DECORATIVE);
            tileTypeDefs[51] = new TileProperties(TileType.Web, false, Constants.Colors.WEB);
            tileTypeDefs[52] = new TileProperties(TileType.Vines, false, Constants.Colors.PLANTS);
            tileTypeDefs[53] = new TileProperties(TileType.Sand, false, Constants.Colors.SAND);
            tileTypeDefs[54] = new TileProperties(TileType.Glass, false, Constants.Colors.DECORATIVE);
            tileTypeDefs[55] = new TileProperties(TileType.Sign, true, Constants.Colors.DECORATIVE, true);
			tileTypeDefs[56] = new TileProperties(TileType.Obsidian, false, Constants.Colors.OBSIDIAN, true);
            tileTypeDefs[57] = new TileProperties(TileType.Ash, false, Constants.Colors.ASH);
			tileTypeDefs[58] = new TileProperties(TileType.Hellstone, false, Constants.Colors.HELLSTONE, true);
            tileTypeDefs[59] = new TileProperties(TileType.Mud, false, Constants.Colors.MUD);

			tileTypeDefs[60] = new TileProperties(TileType.UndergroundJungleGrass, false, Constants.Colors.UNDERGROUNDJUNGLE_GRASS);
            tileTypeDefs[61] = new TileProperties(TileType.UndergroundJunglePlants, true, Constants.Colors.UNDERGROUNDJUNGLE_PLANTS);
            tileTypeDefs[62] = new TileProperties(TileType.UndergroundJungleVines, false, Constants.Colors.UNDERGROUNDJUNGLE_VINES);
            tileTypeDefs[63] = new TileProperties(TileType.Sapphire, false, Constants.Colors.GEMS, true);
            tileTypeDefs[64] = new TileProperties(TileType.Ruby, false, Constants.Colors.GEMS, true);
            tileTypeDefs[65] = new TileProperties(TileType.Emerald, false, Constants.Colors.GEMS, true);
            tileTypeDefs[66] = new TileProperties(TileType.Topaz, false, Constants.Colors.GEMS, true);
            tileTypeDefs[67] = new TileProperties(TileType.Amethyst, false, Constants.Colors.GEMS, true);
            tileTypeDefs[68] = new TileProperties(TileType.Diamond, false, Constants.Colors.GEMS, true);
            tileTypeDefs[69] = new TileProperties(TileType.UndergroundJungleThorns, false, Constants.Colors.UNDERGROUNDJUNGLE_THORNS);

			tileTypeDefs[70] = new TileProperties(TileType.UndergroundMushroomGrass, false, Constants.Colors.UNDERGROUNDMUSHROOM_GRASS);
            tileTypeDefs[71] = new TileProperties(TileType.UndergroundMushroomPlants, true, Constants.Colors.UNDERGROUNDMUSHROOM_PLANTS);
            tileTypeDefs[72] = new TileProperties(TileType.UndergroundMushroomTrees, true, Constants.Colors.UNDERGROUNDMUSHROOM_TREES);
            tileTypeDefs[73] = new TileProperties(TileType.Plants2, true, Constants.Colors.PLANTS);
            tileTypeDefs[74] = new TileProperties(TileType.Plants3, true, Constants.Colors.PLANTS);
            tileTypeDefs[75] = new TileProperties(TileType.BlockObsidian, false, Constants.Colors.BLOCK);
            tileTypeDefs[76] = new TileProperties(TileType.BlockHellstone, false, Constants.Colors.BLOCK);
            tileTypeDefs[77] = new TileProperties(TileType.Hellforge, true, Constants.Colors.IMPORTANT, true);
            tileTypeDefs[78] = new TileProperties(TileType.DecorativePot, true, Constants.Colors.DECORATIVE);
            tileTypeDefs[79] = new TileProperties(TileType.Bed, true, Constants.Colors.DECORATIVE);

            tileTypeDefs[80] = new TileProperties(TileType.Cactus, false, Constants.Colors.CACTUS);
            tileTypeDefs[81] = new TileProperties(TileType.Coral, true, Constants.Colors.CORAL);
            tileTypeDefs[82] = new TileProperties(TileType.HerbImmature, true, Constants.Colors.HERB);
            tileTypeDefs[83] = new TileProperties(TileType.HerbMature, true, Constants.Colors.HERB);
            tileTypeDefs[84] = new TileProperties(TileType.HerbBlooming, true, Constants.Colors.HERB);
            tileTypeDefs[85] = new TileProperties(TileType.Tombstone, true, Constants.Colors.TOMBSTONE);



            tileTypeDefs[86] = new TileProperties(TileType.Unknown, false, Constants.Colors.UNKNOWN);

            for (int i = 87; i < 255; i++)
            {
                tileTypeDefs[i] = new TileProperties(TileType.Unknown, false, Color.Magenta);
            }

			tileTypeDefs[256] = new TileProperties(TileType.Spawn, false, Constants.Colors.UNKNOWN, true);
			tileTypeDefs[257] = new TileProperties(TileType.ArmsDealer, false, Constants.Colors.UNKNOWN, true);
			tileTypeDefs[258] = new TileProperties(TileType.Clothier, false, Constants.Colors.UNKNOWN, true);
			tileTypeDefs[259] = new TileProperties(TileType.Demolitionist, false, Constants.Colors.UNKNOWN, true);
			tileTypeDefs[260] = new TileProperties(TileType.Dryad, false, Constants.Colors.UNKNOWN, true);
			tileTypeDefs[261] = new TileProperties(TileType.Guide, false, Constants.Colors.UNKNOWN, true);
			tileTypeDefs[262] = new TileProperties(TileType.Merchant, false, Constants.Colors.UNKNOWN, true);
			tileTypeDefs[263] = new TileProperties(TileType.Nurse, false, Constants.Colors.UNKNOWN, true);
			tileTypeDefs[264] = new TileProperties(TileType.OldMan, false, Constants.Colors.UNKNOWN, true);

            tileTypeDefs[265] = new TileProperties(TileType.Sky, false, Constants.Colors.SKY);
            tileTypeDefs[266] = new TileProperties(TileType.Water, false, Constants.Colors.WATER);
            tileTypeDefs[267] = new TileProperties(TileType.Lava, false, Constants.Colors.LAVA);

            // Walls
            tileTypeDefs[268] = new TileProperties(TileType.WallStone, false, Constants.Colors.WALL_STONE);
            tileTypeDefs[269] = new TileProperties(TileType.WallDirt, false, Constants.Colors.WALL_DIRT);
            tileTypeDefs[270] = new TileProperties(TileType.WallStone2, false, Constants.Colors.WALL_STONE2);
            tileTypeDefs[271] = new TileProperties(TileType.WallWood, false, Constants.Colors.WALL_WOOD);
            tileTypeDefs[272] = new TileProperties(TileType.WallBrick, false, Constants.Colors.WALL_BRICK);
            tileTypeDefs[273] = new TileProperties(TileType.WallRed, false, Constants.Colors.WALL_BRICK);
            tileTypeDefs[274] = new TileProperties(TileType.WallBlue, false, Constants.Colors.WALL_DUNGEON_BLUE);
            tileTypeDefs[275] = new TileProperties(TileType.WallGreen, false, Constants.Colors.WALL_DUNGEON_GREEN);
            tileTypeDefs[276] = new TileProperties(TileType.WallPink, false, Constants.Colors.WALL_DUNGEON_PINK);
            tileTypeDefs[277] = new TileProperties(TileType.WallGold, false, Constants.Colors.WALL_BRICK);
            tileTypeDefs[278] = new TileProperties(TileType.WallSilver, false, Constants.Colors.WALL_BRICK);
            tileTypeDefs[279] = new TileProperties(TileType.WallCopper, false, Constants.Colors.WALL_BRICK);
            tileTypeDefs[280] = new TileProperties(TileType.WallHellstone, false, Constants.Colors.WALL_BRICK);
            tileTypeDefs[281] = new TileProperties(TileType.WallHellstone, false, Constants.Colors.WALL_BACKGROUND);

			// Now we set DrawSymbol for each of the symbols we can potentially draw.
			// This makes for a much faster lookup to see if we need to draw an symbol
			// rather than mass calling the DrawSymbol function.
			foreach (KeyValuePair<int, TileProperties> kv in tileTypeDefs)
			{
				if (kv.Value.HasSymbol)
				{
					kv.Value.DrawSymbol = SettingsManager.Instance.DrawMarker(kv.Value.TileType);
				}
			}
        }

        public void OpenWorld(string worldPath)
        {
            reader = new WorldReader(worldPath);
            this.worldHeader = reader.ReadHeader(true);
        }

        public void ReadWorldTiles()
        {
			Int32 i, col, row;

			progress = 0;

            maxX = (int)Header.MaxTiles.X;
            maxY = (int)Header.MaxTiles.Y;

            // Reset Symbol List
            tileSymbolsToAdd = new Dictionary<TileType, List<SymbolLoc>>();
            tiles = new TileType[maxX, maxY];
            processed = new Boolean[maxX, maxY];

            reader.SeekToTiles();

			bool isTileActive;
			TileType tileType;
			byte blockType;
			BinaryReader useReader = reader.Reader;

			byte wallType;
			bool isLighted, isLava;
			bool isWall, isLiquid;


			//Read all the tile data
			for (col = 0; col < maxX; col++)
			{
				progress = (int)(((float)col / (float)maxX) * 30f);

				for (row = 0; row < maxY; row++)
				{
					isTileActive = useReader.ReadBoolean();
					tileType = TileType.Unknown;
					blockType = 0x00;

					if (isTileActive)
					{
						blockType = useReader.ReadByte();
						if (WorldMapper.tileTypeDefs[blockType].IsImportant)
						{
							useReader.ReadInt16();
							useReader.ReadInt16();
						}
						tileType = WorldMapper.tileTypeDefs[blockType].TileType;
					}
					else
					{
						tileType = TileType.Sky;
					}
					isLighted = useReader.ReadBoolean();

					if (isLighted == true)
						isWall = true;

					isWall = useReader.ReadBoolean();
					if (isWall)
					{
						wallType = useReader.ReadByte();
						if (tileType == TileType.Unknown || tileType == TileType.Sky)
						{
							if (!WorldMapper.tileTypeDefs.ContainsKey((int)wallType + Constants.WallOffset))
							{
								tileType = TileType.Unknown;
							}
							else
							{
								tileType = WorldMapper.tileTypeDefs[(int)wallType + Constants.WallOffset].TileType;
							}

						}
					}
					isLiquid = useReader.ReadBoolean();
					if (isLiquid)
					{
						byte liquidLevel = useReader.ReadByte();
						isLava = useReader.ReadBoolean();
						if (isWall || tileType == TileType.Sky)
						{
							tileType = isLava ? TileType.Lava : TileType.Water;
						}

					}

					tiles[col, row] = tileType;
				}
			}

			// If we are drawing chests then add an empty list of Chests to populate
			if (SettingsManager.Instance.DrawMarker(TileType.Chest))
				tileSymbolsToAdd.Add(TileType.Chest, new List<SymbolLoc>());

            Dictionary<string, bool> itemFilters = SettingsManager.Instance.FilterItemStates;
            // Read the Chests
            for (i = 0; i < Constants.ChestMaxNumber; i++)
            {
                progress = (int)(((float)i / (float)Constants.ChestMaxNumber) * 20f + 30f);
                Chest chest = this.reader.GetNextChest(i);

                if (chest == null) continue;

                this.chests.Add(chest);

				// See if we are bothering to draw chests at all.
				if (SettingsManager.Instance.DrawMarker(TileType.Chest) == true)
				{
					// Find out if the chest is relevant to our interests based on what is in it.
					foreach (Item item in chest.Items)
					{
						// If we're not filtering or if we want it
						if (!SettingsManager.Instance.FilterChests || (itemFilters.ContainsKey(item.Name) && itemFilters[item.Name] == true))
						{
							// Draw the symbol
							tileSymbolsToAdd[TileType.Chest].Add(new SymbolLoc(chest.Coordinates, 1));
							break;
						}
					}
				}
            }

			if (SettingsManager.Instance.ScanForNewItems && SettingsManager.Instance.InConsole == false)
				ScanChests(this.chests);

			Sign newSign;
			// Pull all of the Signs out of the file.
			for (i = 0; i < Constants.SignMaxNumber; i++)
			{
				newSign = reader.GetNextSign(i);
				if (newSign != null)
				{
					if (SettingsManager.Instance.SymbolStates["Sign"] == true)
					{
						if (!tileSymbolsToAdd.ContainsKey(TileType.Sign))
							tileSymbolsToAdd.Add(TileType.Sign, new List<SymbolLoc>());
						tileSymbolsToAdd[TileType.Sign].Add(new SymbolLoc(newSign.signPosition, 1));
					}
				}
			}

			i = 0;
			NPC newNPC;
			while ((newNPC = reader.GetNextNPC(i)) != null)
			{
				// Convert from the NPC list with names to the one without.  This is
				// sadly needed because resources can not have spaces but some npcs
				// have spaces in their names.  I could just strip the space out, I guess.
				Int32 pos = 0;
				String markerName = String.Empty;
				TileType tt;

				foreach (String s in Constants.NPCList)
				{
					if (s == newNPC.npcName)
					{
						markerName = Constants.PeopleSymbols[pos];
						break;
					}
					pos++;
				}

				if (markerName == String.Empty)
					continue;

				tt = (TileType) Enum.Parse(typeof(TileType), markerName);

				if (SettingsManager.Instance.SymbolStates[markerName] == true)
				{
					// I didn't think we ever needed more than one of each NPC.  Then I remembered that
					// if conditions are right two nurses and three merchants can spawn.
					if (!tileSymbolsToAdd.ContainsKey(tt))
						tileSymbolsToAdd.Add(tt, new List<SymbolLoc>());
					tileSymbolsToAdd[tt].Add(new SymbolLoc(newNPC.npcPosition, 1));
				}

				i++;
			}

			progress = 50;
			
		}
	

        public void ReadChests()
        {
			Dictionary<String, Boolean> itemFilters = SettingsManager.Instance.FilterItemStates;
            progress = 0;

            this.chests = new List<Chest>();

            reader.SeekToChests();

            // Read the Chests
            for (int i = 0; i < Constants.ChestMaxNumber; i++)
            {
                Chest chest = this.reader.GetNextChest(i);

                if (chest == null) continue;
                else this.chests.Add(chest);
            }

			if (SettingsManager.Instance.ScanForNewItems && SettingsManager.Instance.InConsole == false)
				ScanChests(this.chests);
		}

		private void ScanChests(List<Chest> lstChests)
		{
			Dictionary<String, Boolean> itemFilters = SettingsManager.Instance.FilterItemStates;
			FormMessageBoxWithCheckBox dialogBox = 
				new FormMessageBoxWithCheckBox("No change", "Turn off item scanning", "New item found!");
			Boolean stopScanning = false;

			foreach (Chest c in this.chests)
			{
				foreach (Item it in c.Items)
				{
					if (!itemFilters.ContainsKey(it.Name))
					{
						if (!alreadyDenied.Contains(it.Name))
						{
							dialogBox.labelText = String.Format(
								"Item '{0}' was not in the item list." + Environment.NewLine +
								"Would you like to add it?", it.Name);

							DialogResult res = dialogBox.ShowDialog();
									 
							if (res == DialogResult.Yes)
								itemFilters.Add(it.Name, false);
							else
								alreadyDenied.Add(it.Name);

							stopScanning = dialogBox.checkBoxChecked;

							if (stopScanning == true)
							{
								SettingsManager.Instance.ScanForNewItems = false;
								return;
							}
						}
					}
				}
			}

            progress = 100;
        }

        public void CreatePreviewPNG(string outputPngPath)
        {
			Bitmap bitmap = new Bitmap(maxX, maxY, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            Graphics graphicsHandle = Graphics.FromImage((Image)bitmap);

            graphicsHandle.FillRectangle(new SolidBrush(Constants.Colors.SKY), 0, 0, bitmap.Width, bitmap.Height);

            System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
            byte[] rgbValues = new byte[bytes];
            const int byteOffset = 3;

            Dictionary<byte, Color> randomColors = new Dictionary<byte, Color>();
            Random random = new Random();

            TileProperties properties;
            TileType tileType;

            //Generate the bitmap
            int index = -1 * byteOffset;    //first increment will be 0;

            for (int row = 0; row < maxY; row++)
            {
                progress = (int)(((float)row / (float)maxY) * 50.0f + 50f);

                for (int col = 0; col < maxX; col++)
                {
                    index += byteOffset;    //increase here to avoid adding increments to each continue
                    tileType = tiles[col, row];

                    if (tileType == TileType.Sky && row > (int)this.Header.SurfaceLevel)
						tileType = TileType.WallBackground;

                    // Skip Walls
                    if (!SettingsManager.Instance.DrawWalls && tileType >= TileType.WallStone)
						tileType = TileType.Sky;

                    // Skip chests and signs because we read the coordinates in ReadWorld()
                    if (tileType == TileType.Chest || tileType == TileType.Sign) continue;

					properties = tileTypeDefs[(int)tileType];

                    if (properties.DrawSymbol)
					{
						// If we have already processed this then skip it.
						if (!processed[col, row])
						{
							Int32 tileCount = 1;
							Int32 foundCol = col;
							Int32 foundRow = row;

							#region AdvancedMarkerComment
							/*
								* First off, we know from the way the level is processed we never have to
								* check up as if something higher than the start existed we'd have hit it
								* while processing down anyways.
								* 
								* We could parse around and find only stuff that is directly connected to
								* each other and have a "perfect" look but that would take more processing
								* than I'm willing to do.  Instead we'll keep a bounding box around what
								* we've found and use that in the end.
								* 
								* To keep things from getting out of hand from player made clumps of things
								* there is a maximum size the box can be.
								* 
								* ----------
								* Pseudocode
								* ----------
								* First we check to see if we expanded since last run.
								* Then we check all rows but the bottom row for further expansion.
								* One pass one we'll skip the top row as it's also the bottom row.
								* 
								* Next we scan the side edges of the bottom row to try and expand.
								* Then we scan the row below the bottom for vertical expansion.
								* 
								* If there was vertical expansion we now have a new bottom row and
								* so we loop through scanning below the bottom row again.
								* 
								* Once we run out of vertical expansion we'll loop back (if we expanded
								* at all) and rescan the edges to see if by expanding we opened up
								* something new in those newly opened sides.
								* 
								* Once we do one full pass without any new expansion we have our final
								* bounding box so we scan straight through from one end to the other
								* and call everything that matches that we find in it our marker block.
								*/
							#endregion

							Int32 boundsMax = 32;
							Rectangle bounds = new Rectangle(col, row, 1, 1);

							Int32 screenMaxWidth = Header.MaxTiles.X;
							Int32 screenMaxHeight = Header.MaxTiles.Y;
							Boolean expandedHoriz = false;
							Boolean expandedVert = false;
							Boolean doneProcessing = true;
							Int32 i, j;


							do 
							{
								doneProcessing = true;

								for (j = bounds.Y; j < row + boundsMax; j++)
								{
									if (bounds.Bottom >= screenMaxHeight || bounds.Height >= boundsMax)
										break;

									expandedVert = false;

									// We only scan the sides if we either changed the width since last
									// loop or we are on the bottom row.
									if (expandedHoriz == true || j == (bounds.Bottom - 1))
									{
										expandedHoriz = false;

										#region ExpandLeft

										if (tiles[bounds.X, j] == tileType)
										{
											for (i = bounds.X - 1; i > (col - boundsMax); i--)
											{
												if (i <= 0)
													break;

												if (tiles[i, j] == tileType && processed[i, j] == false)
												{
													bounds.X = i;
													bounds.Width++;
													expandedHoriz = true;
													doneProcessing = false;
												}
												else
												{
													break;
												}

											}
										}
										#endregion

										#region ExpandRight
										if (tiles[bounds.Right - 1, j] == tileType)
										{
											for (i = bounds.Right; i < (bounds.X + boundsMax); i++)
											{
												if (i >= screenMaxWidth)
													break;

												if (tiles[i, j] == tileType && processed[i, j] == false)
												{
													bounds.Width++;
													expandedHoriz = true;
													doneProcessing = false;

													if (bounds.Width == boundsMax)
														break;
												}
												else
												{
													break;
												}
											}
										}
										#endregion
									}

									if (j == (bounds.Bottom - 1))
									{
										#region ExpandDown

										if (j + 1 < screenMaxHeight && bounds.Height < boundsMax)
										{
											for (i = bounds.X; i < (bounds.Right); i++)
											{
												if (tiles[i, j] == tileType)
												{
													if (tiles[i, j + 1] == tileType)
													{
														if (j + 1 >= bounds.Bottom)
														{
															bounds.Height++;
															expandedVert = true;
															doneProcessing = false;
															break;
														}
													}
												}
											}

										}
										#endregion
									}
									if (expandedVert == false)
										break;
								}
							} while(doneProcessing == false);

							tileCount = 0;
							for (j = bounds.Y; j < bounds.Bottom; j++)
								for (i = bounds.X; i < bounds.Right; i++)
									if (tiles[i, j] == tileType && processed[i, j] == false)
									{
										tileCount++;
										processed[i, j] = true;
									}

							foundCol = (int)(bounds.X + (bounds.Width / 2));
							foundRow = (int)(bounds.Y + (bounds.Height / 2));

							if (!tileSymbolsToAdd.ContainsKey(tileType))
								tileSymbolsToAdd.Add(tileType, new List<SymbolLoc>());

							tileSymbolsToAdd[tileType].Add(new SymbolLoc(new Point(foundCol, foundRow), tileCount));
						}
					}

					// This used to not draw at all if you chose to put a marker down for that type, which makes
					// sense as the marker did cover it.  However this also caused a bug where things you had not
					// chosen to put a marker didn't draw their pixel color in.  There was a different fix for
					// this but combining the fact that skipping that draw saved very little time and that
					// with the new "One symbol per set of items" instead of "One symbol per item" mechanic
					// means that you might see some of the color hanging out the edge.
                    Color color = tileTypeDefs[(int)tileType].Colour;
                    rgbValues[index + 2] = color.R;
                    rgbValues[index + 1] = color.G;
                    rgbValues[index] = color.B;

                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            bitmap.UnlockBits(bmpData);


            // Add Spawn
			if (tileTypeDefs[(int)TileType.Spawn].DrawSymbol == true)
			{
				tileSymbolsToAdd.Add(TileType.Spawn, new List<SymbolLoc>());
				tileSymbolsToAdd[TileType.Spawn].Add(new SymbolLoc(new Point(this.Header.SpawnPoint.X, this.Header.SpawnPoint.Y), 1));
			}

            // Draw Symbols
            foreach (KeyValuePair<TileType, List<SymbolLoc>> kv in tileSymbolsToAdd)
            {
                Bitmap symbolBitmap = ResourceManager.Instance.GetSymbol(kv.Key);
                foreach (SymbolLoc sl in kv.Value)
                {
                    int x = Math.Max((int)sl.pv.X - (symbolBitmap.Width / 2), 0);
                    int y = Math.Max((int)sl.pv.Y - (symbolBitmap.Height / 2), 0);
					
					if (x > maxX || y > maxY) continue;

                    graphicsHandle.DrawImage(symbolBitmap, x, y);
                }
            }
            bitmap.Save(outputPngPath, ImageFormat.Png);
            progress = 100;
        }

        public void CloseWorld()
        {
            reader.Close();
        }

        public List<Chest> Chests
        {
            get
            {
                if (this.chests.Count == 0) ReadChests();
                return this.chests;
            }
        }


    }
}