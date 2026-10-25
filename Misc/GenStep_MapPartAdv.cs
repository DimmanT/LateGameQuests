using QuestEditor_Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using Verse;
//using System.Diagnostics;

namespace LoGiQ
{
    public class GenStep_MapPartAdv : CustomMapStep
    {
        public enum MapZone : byte { Anywhere, Center, AroundCenter, Perifery, North, East, South, West, NorthEast, SouthEast, SouthWest, NorthWest, Invalid};

        public string buffer;
        public string buffer2;

        public IntRange count = IntRange.One;
        public CustomMapGenerationSet set = new CustomMapGenerationSet();
        public MapZone zone = MapZone.Anywhere;
        public bool allowIntersections = false;

        public override void Draw(ref float y, Rect inRect, float x)
        {
            Widgets.Label(new Rect(x, y, inRect.width - x - 12f, 30f), "Generate map parts (LoGiQ)".Colorize(ColorLibrary.SkyBlue));
            y += 35f;
            CQFEditorTools.DrawIntRange(ref y, "Parts count", ref count, ref buffer, ref buffer2, x, 60f);
            set.Draw(ref y, inRect, x);

            //.... Zones ....
            Listing_Standard listing = new Listing_Standard();
            listing.ColumnWidth = inRect.width;
            Rect listingRect = inRect;
            listingRect.y = y;
            listingRect.height = 512;
            listing.Begin(listingRect);
            listing.Label("Zone to generate in");

            if (listing.ButtonText(zone.ToString()))
            {
                var zones = new List<FloatMenuOption>();
                for(var z = MapZone.Anywhere; z < MapZone.Invalid; ++z)
                    zones.Add(new FloatMenuOption(z.ToString(), () => { zone = z; Log.Message($"Choosed {z}."); }));
                Find.WindowStack.Add(new FloatMenu(zones));
            }
            listing.End();
            //......................

            //todo draw 'allowIntersections'
        }

        public override void Generate(Map map, CustomMapDataDef def, CustomMapGenerationContext context)
        {
            int num = count.RandomInRange;
            Log.Message($"Start generating in {zone}. Total parts {num}.");
            List<CellRect> filledRects = new List<CellRect>();
            int remNum = num;
            while (num-->0)
            {
                //Stopwatch stopwatch = Stopwatch.StartNew();
                CustomMapDataDef partDef = set.GetMap();
                if (partDef == null)
                {
                    Log.Warning("Null map in set.GetMap(), skip.");
                    continue;
                }
                if (Prefs.DevMode)
                    Log.Message("Spawning Part:" + partDef.defName);

                var pos = allowIntersections ? getPosToGenerateAt(map, partDef)
                                             : getPosToGenerateAtWithoutIntersections(map, partDef, ref filledRects);

                partDef.GenerateByCore(pos, map, GameTools.GetQuestFromMap(map),destroyThings: partDef.destroyAllThing);
            }

            //Log.Message($"A={elapsedTime1},B={elapsedTime2},C={elapsedTime3}");
        }

        public override XElement SaveToXElement(string nodeName)
        {
            XElement result = base.SaveToXElement(nodeName);
            result.Add(new XElement("count", count));
            result.Add(set.SaveToXElement("set"));
            result.Add(new XElement("zone", zone));
            result.Add(new XElement("allowIntersections", allowIntersections));
            return result;
        }

        protected IntVec3 getPosToGenerateAt(Map map, CustomMapDataDef partDef)
        {
            int W = map.Size.x;
            int H = map.Size.z;
            int partW = partDef.size.x;
            int partH = partDef.size.z;
            //Log.Message($"Try find cell for part({partW}x{partH}) at map({W}x{H}) name={partDef.defName} zone={zone}");
            switch (zone)
            {
                case MapZone.Center:  
                    {
                        IntVec3 cell = new IntVec3(W / 2, 0, H / 2);
                        cell.x += Verse.Rand.Range(-W / 8, W / 8);
                        cell.z += Verse.Rand.Range(-H / 8, H / 8);
                        //todo check size of partMap
                        return cell;
                    }
                case MapZone.AroundCenter:
                    {
                        IntVec3 cell = new IntVec3(W / 2, 0, H / 2);
                        var R = Verse.Rand.Range((W+H) / 16, (W+H) / 8);
                        var alpha = Verse.Rand.Range(0.0f, 6.283f);
                        cell.x += (int)(R * Mathf.Cos(alpha));
                        cell.z += (int)(R * Mathf.Sin(alpha));
                        //todo check size of partMap
                        return cell;
                    }
                case MapZone.Perifery:
                    {
                        IntVec3 cell = new IntVec3(W / 2, 0, H / 2);
                        var R = Verse.Rand.Range((W + H) / 7, (W + H) / 5);
                        var alpha = Verse.Rand.Range(0.0f, 6.283f);
                        cell.x += (int)(R * Mathf.Cos(alpha));
                        cell.z += (int)(R * Mathf.Sin(alpha));
                        return cell;
                    }
                //todo more
                case MapZone.Anywhere:
                default: //same as MapZone.Anywhere
                    {
                        IntVec3 cell = new IntVec3(W / 2, 0, H / 2);
                        cell.x += (int)Verse.Rand.Range(-0.45f * (W - partW), 0.45f * (W - partW));
                        cell.z += (int)Verse.Rand.Range(-0.45f * (H - partH), 0.45f * (H - partH));
                        return cell; 
                    }
            }
        }

        protected IntVec3 getPosToGenerateAtWithoutIntersections(Map map, CustomMapDataDef partDef, ref List<CellRect> filledRects)
        {
            const int LIM=10;
            int lim = LIM;
            IntVec3 resPos = IntVec3.Zero;
            CellRect resRect = CellRect.Empty;
            while (lim-->0)
            {
                resPos = getPosToGenerateAt(map, partDef);
                resRect = new CellRect(resPos.x - partDef.size.x / 2, resPos.z - partDef.size.z / 2,
                                       partDef.size.x, partDef.size.z);
                bool bad = false;
                foreach (var R in filledRects)
                {
                    foreach (var c in resRect.Corners)
                    {
                        bad = R.Contains(c);
                        if (bad) break;
                    }
                    //Log.Message($"---Check -> {resPos} in {R} ok={!bad}");
                    if (bad) break;
                }
                //if (!bad)
                //{
                    //todo check for stone
                    //map.thingGrid.CellContains(resPos,ThingCategory.Item)
                //}
                if (!bad)
                {
                    lim = LIM;
                    break;
                }
            }
            //Log.Message($"Generated pos when lim={lim}, now filled {filledRects.Count()} it is {resPos}");
            filledRects.Add(resRect);
            return resPos;
        }

    }
}
