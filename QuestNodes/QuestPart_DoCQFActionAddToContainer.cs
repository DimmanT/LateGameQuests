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
    public class QuestPart_DoCQFActionAddToContainer : QuestPart
    {
		public string inSignal;

		[NoTranslate]
		public string containerTargetName;

		[NoTranslate]
		public string thingDefName;

		public int thingCount;

		public void Init()
        {
			var val   = new CQFThingDefCount();
			val.count = new IntRange(thingCount, thingCount);
            val.thing = DefDatabase<ThingDef>.GetNamed(thingDefName);

			LootData data = new LootData();
			data.things = new List<CQFThingDefCount>();
			data.things.Add(val);

			action = new CQFAction_SpawnAndAddToContainer();
			action.datas.Add(data);
			action.targetsText = new List<string>( );
			action.targetsText.Add(containerTargetName);
		}

        protected CQFAction_SpawnAndAddToContainer action;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				action.Work(new Dictionary<string, TargetInfo>(), quest);
			}

		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Deep.Look(ref action, "action");
			//container, thing_def and thing_count not needed
		}
	}
}
