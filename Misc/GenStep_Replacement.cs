using QuestEditor_Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace LoGiQ
{
    public class GenStep_Replacement : CustomMapStep
    {
        public List<string> replaceDataDefNames; //names of ReplaceDataDef

        public override void Draw(ref float y, Rect inRect, float x)
        {
             
        }

        public override void Generate(Map map, CustomMapDataDef def, CustomSitePartParams param)
        {
            if (DebugSettings.godMode)
                return; //skip this step, so we can edit map and replacements on it.

            // . . . . create replace rules ....
            var replaceThings = new Dictionary<string, ThingDef>();

            if(replaceDataDefNames.NullOrEmpty())
            {
                Log.Warning("Null or empty 'replaceDefs'");
                return;
            }
            foreach (var defName in replaceDataDefNames)
            {
                var replaceDef = DefDatabase<ReplacementDataDef>.GetNamed(defName);
                if (replaceDef.datas.NullOrEmpty())
                {
                    Log.Warning($"Replacement failed: {defName} is empty!");
                    continue;
                }
                var data = replaceDef.datas.RandomElement();
                Log.Message($"Choosen kit {data.DataName} from {replaceDef.datas.Count} variants");
                foreach (var originalName in data.replaceThings.Keys) //todo via GetEnumerator
                {
                    string newName = data.replaceThings[originalName];
                    ThingDef newDef = DefDatabase<ThingDef>.GetNamed(newName);
                    if (newDef == null)
                    {
                        Log.Warning($"Can not find ThingDef={newName}, replacing with 'Steel'");
                        newDef = DefDatabase<ThingDef>.GetNamed("Steel");
                    }
                    replaceThings[originalName] = newDef;
                }
            }
            // . . . . . . . . . . . . .

            //. . . .  find all things to replace . . . .
            var thingsToReplace = new List<Pair<Thing, ThingDef>>();
            bool found;
            ThingDef temp;
            foreach (var thing in map.spawnedThings)
            {
                found = replaceThings.TryGetValue(thing.def.defName, out temp);
                if(found)
                {
                    thingsToReplace.Add(new Pair<Thing, ThingDef>(thing, temp));
                }
            }
            // . . . . . . . . . . . . .

            // . . . do replace . . . .
            for (int i = 0; i < thingsToReplace.Count(); ++i)
            {
                temp = thingsToReplace[i].Second;
                Thing oldThing = thingsToReplace[i].First;

                var pos     = oldThing.Position;
                var rot     = oldThing.Rotation;
                var stack   = oldThing.stackCount;
                var faction = oldThing.Faction;

                oldThing.Destroy();
                if (temp.CanSpawnAt(pos, rot, map))
                {
                    
                    var newThing = ThingMaker.MakeThing(temp, temp.defaultStuff);
                        newThing.stackCount = stack;
                    if(newThing.def.CanHaveFaction)
                        newThing.SetFaction(faction);
                    GenPlace.TryPlaceThing(newThing, pos, map, ThingPlaceMode.Direct, null, null, rot);
                }
                else Log.Warning($"Can not spawn {temp.defName} at {pos}, rot{rot}, map{map}");
            }
            // . . . . . . . . . . . . .
        }
    }
}
