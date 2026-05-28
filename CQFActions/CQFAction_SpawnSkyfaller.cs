using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using QuestEditor_Library;
using RimWorld;
using UnityEngine;
using Verse;

namespace LoGiQ.CQFActions
{
    public class CQFAction_SpawnSkyfaller : QuestEditor_Library.CQFAction_Target
    {
        public string skyfallerDefName;
        public override void RealWork(Dictionary<string, TargetInfo> targets, Quest quest)
        {
            if(targets.NullOrEmpty()) {
                Log.Warning("LoGiQ.CQFAction_SpawnSkyfaller: Null or empty target.");
                return;
            }
            var target = targets.First().Value;
            ThingDef skyfallerDef = DefDatabase<ThingDef>.GetNamed(skyfallerDefName);
            if(skyfallerDef == null)
            {
                Log.Warning($"LoGiQ.CQFAction_SpawnSkyfaller: Def not found '{skyfallerDefName}'.");
                return;
            }
            Skyfaller thing = SkyfallerMaker.MakeSkyfaller(skyfallerDef);

            GenPlace.TryPlaceThing(thing, target.Cell, target.Map, ThingPlaceMode.Near);
            Log.Message($"skyfaller made at {target.Cell}");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref skyfallerDefName, "skyfallerDefName");
        }
    }
}
