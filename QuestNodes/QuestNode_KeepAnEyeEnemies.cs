using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LoGiQ.QuestNodes
{
    class QuestNode_KeepAnEyeEnemies : QuestNode
    {
        SlateRef<Site>   site;
        [NoTranslate]
        SlateRef<string> inSignal;
        [NoTranslate]
        SlateRef<string> inSignalDisable;
        [NoTranslate]
        SlateRef<string> outSignalEnemiesArrived;
        [NoTranslate]
        SlateRef<string> outSignalEnemiesDefeated;

        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            var questPart = new QuestPart_KeepAnEyeEnemies();

            //activate - deactivate
            if (inSignal.TryGetValue(slate, out string inSignalStr))
                questPart.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignalStr);
            else questPart.active = true;
            if (inSignalDisable.TryGetValue(slate, out string inSignalDisableStr))
                questPart.inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisableStr);

            //site
            if (site.TryGetValue(slate, out Site siteVal))
                questPart.site = siteVal;
            else  {
                var mapVal = slate.Get<Map>("map");
                questPart.site = (Site)mapVal.Parent;
            }

            // arrive -  defeat
            if (outSignalEnemiesArrived.TryGetValue(slate, out string outSignalEnemiesArrivedStr))
                questPart.outSignalEnemiesArrived = QuestGenUtility.HardcodedSignalWithQuestID(outSignalEnemiesArrivedStr);
            if (outSignalEnemiesDefeated.TryGetValue(slate, out string outSignalEnemiesDefeatedStr))
                questPart.outSignalEnemiesDefeated = QuestGenUtility.HardcodedSignalWithQuestID(outSignalEnemiesDefeatedStr);

            QuestGen.quest.AddPart(questPart);
            //Find.SignalManager.RegisterReceiver(questPart);
        }

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }
    }
}
