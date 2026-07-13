using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace LoGiQ.Events
{
    public class IncidentWorker_QuestGiver : IncidentWorker
    {
        static List<QuestScriptDef> LoGiQ_Quests;
        static float minPointsOverAll = 0;

        public override float BaseChanceThisGame
        {
            get
            {
                float[] MULT = { 0, 0.2f, 0.5f, 1, 2, 4 };
                var chance = base.BaseChanceThisGame;
                var rate = Math.Min(5, Mod.Settings.ChoosenRate);
                    rate = Math.Max(0, rate);

                //Log.Message($"Chance is {chance}->{chance * MULT[rate]}");
                return chance * MULT[rate];
            }
        }
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }

            if (LoGiQ_Quests.NullOrEmpty())
                RecacheLoGiQQuests();

            //Log.Message($"CanFireNowSub points {parms.points}/{minPointsOverAll}");
            if (parms.points < minPointsOverAll)
                return false;

            var ok =  PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists_NoSuspended.Any();

            //Log.Message($"CanFireNowSub {ok}");

            return ok;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            //Log.Message($"EXECUTE {parms.points}.");
            QuestScriptDef questScriptDef = ChooseLoGiQRandomQuest(parms.points, parms.target);
            if (questScriptDef == null)
            {
                //Log.Message("No quest to run now.");
                return false;
            }
            //Log.Message($"quest is {questScriptDef.defName}");
            parms.questScriptDef = questScriptDef;
            GiveQuest(parms, questScriptDef);
            return true;
        }

        protected virtual void GiveQuest(IncidentParms parms, QuestScriptDef questDef)
        {
            Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(questDef, parms.points);
            if (!quest.hidden && questDef.sendAvailableLetter)
            {
                QuestUtility.SendLetterQuestAvailable(quest);
            }
        }

        static protected bool CanQuestOccurOnTile(PlanetTile tile, QuestScriptDef quest)
        {
            if (!tile.Valid)
            {
                return true;
            }
            if (quest != null)
            {
                PlanetLayerDef layerDef = tile.LayerDef;
                if (!quest.layerWhitelist.NullOrEmpty() && !quest.layerWhitelist.Contains(layerDef))
                {
                    return false;
                }
                if (!quest.layerBlacklist.NullOrEmpty() && quest.layerBlacklist.Contains(layerDef))
                {
                    return false;
                }
                if (!quest.canOccurOnAllPlanetLayers && layerDef.onlyAllowWhitelistedIncidents && (quest.layerWhitelist.NullOrEmpty() || !quest.layerWhitelist.Contains(layerDef)))
                {
                    return false;
                }
            }
            return !tile.LayerDef.onlyAllowWhitelistedQuests;
        }

        static void RecacheLoGiQQuests()
        {
            LoGiQ_Quests = new List<QuestScriptDef>();
            var collection = DefDatabase<QuestScriptDef>.AllDefs.Where((QuestScriptDef x) => x.decreeTags!=null && x.decreeTags.Contains("LoGiQ_Random"));
            LoGiQ_Quests.AddRange(collection);

            //... find minimal required points ...
            minPointsOverAll = 9999;
            foreach (var q in collection)
                if (q.rootMinPoints < minPointsOverAll)
                    minPointsOverAll = q.rootMinPoints;

            Log.Message($"Recaching LoGiQ quests. Found {LoGiQ_Quests.Count()} quests. MinPointsOverAll = {minPointsOverAll}.");
        }

        protected static QuestScriptDef ChooseLoGiQRandomQuest(float points, IIncidentTarget target)
        {
            if(LoGiQ_Quests.NullOrEmpty())
                RecacheLoGiQQuests();
            QuestScriptDef result = null;
            LoGiQ_Quests.Where((QuestScriptDef x) => x.CanRun(points, target) && CanQuestOccurOnTile(target.Tile, x))
                        .TryRandomElementByWeight((QuestScriptDef x) => RimWorld.NaturalRandomQuestChooser.GetNaturalRandomSelectionWeight(x, points, target.StoryState), out result);

            return result;
        }
    }
}
