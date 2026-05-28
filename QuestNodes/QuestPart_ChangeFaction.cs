using RimWorld;
using RimWorld.QuestGen;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using RimWorld.Planet;

namespace LoGiQ.QuestNodes
{
    internal class QuestPart_ChangeFaction : QuestPart
    {
        [NoTranslate]
        public string inSignal;

        public Faction newFaction;

        public MapParent mapParent = null;

        //... filters ...
        public Faction onlyFaction    = null;
        public bool    onlyPawn       = false;
        public bool    onlyBuilding   = false;
        public Type    onlyThingClass = null;

        //... certain ...
        public Thing   certainThing = null;
        

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            if (signal.tag != inSignal)
                return;

            if (certainThing != null)
            {
                //Log.Message($"Will change {certainThing} faction to {newFaction}");
                if (certainThing.def.CanHaveFaction)
                    certainThing.SetFaction(newFaction);
                else Log.Warning($"QuestPart_ChangeFaction: Thing {certainThing} can not have faction, skip.");
            }
            else
            {
                if (mapParent == null || mapParent.Map == null)
                {
                    Log.Warning($"QuestPart_ChangeFaction: Invalid site '{mapParent}', skip.");
                    return;
                }
                Log.Message($"replacing faction of {onlyFaction},{onlyPawn},{onlyBuilding},{onlyThingClass}");
                foreach (Thing t in mapParent.Map.listerThings.GetAllThings(validate))
                    t.SetFaction(newFaction);
            }
        }


        bool validate(Thing t)
        {
            bool ok = t.def.CanHaveFaction;

            ok = ok && (onlyFaction == null || t.Faction == onlyFaction);
            ok = ok && (!onlyPawn || t is Pawn);
            ok = ok && (!onlyBuilding || t is Building);
            ok = ok && (onlyThingClass == null || t.def.thingClass == onlyThingClass);

            return ok;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_References.Look(ref newFaction, "newFaction");
            Scribe_References.Look(ref onlyFaction, "onlyFaction");
            Scribe_Values.Look(ref onlyPawn, "onlyPawn");
            Scribe_Values.Look(ref onlyBuilding, "onlyBuilding");
            Scribe_Values.Look(ref onlyThingClass, "onlyThingClass");
            Scribe_References.Look(ref mapParent, "mapParent");
            Scribe_References.Look(ref certainThing, "certainThing");
        }
    }
}
