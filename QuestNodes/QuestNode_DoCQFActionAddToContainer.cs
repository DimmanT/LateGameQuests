using QuestEditor_Library;
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
    internal class QuestNode_DoCQFActionAddToContainer : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		[NoTranslate]
		public SlateRef<string> containerTargetName;

		[NoTranslate]
		public SlateRef<string> thingDefName;

		public SlateRef<int> thingCount;

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_DoCQFActionAddToContainer questPart = new QuestPart_DoCQFActionAddToContainer();

			questPart.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate));
			questPart.containerTargetName = containerTargetName.GetValue(slate);
			questPart.thingDefName = thingDefName.GetValue(slate);
			questPart.thingCount = thingCount.GetValue(slate);
			questPart.Init();
			QuestGen.quest.AddPart(questPart);
		}

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

    }
}
