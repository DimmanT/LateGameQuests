using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LoGiQ.QuestNodes
{
    public class QuestPart_GameConditionRemovable : QuestPart_GameCondition
    {
        public string removeSignal;

        protected bool applied = false;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            if (signal.tag == inSignal && !applied)
            {
                Log.Message($"QuestPart_GameConditionRemovable: BEG '{gameCondition}'");
                GameCondition copy = gameCondition;
                base.Notify_QuestSignalReceived(signal);
                gameCondition = copy;
                applied = true;
            }
			else
            if (signal.tag == removeSignal && applied && gameCondition!=null)
            {
                Log.Message($"QuestPart_GameConditionRemovable: END '{gameCondition}'");
                gameCondition.End();
                gameCondition = null;
            }
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref removeSignal, "removeSignal");
            Scribe_Values.Look(ref applied, "applied");
        }
    }
}
