using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness
{
    class GankPotentialTracker
    {
        readonly Dictionary<Obj_AI_Hero, double> Enemies = new Dictionary<Obj_AI_Hero, double>();

        public GankPotentialTracker()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    Enemies.Add(hero, 0);
                }
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~GankPotentialTracker()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Menu.GankTracker.GetActive();
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            foreach (var enemy in Enemies)
            {
                double dmg = 0;
                try { dmg += DamageLib.getDmg(enemy.Key, DamageLib.SpellType.Q); } catch (InvalidSpellTypeException ex){}
                try { dmg += DamageLib.getDmg(enemy.Key, DamageLib.SpellType.W); } catch (InvalidSpellTypeException ex){}
                try { dmg += DamageLib.getDmg(enemy.Key, DamageLib.SpellType.E); } catch (InvalidSpellTypeException ex){}
                try { dmg += DamageLib.getDmg(enemy.Key, DamageLib.SpellType.R); } catch (InvalidSpellTypeException ex){}
                try { dmg += DamageLib.getDmg(enemy.Key, DamageLib.SpellType.AD); } catch (InvalidSpellTypeException ex){}
                Enemies[enemy.Key] = dmg;
            } 
        }

        void Drawing_OnDraw(EventArgs args)
        {
            if(!IsActive())
                return;
            Vector2 myPos = Drawing.WorldToScreen(ObjectManager.Player.ServerPosition);
            foreach (var enemy in Enemies)
            {
                if (enemy.Key.IsDead || ObjectManager.Player.IsDead)
                    continue;
                Vector2 ePos = Drawing.WorldToScreen(enemy.Key.ServerPosition);
                if (enemy.Value > enemy.Key.Health)
                {
                    Drawing.DrawLine(myPos.X, myPos.Y, ePos.X, ePos.Y, 2.0f, System.Drawing.Color.OrangeRed);
                }
                if (enemy.Value < enemy.Key.Health)
                {
                    Drawing.DrawLine(myPos.X, myPos.Y, ePos.X, ePos.Y, 2.0f, System.Drawing.Color.GreenYellow);
                }
                else if (enemy.Key.Health / enemy.Key.MaxHealth < 0.1)
                {
                    Drawing.DrawLine(myPos.X, myPos.Y, ePos.X, ePos.Y, 2.0f, System.Drawing.Color.Red);
                }
            } 
        }
    }

    class GankDetector
    {
        static readonly Dictionary<Obj_AI_Hero, Time> Enemies = new Dictionary<Obj_AI_Hero, Time>();

        public class Time
        {
            public int VisibleTime;
            public int InvisibleTime;
            public bool Called;
        }

        public GankDetector()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
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
            return Menu.GankDetector.GetActive();
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (KeyValuePair<Obj_AI_Hero, Time> enemy in Enemies)
            {
                UpdateTime(enemy);                
            }
        }

        void HandleGank(KeyValuePair<Obj_AI_Hero, Time> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            if (enemy.Value.InvisibleTime > 5)
            {
                if (!enemy.Value.Called && hero.IsValid && hero.IsVisible && Vector3.Distance(ObjectManager.Player.ServerPosition, hero.ServerPosition) < 1000/**Variable*/)
                {
                    Vector3 pos = hero.ServerPosition;
                    GamePacket gPacketT = Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(pos[0], pos[1], 0, Packet.PingType.Danger));
                    gPacketT.Send();
                    //TODO: Check for Teleport etc.
                    Game.PrintChat("Gank: {0}", hero.ChampionName);
                    enemy.Value.Called = true;
                }
            }
        }

        void UpdateTime(KeyValuePair<Obj_AI_Hero, Time> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            if (!hero.IsValid)
                return;
            if (hero.IsVisible)
            {
                HandleGank(enemy);
                Enemies[hero].InvisibleTime = 0;
                Enemies[hero].VisibleTime = (int)Game.Time;
                enemy.Value.Called = false;
            }
            else
            {
                if (Enemies[hero].VisibleTime != 0)
                {
                    Enemies[hero].InvisibleTime = (int)(Game.Time - Enemies[hero].VisibleTime);
                }
                else
                {
                    Enemies[hero].InvisibleTime = 0;
                }
            }
        }
    }
}
