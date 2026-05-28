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
	class QuestNode_DropPodsSite : RimWorld.QuestGen.QuestNode_DropPods
	{
		public SlateRef<Site> siteToDrop;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			Site site = siteToDrop.GetValue(slate);
			QuestGen.slate.Set<Map>("map", site.Map);
			if (contents.GetValue(slate).EnumerableNullOrEmpty() && contentsDefs.GetValue(slate).EnumerableNullOrEmpty())
			{
				return;
			}
			QuestPart_DropPods dropPods = new QuestPart_DropPods();
			dropPods.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			if (!customLetterLabel.GetValue(slate).NullOrEmpty() || customLetterLabelRules.GetValue(slate) != null)
			{
				QuestGen.AddTextRequest("root", delegate (string x)
				{
					dropPods.customLetterLabel = x;
				}, QuestGenUtility.MergeRules(customLetterLabelRules.GetValue(slate), customLetterLabel.GetValue(slate), "root"));
			}
			if (!customLetterText.GetValue(slate).NullOrEmpty() || customLetterTextRules.GetValue(slate) != null)
			{
				QuestGen.AddTextRequest("root", delegate (string x)
				{
					dropPods.customLetterText = x;
				}, QuestGenUtility.MergeRules(customLetterTextRules.GetValue(slate), customLetterText.GetValue(slate), "root"));
			}
			dropPods.sendStandardLetter = sendStandardLetter.GetValue(slate) ?? dropPods.sendStandardLetter;
			dropPods.useTradeDropSpot = useTradeDropSpot.GetValue(slate);
			dropPods.dropSpot = dropSpot.GetValue(slate) ?? IntVec3.Invalid;
			dropPods.joinPlayer = joinPlayer.GetValue(slate);
			dropPods.makePrisoners = makePrisoners.GetValue(slate);
			dropPods.mapParent = site;
			dropPods.Things = contents.GetValue(slate);
			dropPods.canRetargetAnyMap = false;
			if (contentsDefs.GetValue(slate) != null)
			{
				dropPods.thingDefs.AddRange(contentsDefs.GetValue(slate));
			}
			if (thingsToExcludeFromHyperlinks.GetValue(slate) != null)
			{
				dropPods.thingsToExcludeFromHyperlinks.AddRange(from t in thingsToExcludeFromHyperlinks.GetValue(slate)
																select t.GetInnerIfMinified().def);
			}
			QuestGen.quest.AddPart(dropPods);
		}
	}
}
