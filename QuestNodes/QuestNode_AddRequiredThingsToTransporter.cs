using QuestEditor_Library;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace LoGiQ.QuestNodes
{
	internal class QuestNode_AddRequiredThingsToTransporter : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		[NoTranslate]
		public SlateRef<string> transporterTargetName;

		[NoTranslate]
		public SlateRef<string> thingDefName;

		public SlateRef<int> thingCount;

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			var questPart = new QuestPart_AddRequiredThingsToTransporter();

			questPart.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate));
			questPart.transporterName = transporterTargetName.GetValue(slate);
			var def = DefDatabase<ThingDef>.GetNamed(thingDefName.GetValue(slate));
			var thingToLoad = new ThingDefCount(def, thingCount.GetValue(slate));
			questPart.thingsToLoad = new List<ThingDefCount>();
			questPart.thingsToLoad.Add(thingToLoad);

			//questPart.thingsToLoadCount = thingCount.GetValue(slate);
			//TransferableOneWay thingsToLoad = new TransferableOneWay();
			//var def = DefDatabase<ThingDef>.GetNamed(thingDefName.GetValue(slate));
			//var t = ThingMaker.MakeThing(def);
			//thingsToLoad.things.Add(t);
			//questPart.thingsToLoad = thingsToLoad;

			QuestGen.quest.AddPart(questPart);
		}

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

	}
}
