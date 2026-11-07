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
    class QuestPart_KeepAnEyeEnemies: QuestPartActivable//, ISignalReceiver
    {


        public Site site;
        public string outSignalEnemiesArrived = ""; 
        public string outSignalEnemiesDefeated = "";
        public bool canBeFogged = false;
        public bool countDormantPawnsAsHostile = false;

        private bool Active => State == QuestPartState.Enabled;
        private int skiper = 0;
        private bool prevAnyEnemy = false; //previouse value of anyEnemy flag

        /// manually set active/incative (enable/disable)
        /// <param name="on">set enable (active) or disable (inactive)</param>
        public void SetActive(bool on)
        {
            if (on)
                 Enable(new SignalArgs());
            else Disable();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref site, "Site");
            Scribe_Values.Look(ref outSignalEnemiesArrived, "OutSignalEnemiesArrived");
            Scribe_Values.Look(ref outSignalEnemiesDefeated, "OutSignalEnemiesDefeated");
            Scribe_Values.Look(ref prevAnyEnemy, "PrevAnyEnemy");
        }

        public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
        {
            if (!Active)
                return;
            
            base.Notify_PawnKilled(pawn, dinfo);
            bool anyEnemy = GenHostility.AnyHostileActiveThreatToPlayer(site.Map,countDormantPawnsAsHostile, canBeFogged);
            //Log.Message($"DBG: anyEnemy? {anyEnemy}");
            if(!anyEnemy)
                Find.SignalManager.SendSignal(new Signal(outSignalEnemiesDefeated));
            prevAnyEnemy = anyEnemy;
        }
        public override void QuestPartTick()
        {
            if (Active && skiper++ == 300)
            {
                skiper = 0;
                bool anyEnemy = GenHostility.AnyHostileActiveThreatToPlayer(site.Map, countDormantPawnsAsHostile, canBeFogged);
                if(anyEnemy && !prevAnyEnemy)
                    Find.SignalManager.SendSignal(new Signal(outSignalEnemiesArrived));
                else
                if (!anyEnemy && prevAnyEnemy)
                    Find.SignalManager.SendSignal(new Signal(outSignalEnemiesDefeated));

                //Log.Message($"checking enemies: now {anyEnemy} / was {prevAnyEnemy}");

                prevAnyEnemy = anyEnemy;
            }

        }

        //public void Notify_SignalReceived(Signal signal)
        //{
        //Log.Message($"QuestPart_KeepAnEyeEnemies: Generic input signal {signal.tag}, args={signal.args}.");
        //}
    }
}
