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
    public class QuestNode_DoCQFActionChangeFaction : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> inSignal;

        [NoTranslate]
        public SlateRef<IEnumerable<string>> targetsText;

        public SlateRef<Faction> faction;

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            QuestPart_DoCQFActionChangeFaction questPart = new QuestPart_DoCQFActionChangeFaction();

            questPart.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate));
            questPart.targetsText = (List<string>)targetsText.GetValue(slate);
            questPart.faction = faction.GetValue(slate);
            QuestGen.quest.AddPart(questPart);
        }

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }
    }
}
