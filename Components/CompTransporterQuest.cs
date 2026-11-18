using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoGiQ.Components
{
    //todo remove gizmos
    class CompTransporterQuest : CompTransporter, ISignalReceiver
    {
        public string name = "";
        public CompTransporterQuest()
        {
            Verse.Find.SignalManager.RegisterReceiver(this);
        }

        public override void Notify_SignalReceived(Signal signal)
        {
            var awaitedTag = parent.questTags.FirstOrDefault() + '.' + "ThingAdded";
            //Verse.Log.Message($"income signal {signal.tag} vs {awaitedTag}");
            if(awaitedTag == signal.tag)
            {
                //... checking is all loaded ...
                if (!AnythingLeftToLoad)
                {
                    innerContainer.ClearAndDestroyContents(Verse.DestroyMode.QuestLogic);//consume all things in it
                    CleanUpLoadingVars(parent.MapHeld);
                    string tag = name;
                    if (name != "") 
                        tag += '.';
                    tag += "LoadingFinished";
                    QuestUtility.SendQuestTargetSignals(parent.questTags, tag);
                }
                //............
            }
        }

        public override IEnumerable<Verse.Gizmo> CompGetGizmosExtra()
        {
            return Enumerable.Empty<Verse.Gizmo>();
        }
    }

    class CompProperties_TransporterQuest : CompProperties_Transporter
    {
        public CompProperties_TransporterQuest()
        {
            compClass = typeof(CompTransporterQuest);
        }
    }
}
