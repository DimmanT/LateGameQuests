using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LoGiQ.QuestNodes
{
    internal class QuestNode_VisitSite : QuestNode_VisitColony
    {
        public SlateRef<MapParent> site;

        protected override void RunInt()
        {
			Slate slate = QuestGen.slate;
			if (!pawns.GetValue(slate).EnumerableNullOrEmpty())
			{
				QuestPart_VisitSite questPart_VisitSite = new QuestPart_VisitSite();
				questPart_VisitSite.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
				questPart_VisitSite.pawns.AddRange(pawns.GetValue(slate));
				questPart_VisitSite.mapParent = site.GetValue(slate);
				questPart_VisitSite.faction = faction.GetValue(slate);
				questPart_VisitSite.durationTicks = durationTicks.GetValue(slate);
				QuestGen.quest.AddPart(questPart_VisitSite);
			}
		}

    }
}
