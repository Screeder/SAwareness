using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Toolkit.Diagnostics;
using MenuItem = LeagueSharp.Common.MenuItem;

namespace SAwareness
{
    static class Menu
    {
        public class MenuItemSettings
        {
            public String Name;
            public dynamic Item;
            public Type Type;
            public bool ForceDisable;
            public LeagueSharp.Common.Menu Menu;
            public List<MenuItemSettings> SubMenus = new List<MenuItemSettings>();
            public List<MenuItem> MenuItems = new List<MenuItem>();

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
                SubMenus.Add(new Menu.MenuItemSettings(name));
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
                foreach (var item in Menu.Items)
                {
                    if (item.DisplayName == "Active")
                    {
                        if (item.GetValue<bool>())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return false;
            }

            public void SetActive(bool active)
            {
                if (Menu == null)
                    return;
                foreach (var item in Menu.Items)
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
                foreach (var item in Menu.Items)
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
                foreach (var menu in SubMenus)
                {
                    if (menu.Name.Contains(name))
                        return menu;
                }
                return null;
            }
        }

        public static MenuItemSettings ItemPanel = new MenuItemSettings();
        public static MenuItemSettings AutoLevler = new MenuItemSettings(typeof(SAwareness.AutoLevler)); //Only priority works
        public static MenuItemSettings UiTracker = new MenuItemSettings(typeof(SAwareness.UITracker)); //Works but need many improvements
        public static MenuItemSettings UimTracker = new MenuItemSettings(typeof(SAwareness.UIMTracker)); //Works but need many improvements
        public static MenuItemSettings SsCaller = new MenuItemSettings(typeof(SAwareness.SsCaller)); //Works
        public static MenuItemSettings Tracker = new MenuItemSettings();
        public static MenuItemSettings WaypointTracker = new MenuItemSettings(typeof(SAwareness.WaypointTracker)); //Works
        public static MenuItemSettings CloneTracker = new MenuItemSettings(typeof(SAwareness.CloneTracker)); //Works
        public static MenuItemSettings Timers = new MenuItemSettings(typeof(SAwareness.Timers));
        public static MenuItemSettings JungleTimer = new MenuItemSettings(); //Works
        public static MenuItemSettings RelictTimer = new MenuItemSettings(); //Works
        public static MenuItemSettings HealthTimer = new MenuItemSettings(); //Works
        public static MenuItemSettings InhibitorTimer = new MenuItemSettings(); //Works
        public static MenuItemSettings Health = new MenuItemSettings(typeof(SAwareness.Health));
        public static MenuItemSettings TowerHealth = new MenuItemSettings(); //Missing HPBarPos
        public static MenuItemSettings InhibitorHealth = new MenuItemSettings(); //Works
        public static MenuItemSettings DestinationTracker = new MenuItemSettings(typeof(SAwareness.DestinationTracker));  //Work & Needs testing
        public static MenuItemSettings Detector = new MenuItemSettings();
        public static MenuItemSettings VisionDetector = new MenuItemSettings(typeof(SAwareness.HiddenObject)); //Works - OnProcessSpell bugged
        public static MenuItemSettings RecallDetector = new MenuItemSettings(typeof(SAwareness.Recall)); //Works
        public static MenuItemSettings Range = new MenuItemSettings(typeof(SAwareness.Ranges)); //Many ranges are bugged. Waiting for SpellLib
        public static MenuItemSettings TowerRange = new MenuItemSettings();
        public static MenuItemSettings ExperienceRange = new MenuItemSettings();
        public static MenuItemSettings AttackRange = new MenuItemSettings();
        public static MenuItemSettings SpellQRange = new MenuItemSettings();
        public static MenuItemSettings SpellWRange = new MenuItemSettings();
        public static MenuItemSettings SpellERange = new MenuItemSettings();
        public static MenuItemSettings SpellRRange = new MenuItemSettings();
        public static MenuItemSettings ImmuneTimer = new MenuItemSettings(typeof(SAwareness.ImmuneTimer)); //Works
        public static MenuItemSettings Ganks = new MenuItemSettings();
        public static MenuItemSettings GankTracker = new MenuItemSettings(typeof(SAwareness.GankPotentialTracker)); //Works
        public static MenuItemSettings GankDetector = new MenuItemSettings(typeof(SAwareness.GankDetector)); //Works
        public static MenuItemSettings AltarTimer = new MenuItemSettings();
        public static MenuItemSettings Wards = new MenuItemSettings();
        public static MenuItemSettings WardCorrector = new MenuItemSettings(typeof(SAwareness.WardCorrector)); //Works
        public static MenuItemSettings SkinChanger = new MenuItemSettings(typeof(SAwareness.SkinChanger)); //Works
        public static MenuItemSettings AutoSmite = new MenuItemSettings(typeof(SAwareness.AutoSmite)); //Works
        public static MenuItemSettings AutoPot = new MenuItemSettings(typeof(SAwareness.AutoPot));
        public static MenuItemSettings SafeMovement = new MenuItemSettings(typeof(SAwareness.SafeMovement));
        public static MenuItemSettings AutoShield = new MenuItemSettings(typeof(SAwareness.AutoShield));
        public static MenuItemSettings Misc = new MenuItemSettings();
        public static MenuItemSettings Activator = new MenuItemSettings(typeof(SAwareness.Activator));
        public static MenuItemSettings ActivatorAutoSummonerSpell = new MenuItemSettings();
        public static MenuItemSettings ActivatorAutoSummonerSpellIgnite = new MenuItemSettings();
        public static MenuItemSettings ActivatorAutoSummonerSpellHeal = new MenuItemSettings();
        public static MenuItemSettings ActivatorAutoSummonerSpellBarrier = new MenuItemSettings();
        public static MenuItemSettings ActivatorOffensive = new MenuItemSettings();
        public static MenuItemSettings ActivatorOffensiveAd = new MenuItemSettings();
        public static MenuItemSettings ActivatorOffensiveAp = new MenuItemSettings();
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                
                SUpdater.UpdateCheck();                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
            
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void CreateMenu()
        {
            //http://www.cambiaresearch.com/articles/15/javascript-char-codes-key-codes
            try
            {
                Menu.MenuItemSettings tempSettings;
                Menu.MenuItemSettings oldTempSettings;
                LeagueSharp.Common.Menu menu = new LeagueSharp.Common.Menu("SAwareness", "SAwareness", true);

                Menu.Timers.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Timers", "SAwarenessTimers"));
                Menu.Timers.MenuItems.Add(Menu.Timers.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTimersPingTimes", "Ping Times").SetValue(new Slider(0, 5, 0))));
                Menu.Timers.MenuItems.Add(Menu.Timers.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTimersRemindTime", "Remind Time").SetValue(new Slider(0, 50, 0))));
                Menu.Timers.MenuItems.Add(Menu.Timers.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTimersLocalPing", "Local Ping").SetValue(false)));
                Menu.Timers.MenuItems.Add(Menu.Timers.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTimersLocalChat", "Local Chat").SetValue(true)));
                Menu.JungleTimer.Menu = Menu.Timers.Menu.AddSubMenu(new LeagueSharp.Common.Menu("JungleTimer", "SAwarenessJungleTimer"));
                Menu.JungleTimer.MenuItems.Add(Menu.JungleTimer.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessJungleTimersActive", "Active").SetValue(false)));
                Menu.RelictTimer.Menu = Menu.Timers.Menu.AddSubMenu(new LeagueSharp.Common.Menu("RelictTimer", "SAwarenessRelictTimer"));
                Menu.RelictTimer.MenuItems.Add(Menu.RelictTimer.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRelictTimersActive", "Active").SetValue(false)));
                Menu.HealthTimer.Menu = Menu.Timers.Menu.AddSubMenu(new LeagueSharp.Common.Menu("HealthTimer", "SAwarenessHealthTimer"));
                Menu.HealthTimer.MenuItems.Add(Menu.HealthTimer.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessHealthTimersActive", "Active").SetValue(false)));
                Menu.InhibitorTimer.Menu = Menu.Timers.Menu.AddSubMenu(new LeagueSharp.Common.Menu("InhibitorTimer", "SAwarenessInhibitorTimer"));
                Menu.InhibitorTimer.MenuItems.Add(Menu.InhibitorTimer.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessInhibitorTimersActive", "Active").SetValue(false)));
                Menu.AltarTimer.Menu = Menu.Timers.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AltarTimer", "SAwarenessAltarTimer"));
                Menu.AltarTimer.MenuItems.Add(Menu.AltarTimer.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAltarTimersActive", "Active").SetValue(false)));
                Menu.ImmuneTimer.Menu = Menu.Timers.Menu.AddSubMenu(new LeagueSharp.Common.Menu("ImmuneTimer", "SAwarenessImmuneTimer"));
                Menu.ImmuneTimer.MenuItems.Add(Menu.ImmuneTimer.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessImmuneTimersActive", "Active").SetValue(false)));
                Menu.Timers.MenuItems.Add(Menu.Timers.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTimersActive", "Active").SetValue(false)));

                Menu.Range.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Ranges", "SAwarenessRanges"));
                Menu.ExperienceRange.Menu = Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("ExperienceRange", "SAwarenessExperienceRange"));
                Menu.ExperienceRange.MenuItems.Add(Menu.ExperienceRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessExperienceRangeActive", "Active").SetValue(false)));
                Menu.AttackRange.Menu = Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AttackRange", "SAwarenessAttackRange"));
                Menu.AttackRange.MenuItems.Add(Menu.AttackRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAttackRangeActive", "Active").SetValue(false)));
                Menu.TowerRange.Menu = Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("TowerRange", "SAwarenessTowerRange"));
                Menu.TowerRange.MenuItems.Add(Menu.TowerRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTowerRangeActive", "Active").SetValue(false)));
                Menu.SpellQRange.Menu = Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SpellQRange", "SAwarenessSpellQRange"));
                Menu.SpellQRange.MenuItems.Add(Menu.SpellQRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSpellQRangeActive", "Active").SetValue(false)));
                Menu.SpellWRange.Menu = Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SpellWRange", "SAwarenessSpellWRange"));
                Menu.SpellWRange.MenuItems.Add(Menu.SpellWRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSpellWRangeActive", "Active").SetValue(false)));
                Menu.SpellERange.Menu = Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SpellERange", "SAwarenessSpellERange"));
                Menu.SpellERange.MenuItems.Add(Menu.SpellERange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSpellERangeActive", "Active").SetValue(false)));
                Menu.SpellRRange.Menu = Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SpellRRange", "SAwarenessSpellRRange"));
                Menu.SpellRRange.MenuItems.Add(Menu.SpellRRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSpellRRangeActive", "Active").SetValue(false)));
                Menu.Range.MenuItems.Add(Menu.Range.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRangesActive", "Active").SetValue(false)));

                Menu.Tracker.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Tracker", "SAwarenessTracker"));
                Menu.WaypointTracker.Menu = Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("WaypointTracker", "SAwarenessWaypointTracker"));
                Menu.WaypointTracker.MenuItems.Add(Menu.WaypointTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessWaypointTrackerActive", "Active").SetValue(true)));
                Menu.DestinationTracker.Menu = Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("DestinationTracker", "SAwarenessDestinationTracker"));
                Menu.DestinationTracker.MenuItems.Add(Menu.DestinationTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessDestinationTrackerActive", "Active").SetValue(true)));
                Menu.CloneTracker.Menu = Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("CloneTracker", "SAwarenessCloneTracker"));
                Menu.CloneTracker.MenuItems.Add(Menu.CloneTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessCloneTrackerActive", "Active").SetValue(true)));
                Menu.UiTracker.Menu = Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("UITracker", "SAwarenessUITracker"));
                Menu.UiTracker.MenuItems.Add(Menu.UiTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessItemPanelActive", "ItemPanel").SetValue(true)));
                Menu.UiTracker.MenuItems.Add(Menu.UiTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerScale", "Scale").SetValue(new Slider(100, 100, 0))));
                Menu.UiTracker.MenuItems.Add(Menu.UiTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerXPos", "X Position").SetValue(new Slider(-1, 10000, 0))));
                Menu.UiTracker.MenuItems.Add(Menu.UiTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerYPos", "Y Position").SetValue(new Slider(-1, 10000, 0))));
                Menu.UiTracker.MenuItems.Add(Menu.UiTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerMode", "Mode").SetValue(new StringList(new string[] { "Side", "Unit", "Both" }))));
                Menu.UiTracker.MenuItems.Add(Menu.UiTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerActive", "Active").SetValue(true)));
                Menu.UimTracker.Menu = Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("UIMTracker", "SAwarenessUIMTracker"));
                Menu.UimTracker.MenuItems.Add(Menu.UimTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUIMTrackerScale", "Scale").SetValue(new Slider(100, 100, 0))));
                Menu.UimTracker.MenuItems.Add(Menu.UimTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUIMTrackerActive", "Active").SetValue(true)));
                Menu.SsCaller.Menu = Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SSCaller", "SAwarenessSSCaller"));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerPingTimes", "Ping Times").SetValue(new Slider(0, 5, 0))));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerPingType", "Ping Type").SetValue(new StringList(new string[] { "Normal", "Danger", "EnemyMissing", "OnMyWay", "Fallback", "AssistMe" }))));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerLocalPing", "Local Ping").SetValue(false)));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerLocalChat", "Local Chat").SetValue(true)));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerDisableTime", "Disable Time").SetValue(new Slider(20, 180, 1))));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerActive", "Active").SetValue(true)));
                Menu.Tracker.MenuItems.Add(Menu.Tracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTrackerActive", "Active").SetValue(true)));

                Menu.Detector.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Detector", "SAwarenessDetector"));
                Menu.VisionDetector.Menu = Menu.Detector.Menu.AddSubMenu(new LeagueSharp.Common.Menu("VisionDetector", "SAwarenessVisionDetector"));
                Menu.VisionDetector.MenuItems.Add(Menu.VisionDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessVisionDetectorActive", "Active").SetValue(true)));
                Menu.RecallDetector.Menu = Menu.Detector.Menu.AddSubMenu(new LeagueSharp.Common.Menu("RecallDetector", "SAwarenessRecallDetector"));
                Menu.RecallDetector.MenuItems.Add(Menu.RecallDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRecallDetectorPingTimes", "Ping Times").SetValue(new Slider(0, 5, 0))));
                Menu.RecallDetector.MenuItems.Add(Menu.RecallDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRecallDetectorLocalPing", "Local Ping").SetValue(false)));
                Menu.RecallDetector.MenuItems.Add(Menu.RecallDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRecallDetectorLocalChat", "Local Chat").SetValue(true)));
                Menu.RecallDetector.MenuItems.Add(Menu.RecallDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRecallDetectorMode", "Mode").SetValue(new StringList(new string[] { "Chat", "CDTracker", "Both" }))));
                Menu.RecallDetector.MenuItems.Add(Menu.RecallDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRecallDetectorActive", "Active").SetValue(true)));
                Menu.Detector.MenuItems.Add(Menu.Detector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessDetectorActive", "Active").SetValue(true)));

                Menu.Ganks.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Ganks", "SAwarenessGanks"));
                Menu.GankTracker.Menu = Menu.Ganks.Menu.AddSubMenu(new LeagueSharp.Common.Menu("GankTracker", "SAwarenessGankTracker"));
                Menu.GankTracker.MenuItems.Add(Menu.GankTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGankTrackerPingTimes", "Ping Times").SetValue(new Slider(0, 5, 0))));
                Menu.GankTracker.MenuItems.Add(Menu.GankTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGankTrackerPingType", "Ping Type").SetValue(new StringList(new string[] { "Normal", "Danger", "EnemyMissing", "OnMyWay", "Fallback", "AssistMe" }))));
                Menu.GankTracker.MenuItems.Add(Menu.GankTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGankTrackerLocalPing", "Local Ping").SetValue(true)));
                Menu.GankTracker.MenuItems.Add(Menu.GankTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGankTrackerLocalChat", "Local Chat").SetValue(true)));
                Menu.GankTracker.MenuItems.Add(Menu.GankTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGankTrackerTrackRange", "Track Range").SetValue(new Slider(1, 10000, 1))));
                Menu.GankTracker.MenuItems.Add(Menu.GankTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGankTrackerActive", "Active").SetValue(true)));
                Menu.GankDetector.Menu = Menu.Ganks.Menu.AddSubMenu(new LeagueSharp.Common.Menu("GankDetector", "SAwarenessGankDetector"));
                Menu.GankDetector.MenuItems.Add(Menu.GankDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGankDetectorActive", "Active").SetValue(true)));
                Menu.Ganks.MenuItems.Add(Menu.Ganks.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGanksActive", "Active").SetValue(true)));

                Menu.Health.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Object Health", "SAwarenessObjectHealth"));
                Menu.TowerHealth.Menu = Menu.Health.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Tower Health", "SAwarenessTowerHealth"));
                Menu.TowerHealth.MenuItems.Add(Menu.TowerHealth.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTowerHealthActive", "Active").SetValue(true)));
                Menu.InhibitorHealth.Menu = Menu.Health.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Inhibitor Health", "SAwarenessInhibitorHealth"));
                Menu.InhibitorHealth.MenuItems.Add(Menu.InhibitorHealth.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessInhibitorHealthActive", "Active").SetValue(false)));
                Menu.Health.MenuItems.Add(Menu.Health.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessHealthMode", "Mode").SetValue(new StringList(new string[] { "Percent", "Normal" }))));
                Menu.Health.MenuItems.Add(Menu.Health.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessHealthActive", "Active").SetValue(true)));

                Menu.Wards.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Wards", "SAwarenessWards"));
                Menu.WardCorrector.Menu = Menu.Wards.Menu.AddSubMenu(new LeagueSharp.Common.Menu("WardCorrector", "SAwarenessWardCorrector"));
                Menu.WardCorrector.MenuItems.Add(Menu.WardCorrector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessWardCorrectorActive", "Active").SetValue(true)));
                Menu.Wards.MenuItems.Add(Menu.Wards.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessWardsActive", "Active").SetValue(true)));                                          

                Menu.Activator.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Activator", "SAwarenessActivator"));
                Menu.ActivatorAutoSummonerSpell.Menu = Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Auto Summoner Spells", "SAwarenessActivatorAutoSummonerSpell"));
                Menu.ActivatorAutoSummonerSpell.MenuItems.Add(Menu.ActivatorAutoSummonerSpell.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorActivatorAutoSummonerSpellActive", "Active").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellIgnite.Menu = Menu.ActivatorAutoSummonerSpell.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Auto Ignite", "SAwarenessActivatorAutoSummonerSpellIgnite"));
                Menu.ActivatorAutoSummonerSpellIgnite.MenuItems.Add(Menu.ActivatorAutoSummonerSpellIgnite.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellIgniteActive", "Active").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellHeal.Menu = Menu.ActivatorAutoSummonerSpell.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Auto Heal", "SAwarenessActivatorAutoSummonerSpellHeal"));
                Menu.ActivatorAutoSummonerSpellHeal.MenuItems.Add(Menu.ActivatorAutoSummonerSpellHeal.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellHealPercent", "Percent").SetValue(new Slider(20, 100, 1))));
                Menu.ActivatorAutoSummonerSpellHeal.MenuItems.Add(Menu.ActivatorAutoSummonerSpellHeal.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellHealAllyActive", "Ally Active").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellHeal.MenuItems.Add(Menu.ActivatorAutoSummonerSpellHeal.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellHealActive", "Active").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellBarrier.Menu = Menu.ActivatorAutoSummonerSpell.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Auto Barrier", "SAwarenessActivatorAutoSummonerSpellBarrier"));
                Menu.ActivatorAutoSummonerSpellBarrier.MenuItems.Add(Menu.ActivatorAutoSummonerSpellBarrier.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellBarrierPercent", "Percent").SetValue(new Slider(20, 100, 1))));
                Menu.ActivatorAutoSummonerSpellBarrier.MenuItems.Add(Menu.ActivatorAutoSummonerSpellBarrier.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellBarrierActive", "Active").SetValue(false)));

                Menu.ActivatorOffensive.Menu = Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Offensive Items", "SAwarenessActivatorOffensive"));
                Menu.ActivatorOffensiveAd.Menu = Menu.ActivatorOffensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AD", "SAwarenessActivatorOffensiveAd"));
                Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveBOTRK", "Blade of the Ruined King").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveEntropy", "Entropy").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveRavenousHydra", "Ravenous Hydra").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveSwordOfTheDevine", "Sword Of The Devine").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveTiamat", "Tiamat").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveYoumuusGhostblade", "Youmuu's Ghostblade").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveMuramana", "Muramana").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveAdActive", "Active").SetValue(false)));

                Menu.ActivatorOffensiveAp.Menu = Menu.ActivatorOffensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AP", "SAwarenessActivatorOffensiveAp"));
                Menu.ActivatorOffensiveAp.MenuItems.Add(Menu.ActivatorOffensiveAp.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveApBilgewaterCutlass", "Bilgewater Cutlass").SetValue(false)));
                Menu.ActivatorOffensiveAp.MenuItems.Add(Menu.ActivatorOffensiveAp.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveApBlackfireTorch", "Blackfire Torch").SetValue(false)));
                Menu.ActivatorOffensiveAp.MenuItems.Add(Menu.ActivatorOffensiveAp.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveApDFG", "Deathfire Grasp").SetValue(false)));
                Menu.ActivatorOffensiveAp.MenuItems.Add(Menu.ActivatorOffensiveAp.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveApHextechGunblade", "Hextech Gunblade").SetValue(false)));
                Menu.ActivatorOffensiveAp.MenuItems.Add(Menu.ActivatorOffensiveAp.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveApTwinShadows", "Twin Shadows").SetValue(false)));
                Menu.ActivatorOffensiveAp.MenuItems.Add(Menu.ActivatorOffensiveAp.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveApOdynsVeil", "Odyn's Veil").SetValue(false)));
                Menu.ActivatorOffensiveAp.MenuItems.Add(Menu.ActivatorOffensiveAp.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveApActive", "Active").SetValue(false)));
                Menu.ActivatorOffensive.MenuItems.Add(Menu.ActivatorOffensive.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveKey", "Key").SetValue(new KeyBind(32, KeyBindType.Press))));
                Menu.ActivatorOffensive.MenuItems.Add(Menu.ActivatorOffensive.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveActive", "Active").SetValue(true)));
                Menu.AutoSmite.Menu = Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoSmite", "SAwarenessAutoSmite"));
                Menu.AutoSmite.MenuItems.Add(Menu.AutoSmite.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoSmiteActive", "Active").SetValue(true)));
                Menu.AutoShield.Menu = Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoShield | Not implemented", "SAwarenessAutoShield"));
                Menu.AutoShield.MenuItems.Add(Menu.AutoShield.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoShieldActive", "Active").SetValue(true)));
                Menu.AutoPot.Menu = Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoPot", "SAwarenessAutoPot"));
                tempSettings = Menu.AutoPot.AddMenuItemSettings("HealthPot",
                    "SAwarenessAutoPotHealthPot");
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoPotHealthPotPercent", "Health Percent").SetValue(new Slider(20, 99, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoPotHealthPotActive", "Active").SetValue(true)));
                tempSettings = Menu.AutoPot.AddMenuItemSettings("ManaPot",
                    "SAwarenessAutoPotManaPot");
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoPotManaPotPercent", "Mana Percent").SetValue(new Slider(20, 99, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoPotManaPotActive", "Active").SetValue(true)));
                Menu.AutoPot.MenuItems.Add(Menu.AutoPot.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoPotActive", "Active").SetValue(true)));   
                Menu.Activator.MenuItems.Add(Menu.Activator.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorActive", "Active").SetValue(true)));

                Menu.Misc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Misc", "SAwarenessMisc"));
                Menu.Misc.MenuItems.Add(Menu.Misc.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessMiscActive", "Active").SetValue(true)));
                Menu.SkinChanger.Menu = Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SkinChanger", "SAwarenessSkinChanger"));
                Menu.SkinChanger.MenuItems.Add(Menu.SkinChanger.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSkinChangerSkinName", "Skin").SetValue(new StringList(SkinChanger.GetSkinList(ObjectManager.Player.ChampionName))).DontSave()));
                Menu.SkinChanger.MenuItems.Add(Menu.SkinChanger.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSkinChangerActive", "Active").SetValue(false)));
                Menu.SafeMovement.Menu = Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SafeMovement", "SAwarenessSafeMovement"));
                Menu.SafeMovement.MenuItems.Add(Menu.SafeMovement.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSafeMovementBlockIntervall", "Block Interval").SetValue(new Slider(20, 1000, 0))));
                Menu.SafeMovement.MenuItems.Add(Menu.SafeMovement.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSafeMovementActive", "Active").SetValue(true)));
                Menu.AutoLevler.Menu = Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoLevler", "SAwarenessAutoLevler"));
                tempSettings = Menu.AutoLevler.AddMenuItemSettings("Priority",
                    "SAwarenessAutoLevlerPriority");
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerPrioritySliderQ", "Q").SetValue(new Slider(0, 3, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerPrioritySliderW", "W").SetValue(new Slider(0, 3, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerPrioritySliderE", "E").SetValue(new Slider(0, 3, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerPrioritySliderR", "R").SetValue(new Slider(0, 3, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerPriorityActive", "Active").SetValue(false).DontSave()));
                tempSettings = Menu.AutoLevler.AddMenuItemSettings("Sequence | not implemented",
                    "SAwarenessAutoLevlerSequence");
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerSequenceLoadChampion", "Load Champion").SetValue(false).DontSave()));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerSequenceActive", "Active").SetValue(false).DontSave()));
                Menu.AutoLevler.MenuItems.Add(Menu.AutoLevler.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerMode", "Mode").SetValue(new StringList(new string[] { "Sequence", "Priority" }))));
                Menu.AutoLevler.MenuItems.Add(Menu.AutoLevler.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerActive", "Active").SetValue(true)));

                menu.AddItem(new LeagueSharp.Common.MenuItem("By Screeder", "By Screeder V0.8"));
                menu.AddToMainMenu();
            }
            catch (Exception ex)
            {
                
                throw;
            }            
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                CreateMenu();
                Game.PrintChat("SAwareness loaded!");
                Game.OnGameUpdate += GameOnOnGameUpdate;
            }
            catch (Exception e)
            {
                Console.WriteLine("SAwareness: " +  e.ToString());
            }
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            Type classType = typeof(Menu);
            BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo[] fields = classType.GetFields(flags);
            foreach (FieldInfo p in fields)
            {
                var item = (Menu.MenuItemSettings)p.GetValue(null);
                if (item.GetActive() == false && item.Item != null)
                {
                    item.Item = null;
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
                    var subType = queue.Dequeue();
                    foreach (var subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface)) continue;

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    var typeProperties = subType.GetProperties(
                        BindingFlags.FlattenHierarchy
                        | BindingFlags.Public
                        | BindingFlags.Instance);

                    var newPropertyInfos = typeProperties
                        .Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }

            return type.GetProperties(BindingFlags.Static | BindingFlags.Public);
        }
    }
}
