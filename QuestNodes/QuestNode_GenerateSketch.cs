using RimWorld;
using RimWorld.QuestGen;
using RimWorld.SketchGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace LoGiQ.QuestNodes
{
    public class QuestNode_GenerateSketch : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> storeAs;

        [NoTranslate]
        public SlateRef<string> sketchResolverDefName;

        public SlateRef<string> cornerThing; //defname of thing, that should be place at corners
        public SlateRef<string> centerThing; //defname of thing, that should be place at center
        public SlateRef<string> edgeThing;   //defname of thing, that should be place at edges

        public SlateRef<int> points;

        public SlateRef<int?> maxSize;

        public SlateRef<float> pointsPerArea;

        public SlateRef<bool?> clearStuff;

        public SlateRef<bool?> onlyStoneFloors;
        public SlateRef<bool?> allowConcrete;

        private static readonly FloatRange RandomAspectRatioRange = new FloatRange(1f, 2.25f);

        private const int MinEdgeLength = 3;

        private const int MaxArea = 2500;

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            string sketchResolverName = sketchResolverDefName.GetValue(slate);
            SketchResolverDef sketchResolver = DefDatabase<SketchResolverDef>.GetNamed(sketchResolverName);
            if (sketchResolver == null)
            {
                Log.Warning($"Invalid SketchResolverDef='{sketchResolverName}'");
                return;
            }
            var points = this.points.GetValue(slate);
            if(points < 10)
            {
                points = slate.Get<int>("points");
                if (points < 10)
                {
                    Log.Warning($"Invalid points value={points}");
                    return;
                }
            }
            var ppa = pointsPerArea.GetValue(slate);
            IntVec2 size = calcSize(points, ppa, maxSize.GetValue(slate));

            SketchResolveParams parms = new SketchResolveParams
            {
                sketch = new Sketch(),
                monumentSize = size,
                useOnlyStonesAvailableOnMap = null,
                onlyBuildableByPlayer = true
            }; 
            parms.onlyStoneFloors = onlyStoneFloors.GetValue(slate) ?? false;
            parms.allowConcrete   = allowConcrete.GetValue(slate) ?? false;

            var edgeStr = edgeThing  .GetValue(slate);
            var centStr = centerThing.GetValue(slate);
            var cornStr = cornerThing.GetValue(slate);
            if (!edgeStr.NullOrEmpty()) parms.wallEdgeThing = DefDatabase<ThingDef>.GetNamed(edgeStr);
            if (!centStr.NullOrEmpty()) parms.thingCentral  = DefDatabase<ThingDef>.GetNamed(centStr);
            if (!cornStr.NullOrEmpty()) parms.cornerThing   = DefDatabase<ThingDef>.GetNamed(cornStr);

            Log.Message($"Choosen things {parms.wallEdgeThing},{parms.thingCentral},{parms.cornerThing}");

            Sketch sketch = SketchGen.Generate(sketchResolver, parms);

            if (clearStuff.GetValue(slate) ?? true)
            {
                List<SketchThing> things = sketch.Things;
                for (int i = 0; i < things.Count; i++)
                {
                    things[i].stuff = null;
                }
                List<SketchTerrain> terrain = sketch.Terrain;
                for (int j = 0; j < terrain.Count; j++)
                {
                    terrain[j].treatSimilarAsSame = true;
                }
            }
            slate.Set(storeAs.GetValue(slate), sketch);
        }

        protected IntVec2 calcSize(int points, float pointsPerArea, int? maxLen)
        {
            float area = Mathf.Min(points / pointsPerArea, MaxArea);
            float aspectRatio = RandomAspectRatioRange.RandomInRange;

            //Calc A B (sides of rectangle)
            float lenAf = Mathf.Sqrt(aspectRatio * area);
            float lenBf = Mathf.Sqrt(area / aspectRatio);
            int lenA = GenMath.RoundRandom(Mathf.Sqrt(aspectRatio * area));
            int lenB = GenMath.RoundRandom(Mathf.Sqrt(area / aspectRatio));

            //Random swap A<->B
            if (Rand.Bool)
            {
                var tmp = lenB;
                lenB = lenA;
                lenA = tmp;
            }

            //Check maximum limit
            if (maxLen.HasValue)
            {
                lenA = Mathf.Min(lenA, maxLen.Value);
                lenB = Mathf.Min(lenB, maxLen.Value);
            }

            //Check minimum limit
            lenA = Mathf.Max(lenA, 3);
            lenB = Mathf.Max(lenB, 3);

            return new IntVec2(lenA, lenB);
        }
    }
}
