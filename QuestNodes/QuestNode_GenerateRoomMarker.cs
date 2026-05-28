using RimWorld;
using RimWorld.QuestGen;
using RimWorld.Planet;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoGiQ.QuestNodes
{

	class QuestNode_GenerateRoomMarker : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<Sketch> sketch;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			MonumentMarker roomMarker = (MonumentMarker)ThingMaker.MakeThing(ThingDefOfLocal.RoomMarker);
			               roomMarker.sketch = sketch.GetValue(slate);
			slate.Set(storeAs.GetValue(slate), roomMarker);
		}
	}
}
