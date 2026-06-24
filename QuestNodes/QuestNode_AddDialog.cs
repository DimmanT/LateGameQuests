using QuestEditor_Library;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;


namespace LoGiQ.QuestNodes
{
    class QuestNode_AddDialog : QuestNode
    { 
        public SlateRef<Pawn> pawn;
        public SlateRef<DialogManagerDef> dialogManager;

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            Pawn pawnValue = pawn.GetValue(slate);
            if(pawnValue == null)
            {
                Log.Error("QuestNode_AddDialog: pawn is null");
                return;
            }
            DialogManagerDef dialogValue = dialogManager.GetValue(slate);
            if (dialogValue == null)
            {
                Log.Error("QuestNode_AddDialog: dialog is null");
                return;
            }
            Current.Game.GetComponent<GameComponent_Editor>().AddDialog(pawnValue, dialogValue);
        }

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }
    }

}
