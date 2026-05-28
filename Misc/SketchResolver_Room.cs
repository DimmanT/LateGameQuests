using RimWorld;
using RimWorld.SketchGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LoGiQ
{
    public class SketchResolver_Room : SketchResolver
    {
        protected override bool CanResolveInt(SketchResolveParams parms)
        {
			return true;
        }

        protected override void ResolveInt(SketchResolveParams parms)
        {
			IntVec2 size;
			if (parms.monumentSize.HasValue)
			{
				size = parms.monumentSize.Value;
			}
			else
			{
				int num = Rand.Range(7, 20);
				size = new IntVec2(num, num);
			}
			int width = size.x;
			int height = size.z;
			//bool flag = false;
			Sketch room = new Sketch();
			bool onlyBuildableByPlayer = parms.onlyBuildableByPlayer == true;
			List<IntVec3> list = new List<IntVec3>();
            {
				//Generate figures 
				bool hasCorner=false;
				bool[,] shape = new bool[width, height];
				{
					const int THRESH = 3;
					int width_  = width  - 1;
					int height_ = height - 1;
					bool isVerticalSmall = (height / 2) < (THRESH+1);


					int corner1Pos = Rand.Range(THRESH, (int)(1.1* width_));
					int corner2Pos = corner1Pos > width_ ? -1 : Rand.Range(corner1Pos, (int)(1.5 * width_));
					if (corner1Pos >= width_ - THRESH) 
						corner2Pos = corner1Pos = -1;
					if (corner2Pos > 0 && (corner2Pos - corner1Pos) <= THRESH)
						corner2Pos = -1;

					int startX = 0;
					int startY = (Rand.Bool || isVerticalSmall || corner1Pos<0) ? 0 : Rand.Range(3, height_ / 2);
					int x = startX;
					int y = startY;

					while (x != corner1Pos && x < width_) shape[x++,y] = true;
					if(x == corner1Pos) {
						hasCorner = true;
						if (startY == 0) 
							 while (y <  3) shape[x, y++] = true;
						else while (y >  0) shape[x, y--] = true;
					}
					while (x != corner2Pos && x < width_) shape[x++,y] = true;
					if (x == corner2Pos)
					{
						if (startY != 0)
							 while (y < startY) shape[x, y++] = true;
						else while (y >  0    ) shape[x, y--] = true;
					}
					while (x <  width_ ) shape[x++, y  ] = true;
					while (y <  height_) shape[x  , y++] = true;
					while (x != startX ) shape[x--, y  ] = true;
					while (y != startY ) shape[x  , y--] = true;
					//Log.Message($"W{width},H{height},sX{startX},sY{startY}");
				}

				//mirroring OX;
				if (hasCorner && Rand.Bool)
                {
					
					bool swappa; int y_mirr;
					var halfH = height / 2;
					for (int y = 0; y < halfH; y++)
						for (int x = 0; x < width; x++)
						{
							y_mirr = height - 1 - y;
							swappa = shape[x, y];
							shape[x, y     ] = shape[x, y_mirr];
							shape[x, y_mirr] = swappa;
						}
				}
				
				//simple copy walls
				for (int z = 0; z < height; z++)
					for (int x = 0; x < width; x++)
						if (shape[x, z])
							room.AddThing(ThingDefOf.Wall, new IntVec3(x, 0, z), Rot4.North, ThingDefOf.WoodLog);
				Func<int, int, bool> func = (int x, int z) => x >= 0 && z >= 0 && x < shape.GetLength(0) && z < shape.GetLength(1) && shape[x, z];

				//old function from monument:
				for (int num4 = -1; num4 < shape.GetLength(0) + 1; num4++)
				{
					for (int num5 = -1; num5 < shape.GetLength(1) + 1; num5++)
					{
						if (!func(num4, num5) && (func(num4 - 1, num5) || func(num4, num5 - 1) || func(num4, num5 + 1) || func(num4 + 1, num5)))
						{
							int num6 = num4 + 1;
							int num7 = num5 + 1;
							if ((!func(num4 - 1, num5) &&
								room.Passable(new IntVec3(num6 - 1, 0, num7))) || (!func(num4, num5 - 1) &&
								room.Passable(new IntVec3(num6, 0, num7 - 1))) || (!func(num4, num5 + 1) &&
								room.Passable(new IntVec3(num6, 0, num7 + 1))) || (!func(num4 + 1, num5) &&
								room.Passable(new IntVec3(num6 + 1, 0, num7))))
							{
								list.Add(new IntVec3(num6, 0, num7));
							}
						}
					}
				}
			}

			//Stage assign random staff
            {
				SketchResolveParams randstaffParams = parms;
				randstaffParams.sketch = room;
				randstaffParams.connectedGroupsSameStuff = true;
				randstaffParams.assignRandomStuffTo = ThingDefOf.Wall;
				SketchResolverDefOf.AssignRandomStuff.Resolve(randstaffParams);
			}

			//Stage addFloors
			if (parms.addFloors ?? true)
			{
				SketchResolveParams floorParams = parms;
				floorParams.singleFloorType = true;
				floorParams.sketch = room;
				floorParams.floorFillRoomsOnly = true;
				floorParams.onlyStoneFloors = parms.onlyStoneFloors ?? false;
				floorParams.allowConcrete = parms.allowConcrete;
				floorParams.rect = new CellRect(0, 0, width, height);
				SketchResolverDefOf.FloorFill.Resolve(floorParams);
			}

			//ApplySymmetry(parms, horizontalSymmetry, verticalSymmetry, room, width, height);
			TryPlaceFurniture(parms, room, CanUse);
			TryPlaceOutDoors(list, width, height, false, false, parms, room);

			List<SketchThing> things = room.Things;
			for (int num10 = 0; num10 < things.Count; num10++)
			{
				if (things[num10].def == ThingDefOf.Wall)
				{
					room.RemoveTerrain(things[num10].pos);
				}
			}
			parms.sketch.MergeAt(room, default(IntVec3), Sketch.SpawnPosType.OccupiedCenter);
			return;


			bool CanUse(ThingDef def)
			{
				if (def == null)
					return false;

				if (onlyBuildableByPlayer && !SketchGenUtility.PlayerCanBuildNow(def))
				{
					//Log.Message($"Checking {def} ... fail!");
					return false;
				}
				//Log.Message($"Checking {def} ... ok");
				return true;
			}
		}

		protected void TryPlaceFurniture(SketchResolveParams parms, Sketch room, Func<ThingDef, bool> canUseValidator)
		{
			if (canUseValidator == null || canUseValidator(parms.thingCentral))
			{
				SketchResolveParams parms5 = parms;
				parms5.sketch = room;
				parms5.requireFloor = true;
				parms5.chance = 1.0f;
				ResolveCenterThing(parms5);
			}
			if (canUseValidator == null || canUseValidator(parms.wallEdgeThing))
			{
				SketchResolveParams parms5 = parms;
				parms5.sketch = room;
				parms5.requireFloor = true;
				parms5.chance = 0.85f;
				SketchResolverDefOf.AddWallEdgeThings.Resolve(parms5);
			}
			if (canUseValidator == null || canUseValidator(parms.cornerThing))
			{
				SketchResolveParams parms5 = parms;
				parms5.sketch = room;
				parms5.requireFloor = true;
				parms5.chance = 0.4f;
				ResolveCornerThing(parms5);
			}
		}
		protected void TryPlaceOutDoors(List<IntVec3> list, int width, int height, bool horizontalSymmetry, bool verticalSymmetry, SketchResolveParams parms, Sketch room)
        {
			if (list.Where((IntVec3 x) => (!horizontalSymmetry || x.x < width  / 2) &&
				                          (!verticalSymmetry   || x.z < height / 2) && 
										  room.ThingsAt(x).Any((SketchThing y) => y.def == ThingDefOf.Wall) && 
										  ((!room.ThingsAt(new IntVec3(x.x - 1, x.y, x.z)).Any() &&
										    !room.ThingsAt(new IntVec3(x.x + 1, x.y, x.z)).Any()) || 
										   (!room.ThingsAt(new IntVec3(x.x, x.y, x.z - 1)).Any() && 
										    !room.ThingsAt(new IntVec3(x.x, x.y, x.z + 1)).Any()))).TryRandomElement(out var result2))
			{
				SketchThing sketchThing2 = room.ThingsAt(result2).FirstOrDefault((SketchThing x) => x.def == ThingDefOf.Wall);
				if (sketchThing2 != null)
				{
					room.Remove(sketchThing2);
					room.AddThing(ThingDefOf.Door, result2, Rot4.North, sketchThing2.Stuff);
				}
			}
		}
		protected void ApplySymmetry(SketchResolveParams parms, bool horizontalSymmetry, bool verticalSymmetry, Sketch monument, int width, int height)
		{
			if (horizontalSymmetry)
			{
				SketchResolveParams parms2 = parms;
				parms2.sketch = monument;
				parms2.symmetryVertical = false;
				parms2.symmetryOrigin = width / 2;
				parms2.symmetryOriginIncluded = width % 2 == 1;
				SketchResolverDefOf.Symmetry.Resolve(parms2);
			}
			if (verticalSymmetry)
			{
				SketchResolveParams parms3 = parms;
				parms3.sketch = monument;
				parms3.symmetryVertical = true;
				parms3.symmetryOrigin = height / 2;
				parms3.symmetryOriginIncluded = height % 2 == 1;
				SketchResolverDefOf.Symmetry.Resolve(parms3);
			}
		}

		protected void ResolveCenterThing(SketchResolveParams parms)
		{
			bool CanPlaceAt(ThingDef def, IntVec3 position, Rot4 rot, Sketch sketch)
			{
				foreach (IntVec3 item in GenAdj.OccupiedRect(position, Rot4.North, def.size).AdjacentCellsCardinal)
				{
					if (sketch.GetDoor(item) != null)
					{
						return false;
					}
				}
				return true;
			}

			HashSet<IntVec3> processed = new HashSet<IntVec3>();
			CellRect outerRect = parms.rect ?? parms.sketch.OccupiedRect;
			bool allowWood = parms.allowWood ?? true;
			ThingDef stuff = GenStuff.RandomStuffInexpensiveFor(parms.thingCentral, null, (ThingDef x) => SketchGenUtility.IsStuffAllowed(x, allowWood, parms.useOnlyStonesAvailableOnMap, allowFlammableWalls: true, parms.thingCentral));
			bool requireFloor = parms.requireFloor == true;
			processed.Clear();
			try
			{
				foreach (IntVec3 item in outerRect.Cells.InRandomOrder())
				{
					CellRect cellRect = SketchGenUtility.FindBiggestRectAt(item, outerRect, parms.sketch, processed, (IntVec3 x) => !parms.sketch.ThingsAt(x).Any() && (!requireFloor || (parms.sketch.TerrainAt(x) != null && parms.sketch.TerrainAt(x).layerable)));
					if (cellRect.Width >= parms.thingCentral.size.x + 2 && cellRect.Height >= parms.thingCentral.size.z + 2)
					{
						IntVec3 intVec = new IntVec3(cellRect.CenterCell.x - parms.thingCentral.size.x / 2, 0, cellRect.CenterCell.z - parms.thingCentral.size.z / 2);
						if (Rand.Chance(parms.chance ?? 0.9f) && CanPlaceAt(parms.thingCentral, intVec, Rot4.North, parms.sketch))
						{
							parms.sketch.AddThing(parms.thingCentral, intVec, Rot4.North, stuff, 1, null, null, wipeIfCollides: false);
						}
					}
				}
			}
			finally
			{
				processed.Clear();
			}
		}


		protected void ResolveCornerThing(SketchResolveParams parms)
		{
			HashSet<IntVec3> wallPositions = new HashSet<IntVec3>();
			wallPositions.Clear();
			for (int i = 0; i < parms.sketch.Things.Count; i++)
			{
				if (parms.sketch.Things[i].def == ThingDefOf.Wall)
				{
					wallPositions.Add(parms.sketch.Things[i].pos);
				}
			}
			bool allowWood = parms.allowWood ?? true;
			ThingDef stuff = GenStuff.RandomStuffInexpensiveFor(parms.cornerThing, null, (ThingDef x) => SketchGenUtility.IsStuffAllowed(x, allowWood, parms.useOnlyStonesAvailableOnMap, allowFlammableWalls: true, parms.cornerThing));
			bool valueOrDefault = parms.requireFloor == true;
			try
			{
				foreach (IntVec3 wallPosition in wallPositions)
				{
					if (Rand.Chance(parms.chance ?? 0.5f))
					{
						if (wallPositions.Contains(new IntVec3(wallPosition.x + 1, 0, wallPosition.z - 1)) && !wallPositions.Contains(new IntVec3(wallPosition.x + 1, 0, wallPosition.z)) && (!valueOrDefault || (parms.sketch.TerrainAt(new IntVec3(wallPosition.x + 1, 0, wallPosition.z)) != null && parms.sketch.TerrainAt(new IntVec3(wallPosition.x + 1, 0, wallPosition.z)).layerable)))
						{
							parms.sketch.AddThing(parms.cornerThing, new IntVec3(wallPosition.x + 1, 0, wallPosition.z), Rot4.North, stuff, 1, null, null, wipeIfCollides: false);
						}
						if (wallPositions.Contains(new IntVec3(wallPosition.x + 1, 0, wallPosition.z - 1)) && !wallPositions.Contains(new IntVec3(wallPosition.x, 0, wallPosition.z - 1)) && (!valueOrDefault || (parms.sketch.TerrainAt(new IntVec3(wallPosition.x, 0, wallPosition.z - 1)) != null && parms.sketch.TerrainAt(new IntVec3(wallPosition.x, 0, wallPosition.z - 1)).layerable)))
						{
							parms.sketch.AddThing(parms.cornerThing, new IntVec3(wallPosition.x, 0, wallPosition.z - 1), Rot4.North, stuff, 1, null, null, wipeIfCollides: false);
						}
						if (wallPositions.Contains(new IntVec3(wallPosition.x + 1, 0, wallPosition.z + 1)) && !wallPositions.Contains(new IntVec3(wallPosition.x, 0, wallPosition.z + 1)) && (!valueOrDefault || (parms.sketch.TerrainAt(new IntVec3(wallPosition.x, 0, wallPosition.z + 1)) != null && parms.sketch.TerrainAt(new IntVec3(wallPosition.x, 0, wallPosition.z + 1)).layerable)))
						{
							parms.sketch.AddThing(parms.cornerThing, new IntVec3(wallPosition.x, 0, wallPosition.z + 1), Rot4.North, stuff, 1, null, null, wipeIfCollides: false);
						}
						if (wallPositions.Contains(new IntVec3(wallPosition.x + 1, 0, wallPosition.z + 1)) && !wallPositions.Contains(new IntVec3(wallPosition.x + 1, 0, wallPosition.z)) && (!valueOrDefault || (parms.sketch.TerrainAt(new IntVec3(wallPosition.x + 1, 0, wallPosition.z)) != null && parms.sketch.TerrainAt(new IntVec3(wallPosition.x + 1, 0, wallPosition.z)).layerable)))
						{
							parms.sketch.AddThing(parms.cornerThing, new IntVec3(wallPosition.x + 1, 0, wallPosition.z), Rot4.North, stuff, 1, null, null, wipeIfCollides: false);
						}
					}
				}
			}
			finally
			{
				wallPositions.Clear();
			}
		}



	}
}
