using QuestEditor_Library;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
namespace LoGiQ.QuestNodes
{
	public class QuestPart_DoCQFActionChangeFaction : QuestPart
	{
		public string inSignal;

		[NoTranslate]
		public List<string> targetsText;

		public Faction faction;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				Dictionary<string, TargetInfo> targets = new Dictionary<string, TargetInfo>();
				Dictionary<string, TargetInfo> targets2 = GameTools.GetTargets(targets, quest, targetsText);
				Log.Message($"QuestPart_DoCQFActionChangeFaction: Resolve {targets2.Count()} targets");
				targets2.ToList().ForEach(delegate (KeyValuePair<string, TargetInfo> t)
				{
					Thing thing = t.Value.Thing;
					Log.Message($"QuestPart_DoCQFActionChangeFaction: Changing {thing} faction {thing.Faction} to {faction}");
					if (thing != null && thing.def.CanHaveFaction)
						thing.SetFaction(faction);
				});
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref targetsText, "targetsText");
			Scribe_References.Look(ref faction, "faction");
		}
	}
}
