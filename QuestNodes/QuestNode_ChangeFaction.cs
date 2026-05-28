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
    internal class QuestNode_ChangeFaction : QuestNode
    {
        [NoTranslate]
        public SlateRef<string>    inSignal;

        public SlateRef<Faction>   newFaction;
       
        public SlateRef<Thing>     certainThing;

        //... filters ... 
        public SlateRef<MapParent> site;
        public SlateRef<Faction> onlyFaction;
        public SlateRef<bool?>   onlyPawn;
        public SlateRef<bool?>   onlyBuilding;
        public SlateRef<Type>    onlyThingClass;

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            var questPart = new QuestPart_ChangeFaction();

            questPart.inSignal     = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate));
            questPart.newFaction   = newFaction.GetValue(slate);
            questPart.onlyPawn     = onlyPawn.GetValue(slate).GetValueOrDefault();
            questPart.onlyBuilding = onlyBuilding.GetValue(slate).GetValueOrDefault();

            Faction defOnlyFaction = null;
            onlyFaction.TryGetValue(slate, out defOnlyFaction);
            questPart.onlyFaction = defOnlyFaction;

            Type defOnlyThingClass = null;
            onlyThingClass.TryGetValue(slate, out defOnlyThingClass);
            questPart.onlyThingClass = defOnlyThingClass;

            MapParent defSite = null;
            site.TryGetValue(slate, out defSite);
            if (defSite == null)
            {
                var map = slate.Get<Map>("map");
                if (map != null)
                    defSite = map.Parent;
            }
            questPart.mapParent = defSite;

            Thing defCertainThing = null;
            certainThing.TryGetValue(slate, out defCertainThing);
            questPart.certainThing = defCertainThing;

            QuestGen.quest.AddPart(questPart);
        }


        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }

    }
}
