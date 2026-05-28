using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LoGiQ.QuestNodes
{
    /// <summary>
    /// Advanced faction searching
    /// </summary>
    internal class QuestNode_GetFactionAdv : RimWorld.QuestGen.QuestNode_GetFaction
    {
		public SlateRef<Faction> mustBeHostileToFaction;

		[NoTranslate]
		public SlateRef<string> minTechLevel;
		[NoTranslate]
		public SlateRef<string> maxTechLevel;

		protected override bool TestRunInt(Slate slate)
		{
			if (slate.TryGet<Faction>(storeAs.GetValue(slate), out var var) && IsGoodFactionAdv(var, slate))
			{
				return true;
			}
			if (TryFindFactionAdv(out var, slate))
			{
				slate.Set(storeAs.GetValue(slate), var);
				return true;
			}
			return false;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			if ((!slate.TryGet<Faction>(storeAs.GetValue(slate), out var var) || !IsGoodFactionAdv(var, slate)) && TryFindFactionAdv(out var, slate))
			{
				slate.Set(storeAs.GetValue(slate), var);
				if (!var.Hidden)
				{
					QuestPart_InvolvedFactions questPart_InvolvedFactions = new QuestPart_InvolvedFactions();
					questPart_InvolvedFactions.factions.Add(var);
					QuestGen.quest.AddPart(questPart_InvolvedFactions);
				}
			}
		}
		protected bool IsGoodFactionAdv(Faction faction, Slate slate)
        {
			bool ok = IsGoodFactionOld(faction, slate); //before all use original decision about faction
			if (ok)
			{
				// check for tech level
				string minTechLevelStr = minTechLevel.GetValue(slate);
				string maxTechLevelStr = maxTechLevel.GetValue(slate);
				if (!minTechLevelStr.NullOrEmpty() || !maxTechLevelStr.NullOrEmpty())
				{
					if (minTechLevelStr.NullOrEmpty()) minTechLevelStr = "Undefined";
					if (maxTechLevelStr.NullOrEmpty()) maxTechLevelStr = "Archotech";
					TechLevel minTechLevelVal = ToTechLevel(minTechLevelStr);
					TechLevel maxTechLevelVal = ToTechLevel(maxTechLevelStr);
					ok = faction.def.techLevel >= minTechLevelVal &&
						 faction.def.techLevel <= maxTechLevelVal;
				}
			}
			
			if (ok)
			{
				//check for hostileTo
				mustBeHostileToFaction.TryGetValue(slate, out Faction hostileFaction);
				if (hostileFaction != null)
					ok = faction.HostileTo(hostileFaction);
			}
			return ok;
		}
		protected bool TryFindFactionAdv(out Faction faction, Slate slate)
		{
			return (from x in Find.FactionManager.GetFactions(allowHidden: true)
					where IsGoodFactionAdv(x, slate)
					select x).TryRandomElement(out faction);
		}
		protected TechLevel ToTechLevel(string s)
        {
            switch (s)
            {
				case "Undefined"  : return TechLevel.Undefined;
				case "Animal"	  : return TechLevel.Animal	;
				case "Neolithic"  : return TechLevel.Neolithic;
				case "Medieval"	  : return TechLevel.Medieval	;
				case "Industrial" : return TechLevel.Industrial;
				case "Spacer"	  : return TechLevel.Spacer	;
				case "Ultra"	  : return TechLevel.Ultra	;
				case "Archotech"  : return TechLevel.Archotech;
				default: return TechLevel.Undefined;
            }
        }
		//One can not derive this functions because they are private. Disappoint 😒.
		//Single way is repeat them.
		protected bool IsGoodFactionOld(Faction faction, Slate slate)
        {
			if (faction.Hidden && (allowedHiddenFactions.GetValue(slate) == null || !allowedHiddenFactions.GetValue(slate).Contains(faction)))
			{
				return false;
			}
			if (ofPawn.GetValue(slate) != null && faction != ofPawn.GetValue(slate).Faction)
			{
				return false;
			}
			if (exclude.GetValue(slate) != null && exclude.GetValue(slate).Contains(faction))
			{
				return false;
			}
			if (mustBePermanentEnemy.GetValue(slate) && !faction.def.permanentEnemy)
			{
				return false;
			}
			if (!allowEnemy.GetValue(slate) && faction.HostileTo(Faction.OfPlayer))
			{
				return false;
			}
			if (!allowNeutral.GetValue(slate) && faction.PlayerRelationKind == FactionRelationKind.Neutral)
			{
				return false;
			}
			if (!allowAlly.GetValue(slate) && faction.PlayerRelationKind == FactionRelationKind.Ally)
			{
				return false;
			}
			bool? value = allowPermanentEnemy.GetValue(slate);
			if (value.HasValue && value != true && faction.def.permanentEnemy)
			{
				return false;
			}
			if (playerCantBeAttackingCurrently.GetValue(slate) && SettlementUtility.IsPlayerAttackingAnySettlementOf(faction))
			{
				return false;
			}
			if (mustHaveGoodwillRewardsEnabled.GetValue(slate) && !faction.allowGoodwillRewards)
			{
				return false;
			}
			if (peaceTalksCantExist.GetValue(slate))
			{
				if (PeaceTalksExist(faction))
				{
					return false;
				}
				string tag = QuestNode_QuestUnique.GetProcessedTag("PeaceTalks", faction);
				if (Find.QuestManager.questsInDisplayOrder.Any((Quest q) => q.tags.Contains(tag)))
				{
					return false;
				}
			}
			if (leaderMustBeSafe.GetValue(slate) && (faction.leader == null || faction.leader.Spawned || faction.leader.IsPrisoner))
			{
				return false;
			}
			Thing value2 = mustBeHostileToFactionOf.GetValue(slate);
			if (value2 != null && value2.Faction != null && (value2.Faction == faction || !faction.HostileTo(value2.Faction)))
			{
				return false;
			}
			return true;
		}
		protected bool PeaceTalksExist(Faction faction)
		{
			List<PeaceTalks> peaceTalks = Find.WorldObjects.PeaceTalks;
			for (int i = 0; i < peaceTalks.Count; i++)
			{
				if (peaceTalks[i].Faction == faction)
				{
					return true;
				}
			}
			return false;
		}
	}

}
