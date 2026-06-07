using RimWorld;
using RimWorld.QuestGen;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using RimWorld.Planet;

namespace LoGiQ.QuestNodes
{
    /// <summary>
    /// \ref QuestNode_Progress
    /// </summary>
    class QuestPart_Progress : QuestPart
    {        
        public string inSignal;
        public string outSignalComplete;
        public int    increment;
        public int    progressCur;
        public int    progressMax;
        public string message;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            if (progressCur >= progressMax)
                return;

            if (signal.tag != inSignal)
                return;
            
            progressCur += increment;

            Log.Message($"progress {progressCur} / {progressMax}");
            TaggedString formattedText = message + $" {progressCur} / {progressMax}";
            if (!formattedText.NullOrEmpty())
            {
                Messages.Message(formattedText, LookTargets.Invalid, MessageTypeDefOf.NeutralEvent, quest.hidden ? null : quest, false);
            }

            if (progressCur >= progressMax && !outSignalComplete.NullOrEmpty())
            {
                Find.SignalManager.SendSignal(new Signal(outSignalComplete, new SignalArgs()));
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref outSignalComplete, "outSignalComplete");
            Scribe_Values.Look(ref increment, "increment");
            Scribe_Values.Look(ref progressCur, "progressCur");
            Scribe_Values.Look(ref progressMax, "progressMax");
            Scribe_Values.Look(ref message, "message");
        }
    }

    /// <summary>
    /// \ref QuestNode_ProgressComplex
    /// </summary>
    class QuestPart_ProgressComplex : QuestPartActivable
    {
        //todo disable signal
        public Dictionary<string, float> inSignals; //map {signal -> increment}
        public float  progressCur;
        public float  progressMax;
        public string message;
        public bool   inverse = false;
        public string progressName;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            //Log.Message($"FACKN SIGNAL RECEIVED {signal.tag}: {progressCur}/{progressMax}");

            if (State != QuestPartState.Enabled)
                Enable(new SignalArgs());

            bool complete = progressCur >= progressMax;
            if (inverse ) complete = !complete;
            if (complete) return;

            float increment = 0; 
            if (!inSignals.TryGetValue(signal.tag, out increment))
                return;

            progressCur += increment;

            Log.Message($"progress {progressCur} / {progressMax}");
            //TaggedString formattedText = message.Formatted(P.currProgress, P.progressMax);
            TaggedString formattedText = message + $" {progressCur} / {progressMax}";
            if (!formattedText.NullOrEmpty())
            {
                Messages.Message(formattedText, LookTargets.Invalid, MessageTypeDefOf.NeutralEvent, quest.hidden ? null : quest, false);
            }

            complete = progressCur >= progressMax;
            if (inverse ) complete = !complete;
            if (complete) Complete();

        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref inSignals, "inSignals");
            Scribe_Values.Look(ref progressCur, "progressCur");
            Scribe_Values.Look(ref progressMax, "progressMax");
            Scribe_Values.Look(ref message, "message");
            Scribe_Values.Look(ref inverse, "inverse");
            Scribe_Values.Look(ref progressName, "progressName");
        }
    }


    /// <summary>
    /// \ref QuestNode_ProgressGiveReward
    /// </summary>
    class QuestPart_ProgressGiveReward : QuestPart
    {
        public MapParent mapParent;

        public string inSignal;
        public float  progressMin;
        public float  progressMax;
        public string progressName;
        public int    maxReward;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            if (signal.tag != inSignal)
                return;

            //Log.Message("QuestPart_ProgressGiveReward A");

            bool  found = false;
            bool  inverse = false;
            float progressCur = 0;
            foreach(var p in quest.PartsListForReading)
                if( p is QuestPart_ProgressComplex ppc)
                    if(ppc.progressName == progressName)
                    {
                        found = true;
                        progressCur = ppc.progressCur;
                        inverse = ppc.inverse;
                        break;
                    }
            //Log.Message("QuestPart_ProgressGiveReward B");
            if (found)
            {
                //Log.Message("QuestPart_ProgressGiveReward C");
                float rel;
                if(progressMin == progressMax) //something gone wrong if they are equal!
                {
                    rel = 1;
                    Log.Warning($"ProgressReward: 'progressMin' is equal to 'progressMax'( {progressMin}={progressMax}), it is wrong!");
                }
                else
                if (inverse)
                {
                    float span = progressMin - progressMax;
                    progressCur -= progressMax;
                    rel = progressCur / span;
                }
                else
                {
                    float span = progressMax - progressMin;
                    progressCur -= progressMin;
                    rel = progressCur / span;
                }
                //Log.Message("QuestPart_ProgressGiveReward D");
                if (rel > 0 && rel < 10)
                {
                    //Log.Message($"Making reward {maxReward}*{rel}");
                    List<Thing> rewardList = new List<Thing>();
                    ThingDef rewardDef = ThingDefOf.Silver;
                    int rewardValue = (int)Math.Ceiling(rel * maxReward);
                    while(rewardValue > 0)
                    {
                        var stack = rewardValue < rewardDef.stackLimit ? rewardValue : rewardDef.stackLimit;
                        //Log.Message($"Stack {stack}/{rewardValue}");
                        rewardValue -= stack;
                        var rewardThing = ThingMaker.MakeThing(rewardDef);
                        rewardThing.stackCount = stack;
                        rewardList.Add(rewardThing);
                    }
                    var cell = SpawnDropPods(rewardList);

                    //todo: add custom labels
                    var text = "LetterQuestDropPodsArrived".Translate(GenLabel.ThingsLabel(rewardList));
                    var label = "LetterLabelQuestDropPodsArrived".Translate();
                    Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, new TargetInfo(cell, mapParent.Map), null, quest);
                }
                else Log.Warning($"Relative progress less then 0 or greater 10! {progressName}");
            }
            else Log.Warning($"Can not find complex progress with name {progressName}");
        }

        protected IntVec3 SpawnDropPods(List<Thing> content)
        {
            Map map = mapParent.Map;
            IntVec3 intVec = GetRandomDropSpot();
            Log.Message($"Ready to spawn at{intVec} of {map} size={content.Count()}");
            DropPodUtility.DropThingGroupsNear(
                intVec, 
                map, 
                new List<List<Thing>> { content }, 
                110, 
                instaDrop: false, 
                leaveSlag: false, 
                false, 
                forbid: false, 
                true, 
                canTransfer: false
                );
            return intVec;
        }

        private IntVec3 GetRandomDropSpot()
        {
            Map map = mapParent.Map;
            return DropCellFinder.TradeDropSpot(map);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref progressMin, "progressMin");
            Scribe_Values.Look(ref progressMax, "progressMax");
            Scribe_Values.Look(ref progressName, "progressName");
            Scribe_Values.Look(ref maxReward, "maxReward");
            Scribe_References.Look(ref mapParent, "mapParent"); 
        }
    }
}
