using QuestEditor_Library;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace LoGiQ.QuestNodes
{
    class QuestNode_MakeCustomLord : QuestNode
    {
        public SlateRef<string> inSignal;
        public SlateRef<IEnumerable<Pawn>> pawns;
        public SlateRef<Pawn> pawn = null;
        public SlateRef<DutyDef> duty = DutyDefOf.Defend;

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            IEnumerable<Pawn> pawnsValue; 
            pawns.TryGetValue(slate, out pawnsValue);
            Pawn singlePawn;
            pawn.TryGetValue(slate, out singlePawn);

            var pawnsList = new List<Pawn>();
            if(!pawnsValue.EnumerableNullOrEmpty())
                pawnsList = (List<Pawn>)pawnsValue;

            if (singlePawn != null)
                pawnsList.Add(singlePawn);

            if (pawnsList.NullOrEmpty())
            {
                Log.Error("QuestNode_MakeCustomLord: pawns is null or empty");
                return;
            }
            var questPart = new QuestPart_MakeCustomLord();
            questPart.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate));
            questPart.pawns = pawnsList;
            questPart.duty = duty.GetValue(slate);

            QuestGen.quest.AddPart(questPart);
        }

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }
    }

    class QuestPart_MakeCustomLord : QuestPart
    {
        public string inSignal;
        public List<Pawn> pawns;
        public Verse.AI.DutyDef duty;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            if (signal.tag != inSignal)
                return;
            var newJob = new LordJob_Custom();
            foreach (Pawn p in pawns)
                newJob.pawnDutyDatas[p] = duty;
            var lord = Verse.AI.Group.LordMaker.MakeNewLord(pawns.First().Faction, newJob, pawns.First().Map,pawns);
            QuestUtility.AddQuestTag(ref lord.questTags, "Quest" + quest.id);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref duty, "duty");
            Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
        }
    }
}
