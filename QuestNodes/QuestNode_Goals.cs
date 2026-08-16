using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LoGiQ.QuestNodes
{
    public class Goal
    {
        
        public string label = "unnamed";
        //todo setVisible/setInvisible (it is important for BuildVillage quest)
    }
    public class GoalBoolean : Goal
    {
        [NoTranslate]
        public string setSignal = "";
        [NoTranslate]
        public string unsetSignal = "";

        public bool initialState = false;
    }
    public class GoalProgress : Goal
    {
        [NoTranslate]
        public string progressName = "";

        public bool percentOnly = true; ///< show only percent, not real values
    }
    public class QuestNode_Goals : QuestNode
    {
        public SlateRef<List<Goal>> mandatory = null;
        public SlateRef<List<Goal>> optional = null;
        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            var questPart = new QuestPart_Goals();
            QuestGen.quest.AddPart(questPart);

            if (mandatory.TryGetValue(slate, out List<Goal> manList) && manList != null)
                questPart.SetMandatory(PrependQuestIdToSignals(ref manList));
            if (optional.TryGetValue(slate, out List<Goal> optList) && optList !=null)
                questPart.SetOptional(PrependQuestIdToSignals(ref optList));
        }

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }

        private ref List<Goal> PrependQuestIdToSignals(ref List<Goal> list)
        {
            foreach(var g in list)
                if(g is GoalBoolean gb)
                {
                    if (!gb.setSignal.NullOrEmpty())
                        gb.setSignal = QuestGenUtility.HardcodedSignalWithQuestID(gb.setSignal);
                    if (!gb.unsetSignal.NullOrEmpty())
                        gb.unsetSignal = QuestGenUtility.HardcodedSignalWithQuestID(gb.unsetSignal);
                }
            return ref list;
        }
    }
}
