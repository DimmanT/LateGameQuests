using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LoGiQ.GameConditions
{
    public class EmergencyOperationBase : GameCondition
    {
        Faction faction = null;
        Map map;
        string signalStr;
        List<Pawn> pawns;
        float totalWeight = 0;

        int skipCnt = 0;
        public override void Init()
        {
            Permanent = true;
            base.Init();
            map = base.AffectedMaps.Last();
            signalStr = $"Quest{quest.id}.AnyPersonSaved";
            foreach (var f in quest.InvolvedFactions)
                if (!f.IsPlayer && !f.HostileTo(Faction.OfPlayer))
                    faction = f;
            if (faction == null)
                throw new Exception("GameCondition.EmergencyOperation: Can not resolve faction!");
            if (map == null)
                throw new Exception("GameCondition.EmergencyOperation: Can not resolve map!");

            //pawns = map.mapPawns.SpawnedPawnsInFaction(faction);
            //pawns = map.mapPawns.AllHumanlikeSpawned;

            //Log.Error($"GameCondition.EmergencyOperation: Any human being at map {map}!");
            //Log.Message($"SJSJJSJS xxx={map.mapPawns.AllHumanlikeSpawned.Count()}");
            pawns = new List<Pawn>();
            foreach (Pawn p in map.mapPawns.AllHumanlikeSpawned)
            {
                //Log.Message($"Try add {p} of {p.Faction} (need {faction}), isDead={p.health.Dead}");
                if ( (p.Faction == null || p.Faction == faction) && !p.health.Dead )
                {
                    p.SetFaction(faction);
                    pawns.Add(p);
                    totalWeight += WeightOfPawn(p);

                    //preparePawn(p);
                    Hediff heatstroke = HediffMaker.MakeHediff(HediffDefOf.Heatstroke, p);
                    heatstroke.Severity = Verse.Rand.Range(0.3f, 0.6f);
                    p.health.AddHediff(heatstroke);

                    //For most pawn set job to fight with fire
                    if (Verse.Rand.Chance(0.75f))
                    {
                        Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.ExtinguishFiresNearby);
                        bool ok = p.jobs.TryTakeOrderedJob(job);
                        if(ok)
                        {
                            Log.Message("OKKKKKK");
                        }
                        else
                        {
                            Log.Message("YOUR JOB IS INVALID !");
                        }
                    }
                }
            }
            


            //Log.Message($"EmergencyOperation.Pawns initial1 count {pawns.Count()} of {faction}");
            //pawns.RemoveAll(p => !p.RaceProps.Humanlike);
            //pawns.RemoveAll(p => p.Faction != faction);
            //pawns.RemoveAll(p => p.health.Dead);
            //foreach (Pawn p in pawns)
            //{
            //    totalWeight += WeightOfPawn(p);
            //}
            Log.Message($"EmergencyOperation.Pawns initial2 count {pawns.Count()}");
        }
        public override void GameConditionTick()
        {
            if (skipCnt++ < 120)
                return;
            
            skipCnt = 0;
            //Log.Message($"EmergencyOperation.Pawns count {pawns.Count()}");

            pawns.RemoveAll(p => p.health.Dead);
            foreach (Pawn p in pawns)
            {
                //Log.Message($"Check pawn {p}. Map={p.Map}");
                if (p.Map == null || p.Map != map)
                {
                    float w = 100 * WeightOfPawn(p) / totalWeight;
                    var args = new SignalArgs(new NamedArgument(w, "IncMult"));
                    Find.SignalManager.SendSignal(new Signal(signalStr, args));
                    pawns.Remove(p);
                    break;
                }
            }

            if (pawns.NullOrEmpty())
            {
                Log.Message("AllPeopleEvacuatedOrDead");
                Find.SignalManager.SendSignal(new Signal($"Quest{quest.id}.AllPeopleEvacuatedOrDead"));
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref map, "Map");
            Scribe_References.Look(ref faction, "Faction");
            Scribe_Values.Look(ref signalStr, "Signal");
            Scribe_Collections.Look(ref pawns, "Pawns", true, LookMode.Reference);
            Scribe_Values.Look(ref totalWeight, "TotalWeight");
        }

        protected float WeightOfPawn(Pawn p)
        {
            float w = 1;
            //children are more important
            if (!p.ageTracker.Adult)
                w *= 1.5f;
            //royals are very more important
            float royalSeniority = p.GetCurrentTitleSeniorityIn(faction);
            w += w * royalSeniority / 2;

            return w;
        }

        protected void preparePawn(Pawn p)
        {
            //move here
        }
    }

    public class EmergencyOperation_FireEvent : EmergencyOperationBase
    {
        public override void Init()
        {
            const int MAX_COUNT = 20;
            const float COEF = 0.92f;

            base.Init();
            var map = base.AffectedMaps.Last();
            var thingsToLit = new List<Thing>();
            foreach(var t in map.spawnedThings)
            {
                if (thingsToLit.Count() >= MAX_COUNT)
                    break;
                if(t is Building bld)
                {
                    if(goodToLit(bld))
                        thingsToLit.Add(t);
                }
            }
            float chance = 1.0f;
            foreach (var t in thingsToLit)
            {
                bool lit = Verse.Rand.Chance(chance);
                if (FireUtility.TryStartFireIn(t.Position, map, 0.9f, null))
                {
                    chance *= COEF;
                    //lit some around
                    for (int i = -3; i < 3; ++i)
                        for (int j = -3; j < 3; ++j)
                        {
                            var c2 = t.Position;
                            c2.x += i;
                            c2.z += j;
                            if (c2.x < 0 || c2.z < 0)
                                continue;
                            if(Verse.Rand.Chance(0.4f))
                                FireUtility.TryStartFireIn(c2, map, Verse.Rand.Range(30,100)*0.1f, null);
                        }
                }
                //todo fix case when fire starts too far from first point
            }
        }

        protected List<ThingDef> DefsToLit = new List<ThingDef>{ 
            ThingDefOf.Bed, 
            ThingDefOf.Bedroll,
            ThingDefOf.Battery,
            ThingDefOf.Shelf,
            ThingDefOf.ShelfSmall,
            ThingDefOf.Campfire,
            ThingDefOf.Heater
        };
        protected bool goodToLit(Building bld)
        {
            //(1) check special defs
            bool good = DefsToLit.Contains(bld.def);
            //(2) check power producing
            if (!good)
            {
                bool isPowerSupply = bld.PowerComp != null;
                isPowerSupply = isPowerSupply && bld.PowerComp.Props != null;
                isPowerSupply = isPowerSupply && bld.PowerComp.Props.PowerConsumption < -100;
                good = isPowerSupply;
                //maybe check tech level?
            }
            return good;
        }
    }
}
