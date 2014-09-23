using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using LeagueSharp;
using LeagueSharp.Common;
using SAwareness.Properties;
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
        public static MenuItemSettings RecallDetector = new MenuItemSettings(typeof(SAwareness.RecallDetector)); //Works
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
        public static MenuItemSettings BushRevealer = new MenuItemSettings(typeof(SAwareness.BushRevealer)); //Works        
        public static MenuItemSettings InvisibleRevealer = new MenuItemSettings(typeof(SAwareness.InvisibleRevealer)); //Not implemented   
        public static MenuItemSettings SkinChanger = new MenuItemSettings(typeof(SAwareness.SkinChanger)); //Works
        public static MenuItemSettings AutoSmite = new MenuItemSettings(typeof(SAwareness.AutoSmite)); //Works
        public static MenuItemSettings AutoPot = new MenuItemSettings(typeof(SAwareness.AutoPot));
        public static MenuItemSettings SafeMovement = new MenuItemSettings(typeof(SAwareness.SafeMovement));
        public static MenuItemSettings AutoShield = new MenuItemSettings(typeof(SAwareness.AutoShield));
        public static MenuItemSettings Misc = new MenuItemSettings();
        public static MenuItemSettings MoveToMouse = new MenuItemSettings(typeof(SAwareness.MoveToMouse));
        public static MenuItemSettings SurrenderVote = new MenuItemSettings(typeof(SAwareness.SurrenderVote));
        public static MenuItemSettings AutoLatern = new MenuItemSettings(typeof(SAwareness.AutoLatern));
        public static MenuItemSettings DisconnectDetector = new MenuItemSettings(typeof(SAwareness.DisconnectDetector));          
        public static MenuItemSettings Activator = new MenuItemSettings(typeof(SAwareness.Activator));
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
        //public static MenuItemSettings  = new MenuItemSettings();
    }

    class Program
    {
        static void Main(string[] args)
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
                Menu.Timers.MenuItems.Add(Menu.Timers.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTimersLocalPing", "Local Ping").SetValue(true)));
                Menu.Timers.MenuItems.Add(Menu.Timers.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTimersChatChoice", "Chat Choice").SetValue(new StringList(new string[] { "None", "Local", "Server" }))));
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
                Menu.ExperienceRange.MenuItems.Add(Menu.ExperienceRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessExperienceRangeMode", "Mode").SetValue(new StringList(new string[] { "Me", "Enemy", "Both" }))));
                Menu.ExperienceRange.MenuItems.Add(Menu.ExperienceRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessExperienceRangeActive", "Active").SetValue(false)));
                Menu.AttackRange.Menu = Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AttackRange", "SAwarenessAttackRange"));
                Menu.AttackRange.MenuItems.Add(Menu.AttackRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAttackRangeMode", "Mode").SetValue(new StringList(new string[] { "Me", "Enemy", "Both" }))));
                Menu.AttackRange.MenuItems.Add(Menu.AttackRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAttackRangeActive", "Active").SetValue(false)));
                Menu.TowerRange.Menu = Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("TowerRange", "SAwarenessTowerRange"));
                Menu.TowerRange.MenuItems.Add(Menu.TowerRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTowerRangeMode", "Mode").SetValue(new StringList(new string[] { "Me", "Enemy", "Both" }))));
                Menu.TowerRange.MenuItems.Add(Menu.TowerRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTowerRangeActive", "Active").SetValue(false)));
                Menu.SpellQRange.Menu = Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SpellQRange", "SAwarenessSpellQRange"));
                Menu.SpellQRange.MenuItems.Add(Menu.SpellQRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSpellQRangeMode", "Mode").SetValue(new StringList(new string[] { "Me", "Enemy", "Both" }))));
                Menu.SpellQRange.MenuItems.Add(Menu.SpellQRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSpellQRangeActive", "Active").SetValue(false)));
                Menu.SpellWRange.Menu = Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SpellWRange", "SAwarenessSpellWRange"));
                Menu.SpellWRange.MenuItems.Add(Menu.SpellWRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSpellWRangeMode", "Mode").SetValue(new StringList(new string[] { "Me", "Enemy", "Both" }))));
                Menu.SpellWRange.MenuItems.Add(Menu.SpellWRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSpellWRangeActive", "Active").SetValue(false)));
                Menu.SpellERange.Menu = Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SpellERange", "SAwarenessSpellERange"));
                Menu.SpellERange.MenuItems.Add(Menu.SpellERange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSpellERangeMode", "Mode").SetValue(new StringList(new string[] { "Me", "Enemy", "Both" }))));
                Menu.SpellERange.MenuItems.Add(Menu.SpellERange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSpellERangeActive", "Active").SetValue(false)));
                Menu.SpellRRange.Menu = Menu.Range.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SpellRRange", "SAwarenessSpellRRange"));
                Menu.SpellRRange.MenuItems.Add(Menu.SpellRRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSpellRRangeMode", "Mode").SetValue(new StringList(new string[] { "Me", "Enemy", "Both" }))));
                Menu.SpellRRange.MenuItems.Add(Menu.SpellRRange.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSpellRRangeActive", "Active").SetValue(false)));
                Menu.Range.MenuItems.Add(Menu.Range.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRangesActive", "Active").SetValue(false)));

                Menu.Tracker.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Tracker", "SAwarenessTracker"));
                Menu.WaypointTracker.Menu = Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("WaypointTracker", "SAwarenessWaypointTracker"));
                Menu.WaypointTracker.MenuItems.Add(Menu.WaypointTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessWaypointTrackerActive", "Active").SetValue(false)));
                Menu.DestinationTracker.Menu = Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("DestinationTracker", "SAwarenessDestinationTracker"));
                Menu.DestinationTracker.MenuItems.Add(Menu.DestinationTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessDestinationTrackerActive", "Active").SetValue(false)));
                Menu.CloneTracker.Menu = Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("CloneTracker", "SAwarenessCloneTracker"));
                Menu.CloneTracker.MenuItems.Add(Menu.CloneTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessCloneTrackerActive", "Active").SetValue(false)));
                Menu.UiTracker.Menu = Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("UITracker", "SAwarenessUITracker"));
                Menu.UiTracker.MenuItems.Add(Menu.UiTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessItemPanelActive", "ItemPanel").SetValue(false)));
                Menu.UiTracker.MenuItems.Add(Menu.UiTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerScale", "Scale").SetValue(new Slider(100, 100, 0))));
                tempSettings = Menu.UiTracker.AddMenuItemSettings("Enemy Tracker",
                    "SAwarenessUITrackerEnemyTracker");
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerEnemyTrackerXPos", "X Position").SetValue(new Slider(-1, 10000, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerEnemyTrackerYPos", "Y Position").SetValue(new Slider(-1, 10000, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerEnemyTrackerMode", "Mode").SetValue(new StringList(new string[] { "Side", "Unit", "Both" }))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerEnemyTrackerHeadMode", "Over Head Mode").SetValue(new StringList(new string[] { "Small", "Big" }))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerEnemyTrackerActive", "Active").SetValue(false)));
                tempSettings = Menu.UiTracker.AddMenuItemSettings("Ally Tracker",
                    "SAwarenessUITrackerAllyTracker");
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerAllyTrackerXPos", "X Position").SetValue(new Slider(-1, 10000, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerAllyTrackerYPos", "Y Position").SetValue(new Slider(-1, 10000, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerAllyTrackerMode", "Mode").SetValue(new StringList(new string[] { "Side", "Unit", "Both" }))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerAllyTrackerHeadMode", "Over Head Mode").SetValue(new StringList(new string[] { "Small", "Big" }))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerAllyTrackerActive", "Active").SetValue(false)));
                //Menu.UiTracker.MenuItems.Add(Menu.UiTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerCameraMoveActive", "Camera move active").SetValue(false)));
                Menu.UiTracker.MenuItems.Add(Menu.UiTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerPingActive", "Ping active").SetValue(false)));
                Menu.UiTracker.MenuItems.Add(Menu.UiTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUITrackerActive", "Active").SetValue(false)));
                Menu.UimTracker.Menu = Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("UIMTracker", "SAwarenessUIMTracker"));
                Menu.UimTracker.MenuItems.Add(Menu.UimTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUIMTrackerScale", "Scale").SetValue(new Slider(100, 100, 0))));
                Menu.UimTracker.MenuItems.Add(Menu.UimTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessUIMTrackerActive", "Active").SetValue(false)));
                Menu.SsCaller.Menu = Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SSCaller", "SAwarenessSSCaller"));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerPingTimes", "Ping Times").SetValue(new Slider(0, 5, 0))));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerPingType", "Ping Type").SetValue(new StringList(new string[] { "Normal", "Danger", "EnemyMissing", "OnMyWay", "Fallback", "AssistMe" }))));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerLocalPing", "Local Ping").SetValue(false)));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSsCallerChatChoice", "Chat Choice").SetValue(new StringList(new string[] { "None", "Local", "Server" }))));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerDisableTime", "Disable Time").SetValue(new Slider(20, 180, 1))));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerActive", "Active").SetValue(false)));
                Menu.Tracker.MenuItems.Add(Menu.Tracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTrackerActive", "Active").SetValue(false)));

                Menu.Detector.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Detector", "SAwarenessDetector"));
                Menu.VisionDetector.Menu = Menu.Detector.Menu.AddSubMenu(new LeagueSharp.Common.Menu("VisionDetector", "SAwarenessVisionDetector"));
                Menu.VisionDetector.MenuItems.Add(Menu.VisionDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessVisionDetectorActive", "Active").SetValue(false)));
                Menu.RecallDetector.Menu = Menu.Detector.Menu.AddSubMenu(new LeagueSharp.Common.Menu("RecallDetector", "SAwarenessRecallDetector"));
                Menu.RecallDetector.MenuItems.Add(Menu.RecallDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRecallDetectorPingTimes", "Ping Times").SetValue(new Slider(0, 5, 0))));
                Menu.RecallDetector.MenuItems.Add(Menu.RecallDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRecallDetectorLocalPing", "Local Ping").SetValue(true)));
                Menu.RecallDetector.MenuItems.Add(Menu.RecallDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRecallDetectorChatChoice", "Chat Choice").SetValue(new StringList(new string[] { "None", "Local", "Server" }))));
                Menu.RecallDetector.MenuItems.Add(Menu.RecallDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRecallDetectorMode", "Mode").SetValue(new StringList(new string[] { "Chat", "CDTracker", "Both" }))));
                Menu.RecallDetector.MenuItems.Add(Menu.RecallDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRecallDetectorActive", "Active").SetValue(false)));
                Menu.Detector.MenuItems.Add(Menu.Detector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessDetectorActive", "Active").SetValue(false)));

                Menu.Ganks.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Ganks", "SAwarenessGanks"));
                Menu.GankTracker.Menu = Menu.Ganks.Menu.AddSubMenu(new LeagueSharp.Common.Menu("GankTracker", "SAwarenessGankTracker"));
                Menu.GankTracker.MenuItems.Add(Menu.GankTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGankTrackerActive", "Active").SetValue(false)));
                Menu.GankDetector.Menu = Menu.Ganks.Menu.AddSubMenu(new LeagueSharp.Common.Menu("GankDetector", "SAwarenessGankDetector"));
                Menu.GankDetector.MenuItems.Add(Menu.GankDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGankDetectorPingTimes", "Ping Times").SetValue(new Slider(0, 5, 0))));
                Menu.GankDetector.MenuItems.Add(Menu.GankDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGankDetectorPingType", "Ping Type").SetValue(new StringList(new string[] { "Normal", "Danger", "EnemyMissing", "OnMyWay", "Fallback", "AssistMe" }))));
                Menu.GankDetector.MenuItems.Add(Menu.GankDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGankDetectorLocalPing", "Local Ping").SetValue(true)));
                Menu.GankDetector.MenuItems.Add(Menu.GankDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGankDetectorChatChoice", "Chat Choice").SetValue(new StringList(new string[] { "None", "Local", "Server" }))));
                Menu.GankDetector.MenuItems.Add(Menu.GankDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGankDetectorTrackRange", "Track Range").SetValue(new Slider(1, 10000, 1))));
                Menu.GankDetector.MenuItems.Add(Menu.GankDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGankDetectorActive", "Active").SetValue(false)));
                Menu.Ganks.MenuItems.Add(Menu.Ganks.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGanksActive", "Active").SetValue(false)));

                Menu.Health.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Object Health", "SAwarenessObjectHealth"));
                Menu.TowerHealth.Menu = Menu.Health.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Tower Health", "SAwarenessTowerHealth"));
                Menu.TowerHealth.MenuItems.Add(Menu.TowerHealth.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTowerHealthActive", "Active").SetValue(false)));
                Menu.InhibitorHealth.Menu = Menu.Health.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Inhibitor Health", "SAwarenessInhibitorHealth"));
                Menu.InhibitorHealth.MenuItems.Add(Menu.InhibitorHealth.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessInhibitorHealthActive", "Active").SetValue(false)));
                Menu.Health.MenuItems.Add(Menu.Health.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessHealthMode", "Mode").SetValue(new StringList(new string[] { "Percent", "Normal" }))));
                Menu.Health.MenuItems.Add(Menu.Health.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessHealthActive", "Active").SetValue(false)));

                Menu.Wards.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Wards", "SAwarenessWards"));
                Menu.WardCorrector.Menu = Menu.Wards.Menu.AddSubMenu(new LeagueSharp.Common.Menu("WardCorrector", "SAwarenessWardCorrector"));
                Menu.WardCorrector.MenuItems.Add(Menu.WardCorrector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessWardCorrectorActive", "Active").SetValue(false)));
                Menu.BushRevealer.Menu = Menu.Wards.Menu.AddSubMenu(new LeagueSharp.Common.Menu("BushRevealer", "SAwarenessBushRevealer"));
                Menu.BushRevealer.MenuItems.Add(Menu.BushRevealer.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessBushRevealerKey", "Key").SetValue(new KeyBind(32, KeyBindType.Press))));
                Menu.BushRevealer.MenuItems.Add(Menu.BushRevealer.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessBushRevealerActive", "Active").SetValue(false)));
                Menu.BushRevealer.MenuItems.Add(Menu.BushRevealer.Menu.AddItem(new LeagueSharp.Common.MenuItem("By Beaving & Blm95", "By Beaving & Blm95")));
                Menu.InvisibleRevealer.Menu = Menu.Wards.Menu.AddSubMenu(new LeagueSharp.Common.Menu("InvisibleRevealer", "SAwarenessInvisibleRevealer"));
                Menu.InvisibleRevealer.MenuItems.Add(Menu.InvisibleRevealer.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessInvisibleRevealerMode", "Mode").SetValue(new StringList(new string[] { "Manual", "Automatic" }))));
                Menu.InvisibleRevealer.MenuItems.Add(Menu.InvisibleRevealer.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessInvisibleRevealerKey", "Key").SetValue(new KeyBind(32, KeyBindType.Press))));
                Menu.InvisibleRevealer.MenuItems.Add(Menu.InvisibleRevealer.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessInvisibleRevealerActive", "Active").SetValue(false)));
                Menu.Wards.MenuItems.Add(Menu.Wards.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessWardsActive", "Active").SetValue(false)));

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
                Menu.ActivatorAutoSummonerSpellExhaust.Menu = Menu.ActivatorAutoSummonerSpell.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Auto Exhaust", "SAwarenessActivatorAutoSummonerSpellExhaust"));
                Menu.ActivatorAutoSummonerSpellExhaust.MenuItems.Add(Menu.ActivatorAutoSummonerSpellExhaust.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellExhaustAutoCast", "AutoCast On Key").SetValue(new KeyBind(32, KeyBindType.Press))));
                Menu.ActivatorAutoSummonerSpellExhaust.MenuItems.Add(Menu.ActivatorAutoSummonerSpellExhaust.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellExhaustMinEnemies", "Min Enemies").SetValue(new Slider(3, 5, 1))));
                Menu.ActivatorAutoSummonerSpellExhaust.MenuItems.Add(Menu.ActivatorAutoSummonerSpellExhaust.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellExhaustAllyPercent", "Ally Percent").SetValue(new Slider(20, 100, 1))));
                Menu.ActivatorAutoSummonerSpellExhaust.MenuItems.Add(Menu.ActivatorAutoSummonerSpellExhaust.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellExhaustSelfPercent", "Self Percent").SetValue(new Slider(20, 100, 1))));
                Menu.ActivatorAutoSummonerSpellExhaust.MenuItems.Add(Menu.ActivatorAutoSummonerSpellExhaust.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellExhaustActive", "Active").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.Menu = Menu.ActivatorAutoSummonerSpell.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Auto Cleanse", "SAwarenessActivatorAutoSummonerSpellCleanse"));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseStun", "Stun").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseSilence", "Silence").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseTaunt", "Taunt").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseFear", "Fear").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseCharm", "Charm").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseBlind", "Blind").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseDisarm", "Disarm").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseSlow", "Slow").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseCombatDehancer", "Combat Dehancer").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseSnare", "Snare").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellCleansePoison", "Posion").SetValue(false)));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseMinSpells", "Min Spells").SetValue(new Slider(2, 10, 1))));
                Menu.ActivatorAutoSummonerSpellCleanse.MenuItems.Add(Menu.ActivatorAutoSummonerSpellCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorAutoSummonerSpellCleanseActive", "Active").SetValue(false)));
                Menu.AutoSmite.Menu = Menu.ActivatorAutoSummonerSpell.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoSmite", "SAwarenessAutoSmite"));
                Menu.AutoSmite.MenuItems.Add(Menu.AutoSmite.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoSmiteSmallCampsActive", "Smite Small Camps").SetValue(false)));
                Menu.AutoSmite.MenuItems.Add(Menu.AutoSmite.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoSmiteKeyActive", "Key").SetValue(new KeyBind(78, KeyBindType.Toggle))));
                Menu.AutoSmite.MenuItems.Add(Menu.AutoSmite.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoSmiteActive", "Active").SetValue(false)));

                Menu.ActivatorOffensive.Menu = Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Offensive Items", "SAwarenessActivatorOffensive"));
                Menu.ActivatorOffensiveAd.Menu = Menu.ActivatorOffensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AD", "SAwarenessActivatorOffensiveAd"));
                Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveAdBOTRK", "Blade of the Ruined King").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveAdEntropy", "Entropy").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveAdRavenousHydra", "Ravenous Hydra").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveAdSwordOfTheDevine", "Sword Of The Devine").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveAdTiamat", "Tiamat").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveAdYoumuusGhostblade", "Youmuu's Ghostblade").SetValue(false)));
                //Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveAdMuramana", "Muramana").SetValue(false)));
                Menu.ActivatorOffensiveAd.MenuItems.Add(Menu.ActivatorOffensiveAd.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveAdActive", "Active").SetValue(false)));

                Menu.ActivatorOffensiveAp.Menu = Menu.ActivatorOffensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AP", "SAwarenessActivatorOffensiveAp"));
                Menu.ActivatorOffensiveAp.MenuItems.Add(Menu.ActivatorOffensiveAp.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveApBilgewaterCutlass", "Bilgewater Cutlass").SetValue(false)));
                Menu.ActivatorOffensiveAp.MenuItems.Add(Menu.ActivatorOffensiveAp.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveApBlackfireTorch", "Blackfire Torch").SetValue(false)));
                Menu.ActivatorOffensiveAp.MenuItems.Add(Menu.ActivatorOffensiveAp.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveApDFG", "Deathfire Grasp").SetValue(false)));
                Menu.ActivatorOffensiveAp.MenuItems.Add(Menu.ActivatorOffensiveAp.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveApHextechGunblade", "Hextech Gunblade").SetValue(false)));
                Menu.ActivatorOffensiveAp.MenuItems.Add(Menu.ActivatorOffensiveAp.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveApTwinShadows", "Twin Shadows").SetValue(false)));
                //Menu.ActivatorOffensiveAp.MenuItems.Add(Menu.ActivatorOffensiveAp.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveApOdynsVeil", "Odyn's Veil").SetValue(false)));
                Menu.ActivatorOffensiveAp.MenuItems.Add(Menu.ActivatorOffensiveAp.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveApActive", "Active").SetValue(false)));
                Menu.ActivatorOffensive.MenuItems.Add(Menu.ActivatorOffensive.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveKey", "Key").SetValue(new KeyBind(32, KeyBindType.Press))));
                Menu.ActivatorOffensive.MenuItems.Add(Menu.ActivatorOffensive.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorOffensiveActive", "Active").SetValue(false)));

                Menu.ActivatorDefensive.Menu = Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Defensive Items", "SAwarenessActivatorDefensive"));
                Menu.ActivatorDefensiveCleanseConfig.Menu = Menu.ActivatorDefensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Cleanse Config", "SAwarenessActivatorDefensiveCleanseConfig"));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseConfigStun", "Stun").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseConfigSilence", "Silence").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseConfigTaunt", "Taunt").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseConfigFear", "Fear").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseConfigCharm", "Charm").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseConfigBlind", "Blind").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseConfigDisarm", "Disarm").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseConfigSuppress", "Suppress").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseConfigSlow", "Slow").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseConfigCombatDehancer", "Combat Dehancer").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseConfigSnare", "Snare").SetValue(false)));
                Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseConfigPoison", "Posion").SetValue(false)));
                //Menu.ActivatorDefensiveCleanseConfig.MenuItems.Add(Menu.ActivatorDefensiveCleanseConfig.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseConfigActive", "Active").SetValue(false)));

                Menu.ActivatorDefensiveSelfShield.Menu = Menu.ActivatorDefensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Self Shield | Not implemented", "SAwarenessActivatorDefensiveSelfShield"));
                Menu.ActivatorDefensiveSelfShield.MenuItems.Add(Menu.ActivatorDefensiveSelfShield.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveSelfShieldSeraphEmbrace", "Seraph Embrace").SetValue(false)));
                Menu.ActivatorDefensiveSelfShield.MenuItems.Add(Menu.ActivatorDefensiveSelfShield.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveSelfShieldOhmwrecker", "Ohmwrecker").SetValue(false)));
                Menu.ActivatorDefensiveSelfShield.MenuItems.Add(Menu.ActivatorDefensiveSelfShield.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveSelfShieldActive", "Active").SetValue(false)));

                Menu.ActivatorDefensiveWoogletZhonya.Menu = Menu.ActivatorDefensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Wooglet/Zhonya | Not implemented", "SAwarenessActivatorDefensiveWoogletZhonya"));
                Menu.ActivatorDefensiveWoogletZhonya.MenuItems.Add(Menu.ActivatorDefensiveWoogletZhonya.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveWoogletWooglet", "Wooglet").SetValue(false)));
                Menu.ActivatorDefensiveWoogletZhonya.MenuItems.Add(Menu.ActivatorDefensiveWoogletZhonya.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveWoogletZhonya", "Zhonya").SetValue(false)));
                Menu.ActivatorDefensiveWoogletZhonya.MenuItems.Add(Menu.ActivatorDefensiveWoogletZhonya.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveWoogletZhonyaActive", "Active").SetValue(false)));

                Menu.ActivatorDefensiveDebuffSlow.Menu = Menu.ActivatorDefensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Slow Enemy", "SAwarenessActivatorDefensiveDebuffSlow"));
                Menu.ActivatorDefensiveDebuffSlow.MenuItems.Add(Menu.ActivatorDefensiveDebuffSlow.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveDebuffSlowRanduins", "Randuins Omen").SetValue(false)));
                Menu.ActivatorDefensiveDebuffSlow.MenuItems.Add(Menu.ActivatorDefensiveDebuffSlow.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveDebuffSlowConfigRanduins", "Enemy Count Randuins").SetValue(new Slider(2, 5, 1))));
                Menu.ActivatorDefensiveDebuffSlow.MenuItems.Add(Menu.ActivatorDefensiveDebuffSlow.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveDebuffSlowFrostQueensClaim", "Frost Queens Claim").SetValue(false)));
                Menu.ActivatorDefensiveDebuffSlow.MenuItems.Add(Menu.ActivatorDefensiveDebuffSlow.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveDebuffSlowConfigFrostQueensClaim", "Enemy Count FQC").SetValue(new Slider(2, 5, 1))));
                Menu.ActivatorDefensiveDebuffSlow.MenuItems.Add(Menu.ActivatorDefensiveDebuffSlow.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveDebuffSlowActive", "Active").SetValue(false)));

                Menu.ActivatorDefensiveCleanseSelf.Menu = Menu.ActivatorDefensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Cleanse me", "SAwarenessActivatorDefensiveCleanseSelf"));
                Menu.ActivatorDefensiveCleanseSelf.MenuItems.Add(Menu.ActivatorDefensiveCleanseSelf.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseSelfQSS", "QSS").SetValue(false)));
                Menu.ActivatorDefensiveCleanseSelf.MenuItems.Add(Menu.ActivatorDefensiveCleanseSelf.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseSelfMercurialScimitar", "Mercurial Scimitar").SetValue(false)));
                Menu.ActivatorDefensiveCleanseSelf.MenuItems.Add(Menu.ActivatorDefensiveCleanseSelf.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseSelfDervishBlade", "Dervish Blade").SetValue(false)));
                Menu.ActivatorDefensiveCleanseSelf.MenuItems.Add(Menu.ActivatorDefensiveCleanseSelf.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseSelfConfigMinSpells", "Min Spells").SetValue(new Slider(2, 10, 1))));
                Menu.ActivatorDefensiveCleanseSelf.MenuItems.Add(Menu.ActivatorDefensiveCleanseSelf.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveCleanseSelfActive", "Active").SetValue(false)));

                Menu.ActivatorDefensiveShieldBoost.Menu = Menu.ActivatorDefensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Shield/Boost | Not implemented", "SAwarenessActivatorDefensiveShieldBoost"));
                Menu.ActivatorDefensiveShieldBoost.MenuItems.Add(Menu.ActivatorDefensiveShieldBoost.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveShieldBoostLocketofIronSolari", "Locket of Iron Solari").SetValue(false)));
                Menu.ActivatorDefensiveShieldBoost.MenuItems.Add(Menu.ActivatorDefensiveShieldBoost.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveShieldBoostTalismanofAscension", "Talisman of Ascension").SetValue(false)));
                Menu.ActivatorDefensiveShieldBoost.MenuItems.Add(Menu.ActivatorDefensiveShieldBoost.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveShieldBoostFaceOfTheMountain", "Face of the Mountain").SetValue(false)));
                Menu.ActivatorDefensiveShieldBoost.MenuItems.Add(Menu.ActivatorDefensiveShieldBoost.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveShieldBoostGuardiansHorn", "Guardians Horn").SetValue(false)));
                Menu.ActivatorDefensiveShieldBoost.MenuItems.Add(Menu.ActivatorDefensiveShieldBoost.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveShieldBoostConfigHealth", "Health Percent").SetValue(new Slider(20, 100, 1))));
                Menu.ActivatorDefensiveShieldBoost.MenuItems.Add(Menu.ActivatorDefensiveShieldBoost.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveShieldBoostActive", "Active").SetValue(false)));

                Menu.ActivatorDefensiveMikaelCleanse.Menu = Menu.ActivatorDefensive.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Mikael Cleanse | Not implemented", "SAwarenessActivatorDefensiveMikaelCleanse"));
                Menu.ActivatorDefensiveMikaelCleanse.MenuItems.Add(Menu.ActivatorDefensiveMikaelCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveMikaelCleanseConfigAlly", "On Allies").SetValue(false)));
                Menu.ActivatorDefensiveMikaelCleanse.MenuItems.Add(Menu.ActivatorDefensiveMikaelCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveMikaelCleanseConfigAllyHealth", "Ally Health").SetValue(new Slider(20, 100, 1))));
                Menu.ActivatorDefensiveMikaelCleanse.MenuItems.Add(Menu.ActivatorDefensiveMikaelCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveMikaelCleanseConfigSelfHealth", "Self Healh").SetValue(new Slider(20, 100, 1))));
                Menu.ActivatorDefensiveMikaelCleanse.MenuItems.Add(Menu.ActivatorDefensiveMikaelCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveMikaelCleanseConfigMinSpells", "Min Spells").SetValue(new Slider(2, 10, 1))));
                Menu.ActivatorDefensiveMikaelCleanse.MenuItems.Add(Menu.ActivatorDefensiveMikaelCleanse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveShieldBoostActive", "Active").SetValue(false)));
                Menu.ActivatorDefensive.MenuItems.Add(Menu.ActivatorDefensive.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorDefensiveActive", "Active").SetValue(false)));

                Menu.AutoShield.Menu = Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoShield | Not implemented", "SAwarenessAutoShield"));
                Menu.AutoShield.MenuItems.Add(Menu.AutoShield.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoShieldActive", "Active").SetValue(false)));
                Menu.AutoPot.Menu = Menu.Activator.Menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoPot", "SAwarenessAutoPot"));
                tempSettings = Menu.AutoPot.AddMenuItemSettings("HealthPot",
                    "SAwarenessAutoPotHealthPot");
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoPotHealthPotPercent", "Health Percent").SetValue(new Slider(20, 99, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoPotHealthPotActive", "Active").SetValue(false)));
                tempSettings = Menu.AutoPot.AddMenuItemSettings("ManaPot",
                    "SAwarenessAutoPotManaPot");
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoPotManaPotPercent", "Mana Percent").SetValue(new Slider(20, 99, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoPotManaPotActive", "Active").SetValue(false)));
                Menu.AutoPot.MenuItems.Add(Menu.AutoPot.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoPotActive", "Active").SetValue(false)));
                Menu.Activator.MenuItems.Add(Menu.Activator.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessActivatorActive", "Active").SetValue(false)));

                Menu.Misc.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Misc", "SAwarenessMisc"));
                Menu.Misc.MenuItems.Add(Menu.Misc.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessMiscActive", "Active").SetValue(false)));
                Menu.SkinChanger.Menu = Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SkinChanger", "SAwarenessSkinChanger"));
                Menu.SkinChanger.MenuItems.Add(Menu.SkinChanger.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSkinChangerSkinName", "Skin").SetValue(new StringList(SkinChanger.GetSkinList(ObjectManager.Player.ChampionName))).DontSave()));
                Menu.SkinChanger.MenuItems.Add(Menu.SkinChanger.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSkinChangerActive", "Active").SetValue(false)));
                Menu.SafeMovement.Menu = Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SafeMovement", "SAwarenessSafeMovement"));
                Menu.SafeMovement.MenuItems.Add(Menu.SafeMovement.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSafeMovementBlockIntervall", "Block Interval").SetValue(new Slider(20, 1000, 0))));
                Menu.SafeMovement.MenuItems.Add(Menu.SafeMovement.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSafeMovementActive", "Active").SetValue(false)));
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
                Menu.AutoLevler.MenuItems.Add(Menu.AutoLevler.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerActive", "Active").SetValue(false)));
                Menu.MoveToMouse.Menu = Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Move To Mouse", "SAwarenessMoveToMouse"));
                Menu.MoveToMouse.MenuItems.Add(Menu.MoveToMouse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessMoveToMouseKey", "Key").SetValue(new KeyBind(90, KeyBindType.Press))));
                Menu.MoveToMouse.MenuItems.Add(Menu.MoveToMouse.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessMoveToMouseActive", "Active").SetValue(false)));
                Menu.SurrenderVote.Menu = Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Surrender Vote", "SAwarenessSurrenderVote"));
                Menu.SurrenderVote.MenuItems.Add(Menu.SurrenderVote.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSurrenderVoteChatChoice", "Chat Choice").SetValue(new StringList(new string[] { "None", "Local", "Server" }))));
                Menu.SurrenderVote.MenuItems.Add(Menu.SurrenderVote.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSurrenderVoteActive", "Active").SetValue(false)));
                Menu.AutoLatern.Menu = Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Auto Latern", "SAwarenessAutoLatern"));
                Menu.AutoLatern.MenuItems.Add(Menu.AutoLatern.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLaternKey", "Key").SetValue(new KeyBind(84, KeyBindType.Press))));
                Menu.AutoLatern.MenuItems.Add(Menu.AutoLatern.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLaternActive", "Active").SetValue(false)));
                Menu.DisconnectDetector.Menu = Menu.Misc.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Disconnect Detector", "SAwarenessDisconnectDetector"));
                Menu.DisconnectDetector.MenuItems.Add(Menu.DisconnectDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessDisconnectDetectorChatChoice", "Chat Choice").SetValue(new StringList(new string[] { "None", "Local", "Server" }))));
                Menu.DisconnectDetector.MenuItems.Add(Menu.DisconnectDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessDisconnectDetectorActive", "Active").SetValue(false)));

                menu.AddItem(new LeagueSharp.Common.MenuItem("By Screeder", "By Screeder V0.84"));
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
                Console.WriteLine("SAwareness: " + e.ToString());
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

