using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LoGiQ.QuestNodes
{

    class GoalRuntime : IExposable
    {
        public string Label => _config.label;
        public Goal Config => _config;
        protected Goal _config;

        protected GoalRuntime() { } //for loading
        protected GoalRuntime(Goal config)
        {
            this._config = config;
        }
        public virtual void ExposeData() 
        {
            Scribe_Values.Look(ref Config.label, "Label");
        }

    }

    interface IGoalBooleanRuntime
    {
        bool IsCompleted();
    }
    class GoalBooleanRuntime : GoalRuntime, IGoalBooleanRuntime
    {
        public GoalBooleanRuntime() { _config = new GoalBoolean(); } //for loading
        public GoalBooleanRuntime(GoalBoolean cfg) : base(cfg) { state = cfg.initialState; }

        public bool state = false;

        public bool IsCompleted()
        {
            return state;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ((GoalBoolean)Config).setSignal, "SetSignal");
            Scribe_Values.Look(ref ((GoalBoolean)Config).unsetSignal, "UnsetSignal");
            Scribe_Values.Look(ref state, "State");
        }
    }
    interface IGoalProgressRuntime
    {
        float relative();
        float min();
        float max();
        float cur();
    }
    class GoalProgressRuntime : GoalRuntime, IGoalProgressRuntime
    {
        public GoalProgressRuntime() { _config = new GoalProgress(); } //for loading
        public GoalProgressRuntime(GoalProgress cfg, Quest quest) : base(cfg)
        {
            questID = quest.id;
            progressPart = findPart(quest, cfg.progressName);
        }
        private QuestPart_ProgressComplex progressPart = null;

        public float relative(){ return ProgressPart().getRelativeProgress(); }
        public float min() { return 0; }//{ return ProgressPart().progressMin; }
        public float max() { return ProgressPart().progressMax; }
        public float cur() { return ProgressPart().progressCur; }

        private int questID = 0;

        private QuestPart_ProgressComplex ProgressPart()
        {
            if (progressPart != null)
                return progressPart;
            else
            {
                //Find quest
                Quest quest = null;
                foreach (var q in Find.QuestManager.QuestsListForReading)
                    if (q.id == questID)
                        quest = q;
                if(quest == null)
                    throw new Exception($"can not find Quest with id='{questID}'.");

                //Find questPart and remember it
                string progressName = ((GoalProgress)Config).progressName;
                progressPart = findPart(quest, progressName);
                if(progressPart == null)
                    throw new Exception($"can not find QuestPart_ProgressComplex with progressName='{progressName}'.");

                return progressPart;
            }
        }

        private QuestPart_ProgressComplex findPart(Quest quest, string progressName)
        {
            foreach (var part in quest.PartsListForReading)
                if (part is QuestPart_ProgressComplex qp_pcx && qp_pcx.progressName == progressName)
                {
                    progressPart = qp_pcx;
                    return progressPart;
                }
            return null;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ((GoalProgress)Config).progressName, "ProgressName");
            Scribe_Values.Look(ref questID, "QuestID");
        }
    }
    class QuestPart_Goals : QuestPart
    {
        List<GoalRuntime> mandatory = new List<GoalRuntime>();
        List<GoalRuntime> optional = new List<GoalRuntime>();
        List<string> triggerSignals = new List<string>();

        private void checkSignal(string tag, GoalRuntime gr)
        {
            if (gr is GoalBooleanRuntime gbr)
            {
                var gb = (GoalBoolean)(gbr.Config);
                //Log.Message($"Comparing signals '{tag}' vs '{gb.setSignal}' vs '{gb.unsetSignal}' of '{gbr.Label}'");
                if (gb.setSignal == tag)
                    gbr.state = true;
                else
                if (gb.unsetSignal == tag)
                    gbr.state = false;
            }
        }

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            //Log.Message($"Input signal {signal.tag}. Total: {triggerSignals}");
            if (!triggerSignals.Contains(signal.tag))
                return;
            //Log.Message($"Processing signal {signal.tag}");
            foreach (var g in mandatory)
                checkSignal(signal.tag, g);
            foreach (var g in optional)
                checkSignal(signal.tag, g);
        }

        private List<GoalRuntime> MakeRuntime(List<Goal> list)
        {
            if (list == null)
                return null;

            List<GoalRuntime> res = new List<GoalRuntime>();
            foreach( var g in list)
            {
                try
                {
                    if (g is GoalBoolean gb)
                    {
                        res.Add(new GoalBooleanRuntime(gb));
                        if (!gb.setSignal.NullOrEmpty())
                            triggerSignals.Add(gb.setSignal);
                        if (!gb.unsetSignal.NullOrEmpty())
                            triggerSignals.Add(gb.unsetSignal);
                    }
                    else
                    if (g is GoalProgress gp)
                    {
                        res.Add(new GoalProgressRuntime(gp, quest));
                    }
                    else Log.Warning($"Goal {g.label} skipped: undefined type (Boolean/Progress).");
                }
                catch (Exception ex) 
                {
                    Log.Error($"Goal {g.label} skipped: "+ex.ToString());
                }
            }
            return res;
        }

        public void SetMandatory(List<Goal> list)
        {
            mandatory = MakeRuntime(list);
        }
        public void SetOptional (List<Goal> list)
        {
            optional = MakeRuntime(list);
        }
        public IEnumerable<GoalRuntime> getMandatory()
        {
            return mandatory;
        }
        public IEnumerable<GoalRuntime> getOptional()
        {
            return optional;
        }
        public override string ToString() 
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.AppendLine();
            sb.Append("Goals. Mandatory count=");
            sb.Append(mandatory == null ? -1 : mandatory.Count());
            sb.AppendLine();
            sb.Append("Goals. Optional count=");
            sb.Append(optional == null ? -1 : optional.Count());
            sb.AppendLine();
            sb.Append("Goals. Mandatory: ");
            if (mandatory != null)
                foreach(var g in mandatory)
                    sb.Append(g.Label+"("+g.GetType().Name+"),");
            sb.AppendLine();
            sb.Append("Goals. Optional: ");
            if (optional != null)
                foreach (var g in optional)
                    sb.Append(g.Label + ",");
            return sb.ToString();
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref triggerSignals, "triggerSignals");
            Scribe_Collections.Look(ref mandatory, "mandatory");
            Scribe_Collections.Look(ref optional, "optional");
        }
    }
}
