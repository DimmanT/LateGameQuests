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
    class DefenceOperation : GameCondition
    {
        const int PAWNS_MAX_COUNT = 5;
        Map map;
        string signalBldStr;
        string signalPawnStr;
        int skipCnt = 0;

        int pawnsInitialCount = PAWNS_MAX_COUNT;
        List<Pawn> pawns;
        List<Thing> things = new List<Thing>();
        float totalMarketValue = 100;
        Dictionary<int, float> initialThingsMarketValues = new Dictionary<int, float>();

        public override void Init()
        {
            Permanent = true;
            base.Init();
            map = base.AffectedMaps.Last();
            signalBldStr  = $"Quest{quest.id}.AnyBuildingDestroyed";
            signalPawnStr = $"Quest{quest.id}.AnyVillagerKilled";

            things = new List<Thing>();

            foreach (var th in map.spawnedThings)
                if (th is Building bld)
                {
                    if (bld.Faction != null)
                        continue;
                    if (bld.def == ThingDefOf.SteleLarge || bld.def == ThingDefOf.SteleGrand || bld.def == ThingDefOf.Sarcophagus)
                        continue;
                    if (bld.MarketValue < 75)
                        continue;

                    totalMarketValue += bld.MarketValue;
                    initialThingsMarketValues[bld.thingIDNumber] = bld.MarketValue;
                    things.Add(bld);

                    //Log.Message($"Track building:  {bld}");

                }
            Log.Message($"Total buildings in account:  {things.Count} of {totalMarketValue}$");

            pawns = new List<Pawn>();
            int cnt = PAWNS_MAX_COUNT;
            foreach (Pawn p in map.mapPawns.AllHumanlikeSpawned)
            {
                Log.Message($"Try add {p} of {p.Faction}, isDead={p.health.Dead}");
                if (p.Faction == null && !p.health.Dead)
                {
                    if (cnt-- > 0)
                    {
                        //Log.Message($"ready to set faction to player for {p}");
                        p.SetFaction(Faction.OfPlayer);
                        pawns.Add(p);
                    }
                    else p.DeSpawn();
                }
            }
            pawnsInitialCount = pawns.Count > 0 ? pawns.Count : -1;
            Log.Message($"Total pawns in account:  {pawns.Count}");

        }

        public override void End()
        {
            base.End();
            foreach(Pawn p in pawns)
            { 
                //Log.Message($"ready to reset faction for {p}");
                p.SetFaction(Faction.OfAncients);
                Job job = JobMaker.MakeJob(JobDefOf.Wait_Wander);
                job.expiryInterval = 30000;
                job.checkOverrideOnExpire = true;
                p.jobs.TryTakeOrderedJob(job);
            }
        }

        public override void GameConditionTick()
        {
            if (skipCnt++ > 120)
            {
                skipCnt = 0;
                for (int i = 0; i < things.Count(); ++i)
                {
                    var t = things[i];
                    if (t.HitPoints < 1 || t.Map != map)
                    {
                        float w = initialThingsMarketValues[t.thingIDNumber] / totalMarketValue;
                        var args = new SignalArgs(new NamedArgument(w, "IncMult"));
                        Find.SignalManager.SendSignal(new Signal(signalBldStr, args));
                        initialThingsMarketValues.Remove(t.thingIDNumber);
                        things.Remove(t);
                        i--;
                    }
                }
                for (int i = 0; i < pawns.Count(); ++i) //todo change to signalreceiver
                {
                    var p = pawns[i];
                    if(p.health.Dead)
                    {
                        float w = 1.0f / pawnsInitialCount;
                        var args = new SignalArgs(new NamedArgument(w, "IncMult"));
                        Find.SignalManager.SendSignal(new Signal(signalPawnStr, args));
                        p.SetFaction(Faction.OfAncients);
                        pawns.Remove(p);
                        i--;
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref map, "Map");
            Scribe_Values.Look(ref signalBldStr, "SignalBld");
            Scribe_Values.Look(ref signalPawnStr, "SignalPawn");
            Scribe_Values.Look(ref pawnsInitialCount, "PawnsInitialCount");
            Scribe_Collections.Look(ref things, "Things", true, LookMode.Reference);
            Scribe_Collections.Look(ref pawns, "Pawns", true, LookMode.Reference);
            Scribe_Collections.Look(ref initialThingsMarketValues, "InitialThingsMarketValues", LookMode.Value);
            Scribe_Values.Look(ref totalMarketValue, "TotalMarketValue");
        }
    }
}
