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
    class CQFCondition_ColonistsInRadius : DialogCondition_Target
    {
        public int radius = 5;

        public int colonistCount = 0;

        public bool needToBeGreater = true;

        private string buffer1,buffer2;

        public override bool Satisfied(Dictionary<string, TargetInfo> targets, out string reason, Quest quest)
        {
            reason = base.failReason;
            List<ThingWithComps> things = new List<ThingWithComps>();

            var centerTarget = targets[targetText];
            if (centerTarget == null)
                Log.Warning("bad center target");
            else
            {
                var centerThing = (Thing)centerTarget.Thing;
                var center = centerThing.Position;
                var map = centerThing.Map;
                if(map == null)
                {
                    Log.Error($"Bad map of thing {centerThing}");
                    return false;
                }
                //... get all player colonists from map and check thier position ...
                int colonistsInRadius = 0;
                foreach(Pawn p in map.PlayerPawnsForStoryteller)
                {
                    if (p.Position.DistanceTo(center) < radius)
                        colonistsInRadius++;
                    //optimize littlebit
                    if (needToBeGreater && colonistsInRadius > colonistCount)
                        break;
                }
                Log.Message($"Checking colonists total={map.PlayerPawnsForStoryteller.Count()} , result= {colonistsInRadius} / {colonistCount}");
                return needToBeGreater ^ (colonistsInRadius <= colonistCount);
                //.....................................................................
                
            }

            return false;
        }

        public override XElement SaveToXElement(string nodeName)
        {
            XElement xElement = base.SaveToXElement(nodeName);
            xElement.Add(new XElement("radius", radius));
            xElement.Add(new XElement("colonistCount", colonistCount));
            xElement.Add(new XElement("needToBeGreater", needToBeGreater));
            return xElement;
        }

        public override void Draw(ref float y, Rect inRect, float x)
        {
            base.Draw(ref y, inRect, x);
            CQFEditorTools.DrawLabelAndText_Line(y, "Radius", ref radius, ref buffer1, x);
            y += 30f;
            CQFEditorTools.DrawLabelAndText_Line(y, "Player's colonists in radius", ref colonistCount, ref buffer2, x);
            y += 30f;
            Widgets.CheckboxLabeled(new Rect(x, y, 325f, 20f), "NeedToBeGreater".Translate(), ref needToBeGreater);
            y += 25f;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref radius, "radius", 0);
            Scribe_Values.Look(ref colonistCount, "colonistCount", 0);
            Scribe_Values.Look(ref needToBeGreater,"needToBeGreater", true);
        }
    }
}
