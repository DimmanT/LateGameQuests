using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LoGiQ.QuestNodes
{
    class QuestPart_KeepAnEyeEnemies: QuestPart//, ISignalReceiver
    {
        public bool active = false;

        public Site site;
        public string inSignal = "";
        public string inSignalDisable = "";
        public string outSignalEnemiesArrived = ""; //not working now (16.09.2026)
        public string outSignalEnemiesDefeated = "";
        public bool canBeFogged = false;
        public bool countDormantPawnsAsHostile = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref site, "Site");
            Scribe_Values.Look(ref inSignal, "InSignal");
            Scribe_Values.Look(ref inSignalDisable, "InSignalDisable");
            Scribe_Values.Look(ref outSignalEnemiesArrived, "OutSignalEnemiesArrived");
            Scribe_Values.Look(ref outSignalEnemiesDefeated, "OutSignalEnemiesDefeated");
        }

        public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
        {
            if (!active)
                return;
            
            base.Notify_PawnKilled(pawn, dinfo);
            bool anyEnemy = GenHostility.AnyHostileActiveThreatToPlayer(site.Map,countDormantPawnsAsHostile, canBeFogged);
            //Log.Message($"DBG: anyEnemy? {anyEnemy}");
            if(!anyEnemy)
                Find.SignalManager.SendSignal(new Signal(outSignalEnemiesDefeated));
        }

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            //Log.Message($"QuestPart_KeepAnEyeEnemies: Input signal {signal.tag}.");
            
            //first of all - check activity
            if(active)
            {
                //check disable
                if (signal.tag == inSignalDisable)
                    active = false;
            }
            else
            {
                if (signal.tag == inSignal)
                    active = true;
            }
        }

        //public void Notify_SignalReceived(Signal signal)
        //{
            //Log.Message($"QuestPart_KeepAnEyeEnemies: Generic input signal {signal.tag}, args={signal.args}.");
        //}
    }
}
