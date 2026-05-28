using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LoGiQ.QuestNodes
{
    class QuestNode_SpawnAtAnchor : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> inSignal;

        public SlateRef<Thing> thing;

        [NoTranslate]
        public SlateRef<string> anchorName;

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            string ancName;
            anchorName.TryGetValue(slate, out ancName);
            if (ancName.NullOrEmpty())
                return;

            Thing th = null;
            thing.TryGetValue(slate, out th);
            if (th == null) {
                Log.Error($"Invalid thing in LoGiQ.QuestNode_SpawnAtAnchor (anchorName='{ancName}'.");
                return;
            }

            var questPart = new QuestPart_SpawnAtAnchor();
            questPart.anchorName = ancName;
            questPart.thing = th;
            questPart.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate));
            QuestGen.quest.AddPart(questPart);
        }

        protected override bool TestRunInt(Slate slate)
        {
            string ancName;
            anchorName.TryGetValue(slate, out ancName);
            return !ancName.NullOrEmpty();
        }
    }
}
