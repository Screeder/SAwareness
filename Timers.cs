using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace SAwareness
{
    class ImmuneTimer //TODO: Maybe add Packetcheck
    {
        static readonly List<Ability> Abilities = new List<Ability>();

        public class Ability
        {
            public String SpellName;
            public int Range;
            public float Delay;
            public bool Casted;
            public int TimeCasted;
            public Obj_AI_Hero Owner;
            public Obj_AI_Hero Target;

            public Ability(string spellName, float delay)
            {
                SpellName = spellName;
                Delay = delay;
            }
        }

        public ImmuneTimer()
        {
            Abilities.Add(new Ability("zhonyas_ring_activate.troy", 2.5f)); //Zhonya
            Abilities.Add(new Ability("Aatrox_Passive_Death_Activate.troy", 3f)); //Aatrox Passive
            Abilities.Add(new Ability("LifeAura.troy", 4f)); //Zil und GA
            Abilities.Add(new Ability("nickoftime_tar.troy", 7f)); //Zil before death
            Abilities.Add(new Ability("eyeforaneye_self.troy", 2f)); // Kayle
            Abilities.Add(new Ability("UndyingRage_buf.troy", 5f)); //Tryn
            Abilities.Add(new Ability("EggTimer.troy", 6f)); //Anivia

            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~ImmuneTimer()
        {
            GameObject.OnCreate -= Obj_AI_Base_OnCreate;
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Menu.ImmuneTimer.GetActive();
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var ability in Abilities)
            {
                if ((ability.TimeCasted + ability.Delay) < Game.Time)
                {
                    ability.Casted = false;
                    ability.TimeCasted = 0;
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var ability in Abilities)
            {
                if (ability.Casted)
                {
                    Vector2 mPos = Drawing.WorldToScreen(ability.Owner.ServerPosition);
                    var endTime = ability.TimeCasted - (int)Game.Time + ability.Delay;
                    var m = (float)Math.Floor(endTime / 60);
                    var s = (float)Math.Ceiling(endTime % 60);
                    String ms = (s < 10 ? m + ":0" + s : m + ":" + s);
                    Drawing.DrawText(mPos[0], mPos[1], System.Drawing.Color.Red, ms);
                }
            }
        }

        private void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (!hero.IsEnemy)
                {
                    foreach (var ability in Abilities)
                    {
                        if (ability.SpellName == sender.Name &&
                            Vector3.Distance(sender.Position, hero.ServerPosition) <= 100 &&
                            /*variable*/ Vector3.Distance(sender.Position, ObjectManager.Player.ServerPosition) <= 4000)
                        {
                            ability.Owner = hero;
                            ability.Casted = true;
                            ability.TimeCasted = (int)Game.Time;
                        }
                    }
                }
            }
        }
    }

    public class Timers
    {
        private static Utility.Map.MapType GMapId = Utility.Map.GetMap();
        private static Inhibitor _inhibitors = null;
        private static readonly List<Relic> Relics = new List<Relic>();
        private static readonly List<Altar> Altars = new List<Altar>();
        private static readonly List<Health> Healths = new List<Health>();
        private static readonly List<JungleMob> JungleMobs = new List<JungleMob>();
        private static readonly List<JungleCamp> JungleCamps = new List<JungleCamp>();
        private static readonly List<Obj_AI_Minion> JungleMobList = new List<Obj_AI_Minion>();

        private bool drawActive = true;
        Font font;

        public class Health
        {
            public Obj_AI_Minion Obj;
            public Vector3 Position;
            public int SpawnTime;
            public int RespawnTime;
            public int NextRespawnTime;
            public bool Locked;
            public Utility.Map.MapType MapId;
            public bool Called;

            public Health(Obj_AI_Minion obj)
            {
                Obj = obj;
                if (obj != null && obj.IsValid)
                    Position = obj.Position;
                else
                    Position = new Vector3();
                SpawnTime = (int)Game.Time;
                RespawnTime = 40;
                NextRespawnTime = 0;
                Locked = false;
                MapId = Utility.Map.MapType.HowlingAbyss;
                Called = false;
            }
        }

        public class Inhibitor
        {
            public Obj_BarracksDampener Obj;
            public int SpawnTime;
            public int RespawnTime;
            public int NextRespawnTime;
            public bool Locked;
            public List<Inhibitor> Inhibitors;
            public bool Called;

            public Inhibitor()
            {
                Inhibitors = new List<Inhibitor>();
            }

            public Inhibitor(Obj_BarracksDampener obj)
            {
                Obj = obj;
                SpawnTime = (int)Game.Time;
                RespawnTime = 240;
                NextRespawnTime = 0;
                Locked = false;
                Called = false;
            }
        }

        public class Relic
        {
            public String Name;
            public String ObjectName;
            public GameObjectTeam Team;
            public GameObject Obj;
            public int SpawnTime;
            public int RespawnTime;
            public int NextRespawnTime;
            public bool Locked;
            public Vector3 MapPosition;
            public Vector3 MinimapPosition;
            public Utility.Map.MapType MapId;
            public bool Called;

            public Relic(string name, String objectName, GameObjectTeam team, Obj_AI_Minion obj, int spawnTime, int respawnTime, Vector3 mapPosition, Vector3 minimapPosition)
            {
                Name = name;
                ObjectName = objectName;
                Team = team;
                Obj = obj;
                SpawnTime = spawnTime;
                RespawnTime = respawnTime;
                Locked = false;
                MapPosition = mapPosition;
                MinimapPosition = minimapPosition;
                MapId = Utility.Map.MapType.CrystalScar;
                NextRespawnTime = 0;
                Called = false;
            }
        }

        public class Altar
        {
            public String Name;
            public String ObjectName;
            public Obj_AI_Minion Obj;
            public GameObject ObjOld;
            public int SpawnTime;
            public int RespawnTime;
            public int NextRespawnTime;
            public bool Locked;
            public Vector3 MapPosition;
            public Vector3 MinimapPosition;
            public String[] LockNames;
            public String[] UnlockNames;
            public Utility.Map.MapType MapId;
            public bool Called;

            public Altar(String name, Obj_AI_Minion obj)
            {
                Name = name;
                Obj = obj;
                SpawnTime = 185;
                RespawnTime = 90;
                Locked = false;
                NextRespawnTime = 0;
                MapId = Utility.Map.MapType.TwistedTreeline;
                Called = false;
            }
        }

        public class JungleMob
        {
            public String Name;
            public Obj_AI_Minion Obj;
            public bool Smite;
            public bool Buff;
            public bool Boss;
            public Utility.Map.MapType MapId;

            public JungleMob(string name, Obj_AI_Minion obj, bool smite, bool buff, bool boss, Utility.Map.MapType mapId)
            {
                Name = name;
                Obj = obj;
                Smite = smite;
                Buff = buff;
                Boss = boss;
                MapId = mapId;
            }
        }

        public class JungleCamp
        {
            public JungleCamp(String name, GameObjectTeam team, int campId, int spawnTime, int respawnTime, Utility.Map.MapType mapId, Vector3 mapPosition, Vector3 minimapPosition, JungleMob[] creeps)
            {
                Name = name;
                Team = team;
                CampId = campId;
                SpawnTime = spawnTime;
                RespawnTime = respawnTime;
                MapId = mapId;
                MapPosition = mapPosition;
                MinimapPosition = minimapPosition;
                Creeps = creeps;
                NextRespawnTime = 0;
                Called = false;
            }

            public String Name;
            public GameObjectTeam Team;
            public int CampId;
            public int SpawnTime;
            public int RespawnTime;
            public int NextRespawnTime;
            public Utility.Map.MapType MapId;
            public Vector3 MapPosition;
            public Vector3 MinimapPosition;
            public JungleMob[] Creeps;
            public bool Called;
        }

        public Timers()
        {
            try
            {
                font = new Font(Drawing.Direct3DDevice, new System.Drawing.Font("Times New Roman", 8));
            }
            catch (Exception)
            {
                Menu.Health.ForceDisable = true;
                Console.WriteLine("Timer: Cannot create Font");
                return;
            }
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            InitJungleMobs();
        }

        ~Timers()
        {
            GameObject.OnCreate -= Obj_AI_Base_OnCreate;
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Game.OnGameProcessPacket -= Game_OnGameProcessPacket;
            Drawing.OnPreReset -= Drawing_OnPreReset;
            Drawing.OnPostReset -= Drawing_OnPostReset;
            Drawing.OnEndScene -= Drawing_OnEndScene;
        }

        public bool IsActive()
        {
            return Menu.Timers.GetActive();
        }

        String AlignTime(float endTime)
        {
            if (!float.IsInfinity(endTime) && !float.IsNaN(endTime))
            {
                var m = (float)Math.Floor(endTime / 60);
                var s = (float)Math.Ceiling(endTime % 60);
                String ms = (s < 10 ? m + ":0" + s : m + ":" + s);
                return ms;
            }
            return "";
        }

        bool PingAndCall(String text, Vector3 pos)
        {
            for (int i = 0; i < Menu.Timers.GetMenuItem("SAwarenessTimersPingTimes").GetValue<Slider>().Value; i++)
            {
                GamePacket gPacketT = Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(pos.X, pos.Y));
                if (Menu.Timers.GetMenuItem("SAwarenessTimersLocalPing").GetValue<bool>())
                {
                    //TODO: Add local ping
                }
                else
                {
                    gPacketT.Send();
                }
            }
            if (Menu.Timers.GetMenuItem("SAwarenessTimersLocalChat").GetValue<bool>())
            {
                Game.PrintChat(text);
            }
            else
            {
                Game.Say(text);
            }
            return true;
        }

        void Drawing_OnPostReset(EventArgs args)
        {
            if (Drawing.Direct3DDevice == null || Drawing.Direct3DDevice.IsDisposed)
                return;
            font.OnResetDevice();
            drawActive = true;
        }

        void Drawing_OnPreReset(EventArgs args)
        {
            font.OnLostDevice();
            drawActive = false;
        }

        void Drawing_OnEndScene(EventArgs args)
        {
            if (!IsActive() || !drawActive)
                return;

            if (Menu.JungleTimer.GetActive())
            {
                foreach (var jungleCamp in JungleCamps)
                {
                    if (jungleCamp.NextRespawnTime <= 0 || jungleCamp.MapId != GMapId)
                        continue;
                    Vector2 sPos = Drawing.WorldToMinimap(jungleCamp.MinimapPosition);
                    DirectXDrawer.DrawText(font, (jungleCamp.NextRespawnTime - (int)Game.Time).ToString(), (int)sPos[0], (int)sPos[1], Color.White);
                    int time = Menu.Timers.GetMenuItem("SAwarenessTimersRemindTime").GetValue<Slider>().Value;
                    if (!jungleCamp.Called && jungleCamp.NextRespawnTime - (int)Game.Time <= time && jungleCamp.NextRespawnTime - (int)Game.Time >= time - 1)
                    {
                        jungleCamp.Called = true;
                        PingAndCall(jungleCamp.Name + " respawns in " + time + " seconds!", jungleCamp.MinimapPosition);
                    }
                }
            }

            if (Menu.AltarTimer.GetActive())
            {
                foreach (var altar in Altars)
                {
                    if (altar.Locked)
                    {
                        if (altar.NextRespawnTime <= 0 || altar.MapId != GMapId)
                            continue;
                        Vector2 sPos = Drawing.WorldToMinimap(altar.Obj.ServerPosition);
                        DirectXDrawer.DrawText(font, (altar.NextRespawnTime - (int)Game.Time).ToString(), (int)sPos[0], (int)sPos[1], Color.White);
                        int time = Menu.Timers.GetMenuItem("SAwarenessTimersRemindTime").GetValue<Slider>().Value;
                        if (!altar.Called && altar.NextRespawnTime - (int)Game.Time <= time && altar.NextRespawnTime - (int)Game.Time >= time - 1)
                        {
                            altar.Called = true;
                            PingAndCall(altar.Name + " unlocks in " + time + " seconds!", altar.Obj.ServerPosition);
                        }
                    }
                }
            }

            if (Menu.RelictTimer.GetActive())
            {
                foreach (var relic in Relics)
                {
                    if (relic.Locked)
                    {
                        if (relic.NextRespawnTime <= 0 || relic.MapId != GMapId)
                            continue;
                        Vector2 sPos = Drawing.WorldToMinimap(relic.MinimapPosition);
                        DirectXDrawer.DrawText(font, (relic.NextRespawnTime - (int)Game.Time).ToString(), (int)sPos[0], (int)sPos[1], Color.White);
                        int time = Menu.Timers.GetMenuItem("SAwarenessTimersRemindTime").GetValue<Slider>().Value;
                        if (!relic.Called && relic.NextRespawnTime - (int)Game.Time <= time && relic.NextRespawnTime - (int)Game.Time >= time - 1)
                        {
                            relic.Called = true;
                            PingAndCall(relic.Name + " respawns in " + time + " seconds!", relic.MinimapPosition);
                        }
                    }
                }
            }

            if (Menu.InhibitorTimer.GetActive())
            {
                if (_inhibitors.Inhibitors == null)
                    return;
                foreach (var inhibitor in _inhibitors.Inhibitors)
                {
                    if (inhibitor.Locked)
                    {
                        if (inhibitor.NextRespawnTime <= 0)
                            continue;
                        Vector2 sPos = Drawing.WorldToMinimap(inhibitor.Obj.Position);
                        DirectXDrawer.DrawText(font, (inhibitor.NextRespawnTime - (int)Game.Time).ToString(), (int)sPos[0], (int)sPos[1], Color.White);
                        int time = Menu.Timers.GetMenuItem("SAwarenessTimersRemindTime").GetValue<Slider>().Value;
                        if (!inhibitor.Called && inhibitor.NextRespawnTime - (int)Game.Time <= time && inhibitor.NextRespawnTime - (int)Game.Time >= time - 1)
                        {
                            inhibitor.Called = true;
                            PingAndCall("Inhibitor respawns in " + time + " seconds!", inhibitor.Obj.Position);
                        }
                    }
                }
            }

            if (Menu.HealthTimer.GetActive())
            {
                foreach (var health in Healths)
                {
                    if (health.Locked)
                    {
                        if (health.NextRespawnTime <= 0 || health.MapId != GMapId)
                            continue;
                        Vector2 sPos = Drawing.WorldToMinimap(health.Position);
                        DirectXDrawer.DrawText(font, (health.NextRespawnTime - (int)Game.Time).ToString(), (int)sPos[0], (int)sPos[1], Color.White);
                        int time = Menu.Timers.GetMenuItem("SAwarenessTimersRemindTime").GetValue<Slider>().Value;
                        if (!health.Called && health.NextRespawnTime - (int)Game.Time <= time && health.NextRespawnTime - (int)Game.Time >= time - 1)
                        {
                            health.Called = true;
                            PingAndCall("Heal respawns in " + time + " seconds!", health.Position);
                        }
                    }
                }
            }

            //var test = ObjectManager.Get<Obj_AI_Minion>().ToList();
            //foreach (var objectType in test)
            //{
            //    float[] w = Drawing.WorldToScreen(objectType.Position);
            //    Drawing.DrawText(w[0], w[1], System.Drawing.Color.Red, objectType.Name);
            //}
        }

        void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;
            if (sender.IsValid)
            {
                if (Menu.JungleTimer.GetActive())
                {
                    if (sender.Type == GameObjectType.obj_AI_Minion
                        && sender.Team == GameObjectTeam.Neutral)
                    {
                        if (JungleMobs.Any(mob => sender.Name.Contains(mob.Name)))
                        {
                            JungleMobList.Add((Obj_AI_Minion)sender);
                        }
                    }
                }

                if (Menu.RelictTimer.GetActive())
                {
                    foreach (var relic in Relics)
                    {
                        if (sender.Name.Contains(relic.ObjectName))
                        {
                            relic.Obj = sender;
                            relic.Locked = false;
                        }
                    }
                }
            }
        }

        public bool IsBigMob(Obj_AI_Minion jungleBigMob)
        {
            foreach (JungleMob jungleMob in JungleMobs)
            {
                if (jungleBigMob.Name.Contains(jungleMob.Name))
                {
                    return jungleMob.Smite;
                }
            }
            return false;
        }

        public bool IsBossMob(Obj_AI_Minion jungleBossMob)
        {
            foreach (JungleMob jungleMob in JungleMobs)
            {
                if (jungleBossMob.SkinName.Contains(jungleMob.Name))
                {
                    return jungleMob.Boss;
                }
            }
            return false;
        }

        public bool HasBuff(Obj_AI_Minion jungleBigMob)
        {
            foreach (JungleMob jungleMob in JungleMobs)
            {
                if (jungleBigMob.SkinName.Contains(jungleMob.Name))
                {
                    return jungleMob.Buff;
                }
            }
            return false;
        }

        private JungleMob GetJungleMobByName(string name, Utility.Map.MapType mapId)
        {
            return JungleMobs.Find(jm => jm.Name == name && jm.MapId == mapId);
        }

        private JungleCamp GetJungleCampByID(int id, Utility.Map.MapType mapId)
        {
            return JungleCamps.Find(jm => jm.CampId == id && jm.MapId == mapId);
        }

        public void InitJungleMobs()
        {
            //All
            //_inhibitors = new Inhibitor("Inhibitor", new[] { "Order_Inhibit_Gem.troy", "Chaos_Inhibit_Gem.troy" }, new[] { "Order_Inhibit_Crystal_Shatter.troy", "Chaos_Inhibit_Crystal_Shatter.troy" });

            //Summoner's Rift
            JungleMobs.Add(new JungleMob("GreatWraith", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("AncientGolem", null, true, true, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("GiantWolf", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("Wraith", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("LizardElder", null, true, true, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("Golem", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("Worm", null, true, true, true, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("Dragon", null, true, false, true, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("Wight", null, true, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("YoungLizard", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("Wolf", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("LesserWraith", null, false, false, false, Utility.Map.MapType.SummonersRift));
            JungleMobs.Add(new JungleMob("SmallGolem", null, false, false, false, Utility.Map.MapType.SummonersRift));

            //Twisted Treeline
            JungleMobs.Add(new JungleMob("TT_NWraith", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NGolem", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_NWolf", null, false, false, false, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_Spiderboss", null, true, true, true, Utility.Map.MapType.TwistedTreeline));
            JungleMobs.Add(new JungleMob("TT_Relic", null, false, false, false, Utility.Map.MapType.TwistedTreeline));

            //Altars.Add(new Altar("Left Altar", "TT_Buffplat_L", null, 180, 85, new[] { "TT_Lock_Blue_L.troy", "TT_Lock_Purple_L.troy", "TT_Lock_Neutral_L.troy" }, new[] { "TT_Unlock_Blue_L.troy", "TT_Unlock_purple_L.troy", "TT_Unlock_Neutral_L.troy" }, 1));
            //Altars.Add(new Altar("Right Altar", "TT_Buffplat_R", null, 180, 85, new[] { "TT_Lock_Blue_R.troy", "TT_Lock_Purple_R.troy", "TT_Lock_Neutral_R.troy" }, new[] { "TT_Unlock_Blue_R.troy", "TT_Unlock_purple_R.troy", "TT_Unlock_Neutral_R.troy" }, 1));

            //Crystal Scar
            Relics.Add(new Relic("Relic", ObjectManager.Player.Team == GameObjectTeam.Order ? "Odin_Prism_Green.troy" : "Odin_Prism_Red.troy", GameObjectTeam.Order, null, 180, 180, new Vector3(5500, 6500, 60), new Vector3(5500, 6500, 60)));
            Relics.Add(new Relic("Relic", ObjectManager.Player.Team == GameObjectTeam.Chaos ? "Odin_Prism_Green.troy" : "Odin_Prism_Red.troy", GameObjectTeam.Chaos, null, 180, 180, new Vector3(7550, 6500, 60), new Vector3(7550, 6500, 60)));

            //Howling Abyss
            //JungleMobs.Add(new JungleMob("HA_AP_HealthRelic", null, false, false, false, 1));

            JungleCamps.Add(new JungleCamp("blue", GameObjectTeam.Order, 1, 115, 300, Utility.Map.MapType.SummonersRift, new Vector3(3570, 7670, 54), new Vector3(3670, 7520, 54), new[] { GetJungleMobByName("AncientGolem", Utility.Map.MapType.SummonersRift), GetJungleMobByName("YoungLizard", Utility.Map.MapType.SummonersRift), GetJungleMobByName("YoungLizard", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("wolves", GameObjectTeam.Order, 2, 125, 50, Utility.Map.MapType.SummonersRift, new Vector3(3430, 6300, 56), new Vector3(3360, 6310, 56), new[] { GetJungleMobByName("GiantWolf", Utility.Map.MapType.SummonersRift), GetJungleMobByName("Wolf", Utility.Map.MapType.SummonersRift), GetJungleMobByName("Wolf", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("wraiths", GameObjectTeam.Order, 3, 125, 50, Utility.Map.MapType.SummonersRift, new Vector3(6540, 5230, 56), new Vector3(6620, 5350, 56), new[] { GetJungleMobByName("Wraith", Utility.Map.MapType.SummonersRift), GetJungleMobByName("LesserWraith", Utility.Map.MapType.SummonersRift), GetJungleMobByName("LesserWraith", Utility.Map.MapType.SummonersRift), GetJungleMobByName("LesserWraith", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("red", GameObjectTeam.Order, 4, 115, 300, Utility.Map.MapType.SummonersRift, new Vector3(7370, 3830, 58), new Vector3(7560, 3800, 58), new[] { GetJungleMobByName("LizardElder", Utility.Map.MapType.SummonersRift), GetJungleMobByName("YoungLizard", Utility.Map.MapType.SummonersRift), GetJungleMobByName("YoungLizard", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("golems", GameObjectTeam.Order, 5, 125, 50, Utility.Map.MapType.SummonersRift, new Vector3(7990, 2550, 54), new Vector3(8050, 2460, 54), new[] { GetJungleMobByName("Golem", Utility.Map.MapType.SummonersRift), GetJungleMobByName("SmallGolem", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("wight", GameObjectTeam.Order, 13, 125, 50, Utility.Map.MapType.SummonersRift, new Vector3(1688, 8248, 54), new Vector3(1820, 8100, 54), new[] { GetJungleMobByName("Wight", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("blue", GameObjectTeam.Chaos, 7, 115, 300, Utility.Map.MapType.SummonersRift, new Vector3(10455, 6800, 55), new Vector3(10570, 6780, 54), new[] { GetJungleMobByName("AncientGolem", Utility.Map.MapType.SummonersRift), GetJungleMobByName("YoungLizard", Utility.Map.MapType.SummonersRift), GetJungleMobByName("YoungLizard", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("wolves", GameObjectTeam.Chaos, 8, 125, 50, Utility.Map.MapType.SummonersRift, new Vector3(10570, 8150, 63), new Vector3(10644, 8070, 63), new[] { GetJungleMobByName("GiantWolf", Utility.Map.MapType.SummonersRift), GetJungleMobByName("Wolf", Utility.Map.MapType.SummonersRift), GetJungleMobByName("Wolf", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("wraiths", GameObjectTeam.Chaos, 9, 125, 50, Utility.Map.MapType.SummonersRift, new Vector3(7465, 9220, 56), new Vector3(7480, 9238, 56), new[] { GetJungleMobByName("Wraith", Utility.Map.MapType.SummonersRift), GetJungleMobByName("LesserWraith", Utility.Map.MapType.SummonersRift), GetJungleMobByName("LesserWraith", Utility.Map.MapType.SummonersRift), GetJungleMobByName("LesserWraith", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("red", GameObjectTeam.Chaos, 10, 115, 300, Utility.Map.MapType.SummonersRift, new Vector3(6620, 10637, 55), new Vector3(6648, 10570, 54), new[] { GetJungleMobByName("LizardElder", Utility.Map.MapType.SummonersRift), GetJungleMobByName("YoungLizard", Utility.Map.MapType.SummonersRift), GetJungleMobByName("YoungLizard", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("golems", GameObjectTeam.Chaos, 11, 125, 50, Utility.Map.MapType.SummonersRift, new Vector3(6010, 11920, 40), new Vector3(5920, 11900, 40), new[] { GetJungleMobByName("Golem", Utility.Map.MapType.SummonersRift), GetJungleMobByName("SmallGolem", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("wight", GameObjectTeam.Chaos, 14, 125, 50, Utility.Map.MapType.SummonersRift, new Vector3(12266, 6215, 54), new Vector3(12385, 6081, 58), new[] { GetJungleMobByName("Wight", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("dragon", GameObjectTeam.Neutral, 6, 2 * 60 + 30, 360, Utility.Map.MapType.SummonersRift, new Vector3(9400, 4130, -61), new Vector3(9600, 4120, -61), new[] { GetJungleMobByName("Dragon", Utility.Map.MapType.SummonersRift) }));
            JungleCamps.Add(new JungleCamp("nashor", GameObjectTeam.Neutral, 12, 15 * 60, 420, Utility.Map.MapType.SummonersRift, new Vector3(4620, 10265, -63), new Vector3(4700, 10165, -63), new[] { GetJungleMobByName("Worm", Utility.Map.MapType.SummonersRift) }));

            JungleCamps.Add(new JungleCamp("wraiths", GameObjectTeam.Order, 1, 100, 50, Utility.Map.MapType.TwistedTreeline, new Vector3(4414, 5774, 60), new Vector3(4414, 5774, 60), new[] { GetJungleMobByName("TT_NWraith", Utility.Map.MapType.TwistedTreeline), GetJungleMobByName("TT_NWraith", Utility.Map.MapType.TwistedTreeline), GetJungleMobByName("TT_NWraith", Utility.Map.MapType.TwistedTreeline) }));
            JungleCamps.Add(new JungleCamp("golems", GameObjectTeam.Order, 2, 100, 50, Utility.Map.MapType.TwistedTreeline, new Vector3(5088, 8065, 60), new Vector3(5088, 8065, 60), new[] { GetJungleMobByName("TT_NGolem", Utility.Map.MapType.TwistedTreeline), GetJungleMobByName("TT_NGolem", Utility.Map.MapType.TwistedTreeline) }));
            JungleCamps.Add(new JungleCamp("wolves", GameObjectTeam.Order, 3, 100, 50, Utility.Map.MapType.TwistedTreeline, new Vector3(6148, 5993, 60), new Vector3(6148, 5993, 60), new[] { GetJungleMobByName("TT_NWolf", Utility.Map.MapType.TwistedTreeline), GetJungleMobByName("TT_NWolf", Utility.Map.MapType.TwistedTreeline), GetJungleMobByName("TT_NWolf", Utility.Map.MapType.TwistedTreeline) }));
            JungleCamps.Add(new JungleCamp("wraiths", GameObjectTeam.Chaos, 4, 100, 50, Utility.Map.MapType.TwistedTreeline, new Vector3(11008, 5775, 60), new Vector3(11008, 5775, 60), new[] { GetJungleMobByName("TT_NWraith", Utility.Map.MapType.TwistedTreeline), GetJungleMobByName("TT_NWraith", Utility.Map.MapType.TwistedTreeline), GetJungleMobByName("TT_NWraith", Utility.Map.MapType.TwistedTreeline) }));
            JungleCamps.Add(new JungleCamp("golems", GameObjectTeam.Chaos, 5, 100, 50, Utility.Map.MapType.TwistedTreeline, new Vector3(10341, 8084, 60), new Vector3(10341, 8084, 60), new[] { GetJungleMobByName("TT_NGolem", Utility.Map.MapType.TwistedTreeline), GetJungleMobByName("TT_NGolem", Utility.Map.MapType.TwistedTreeline) }));
            JungleCamps.Add(new JungleCamp("wolves", GameObjectTeam.Chaos, 6, 100, 50, Utility.Map.MapType.TwistedTreeline, new Vector3(9239, 6022, 60), new Vector3(9239, 6022, 60), new[] { GetJungleMobByName("TT_NWolf", Utility.Map.MapType.TwistedTreeline), GetJungleMobByName("TT_NWolf", Utility.Map.MapType.TwistedTreeline), GetJungleMobByName("TT_NWolf", Utility.Map.MapType.TwistedTreeline) }));
            JungleCamps.Add(new JungleCamp("heal", GameObjectTeam.Neutral, 7, 115, 90, Utility.Map.MapType.TwistedTreeline, new Vector3(7711, 6722, 60), new Vector3(7711, 6722, 60), new[] { GetJungleMobByName("TT_Relic", Utility.Map.MapType.TwistedTreeline) }));
            JungleCamps.Add(new JungleCamp("vilemaw", GameObjectTeam.Neutral, 8, 10 * 60, 300, Utility.Map.MapType.TwistedTreeline, new Vector3(7711, 10080, 60), new Vector3(7711, 10080, 60), new[] { GetJungleMobByName("TT_Spiderboss", Utility.Map.MapType.TwistedTreeline) }));

            //JungleCamps.Add(new JungleCamp("heal", GameObjectTeam.Neutral, 1, 190, 40, 3, new Vector3(8922, 7868, 60), new Vector3(8922, 7868, 60), new[] { GetJungleMobByName("HA_AP_HealthRelic", 3) }));
            //JungleCamps.Add(new JungleCamp("heal", GameObjectTeam.Neutral, 2, 190, 40, 3, new Vector3(7473, 6617, 60), new Vector3(7473, 6617, 60), new[] { GetJungleMobByName("HA_AP_HealthRelic", 3) }));
            //JungleCamps.Add(new JungleCamp("heal", GameObjectTeam.Neutral, 3, 190, 40, 3, new Vector3(5929, 5190, 60), new Vector3(5929, 5190, 60), new[] { GetJungleMobByName("HA_AP_HealthRelic", 3) }));
            //JungleCamps.Add(new JungleCamp("heal", GameObjectTeam.Neutral, 4, 190, 40, 3, new Vector3(4751, 3901, 60), new Vector3(4751, 3901, 60), new[] { GetJungleMobByName("HA_AP_HealthRelic", 3) }));

            foreach (var objAiBase in ObjectManager.Get<GameObject>())
            {
                Obj_AI_Base_OnCreate(objAiBase, new EventArgs());
            }

            _inhibitors = new Inhibitor();
            foreach (var inhib in ObjectManager.Get<Obj_BarracksDampener>())
            {
                _inhibitors.Inhibitors.Add(new Inhibitor(inhib));
            }

            foreach (var objectType in ObjectManager.Get<Obj_AI_Minion>())
            {
                if (objectType.Name.Contains("Health"))
                    Healths.Add(new Health(objectType));
                if (objectType.Name.Contains("Buffplat"))
                {
                    if (objectType.Name.Contains("_L"))
                        Altars.Add(new Altar("Left Altar", objectType));
                    else
                        Altars.Add(new Altar("Right Altar", objectType));
                }

            }

            //foreach (JungleCamp jungleCamp in JungleCamps) //GAME.TIME BUGGED
            //{
            //    if (Game.Time > 30) //TODO: Reduce when game.time got fixed
            //    {
            //        jungleCamp.NextRespawnTime = 0;
            //    }
            //    int nextRespawnTime = jungleCamp.SpawnTime - (int)Game.Time;
            //    if (nextRespawnTime > 0)
            //    {
            //        jungleCamp.NextRespawnTime = nextRespawnTime;
            //    }
            //}
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            if (Menu.JungleTimer.GetActive())
            {
                foreach (JungleCamp jungleCamp in JungleCamps)
                {
                    if ((jungleCamp.NextRespawnTime - (int)Game.Time) < 0)
                    {
                        jungleCamp.NextRespawnTime = 0;
                        jungleCamp.Called = false;
                    }
                }
            }

            if (Menu.AltarTimer.GetActive())
            {
                Altar altarDestroyed = new Altar(null, null);
                foreach (var altar in Altars)
                {
                    if (altar.Obj.IsValid)
                    {
                        bool hasBuff = false;
                        foreach (var buff in altar.Obj.Buffs)
                        {
                            if (buff.Name == "treelinelanternlock")
                            {
                                hasBuff = true;
                                break;
                            }
                        }
                        if (!hasBuff)
                        {
                            altar.Locked = false;
                            altar.NextRespawnTime = 0;
                            altar.Called = false;
                        }
                        else if (hasBuff && altar.Locked == false)
                        {
                            altar.Locked = true;
                            altar.NextRespawnTime = altar.RespawnTime + (int)Game.Time;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        if (altar.NextRespawnTime < (int)Game.Time)
                        {
                            altarDestroyed = altar;
                        }
                    }
                }
                if (Altars.Remove(altarDestroyed))
                {

                }
                foreach (var altar in ObjectManager.Get<Obj_AI_Minion>())
                {
                    Altar nAltar = null;
                    if (altar.Name.Contains("Buffplat"))
                    {
                        Altar health1 = Altars.Find(jm => jm.Obj.NetworkId == altar.NetworkId);
                        if (health1 == null)
                            if (altar.Name.Contains("_L"))
                                nAltar = new Altar("Left Altar", altar);
                            else
                                nAltar = new Altar("Right Altar", altar);
                    }

                    if (nAltar != null)
                        Altars.Add(nAltar);
                }
            }

            if (Menu.RelictTimer.GetActive())
            {
                foreach (var relic in Relics)
                {
                    if (!relic.Locked && (relic.Obj != null && (!relic.Obj.IsValid || relic.Obj.IsDead)))
                    {
                        if (Game.Time < relic.SpawnTime)
                        {
                            relic.NextRespawnTime = relic.SpawnTime - (int)Game.Time;
                        }
                        else
                        {
                            relic.NextRespawnTime = relic.RespawnTime + (int)Game.Time;
                        }
                        relic.Locked = true;
                    }
                    if ((relic.NextRespawnTime - (int)Game.Time) < 0)
                    {
                        relic.NextRespawnTime = 0;
                        relic.Called = false;
                    }
                }
            }

            //if (Menu.InhibitorTimer.GetActive())
            //{
            //    if (_inhibitors.Inhibitors == null)
            //        return;
            //    foreach (var inhibitor in _inhibitors.Inhibitors)
            //    {
            //        if (inhibitor.Locked)
            //        {
            //            if (inhibitor.NextRespawnTime < Game.Time)
            //            {
            //                inhibitor.Locked = false;
            //            }
            //        }
            //    }
            //}

            if (Menu.HealthTimer.GetActive())
            {
                Health healthDestroyed = new Health(null);
                foreach (var health in Healths)
                {
                    if (health.Obj.IsValid)
                        if (health.Obj.Health > 0)
                        {
                            health.Locked = false;
                            health.NextRespawnTime = 0;
                            health.Called = false;
                        }
                        else if (health.Obj.Health < 1 && health.Locked == false)
                        {
                            health.Locked = true;
                            health.NextRespawnTime = health.RespawnTime + (int)Game.Time;
                        }
                        else { }
                    else
                    {
                        if (health.NextRespawnTime < (int)Game.Time)
                        {
                            healthDestroyed = health;
                        }
                    }
                }
                if (Healths.Remove(healthDestroyed))
                {

                }
                foreach (var health in ObjectManager.Get<Obj_AI_Minion>())
                {
                    Health nHealth = null;
                    if (health.Name.Contains("Health"))
                    {
                        Health health1 = Healths.Find(jm => jm.Obj.NetworkId == health.NetworkId);
                        if (health1 == null)
                            nHealth = new Health(health);
                    }

                    if (nHealth != null)
                        Healths.Add(nHealth);
                }
            }

            if (Menu.InhibitorTimer.GetActive())
            {
                if (_inhibitors.Inhibitors == null)
                    return;
                foreach (var inhibitor in _inhibitors.Inhibitors)
                {
                    if (inhibitor.Obj.Health > 0)
                    {
                        inhibitor.Locked = false;
                        inhibitor.NextRespawnTime = 0;
                        inhibitor.Called = false;
                    }
                    else if (inhibitor.Obj.Health < 1 && inhibitor.Locked == false)
                    {
                        inhibitor.Locked = true;
                        inhibitor.NextRespawnTime = inhibitor.RespawnTime + (int)Game.Time;
                    }
                }
            }
        }

        private void UpdateCamps(int networkId, int campId, byte emptyType)
        {
            if (emptyType != 3)
            {
                JungleCamp jungleCamp = GetJungleCampByID(campId, GMapId);
                if (jungleCamp != null)
                {
                    jungleCamp.NextRespawnTime = (int)Game.Time + jungleCamp.RespawnTime;
                }
            }
        }

        private void EmptyCamp(BinaryReader b)
        {
            byte[] h = b.ReadBytes(4);
            int nwId = BitConverter.ToInt32(h, 0);

            h = b.ReadBytes(4);
            int cId = BitConverter.ToInt32(h, 0);

            byte emptyType = b.ReadByte();
            UpdateCamps(nwId, cId, emptyType);
        }

        void Game_OnGameProcessPacket(GamePacketEventArgs args) //TODO: Check if Packet is right
        {
            if (!IsActive())
                return;
            if (!Menu.JungleTimer.GetActive())
                return;
            try
            {
                MemoryStream stream = new MemoryStream(args.PacketData);
                using (BinaryReader b = new BinaryReader(stream))
                {
                    int pos = 0;
                    var length = (int)b.BaseStream.Length;
                    while (pos < length)
                    {
                        int v = b.ReadInt32();
                        if (v == 195) //OLD 194
                        {
                            byte[] h = b.ReadBytes(1);
                            EmptyCamp(b);
                        }
                        pos += sizeof(int);
                    }
                }
            }
            catch (EndOfStreamException)
            {
            }
        }
    }
}
