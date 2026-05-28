using QuestEditor_Library;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using Verse;

namespace LoGiQ.CQFActions 
{
    class CQFAction_Leave : QuestEditor_Library.CQFAction_Target
    {
        public string customLetter;
        public bool sendStandardLetter = false;
        public bool wakeUp = true;
        public override void RealWork(Dictionary<string, TargetInfo> targets, Quest quest)
        {
            var pawns = resolveTargets(targets, quest);
            if (!pawns.NullOrEmpty())
            {
                LeaveQuestPartUtility.MakePawnsLeave(pawns, sendStandardLetter, quest, wakeUp);
                if(!customLetter.NullOrEmpty())
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelPawnLeaving".Translate(), customLetter.Translate(), LetterDefOf.NeutralEvent, pawns[0], null, quest);
                }
            }
        }

        protected List<Pawn> resolveTargets(Dictionary<string, TargetInfo> targets, Quest quest)
        {
            List<Pawn> pawns = new List<Pawn>();
            foreach(var t in targets)
                if (t.Value.Thing is Pawn p)
                    pawns.Add(p);
            return pawns;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref customLetter, "customLetter");
            Scribe_Values.Look(ref sendStandardLetter, "sendStandardLetter");
            Scribe_Values.Look(ref wakeUp, "wakeUp");
        }
        public override XElement SaveToXElement(string nodeName)
        {
            XElement result = base.SaveToXElement(nodeName);
            result.Add(new XElement("customLetter", customLetter));
            result.Add(new XElement("sendStandardLetter", sendStandardLetter));
            result.Add(new XElement("wakeUp", wakeUp));
            return result;
        }

        public override void Draw(ref float y, Rect inRect, float x)
        {
            base.Draw(ref y, inRect, x);
            CQFEditorTools.DrawLabelAndText_Line(y, "CustomLetter", ref customLetter, x, 150f);
            y += 30f;
            Rect rect = new Rect(x, y, 350f, 25f);
            Widgets.CheckboxLabeled(rect, "sendStandardLetter", ref sendStandardLetter);
            y += 30f;
            rect.y += 30f;
            Widgets.CheckboxLabeled(rect, "wakeUp", ref wakeUp);
            y += 30f;
            rect.y += 30f;
        }
    }
}
