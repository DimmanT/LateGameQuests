using RimWorld;
using RimWorld.QuestGen;
using RimWorld.Planet;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.Grammar;

namespace LoGiQ.QuestNodes
{
    public class QuestNode_RaidSite : QuestNode_Raid 
    {
        public SlateRef<Site> siteToRaid;
		public SlateRef<RaidStrategyDef> raidStrategy;
		public SlateRef<float> points;

		protected override bool TestRunInt(Slate slate)
        {
			if (!Find.Storyteller.difficulty.allowViolentQuests)
			{
				Log.Message("Violence is not allowed");
				return false;
			}
			//todo check that WorlObject can generate map (player can investigate it)  ??? WorldObjectDef.canHaveMap
			if (!slate.Exists("enemyFaction"))
			{
				Log.Message("'enemyFaction' is not set!");
				return false;
			}
			//Log.Message("OK 'EndGameQuests.QuestNodes.QuestNode_RaidSite' can be run!");
			return true;
		}

		protected override void RunInt()
        {
			//Log.Message("Run derived");
			Slate slate = QuestGen.slate;
			Site site = siteToRaid.GetValue(slate);
			//Log.Message($"Decoded site '{site.Label}'");
			QuestGen.slate.Set<Map>("map", site.Map);

			float points = 0f;
			this.points.TryGetValue(slate, out points);

			Faction faction = QuestGen.slate.Get<Faction>("enemyFaction");

			QuestPart_Incident questPart_Incident = new QuestPart_Incident();
			questPart_Incident.debugLabel = "raid";
			questPart_Incident.incident = IncidentDefOf.RaidEnemy;
			int num = 0;
			IncidentParms incidentParms;
			PawnGroupMakerParms defaultPawnGroupMakerParms;
			IEnumerable<PawnKindDef> enumerable;
			do
			{
				incidentParms = GenerateIncidentParmsS(points, faction, slate, questPart_Incident);
				defaultPawnGroupMakerParms = GetDefaultPawnGroupMakerParmsS(PawnGroupKindDefOf.Combat, incidentParms, site.Tile, ensureCanGenerateAtLeastOnePawn: true);
				defaultPawnGroupMakerParms.points = AdjustedRaidPointsS(defaultPawnGroupMakerParms.points, incidentParms.raidArrivalMode, incidentParms.raidStrategy, defaultPawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat);
				enumerable = PawnGroupMakerUtility.GeneratePawnKindsExample(defaultPawnGroupMakerParms);
				num++;
			}
			while (!enumerable.Any() && num < 50);
			if (!enumerable.Any())
			{
				Log.Error("No pawnkinds example for " + QuestGen.quest.root.defName + " parms=" + defaultPawnGroupMakerParms?.ToString() + " iterations=" + num);
			}
			questPart_Incident.SetIncidentParmsAndRemoveTarget(incidentParms);
			questPart_Incident.MapParent = site;
			questPart_Incident.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			QuestGen.quest.AddPart(questPart_Incident);
			QuestGen.AddQuestDescriptionRules(new List<Rule>
			{
				new Rule_String("raidPawnKinds", PawnUtility.PawnKindsToLineList(enumerable, "  - ", ColoredText.ThreatColor)),
				new Rule_String("raidArrivalModeInfo", incidentParms.raidArrivalMode.textWillArrive.Formatted(faction))
			});

			//Log.Message("End of derived RunInt");
		}

		private IncidentParms GenerateIncidentParmsS(float points, Faction faction, Slate slate, QuestPart_Incident questPart)
        {
			IncidentParms incidentParms = new IncidentParms
			{
				forced = true,
				target = null,
				points = System.Math.Max(points, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat)),
				faction = faction,
				pawnGroupMakerSeed = Rand.Int,
				inSignalEnd = QuestGenUtility.HardcodedSignalWithQuestID(inSignalLeave.GetValue(slate)),
				questTag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID(tag.GetValue(slate)),
				canTimeoutOrFlee = (canTimeoutOrFlee.GetValue(slate) ?? true)
			};
			if (raidPawnKind.GetValue(slate) != null)
			{
				incidentParms.pawnKind = raidPawnKind.GetValue(slate);
				
				incidentParms.pawnCount = System.Math.Max(1, (int)System.Math.Round(incidentParms.points / incidentParms.pawnKind.combatPower));
			}
			if (arrivalMode.GetValue(slate) != null)
			{
				incidentParms.raidArrivalMode = arrivalMode.GetValue(slate);
			}
			if (raidStrategy.GetValue(slate) != null)
            {
				incidentParms.raidStrategy = raidStrategy.GetValue(slate);
			}
			if (!customLetterLabel.GetValue(slate).NullOrEmpty() || customLetterLabelRules.GetValue(slate) != null)
			{
				QuestGen.AddTextRequest("root", delegate (string x)
				{
					incidentParms.customLetterLabel = x;
				}, QuestGenUtility.MergeRules(customLetterLabelRules.GetValue(slate), customLetterLabel.GetValue(slate), "root"));
			}
			if (!customLetterText.GetValue(slate).NullOrEmpty() || customLetterTextRules.GetValue(slate) != null)
			{
				QuestGen.AddTextRequest("root", delegate (string x)
				{
					incidentParms.customLetterText = x;
				}, QuestGenUtility.MergeRules(customLetterTextRules.GetValue(slate), customLetterText.GetValue(slate), "root"));
			}
			IncidentWorker_Raid obj = (IncidentWorker_Raid)questPart.incident.Worker;
			obj.ResolveRaidStrategy(incidentParms, PawnGroupKindDefOf.Combat);
			obj.ResolveRaidArriveMode(incidentParms);
			obj.ResolveRaidAgeRestriction(incidentParms);
			if (incidentParms.raidArrivalMode.walkIn)
			{
				incidentParms.spawnCenter = walkInSpot.GetValue(slate) ?? QuestGen.slate.Get<IntVec3?>("walkInSpot") ?? IntVec3.Invalid;
			}
			else
			{
				incidentParms.spawnCenter = dropSpot.GetValue(slate) ?? QuestGen.slate.Get<IntVec3?>("dropSpot") ?? IntVec3.Invalid;
			}
			return incidentParms;
		}

		private static PawnGroupMakerParms GetDefaultPawnGroupMakerParmsS(PawnGroupKindDef groupKind, IncidentParms parms, PlanetTile t, bool ensureCanGenerateAtLeastOnePawn = false)
        {
			PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
			pawnGroupMakerParms.groupKind = groupKind;
			pawnGroupMakerParms.tile = t;
			pawnGroupMakerParms.points = parms.points;
			pawnGroupMakerParms.faction = parms.faction;
			pawnGroupMakerParms.traderKind = parms.traderKind;
			pawnGroupMakerParms.generateFightersOnly = parms.generateFightersOnly;
			pawnGroupMakerParms.raidStrategy = parms.raidStrategy;
			pawnGroupMakerParms.forceOneDowned = parms.raidForceOneDowned;
			pawnGroupMakerParms.seed = parms.pawnGroupMakerSeed;
			pawnGroupMakerParms.ideo = parms.pawnIdeo;
			pawnGroupMakerParms.raidAgeRestriction = parms.raidAgeRestriction;
			if (ensureCanGenerateAtLeastOnePawn && parms.faction != null)
			{
				pawnGroupMakerParms.points = System.Math.Max(pawnGroupMakerParms.points, parms.faction.def.MinPointsToGeneratePawnGroup(groupKind));
			}
			return pawnGroupMakerParms;
		}

		private static float AdjustedRaidPointsS(float points, PawnsArrivalModeDef raidArrivalMode, RaidStrategyDef raidStrategy, Faction faction, PawnGroupKindDef groupKind, RaidAgeRestrictionDef ageRestriction = null)
		{
			if (raidArrivalMode.pointsFactorCurve != null)
			{
				points *= raidArrivalMode.pointsFactorCurve.Evaluate(points);
			}
			if (raidStrategy.pointsFactorCurve != null)
			{
				points *= raidStrategy.pointsFactorCurve.Evaluate(points);
			}
			if (ageRestriction != null)
			{
				points *= ageRestriction.threatPointsFactor;
			}
			points = System.Math.Max(points, raidStrategy.Worker.MinimumPoints(faction, groupKind) * 1.05f);
			return points;
		}

	}
}
