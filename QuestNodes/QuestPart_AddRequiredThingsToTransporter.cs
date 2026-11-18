using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestEditor_Library;

namespace LoGiQ.QuestNodes
{
    class QuestPart_AddRequiredThingsToTransporter : QuestPart
    {
        [NoTranslate]
        public string inSignal;

        [NoTranslate]
        public string transporterName;

        public List<ThingDefCount> thingsToLoad;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref transporterName, "TransporterName");
            Scribe_Collections.Look(ref thingsToLoad, "ThingsToLoad");
        }

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            //... check signal ...
            base.Notify_QuestSignalReceived(signal);
            //Log.Message($"I'm QuestPart_AddRequiredThingsToTransporter. Signal is '{signal.tag}', my is '{inSignal}'.");
            if (signal.tag != inSignal)
                return;
            //....................

            //... resolve target name ....
            List<string> targetsText = new List<string>();
            targetsText.Add(transporterName);
            Dictionary<string, TargetInfo> targets = new Dictionary<string, TargetInfo>();
            Dictionary<string, TargetInfo> targets2 = GameTools.GetTargets(targets, quest, targetsText);
            Log.Message($"QuestPart_AddRequiredThingsToTransporter: Resolve {targets2.Count()} targets");
            if (targets2.NullOrEmpty())
            {
                Log.Warning($"Can not find transporter='{transporterName}'");
                return;
            }

            var transporter = targets2.First().Value.Thing;

            var comp = transporter.TryGetComp<LoGiQ.Components.CompTransporterQuest>();
            if (comp == null)
            {
                Log.Warning("Can not get 'CompTransporterQuest'");
                return;
            }
            //...............................

            TransporterUtility.InitiateLoading(Gen.YieldSingle(comp));
            comp.name = transporterName;

            if (comp.leftToLoad == null)
                comp.leftToLoad = new List<TransferableOneWay>();
           
            var lister = transporter.Map.listerThings;
            foreach(var things in thingsToLoad)
            {
                var all = lister.ThingsOfDef(things.ThingDef);
                TransferableOneWay t = new TransferableOneWay();
                if (all.Count > 0)
                {
                    int fullstack = thingsToLoad.FirstOrDefault().Count;
                    int stack = fullstack;
                    foreach (var th in all)
                    {
                        if (stack < 0)
                            break;
                        stack -= th.stackCount;
                        t.things.Add(th);
                    }
                    t.interactive = false;
                    t.things.Add(all.First());
                    t.AdjustTo(fullstack);
                    comp.leftToLoad.Add(t);

                }
            }
        }
    }
}
