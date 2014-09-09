using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace SAwareness
{
    class GankPotentialTracker
    {
        private readonly Dictionary<Obj_AI_Hero, double> Enemies = new Dictionary<Obj_AI_Hero, double>();
        private Line line;
        private bool drawActive = true;

        public GankPotentialTracker()
        {
            line = new Line(Drawing.Direct3DDevice);
            line.Antialias = true;
            line.Width = 2;
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    Enemies.Add(hero, 0);
                }
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            AppDomain.CurrentDomain.DomainUnload += delegate { Drawing_OnPreReset(new EventArgs()); };
            AppDomain.CurrentDomain.ProcessExit += delegate { Drawing_OnPreReset(new EventArgs()); };
            Drawing.OnEndScene += Drawing_OnEndScene;
        }

        ~GankPotentialTracker()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Drawing.OnPreReset -= Drawing_OnPreReset;
            Drawing.OnPostReset -= Drawing_OnPostReset;
            Drawing.OnEndScene -= Drawing_OnEndScene;
        }

        public bool IsActive()
        {
            return Menu.Ganks.GetActive() && Menu.GankTracker.GetActive();
        }

        void Drawing_OnPostReset(EventArgs args)
        {
            if (Drawing.Direct3DDevice == null || Drawing.Direct3DDevice.IsDisposed)
                return;
            line.OnResetDevice();
            drawActive = true;
        }

        void Drawing_OnPreReset(EventArgs args)
        {
            line.OnLostDevice();
            drawActive = false;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            foreach (var enemy in Enemies.ToList())
            {
                double dmg = 0;
                try
                {
                    dmg += DamageLib.getDmg(enemy.Key, DamageLib.SpellType.Q);
                }
                catch (InvalidSpellTypeException ex)
                {
                }
                try
                {
                    dmg += DamageLib.getDmg(enemy.Key, DamageLib.SpellType.W);
                }
                catch (InvalidSpellTypeException ex)
                {
                }
                try
                {
                    dmg += DamageLib.getDmg(enemy.Key, DamageLib.SpellType.E);
                }
                catch (InvalidSpellTypeException ex)
                {
                }
                try
                {
                    dmg += DamageLib.getDmg(enemy.Key, DamageLib.SpellType.R);
                }
                catch (InvalidSpellTypeException ex)
                {
                }
                try
                {
                    dmg += DamageLib.getDmg(enemy.Key, DamageLib.SpellType.AD);
                }
                catch (InvalidSpellTypeException ex)
                {
                }
                Enemies[enemy.Key] = dmg;
            }
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (!IsActive())
                return;
            Vector2 myPos = Drawing.WorldToScreen(ObjectManager.Player.ServerPosition);
            foreach (var enemy in Enemies)
            {
                if (enemy.Key.IsDead || ObjectManager.Player.IsDead)
                    continue;
                Vector2 ePos = Drawing.WorldToScreen(enemy.Key.ServerPosition);
                line.Begin();
                if (enemy.Value > enemy.Key.Health)
                {
                    //Drawing.DrawLine(myPos.X, myPos.Y, ePos.X, ePos.Y, 2.0f, System.Drawing.Color.OrangeRed);
                    //DirectXDrawer.DrawLine(line, ObjectManager.Player.ServerPosition, enemy.Key.ServerPosition,
                    //    Color.OrangeRed);
                    line.Draw(new[] { myPos, ePos }, Color.OrangeRed);
                    //DirectXDrawer.DrawLine(ObjectManager.Player.ServerPosition, enemy.Key.ServerPosition,
                    //    System.Drawing.Color.OrangeRed);
                }
                if (enemy.Value < enemy.Key.Health)
                {
                    //Drawing.DrawLine(myPos.X, myPos.Y, ePos.X, ePos.Y, 2.0f, System.Drawing.Color.GreenYellow);
                    //DirectXDrawer.DrawLine(line, ObjectManager.Player.ServerPosition, enemy.Key.ServerPosition,
                    //    Color.GreenYellow);
                    line.Draw(new[] { myPos, ePos }, Color.GreenYellow);
                    //DirectXDrawer.DrawLine(ObjectManager.Player.ServerPosition, enemy.Key.ServerPosition,
                    //    System.Drawing.Color.GreenYellow);
                }
                else if (enemy.Key.Health/enemy.Key.MaxHealth < 0.1)
                {
                    //Drawing.DrawLine(myPos.X, myPos.Y, ePos.X, ePos.Y, 2.0f, System.Drawing.Color.Red);
                    //DirectXDrawer.DrawLine(line, ObjectManager.Player.ServerPosition, enemy.Key.ServerPosition, Color.Red);
                    line.Draw(new[] { myPos, ePos }, Color.Red);
                    //DirectXDrawer.DrawLine(ObjectManager.Player.ServerPosition, enemy.Key.ServerPosition,
                    //    System.Drawing.Color.Red);
                }
               line.End();
            }
        }
    }

    class GankDetector
    {
        private static readonly Dictionary<Obj_AI_Hero, Time> Enemies = new Dictionary<Obj_AI_Hero, Time>();

        public GankDetector()
        {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    Enemies.Add(hero, new Time());
                }
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~GankDetector()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Menu.Ganks.GetActive() && Menu.GankDetector.GetActive();
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var enemy in Enemies)
            {
                UpdateTime(enemy);
            }
        }

        private void HandleGank(KeyValuePair<Obj_AI_Hero, Time> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            if (enemy.Value.InvisibleTime > 5)
            {
                if (!enemy.Value.Called && hero.IsValid && !hero.IsDead && hero.IsVisible &&
                    Vector3.Distance(ObjectManager.Player.ServerPosition, hero.ServerPosition) <
                    Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorTrackRange").GetValue<Slider>().Value)
                {
                    var pingType = Packet.PingType.Normal;
                    var t = Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorPingType").GetValue<StringList>();
                    pingType = (Packet.PingType) t.SelectedIndex + 1;
                    Vector3 pos = hero.ServerPosition;
                    GamePacket gPacketT;
                    for (int i = 0;
                        i < Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorPingTimes").GetValue<Slider>().Value;
                        i++)
                    {
                        if (Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorLocalPing").GetValue<bool>())
                        {
                            gPacketT = Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(pos[0], pos[1], 0, 0, pingType));
                            gPacketT.Process();
                        }
                        else
                        {
                            gPacketT = Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(pos[0], pos[1], 0, pingType));
                            gPacketT.Send();
                        }
                    }

                    if (Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorChatChoice").GetValue<StringList>().SelectedIndex == 1)
                    {
                        Game.PrintChat("Gank: {0}", hero.ChampionName);
                    }
                    else if (Menu.GankDetector.GetMenuItem("SAwarenessGankDetectorChatChoice").GetValue<StringList>().SelectedIndex == 2)
                    {
                        Game.Say("Gank: {0}", hero.ChampionName);
                    }
                    //TODO: Check for Teleport etc.                    
                    enemy.Value.Called = true;
                }
            }
        }

        private void UpdateTime(KeyValuePair<Obj_AI_Hero, Time> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            if (!hero.IsValid)
                return;
            if (hero.IsVisible)
            {
                HandleGank(enemy);
                Enemies[hero].InvisibleTime = 0;
                Enemies[hero].VisibleTime = (int) Game.Time;
                enemy.Value.Called = false;
            }
            else
            {
                if (Enemies[hero].VisibleTime != 0)
                {
                    Enemies[hero].InvisibleTime = (int) (Game.Time - Enemies[hero].VisibleTime);
                }
                else
                {
                    Enemies[hero].InvisibleTime = 0;
                }
            }
        }

        public class Time
        {
            public bool Called;
            public int InvisibleTime;
            public int VisibleTime;
        }
    }
}