using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;

namespace LoGiQ.QuestNodes
{
    public class QuestPart_VisitSite : QuestPart_VisitColony
    {
		protected override Lord MakeLord()
		{
			var map = mapParent.Map;
			var point = map.Size / 2;
			var result = CellFinder.RandomClosewalkCellNear(point, map, 15);
			LordJob_VisitColony lordJob = new LordJob_VisitColony(faction ?? pawns[0].Faction, result, durationTicks);
			return LordMaker.MakeNewLord(faction ?? pawns[0].Faction, lordJob, mapParent.Map);
		}
	}
}
