using RimWorld;
using RimWorld.QuestGen;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using Verse.Grammar;

namespace LoGiQ.QuestNodes
{
    public class QuestNode_GameConditionSite : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		[NoTranslate]
		public SlateRef<string> removeSignal;

		public SlateRef<GameConditionDef> gameCondition;

		public SlateRef<bool> targetWorld;

		public SlateRef<int> duration;

		public SlateRef<Site> site;

		[NoTranslate]
		public SlateRef<string> storeGameConditionDescriptionFutureAs;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			float points = QuestGen.slate.Get("points", 500f);
			GameCondition gameCondition = GameConditionMaker.MakeCondition(this.gameCondition.GetValue(slate), duration.GetValue(slate));
			QuestPart_GameConditionRemovable questPart_GameCondition = new QuestPart_GameConditionRemovable();
			List<Rule> list = new List<Rule>();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (targetWorld.GetValue(slate))
			{
				questPart_GameCondition.targetWorld = true;
				gameCondition.RandomizeSettings(points, null, list, dictionary);
			}
			else
			{
				site.TryGetValue(slate, out Site s);
				questPart_GameCondition.mapParent = s;
				gameCondition.RandomizeSettings(points, null, list, dictionary);
			}
			questPart_GameCondition.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_GameCondition.removeSignal = QuestGenUtility.HardcodedSignalWithQuestID(removeSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("removeSignal");
			questPart_GameCondition.gameCondition = gameCondition;
			QuestGen.quest.AddPart(questPart_GameCondition);
			if (!storeGameConditionDescriptionFutureAs.GetValue(slate).NullOrEmpty())
			{
				slate.Set(storeGameConditionDescriptionFutureAs.GetValue(slate), gameCondition.def.descriptionFuture);
			}
			QuestGen.AddQuestNameRules(list);
			QuestGen.AddQuestDescriptionRules(list);
			QuestGen.AddQuestDescriptionConstants(dictionary);
		}
	}
}
