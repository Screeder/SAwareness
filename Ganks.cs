using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness
{
    class GankPotentialTracker //TODO: Implement it
    {
        public GankPotentialTracker()
        {

        }

        ~GankPotentialTracker()
        {

        }

        public bool IsActive()
        {
            return Menu.GankTracker.GetActive();
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
                HandleGank(enemy);
            }
        }

        void HandleGank(KeyValuePair<Obj_AI_Hero, Time> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            if (enemy.Value.InvisibleTime > 5)
            {
                if (hero.IsValid && hero.IsVisible && Vector3.Distance(ObjectManager.Player.ServerPosition, hero.ServerPosition) < 1000/**Variable*/)
                {
                    float[] pos = Drawing.WorldToScreen(hero.ServerPosition);
                    GamePacket gPacketT = Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(pos[0], pos[1], 0, Packet.PingType.Danger));
                    gPacketT.Send();
                    //TODO: Check for Teleport etc.
                    Game.Say("Gank: {0}", hero.ChampionName);
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
