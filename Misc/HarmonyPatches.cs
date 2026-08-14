using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;


namespace LoGiQ
{
    [StaticConstructorOnStartup]
    public static class PatchInitiator
    {
        static PatchInitiator()
        {
            Harmony h = new Harmony("rimworld.mod.Dimman.LoGiQ");
            h.PatchAll();
        }
    }

    [HarmonyPatch(typeof(MainTabWindow_Quests))]
    [HarmonyPatch("DoDescription")]
    class MainTabWindow_Quests_DoDescription_Patch
    {
        private static void Postfix(ref Rect innerRect, ref float curY)
        {
            QuestGoalsPanelMaker.makePanel(innerRect, ref curY);
        }
    }

    [HarmonyPatch(typeof(MainTabWindow_Quests))]
    [HarmonyPatch("Select")]
    class MainTabWindow_Quests_Select_Patch
    {
        private static void Postfix(Quest quest)
        {
            //if (quest == null)
            //     Log.Message($"unselect quest"); 
            //else Log.Message($"selected quest: {quest.name}");
            QuestGoalsPanelMaker.Select(quest);
        }
    }
}
