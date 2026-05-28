using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using QuestEditor_Library;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace LoGiQ.CQFActions
{
    class CQFAction_InteractWith : QuestEditor_Library.CQFAction_Target
    {
        public string targetForInteract;
        public string interactionText;
        public override void RealWork(Dictionary<string, TargetInfo> targets, Quest quest)
        {
            var targetForInteractInfo = resolveTargetForInteract(quest);
            if(targetForInteractInfo == null)
            {
                Log.Warning($"Can not resolve target for interact with '{targetForInteract}'");
            }
            //Log.Message($"A-A-A {targetForInteractInfo.Label},{targetForInteractInfo.Cell}.");
            targets.ToList().ForEach(delegate (KeyValuePair<string, TargetInfo> t)
            {
                var targetPawn = t.Value.Thing;
                if (targetPawn is Pawn p)
                {
                    Log.Message($"Now {p.Label} will go to {targetForInteractInfo.Label} at {targetForInteractInfo.Cell}.");
                    //JobDriver_Goto jobGoTo = new JobDriver_Goto();
                    //               jobGoTo.job.targetA = new LocalTargetInfo(targetForInteractInfo.Thing);
                    //               jobGoTo.pawn = p;
                    //p.jobs.TryTakeOrderedJob(jobGoTo.job);
                    Job job = new Job(QEDefOf.QE_InteractingWithTarget);
                    job.targetA = new LocalTargetInfo(targetForInteractInfo.Thing);
                    job.reportStringOverride = interactionText;
                    bool ok = p.jobs.TryTakeOrderedJob(job);
                    Log.Message($"is success? {ok}, '{job.GetReport(p)}'");
                }
                else
                {
                    Log.Warning("Only Pawns can interact with something.");
                }
            });
        }
        protected TargetInfo resolveTargetForInteract(Quest quest)
        {
            var tmpList = new List<string> {targetForInteract};
            Dictionary<string, TargetInfo> tmpTargets = new Dictionary<string, TargetInfo>();
            var eligibleTargets = GameTools.GetTargets(tmpTargets, quest, tmpList);
            if (eligibleTargets.NullOrEmpty())
                return null;
            else return eligibleTargets[targetForInteract];
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref targetForInteract, "targetForInteract");
            Scribe_Values.Look(ref interactionText, "interactionText");
        }
        public override XElement SaveToXElement(string nodeName)
        {
            XElement result = base.SaveToXElement(nodeName);
            result.Add(new XElement("targetForInteract", targetForInteract));
            return result;
        }

        public override void Draw(ref float y, Rect inRect, float x)
        {
            base.Draw(ref y, inRect, x);
            CQFEditorTools.DrawLabelAndText_Line(y, "InteractWith", ref targetForInteract, x, 150f);
            y += 30f;
            CQFEditorTools.DrawLabelAndText_Line(y, "interactionText", ref interactionText, x, 150f);
            y += 30f;
        }
    }
}
