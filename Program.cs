using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness
{
    internal static class Menu
    {
        public static MenuItemSettings ItemPanel = new MenuItemSettings();
        public static MenuItemSettings AutoLevler = new MenuItemSettings(typeof (AutoLevler)); //Only priority works

        public static MenuItemSettings UiTracker = new MenuItemSettings(typeof (UiTracker));
            //Works but need many improvements

        public static MenuItemSettings UimTracker = new MenuItemSettings(typeof (UimTracker));
            //Works but need many improvements

        public static MenuItemSettings SsCaller = new MenuItemSettings(typeof (SsCaller)); //Works
        public static MenuItemSettings Tracker = new MenuItemSettings();
        public static MenuItemSettings WaypointTracker = new MenuItemSettings(typeof (WaypointTracker)); //Works
        public static MenuItemSettings CloneTracker = new MenuItemSettings(typeof (CloneTracker)); //Works
        public static MenuItemSettings Timers = new MenuItemSettings(typeof (Timers));
        public static MenuItemSettings JungleTimer = new MenuItemSettings(); //Works
        public static MenuItemSettings RelictTimer = new MenuItemSettings(); //Works
        public static MenuItemSettings HealthTimer = new MenuItemSettings(); //Works
        public static MenuItemSettings InhibitorTimer = new MenuItemSettings(); //Works
        public static MenuItemSettings SummonerTimer = new MenuItemSettings(); //Works
        public static MenuItemSettings Health = new MenuItemSettings(typeof (Health));
        public static MenuItemSettings TowerHealth = new MenuItemSettings(); //Missing HPBarPos
        public static MenuItemSettings InhibitorHealth = new MenuItemSettings(); //Works

        public static MenuItemSettings DestinationTracker = new MenuItemSettings(typeof (DestinationTracker));
            //Work & Needs testing

        public static MenuItemSettings Detector = new MenuItemSettings();

        public static MenuItemSettings VisionDetector = new MenuItemSettings(typeof (HiddenObject));
            //Works - OnProcessSpell bugged

        public static MenuItemSettings RecallDetector = new MenuItemSettings(typeof (RecallDetector)); //Works

        public static MenuItemSettings Range = new MenuItemSettings(typeof (Ranges));
            //Many ranges are bugged. Waiting for SpellLib

        public static MenuItemSettings TowerRange = new MenuItemSettings();
        public static MenuItemSettings ShopRange = new MenuItemSettings();
        public static MenuItemSettings VisionRange = new MenuItemSettings();
        public static MenuItemSettings ExperienceRange = new MenuItemSettings();
        public static MenuItemSettings AttackRange = new MenuItemSettings();
        public static MenuItemSettings SpellQRange = new MenuItemSettings();
        public static MenuItemSettings SpellWRange = new MenuItemSettings();
        public static MenuItemSettings SpellERange = new MenuItemSettings();
        public static MenuItemSettings SpellRRange = new MenuItemSettings();
        public static MenuItemSettings ImmuneTimer = new MenuItemSettings(typeof (ImmuneTimer)); //Works
        public static MenuItemSettings Ganks = new MenuItemSettings();
        public static MenuItemSettings GankTracker = new MenuItemSettings(typeof (GankPotentialTracker)); //Works
        public static MenuItemSettings GankDetector = new MenuItemSettings(typeof (GankDetector)); //Works
        public static MenuItemSettings AltarTimer = new MenuItemSettings();
        public static MenuItemSettings Wards = new MenuItemSettings();
        public static MenuItemSettings WardCorrector = new MenuItemSettings(typeof (WardCorrector)); //Works
        public static MenuItemSettings BushRevealer = new MenuItemSettings(typeof (BushRevealer)); //Works        
        public static MenuItemSettings InvisibleRevealer = new MenuItemSettings(typeof (InvisibleRevealer)); //Works   
        public static MenuItemSettings SkinChanger = new MenuItemSettings(typeof (SkinChanger)); //Works
        public static MenuItemSettings AutoSmite = new MenuItemSettings(typeof (AutoSmite)); //Works
        public static MenuItemSettings AutoPot = new MenuItemSettings(typeof (AutoPot));
        public static MenuItemSettings SafeMovement = new MenuItemSettings(typeof (SafeMovement));
        public static MenuItemSettings AutoShield = new MenuItemSettings(typeof (AutoShield));
        public static MenuItemSettings AutoShieldBlockableSpells = new MenuItemSettings();
        public static MenuItemSettings Misc = new MenuItemSettings();
        public static MenuItemSettings MoveToMouse = new MenuItemSettings(typeof (MoveToMouse));
        public static MenuItemSettings SurrenderVote = new MenuItemSettings(typeof (SurrenderVote));
        public static MenuItemSettings AutoLatern = new MenuItemSettings(typeof (AutoLatern));
        public static MenuItemSettings DisconnectDetector = new MenuItemSettings(typeof (DisconnectDetector));
        public static MenuItemSettings AutoJump = new MenuItemSettings(typeof (AutoJump));
        public static MenuItemSettings TurnAround = new MenuItemSettings(typeof (TurnAround));
        public static MenuItemSettings MinionBars = new MenuItemSettings(typeof(MinionBars));
        public static MenuItemSettings MinionLocation = new MenuItemSettings(typeof(MinionLocation));
        public static MenuItemSettings FlashJuke = new MenuItemSettings(typeof(FlashJuke));
        public static MenuItemSettings Activator = new MenuItemSettings(typeof (Activator));
        public static MenuItemSettings ActivatorAutoSummonerSpell = new MenuItemSettings();
        public static MenuItemSettings ActivatorAutoSummonerSpellIgnite = new MenuItemSettings();
        public static MenuItemSettings ActivatorAutoSummonerSpellHeal = new MenuItemSettings();
        public static MenuItemSettings ActivatorAutoSummonerSpellBarrier = new MenuItemSettings();
        public static MenuItemSettings ActivatorAutoSummonerSpellExhaust = new MenuItemSettings();
        public static MenuItemSettings ActivatorAutoSummonerSpellCleanse = new MenuItemSettings();
        public static MenuItemSettings ActivatorOffensive = new MenuItemSettings();
        public static MenuItemSettings ActivatorOffensiveAd = new MenuItemSettings();
        public static MenuItemSettings ActivatorOffensiveAp = new MenuItemSettings();
        public static MenuItemSettings ActivatorDefensive = new MenuItemSettings();
        public static MenuItemSettings ActivatorDefensiveCleanseConfig = new MenuItemSettings();
        public static MenuItemSettings ActivatorDefensiveSelfShield = new MenuItemSettings();
        public static MenuItemSettings ActivatorDefensiveWoogletZhonya = new MenuItemSettings();
        public static MenuItemSettings ActivatorDefensiveDebuffSlow = new MenuItemSettings();
        public static MenuItemSettings ActivatorDefensiveCleanseSelf = new MenuItemSettings();
        public static MenuItemSettings ActivatorDefensiveShieldBoost = new MenuItemSettings();
        public static MenuItemSettings ActivatorDefensiveMikaelCleanse = new MenuItemSettings();
        public static MenuItemSettings ActivatorMisc = new MenuItemSettings();
        public static MenuItemSettings ActivatorAutoHeal = new MenuItemSettings(typeof(AutoHeal));
        public static MenuItemSettings ActivatorAutoUlt = new MenuItemSettings(typeof(AutoUlt));
        public static MenuItemSettings ActivatorAutoQss = new MenuItemSettings(typeof(AutoQSS));
        public static MenuItemSettings ActivatorAutoQssConfig = new MenuItemSettings(typeof(AutoQSS));
        public static MenuItemSettings Killable = new MenuItemSettings(typeof (Killable));
        public static MenuItemSettings EasyRangedJungle = new MenuItemSettings(typeof(EasyRangedJungle));
        public static MenuItemSettings FowWardPlacement = new MenuItemSettings(typeof(FowWardPlacement));
        public static MenuItemSettings RealTime = new MenuItemSettings(typeof(RealTime));

        public static MenuItemSettings GlobalSettings = new MenuItemSettings();

        public class MenuItemSettings
        {
            public bool ForceDisable;
            public dynamic Item;
            public LeagueSharp.Common.Menu Menu;
            public List<MenuItem> MenuItems = new List<MenuItem>();
            public String Name;
            public List<MenuItemSettings> SubMenus = new List<MenuItemSettings>();
            public Type Type;

            public MenuItemSettings(Type type, dynamic item)
            {
                Type = type;
                Item = item;
            }

            public MenuItemSettings(dynamic item)
            {
                Item = item;
            }

            public MenuItemSettings(Type type)
            {
                Type = type;
            }

            public MenuItemSettings(String name)
            {
                Name = name;
            }

            public MenuItemSettings()
            {
            }

            public MenuItemSettings AddMenuItemSettings(String displayName, String name)
            {
                SubMenus.Add(new MenuItemSettings(name));
                MenuItemSettings tempSettings = GetMenuSettings(name);
                if (tempSettings == null)
                {
                    throw new NullReferenceException(name + " not found");
                }
                tempSettings.Menu = Menu.AddSubMenu(new LeagueSharp.Common.Menu(displayName, name));
                return tempSettings;
            }

            public bool GetActive()
            {
                if (Menu == null)
                    return false;
                foreach (MenuItem item in Menu.Items)
                {
                    if (item.DisplayName == "Active")
                    {
                        if (item.GetValue<bool>())
                        {
                            return true;
                        }
                        return false;
                    }
                }
                return false;
            }

            public void SetActive(bool active)
            {
                if (Menu == null)
                    return;
                foreach (MenuItem item in Menu.Items)
                {
                    if (item.DisplayName == "Active")
                    {
                        item.SetValue(active);
                        return;
                    }
                }
            }

            public MenuItem GetMenuItem(String menuName)
            {
                if (Menu == null)
                    return null;
                foreach (MenuItem item in Menu.Items)
                {
                    if (item.Name == menuName)
                    {
                        return item;
                    }
                }
                return null;
            }

            public LeagueSharp.Common.Menu GetSubMenu(String menuName)
            {
                if (Menu == null)
                    return null;
                return Menu.SubMenu(menuName);
            }

            public MenuItemSettings GetMenuSettings(String name)
            {
                foreach (MenuItemSettings menu in SubMenus)
                {
                    if (menu.Name.Contains(name))
                        return menu;
                }
                return null;
            }
        }

        //public static MenuItemSettings  = new MenuItemSettings();
    }

    internal class Program
    {
        private static float lastDebugTime = 0;

        private static void Main(string[] args)
        {
            try
            {
                //SUpdater.UpdateCheck();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void CreateMenu()
        {
            //http://www.cambiaresearch.com/articles/15/javascript-char-codes-key-codes
            try
            {
                Menu.MenuItemSettings tempSettings;
                var menu = new LeagueSharp.Common.Menu("SAwareness", "SAwareness", true);

                //Not crashing
                Menu.Timers.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Timers", "SAwarenessTimers"));
                Menu.Timers.MenuItems.Add(
                    Menu.Timers.Menu.AddItem(
                        new MenuItem("SAwarenessTimersPingTimes", "Ping Times").SetValue(new Slider(0, 5, 0))));
                Menu.Timers.MenuItems.Add(
                    Menu.Timers.Menu.AddItem(
                        new MenuItem("SAwarenessTimersRemindTime", "Remind Time").SetValue(new Slider(0, 50, 0))));
                Menu.Timers.MenuItems.Add(
                    Menu.Timers.Menu.AddItem(new MenuItem("SAwarenessTimersLocalPing", "Local Ping").SetValue(true)));
                Menu.Timers.MenuItems.Add(
                    Menu.Timers.Menu.AddItem(
                        new MenuItem("SAwarenessTimersChatChoice", "Chat Choice").SetValue(
                            new StringList(new[] { "None", "Local", "Server" }))));
                Menu.JungleTimer.Menu =
                    Menu.Timers.Menu.AddSubMenu(new LeagueSharp.Common.Menu("JungleTimer", "SAwarenessJungleTimer"));
                Menu.JungleTimer.MenuItems.Add(
                    Menu.JungleTimer.Menu.AddItem(new MenuItem("SAwarenessJungleTimersActive", "Active").SetValue(false)));
                Menu.RelictTimer.Menu =
                    Menu.Timers.Menu.AddSubMenu(new LeagueSharp.Common.Menu("RelictTimer", "SAwarenessRelictTimer"));
                Menu.RelictTimer.MenuItems.Add(
                    Menu.RelictTimer.Menu.AddItem(new MenuItem("SAwarenessRelictTimersActive", "Active").SetValue(false)));
                Menu.HealthTimer.Menu =
                    Menu.Timers.Menu.AddSubMenu(new LeagueSharp.Common.Menu("HealthTimer", "SAwarenessHealthTimer"));
                Menu.HealthTimer.MenuItems.Add(
                    Menu.HealthTimer.Menu.AddItem(new MenuItem("SAwarenessHealthTimersActive", "Active").SetValue(false)));
                Menu.InhibitorTimer.Menu =
                    Menu.Timers.Menu.AddSubMenu(new LeagueSharp.Common.Menu("InhibitorTimer", "SAwarenessInhibitorTimer"));
                Menu.InhibitorTimer.MenuItems.Add(
                    Menu.InhibitorTimer.Menu.AddItem(
                        new MenuItem("SAwarenessInhibitorTimersActive", "Active").SetValue(false)));
                Menu.AltarTimer.Menu =
                    Menu.Timers.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AltarTimer", "SAwarenessAltarTimer"));
                Menu.AltarTimer.MenuItems.Add(
                    Menu.AltarTimer.Menu.AddItem(new MenuItem("SAwarenessAltarTimersActive", "Active").SetValue(false)));
                Menu.ImmuneTimer.Menu =
                    Menu.Timers.Menu.AddSubMenu(new LeagueSharp.Common.Menu("ImmuneTimer", "SAwarenessImmuneTimer"));
                Menu.ImmuneTimer.MenuItems.Add(
                    Menu.ImmuneTimer.Menu.AddItem(new MenuItem("SAwarenessImmuneTimersActive", "Active").SetValue(false)));
                Menu.SummonerTimer.Menu =
                    Menu.Timers.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SummonerTimer", "SAwarenessSummonerTimer"));
                Menu.SummonerTimer.MenuItems.Add(
                    Menu.SummonerTimer.Menu.AddItem(new MenuItem("SAwarenessSummonerTimersActive", "Active").SetValue(false)));
                Menu.Timers.MenuItems.Add(
                    Menu.Timers.Menu.AddItem(new MenuItem("SAwarenessTimersActive", "Active").SetValue(false)));

                //Not crashing
                Menu.Range.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Ranges", "SAwarenessRanges"));
                Menu.ShopRange.Menu =
                    Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("ShopRange",
                        "SAwarenessShopRange"));
                Menu.ShopRange.MenuItems.Add(
                    Menu.ShopRange.Menu.AddItem(
                        new MenuItem("SAwarenessShopRangeMode", "Mode").SetValue(
                            new StringList(new[] { "Me", "Enemy", "Both" }))));
                Menu.ShopRange.MenuItems.Add(
                    Menu.ShopRange.Menu.AddItem(
                        new MenuItem("SAwarenessShopRangeActive", "Active").SetValue(false)));
                Menu.VisionRange.Menu =
                    Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("VisionRange",
                        "SAwarenessVisionRange"));
                Menu.VisionRange.MenuItems.Add(
                    Menu.VisionRange.Menu.AddItem(
                        new MenuItem("SAwarenessVisionRangeMode", "Mode").SetValue(
                            new StringList(new[] { "Me", "Enemy", "Both" }))));
                Menu.VisionRange.MenuItems.Add(
                    Menu.VisionRange.Menu.AddItem(
                        new MenuItem("SAwarenessVisionRangeActive", "Active").SetValue(false)));
                Menu.ExperienceRange.Menu =
                    Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("ExperienceRange",
                        "SAwarenessExperienceRange"));
                Menu.ExperienceRange.MenuItems.Add(
                    Menu.ExperienceRange.Menu.AddItem(
                        new MenuItem("SAwarenessExperienceRangeMode", "Mode").SetValue(
                            new StringList(new[] { "Me", "Enemy", "Both" }))));
                Menu.ExperienceRange.MenuItems.Add(
                    Menu.ExperienceRange.Menu.AddItem(
                        new MenuItem("SAwarenessExperienceRangeActive", "Active").SetValue(false)));
                Menu.AttackRange.Menu =
                    Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AttackRange", "SAwarenessAttackRange"));
                Menu.AttackRange.MenuItems.Add(
                    Menu.AttackRange.Menu.AddItem(
                        new MenuItem("SAwarenessAttackRangeMode", "Mode").SetValue(
                            new StringList(new[] { "Me", "Enemy", "Both" }))));
                Menu.AttackRange.MenuItems.Add(
                    Menu.AttackRange.Menu.AddItem(new MenuItem("SAwarenessAttackRangeActive", "Active").SetValue(false)));
                Menu.TowerRange.Menu =
                    Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("TowerRange", "SAwarenessTowerRange"));
                Menu.TowerRange.MenuItems.Add(
                    Menu.TowerRange.Menu.AddItem(
                        new MenuItem("SAwarenessTowerRangeMode", "Mode").SetValue(
                            new StringList(new[] { "Me", "Enemy", "Both" }))));
                Menu.TowerRange.MenuItems.Add(
                    Menu.TowerRange.Menu.AddItem(new MenuItem("SAwarenessTowerRangeRange", "Range").SetValue(new Slider(2000, 10000,
                            0))));
                Menu.TowerRange.MenuItems.Add(
                    Menu.TowerRange.Menu.AddItem(new MenuItem("SAwarenessTowerRangeActive", "Active").SetValue(false)));
                Menu.SpellQRange.Menu =
                    Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SpellQRange", "SAwarenessSpellQRange"));
                Menu.SpellQRange.MenuItems.Add(
                    Menu.SpellQRange.Menu.AddItem(
                        new MenuItem("SAwarenessSpellQRangeMode", "Mode").SetValue(
                            new StringList(new[] { "Me", "Enemy", "Both" }))));
                Menu.SpellQRange.MenuItems.Add(
                    Menu.SpellQRange.Menu.AddItem(new MenuItem("SAwarenessSpellQRangeActive", "Active").SetValue(false)));
                Menu.SpellWRange.Menu =
                    Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SpellWRange", "SAwarenessSpellWRange"));
                Menu.SpellWRange.MenuItems.Add(
                    Menu.SpellWRange.Menu.AddItem(
                        new MenuItem("SAwarenessSpellWRangeMode", "Mode").SetValue(
                            new StringList(new[] { "Me", "Enemy", "Both" }))));
                Menu.SpellWRange.MenuItems.Add(
                    Menu.SpellWRange.Menu.AddItem(new MenuItem("SAwarenessSpellWRangeActive", "Active").SetValue(false)));
                Menu.SpellERange.Menu =
                    Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SpellERange", "SAwarenessSpellERange"));
                Menu.SpellERange.MenuItems.Add(
                    Menu.SpellERange.Menu.AddItem(
                        new MenuItem("SAwarenessSpellERangeMode", "Mode").SetValue(
                            new StringList(new[] { "Me", "Enemy", "Both" }))));
                Menu.SpellERange.MenuItems.Add(
                    Menu.SpellERange.Menu.AddItem(new MenuItem("SAwarenessSpellERangeActive", "Active").SetValue(false)));
                Menu.SpellRRange.Menu =
                    Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SpellRRange", "SAwarenessSpellRRange"));
                Menu.SpellRRange.MenuItems.Add(
                    Menu.SpellRRange.Menu.AddItem(
                        new MenuItem("SAwarenessSpellRRangeMode", "Mode").SetValue(
                            new StringList(new[] { "Me", "Enemy", "Both" }))));
                Menu.SpellRRange.MenuItems.Add(
                    Menu.SpellRRange.Menu.AddItem(new MenuItem("SAwarenessSpellRRangeActive", "Active").SetValue(false)));
                Menu.Range.MenuItems.Add(
                    Menu.Range.Menu.AddItem(new MenuItem("SAwarenessRangesActive", "Active").SetValue(false)));

                //Not crashing
                Menu.Tracker.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Tracker", "SAwarenessTracker"));
                Menu.WaypointTracker.Menu =
                    Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("WaypointTracker",
                        "SAwarenessWaypointTracker"));
                Menu.WaypointTracker.MenuItems.Add(
                    Menu.WaypointTracker.Menu.AddItem(
                        new MenuItem("SAwarenessWaypointTrackerActive", "Active").SetValue(false)));
                Menu.DestinationTracker.Menu =
                    Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("DestinationTracker",
                        "SAwarenessDestinationTracker"));
                Menu.DestinationTracker.MenuItems.Add(
                    Menu.DestinationTracker.Menu.AddItem(
                        new MenuItem("SAwarenessDestinationTrackerActive", "Active").SetValue(false)));
                Menu.CloneTracker.Menu =
                    Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("CloneTracker", "SAwarenessCloneTracker"));
                Menu.CloneTracker.MenuItems.Add(
                    Menu.CloneTracker.Menu.AddItem(new MenuItem("SAwarenessCloneTrackerActive", "Active").SetValue(false)));
                Menu.UiTracker.Menu =
                    Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("UITracker", "SAwarenessUITracker"));
                Menu.UiTracker.MenuItems.Add(
                    Menu.UiTracker.Menu.AddItem(new MenuItem("SAwarenessItemPanelActive", "ItemPanel").SetValue(false)));
                Menu.UiTracker.MenuItems.Add(
                    Menu.UiTracker.Menu.AddItem(
                        new MenuItem("SAwarenessUITrackerScale", "Scale").SetValue(new Slider(100, 100, 0))));
                tempSettings = Menu.UiTracker.AddMenuItemSettings("Enemy Tracker",
                    "SAwarenessUITrackerEnemyTracker");
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessUITrackerEnemyTrackerXPos", "X Position").SetValue(new Slider(-1, 10000,
                            0))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessUITrackerEnemyTrackerYPos", "Y Position").SetValue(new Slider(-1, 10000,
                            0))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessUITrackerEnemyTrackerMode", "Mode").SetValue(
                            new StringList(new[] { "Side", "Unit", "Both" }))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessUITrackerEnemyTrackerSideDisplayMode", "Side Display").SetValue(
                            new StringList(new[] { "Default", "Simple", "League" }))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessUITrackerEnemyTrackerHeadMode", "Head Mode").SetValue(
                            new StringList(new[] { "Small", "Big" }))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessUITrackerEnemyTrackerHeadDisplayMode", "Head Display").SetValue(
                            new StringList(new[] { "Default", "Simple" }))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessUITrackerEnemyTrackerActive", "Active").SetValue(false)));
                tempSettings = Menu.UiTracker.AddMenuItemSettings("Ally Tracker",
                    "SAwarenessUITrackerAllyTracker");
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessUITrackerAllyTrackerXPos", "X Position").SetValue(new Slider(-1, 10000,
                            0))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessUITrackerAllyTrackerYPos", "Y Position").SetValue(new Slider(-1, 10000,
                            0))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessUITrackerAllyTrackerMode", "Mode").SetValue(
                            new StringList(new[] { "Side", "Unit", "Both" }))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessUITrackerAllyTrackerSideDisplayMode", "Side Display").SetValue(
                            new StringList(new[] { "Default", "Simple", "League" }))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessUITrackerAllyTrackerHeadMode", "Over Head Mode").SetValue(
                            new StringList(new[] { "Small", "Big" }))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessUITrackerAllyTrackerHeadDisplayMode", "Over Head Display").SetValue
                            (new StringList(new[] { "Default", "Simple" }))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessUITrackerAllyTrackerActive", "Active").SetValue(false)));
                //Menu.UiTracker.MenuItems.Add(Menu.UiTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerCameraMoveActive", "Camera move active").SetValue(false)));
                Menu.UiTracker.MenuItems.Add(
                    Menu.UiTracker.Menu.AddItem(
                        new MenuItem("SAwarenessUITrackerPingActive", "Ping active").SetValue(false)));
                Menu.UiTracker.MenuItems.Add(
                    Menu.UiTracker.Menu.AddItem(new MenuItem("SAwarenessUITrackerActive", "Active").SetValue(false)));
                Menu.UimTracker.Menu =
                    Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("UIMTracker", "SAwarenessUIMTracker"));
                Menu.UimTracker.MenuItems.Add(
                    Menu.UimTracker.Menu.AddItem(
                        new MenuItem("SAwarenessUIMTrackerScale", "Scale").SetValue(new Slider(100, 100, 0))));
                Menu.UimTracker.MenuItems.Add(
                    Menu.UimTracker.Menu.AddItem(new MenuItem("SAwarenessUIMTrackerShowSS", "SS Time").SetValue(false)));
                Menu.UimTracker.MenuItems.Add(
                    Menu.UimTracker.Menu.AddItem(new MenuItem("SAwarenessUIMTrackerActive", "Active").SetValue(false)));
                Menu.SsCaller.Menu =
                    Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SSCaller", "SAwarenessSSCaller"));
                Menu.SsCaller.MenuItems.Add(
                    Menu.SsCaller.Menu.AddItem(
                        new MenuItem("SAwarenessSSCallerPingTimes", "Ping Times").SetValue(new Slider(0, 5, 0))));
                Menu.SsCaller.MenuItems.Add(
                    Menu.SsCaller.Menu.AddItem(
                        new MenuItem("SAwarenessSSCallerPingType", "Ping Type").SetValue(
                            new StringList(new[] { "Normal", "Danger", "EnemyMissing", "OnMyWay", "Fallback", "AssistMe" }))));
                Menu.SsCaller.MenuItems.Add(
                    Menu.SsCaller.Menu.AddItem(new MenuItem("SAwarenessSSCallerLocalPing", "Local Ping").SetValue(false)));
                Menu.SsCaller.MenuItems.Add(
                    Menu.SsCaller.Menu.AddItem(
                        new MenuItem("SAwarenessSSCallerChatChoice", "Chat Choice").SetValue(
                            new StringList(new[] { "None", "Local", "Server" }))));
                Menu.SsCaller.MenuItems.Add(
                    Menu.SsCaller.Menu.AddItem(
                        new MenuItem("SAwarenessSSCallerDisableTime", "Disable Time").SetValue(new Slider(20, 180, 1))));
                Menu.SsCaller.MenuItems.Add(
                    Menu.SsCaller.Menu.AddItem(new MenuItem("SAwarenessSSCallerActive", "Active").SetValue(false)));
                Menu.Killable.Menu =
                    Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Killable", "SAwarenessKillable"));
                Menu.Killable.MenuItems.Add(
                    Menu.Killable.Menu.AddItem(new MenuItem("SAwarenessKillableActive", "Active").SetValue(false)));
                Menu.Tracker.MenuItems.Add(
                    Menu.Tracker.Menu.AddItem(new MenuItem("SAwarenessTrackerActive", "Active").SetValue(false)));

                //Not crashing
                Menu.Detector.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Detector", "SAwarenessDetector"));
                Menu.VisionDetector.Menu =
                    Menu.Detector.Menu.AddSubMenu(new LeagueSharp.Common.Menu("VisionDetector",
                        "SAwarenessVisionDetector"));
                Menu.VisionDetector.MenuItems.Add(
                    Menu.VisionDetector.Menu.AddItem(
                        new MenuItem("SAwarenessVisionDetectorDrawRange", "Draw Range").SetValue(false)));
                Menu.VisionDetector.MenuItems.Add(
                    Menu.VisionDetector.Menu.AddItem(
                        new MenuItem("SAwarenessVisionDetectorActive", "Active").SetValue(false)));
                Menu.RecallDetector.Menu =
                    Menu.Detector.Menu.AddSubMenu(new LeagueSharp.Common.Menu("RecallDetector",
                        "SAwarenessRecallDetector"));
                Menu.RecallDetector.MenuItems.Add(
                    Menu.RecallDetector.Menu.AddItem(
                        new MenuItem("SAwarenessRecallDetectorPingTimes", "Ping Times").SetValue(new Slider(0, 5, 0))));
                Menu.RecallDetector.MenuItems.Add(
                    Menu.RecallDetector.Menu.AddItem(
                        new MenuItem("SAwarenessRecallDetectorLocalPing", "Local Ping").SetValue(true)));
                Menu.RecallDetector.MenuItems.Add(
                    Menu.RecallDetector.Menu.AddItem(
                        new MenuItem("SAwarenessRecallDetectorChatChoice", "Chat Choice").SetValue(
                            new StringList(new[] { "None", "Local", "Server" }))));
                Menu.RecallDetector.MenuItems.Add(
                    Menu.RecallDetector.Menu.AddItem(
                        new MenuItem("SAwarenessRecallDetectorMode", "Mode").SetValue(
                            new StringList(new[] { "Chat", "CDTracker", "Both" }))));
                Menu.RecallDetector.MenuItems.Add(
                    Menu.RecallDetector.Menu.AddItem(
                        new MenuItem("SAwarenessRecallDetectorActive", "Active").SetValue(false)));
                Menu.Detector.MenuItems.Add(
                    Menu.Detector.Menu.AddItem(new MenuItem("SAwarenessDetectorActive", "Active").SetValue(false)));

                //Not crashing
                Menu.Ganks.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Ganks", "SAwarenessGanks"));
                Menu.GankTracker.Menu =
                    Menu.Ganks.Menu.AddSubMenu(new LeagueSharp.Common.Menu("GankTracker", "SAwarenessGankTracker"));
                Menu.GankTracker.MenuItems.Add(
                    Menu.GankTracker.Menu.AddItem(
                        new MenuItem("SAwarenessGankTrackerTrackRange", "Track Range").SetValue(new Slider(1, 20000, 1))));
                Menu.GankTracker.MenuItems.Add(
                   Menu.GankTracker.Menu.AddItem(new MenuItem("SAwarenessGankTrackerKillable", "Only Killable").SetValue(false)));
                Menu.GankTracker.MenuItems.Add(
                    Menu.GankTracker.Menu.AddItem(new MenuItem("SAwarenessGankTrackerActive", "Active").SetValue(false)));
                Menu.GankDetector.Menu =
                    Menu.Ganks.Menu.AddSubMenu(new LeagueSharp.Common.Menu("GankDetector", "SAwarenessGankDetector"));
                Menu.GankDetector.MenuItems.Add(
                    Menu.GankDetector.Menu.AddItem(
                        new MenuItem("SAwarenessGankDetectorPingTimes", "Ping Times").SetValue(new Slider(0, 5, 0))));
                Menu.GankDetector.MenuItems.Add(
                    Menu.GankDetector.Menu.AddItem(
                        new MenuItem("SAwarenessGankDetectorPingType", "Ping Type").SetValue(
                            new StringList(new[] { "Normal", "Danger", "EnemyMissing", "OnMyWay", "Fallback", "AssistMe" }))));
                Menu.GankDetector.MenuItems.Add(
                    Menu.GankDetector.Menu.AddItem(
                        new MenuItem("SAwarenessGankDetectorLocalPing", "Local Ping").SetValue(true)));
                Menu.GankDetector.MenuItems.Add(
                    Menu.GankDetector.Menu.AddItem(
                        new MenuItem("SAwarenessGankDetectorChatChoice", "Chat Choice").SetValue(
                            new StringList(new[] { "None", "Local", "Server" }))));
                Menu.GankDetector.MenuItems.Add(
                    Menu.GankDetector.Menu.AddItem(
                        new MenuItem("SAwarenessGankDetectorTrackRangeMin", "Track Range Min").SetValue(new Slider(1, 10000, 1))));
                Menu.GankDetector.MenuItems.Add(
                    Menu.GankDetector.Menu.AddItem(
                        new MenuItem("SAwarenessGankDetectorTrackRangeMax", "Track Range Max").SetValue(new Slider(1, 10000, 1))));
                Menu.GankDetector.MenuItems.Add(
                    Menu.GankDetector.Menu.AddItem(new MenuItem("SAwarenessGankDetectorShowJungler", "Show Jungler").SetValue(false)));
                Menu.GankDetector.MenuItems.Add(
                    Menu.GankDetector.Menu.AddItem(new MenuItem("SAwarenessGankDetectorActive", "Active").SetValue(false)));
                Menu.Ganks.MenuItems.Add(
                    Menu.Ganks.Menu.AddItem(new MenuItem("SAwarenessGanksActive", "Active").SetValue(false)));

                //Not crashing
                Menu.Health.Menu =
                    menu.AddSubMenu(new LeagueSharp.Common.Menu("Object Health", "SAwarenessObjectHealth"));
                Menu.TowerHealth.Menu =
                    Menu.Health.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Tower Health", "SAwarenessTowerHealth"));
                Menu.TowerHealth.MenuItems.Add(
                    Menu.TowerHealth.Menu.AddItem(new MenuItem("SAwarenessTowerHealthActive", "Active").SetValue(false)));
                Menu.InhibitorHealth.Menu =
                    Menu.Health.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Inhibitor Health",
                        "SAwarenessInhibitorHealth"));
                Menu.InhibitorHealth.MenuItems.Add(
                    Menu.InhibitorHealth.Menu.AddItem(
                        new MenuItem("SAwarenessInhibitorHealthActive", "Active").SetValue(false)));
                Menu.Health.MenuItems.Add(
                    Menu.Health.Menu.AddItem(
                        new MenuItem("SAwarenessHealthMode", "Mode").SetValue(new StringList(new[] { "Percent", "Normal" }))));
                Menu.Health.MenuItems.Add(
                    Menu.Health.Menu.AddItem(new MenuItem("SAwarenessHealthActive", "Active").SetValue(false)));

                //Not crashing
                Menu.Wards.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Wards", "SAwarenessWards"));
                Menu.WardCorrector.Menu =
                    Menu.Wards.Menu.AddSubMenu(new LeagueSharp.Common.Menu("WardCorrector", "SAwarenessWardCorrector"));
                Menu.WardCorrector.MenuItems.Add(
                    Menu.WardCorrector.Menu.AddItem(
                        new MenuItem("SAwarenessWardCorrectorKey", "Trinket Key").SetValue(new KeyBind(52, KeyBindType.Press))));
                Menu.WardCorrector.MenuItems.Add(
                    Menu.WardCorrector.Menu.AddItem(
                        new MenuItem("SAwarenessWardCorrectorActive", "Active").SetValue(false)));
                Menu.BushRevealer.Menu =
                    Menu.Wards.Menu.AddSubMenu(new LeagueSharp.Common.Menu("BushRevealer", "SAwarenessBushRevealer"));
                Menu.BushRevealer.MenuItems.Add(
                    Menu.BushRevealer.Menu.AddItem(
                        new MenuItem("SAwarenessBushRevealerKey", "Key").SetValue(new KeyBind(32, KeyBindType.Press))));
                Menu.BushRevealer.MenuItems.Add(
                    Menu.BushRevealer.Menu.AddItem(new MenuItem("SAwarenessBushRevealerActive", "Active").SetValue(false)));
                Menu.BushRevealer.MenuItems.Add(
                    Menu.BushRevealer.Menu.AddItem(new MenuItem("By Beaving & Blm95", "By Beaving & Blm95")));
                Menu.InvisibleRevealer.Menu =
                    Menu.Wards.Menu.AddSubMenu(new LeagueSharp.Common.Menu("InvisibleRevealer",
                        "SAwarenessInvisibleRevealer"));
                Menu.InvisibleRevealer.MenuItems.Add(
                    Menu.InvisibleRevealer.Menu.AddItem(
                        new MenuItem("SAwarenessInvisibleRevealerMode", "Mode").SetValue(
                            new StringList(new[] { "Manual", "Automatic" }))));
                Menu.InvisibleRevealer.MenuItems.Add(
                    Menu.InvisibleRevealer.Menu.AddItem(
                        new MenuItem("SAwarenessInvisibleRevealerKey", "Key").SetValue(new KeyBind(32, KeyBindType.Press))));
                Menu.InvisibleRevealer.MenuItems.Add(
                    Menu.InvisibleRevealer.Menu.AddItem(
                        new MenuItem("SAwarenessInvisibleRevealerActive", "Active").SetValue(false)));
                Menu.Wards.MenuItems.Add(
                    Menu.Wards.Menu.AddItem(new MenuItem("SAwarenessWardsActive", "Active").SetValue(false)));

                Menu.Activator.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Activator", "SAwarenessActivator"));
                Menu.ActivatorAutoSummonerSpell.Menu =
                    Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Auto Summoner Spells",
                        "SAwarenessActivatorAutoSummonerSpell"));
                Menu.ActivatorAutoSummonerSpell.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpell.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorActivatorAutoSummonerSpellActive", "Active").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellIgnite.Menu =
                    Menu.ActivatorAutoSummonerSpell.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Auto Ignite",
                        "SAwarenessActivatorAutoSummonerSpellIgnite"));
                Menu.ActivatorAutoSummonerSpellIgnite.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellIgnite.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellIgniteActive", "Active").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellHeal.Menu =
                    Menu.ActivatorAutoSummonerSpell.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Auto Heal",
                        "SAwarenessActivatorAutoSummonerSpellHeal"));
                Menu.ActivatorAutoSummonerSpellHeal.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellHeal.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellHealPercent", "Percent").SetValue(
                            new Slider(20, 100, 1))));
                Menu.ActivatorAutoSummonerSpellHeal.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellHeal.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellHealAllyActive", "Ally Active").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellHeal.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellHeal.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellHealActive", "Active").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellBarrier.Menu =
                    Menu.ActivatorAutoSummonerSpell.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Auto Barrier",
                        "SAwarenessActivatorAutoSummonerSpellBarrier"));
                Menu.ActivatorAutoSummonerSpellBarrier.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellBarrier.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellBarrierPercent", "Percent").SetValue(
                            new Slider(20, 100, 1))));
                Menu.ActivatorAutoSummonerSpellBarrier.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellBarrier.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellBarrierActive", "Active").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellExhaust.Menu =
                    Menu.ActivatorAutoSummonerSpell.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Auto Exhaust",
                        "SAwarenessActivatorAutoSummonerSpellExhaust"));
                Menu.ActivatorAutoSummonerSpellExhaust.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellExhaust.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellExhaustAutoCast", "AutoCast On Key").SetValue(
                            new KeyBind(32, KeyBindType.Press))));
                Menu.ActivatorAutoSummonerSpellExhaust.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellExhaust.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellExhaustMinEnemies", "Min Enemies").SetValue(
                            new Slider(3, 5, 1))));
                Menu.ActivatorAutoSummonerSpellExhaust.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellExhaust.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellExhaustAllyPercent", "Ally Percent").SetValue(
                            new Slider(20, 100, 1))));
                Menu.ActivatorAutoSummonerSpellExhaust.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellExhaust.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellExhaustSelfPercent", "Self Percent").SetValue(
                            new Slider(20, 100, 1))));
                Menu.ActivatorAutoSummonerSpellExhaust.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellExhaust.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellExhaustUseUltSpells", "Ult Spells").SetValue(
                            false)));
                Menu.ActivatorAutoSummonerSpellExhaust.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellExhaust.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellExhaustActive", "Active").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.Menu =
                    Menu.ActivatorAutoSummonerSpell.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Auto Cleanse",
                        "SAwarenessActivatorAutoSummonerSpellCleanse"));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseStun", "Stun").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseSilence", "Silence").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseTaunt", "Taunt").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseFear", "Fear").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseCharm", "Charm").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseBlind", "Blind").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseDisarm", "Disarm").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseSlow", "Slow").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseCombatDehancer", "Combat Dehancer")
                            .SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseSnare", "Snare").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellCleansePoison", "Posion").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseMinSpells", "Min Spells").SetValue(
                            new Slider(2, 10, 1))));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(
                    Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseActive", "Active").SetValue(false)));
                Menu.AutoSmite.Menu =
                    Menu.ActivatorAutoSummonerSpell.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoSmite",
                        "SAwarenessAutoSmite"));
                Menu.AutoSmite.MenuItems.Add(
                    Menu.AutoSmite.Menu.AddItem(
                        new MenuItem("SAwarenessAutoSmiteSmallCampsActive", "Smite Small Camps").SetValue(false)));
                Menu.AutoSmite.MenuItems.Add(
                    Menu.AutoSmite.Menu.AddItem(
                        new MenuItem("SAwarenessAutoSmiteAutoSpell", "Use Auto Spell").SetValue(false)));
                Menu.AutoSmite.MenuItems.Add(
                    Menu.AutoSmite.Menu.AddItem(
                        new MenuItem("SAwarenessAutoSmiteKeyActive", "Key").SetValue(new KeyBind(78, KeyBindType.Toggle))));
                Menu.AutoSmite.MenuItems.Add(
                    Menu.AutoSmite.Menu.AddItem(new MenuItem("SAwarenessAutoSmiteActive", "Active").SetValue(false)));

                Menu.ActivatorOffensive.Menu =
                    Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Offensive Items",
                        "SAwarenessActivatorOffensive"));
                Menu.ActivatorOffensiveAd.Menu =
                    Menu.ActivatorOffensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AD",
                        "SAwarenessActivatorOffensiveAd"));
                Menu.ActivatorOffensiveAd.MenuItems.Add(
                    Menu.ActivatorOffensiveAd.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorOffensiveAdBOTRK", "Blade of the Ruined King").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(
                    Menu.ActivatorOffensiveAd.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorOffensiveAdEntropy", "Entropy").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(
                    Menu.ActivatorOffensiveAd.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorOffensiveAdRavenousHydra", "Ravenous Hydra").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(
                    Menu.ActivatorOffensiveAd.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorOffensiveAdSwordOfTheDevine", "Sword Of The Devine").SetValue(
                            false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(
                    Menu.ActivatorOffensiveAd.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorOffensiveAdTiamat", "Tiamat").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(
                    Menu.ActivatorOffensiveAd.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorOffensiveAdYoumuusGhostblade", "Youmuu's Ghostblade").SetValue(
                            false)));
                //Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveAdMuramana", "Muramana").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(
                    Menu.ActivatorOffensiveAd.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorOffensiveAdActive", "Active").SetValue(false)));

                Menu.ActivatorOffensiveAp.Menu =
                    Menu.ActivatorOffensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AP",
                        "SAwarenessActivatorOffensiveAp"));
                Menu.ActivatorOffensiveAp.MenuItems.Add(
                    Menu.ActivatorOffensiveAp.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorOffensiveApBilgewaterCutlass", "Bilgewater Cutlass").SetValue(
                            false)));
                Menu.ActivatorOffensiveAp.MenuItems.Add(
                    Menu.ActivatorOffensiveAp.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorOffensiveApBlackfireTorch", "Blackfire Torch").SetValue(false)));
                Menu.ActivatorOffensiveAp.MenuItems.Add(
                    Menu.ActivatorOffensiveAp.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorOffensiveApDFG", "Deathfire Grasp").SetValue(false)));
                Menu.ActivatorOffensiveAp.MenuItems.Add(
                    Menu.ActivatorOffensiveAp.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorOffensiveApHextechGunblade", "Hextech Gunblade").SetValue(false)));
                Menu.ActivatorOffensiveAp.MenuItems.Add(
                    Menu.ActivatorOffensiveAp.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorOffensiveApTwinShadows", "Twin Shadows").SetValue(false)));
                //Menu.ActivatorOffensiveAp.MenuItems.Add(Menu.ActivatorOffensiveAp.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveApOdynsVeil", "Odyn's Veil").SetValue(false)));
                Menu.ActivatorOffensiveAp.MenuItems.Add(
                    Menu.ActivatorOffensiveAp.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorOffensiveApActive", "Active").SetValue(false)));
                Menu.ActivatorOffensive.MenuItems.Add(
                    Menu.ActivatorOffensive.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorOffensiveKey", "Key").SetValue(new KeyBind(32,
                            KeyBindType.Press))));
                Menu.ActivatorOffensive.MenuItems.Add(
                    Menu.ActivatorOffensive.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorOffensiveActive", "Active").SetValue(false)));

                Menu.ActivatorDefensive.Menu =
                    Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Defensive Items",
                        "SAwarenessActivatorDefensive"));
                Menu.ActivatorDefensiveCleanseConfig.Menu =
                    Menu.ActivatorDefensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Cleanse Config",
                        "SAwarenessActivatorDefensiveCleanseConfig"));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseConfigStun", "Stun").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseConfigSilence", "Silence").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseConfigTaunt", "Taunt").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseConfigFear", "Fear").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseConfigCharm", "Charm").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseConfigBlind", "Blind").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseConfigDisarm", "Disarm").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseConfigSuppress", "Suppress").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseConfigSlow", "Slow").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseConfigCombatDehancer", "Combat Dehancer")
                            .SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseConfigSnare", "Snare").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseConfigPoison", "Posion").SetValue(false)));
                //Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseConfigActive", "Active").SetValue(false)));

                Menu.ActivatorDefensiveSelfShield.Menu =
                    Menu.ActivatorDefensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu(
                        "Self Shield | Not implemented", "SAwarenessActivatorDefensiveSelfShield"));
                Menu.ActivatorDefensiveSelfShield.MenuItems.Add(
                    Menu.ActivatorDefensiveSelfShield.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveSelfShieldSeraphEmbrace", "Seraph Embrace").SetValue(
                            false)));
                Menu.ActivatorDefensiveSelfShield.MenuItems.Add(
                    Menu.ActivatorDefensiveSelfShield.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveSelfShieldOhmwrecker", "Ohmwrecker").SetValue(false)));
                Menu.ActivatorDefensiveSelfShield.MenuItems.Add(
                    Menu.ActivatorDefensiveSelfShield.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveSelfShieldActive", "Active").SetValue(false)));

                Menu.ActivatorDefensiveWoogletZhonya.Menu =
                    Menu.ActivatorDefensive.Menu.AddSubMenu(
                        new LeagueSharp.Common.Menu("Wooglet/Zhonya",
                            "SAwarenessActivatorDefensiveWoogletZhonya"));
                Menu.ActivatorDefensiveWoogletZhonya.MenuItems.Add(
                    Menu.ActivatorDefensiveWoogletZhonya.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveWoogletZhonyaWooglet", "Wooglet").SetValue(false)));
                Menu.ActivatorDefensiveWoogletZhonya.MenuItems.Add(
                    Menu.ActivatorDefensiveWoogletZhonya.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveWoogletZhonyaZhonya", "Zhonya").SetValue(false)));
                Menu.ActivatorDefensiveWoogletZhonya.MenuItems.Add(
                    Menu.ActivatorDefensiveWoogletZhonya.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveWoogletZhonyaActive", "Active").SetValue(false)));

                Menu.ActivatorDefensiveDebuffSlow.Menu =
                    Menu.ActivatorDefensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Slow Enemy",
                        "SAwarenessActivatorDefensiveDebuffSlow"));
                Menu.ActivatorDefensiveDebuffSlow.MenuItems.Add(
                    Menu.ActivatorDefensiveDebuffSlow.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveDebuffSlowRanduins", "Randuins Omen").SetValue(false)));
                Menu.ActivatorDefensiveDebuffSlow.MenuItems.Add(
                    Menu.ActivatorDefensiveDebuffSlow.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveDebuffSlowConfigRanduins", "Enemy Count Randuins")
                            .SetValue(new Slider(2, 5, 1))));
                Menu.ActivatorDefensiveDebuffSlow.MenuItems.Add(
                    Menu.ActivatorDefensiveDebuffSlow.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveDebuffSlowFrostQueensClaim", "Frost Queens Claim")
                            .SetValue(false)));
                Menu.ActivatorDefensiveDebuffSlow.MenuItems.Add(
                    Menu.ActivatorDefensiveDebuffSlow.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveDebuffSlowConfigFrostQueensClaim", "Enemy Count FQC")
                            .SetValue(new Slider(2, 5, 1))));
                Menu.ActivatorDefensiveDebuffSlow.MenuItems.Add(
                    Menu.ActivatorDefensiveDebuffSlow.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveDebuffSlowActive", "Active").SetValue(false)));

                Menu.ActivatorDefensiveCleanseSelf.Menu =
                    Menu.ActivatorDefensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Cleanse me",
                        "SAwarenessActivatorDefensiveCleanseSelf"));
                Menu.ActivatorDefensiveCleanseSelf.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseSelf.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseSelfQSS", "QSS").SetValue(false)));
                Menu.ActivatorDefensiveCleanseSelf.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseSelf.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseSelfMercurialScimitar", "Mercurial Scimitar")
                            .SetValue(false)));
                Menu.ActivatorDefensiveCleanseSelf.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseSelf.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseSelfDervishBlade", "Dervish Blade").SetValue(
                            false)));
                Menu.ActivatorDefensiveCleanseSelf.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseSelf.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseSelfConfigMinSpells", "Min Spells").SetValue(
                            new Slider(2, 10, 1))));
                Menu.ActivatorDefensiveCleanseSelf.MenuItems.Add(
                    Menu.ActivatorDefensiveCleanseSelf.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveCleanseSelfActive", "Active").SetValue(false)));

                Menu.ActivatorDefensiveShieldBoost.Menu =
                    Menu.ActivatorDefensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu(
                        "Shield/Boost | Not implemented", "SAwarenessActivatorDefensiveShieldBoost"));
                Menu.ActivatorDefensiveShieldBoost.MenuItems.Add(
                    Menu.ActivatorDefensiveShieldBoost.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveShieldBoostLocketofIronSolari",
                            "Locket of Iron Solari").SetValue(false)));
                Menu.ActivatorDefensiveShieldBoost.MenuItems.Add(
                    Menu.ActivatorDefensiveShieldBoost.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveShieldBoostTalismanofAscension",
                            "Talisman of Ascension").SetValue(false)));
                Menu.ActivatorDefensiveShieldBoost.MenuItems.Add(
                    Menu.ActivatorDefensiveShieldBoost.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveShieldBoostFaceOfTheMountain", "Face of the Mountain")
                            .SetValue(false)));
                Menu.ActivatorDefensiveShieldBoost.MenuItems.Add(
                    Menu.ActivatorDefensiveShieldBoost.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveShieldBoostGuardiansHorn", "Guardians Horn").SetValue(
                            false)));
                Menu.ActivatorDefensiveShieldBoost.MenuItems.Add(
                    Menu.ActivatorDefensiveShieldBoost.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveShieldBoostConfigHealth", "Health Percent").SetValue(
                            new Slider(20, 100, 1))));
                Menu.ActivatorDefensiveShieldBoost.MenuItems.Add(
                    Menu.ActivatorDefensiveShieldBoost.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveShieldBoostActive", "Active").SetValue(false)));

                Menu.ActivatorDefensiveMikaelCleanse.Menu =
                    Menu.ActivatorDefensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Mikael Cleanse",
                        "SAwarenessActivatorDefensiveMikaelCleanse"));
                Menu.ActivatorDefensiveMikaelCleanse.MenuItems.Add(
                    Menu.ActivatorDefensiveMikaelCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveMikaelCleanseConfigAlly", "On Allies").SetValue(false)));
                Menu.ActivatorDefensiveMikaelCleanse.MenuItems.Add(
                    Menu.ActivatorDefensiveMikaelCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveMikaelCleanseConfigAllyHealth", "Ally Health")
                            .SetValue(new Slider(20, 100, 0))));
                Menu.ActivatorDefensiveMikaelCleanse.MenuItems.Add(
                    Menu.ActivatorDefensiveMikaelCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveMikaelCleanseConfigSelfHealth", "Self Health")
                            .SetValue(new Slider(20, 100, 0))));
                Menu.ActivatorDefensiveMikaelCleanse.MenuItems.Add(
                    Menu.ActivatorDefensiveMikaelCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveMikaelCleanseConfigMinSpells", "Min Spells").SetValue(
                            new Slider(2, 10, 1))));
                Menu.ActivatorDefensiveMikaelCleanse.MenuItems.Add(
                    Menu.ActivatorDefensiveMikaelCleanse.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveShieldBoostActive", "Active").SetValue(false)));
                Menu.ActivatorDefensive.MenuItems.Add(
                    Menu.ActivatorDefensive.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorDefensiveActive", "Active").SetValue(false)));

                //Menu.ActivatorMisc.Menu =
                //    Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Misc Items",
                //        "SAwarenessActivatorMisc"));
                //Menu.ActivatorMisc.MenuItems.Add(
                //    Menu.ActivatorMisc.Menu.AddItem(
                //        new MenuItem("SAwarenessActivatorMisc", "Banner of Command").SetValue(false)));
                //Menu.ActivatorMisc.MenuItems.Add(
                //    Menu.ActivatorMisc.Menu.AddItem(
                //        new MenuItem("SAwarenessActivatorMisc", "Entropy").SetValue(false)));
                //Menu.ActivatorMisc.MenuItems.Add(
                //    Menu.ActivatorMisc.Menu.AddItem(
                //        new MenuItem("SAwarenessActivatorMisc", "Ravenous Hydra").SetValue(false)));
                //Menu.ActivatorOffensiveAd.MenuItems.Add(
                //    Menu.ActivatorOffensiveAd.Menu.AddItem(
                //        new MenuItem("SAwarenessActivatorOffensiveAdSwordOfTheDevine", "Sword Of The Devine").SetValue(
                //            false)));
                //Menu.ActivatorOffensiveAd.MenuItems.Add(
                //    Menu.ActivatorOffensiveAd.Menu.AddItem(
                //        new MenuItem("SAwarenessActivatorOffensiveAdTiamat", "Tiamat").SetValue(false)));
                //Menu.ActivatorOffensiveAd.MenuItems.Add(
                //    Menu.ActivatorOffensiveAd.Menu.AddItem(
                //        new MenuItem("SAwarenessActivatorOffensiveAdYoumuusGhostblade", "Youmuu's Ghostblade").SetValue(
                //            false)));
                ////Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveAdMuramana", "Muramana").SetValue(false)));
                //Menu.ActivatorOffensiveAd.MenuItems.Add(
                //    Menu.ActivatorOffensiveAd.Menu.AddItem(
                //        new MenuItem("SAwarenessActivatorOffensiveAdActive", "Active").SetValue(false)));

                //Menu.ActivatorOffensiveAp.Menu =
                //    Menu.ActivatorOffensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AP",
                //        "SAwarenessActivatorOffensiveAp"));
                //Menu.ActivatorOffensiveAp.MenuItems.Add(
                //    Menu.ActivatorOffensiveAp.Menu.AddItem(
                //        new MenuItem("SAwarenessActivatorOffensiveApBilgewaterCutlass", "Bilgewater Cutlass").SetValue(
                //            false)));
                //Menu.ActivatorOffensiveAp.MenuItems.Add(
                //    Menu.ActivatorOffensiveAp.Menu.AddItem(
                //        new MenuItem("SAwarenessActivatorOffensiveApBlackfireTorch", "Blackfire Torch").SetValue(false)));
                //Menu.ActivatorOffensiveAp.MenuItems.Add(
                //    Menu.ActivatorOffensiveAp.Menu.AddItem(
                //        new MenuItem("SAwarenessActivatorOffensiveApDFG", "Deathfire Grasp").SetValue(false)));
                //Menu.ActivatorOffensiveAp.MenuItems.Add(
                //    Menu.ActivatorOffensiveAp.Menu.AddItem(
                //        new MenuItem("SAwarenessActivatorOffensiveApHextechGunblade", "Hextech Gunblade").SetValue(false)));
                //Menu.ActivatorOffensiveAp.MenuItems.Add(
                //    Menu.ActivatorOffensiveAp.Menu.AddItem(
                //        new MenuItem("SAwarenessActivatorOffensiveApTwinShadows", "Twin Shadows").SetValue(false)));
                ////Menu.ActivatorOffensiveAp.MenuItems.Add(Menu.ActivatorOffensiveAp.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveApOdynsVeil", "Odyn's Veil").SetValue(false)));
                //Menu.ActivatorOffensiveAp.MenuItems.Add(
                //    Menu.ActivatorOffensiveAp.Menu.AddItem(
                //        new MenuItem("SAwarenessActivatorOffensiveApActive", "Active").SetValue(false)));
                //Menu.ActivatorOffensive.MenuItems.Add(
                //    Menu.ActivatorOffensive.Menu.AddItem(
                //        new MenuItem("SAwarenessActivatorOffensiveKey", "Key").SetValue(new KeyBind(32,
                //            KeyBindType.Press))));
                //Menu.ActivatorOffensive.MenuItems.Add(
                //    Menu.ActivatorOffensive.Menu.AddItem(
                //        new MenuItem("SAwarenessActivatorOffensiveActive", "Active").SetValue(false)));

                Menu.AutoShield.Menu =
                    Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoShield | Beta",
                        "SAwarenessAutoShield"));
                Menu.AutoShield.MenuItems.Add(
                    Menu.AutoShield.Menu.AddItem(
                        new MenuItem("SAwarenessAutoShieldBlockAA", "Block AutoAttack").SetValue(false)));
                Menu.AutoShield.MenuItems.Add(
                    Menu.AutoShield.Menu.AddItem(new MenuItem("SAwarenessAutoShieldBlockCC", "Block CC").SetValue(false)));
                Menu.AutoShield.MenuItems.Add(
                    Menu.AutoShield.Menu.AddItem(
                        new MenuItem("SAwarenessAutoShieldBlockDamageAmount", "Block Damage").SetValue(
                            new StringList(new[] { "Medium", "High", "Extreme" }))));
                Menu.AutoShield.MenuItems.Add(
                    Menu.AutoShield.Menu.AddItem(
                        new MenuItem("SAwarenessAutoShieldBlockMinDamageAmount", "Block min Damage").SetValue(
                            new Slider(50, 2000, 1))));
                Menu.AutoShield.MenuItems.Add(
                    Menu.AutoShield.Menu.AddItem(new MenuItem("SAwarenessAutoShieldBlockableSpellsActive", "Block specified Spells").SetValue(false)));
                Menu.AutoShieldBlockableSpells.Menu =
                    Menu.AutoShield.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Blockable Spells",
                        "SAwarenessAutoShieldBlockableSpells"));
                foreach (var spell in AutoShield.GetBlockableSpells())
                {
                    Menu.AutoShieldBlockableSpells.MenuItems.Add(
                        Menu.AutoShieldBlockableSpells.Menu.AddItem(new MenuItem("SAwarenessAutoShieldBlockableSpells" + spell, spell).SetValue(false)));
                }
                Menu.AutoShield.MenuItems.Add(
                    Menu.AutoShield.Menu.AddItem(new MenuItem("SAwarenessAutoShieldAlly", "Shield Ally").SetValue(false)));
                Menu.AutoShield.MenuItems.Add(
                    Menu.AutoShield.Menu.AddItem(new MenuItem("SAwarenessAutoShieldActive", "Active").SetValue(false)));
                Menu.AutoPot.Menu =
                    Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoPot", "SAwarenessAutoPot"));
                tempSettings = Menu.AutoPot.AddMenuItemSettings("HealthPot",
                    "SAwarenessAutoPotHealthPot");
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessAutoPotHealthPotPercent", "Health Percent").SetValue(new Slider(20, 99,
                            0))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(new MenuItem("SAwarenessAutoPotHealthPotActive", "Active").SetValue(false)));
                tempSettings = Menu.AutoPot.AddMenuItemSettings("ManaPot",
                    "SAwarenessAutoPotManaPot");
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessAutoPotManaPotPercent", "Mana Percent").SetValue(new Slider(20, 99, 0))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(new MenuItem("SAwarenessAutoPotManaPotActive", "Active").SetValue(false)));
                Menu.AutoPot.MenuItems.Add(
                    Menu.AutoPot.Menu.AddItem(new MenuItem("SAwarenessAutoPotOverusage", "Prevent Overusage").SetValue(false)));
                Menu.AutoPot.MenuItems.Add(
                    Menu.AutoPot.Menu.AddItem(new MenuItem("SAwarenessAutoPotActive", "Active").SetValue(false)));
                Menu.ActivatorAutoHeal.Menu =
                    Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoHeal | Beta",
                        "SAwarenessActivatorAutoHeal"));
                Menu.ActivatorAutoHeal.MenuItems.Add(
                    Menu.ActivatorAutoHeal.Menu.AddItem(new MenuItem("SAwarenessActivatorAutoHealPercent", "Percent").SetValue(new Slider(20, 99, 0))));
                Menu.ActivatorAutoHeal.MenuItems.Add(
                    Menu.ActivatorAutoHeal.Menu.AddItem(new MenuItem("SAwarenessActivatorAutoHealActive", "Active").SetValue(false)));
                Menu.ActivatorAutoUlt.Menu =
                    Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoUlt | Beta",
                        "SAwarenessActivatorAutoUlt"));
                Menu.ActivatorAutoUlt.MenuItems.Add(
                    Menu.ActivatorAutoUlt.Menu.AddItem(new MenuItem("SAwarenessActivatorAutoUltAlly", "Ally").SetValue(false)));
                Menu.ActivatorAutoUlt.MenuItems.Add(
                    Menu.ActivatorAutoUlt.Menu.AddItem(new MenuItem("SAwarenessActivatorAutoUltActive", "Active").SetValue(false)));
                Menu.Activator.MenuItems.Add(
                    Menu.Activator.Menu.AddItem(new MenuItem("SAwarenessActivatorActive", "Active").SetValue(false)));

                Menu.ActivatorAutoQss.Menu =
                   Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("QSS | Beta",
                       "SAwarenessActivatorAutoQssConfig"));
                Menu.ActivatorAutoQss.MenuItems.Add(
                    Menu.ActivatorAutoQss.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoQssMinSpells", "Min Spells").SetValue(
                            new Slider(2, 10, 1))));
                Menu.ActivatorAutoQss.MenuItems.Add(
                    Menu.ActivatorAutoQss.Menu.AddItem(new MenuItem("SAwarenessActivatorAutoQssActive", "Active").SetValue(false)));

                Menu.ActivatorAutoQssConfig.Menu =
                    Menu.ActivatorAutoQss.Menu.AddSubMenu(new LeagueSharp.Common.Menu("QSS Config",
                        "SAwarenessActivatorAutoQssConfig"));
                Menu.ActivatorAutoQssConfig.MenuItems.Add(
                    Menu.ActivatorAutoQssConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoQssConfigStun", "Stun").SetValue(false)));
                Menu.ActivatorAutoQssConfig.MenuItems.Add(
                    Menu.ActivatorAutoQssConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoQssConfigSilence", "Silence").SetValue(false)));
                Menu.ActivatorAutoQssConfig.MenuItems.Add(
                    Menu.ActivatorAutoQssConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoQssConfigTaunt", "Taunt").SetValue(false)));
                Menu.ActivatorAutoQssConfig.MenuItems.Add(
                    Menu.ActivatorAutoQssConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoQssConfigFear", "Fear").SetValue(false)));
                Menu.ActivatorAutoQssConfig.MenuItems.Add(
                    Menu.ActivatorAutoQssConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoQssConfigCharm", "Charm").SetValue(false)));
                Menu.ActivatorAutoQssConfig.MenuItems.Add(
                    Menu.ActivatorAutoQssConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoQssConfigBlind", "Blind").SetValue(false)));
                Menu.ActivatorAutoQssConfig.MenuItems.Add(
                    Menu.ActivatorAutoQssConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoQssConfigDisarm", "Disarm").SetValue(false)));
                Menu.ActivatorAutoQssConfig.MenuItems.Add(
                    Menu.ActivatorAutoQssConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoQssConfigSuppress", "Suppress").SetValue(false)));
                Menu.ActivatorAutoQssConfig.MenuItems.Add(
                    Menu.ActivatorAutoQssConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoQssConfigSlow", "Slow").SetValue(false)));
                Menu.ActivatorAutoQssConfig.MenuItems.Add(
                    Menu.ActivatorAutoQssConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoQssConfigCombatDehancer", "Combat Dehancer")
                            .SetValue(false)));
                Menu.ActivatorAutoQssConfig.MenuItems.Add(
                    Menu.ActivatorAutoQssConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoQssConfigSnare", "Snare").SetValue(false)));
                Menu.ActivatorAutoQssConfig.MenuItems.Add(
                    Menu.ActivatorAutoQssConfig.Menu.AddItem(
                        new MenuItem("SAwarenessActivatorAutoQssConfigPoison", "Posion").SetValue(false)));  

                ////Not crashing
                Menu.Misc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Misc", "SAwarenessMisc"));
                Menu.Misc.MenuItems.Add(
                    Menu.Misc.Menu.AddItem(new MenuItem("SAwarenessMiscActive", "Active").SetValue(false)));
                Menu.SkinChanger.Menu =
                    Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SkinChanger", "SAwarenessSkinChanger"));
                Menu.SkinChanger.MenuItems.Add(
                    Menu.SkinChanger.Menu.AddItem(
                        new MenuItem("SAwarenessSkinChangerSkinName", "Skin").SetValue(
                            new StringList(SkinChanger.GetSkinList(ObjectManager.Player.ChampionName))).DontSave()));
                Menu.SkinChanger.MenuItems.Add(
                    Menu.SkinChanger.Menu.AddItem(new MenuItem("SAwarenessSkinChangerActive", "Active").SetValue(false)));
                Menu.SafeMovement.Menu =
                    Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SafeMovement", "SAwarenessSafeMovement"));
                Menu.SafeMovement.MenuItems.Add(
                    Menu.SafeMovement.Menu.AddItem(
                        new MenuItem("SAwarenessSafeMovementBlockIntervall", "Block Interval").SetValue(new Slider(20,
                            1000, 0))));
                Menu.SafeMovement.MenuItems.Add(
                    Menu.SafeMovement.Menu.AddItem(new MenuItem("SAwarenessSafeMovementActive", "Active").SetValue(false)));
                Menu.AutoLevler.Menu =
                    Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoLevler", "SAwarenessAutoLevler"));
                tempSettings = Menu.AutoLevler.AddMenuItemSettings("Priority",
                    "SAwarenessAutoLevlerPriority");
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessAutoLevlerPrioritySliderQ", "Q").SetValue(new Slider(0, 3, 0))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessAutoLevlerPrioritySliderW", "W").SetValue(new Slider(0, 3, 0))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessAutoLevlerPrioritySliderE", "E").SetValue(new Slider(0, 3, 0))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessAutoLevlerPrioritySliderR", "R").SetValue(new Slider(0, 3, 0))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessAutoLevlerPriorityFirstSpells", "Mode").SetValue(
                            new StringList(new[] { "Q W E", "Q E W", "W Q E", "W E Q", "E Q W", "E W Q" }))));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessAutoLevlerPriorityFirstSpellsActive", "Mode Active").SetValue(false)));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessAutoLevlerPriorityActive", "Active").SetValue(false).DontSave()));
                tempSettings = Menu.AutoLevler.AddMenuItemSettings("Sequence | not implemented",
                    "SAwarenessAutoLevlerSequence");
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessAutoLevlerSequenceLoadChampion", "Load Champion").SetValue(false)
                            .DontSave()));
                tempSettings.MenuItems.Add(
                    tempSettings.Menu.AddItem(
                        new MenuItem("SAwarenessAutoLevlerSequenceActive", "Active").SetValue(false).DontSave()));
                Menu.AutoLevler.MenuItems.Add(
                    Menu.AutoLevler.Menu.AddItem(
                        new MenuItem("SAwarenessAutoLevlerSMode", "Mode").SetValue(
                            new StringList(new[] { "Sequence", "Priority", "Only R" }))));
                Menu.AutoLevler.MenuItems.Add(
                    Menu.AutoLevler.Menu.AddItem(new MenuItem("SAwarenessAutoLevlerActive", "Active").SetValue(false)));
                Menu.MoveToMouse.Menu =
                    Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Move To Mouse", "SAwarenessMoveToMouse"));
                Menu.MoveToMouse.MenuItems.Add(
                    Menu.MoveToMouse.Menu.AddItem(
                        new MenuItem("SAwarenessMoveToMouseKey", "Key").SetValue(new KeyBind(90, KeyBindType.Press))));
                Menu.MoveToMouse.MenuItems.Add(
                    Menu.MoveToMouse.Menu.AddItem(new MenuItem("SAwarenessMoveToMouseActive", "Active").SetValue(false)));
                Menu.SurrenderVote.Menu =
                    Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Surrender Vote", "SAwarenessSurrenderVote"));
                Menu.SurrenderVote.MenuItems.Add(
                    Menu.SurrenderVote.Menu.AddItem(
                        new MenuItem("SAwarenessSurrenderVoteChatChoice", "Chat Choice").SetValue(
                            new StringList(new[] { "None", "Local", "Server" }))));
                Menu.SurrenderVote.MenuItems.Add(
                    Menu.SurrenderVote.Menu.AddItem(
                        new MenuItem("SAwarenessSurrenderVoteActive", "Active").SetValue(false)));
                Menu.AutoLatern.Menu =
                    Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Auto Latern", "SAwarenessAutoLatern"));
                Menu.AutoLatern.MenuItems.Add(
                    Menu.AutoLatern.Menu.AddItem(
                        new MenuItem("SAwarenessAutoLaternKey", "Key").SetValue(new KeyBind(84, KeyBindType.Press))));
                Menu.AutoLatern.MenuItems.Add(
                    Menu.AutoLatern.Menu.AddItem(new MenuItem("SAwarenessAutoLaternActive", "Active").SetValue(false)));
                Menu.AutoJump.Menu =
                    Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Auto Jump", "SAwarenessAutoJump"));
                Menu.AutoJump.MenuItems.Add(
                    Menu.AutoJump.Menu.AddItem(
                        new MenuItem("SAwarenessAutoJumpKey", "Key").SetValue(new KeyBind(85, KeyBindType.Press))));
                Menu.AutoJump.MenuItems.Add(
                    Menu.AutoJump.Menu.AddItem(new MenuItem("SAwarenessAutoJumpActive", "Active").SetValue(false)));
                Menu.DisconnectDetector.Menu =
                    Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Disconnect Detector",
                        "SAwarenessDisconnectDetector"));
                Menu.DisconnectDetector.MenuItems.Add(
                    Menu.DisconnectDetector.Menu.AddItem(
                        new MenuItem("SAwarenessDisconnectDetectorChatChoice", "Chat Choice").SetValue(
                            new StringList(new[] { "None", "Local", "Server" }))));
                Menu.DisconnectDetector.MenuItems.Add(
                    Menu.DisconnectDetector.Menu.AddItem(
                        new MenuItem("SAwarenessDisconnectDetectorActive", "Active").SetValue(false)));
                Menu.TurnAround.Menu =
                    Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Turn Around", "SAwarenessTurnAround"));
                Menu.TurnAround.MenuItems.Add(
                    Menu.TurnAround.Menu.AddItem(new MenuItem("SAwarenessTurnAroundActive", "Active").SetValue(false)));
                Menu.MinionBars.Menu =
                    Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Minion Bars", "SAwarenessMinionBars"));
                Menu.MinionBars.MenuItems.Add(
                    Menu.MinionBars.Menu.AddItem(new MenuItem("SAwarenessMinionBarsGlowActive", "Glow Killable").SetValue(false)));
                Menu.MinionBars.MenuItems.Add(
                    Menu.MinionBars.Menu.AddItem(new MenuItem("SAwarenessMinionBarsActive", "Active").SetValue(false)));
                //Menu.MinionLocation.Menu =
                //    Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Minion Location", "SAwarenessMinionLocation"));
                //Menu.MinionLocation.MenuItems.Add(
                //    Menu.MinionLocation.Menu.AddItem(new MenuItem("SAwarenessMinionLocationActive", "Active").SetValue(false)));
                Menu.FlashJuke.Menu =
                    Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Flash Juke", "SAwarenessFlashJuke"));
                Menu.FlashJuke.MenuItems.Add(
                    Menu.FlashJuke.Menu.AddItem(new MenuItem("SAwarenessFlashJukeKeyActive", "Active Key").SetValue(new KeyBind(90, KeyBindType.Press))));
                Menu.FlashJuke.MenuItems.Add(
                    Menu.FlashJuke.Menu.AddItem(new MenuItem("SAwarenessFlashJukeRecall", "Recall").SetValue(false)));
                Menu.FlashJuke.MenuItems.Add(
                    Menu.FlashJuke.Menu.AddItem(new MenuItem("SAwarenessFlashJukeActive", "Active").SetValue(false)));
                Menu.EasyRangedJungle.Menu =
                    Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Ranged Jungle Spots", "SAwarenessEasyRangedJungle"));
                Menu.EasyRangedJungle.MenuItems.Add(
                    Menu.EasyRangedJungle.Menu.AddItem(new MenuItem("SAwarenessEasyRangedJungleActive", "Active").SetValue(false)));
                Menu.FowWardPlacement.Menu =
                    Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Fow Ward Placement", "SAwarenessFowWardPlacement"));
                Menu.FowWardPlacement.MenuItems.Add(
                    Menu.FowWardPlacement.Menu.AddItem(new MenuItem("SAwarenessFowWardPlacementActive", "Active").SetValue(false)));
                Menu.RealTime.Menu =
                    Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Real Time", "SAwarenessRealTime"));
                Menu.RealTime.MenuItems.Add(
                    Menu.RealTime.Menu.AddItem(new MenuItem("SAwarenessRealTimeActive", "Active").SetValue(false)));

                Menu.GlobalSettings.Menu =
                    menu.AddSubMenu(new LeagueSharp.Common.Menu("Global Settings", "SAwarenessGlobalSettings"));
                Menu.GlobalSettings.MenuItems.Add(
                    Menu.GlobalSettings.Menu.AddItem(
                        new MenuItem("SAwarenessGlobalSettingsServerChatPingActive", "Server Chat/Ping").SetValue(false)));

                menu.AddItem(new MenuItem("By Screeder", "By Screeder V0.85"));
                menu.AddToMainMenu();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                CreateMenu();
                Game.PrintChat("SAwareness loaded!");
                //Game.OnGameUpdate += GameOnOnGameUpdate;
                new Thread(GameOnOnGameUpdate).Start();
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                AppDomain.CurrentDomain.DomainUnload += delegate { threadActive = false; };
                AppDomain.CurrentDomain.ProcessExit += delegate { threadActive = false; };
            }
            catch (Exception e)
            {
                Console.WriteLine("SAwareness: " + e);
            }
        }

        private static bool threadActive = true;

        private static void GameOnOnGameUpdate(/*EventArgs args*/)
        {
            try
            {
                while (threadActive)
                {
                    Thread.Sleep(1);
                    Type classType = typeof(Menu);
                    BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
                    FieldInfo[] fields = classType.GetFields(flags);
                    foreach (FieldInfo p in fields)
                    {
                        var item = (Menu.MenuItemSettings)p.GetValue(null);
                        if (item.GetActive() == false && item.Item != null)
                        {
                            //item.Item = null;
                        }
                        else if (item.GetActive() && item.Item == null && !item.ForceDisable && item.Type != null)
                        {
                            try
                            {
                                item.Item = System.Activator.CreateInstance(item.Type);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("SAwareness: " + e);
                throw;
            }
            
            //CreateDebugInfos();
        }

        public static PropertyInfo[] GetPublicProperties(Type type)
        {
            if (type.IsInterface)
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    Type subType = queue.Dequeue();
                    foreach (Type subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface)) continue;

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    PropertyInfo[] typeProperties = subType.GetProperties(
                        BindingFlags.FlattenHierarchy
                        | BindingFlags.Public
                        | BindingFlags.Instance);

                    IEnumerable<PropertyInfo> newPropertyInfos = typeProperties
                        .Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }

            return type.GetProperties(BindingFlags.Static | BindingFlags.Public);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Load();
        }

        public static Assembly Load()
        {
            byte[] ba = null;
            string resource = "SAwareness.Resources.DLL.Evade.dll";
            Assembly curAsm = Assembly.GetExecutingAssembly();
            using (Stream stm = curAsm.GetManifestResourceStream(resource))
            {
                ba = new byte[(int) stm.Length];
                stm.Read(ba, 0, (int) stm.Length);
                return Assembly.Load(ba);
            }
        }

        private static void CreateDebugInfos()
        {
            if (lastDebugTime + 60 > Game.ClockTime)
                return;
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter("C:\\SAwarenessDebug.log");
                if(writer == null)
                    return;
                writer.WriteLine("Debug Infos of game: " + Game.Id);
                writer.WriteLine("MapId: " + Game.MapId);
                writer.WriteLine("Mode: " + Game.Mode);
                writer.WriteLine("Region: " + Game.Region);
                writer.WriteLine("Type: " + Game.Type);
                writer.WriteLine("Time: " + Game.ClockTime);

                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (hero.IsMe)
                    {
                        writer.WriteLine("Player: ");
                    }
                    else if (hero.IsAlly)
                    {
                        writer.WriteLine("Ally: ");
                    }
                    else if (hero.IsEnemy)
                    {
                        writer.WriteLine("Enemy: ");
                    }
                    writer.WriteLine("Character: " + hero.ChampionName);
                    writer.Write("Summoners: ");
                    foreach (var spell in hero.SummonerSpellbook.Spells)
                    {
                        writer.Write(spell.SData.Name + ", ");
                    }
                    writer.WriteLine("");
                    writer.Write("Items: ");
                    foreach (var item in hero.InventoryItems)
                    {
                        writer.Write(item.Name + ", ");
                    }
                    writer.WriteLine("");
                }
                Type classType = typeof(Menu);
                BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
                FieldInfo[] fields = classType.GetFields(flags);
                writer.WriteLine("Activated Options: ");
                foreach (FieldInfo p in fields)
                {
                    var item = (Menu.MenuItemSettings)p.GetValue(null);
                    if (item.GetActive() == false && item.Item != null)
                    {
                        //item.Item = null;
                    }
                    else if (item.GetActive() && !item.ForceDisable)
                    {
                        try
                        {
                            writer.WriteLine("- " + item.Menu.Name);
                            foreach (var menuItem in item.MenuItems)
                            {
                                try{ writer.WriteLine("  - " + menuItem.Name + " | " + menuItem.GetValue<Boolean>()); }
                                catch (Exception e){ if (e is InvalidCastException || e is NullReferenceException) { } }
                                try { writer.WriteLine("  - " + menuItem.Name + " | " + menuItem.GetValue<Slider>().Value); }
                                catch (Exception e) { if (e is InvalidCastException || e is NullReferenceException) { } }
                                try { writer.WriteLine("  - " + menuItem.Name + " | " + menuItem.GetValue<KeyBind>().Active); }
                                catch (Exception e) { if (e is InvalidCastException || e is NullReferenceException) { } }
                                try { writer.WriteLine("  - " + menuItem.Name + " | " + menuItem.GetValue<StringList>().SelectedIndex); }
                                catch (Exception e) { if (e is InvalidCastException || e is NullReferenceException) { } }
                            }
                            //item.Item = System.Activator.CreateInstance(item.Type);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                }
                lastDebugTime = Game.ClockTime;
            }
            catch (Exception e)
            {
                Console.WriteLine("SAwareness: " + e);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Flush();
                    writer.Close();
                }               
            }            
        }
    }
}