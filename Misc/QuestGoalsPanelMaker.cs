using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace LoGiQ
{
    static class QuestGoalsPanelMaker
    {
        static public void makePanel(Rect innerRect, ref float curY)
        {
            if (selectedQuest == null || !selectedQuest.EverAccepted)
                return;

            QuestNodes.QuestPart_Goals goalsPart = null;
            foreach (var part in selectedQuest.PartsListForReading)
                if(part is QuestNodes.QuestPart_Goals gp)
                {
                    goalsPart = gp;
                    break;
                }
            if (goalsPart == null)
                return;

            const int H_MARGIN = 2;
            const int W_MARGIN = 4;
            Rect boxRect = innerRect;
            boxRect.y = curY + H_MARGIN;
            curY += H_MARGIN;
            innerRect.x += W_MARGIN;
            innerRect.width -= 2 * W_MARGIN;

            curY += H_MARGIN;
            DrawHeaderL1("LoGiQ_QuestGoals".Translate(), innerRect, ref curY);
            DrawMandatory(goalsPart.getMandatory(), innerRect, ref curY);
            DrawOptional (goalsPart.getOptional (), innerRect, ref curY);
            boxRect.height = curY + 2*H_MARGIN - boxRect.y;
            DrawBox(boxRect, H_MARGIN);
            curY += H_MARGIN;
        }

        static private void DrawBox(Rect rect, int thickness)
        {
            var remColor = GUI.color;
            try  {
                GUI.color = new Color(0.60f,0.60f,0.60f);
                Widgets.DrawBox(rect, thickness);
            } catch { }
            GUI.color = remColor;
        }

        static private void DrawGoal(QuestNodes.GoalRuntime goal, Rect innerRect, ref float curY)
        {
            var label = goal.Label.Translate();
            if(goal is QuestNodes.IGoalBooleanRuntime gb)
            {
                bool checkedOn = gb.IsCompleted();
                var h = Text.CalcHeight(label, innerRect.width);
                Rect rect = new Rect(innerRect.x, curY, innerRect.width, h);
                Widgets.CheckboxLabeled(rect, label, ref checkedOn, disabled: true);
                curY += h;
            }
            else
            if (goal is QuestNodes.IGoalProgressRuntime gp)
            {
                const int W_MARGIN = 4;
                var width = (innerRect.width / 2) - W_MARGIN;
                var h = Text.CalcHeight(label, width);
                Rect rect = new Rect(innerRect.x, curY, width, h);
                Widgets.Label(rect, label); //label of progress

                Rect rect2 = new Rect(innerRect.x + width + 2* W_MARGIN, curY, width, 0.85f*h);
                //todo check percentOnly
                float p = gp.relative();
                Widgets.FillableBar(rect2, p);  //progressbar

                curY += h;
            }
        }

        static private void DrawHeaderL1(string text, Rect innerRect, ref float curY)
        {
            var remAnchor = Text.Anchor;
            var remFont   = Text.Font;
            try {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                var h = Text.CalcHeight(text, innerRect.width);
                Rect rect = new Rect(innerRect.x, curY, innerRect.width, h);
                Widgets.Label(rect, text);
                curY += h;
            } catch { }
            Text.Anchor = remAnchor;
            Text.Font = remFont;
        }

        static private void DrawHeaderL2(string text, Rect innerRect, ref float curY)
        {
            var h = Text.CalcHeight(text, innerRect.width);
            Rect rect = new Rect(innerRect.x, curY, innerRect.width, h);
            Widgets.DrawBoxSolid(rect, new Color(0.1f,0.1f,0.1f));
            Widgets.Label(rect, text);
            curY += h;
        }
        static private void DrawMandatory(IEnumerable<QuestNodes.GoalRuntime> goals, Rect innerRect, ref float curY)
        {
            if (goals.EnumerableNullOrEmpty())
                return;

            DrawHeaderL2("LoGiQ_MandatoryGoals".Translate(), innerRect, ref curY);
            innerRect.x += 10;
            innerRect.width -= 10;

            foreach (var g in goals)
                DrawGoal(g,innerRect,ref curY);
        }
        static private void DrawOptional(IEnumerable<QuestNodes.GoalRuntime> goals, Rect innerRect, ref float curY)
        {
            if (goals.EnumerableNullOrEmpty())
                return;

            DrawHeaderL2("LoGiQ_OptionalGoals".Translate(), innerRect, ref curY);
            innerRect.x += 10;
            innerRect.width -= 10;

            foreach (var g in goals)
                DrawGoal(g, innerRect, ref curY);
        }

        static public void Select(Quest quest) 
        { 
            selectedQuest = quest; 
            //todo clear cache
        }
        static public Quest Selected() { return selectedQuest; }

        static private Quest selectedQuest = null; ///< for sharing between Select() and DoDescription() 
    }
}
