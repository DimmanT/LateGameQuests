using QuestEditor_Library;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LoGiQ.QuestNodes
{
    class QuestPart_SpawnAtAnchor : QuestPart
    {
        [NoTranslate]
        public string inSignal;

        [NoTranslate]
        public string anchorName;

        public Thing thing;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            //Log.Message($"I'm QuestPart_SpawnAtAnchor. Signal is '{signal.tag}', my is '{inSignal}'.");
            if (signal.tag != inSignal)
                return;
            //Log.Message($"Its ok !!");

            List<string> targetsText = new List<string>();
            targetsText.Add(anchorName);
            Dictionary<string, TargetInfo> targets = new Dictionary<string, TargetInfo>();
            Dictionary<string, TargetInfo> targets2 = GameTools.GetTargets(targets, quest, targetsText);
            Log.Message($"QuestPart_SpawnAtAnchor: Resolve {targets2.Count()} targets");
            if(targets2.NullOrEmpty())
            {
                Log.Warning($"Can not find anchor='{anchorName}'");
                return;
            }
            //Log.Message($"QuestPart_SpawnAtAnchor: Resolved(A) {targets2.First().Value.Label} cell={targets2.First().Value.Cell}");
            var pos = targets2.First().Value.Cell;
            var mapp = targets2.First().Value.Map;
            GenPlace.TryPlaceThing(thing, pos, mapp, ThingPlaceMode.Near);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref anchorName, "anchorName");
            Scribe_References.Look(ref thing, "thing");
        }
    }
}
