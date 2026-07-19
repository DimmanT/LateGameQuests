using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace LoGiQ
{
    public class ModSettings : Verse.ModSettings
    {
        private Vector2 buttonPosition = Vector2.zero;
        private float viewHeight = 0;
        public int ChoosenRate { get; private set; } = 3;
        private string ChoosenRateStr;
        public void DoSettingsWindowContents(Rect inRect)
        {
            var NAMES = new List<string> { "Never".Translate(), "LoGiQ_VeryRarely".Translate(), "LoGiQ_Rarely".Translate(), "LoGiQ_Normal".Translate(), "LoGiQ_Often".Translate(), "LoGiQ_VeryOften".Translate() };
            Rect viewRect = new Rect(0,0,inRect.width - 16, viewHeight);
            Widgets.BeginScrollView(inRect, ref buttonPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.ColumnWidth = inRect.width;
            Rect listingRect = inRect.AtZero();
            listingRect.height = 1024;
            listing.Begin(listingRect);
            listing.Label("LoGiQSettings_RateButtonText".Translate());
            if (ChoosenRateStr.NullOrEmpty())
                ChoosenRateStr = NAMES[ChoosenRate];
            if (listing.ButtonText(ChoosenRateStr))
            {
                var rates = new List<FloatMenuOption>();
                rates.Add(new FloatMenuOption(NAMES[0], () => { ChoosenRate = 0; ChoosenRateStr = NAMES[ChoosenRate]; Log.Message(ChoosenRateStr);}));
                rates.Add(new FloatMenuOption(NAMES[1], () => { ChoosenRate = 1; ChoosenRateStr = NAMES[ChoosenRate]; Log.Message(ChoosenRateStr);}));
                rates.Add(new FloatMenuOption(NAMES[2], () => { ChoosenRate = 2; ChoosenRateStr = NAMES[ChoosenRate]; Log.Message(ChoosenRateStr);}));
                rates.Add(new FloatMenuOption(NAMES[3], () => { ChoosenRate = 3; ChoosenRateStr = NAMES[ChoosenRate]; Log.Message(ChoosenRateStr);}));
                rates.Add(new FloatMenuOption(NAMES[4], () => { ChoosenRate = 4; ChoosenRateStr = NAMES[ChoosenRate]; Log.Message(ChoosenRateStr);}));
                rates.Add(new FloatMenuOption(NAMES[5], () => { ChoosenRate = 5; ChoosenRateStr = NAMES[ChoosenRate]; Log.Message(ChoosenRateStr); }));
                Find.WindowStack.Add(new FloatMenu(rates));
            }
            listing.End();

            viewHeight = listing.CurHeight;
            Widgets.EndScrollView();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            var tmp = ChoosenRate;
            Scribe_Values.Look(ref tmp, "ChoosenRate",3);
            ChoosenRate = tmp;
        }

    }
    public class Mod : Verse.Mod
    {
        public static ModSettings Settings {get; private set;}
        public Mod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ModSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return "Late Game Quests";
        }
    }

}
