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

namespace LoGiQ.CQFConditions
{
    internal class CQFCondition_PowerOn : DialogCondition_Target
    {
        public override bool Satisfied(Dictionary<string, TargetInfo> targets, out string reason, Quest quest)
        {
            reason = base.failReason;
            List<ThingWithComps> things = new List<ThingWithComps>();

            var ttt = targets[targetText];
            if (ttt == null)
                Log.Warning("bad target");
            else
            {
                var tttt = (ThingWithComps)ttt.Thing;
                var comp = tttt.GetComp<CompPower>();
                if (comp != null)
                {
                    //Log.Message($"all good {comp.PowerNet.HasActivePowerSource}");
                    return comp.PowerNet.HasActivePowerSource;
                }

            }

            return false;
        }

    }
}
