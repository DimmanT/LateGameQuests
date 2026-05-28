using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace LoGiQ.GameConditions
{
    public class FactoryAssault : GameCondition
    {
        Map map;
        Signal signal;
        int skipCnt = 0;

        List<Thing> things;
        List<int>   hitpoints;

        public override void Init()
        {
            //Log.Message("---beg---");
            Permanent = true;
            base.Init();
            map = base.AffectedMaps.Last();
            signal = new Signal($"Quest{quest.id}.AnyProductionBuildingDamaged");
            
            things = new List<Thing>();
            hitpoints = new List<int>();

            foreach (var th in map.spawnedThings)
                if (th is Building bld)
                {
                    bool isProduction  = bld.def.designationCategory == DesignationCategoryDefOf.Production;
                    bool isPowerSupply = bld.PowerComp != null;
                         isPowerSupply = isPowerSupply && bld.PowerComp.Props != null;
                         isPowerSupply = isPowerSupply && bld.PowerComp.Props.PowerConsumption < -100;

                    bool ok = bld.def.useHitPoints && (isProduction || isPowerSupply);
                    if (ok)
                    {
                        things.Add(bld);
                        hitpoints.Add(bld.HitPoints);
                    }

                    //if (ok) Log.Message($"Add building {bld} to thingHitpoints");

                }
            //Log.Message("---end---");
        }
        public override void GameConditionTick()
        {
            if(skipCnt++ > 120)
            {
                skipCnt = 0;
                for(int i = 0 ; i < things.Count(); ++i)
                {
                    var t = things[i];
                    float rel = (hitpoints[i] - t.HitPoints) / t.MaxHitPoints;

                    if (rel > 0.1) 
                    {
                        //if damage more then 10% of max health from last check
                        Log.Message($"Relative health decrement={rel} of {t}");
                        hitpoints[i] = t.HitPoints;

                        quest.Notify_SignalReceived(signal);
                    }
                }
            }
        }

        public override void End()
        {
            Faction enemyFaction = map.ParentFaction;
            Faction askerFaction = null;
            foreach (var faction in quest.InvolvedFactions)
                if (faction != enemyFaction && !faction.IsPlayer)
                    askerFaction = faction;
            Log.Message($"Replace faction of buildings from {enemyFaction} to {askerFaction}");
            if(askerFaction != null)
            {
                foreach (var thing in map.spawnedThings)
                {
                    if (thing is Building && thing.def.CanHaveFaction && thing.Faction == enemyFaction)
                    {
                        thing.SetFaction(askerFaction);
                    } 
                    else
                    if (thing.Faction == enemyFaction && thing is Pawn)
                    {
                        var p = (Pawn)thing;
                        Log.Message($"Try change faction of {p} from {enemyFaction} to {askerFaction}. Its defName={p.kindDef.defName}");
                        if (p.kindDef.defName == "LoGiQ_EngineerIndustrial")
                        {
                            thing.SetFaction(askerFaction);

                            Job job = JobMaker.MakeJob(JobDefOf.Wait_Wander);
                            job.expiryInterval = 30000;
                            job.checkOverrideOnExpire = true;
                            p.jobs.TryTakeOrderedJob(job);
                        }
                        //maybe heal bleeding and etc?
                    }
                }
            }
            base.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref map, "Map");
            Scribe_Values.Look(ref signal, "Signal");
            Scribe_Collections.Look(ref things, "Things", true, LookMode.Reference);
            Scribe_Collections.Look(ref hitpoints, "Hitpoints");
        }
    }
}
