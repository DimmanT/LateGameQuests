using RimWorld;
using RimWorld.QuestGen;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;

namespace LoGiQ.QuestNodes
{
	/// <summary>
	/// Variant for simple progress, just one signal, that increments progress value 
	/// </summary>
	class QuestNode_Progress : QuestNode
    {
		[NoTranslate]
		public SlateRef<string> inSignal;

		[NoTranslate]
		public SlateRef<string> outSignalComplete;

		public SlateRef<string> progressMessage;

		public SlateRef<int> progressMax; ///< default is 100
		public SlateRef<int> progressMin; ///< default is 0
		public SlateRef<int> increment;   ///< default is 1

		protected override bool TestRunInt(Slate slate)
		{
			return !outSignalComplete.GetValue(slate).NullOrEmpty();
		}
		protected override void RunInt()
		{

			Slate slate = QuestGen.slate;
			QuestPart_Progress questPart = new QuestPart_Progress();

			questPart.outSignalComplete = QuestGenUtility.HardcodedSignalWithQuestID(outSignalComplete.GetValue(slate));
			questPart.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate));

            if (!progressMin.TryGetValue(slate, out int defProgressCur))
                defProgressCur = 0;
			if (!progressMax.TryGetValue(slate, out int defProgressMax))
				defProgressCur = 100;
			if (!increment.TryGetValue(slate, out int defIncrement))
				defIncrement = 1;
			if (!progressMessage.TryGetValue(slate, out string defMessage))
				defMessage = "";

			questPart.increment   = defIncrement;
			questPart.progressCur = defProgressCur;
			questPart.progressMax = defProgressMax;
			questPart.message     = defMessage;

			QuestGen.quest.AddPart(questPart);
		}
	}

	/// <summary>
	/// Variant for complex progress, with many input signals, enable/disable signals and so on.
	/// </summary>
	class QuestNode_ProgressComplex : QuestNode
    {
		[NoTranslate]
		public SlateRef<IEnumerable<string>> inSignals;
		public SlateRef<IEnumerable<float>> increments;

		[NoTranslate]
		public SlateRef<string> inSignalEnable;

		[NoTranslate]
		public SlateRef<string> inSignalDisable;

		[NoTranslate]
		public SlateRef<string> outSignalComplete;

		[NoTranslate]
		public SlateRef<string> progressName;

		public SlateRef<float> progressMax;
		public SlateRef<float> progressMin;

		public SlateRef<string> progressMessage;

		protected override bool TestRunInt(Slate slate)
        {
			IEnumerable<string> signals;
			IEnumerable<float>  tmp_incs;
			inSignals.TryGetValue(slate, out signals);
			increments.TryGetValue(slate, out tmp_incs);
			if (signals.EnumerableNullOrEmpty())
			{
				Log.Warning("'inSignals' is empty!");
				return false;
			}
			if (signals.Count() != tmp_incs.Count())
			{
				Log.Warning("'increments' size not equal to 'inSignals' size!");
				return false;
			}
			if (outSignalComplete.GetValue(slate).NullOrEmpty())
            {
				Log.Warning("'outSignalComplete' not specified!");
				return false;
            }
			return true;
		}
		protected override void RunInt()
        {
			
			Slate slate = QuestGen.slate;
			QuestPart_ProgressComplex questPart = new QuestPart_ProgressComplex();
			questPart.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate));
			questPart.inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate));
			questPart.outSignalsCompleted.Add(QuestGenUtility.HardcodedSignalWithQuestID(outSignalComplete.GetValue(slate)));

			//... resolve: signal -> increment ...
			var signalToInc = new Dictionary<string, float>();
			IEnumerable<string> signals;
			IEnumerable<float>    tmp_incs;
			inSignals .TryGetValue(slate, out signals );
			increments.TryGetValue(slate, out tmp_incs);
			List<float> incs = (List<float>)tmp_incs;
			int cnt = 0;
			foreach(string s in signals) {
				signalToInc[QuestGenUtility.HardcodedSignalWithQuestID(s)] = incs[cnt];
				cnt++;
            }
			questPart.inSignals = signalToInc;
			//....................................

			if (!progressMin.TryGetValue(slate, out float defProgressCur))
				defProgressCur = 0;
			if (!progressMax.TryGetValue(slate, out float defProgressMax))
				defProgressMax = 100;
			if (!progressMessage.TryGetValue(slate, out string defMessage))
				defMessage = "";

			string defProgressName="";
			progressName.TryGetValue(slate, out defProgressName);

			questPart.progressCur = defProgressCur;
			questPart.progressMax = defProgressMax;
			questPart.message     = defMessage;
			questPart.inverse     = defProgressCur > defProgressMax;
			questPart.progressName = defProgressName;

			QuestGen.quest.AddPart(questPart);
		}
	}

	class QuestNode_ProgressGiveReward : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		[NoTranslate]
		public SlateRef<string> progressName;

		public SlateRef<float> progressMax;
		public SlateRef<float> progressMin;
		public SlateRef<int> maxReward;

		public SlateRef<MapParent> site;

		protected override bool TestRunInt(Slate slate)
		{
			int maxR = -1;
			maxReward.TryGetValue(QuestGen.slate, out maxR);

			string pName = "";
			progressName.TryGetValue(QuestGen.slate, out pName);

			bool ok = maxR > 0 && !pName.NullOrEmpty();
			if (!ok) Log.Error($"LoGiQ.QuestNodes.QuestNode_ProgressGiveReward: maxReward={maxR} or progressName={pName} not setted up!");

			return ok;
		}
		protected override void RunInt()
		{
			
			Slate slate = QuestGen.slate;
			QuestPart_ProgressGiveReward questPart = new QuestPart_ProgressGiveReward();

			questPart.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate));

			float defProgressMin = 0;
			float defProgressMax = 100;
			int defMaxReward   = 1000;
			string defProgressName = "";
			MapParent defSite=null;


			progressMin .TryGetValue(slate, out defProgressMin); 
			progressMax .TryGetValue(slate, out defProgressMax); 
			maxReward   .TryGetValue(slate, out defMaxReward);   
			progressName.TryGetValue(slate, out defProgressName);
			site.TryGetValue(slate, out defSite);

			if(defSite == null)
			{
				var map = slate.Get<Map>("map");
				if (map != null)
					defSite = map.Parent;
			}
			
			questPart.progressMin  = defProgressMin;  
			questPart.progressMax  = defProgressMax;  
			questPart.maxReward    = defMaxReward;    
			questPart.progressName = defProgressName;
			questPart.mapParent = defSite;
			//Log.Message($"QuestNode_ProgressGiveReward resolved site: {defSite}");

			QuestGen.quest.AddPart(questPart);
		}
	}
}
