using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;
using Font = SharpDX.Direct3D9.Font;
using MenuItem = LeagueSharp.Common.MenuItem;
using Rectangle = SharpDX.Rectangle;

namespace SAwareness
{
    /*Send Ping
     * 
     * GamePacket gPacketT = Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(100, 100));
            gPacketT.Send();*/

    static class Log
    {
        public static String File = "C:\\SAwareness.log";
        public static String Prefix = "Packet";

        public static void LogString(String text, String file = null, String prefix = null)
        {
            switch (text)
            {
                case "missile":
                case "DrawFX":
                case "Mfx_pcm_mis.troy":
                case "Mfx_bcm_tar.troy":
                case "Mfx_bcm_mis.troy":
                case "Mfx_pcm_tar.troy":
                    return;
            }
            LogWrite(text, file, prefix);
        }

        public static void LogPacket(byte[] data, String file = null, String prefix = null)
        {
            LogWrite(BitConverter.ToString(data), file, prefix);
        }

        private static void LogWrite(String text, String file = null, String prefix = null)
        {
            if (text == null)
                return;
            if (file == null)
                file = File;
            if (prefix == null)
                prefix = Prefix;
            using (StreamWriter stream = new StreamWriter(file, true))
            {
                stream.WriteLine(prefix + "@" + Environment.TickCount + ": " + text);
            }
        }
    }

    static class Common
    {
        public static bool IsOnScreen(Vector3 vector)
        {
            float[] screen = Drawing.WorldToScreen(vector);
            if (screen[0] < 0 || screen[0] > Drawing.Width || screen[1] < 0 || screen[1] > Drawing.Height)
                return false;
            return true;
        }
    }

    static class Download
    {
        public static String Host = "https://github.com/Screeder/SAwareness/raw/master/";
        public static String Path = "Sprites/SAwareness/CHAMP/";

        public static void DownloadFile(String hostfile, String localfile)
        {
            var webClient = new WebClient();
            webClient.DownloadFile(Host + Path + hostfile, localfile);
        }
    }

    static class DirectXDrawer
    {
        public static void DrawText(Font font, String text, int posX, int posY, Color color)
        {
            if (font == null || font.IsDisposed)
            {
                return;
            }
            var rec = font.MeasureText(null, text, FontDrawFlags.Center);
            font.DrawText(null, text, posX + 1 + rec.X, posY, Color.Black);
            font.DrawText(null, text, posX + 1 + rec.X, posY + 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY + 1, Color.Black);
            font.DrawText(null, text, posX - 1 + rec.X, posY, Color.Black);
            font.DrawText(null, text, posX - 1 + rec.X, posY - 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY - 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY, color);
        }

        public static void DrawSprite(Sprite sprite, Texture texture, Size size, Color color, Rectangle? spriteResize)
        {
            if (sprite != null && texture != null)
            {
                sprite.Draw(texture, color, spriteResize, new Vector3(size.Width, size.Height, 0));
            }
        }

        public static void DrawSprite(Sprite sprite, Texture texture, Size size, Color color)
        {
            if (sprite != null && texture != null)
            {
                DrawSprite(sprite, texture, size, color, null);
            }
        }
    }

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
                //Menu.AutoLevler.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoLevler", "SAwarenessAutoLevler"));
                //tempSettings = Menu.AutoLevler.AddMenuItemSettings("Priority",
                //    "SAwarenessAutoLevlerPriority");

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
        public static MenuItemSettings CdPanel = new MenuItemSettings(typeof(SAwareness.CdTracker)); //Works but need many improvements
        public static MenuItemSettings SsCaller = new MenuItemSettings(typeof(SAwareness.SsCaller)); //Missing local ping
        public static MenuItemSettings Tracker = new MenuItemSettings();
        public static MenuItemSettings WaypointTracker = new MenuItemSettings(typeof(SAwareness.WaypointTracker)); //Works
        public static MenuItemSettings CloneTracker = new MenuItemSettings(typeof(SAwareness.CloneTracker)); //Works
        public static MenuItemSettings Timers = new MenuItemSettings(typeof(SAwareness.JungleTimer));
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
        public static MenuItemSettings GankTracker = new MenuItemSettings(typeof(SAwareness.GankPotentialTracker)); //Needs testing
        public static MenuItemSettings GankDetector = new MenuItemSettings(typeof(SAwareness.GankDetector)); //Needs testing
        public static MenuItemSettings AltarTimer = new MenuItemSettings();
        public static MenuItemSettings Ward = new MenuItemSettings(typeof(SAwareness.WardIt)); //Works
        public static MenuItemSettings SkinChanger = new MenuItemSettings(typeof(SAwareness.SkinChanger)); //Need to send local packet
        public static MenuItemSettings AutoSmite = new MenuItemSettings(typeof(SAwareness.AutoSmite)); //Works
        public static MenuItemSettings AutoPot = new MenuItemSettings(typeof(SAwareness.AutoPot));
    }

    class AutoPot
    {
        float lastTimeActive = 0;
        private List<Pot> pots = new List<Pot>();

        public class Pot
        {
            public int Id;
            public String Buff;
            public float LastTime;
            public PotType Type;

            public Pot()
            {
                
            }

            public Pot(int id, String buff, PotType type)
            {
                Id = id;
                Buff = buff;
                Type = type;
            }

            public enum PotType
            {
                None,
                Health,
                Mana,
                Both
            }
        }

        public AutoPot()
        {
            pots.Add(new Pot(2037, "PotionOfGiantStrengt", Pot.PotType.Health)); //elixirOfFortitude
            pots.Add(new Pot(2039, "PotionOfBrilliance", Pot.PotType.Mana)); //elixirOfBrilliance            
            pots.Add(new Pot(2041, "ItemCrystalFlask", Pot.PotType.Both)); //crystalFlask
            pots.Add(new Pot(2009, "ItemMiniRegenPotion", Pot.PotType.Both)); //biscuit
            pots.Add(new Pot(2003, "RegenerationPotion", Pot.PotType.Health)); //healthPotion
            pots.Add(new Pot(2004, "FlaskOfCrystalWater", Pot.PotType.Mana)); //manaPotion
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoPot()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Menu.AutoPot.GetActive();
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if(!IsActive() || ObjectManager.Player.IsDead)
                return;
            Pot myPot = null;
            if (
                Menu.AutoPot.GetMenuSettings("SAwarenessAutoPotHealthPot")
                    .GetMenuItem("SAwarenessAutoPotHealthPotActive")
                    .GetValue<bool>())
            {
                foreach (var pot in pots)
                {
                    if (pot.Type == Pot.PotType.Health || pot.Type == Pot.PotType.Both)
                    {
                        if (ObjectManager.Player.Health/ObjectManager.Player.MaxHealth*100 <=
                            Menu.AutoPot.GetMenuSettings("SAwarenessAutoPotHealthPot")
                                .GetMenuItem("SAwarenessAutoPotHealthPotPercent")
                                .GetValue<Slider>().Value)
                        {
                            if (!Items.HasItem(pot.Id))
                                continue;
                            if (!Items.CanUseItem(pot.Id))
                                continue;
                            myPot = pot;
                            break;
                        }
                    }
                }
            }
            if (myPot != null)
                UsePot(myPot);
            if (
                Menu.AutoPot.GetMenuSettings("SAwarenessAutoPotManaPot")
                    .GetMenuItem("SAwarenessAutoPotManaPotActive")
                    .GetValue<bool>())
            {
                foreach (var pot in pots)
                {
                    if (pot.Type == Pot.PotType.Mana || pot.Type == Pot.PotType.Both)
                    {
                        if (ObjectManager.Player.Mana/ObjectManager.Player.MaxMana*100 <=
                            Menu.AutoPot.GetMenuSettings("SAwarenessAutoPotManaPot")
                                .GetMenuItem("SAwarenessAutoPotManaPotPercent")
                                .GetValue<Slider>().Value)
                        {
                            if (!Items.HasItem(pot.Id))
                                continue;
                            if (!Items.CanUseItem(pot.Id))
                                continue;
                            myPot = pot;
                            break;
                        }
                    }
                }
            }
            if(myPot != null)
                UsePot(myPot);
        }

        void UsePot(Pot pot)
        {
            foreach (var buff in ObjectManager.Player.Buffs)
            {
                Console.WriteLine(buff.Name);
                if (buff.Name.Contains(pot.Buff))
                {
                    
                    return;
                }
            }
            if (pot.LastTime + 5 > Game.Time)
                return;
            if (!Items.HasItem(pot.Id))
                return;
            if (!Items.CanUseItem(pot.Id))
                return;
            Items.UseItem(pot.Id);
            pot.LastTime = Game.Time;
        }
    }

    class Health
    {
        Font font;

        public Health()
        {
            try
            {
                font = new Font(Drawing.Direct3DDevice, new System.Drawing.Font("Times New Roman", 8));
            }
            catch (Exception)
            {
                Menu.Health.ForceDisable = true;
                Console.WriteLine("Health: Cannot create Font");
                return;
            }
            Drawing.OnEndScene += Drawing_OnEndScene;
        }

        void Drawing_OnEndScene(EventArgs args)
        {
            DrawTurrentHealth();
            DrawInhibitorHealth();
        }

        ~Health()
        {
            Drawing.OnEndScene -= Drawing_OnEndScene;
        }

        public bool IsActive()
        {
            return Menu.Health.GetActive();
        }

        private void DrawInhibitorHealth()
        {
            if (!IsActive())
                return;
            if (!Menu.InhibitorHealth.GetActive())
                return;
            List<Obj_Barracks> _baseB = new List<Obj_Barracks>();
            List<Obj_BarracksDampener> _baseBD = ObjectManager.Get<Obj_BarracksDampener>().ToList();

            foreach (var inhibitor in _baseB)
            {
                if (!inhibitor.IsDead && inhibitor.IsValid && inhibitor.Health > 0)
                {
                    float[] pos = Drawing.WorldToMinimap(inhibitor.Position);
                    int health = 0;
                    StringList mode =
                        Menu.Health.GetMenuItem("SAwarenessHealthMode")
                            .GetValue<StringList>();
                    switch (mode.SelectedIndex)
                    {
                        case 0:
                            health = (int)((inhibitor.Health / inhibitor.MaxHealth) * 100);
                            break;

                        case 1:
                            health = (int)inhibitor.Health;
                            break;
                    }
                    DirectXDrawer.DrawText(font, health.ToString(), (int)pos[0], (int)pos[1], Color.AliceBlue);
                    //Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.Green, ((int)turret.Health).ToString());
                    //Drawing.DrawText(turret.HealthBarPosition.X, turret.HealthBarPosition.Y + 20, System.Drawing.Color.Green, ((int)turret.Health).ToString());
                }
            }

            foreach (var inhibitor in _baseBD)
            {
                if (!inhibitor.IsDead && inhibitor.IsValid)
                {
                    float[] pos = Drawing.WorldToMinimap(inhibitor.Position);
                    int health = 0;
                    StringList mode =
                        Menu.Health.GetMenuItem("SAwarenessHealthMode")
                            .GetValue<StringList>();
                    switch (mode.SelectedIndex)
                    {
                        case 0:
                            health = (int)((inhibitor.Health / inhibitor.MaxHealth) * 100);
                            break;

                        case 1:
                            health = (int)inhibitor.Health;
                            break;
                    }
                    DirectXDrawer.DrawText(font, health.ToString(), (int)pos[0], (int)pos[1], Color.AliceBlue);
                    //Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.Green, ((int)turret.Health).ToString());
                    //Drawing.DrawText(turret.HealthBarPosition.X, turret.HealthBarPosition.Y + 20, System.Drawing.Color.Green, ((int)turret.Health).ToString());
                }
            }
            //throw new NotImplementedException(); TODO: Implement it
        }

        private void DrawTurrentHealth() //TODO: Draw HP above BarPos
        {
            if (!IsActive())
                return;
            if (!Menu.TowerHealth.GetActive())
                return;
            foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
            {
                if (!turret.IsDead && turret.IsValid && turret.Health != 9999)
                {
                    float[] pos = Drawing.WorldToMinimap(turret.Position);
                    int health = 0;
                    StringList mode =
                        Menu.Health.GetMenuItem("SAwarenessHealthMode")
                            .GetValue<StringList>();
                    switch (mode.SelectedIndex)
                    {
                        case 0:
                            health = (int)((turret.Health / turret.MaxHealth) * 100);
                            break;

                        case 1:
                            health = (int)turret.Health;
                            break;
                    }
                    DirectXDrawer.DrawText(font, health.ToString(), (int)pos[0], (int)pos[1], Color.AliceBlue);
                    //Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.Green, ((int)turret.Health).ToString());
                    //Drawing.DrawText(turret.HealthBarPosition.X, turret.HealthBarPosition.Y + 20, System.Drawing.Color.Green, ((int)turret.Health).ToString());
                }
            }
        }
    }

    class CloneTracker
    {
        public CloneTracker()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~CloneTracker()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Menu.CloneTracker.GetActive();
        }

        void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    if (hero.ChampionName.Contains("Shaco") ||
                        hero.ChampionName.Contains("LeBlanc") ||
                        hero.ChampionName.Contains("MonkeyKing") ||
                        hero.ChampionName.Contains("Yorick"))
                    {
                        Drawing.DrawCircle(hero.ServerPosition, 100, System.Drawing.Color.Red);
                        Drawing.DrawCircle(hero.ServerPosition, 110, System.Drawing.Color.Red);
                    }
                }                
            }
        }
    }

    class AutoSmite
    {
        String[] monsters = { "GreatWraith", "Wraith", "AncientGolem", "GiantWolf", "LizardElder", "Golem", "Worm", "Dragon", "Wight" };

        public AutoSmite()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~AutoSmite()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        void Drawing_OnDraw(EventArgs args)
        {
            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
            {
                int smiteDamage = GetSmiteDamage();
                if (Vector3.Distance(ObjectManager.Player.ServerPosition, minion.ServerPosition) < 1500)
                {                    
                    foreach (var monster in monsters)
                    {
                        if (minion.SkinName == monster && minion.IsVisible)
                        {
                            float[] pos = Drawing.WorldToScreen(minion.ServerPosition);
                            Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.SkyBlue, minion.Health != 0 ? (((int)minion.Health - smiteDamage)).ToString() : "");
                        }
                    }
                }
            }
        }
        
        public bool IsActive()
        {
            return Menu.AutoSmite.GetActive();
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
            {
                List<Obj_AI_Minion> min = ObjectManager.Get<Obj_AI_Minion>().ToList();
                if (Vector3.Distance(ObjectManager.Player.ServerPosition, minion.ServerPosition) < 750)
                {
                    int smiteDamage = GetSmiteDamage();
                    if (minion.Health <= smiteDamage && minion.Health > 0)
                    {
                        foreach (var monster in monsters)
                        {
                            if (minion.SkinName == monster && minion.IsVisible)
                            {
                                SpellSlot spellSlot = GetSmiteSlot();
                                int slot = -1;
                                if (spellSlot == SpellSlot.Q)
                                    slot = 64;
                                else if (spellSlot == SpellSlot.W)
                                    slot = 65;
                                if (slot != -1)
                                {
                                    GamePacket gPacketT = Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(minion.NetworkId, (SpellSlot)slot));
                                    gPacketT.Send();
                                }
                            }
                        }
                    }
                }
            }
        }

        private int GetSmiteDamage()
        {
            int level = ObjectManager.Player.Level;
            int smiteDamage = 390 +
                                (level < 5 ? 20 * (level - 1) :
                                (level < 10 ? 60 + 30 * (level - 4) :
                                (level < 15 ? 210 + 40 * (level - 9) :
                                              410 + 50 * (level - 14))));
            return smiteDamage;
        }

        private SpellSlot GetSmiteSlot()
        {
            foreach (var spell in ObjectManager.Player.SummonerSpellbook.Spells)
            {
                if (spell.Name.Contains("Smite") && spell.State == SpellState.Ready)
                    return spell.Slot;
            }
            return SpellSlot.Unknown;
        }
    }

    class SkinChanger
    {

        public class Skin
        {
            public static byte Header;

            static Skin()
            {
                Skin.Header = 0x97;
            }

            public static Skin.Struct Decoded(byte[] data)
            {
                //GamePacket gamePacket = new GamePacket(data);
                //Skin.Struct @struct = new Skin.Struct();
                //@struct.SourceNetworkId = gamePacket.ReadInteger();
                //gamePacket.Position = (long)1;
                //@struct.Slot = gamePacket.ReadByte();
                //@struct.FromX = gamePacket.ReadFloat();
                //@struct.FromY = gamePacket.ReadFloat();
                //@struct.ToX = gamePacket.ReadFloat();
                //@struct.ToY = gamePacket.ReadFloat();
                //return @struct;
                return new Skin.Struct(); //TODO: Encode the packet
            }

            public static GamePacket Encoded(Skin.Struct packetStruct)
            {
                GamePacket gamePacket = new GamePacket(Skin.Header);
                gamePacket.WriteInteger(packetStruct.SourceNetworkId);
                //long curPos = gamePacket.Position;
                //gamePacket.Position = (long)1;
                gamePacket.WriteByte(packetStruct.SourceNetworkIdP1);
                gamePacket.WriteByte(packetStruct.SourceNetworkIdP2);
                gamePacket.WriteByte(packetStruct.SourceNetworkIdP3);
                gamePacket.WriteByte(packetStruct.SourceNetworkIdP4);
                gamePacket.WriteByte(packetStruct.Unknown);
                gamePacket.WriteInteger(packetStruct.SkinId); //SKIN ID
                foreach (var b in packetStruct.Unknown2)
                {
                    gamePacket.WriteByte(b);
                }
                foreach (var b in packetStruct.Unknown3)
                {
                    gamePacket.WriteByte(b);
                }
                return gamePacket;
            }

            public struct Struct
            {
                public int SourceNetworkId;
                public byte SourceNetworkIdP1;
                public byte SourceNetworkIdP2;
                public byte SourceNetworkIdP3;
                public byte SourceNetworkIdP4;
                public byte Unknown;
                public int SkinId;
                public byte[] Unknown2;
                public byte[] Unknown3;

                public Struct(String charName, int sourceNetworkId = 0, int skinId = 0)
                {
                    if (sourceNetworkId == 0)
                        SourceNetworkId = ObjectManager.Player.NetworkId;
                    else
                        SourceNetworkId = sourceNetworkId;
                    SkinId = skinId;
                    byte[] tBytes = BitConverter.GetBytes(SourceNetworkId);
                    SourceNetworkIdP1 = tBytes[0];
                    SourceNetworkIdP2 = tBytes[1];
                    SourceNetworkIdP3 = tBytes[2];
                    SourceNetworkIdP4 = tBytes[3];
                    Unknown = 1;
                    Unknown2 = new byte[charName.Length];
                    for (int i = 0; i < charName.Length; i++)
                    {
                        Unknown2[i] = (Convert.ToByte(charName.ToCharArray()[i]));
                    }
                    Unknown3 = new byte[64 - charName.Length];
                    for (int i = 0; i < 64 - charName.Length; i++)
                    {
                        try
                        {
                            Unknown3[i] = (0);
                        }
                        catch (Exception)
                        {
                            break;
                        }                        
                    }
                }
            }
        }

        public static void GenAndSendModelPacket(String champName, int skinId)
        {
            GamePacket gPacket = Skin.Encoded(new Skin.Struct(champName, 0, skinId));
            //gPacket.Send(PacketChannel.S2C); 
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
            byte[] bs = new byte[gPacket.Size()];
            gPacket.Position = 0;
            for (int i = 0; i < gPacket.Size(); i++)
            {
                bs[i] = gPacket.ReadByte();
            }
            Game_OnGameProcessPacket(new GamePacketEventArgs(1, bs, PacketProtocolFlags.Reliable, PacketChannel.S2C)); //TODO: Check if this works


            //Packet paket = new Packet();
            //p = CLoLPacket(0x97)
            //p:EncodeF(player.networkID)
            //p.pos = 1
            //t1 = p:Decode1()
            //t2 = p:Decode1()
            //t3 = p:Decode1()
            //t4 = p:Decode1()
            //p:Encode1(t1)
            //p:Encode1(t2)
            //p:Encode1(t3)
            //p:Encode1(bit32.band(t4,0xB))
            //p:Encode1(1)--hardcode 1 bitfield
            //p:Encode4(skinId)
            //for i = 1, #champ do
            //    p:Encode1(string.byte(champ:sub(i,i)))
            //end
            //for i = #champ + 1, 64 do
            //    p:Encode1(0)
            //end
            //p:Hide()
            //RecvPacket(p)
        }

        static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            var reader = new BinaryReader(new MemoryStream(args.PacketData));
            byte PacketId = reader.ReadByte(); //PacketId
            if (PacketId == Skin.Header) //OLD 180
            {
                //Console.WriteLine("help");
            }
        }
    }

    class Ranges
    {
        public Ranges()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~Ranges()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Menu.Range.GetActive();
        }

        void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            DrawExperienceRanges();
            DrawAttackRanges();
            DrawTurretRanges();
            DrawQ();
            DrawW();
            DrawE();
            DrawR();
        }

        public void DrawExperienceRanges()
        {
            if (!Menu.ExperienceRange.GetActive())
                return;
            Drawing.DrawCircle(ObjectManager.Player.Position, 1400, System.Drawing.Color.LawnGreen);
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                {
                    Drawing.DrawCircle(enemy.Position, enemy.AttackRange, System.Drawing.Color.IndianRed);
                }
            }
        }

        public void DrawAttackRanges()
        {
            if (!Menu.AttackRange.GetActive())
                return;
            Drawing.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.AttackRange, System.Drawing.Color.LawnGreen);
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                {
                    Drawing.DrawCircle(enemy.Position, enemy.AttackRange, System.Drawing.Color.IndianRed);
                }
            }
        }

        public void DrawTurretRanges()
        {
            if (!Menu.TowerRange.GetActive())
                return;
            foreach (Obj_AI_Turret Turret in ObjectManager.Get<Obj_AI_Turret>())
            {
                if (Turret.IsVisible && !Turret.IsDead && Turret.IsValid && Common.IsOnScreen(Turret.ServerPosition))
                {
                    Drawing.DrawCircle(Turret.Position, 950f, Turret.IsEnemy ? System.Drawing.Color.DarkRed : System.Drawing.Color.LawnGreen);
                }
            }
        }

        public void DrawQ()
        {
            if (!Menu.SpellQRange.GetActive())
                return;
            Drawing.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange[0], System.Drawing.Color.LawnGreen);
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                {
                    Drawing.DrawCircle(enemy.Position, enemy.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange[0], System.Drawing.Color.IndianRed);
                }
            }
        }

        public void DrawW()
        {
            if (!Menu.SpellWRange.GetActive())
                return;
            Drawing.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRange[0], System.Drawing.Color.LawnGreen);
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                {
                    Drawing.DrawCircle(enemy.Position, enemy.Spellbook.GetSpell(SpellSlot.W).SData.CastRange[0], System.Drawing.Color.IndianRed);
                }
            }
        }

        public void DrawE()
        {
            if (!Menu.SpellERange.GetActive())
                return;
            Drawing.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange[0], System.Drawing.Color.LawnGreen);
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                {
                    Drawing.DrawCircle(enemy.Position, enemy.Spellbook.GetSpell(SpellSlot.E).SData.CastRange[0], System.Drawing.Color.IndianRed);
                }
            }
        }

        public void DrawR()
        {
            if (!Menu.SpellRRange.GetActive())
                return;
            Drawing.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange[0], System.Drawing.Color.LawnGreen);
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                {
                    Drawing.DrawCircle(enemy.Position, enemy.Spellbook.GetSpell(SpellSlot.R).SData.CastRange[0], System.Drawing.Color.IndianRed);
                }
            }
        }
    }

    class WardIt
    {
        private static readonly List<WardSpot> WardSpots = new List<WardSpot>();
        private static readonly List<WardItem> WardItems = new List<WardItem>();
        private WardSpot latestWardSpot = null;
        private SpellSlot latestSpellSlot = SpellSlot.Unknown;
        private bool drawSpots = false;
        private bool wardAlreadyCorrected = false;

        class WardSpot
        {
            public String Name;
            public Vector3 Pos;
            public Vector3 MagneticPos;
            public Vector3 ClickPos;
            public Vector3 MovePos;
            public bool SafeWard;

            public WardSpot(string name, Vector3 pos)
            {
                Name = name;
                Pos = pos;
                MagneticPos = new Vector3();
                ClickPos = new Vector3();
                MovePos = new Vector3();
                SafeWard = false;
            }

            public WardSpot(string name, Vector3 magneticPos, Vector3 clickPos, Vector3 pos, Vector3 movePos)
            {
                Name = name;
                Pos = pos;
                MagneticPos = magneticPos;
                ClickPos = clickPos;
                MovePos = movePos;
                SafeWard = true;
            }
        }

        class WardItem
        {
            public int Id;
            public String Name;
            public String SpellName;
            public int Range;
            public int Duration;

            public WardItem(int id, string name, string spellName, int range, int duration)
            {
                Id = id;
                Name = name;
                SpellName = spellName;
                Range = range;
                Duration = duration;
            }
        }

        public WardIt() //TODO: Add SpellNames for WardItems
        {
            WardSpots.Add(new WardSpot("BlueGolem", new Vector3(2823.37f, 7617.03f, 55.03f)));
            WardSpots.Add(new WardSpot("BlueLizard", new Vector3(7422f, 3282f, 46.53f)));
            WardSpots.Add(new WardSpot("BlueTriBush", new Vector3(10148f, 2839f, 44.41f)));
            WardSpots.Add(new WardSpot("BluePassBush", new Vector3(6269f, 4445f, 42.51f)));
            WardSpots.Add(new WardSpot("BlueRiver", new Vector3(7151.64f, 4719.66f, 51.67f)));
            WardSpots.Add(new WardSpot("BlueRiverRoundBush", new Vector3(4728f, 8336f, -51.29f)));
            WardSpots.Add(new WardSpot("BlueRiverSplitBush", new Vector3(6762.52f, 2918.75f, 55.68f)));
            WardSpots.Add(new WardSpot("PurpleGolem", new Vector3(11217.39f, 6841.89f, 54.87f)));
            WardSpots.Add(new WardSpot("PurpleLizard", new Vector3(6610.35f, 11064.61f, 54.45f)));
            WardSpots.Add(new WardSpot("PurpleTriBush", new Vector3(3883f, 11577f, 39.87f)));
            WardSpots.Add(new WardSpot("PurplePassBush", new Vector3(7775f, 10046.49f, 43.14f)));
            WardSpots.Add(new WardSpot("PurpleRiver", new Vector3(6867.68f, 9567.63f, 57.01f)));
            WardSpots.Add(new WardSpot("PurpleRoundBush", new Vector3(9720.86f, 7501.50f, 54.85f)));
            WardSpots.Add(new WardSpot("PurpleRiverRoundBush", new Vector3(9233.13f, 6094.48f, -44.63f)));
            WardSpots.Add(new WardSpot("PurpleRiverSplitPush", new Vector3(7282.69f, 11482.53f, 52.59f)));
            WardSpots.Add(new WardSpot("Dragon", new Vector3(10180.18f, 4969.32f, -62.32f)));
            WardSpots.Add(new WardSpot("DragonBush", new Vector3(8875.13f, 5390.57f, -64.07f)));
            WardSpots.Add(new WardSpot("Baron", new Vector3(3920.88f, 9477.78f, -60.42f)));
            WardSpots.Add(new WardSpot("BaronBush", new Vector3(5017.27f, 8954.09f, -62.70f)));
            WardSpots.Add(new WardSpot("PurpleBotTower2", new Vector3(12731.25f, 9132.66f, 50.32f)));
            WardSpots.Add(new WardSpot("PurpleTopTower2", new Vector3(8036.52f, 12882.94f, 45.19f)));
            WardSpots.Add(new WardSpot("PurpleMidTower1", new Vector3(9260.02f, 8582.67f, 54.62f)));
            WardSpots.Add(new WardSpot("BlueMidTower1", new Vector3(4749.79f, 5890.76f, 53.59f)));
            WardSpots.Add(new WardSpot("BlueBotTower2", new Vector3(5983.58f, 1547.98f, 52.99f)));
            WardSpots.Add(new WardSpot("BlueTopTower2", new Vector3(1213.70f, 5324.73f, 58.77f)));

            WardSpots.Add(new WardSpot("NoName", new Vector3(9641.65f, 6368.74f, 53.01f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(8081.43f, 4683.44f, 55.94f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(5943.51f, 9792.40f, 53.18f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(4379.51f, 8093.74f, 42.73f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(4222.72f, 7038.58f, 53.61f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(9068.02f, 11186.68f, 53.22f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(7970.82f, 10005.07f, 53.52f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(4978.19f, 3042.69f, 54.34f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(7907.63f, 11629.32f, 49.94f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(7556.06f, 11739.62f, 50.61f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(5973.48f, 11115.68f, 54.34f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(5732.81f, 10289.76f, 53.39f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(7969.15f, 3307.56f, 56.94f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(12073.18f, 4795.50f, 52.32f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(4044.13f, 11600.50f, 48.59f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(5597.66f, 12491.04f, 39.73f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(10070.20f, 4132.45f, -60.33f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(8320.28f, 4292.80f, 56.47f)));
            WardSpots.Add(new WardSpot("NoName", new Vector3(9603.52f, 7872.23f, 54.71f)));

            WardSpots.Add(new WardSpot("Dragon->TriBush", new Vector3(9695f, 3465f, 43.02f), new Vector3(9843.38f, 3125.16f, 43.02f), new Vector3(9946.10f, 3064.81f, 43.02f), new Vector3(9595f, 3665f, 43.02f)));
            WardSpots.Add(new WardSpot("Nashor->TriBush", new Vector3(4346.10f, 10964.81f, 36.62f), new Vector3(4214.93f, 11202.01f, 36.62f), new Vector3(4146.10f, 11314.81f, 36.62f), new Vector3(4384.36f, 10680.41f, 36.62f)));
            WardSpots.Add(new WardSpot("BlueTop->SoloBush", new Vector3(2349f, 10387f, 44.20f), new Vector3(2267.97f, 10783.37f, 44.20f), new Vector3(2446.10f, 10914.81f, 44.20f), new Vector3(2311f, 10185f, 44.20f)));
            WardSpots.Add(new WardSpot("BlueMid->RoundBush", new Vector3(4946.52f, 6474.56f, 54.71f), new Vector3(4891.98f, 6639.05f, 53.62f), new Vector3(4546.10f, 6864.81f, 53.78f), new Vector3(5217f, 6263f, 54.95f)));
            WardSpots.Add(new WardSpot("BlueMid->RiverLaneBush", new Vector3(5528.96f, 7615.20f, 45.64f), new Vector3(5688.96f, 7825.20f, 45.64f), new Vector3(5796.10f, 7914.81f, 45.64f), new Vector3(5460.13f, 7469.77f, 45.64f)));
            WardSpots.Add(new WardSpot("BlueLizard->DragonPassBush", new Vector3(7745f, 4065f, 47.71f), new Vector3(7927.65f, 4239.77f, 47.71f), new Vector3(8146.10f, 4414.81f, 47.71f), new Vector3(7645f, 4015f, 47.71f)));
            WardSpots.Add(new WardSpot("PurpleMid->RoundBush", new Vector3(9057f, 8245f, 45.73f), new Vector3(9230.77f, 7897.22f, 66.39f), new Vector3(9446.10f, 7814.81f, 54.66f), new Vector3(8895f, 8313f, 54.89f)));
            WardSpots.Add(new WardSpot("PurpleMid->RiverRoundBush", new Vector3(9025.78f, 6591.64f, 46.27f), new Vector3(9200.08f, 6425.05f, 43.21f), new Vector3(9396.10f, 6264.81f, 23.72f), new Vector3(8795f, 6815f, 56.11f)));
            WardSpots.Add(new WardSpot("PurpleMid->RiverLaneBush", new Vector3(8530.27f, 6637.38f, 46.98f), new Vector3(8539.27f, 6637.38f, 46.98f), new Vector3(8396.10f, 6464.81f, 46.98f), new Vector3(8779.17f, 6804.70f, 46.98f)));
            WardSpots.Add(new WardSpot("PurpleBot->SoloBush", new Vector3(11889f, 4205f, 42.84f), new Vector3(11974.23f, 3807.21f, 42.84f), new Vector3(11646.10f, 3464.81f, 42.84f), new Vector3(11939f, 4255f, 42.84f)));
            WardSpots.Add(new WardSpot("PurpleLizard->NashorPassBush", new Vector3(6299f, 10377.75f, 45.47f), new Vector3(6030.24f, 10292.37f, 54.29f), new Vector3(5846.10f, 10164.81f, 53.94f), new Vector3(6447f, 10463f, 54.63f)));

            WardItems.Add(new WardItem(2043, "Vision Ward", "VisionWard", 1450, 180));
            WardItems.Add(new WardItem(2044, "Stealth Ward", "SightWard", 1450, 180));
            WardItems.Add(new WardItem(3154, "Wriggle's Lantern", "WriggleLantern", 1450, 180));
            WardItems.Add(new WardItem(2045, "Ruby Sightstone", "ItemGhostWard", 1450, 180));
            WardItems.Add(new WardItem(2049, "Sightstone", "ItemGhostWard", 1450, 180));
            WardItems.Add(new WardItem(2050, "Explorer's Ward", "ItemMiniWard", 1450, 60));
            WardItems.Add(new WardItem(3340, "Greater Stealth Totem", "", 1450, 120));
            WardItems.Add(new WardItem(3361, "Greater Stealth Totem", "", 1450, 180));
            WardItems.Add(new WardItem(3362, "Greater Vision Totem", "", 1450, 180));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Game.OnGameSendPacket += Game_OnGameSendPacket;
        }

        ~WardIt()
        {
            Game.OnWndProc -= Game_OnWndProc;
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Game.OnGameSendPacket -= Game_OnGameSendPacket;
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Menu.Ward.GetActive();
        }

        class PacketSpellId
        {
            public SpellSlot SSpellSlot;
            public bool IsSummoner;

            public PacketSpellId(SpellSlot spellSlot, bool isSummoner)
            {
                SSpellSlot = spellSlot;
                IsSummoner = isSummoner;
            }

            public static PacketSpellId ConvertPacketCastToId(int id)
            {
                switch (id)
                {
                    case 128:
                        return new PacketSpellId(SpellSlot.Q, false);

                    case 129:
                        return new PacketSpellId(SpellSlot.W, false);

                    case 130:
                        return new PacketSpellId(SpellSlot.E, false);

                    case 131:
                        return new PacketSpellId(SpellSlot.R, false);

                    case 132:
                        return new PacketSpellId(SpellSlot.Item1, false);

                    case 133:
                        return new PacketSpellId(SpellSlot.Item2, false);

                    case 134:
                        return new PacketSpellId(SpellSlot.Item3, false);

                    case 145:
                        return new PacketSpellId(SpellSlot.Item4, false);

                    case 136:
                        return new PacketSpellId(SpellSlot.Item5, false);

                    case 137:
                        return new PacketSpellId(SpellSlot.Item6, false);

                    case 138:
                        return new PacketSpellId(SpellSlot.Trinket, false);

                    case 64:
                        return new PacketSpellId(SpellSlot.Q, true);

                    case 65:
                        return new PacketSpellId(SpellSlot.W, true);

                    case 192:
                        return new PacketSpellId(SpellSlot.Q, true);

                    case 193:
                        return new PacketSpellId(SpellSlot.W, true);

                    case 10:
                        return new PacketSpellId(SpellSlot.Recall, false);
                }
                return new PacketSpellId(SpellSlot.Unknown, false);
            }
        }



        void Game_OnGameSendPacket(GamePacketEventArgs args) //TODO: Add Packetsupp for wards
        {
            if (!IsActive())
                return;

            GamePacket gPacket = new GamePacket(args.PacketData);
            var reader = new BinaryReader(new MemoryStream(args.PacketData));

            byte PacketId = reader.ReadByte(); //PacketId
            if (PacketId == 0x9A) //OLD 0x9A
            {
                var mNetworkId = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                var spellId = reader.ReadByte();
                var fromX = BitConverter.ToSingle(reader.ReadBytes(4), 0);
                var fromY = BitConverter.ToSingle(reader.ReadBytes(4), 0);
                var toX = BitConverter.ToSingle(reader.ReadBytes(4), 0);
                var toY = BitConverter.ToSingle(reader.ReadBytes(4), 0);
                var tNetworkId = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                PacketSpellId nSpellId = PacketSpellId.ConvertPacketCastToId(spellId);
                if (latestSpellSlot == nSpellId.SSpellSlot && latestSpellSlot != SpellSlot.Unknown)
                {
                    drawSpots = false;
                    foreach (var wardSpot in WardSpots)
                    {
                        if (!wardSpot.SafeWard &&
                            Vector3.Distance(wardSpot.Pos,
                                new Vector3((float)fromX, (float)fromY, ObjectManager.Player.ServerPosition.Z)) <= 350 &&
                            !wardAlreadyCorrected)
                        {
                            args.Process = false;
                            wardAlreadyCorrected = true;
                            //SendPacket
                            byte[] s_castPacket = new byte[28];
                            var writer = new BinaryWriter(new MemoryStream(s_castPacket));
                            writer.Write((byte)0x9A);
                            writer.Write(mNetworkId);
                            writer.Write(spellId);
                            writer.Write((float)wardSpot.Pos.X);
                            writer.Write((float)wardSpot.Pos.Y);
                            writer.Write((float)wardSpot.Pos.X);
                            writer.Write((float)wardSpot.Pos.Y);
                            writer.Write(tNetworkId);
                            Game.SendPacket(s_castPacket, PacketChannel.C2S, PacketProtocolFlags.Reliable); //TODO: Check if its correct
                            wardAlreadyCorrected = false;
                            return;
                        }
                        else if (wardSpot.SafeWard &&
                                 Vector3.Distance(wardSpot.MagneticPos,
                                     new Vector3((float)fromX, (float)fromY, ObjectManager.Player.ServerPosition.Z)) <=
                                 100 &&
                                 !wardAlreadyCorrected)
                        {
                            args.Process = false;
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,
                                new Vector3(wardSpot.MovePos.X, wardSpot.MovePos.Y, wardSpot.MovePos.Z));
                            latestWardSpot = wardSpot;
                            return;
                        }
                    }
                }
            }
        }

        void Game_OnWndProc(WndEventArgs args)
        {
            //http://msdn.microsoft.com/en-us/library/windows/desktop/ms632585(v=vs.85).aspx
            const int WM_KEYDOWN = 0x100;
            const int WM_LBUTTONUP = 0x202;
            const int WM_LBUTTONDOWN = 0x201;
            const int WM_RBUTTONUP = 0x205;
            const int WM_RBUTTONDOWN = 0x204;

            if (!IsActive())
                return;
            if (MenuGUI.IsChatOpen == true)
                return;
            if (args.Msg == WM_KEYDOWN)
            {
                InventorySlot inventoryItem = null;
                int inventoryItemID = -1;
                switch (args.WParam)
                {
                    case '1':
                        latestSpellSlot = SpellSlot.Item1;
                        inventoryItemID = 0;
                        break;

                    case '2':
                        latestSpellSlot = SpellSlot.Item2;
                        inventoryItemID = 1;
                        break;

                    case '3':
                        latestSpellSlot = SpellSlot.Item3;
                        inventoryItemID = 2;
                        break;

                    case '4':
                        latestSpellSlot = SpellSlot.Trinket;
                        inventoryItemID = 6;
                        break;

                    case '5':
                        latestSpellSlot = SpellSlot.Item5;
                        inventoryItemID = 3;
                        break;

                    case '6':
                        latestSpellSlot = SpellSlot.Item6;
                        inventoryItemID = 4;
                        break;

                    case '7':
                        latestSpellSlot = SpellSlot.Item4;
                        inventoryItemID = 5;
                        break;

                    default:
                        drawSpots = false;
                        latestSpellSlot = SpellSlot.Unknown;
                        break;
                }
                
                foreach (var inventorySlot in ObjectManager.Player.InventoryItems)
                {
                    if (inventorySlot.Slot == inventoryItemID)
                    {
                        inventoryItem = inventorySlot;
                        break;
                    }
                }

                if (latestSpellSlot != SpellSlot.Unknown)
                {
                    if (inventoryItem != null)
                    {
                        foreach (var wardItem in WardItems)
                        {
                            if ((int)inventoryItem.Id == wardItem.Id &&
                                ObjectManager.Player.Spellbook.CanUseSpell(latestSpellSlot) == SpellState.Ready)
                            {
                                drawSpots = true;
                            }
                        }
                    }
                }
            }
            else if (args.Msg == WM_LBUTTONUP && drawSpots)
            {
                drawSpots = false;
            }
            else if (args.Msg == WM_RBUTTONDOWN && drawSpots)
            {
                drawSpots = false;
            }
            else if (args.Msg == WM_RBUTTONDOWN)
            {
                latestWardSpot = null;
            }
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            if (latestWardSpot != null)
            {
                if (Vector3.Distance(ObjectManager.Player.ServerPosition, latestWardSpot.ClickPos) <= 650)
                {
                    ObjectManager.Player.Spellbook.CastSpell(latestSpellSlot, latestWardSpot.ClickPos);
                    latestWardSpot = null;
                }
            }
        }

        void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            if (!drawSpots)
                return;
            foreach (var ward in WardSpots)
            {
                if (Common.IsOnScreen(ward.Pos))
                    Drawing.DrawCircle(ward.Pos, 50, System.Drawing.Color.GreenYellow);
                if (ward.SafeWard)
                {
                    if (Common.IsOnScreen(ward.MagneticPos))
                    {
                        Drawing.DrawCircle(ward.MagneticPos, 30, System.Drawing.Color.Red);
                        DrawArrow(ward.MagneticPos, ward.Pos, System.Drawing.Color.RoyalBlue);
                    }                    
                }
            }
        }

        void DrawArrow(Vector3 start, Vector3 end, System.Drawing.Color color) //TODO. Check if its correct calculated
        {
            float[] mPos1 = Drawing.WorldToScreen(start);
            float[] mPos2 = Drawing.WorldToScreen(end);
            Drawing.DrawLine(mPos1[0], mPos1[1], mPos2[0], mPos2[1], 1.0f, color);
            //Vector2 mmPos2 = new Vector2(mPos2[0], mPos2[1]);
            //Vector2 end1 = Geometry.Rotated(mmPos2, Geometry.DegreeToRadian(45));
            //Vector2 end2 = Geometry.Rotated(mmPos2, Geometry.DegreeToRadian(315));
            //end1.Normalize();
            //end2.Normalize();
            //end1 = Vector2.Multiply(mmPos2, end1);
            //Drawing.DrawLine(mPos2[0], mPos2[1], mmPos2.X * end1.X, mmPos2.Y * end1.Y, 1.0f, color);
            //Drawing.DrawLine(mPos2[0], mPos2[1], mmPos2.X * end2.X, mmPos2.Y * end2.Y, 1.0f, color);
            //float rad1 = Geometry.DegreeToRadian(45);
            //float cos1 = (float)Math.Cos(rad1);
            //float sin1 = (float)Math.Sin(rad1);
            //float rad2 = Geometry.DegreeToRadian(315);
            //float cos2 = (float)Math.Cos(rad2);
            //float sin2 = (float)Math.Sin(rad2);
            //Drawing.DrawLine(mPos2[0], mPos2[1], mPos1[0] * cos1 - mPos1[1] * sin1, mPos1[0] * sin1 + mPos1[1] * cos1, 1.0f, color);
            //Drawing.DrawLine(mPos2[0], mPos2[1], mPos2[0] * cos2 - mPos2[1] * sin2, mPos2[0] * sin2 + mPos2[1] * cos2, 1.0f, color);
            //double r1 = Math.Sqrt(mPos1[0] * mPos1[0] + mPos1[1] + mPos1[1]);
            //double r2 = Math.Sqrt(mPos2[0] * mPos2[0] + mPos2[1] + mPos2[1]);
            //Vector2 mPos2P = new Vector2((float)(r2 * Math.Cos(45)), (float)(r2 * Math.Sin(45)));
            //Vector2 mPos2N = new Vector2((float)(r2 * Math.Cos(-45)), (float)(r2 * Math.Sin(-45)));
            //Drawing.DrawLine(mPos2[0], mPos2[1], mPos2P.X, mPos2P.Y, 1.0f, color);
            //Drawing.DrawLine(mPos2[0], mPos2[1], mPos2N.X, mPos2N.Y, 1.0f, color);
        }
    }

    public class JungleTimer
    {
        private static Utility.Map.MapType GMapId = Utility.Map.GetMap();
        private static Inhibitor _inhibitors = null;
        private static readonly List<Relic> Relics = new List<Relic>();
        private static readonly List<Altar> Altars = new List<Altar>();
        private static readonly List<Health> Healths = new List<Health>();
        private static readonly List<JungleMob> JungleMobs = new List<JungleMob>();
        private static readonly List<JungleCamp> JungleCamps = new List<JungleCamp>();
        private static readonly List<Obj_AI_Minion> JungleMobList = new List<Obj_AI_Minion>();

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
                if(obj != null && obj.IsValid)
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

        public JungleTimer()
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
            Drawing.OnEndScene += Drawing_OnEndScene;
            InitJungleMobs();            
        }        

        ~JungleTimer()
        {
            GameObject.OnCreate -= Obj_AI_Base_OnCreate;
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Game.OnGameProcessPacket -= Game_OnGameProcessPacket;
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

        void Drawing_OnEndScene(EventArgs args)
        {
            if (!IsActive())
                return;

            if (Menu.JungleTimer.GetActive())
            {
                foreach (var jungleCamp in JungleCamps)
                {
                    if (jungleCamp.NextRespawnTime <= 0 || jungleCamp.MapId != GMapId)
                        continue;
                    float[] sPos = Drawing.WorldToMinimap(jungleCamp.MinimapPosition);
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
                        float[] sPos = Drawing.WorldToMinimap(altar.Obj.ServerPosition);
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
                        float[] sPos = Drawing.WorldToMinimap(relic.MinimapPosition);
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
                        float[] sPos = Drawing.WorldToMinimap(inhibitor.Obj.Position);
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
                        float[] sPos = Drawing.WorldToMinimap(health.Position);
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
                    if(objectType.Name.Contains("_L"))
                        Altars.Add(new Altar("Left Altar", objectType));
                    else
                        Altars.Add(new Altar("Right Altar", objectType));
                }
                    
            }

            foreach (JungleCamp jungleCamp in JungleCamps) //GAME.TIME BUGGED
            {
                if (Game.Time > 30)
                {
                    jungleCamp.NextRespawnTime = 0;
                }
                int nextRespawnTime = jungleCamp.SpawnTime - (int)Game.Time;
                if (nextRespawnTime > 0)
                {
                    jungleCamp.NextRespawnTime = nextRespawnTime;
                }
            }
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
                    if(health.Obj.IsValid)
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
                        else{}
                    else
                    {
                        if (health.NextRespawnTime < (int) Game.Time)
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
                        
                    if(nHealth != null)
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
                    else if(inhibitor.Obj.Health < 1 && inhibitor.Locked == false)
                    {
                        inhibitor.Locked = true;
                        inhibitor.NextRespawnTime = inhibitor.RespawnTime + (int) Game.Time;
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
                    float[] mPos = Drawing.WorldToScreen(ability.Owner.ServerPosition);
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

    class DestinationTracker
    {
        static readonly Dictionary<Obj_AI_Hero, List<Ability>> Enemies = new Dictionary<Obj_AI_Hero, List<Ability>>();

        public class Ability
        {
            public String SpellName;
            public int Range;
            public float Delay;
            public bool Casted;
            public int TimeCasted;
            public Vector3 StartPos;
            public Vector3 EndPos;
            public bool OutOfBush;
            public Obj_AI_Hero Owner;
            public Obj_AI_Hero Target;
            public bool TargetDead;
            public int ExtraTicks;

            public Ability(string spellName, int range, float delay, Obj_AI_Hero owner)
            {
                SpellName = spellName;
                Range = range;
                Delay = delay;
                Owner = owner;
            }
        }

        private bool AddObject(Obj_AI_Hero hero, List<Ability> abilities)
        {
            if(Enemies.ContainsKey(hero))
                return false;
            else
            {
                Enemies.Add(hero, abilities);
            }
            return true;
            //TODO:Add
        }

        public DestinationTracker()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    var abilities = new List<Ability>();
                    foreach (var spell in hero.SummonerSpellbook.Spells)
                    {
                        if (spell.Name.Contains("Flash"))
                        {
                            abilities.Add(new Ability("SummonerFlash", 400, 0, hero));
                            //AddObject(hero, abilities);
                        }
                    }

                    //abilities.Clear(); //TODO: Check if it delets the flash abilities

                    switch (hero.ChampionName)
                    {
                        case "Ezreal":
                            abilities.Add(new Ability("EzrealArcaneShift", 475, 0, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Fiora":
                            abilities.Add(new Ability("FioraDance", 700, 1, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Kassadin":
                            abilities.Add(new Ability("RiftWalk", 700, 0, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Katarina":
                            abilities.Add(new Ability("KatarinaE", 700, 0, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Leblanc":
                            abilities.Add(new Ability("LeblancSlide", 600, 0.5f, hero));
                            abilities.Add(new Ability("leblancslidereturn", 0, 0, hero));
                            abilities.Add(new Ability("LeblancSlideM", 600, 0.5f, hero));
                            abilities.Add(new Ability("leblancslidereturnm", 0, 0, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Lissandra":
                            abilities.Add(new Ability("LissandraE", 700, 0, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "MasterYi":
                            abilities.Add(new Ability("AlphaStrike", 600, 0.9f, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Shaco":
                            abilities.Add(new Ability("Deceive", 400, 0, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Talon":
                            abilities.Add(new Ability("TalonCutthroat", 700, 0, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Vayne":
                            abilities.Add(new Ability("VayneTumble", 250, 0, hero));
                            //AddObject(hero, abilities);
                            break;

                        case "Zed":
                            abilities.Add(new Ability("ZedShadowDash", 999, 0, hero));
                            //AddObject(hero, abilities);
                            break;
                    }
                    if(abilities.Count > 0)
                        AddObject(hero, abilities);
                }
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~DestinationTracker()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast -= Obj_AI_Hero_OnProcessSpellCast;
            GameObject.OnCreate -= Obj_AI_Base_OnCreate;
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Menu.DestinationTracker.GetActive();
        }

        void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var enemy in Enemies)
            {
                foreach (var ability in enemy.Value)
                {
                    if (ability.Casted)
                    {
                        float[] startPos = Drawing.WorldToScreen(ability.StartPos);
                        float[] endPos = Drawing.WorldToScreen(ability.EndPos);

                        if (ability.OutOfBush)
                        {
                            Drawing.DrawCircle(ability.EndPos, ability.Range, System.Drawing.Color.Red);
                        }
                        else
                        {
                            Drawing.DrawCircle(ability.EndPos, ability.Range, System.Drawing.Color.Red);
                            Drawing.DrawLine(startPos[0], startPos[1], endPos[0], endPos[1], 1.0f, System.Drawing.Color.Red);
                        }
                        Drawing.DrawText(endPos[0], endPos[1], System.Drawing.Color.Bisque, enemy.Key.ChampionName + " " + ability.SpellName);
                    }
                }
            }
        }

        void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (KeyValuePair<Obj_AI_Hero, List<Ability>> enemy in Enemies)
            {
                if (enemy.Key.ChampionName == "Shaco")
                {
                    if (sender.Name == "JackintheboxPoof2.troy" && !enemy.Value[0].Casted)
                    {
                        enemy.Value[0].StartPos = sender.Position;
                        enemy.Value[0].EndPos = sender.Position;
                        enemy.Value[0].Casted = true;
                        enemy.Value[0].TimeCasted = (int)Game.Time;
                        enemy.Value[0].OutOfBush = true;
                    }
                }
            }

        }

        Vector3 CalculateEndPos(Ability ability, GameObjectProcessSpellCastEventArgs args)
        {
            float dist = Vector3.Distance(args.Start, args.End);
            if (dist <= ability.Range)
            {
                ability.EndPos = args.End;
            }
            else
            {
                Vector3 norm = args.Start - args.End;
                norm.Normalize();
                Vector3 endPos = args.Start - norm * ability.Range;

                //endPos = FindNearestNonWall(); TODO: Add FindNearestNonWall

                ability.EndPos = endPos;
            }
            return ability.EndPos;
        }

        void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!IsActive())
                return;
            if (sender.GetType() == typeof(Obj_AI_Hero))
            {
                var hero = (Obj_AI_Hero)sender;
                if (hero.IsEnemy)
                {
                    Obj_AI_Hero enemy = hero;
                    foreach (KeyValuePair<Obj_AI_Hero, List<Ability>> abilities in Enemies)
                    {
                        if (abilities.Key.NetworkId != enemy.NetworkId)
                            continue;
                        int index = 0;
                        foreach (var ability in abilities.Value)
                        {
                            if (args.SData.Name == "vayneinquisition")
                            {
                                if (ability.ExtraTicks > 0)
                                {
                                    ability.ExtraTicks = (int)Game.Time + 6 + 2 * args.Level;
                                    return;
                                }
                            }
                            if (args.SData.Name == ability.SpellName)
                            {
                                switch (ability.SpellName)
                                {
                                    case "VayneTumble":
                                        if (Game.Time >= ability.ExtraTicks)
                                            return;
                                        ability.StartPos = args.Start;
                                        ability.EndPos = CalculateEndPos(ability, args);
                                        break;

                                    case "Deceive":
                                        ability.OutOfBush = false;
                                        ability.StartPos = args.Start;
                                        ability.EndPos = CalculateEndPos(ability, args);
                                        break;

                                    case "LeblancSlideM":
                                        abilities.Value[index - 2].Casted = false;
                                        ability.StartPos = abilities.Value[index - 2].StartPos;
                                        ability.EndPos = CalculateEndPos(ability, args);
                                        break;

                                    case "leblancslidereturn":
                                    case "leblancslidereturnm":
                                        if (ability.SpellName == "leblancslidereturn")
                                        {
                                            abilities.Value[index - 1].Casted = false;
                                            abilities.Value[index + 1].Casted = false;
                                            abilities.Value[index + 2].Casted = false;
                                        }
                                        else
                                        {
                                            abilities.Value[index - 3].Casted = false;
                                            abilities.Value[index - 2].Casted = false;
                                            abilities.Value[index - 1].Casted = false;
                                        }
                                        ability.StartPos = args.Start;
                                        ability.EndPos = abilities.Value[index - 1].StartPos;
                                        break;

                                    case "FioraDance":
                                    case "AlphaStrike":
                                        //TODO: Get Target
                                        //ability.Target = args.Target;
                                        ability.TargetDead = false;
                                        ability.StartPos = args.Start;
                                        //ability.EndPos = args.Target.Position;
                                        break;

                                    default:
                                        ability.StartPos = args.Start;
                                        ability.EndPos = CalculateEndPos(ability, args);
                                        break;
                                }
                                ability.Casted = true;
                                ability.TimeCasted = (int)Game.Time;
                                return;
                            }
                            index++;
                        }
                    }
                }
            }
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (KeyValuePair<Obj_AI_Hero, List<Ability>> abilities in Enemies)
            {
                foreach (var ability in abilities.Value)
                {
                    if (ability.Casted)
                    {
                        if (ability.SpellName == "FioraDance" || ability.SpellName == "AlphaStrike" &&
                            !ability.TargetDead)
                        {
                            if (Game.Time > (ability.TimeCasted + ability.Delay + 0.2))
                                ability.Casted = false;
                            /*else if (ability.Target.IsDead()) TODO: Waiting for adding Target
                            {
                                Vector3 temp = ability.EndPos;
                                ability.EndPos = ability.StartPos;
                                ability.StartPos = temp;
                                ability.TargetDead = true;
                            }*/
                            else
                            {
                                //ability.EndPos = ability.Target.ServerPosition; TODO: Waiting for adding Target
                            }
                        }
                        else if (ability.Owner.IsDead ||
                                 (!ability.Owner.IsValid && Game.Time > (ability.TimeCasted + /*variable*/ 2)) ||
                                 (ability.Owner.IsVisible &&
                                  Game.Time > (ability.TimeCasted + /*variable*/ 5 + ability.Delay)))
                        {
                            ability.Casted = false;
                        }
                        else if (!ability.OutOfBush && ability.Owner.IsVisible &&
                                 Game.Time > (ability.TimeCasted + ability.Delay))
                        {
                            ability.EndPos = ability.Owner.ServerPosition;
                        }
                    }
                }
            }
        }
    }

    class SsCaller
    {
        static readonly Dictionary<Obj_AI_Hero, Time> Enemies = new Dictionary<Obj_AI_Hero, Time>();

        public class Time
        {
            public int VisibleTime;
            public int InvisibleTime;
            public int LastTimeCalled;
            public Vector3 LastPosition;
            public bool Called;
        }

        public SsCaller()
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

        ~SsCaller()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Menu.SsCaller.GetActive() && Game.Time < (Menu.SsCaller.GetMenuItem("SAwarenessSSCallerDisableTime").GetValue<Slider>().Value * 60);
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (KeyValuePair<Obj_AI_Hero, Time> enemy in Enemies)
            {
                UpdateTime(enemy);
                HandleSs(enemy);
            }
        }

        void HandleSs(KeyValuePair<Obj_AI_Hero, Time> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            if (enemy.Value.InvisibleTime > 5 && !enemy.Value.Called && Game.Time - enemy.Value.LastTimeCalled > 30)
            {
                Vector2 pos = new Vector2(hero.Position.X, hero.Position.Y);
                Packet.PingType pingType = Packet.PingType.Normal;
                StringList t = Menu.SsCaller.GetMenuItem("SAwarenessSSCallerPingType").GetValue<StringList>();                
                //var result = Enum.TryParse(t, out pingType);
                pingType = (Packet.PingType) t.SelectedIndex + 1;
                GamePacket gPacketT = Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(enemy.Value.LastPosition.X, enemy.Value.LastPosition.Y, 0, pingType));
                for (int i = 0; i < Menu.SsCaller.GetMenuItem("SAwarenessSSCallerPingTimes").GetValue<Slider>().Value; i++)
                {
                    if (Menu.SsCaller.GetMenuItem("SAwarenessSSCallerLocalPing").GetValue<bool>())
                    {
                        //TODO: Add local ping
                    }
                    else
                    {
                        gPacketT.Send();
                    }
                    
                }
                if (Menu.SsCaller.GetMenuItem("SAwarenessSSCallerLocalChat").GetValue<bool>())
                {
                    Game.PrintChat("ss {0}", hero.ChampionName);
                }
                else
                {
                    Game.Say("ss {0}", hero.ChampionName);
                }
                enemy.Value.LastTimeCalled = (int)Game.Time;
                enemy.Value.Called = true;
            }
        }

        void UpdateTime(KeyValuePair<Obj_AI_Hero, Time> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            if (hero.IsVisible)
            {
                Enemies[hero].InvisibleTime = 0;
                Enemies[hero].VisibleTime = (int)Game.Time;
                enemy.Value.Called = false;
                Enemies[hero].LastPosition = hero.ServerPosition;
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

    class AutoLevler
    {
        private bool _usePriority;

        private int[] _priority = { 0, 0, 0, 0 };
        private int[] _sequence;

        public AutoLevler()
        {
            //LoadLevelFile();
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoLevler()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Menu.AutoLevler.GetActive();
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;

            var stringList = Menu.AutoLevler.GetMenuItem("SAwarenessAutoLevlerMode").GetValue<StringList>();
            if (stringList.SelectedIndex == 1)
            {
                _usePriority = true;
                _priority = new[]
                {
                    Menu.AutoLevler.GetMenuSettings("SAwarenessAutoLevlerPriority")
                        .GetMenuItem("SAwarenessAutoLevlerPrioritySliderQ").GetValue<Slider>().Value,
                    Menu.AutoLevler.GetMenuSettings("SAwarenessAutoLevlerPriority")
                        .GetMenuItem("SAwarenessAutoLevlerPrioritySliderW").GetValue<Slider>().Value,
                    Menu.AutoLevler.GetMenuSettings("SAwarenessAutoLevlerPriority")
                        .GetMenuItem("SAwarenessAutoLevlerPrioritySliderE").GetValue<Slider>().Value,
                    Menu.AutoLevler.GetMenuSettings("SAwarenessAutoLevlerPriority")
                        .GetMenuItem("SAwarenessAutoLevlerPrioritySliderR").GetValue<Slider>().Value
                };
            }
            else
            {
                _usePriority = false;
            }            

            Obj_AI_Hero player = ObjectManager.Player;
            SpellSlot[] spellSlotst = GetSortedPriotitySlots();
            if (player.SpellTrainingPoints > 0)
            {
                //TODO: Add level logic// try levelup spell, if fails level another up etc.
                if (_usePriority && Menu.AutoLevler.GetMenuSettings("SAwarenessAutoLevlerPriority")
                        .GetMenuItem("SAwarenessAutoLevlerPriorityActive").GetValue<bool>())
                {
                    SpellSlot[] spellSlots = GetSortedPriotitySlots();
                    for (int slotId = 0; slotId <= 3; slotId++)
                    {
                        int spellLevel = player.Spellbook.GetSpell(spellSlots[slotId]).Level;
                        player.Spellbook.LevelUpSpell(spellSlots[slotId]);
                        if (player.Spellbook.GetSpell(spellSlots[slotId]).Level != spellLevel)
                            break;
                    }
                }
                else
                {
                    if (Menu.AutoLevler.GetMenuSettings("SAwarenessAutoLevlerSequence")
                        .GetMenuItem("SAwarenessAutoLevlerSequenceActive").GetValue<bool>())
                    {
                        SpellSlot spellSlot = GetSpellSlot(_sequence[player.Level - 1]);
                        player.Spellbook.LevelUpSpell(spellSlot);
                        
                    }           
                }
            }
        }

        public void SetPriorities(int priorityQ, int priorityW, int priorityE, int priorityR)
        {
            _sequence[0] = priorityQ;
            _sequence[1] = priorityW;
            _sequence[2] = priorityE;
            _sequence[3] = priorityR;
        }

        public void SetMode(bool usePriority)
        {
            _usePriority = usePriority;
        }

        private void LoadLevelFile()
        {
            //TODO: Read Level File for sequence leveling.
            var loc = Assembly.GetExecutingAssembly().Location;
            loc = loc.Remove(loc.LastIndexOf("\\", StringComparison.Ordinal));
            loc = loc + "\\Config\\SAwareness\\autolevel.conf";
            if (!File.Exists(loc))
            {
                Download.DownloadFile("127.0.0.1", loc);
            }
            try
            {
                StreamReader sr = File.OpenText(loc);
                ReadLevelFile(sr);
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't load autolevel.conf. Using priority mode.");
                _usePriority = true;
            }
        }

        private void ReadLevelFile(StreamReader streamReader)
        {
            var sequence = new int[18];
            while (!streamReader.EndOfStream)
            {
                String line = streamReader.ReadLine();
                String champion = "";
                if (line != null && line.Length > line.IndexOf("="))
                    champion = line.Remove(line.IndexOf("="));
                if (!champion.Contains(ObjectManager.Player.ChampionName))
                    continue;
                if (line != null)
                {
                    string temp = line.Remove(0, line.IndexOf("=") + 2);
                    for (int i = 0; i < 18; i++)
                    {
                        sequence[i] = Int32.Parse(temp.Remove(1));
                        temp = temp.Remove(0, 1);
                    }
                }
                break;
            }
            _sequence = sequence;
        }

        private SpellSlot GetSpellSlot(int id)
        {
            var spellSlot = SpellSlot.Unknown;
            switch (id)
            {
                case 0:
                    spellSlot = SpellSlot.Q;
                    break;

                case 1:
                    spellSlot = SpellSlot.W;
                    break;

                case 2:
                    spellSlot = SpellSlot.E;
                    break;

                case 3:
                    spellSlot = SpellSlot.R;
                    break;
            }
            return spellSlot;
        }

        private SpellSlot[] GetSortedPriotitySlots()
        {
            var listOld = _priority;
            var listNew = new SpellSlot[4];

            listNew = ToSpellSlot(listOld, listNew);

            //listNew = listNew.OrderByDescending(c => c).ToList();



            return listNew;
        }

        private SpellSlot[] ToSpellSlot(int[] listOld, SpellSlot[] listNew)
        {
            for (int i = 0; i <= 3; i++)
            {
                switch (listOld[i])
                {
                    case 0:
                        listNew[0] = GetSpellSlot(i);
                        break;

                    case 1:
                        listNew[1] = GetSpellSlot(i);
                        break;

                    case 2:
                        listNew[2] = GetSpellSlot(i);
                        break;

                    case 3:
                        listNew[3] = GetSpellSlot(i);
                        break;
                }
            }
            return listNew;
        }

        //private List<SpellSlot> SortAlgo(List<int> listOld, List<SpellSlot> listNew)
        //{
        //    int highestPriority = -1;
        //    for (int i = 0; i < listOld.Count; i++)
        //    {
        //        int prio = _priority[i];
        //        if (highestPriority < prio)
        //        {
        //            highestPriority = prio;
        //            listNew.Add(GetSpellSlot(i));
        //            listOld.Remove(_priority[i]);
        //        }
        //    }
        //    if (listOld.Count > 1)
        //        listNew = SortAlgo(listOld, listNew);
        //    return listNew;
        //}

    }

    class HiddenObject
    {
        public class Object
        {
            public ObjectType Type;
            public String Name;
            public String ObjectName;
            public String SpellName;
            public float Duration;
            public int Id;
            public int Id2;
            public System.Drawing.Color Color;

            public Object(ObjectType type, String name, String objectName, String spellName, float duration, int id, int id2, System.Drawing.Color color)
            {
                Type = type;
                Name = name;
                ObjectName = objectName;
                SpellName = spellName;
                Duration = duration;
                Id = id;
                Id2 = id2;
                Color = color;
            }
        }

        public class ObjectData
        {
            public Object ObjectBase;
            public Vector3 Position;
            public float EndTime;
            public String Creator;
            public List<Vector2> Points;
            public int NetworkId;

            public ObjectData(Object objectBase, Vector3 position, float endTime, String creator, List<Vector2> points, int networkId)
            {
                ObjectBase = objectBase;
                Position = position;
                EndTime = endTime;
                Creator = creator;
                Points = points;
                NetworkId = networkId;
            }
        }

        public enum ObjectType
        {
            Vision,
            Sight,
            Trap
        }

        const int WardRange = 1200;
        const int TrapRange = 300;
        public List<Object> Objects = new List<Object>();
        public List<ObjectData> HidObjects = new List<ObjectData>();

        public HiddenObject()
        {
            Objects.Add(new Object(ObjectType.Vision, "Vision Ward", "VisionWard", "VisionWard", float.MaxValue, 8, 6424612, System.Drawing.Color.BlueViolet));
            Objects.Add(new Object(ObjectType.Sight, "Stealth Ward", "SightWard", "SightWard", 180.0f, 161, 234594676, System.Drawing.Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Warding Totem (Trinket)", "SightWard", "TrinketTotemLvl1", 60.0f, 56, 263796881, System.Drawing.Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Warding Totem (Trinket)", "SightWard", "trinkettotemlvl2", 120.0f, 56, 263796882, System.Drawing.Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Greater Stealth Totem (Trinket)", "SightWard", "TrinketTotemLvl3", 180.0f, 56, 263796882, System.Drawing.Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Greater Vision Totem (Trinket)", "SightWard", "TrinketTotemLvl3B", 9999.9f, 137, 194218338, System.Drawing.Color.BlueViolet));
            Objects.Add(new Object(ObjectType.Sight, "Wriggle's Lantern", "SightWard", "wrigglelantern", 180.0f, 73, 177752558, System.Drawing.Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Quill Coat", "SightWard", "", 180.0f, 73, 135609454, System.Drawing.Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Ghost Ward", "SightWard", "ItemGhostWard", 180.0f, 229, 101180708, System.Drawing.Color.Green));

            Objects.Add(new Object(ObjectType.Trap, "Yordle Snap Trap", "Cupcake Trap", "CaitlynYordleTrap", 240.0f, 62, 176176816, System.Drawing.Color.Red));
            Objects.Add(new Object(ObjectType.Trap, "Jack In The Box", "Jack In The Box", "JackInTheBox", 60.0f, 2, 44637032, System.Drawing.Color.Red));
            Objects.Add(new Object(ObjectType.Trap, "Bushwhack", "Noxious Trap", "Bushwhack", 240.0f, 9, 167611995, System.Drawing.Color.Red));
            Objects.Add(new Object(ObjectType.Trap, "Noxious Trap", "Noxious Trap", "BantamTrap", 600.0f, 48, 176304336, System.Drawing.Color.Red));

            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            GameObject.OnDelete += Obj_AI_Base_OnDelete;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;
        }

        ~HiddenObject()
        {
            Game.OnGameProcessPacket -= Game_OnGameProcessPacket;
            Obj_AI_Base.OnProcessSpellCast -= Obj_AI_Base_OnProcessSpellCast;
            GameObject.OnCreate -= GameObject_OnCreate;
            GameObject.OnDelete -= Obj_AI_Base_OnDelete;
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Menu.VisionDetector.GetActive();
        }

        void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                if (sender.Type == GameObjectType.obj_AI_Marker && ObjectManager.Player.Team != sender.Team)
                {
                    foreach (Object obj in Objects)
                    {
                        if (sender.Name == obj.ObjectName && !ObjectExist(sender.Position) )
                        {
                            HidObjects.Add(new ObjectData(obj, sender.Position, Game.Time + obj.Duration, sender.Name, null, sender.NetworkId));
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HiddenObjectCreate: " + ex.ToString());
                return;
            }
        }

        void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                for (int i = 0; i < HidObjects.Count; i++)
                {
                    ObjectData obj = HidObjects[i];
                    if (Game.Time > obj.EndTime)
                    {
                        HidObjects.RemoveAt(i);
                        break;
                    }
                    float[] objMPos = Drawing.WorldToMinimap(obj.Position);
                    float[] objPos = Drawing.WorldToScreen(obj.Position);
                    List<Vector3> posList = new List<Vector3>();
                    switch (obj.ObjectBase.Type)
                    {
                        case ObjectType.Sight:
                            Drawing.DrawCircle(obj.Position, WardRange, obj.ObjectBase.Color);
                            posList = GetVision(obj.Position, WardRange);
                            for (int j = 0; j < posList.Count; j++)
                            {
                                float[] visionPos1 = Drawing.WorldToScreen(posList[i]);
                                float[] visionPos2 = Drawing.WorldToScreen(posList[i]);
                                Drawing.DrawLine(visionPos1[0], visionPos1[1], visionPos2[0], visionPos2[1], 1.0f, obj.ObjectBase.Color);
                            }
                            Drawing.DrawText(objMPos[0], objMPos[1], obj.ObjectBase.Color, "S");
                            break;

                        case ObjectType.Trap:
                            Drawing.DrawCircle(obj.Position, TrapRange, obj.ObjectBase.Color);
                            posList = GetVision(obj.Position, TrapRange);
                            for (int j = 0; j < posList.Count; j++)
                            {
                                float[] visionPos1 = Drawing.WorldToScreen(posList[i]);
                                float[] visionPos2 = Drawing.WorldToScreen(posList[i]);
                                Drawing.DrawLine(visionPos1[0], visionPos1[1], visionPos2[0], visionPos2[1], 1.0f, obj.ObjectBase.Color);
                            }
                            Drawing.DrawText(objMPos[0], objMPos[1], obj.ObjectBase.Color, "T");
                            break;

                        case ObjectType.Vision:
                            Drawing.DrawCircle(obj.Position, WardRange, obj.ObjectBase.Color);
                            posList = GetVision(obj.Position, WardRange);
                            for (int j = 0; j < posList.Count; j++)
                            {
                                float[] visionPos1 = Drawing.WorldToScreen(posList[i]);
                                float[] visionPos2 = Drawing.WorldToScreen(posList[i]);
                                Drawing.DrawLine(visionPos1[0], visionPos1[1], visionPos2[0], visionPos2[1], 1.0f, obj.ObjectBase.Color);
                            }
                            Drawing.DrawText(objMPos[0], objMPos[1], obj.ObjectBase.Color, "V");
                            break;
                    }
                    Drawing.DrawCircle(obj.Position, 50, obj.ObjectBase.Color);
                    float endTime = obj.EndTime - Game.Time;
                    if (!float.IsInfinity(endTime) && !float.IsNaN(endTime) && endTime.CompareTo(float.MaxValue) != 0)
                    {
                        var m = (float)Math.Floor(endTime / 60);
                        var s = (float)Math.Ceiling(endTime % 60);
                        String ms = (s < 10 ? m + ":0" + s : m + ":" + s);
                        Drawing.DrawText(objPos[0], objPos[1], obj.ObjectBase.Color, ms);
                    }

                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HiddenObjectDraw: " + ex.ToString());
                return;
            }
            
        }

        List<Vector3> GetVision(Vector3 viewPos, float range) //TODO: ADD IT
        {
            List<Vector3> list = new List<Vector3>();
            //double qual = 2*Math.PI/25;
            //for (double i = 0; i < 2*Math.PI + qual;)
            //{
            //    Vector3 pos = new Vector3(viewPos.X + range * (float)Math.Cos(i), viewPos.Y - range * (float)Math.Sin(i), viewPos.Z);
            //    for (int j = 1; j < range; j = j + 25)
            //    {
            //        Vector3 nPos = new Vector3(viewPos.X + j * (float)Math.Cos(i), viewPos.Y - j * (float)Math.Sin(i), viewPos.Z);
            //        if (NavMesh.GetCollisionFlags(nPos).HasFlag(CollisionFlags.Wall))
            //        {
            //            pos = nPos;
            //            break;
            //        }
            //    }
            //    list.Add(pos);
            //    i = i + 0.1;
            //}
            return list;
        }

        Object HiddenObjectById(int id)
        {
            return Objects.FirstOrDefault(vision => id == vision.Id2);
        }

        bool ObjectExist(Vector3 pos)
        {
            return HidObjects.Any(obj => pos.Distance(obj.Position) < 30);
        }

        void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;
            try
            {               
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte PacketId = reader.ReadByte(); //PacketId
                if (PacketId == 181) //OLD 180
                {
                    int networkId = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                    var creator = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(networkId);
                    if (creator != null && creator.Team != ObjectManager.Player.Team)
                    {
                        reader.ReadBytes(7);
                        var id = reader.ReadInt32();
                        reader.ReadBytes(21);
                        networkId = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                        reader.ReadBytes(12);
                        float x = BitConverter.ToSingle(reader.ReadBytes(4), 0);
                        float y = BitConverter.ToSingle(reader.ReadBytes(4), 0);
                        float z = BitConverter.ToSingle(reader.ReadBytes(4), 0);
                        var pos = new Vector3(x, y, z);
                        Object obj = HiddenObjectById(id);
                        if (obj != null && !ObjectExist(pos))
                        {
                            if(obj.Type == ObjectType.Trap)
                                pos = new Vector3(x, z, y);
                            networkId = networkId + 2;
                            Utility.DelayAction.Add(1, () =>
                            {
                                for (int i = 0; i < HidObjects.Count; i++)
                                {
                                    var objectData = HidObjects[i];
                                    if (objectData != null && objectData.NetworkId == networkId)
                                    {
                                        var objNew = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(networkId);
                                        if(objNew != null && objNew.IsValid)
                                            objectData.Position = objNew.Position;
                                    }
                                }
                            });
                            HidObjects.Add(new ObjectData(obj, pos, Game.Time + obj.Duration, creator.Name, null, networkId));
                        }
                    }
                }
                else if (PacketId == 178)
                {
                    int networkId = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                    var gObject = ObjectManager.GetUnitByNetworkId<GameObject>(networkId);
                    if (gObject != null)
                    {
                        for (int i = 0; i < HidObjects.Count; i++)
                        {
                            var objectData = HidObjects[i];
                            if (objectData != null && objectData.NetworkId == networkId)
                            {
                                objectData.Position = gObject.Position;
                            }
                        }
                    }    
                }
                else if (PacketId == 50) //OLD 49
                {
                    int networkId = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                    for (int i = 0; i < HidObjects.Count; i++)
                    {
                        var objectData = HidObjects[i];
                        var gObject = ObjectManager.GetUnitByNetworkId<GameObject>(networkId);
                        if (objectData != null && objectData.NetworkId == networkId)
                        {
                            HidObjects.RemoveAt(i);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HiddenObjectProcess: " + ex.ToString());
                return;
            }
            
        }

        void Obj_AI_Base_OnDelete(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                for (int i = 0; i < HidObjects.Count; i++)
                {
                    ObjectData obj = HidObjects[i];
                    if (sender.Name == obj.ObjectBase.ObjectName || sender.Name.Contains("Ward") && sender.Name.Contains("Death"))
                        if(sender.Position.Distance(obj.Position) < 30 )
                        {
                            HidObjects.RemoveAt(i);
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HiddenObjectDelete: " + ex.ToString());
                return;
            }
            
        }

        void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                if (sender.Type == GameObjectType.obj_AI_Minion && ObjectManager.Player.Team != sender.Team)
                {
                    foreach (Object obj in Objects)
                    {
                        if (args.SData.Name == obj.SpellName && !ObjectExist(args.End))
                        {
                            HidObjects.Add(new ObjectData(obj, args.End, Game.Time + obj.Duration, sender.Name, null, sender.NetworkId));
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HiddenObjectSpell: " + ex.ToString());
                return;
            }
            
        }
    }

    class Recall
    {
        
        public List<RecallInfo> _recalls = new List<RecallInfo>();

        public class RecallInfo
        {
            public int NetworkId;
            public Packet.S2C.Recall.Struct Recall;
            public Packet.S2C.Recall.Struct Recall2;
            public int StartTime;

            public RecallInfo(int networkId)
            {
                NetworkId = networkId;
            }
        }

        public Recall()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsEnemy)
                {
                    _recalls.Add(new RecallInfo(enemy.NetworkId));
                }
            }
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
        }

        ~Recall()
        {
            Game.OnGameProcessPacket -= Game_OnGameProcessPacket;
        }

        public bool IsActive()
        {
            return Menu.RecallDetector.GetActive();
        }

        void Game_OnGameProcessPacket(GamePacketEventArgs args) //TODO: Check for Packet id
        {
            if (!IsActive())
                return;
            try
            {
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte PacketId = reader.ReadByte(); //PacketId
                if (PacketId != Packet.S2C.Recall.Header) //OLD 215
                    return;
                Packet.S2C.Recall.Struct recall = Packet.S2C.Recall.Decoded(args.PacketData);
                HandleRecall(recall);
            }
            catch (Exception ex)
            {
                Console.WriteLine("RecallProcess: " + ex.ToString());
                return;
            }
            
        }

        void HandleRecall(Packet.S2C.Recall.Struct recallEx)
        {
            int time = Environment.TickCount - Game.Ping;

            foreach (RecallInfo recall in _recalls)
            {
                if (recall == null) continue;
                
                if (recallEx.Type == Packet.S2C.Recall.ObjectType.Player)
                {
                    var obj = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(recall.NetworkId);
                    var objEx = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(recallEx.UnitNetworkId);
                    if(obj == null)
                        continue;
                    if (obj.NetworkId == objEx.NetworkId) //already existing
                    {
                        recall.Recall = recallEx;
                        recall.Recall2 = new Packet.S2C.Recall.Struct();
                        StringList t = Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorMode").GetValue<StringList>();
                        if(t.SelectedIndex == 0 || t.SelectedIndex == 2)
                        {
                            if (recallEx.Status == Packet.S2C.Recall.RecallStatus.TeleportStart || recallEx.Status == Packet.S2C.Recall.RecallStatus.RecallStarted)
                            {
                                recall.StartTime = (int)Game.Time;
                                if (Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorLocalChat").GetValue<bool>())
                                {
                                    Game.PrintChat(obj.ChampionName + " porting with {0} hp", (int)obj.Health);
                                }
                                else
                                {
                                    Game.Say(obj.ChampionName + " porting with {0} hp", (int)obj.Health);
                                }
                            }
                            else if (recallEx.Status == Packet.S2C.Recall.RecallStatus.TeleportEnd || recallEx.Status == Packet.S2C.Recall.RecallStatus.RecallFinished)
                            {
                                //recall.StartTime = 0;
                                if (Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorLocalChat").GetValue<bool>())
                                {
                                    Game.PrintChat(obj.ChampionName + " ported with {0} hp", (int)obj.Health);
                                }                                
                                else
                                {
                                    Game.Say(obj.ChampionName + " ported with {0} hp", (int)obj.Health);
                                }
                            }
                            else
                            {
                                //recall.StartTime = 0;
                                if (Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorLocalChat").GetValue<bool>())
                                {
                                    Game.PrintChat(obj.ChampionName + " canceled with {0} hp", (int)obj.Health);
                                }                              
                                else
                                {
                                    Game.Say(obj.ChampionName + " canceled with {0} hp", (int)obj.Health);
                                }
                            }
                        }                       
                        return;
                    }
                }
                else if (recallEx.Status == Packet.S2C.Recall.RecallStatus.TeleportStart || recallEx.Status == Packet.S2C.Recall.RecallStatus.TeleportEnd)
                {
                    if (recall.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportStart)
                        recall.Recall2 = recallEx;

                    var obj = ObjectManager.GetUnitByNetworkId<GameObject>(recallEx.UnitNetworkId);
                    var screen = obj.Position;
                    for (int i = 0; i < Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorPingTimes").GetValue<Slider>().Value; i++)
                    {
                        GamePacket gPacketT = Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(screen.X, screen.Y, 0, Packet.PingType.Danger));
                        if (Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorLocalPing").GetValue<bool>())
                        {
                            //TODO: Add local ping
                        }
                        else
                        {
                            gPacketT.Send();
                        }
                    }
                }                                
            }
        }
    }

    class CdTracker
    {
        class ChampInfos
        {

            public class Gui
            {
                public class SpriteInfos
                {
                    public Texture Texture;
                    public int Cd;
                    public Size SizeSideBar;
                    public Size SizeHpBar;
                    public Vector2 CoordsSideBar;
                    public Vector2 CoordsHpBar;
                }

                public SpriteInfos Champ = new SpriteInfos();
                public SpriteInfos SpellPassive = new SpriteInfos();
                public SpriteInfos SpellQ = new SpriteInfos();
                public SpriteInfos SpellW = new SpriteInfos();
                public SpriteInfos SpellE = new SpriteInfos();
                public SpriteInfos SpellR = new SpriteInfos();
                public SpriteInfos SpellSum1 = new SpriteInfos();
                public SpriteInfos SpellSum2 = new SpriteInfos();
                public SpriteInfos BackBar = new SpriteInfos();
                public SpriteInfos HealthBar = new SpriteInfos();
                public SpriteInfos ManaBar = new SpriteInfos();
                public SpriteInfos RecallBar = new SpriteInfos();
                public SpriteInfos[] Item = new SpriteInfos[7];
                public ItemId[] ItemId = new ItemId[7];
                public int DeathTime;
                public int VisibleTime;
                public int InvisibleTime;
                public String SHealth;
                public String SMana;
                public Vector2 Pos = new Vector2();
            }

            public Gui SGui = new Gui();
        }

        public class SpriteHelper
        {
            private static String host = "https://github.com/Screeder/SAwareness/raw/master";

            public static Texture LoadTexture(String pathAndfile, ref Texture texture, bool bForce = false)
            {
                //if (!File.Exists(pathAndfile))
                //{
                //    String filePath = pathAndfile;
                //    filePath = filePath.Remove(0, filePath.LastIndexOf("\\Sprites\\", System.StringComparison.Ordinal));
                //    Download.DownloadFile(host + filePath, pathAndfile);
                //}
                if (File.Exists(pathAndfile) && (bForce || texture == null))
                {
                    texture = Texture.FromFile(Drawing.Direct3DDevice, pathAndfile);
                    if (texture == null)
                    {
                        return null;
                    }
                }
                return texture;
            }

        }

        public CdTracker()
        {
            if(!IsActive())
                return;
            var loaded = false;
            var tries = 0;
            while (!loaded)
            {
                loaded = Init(tries >= 5);

                tries++;
                if (tries > 9)
                {
                    Console.WriteLine("Couldn't load Interface. It got disabled.");
                    Menu.CdPanel.ForceDisable = true;
                    Menu.CdPanel.Item = null;
                    return;
                }
                Thread.Sleep(10);
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnEndScene += Drawing_OnPresent;
        }

        ~CdTracker()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Drawing.OnEndScene -= Drawing_OnPresent;
        }

        Sprite S;
        Font SpellF;
        Font ChampF;
        Font SumF;
        Font RecF;
        Vector2 _screen = new Vector2(-Drawing.Width, -Drawing.Height);
        readonly Dictionary<Obj_AI_Hero, ChampInfos> Enemies = new Dictionary<Obj_AI_Hero, ChampInfos>();
        private Texture _overlaySummoner;
        private Texture _overlaySummonerSpell;
        private Texture _overlaySpellItem;
        private Texture _overlayEmptyItem;
        private Texture _overlayRecall;
        private Texture _healthBar;
        private Texture _manaBar;
        private Texture _backBar;
        Size _champSize = new Size(64, 64);
        Size _sumSize = new Size(32, 32);
        Size _spellSize = new Size(16, 16);
        Size _healthManaBarSize = new Size(96, 10);
        Size _backBarSize = new Size(96, 20);
        Size _recSize = new Size(64, 10);

        public bool IsActive()
        {
            return Menu.CdPanel.GetActive();
        }

        /*
                private static bool IsLoLActive()
                {
                    IntPtr hWnd = GetForegroundWindow();
                    int length = GetWindowTextLength(hWnd);
                    var sb = new StringBuilder(length + 1);
                    GetWindowText(hWnd, sb, sb.Capacity);
                    Console.WriteLine("\nActive window title is '{0}'", sb.ToString());
                    return sb.ToString().Contains("League of Legends");
                }
        */

        private bool Init(bool force)
        {
            try
            {
                S = new Sprite(Drawing.Direct3DDevice);
                RecF = new Font(Drawing.Direct3DDevice, new System.Drawing.Font("Times New Roman", 12));
                SpellF = new Font(Drawing.Direct3DDevice, new System.Drawing.Font("Times New Roman", 8));
                ChampF = new Font(Drawing.Direct3DDevice, new System.Drawing.Font("Times New Roman", 30));
                SumF = new Font(Drawing.Direct3DDevice, new System.Drawing.Font("Times New Roman", 16));
            }
            catch (Exception)
            {
                
                return false;
                //throw;
            }

            var loc = Assembly.GetExecutingAssembly().Location;
            loc = loc.Remove(loc.LastIndexOf("\\", StringComparison.Ordinal));
            loc = loc + "\\Sprites\\SAwareness\\";

            SpriteHelper.LoadTexture(loc + "SUMMONERS\\SummonerTint.dds", ref _overlaySummoner);
            SpriteHelper.LoadTexture(loc + "SUMMONERS\\SummonerSpellTint.dds", ref _overlaySummonerSpell);
            SpriteHelper.LoadTexture(loc + "SUMMONERS\\SpellTint.dds", ref _overlaySpellItem);

            SpriteHelper.LoadTexture(loc + "EXT\\BarBackground.dds", ref _backBar);
            SpriteHelper.LoadTexture(loc + "EXT\\HealthBar.dds", ref _healthBar);
            SpriteHelper.LoadTexture(loc + "EXT\\ManaBar.dds", ref _manaBar);
            SpriteHelper.LoadTexture(loc + "EXT\\ItemSlotEmpty.dds", ref _overlayEmptyItem);
            SpriteHelper.LoadTexture(loc + "EXT\\RecallBar.dds", ref _overlayRecall);
            

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    var champ = new ChampInfos();
                    SpriteHelper.LoadTexture(loc + "CHAMP\\" + hero.ChampionName + ".dds", ref champ.SGui.Champ.Texture);
                    var s1 = hero.Spellbook.Spells;
                    //if (File.Exists(loc + "PASSIVE\\" + s1[0].Name + ".dds") && champ.passiveTexture == null)
                    //{
                    //    champ.passiveTexture = Texture.FromFile(Drawing.Direct3DDevice, loc + "PASSIVE\\" + s1[0].Name + ".dds");
                    //    if (champ.passiveTexture == null  && champ.passiveTexture.NativePointer != null)
                    //    {
                    //        return false;
                    //    }
                    //}
                    //else
                    //{
                    //    champ.passiveTexture = overlaySpellItem;
                    //}
                    SpriteHelper.LoadTexture(loc + "SPELLS\\" + s1[0].Name + ".dds", ref champ.SGui.SpellQ.Texture);
                    SpriteHelper.LoadTexture(loc + "SPELLS\\" + s1[1].Name + ".dds", ref champ.SGui.SpellW.Texture);
                    SpriteHelper.LoadTexture(loc + "SPELLS\\" + s1[2].Name + ".dds", ref champ.SGui.SpellE.Texture);
                    SpriteHelper.LoadTexture(loc + "SPELLS\\" + s1[3].Name + ".dds", ref champ.SGui.SpellR.Texture);

                    var s2 = hero.SummonerSpellbook.Spells;
                    SpriteHelper.LoadTexture(loc + "SUMMONERS\\" + s2[0].Name + ".dds", ref champ.SGui.SpellSum1.Texture);
                    SpriteHelper.LoadTexture(loc + "SUMMONERS\\" + s2[1].Name + ".dds", ref champ.SGui.SpellSum2.Texture);

                    //champ.deathTime = 100;
                    //champ.sum1Cd = 50;
                    //champ.sum2Cd = 100;
                    Enemies.Add(hero, champ);
                }
            }
            UpdateItems();
            CalculateSizes();

            return true;
        }

        private void CalculateSizes() /*TODO: Look for http://sharpdx.org/documentation/api/p-sharpdx-direct3d9-sprite-transform 
                                       to resize sprites*/
        {
            var count = 0;
            foreach (var enemy in Enemies)
            {
                enemy.Value.SGui.Pos.X = 20;
                enemy.Value.SGui.SpellPassive.SizeSideBar = new Size((int)_screen.X + _champSize.Width + _sumSize.Width + _spellSize.Width + (int)enemy.Value.SGui.Pos.X, (int)_screen.Y / 2 - 120 + _spellSize.Height * count + count * (_backBarSize.Height) + count * (_spellSize.Height));
                enemy.Value.SGui.SpellQ.SizeSideBar = new Size(enemy.Value.SGui.SpellPassive.SizeSideBar.Width, (int)_screen.Y / 2 - 120 + _spellSize.Height * (count * 4 - 1) + count * (_backBarSize.Height) + count * (_spellSize.Height));
                enemy.Value.SGui.SpellW.SizeSideBar = new Size(enemy.Value.SGui.SpellPassive.SizeSideBar.Width, (int)_screen.Y / 2 - 120 + _spellSize.Height * (count * 4 - 2) + count * (_backBarSize.Height) + count * (_spellSize.Height));
                enemy.Value.SGui.SpellE.SizeSideBar = new Size(enemy.Value.SGui.SpellPassive.SizeSideBar.Width, (int)_screen.Y / 2 - 120 + _spellSize.Height * (count * 4 - 3) + count * (_backBarSize.Height) + count * (_spellSize.Height));
                enemy.Value.SGui.SpellR.SizeSideBar = new Size(enemy.Value.SGui.SpellPassive.SizeSideBar.Width, (int)_screen.Y / 2 - 120 + _spellSize.Height * (count * 4 - 4) + count * (_backBarSize.Height) + count * (_spellSize.Height));

                enemy.Value.SGui.Champ.SizeSideBar = new Size((int)_screen.X + _champSize.Width + _sumSize.Width + (int)enemy.Value.SGui.Pos.X, (int)_screen.Y / 2 - 120 + _champSize.Height * count + count * (_backBarSize.Height) + count * (_spellSize.Height));
                enemy.Value.SGui.SpellSum1.SizeSideBar = new Size((int)_screen.X + _sumSize.Width + (int)enemy.Value.SGui.Pos.X, (int)_screen.Y / 2 - 120 + _sumSize.Height * (count * 2) + count * (_backBarSize.Height) + count * (_spellSize.Height));
                enemy.Value.SGui.SpellSum2.SizeSideBar = new Size(enemy.Value.SGui.SpellSum1.SizeSideBar.Width, (int)_screen.Y / 2 - 120 + _sumSize.Height * (count * 2 - 1) + count * (_backBarSize.Height) + count * (_spellSize.Height));

                enemy.Value.SGui.Item[0] = new ChampInfos.Gui.SpriteInfos();
                enemy.Value.SGui.Item[0].SizeSideBar = new Size(enemy.Value.SGui.SpellR.SizeSideBar.Width, enemy.Value.SGui.SpellR.SizeSideBar.Height - _spellSize.Height);
                for (int i = 1; i < enemy.Value.SGui.Item.Length; i++)
                {
                    enemy.Value.SGui.Item[i] = new ChampInfos.Gui.SpriteInfos();
                    enemy.Value.SGui.Item[i].SizeSideBar = new Size(enemy.Value.SGui.Item[0].SizeSideBar.Width - _spellSize.Width * i, enemy.Value.SGui.Item[0].SizeSideBar.Height);
                }

                enemy.Value.SGui.SpellSum1.CoordsSideBar = new Vector2((int)-_screen.X - _sumSize.Width / 2 - (int)enemy.Value.SGui.Pos.X, (int)-_screen.Y / 2 + 120 - _sumSize.Height * count * 2 + 5 - count * (_backBarSize.Height) - count * (_spellSize.Height));
                enemy.Value.SGui.SpellSum2.CoordsSideBar = new Vector2((int)enemy.Value.SGui.SpellSum1.CoordsSideBar.X, (int)-_screen.Y / 2 + 120 - _sumSize.Height * (count * 2 - 1) + 5 - count * (_backBarSize.Height) - count * (_spellSize.Height));
                enemy.Value.SGui.Champ.CoordsSideBar = new Vector2((int)-_screen.X - _champSize.Width / 2 - _sumSize.Width - (int)enemy.Value.SGui.Pos.X, (int)-_screen.Y / 2 + 120 - _champSize.Height * count + 10 - count * (_backBarSize.Height) - count * (_spellSize.Height));
                enemy.Value.SGui.SpellPassive.CoordsSideBar = new Vector2((int)-_screen.X - _champSize.Width - _sumSize.Width - _spellSize.Width / 2 - (int)enemy.Value.SGui.Pos.X, (int)-_screen.Y / 2 + 120 - _spellSize.Height * (count * 4) - count * (_backBarSize.Height) - count * (_spellSize.Height));
                enemy.Value.SGui.SpellQ.CoordsSideBar = new Vector2((int)enemy.Value.SGui.SpellPassive.CoordsSideBar.X, (int)-_screen.Y / 2 + 120 - _spellSize.Height * (count * 4 - 1) - count * (_backBarSize.Height) - count * (_spellSize.Height));
                enemy.Value.SGui.SpellW.CoordsSideBar = new Vector2((int)enemy.Value.SGui.SpellPassive.CoordsSideBar.X, (int)-_screen.Y / 2 + 120 - _spellSize.Height * (count * 4 - 2) - count * (_backBarSize.Height) - count * (_spellSize.Height));
                enemy.Value.SGui.SpellE.CoordsSideBar = new Vector2((int)enemy.Value.SGui.SpellPassive.CoordsSideBar.X, (int)-_screen.Y / 2 + 120 - _spellSize.Height * (count * 4 - 3) - count * (_backBarSize.Height) - count * (_spellSize.Height));
                enemy.Value.SGui.SpellR.CoordsSideBar = new Vector2((int)enemy.Value.SGui.SpellPassive.CoordsSideBar.X, (int)-_screen.Y / 2 + 120 - _spellSize.Height * (count * 4 - 4) - count * (_backBarSize.Height) - count * (_spellSize.Height));

                enemy.Value.SGui.BackBar.SizeSideBar = new Size((int)_screen.X + _backBarSize.Width + (int)enemy.Value.SGui.Pos.X, enemy.Value.SGui.SpellSum2.SizeSideBar.Height - _sumSize.Height);
                enemy.Value.SGui.HealthBar.SizeSideBar = new Size((int)_screen.X + _healthManaBarSize.Width + (int)enemy.Value.SGui.Pos.X, enemy.Value.SGui.BackBar.SizeSideBar.Height);
                enemy.Value.SGui.ManaBar.SizeSideBar = new Size((int)_screen.X + _healthManaBarSize.Width + (int)enemy.Value.SGui.Pos.X, enemy.Value.SGui.BackBar.SizeSideBar.Height - _healthManaBarSize.Height + 2);
                enemy.Value.SGui.SHealth = ((int)enemy.Key.Health) + "/" + ((int)enemy.Key.MaxHealth);
                enemy.Value.SGui.SMana = ((int)enemy.Key.Mana) + "/" + ((int)enemy.Key.MaxMana);
                enemy.Value.SGui.HealthBar.CoordsSideBar = new Vector2((int)-_screen.X - _healthManaBarSize.Width / 2 - (int)enemy.Value.SGui.Pos.X, (int)-_screen.Y / 2 + (120 + _champSize.Height - 2) - _champSize.Height * count - count * (_backBarSize.Height) - count * (_spellSize.Height));
                enemy.Value.SGui.ManaBar.CoordsSideBar = new Vector2(enemy.Value.SGui.HealthBar.CoordsSideBar.X, (int)-_screen.Y / 2 + (120 + _champSize.Height + _healthManaBarSize.Height - 4) - _champSize.Height * count - count * (_backBarSize.Height) - count * (_spellSize.Height));

                enemy.Value.SGui.RecallBar.SizeSideBar = new Size((int)enemy.Value.SGui.Champ.SizeSideBar.Width, (int)enemy.Value.SGui.BackBar.SizeSideBar.Height + _champSize.Height / 4 - 5);
                enemy.Value.SGui.RecallBar.CoordsSideBar = new Vector2((int)enemy.Value.SGui.Champ.CoordsSideBar.X, (int)enemy.Value.SGui.Champ.CoordsSideBar.Y - 5);

                count++;
            }
        }

        private void UpdateItems()
        {
            if (!Menu.CdPanel.GetMenuItem("SAwarenessItemPanelActive").GetValue<bool>())
                return;
            var loc = Assembly.GetExecutingAssembly().Location;
            loc = loc.Remove(loc.LastIndexOf("\\", StringComparison.Ordinal));
            loc = loc + "\\Sprites\\SAwareness\\";

            foreach (var enemy in Enemies)
            {
                InventorySlot[] i1 = enemy.Key.InventoryItems;
                var champ = enemy.Value;
                var slot = new List<int>();
                var unusedId = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
                foreach (var inventorySlot in i1)
                {
                    if (File.Exists(loc + "ITEMS\\" + inventorySlot.Id + ".dds"))
                    {
                        slot.Add(inventorySlot.Slot);

                        //for (int i = 0; i < enemy.Value.SGui.Item.Length; i++)
                        //{
                            if (inventorySlot.Slot >= 0 && inventorySlot.Slot <= 6)
                            {
                                unusedId.Remove(inventorySlot.Slot);
                                if (champ.SGui.Item[inventorySlot.Slot] == null)
                                    champ.SGui.Item[inventorySlot.Slot] = new ChampInfos.Gui.SpriteInfos();
                                //if (hamp.SGui.ItemId[i] == null)
                                //    champ.SGui.Item[i] = new ChampInfos.Gui.SpriteInfos();
                                if (champ.SGui.Item[inventorySlot.Slot].Texture == null || champ.SGui.ItemId[inventorySlot.Slot] != inventorySlot.Id)
                                {
                                    champ.SGui.ItemId[inventorySlot.Slot] = inventorySlot.Id;
                                    SpriteHelper.LoadTexture(loc + "ITEMS\\" + inventorySlot.Id + ".dds", ref champ.SGui.Item[inventorySlot.Slot].Texture, true);
                                    //champ.SGui.Item[i].Texture = Texture.FromFile(Drawing.Direct3DDevice,
                                    //    loc + "ITEMS\\" + inventorySlot.Id + ".dds");
                                }
                            }
                        //}
                    }
                }

                for (int i = 0; i < unusedId.Count; i++)
                {
                    int id = unusedId[i];
                    champ.SGui.ItemId[id] = 0;
                    if (champ.SGui.Item[id] == null)
                        champ.SGui.Item[id] = new ChampInfos.Gui.SpriteInfos();
                    champ.SGui.Item[id].Texture = null;
                    if (/*id == i*/champ.SGui.Item[id].Texture == null && champ.SGui.Item[id].Texture != _overlayEmptyItem)
                    {
                        champ.SGui.Item[id].Texture = _overlayEmptyItem;
                    }
                }
            }
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                UpdateItems();

                if (ObjectManager.Player.DeathDuration > 0.0f)
                {

                }
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                    foreach (var enemy in Enemies)
                    {
                        enemy.Value.SGui.SHealth = ((int)enemy.Key.Health) + "/" + ((int)enemy.Key.MaxHealth);
                        enemy.Value.SGui.SMana = ((int)enemy.Key.Mana) + "/" + ((int)enemy.Key.MaxMana);
                        if (enemy.Key.NetworkId == hero.NetworkId)
                        {
                            //InventorySlot[] i1 = hero.InventoryItems; TODO: Add Item Cooldowns

                            var s1 = hero.Spellbook.Spells;
                            if (s1[0].CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SGui.SpellQ.Cd = (int)(s1[0].CooldownExpires - Game.Time);
                            }
                            if (s1[1].CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SGui.SpellW.Cd = (int)(s1[1].CooldownExpires - Game.Time);
                            }
                            if (s1[2].CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SGui.SpellE.Cd = (int)(s1[2].CooldownExpires - Game.Time);
                            }
                            if (s1[3].CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SGui.SpellR.Cd = (int)(s1[3].CooldownExpires - Game.Time);
                            }
                            var s2 = hero.SummonerSpellbook.Spells;
                            if (s2[0].CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SGui.SpellSum1.Cd = (int)(s2[0].CooldownExpires - Game.Time);
                            }
                            if (s2[1].CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SGui.SpellSum2.Cd = (int)(s2[1].CooldownExpires - Game.Time);
                            }
                            if (hero.DeathDuration - Game.Time > 0.0f)
                            {
                                enemy.Value.SGui.DeathTime = (int)(hero.DeathDuration - Game.Time);
                            }
                            if (hero.IsVisible)
                            {
                                enemy.Value.SGui.InvisibleTime = 0;
                                enemy.Value.SGui.VisibleTime = (int)Game.Time;
                            }
                            else
                            {
                                if (enemy.Value.SGui.VisibleTime != 0)
                                {
                                    enemy.Value.SGui.InvisibleTime = (int)(Game.Time - enemy.Value.SGui.VisibleTime);
                                }
                                else
                                {
                                    enemy.Value.SGui.InvisibleTime = 0;
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("CDTrackerUpdate: " + ex.ToString());
                throw;
            }
            
        }

        float CalcHpBar(Obj_AI_Hero hero)
        {
            float percent = (100 / hero.MaxHealth * hero.Health);
            return percent / 100;
        }

        float CalcManaBar(Obj_AI_Hero hero)
        {
            float percent = (100 / hero.MaxMana * hero.Mana);
            return (percent <= 0 || Single.IsNaN(percent) ? 0 : percent / 100);
        }

        float CalcRecallBar(Recall.RecallInfo recall)
        {
            float maxTime = (recall.Recall.Duration/1000);
            float percent = (100 / maxTime * (Game.Time - recall.StartTime));
            return (percent <= 100 ? percent / 100 : 1);
        }

        Recall.RecallInfo GetRecall(int networkId)
        {
            StringList t = Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorMode").GetValue<StringList>();
            if (t.SelectedIndex == 1 || t.SelectedIndex == 2)
            {
                Recall recall = (Recall)Menu.RecallDetector.Item;
                if (recall == null)
                    return null;
                foreach (var info in recall._recalls)
                {
                    if (info.NetworkId == networkId)
                    {
                        return info;
                    }
                }
            }
            return null;
        }

        void Drawing_OnPresent(EventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                if (S.IsDisposed)
                {
                    return;
                }
                S.Begin();
                foreach (var enemy in Enemies)
                {
                    var percentHealth = CalcHpBar(enemy.Key);
                    var percentMana = CalcManaBar(enemy.Key);

                    //DrawSprite(S, enemy.Value.PassiveTexture, nPassiveSize, Color.White);
                    DirectXDrawer.DrawSprite(S, enemy.Value.SGui.SpellQ.Texture, enemy.Value.SGui.SpellQ.SizeSideBar, Color.White);
                    DirectXDrawer.DrawSprite(S, enemy.Value.SGui.SpellW.Texture, enemy.Value.SGui.SpellW.SizeSideBar, Color.White);
                    DirectXDrawer.DrawSprite(S, enemy.Value.SGui.SpellE.Texture, enemy.Value.SGui.SpellE.SizeSideBar, Color.White);
                    DirectXDrawer.DrawSprite(S, enemy.Value.SGui.SpellR.Texture, enemy.Value.SGui.SpellR.SizeSideBar, Color.White);

                    DirectXDrawer.DrawSprite(S, enemy.Value.SGui.Champ.Texture, enemy.Value.SGui.Champ.SizeSideBar, Color.White);
                    DirectXDrawer.DrawSprite(S, enemy.Value.SGui.SpellSum1.Texture, enemy.Value.SGui.SpellSum1.SizeSideBar, Color.White);
                    DirectXDrawer.DrawSprite(S, enemy.Value.SGui.SpellSum2.Texture, enemy.Value.SGui.SpellSum2.SizeSideBar, Color.White);

                    DirectXDrawer.DrawSprite(S, _backBar, enemy.Value.SGui.BackBar.SizeSideBar, Color.White, new Rectangle(0, 0, _backBarSize.Width, _backBarSize.Height - 4));
                    DirectXDrawer.DrawSprite(S, _healthBar, enemy.Value.SGui.HealthBar.SizeSideBar, Color.White, new Rectangle(0, 0, (int)(_healthManaBarSize.Width * percentHealth), _healthManaBarSize.Height - 2));
                    DirectXDrawer.DrawSprite(S, _manaBar, enemy.Value.SGui.ManaBar.SizeSideBar, Color.White, new Rectangle(0, 0, (int)(_healthManaBarSize.Width * percentMana), _healthManaBarSize.Height - 2));

                    if (Menu.CdPanel.GetMenuItem("SAwarenessItemPanelActive").GetValue<bool>())
                    {
                        foreach (var spriteInfo in enemy.Value.SGui.Item)
                        {
                            DirectXDrawer.DrawSprite(S, spriteInfo.Texture, spriteInfo.SizeSideBar, Color.White);
                        }
                    }

                    if (enemy.Value.SGui.SpellQ.Cd > 0.0f || enemy.Key.Spellbook.GetSpell(SpellSlot.Q).Level < 1)
                    {
                        DirectXDrawer.DrawSprite(S, _overlaySpellItem, enemy.Value.SGui.SpellQ.SizeSideBar, new ColorBGRA(Color3.White, 0.55f));
                    }
                    if (enemy.Value.SGui.SpellW.Cd > 0.0f || enemy.Key.Spellbook.GetSpell(SpellSlot.W).Level < 1)
                    {
                        DirectXDrawer.DrawSprite(S, _overlaySpellItem, enemy.Value.SGui.SpellW.SizeSideBar, new ColorBGRA(Color3.White, 0.55f));
                    }
                    if (enemy.Value.SGui.SpellE.Cd > 0.0f || enemy.Key.Spellbook.GetSpell(SpellSlot.E).Level < 1)
                    {
                        DirectXDrawer.DrawSprite(S, _overlaySpellItem, enemy.Value.SGui.SpellE.SizeSideBar, new ColorBGRA(Color3.White, 0.55f));
                    }
                    if (enemy.Value.SGui.SpellR.Cd > 0.0f || enemy.Key.Spellbook.GetSpell(SpellSlot.R).Level < 1)
                    {
                        DirectXDrawer.DrawSprite(S, _overlaySpellItem, enemy.Value.SGui.SpellR.SizeSideBar, new ColorBGRA(Color3.White, 0.55f));
                    }
                    if (enemy.Value.SGui.DeathTime > 0.0f)
                    {
                        DirectXDrawer.DrawSprite(S, _overlaySummoner, enemy.Value.SGui.Champ.SizeSideBar, new ColorBGRA(Color3.White, 0.55f));
                    }
                    if (enemy.Value.SGui.SpellSum1.Cd > 0.0f)
                    {
                        DirectXDrawer.DrawSprite(S, _overlaySummonerSpell, enemy.Value.SGui.SpellSum1.SizeSideBar, new ColorBGRA(Color3.White, 0.55f));
                    }
                    if (enemy.Value.SGui.SpellSum2.Cd > 0.0f)
                    {
                        DirectXDrawer.DrawSprite(S, _overlaySummonerSpell, enemy.Value.SGui.SpellSum2.SizeSideBar, new ColorBGRA(Color3.White, 0.55f));
                    }
                    if (Menu.RecallDetector.GetActive())
                    {
                        Recall.RecallInfo info = GetRecall(enemy.Key.NetworkId);
                        var percentRecall = CalcRecallBar(info);
                        if (info != null && info.StartTime != 0)
                        {
                            float time = Game.Time + info.Recall.Duration / 1000 - info.StartTime;
                            if (time > 0.0f && (info.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportStart || info.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallStarted))
                            {
                                Rectangle rec = new Rectangle(enemy.Value.SGui.RecallBar.SizeSideBar.Width, enemy.Value.SGui.RecallBar.SizeSideBar.Height, (int)(_recSize.Width * percentRecall), _recSize.Height);
                                DirectXDrawer.DrawSprite(S, _overlayRecall, enemy.Value.SGui.RecallBar.SizeSideBar, new ColorBGRA(Color3.White, 0.80f), rec);
                            }
                            else if (time < 30.0f && (info.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportEnd || info.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallFinished))
                            {
                                Rectangle rec = new Rectangle(enemy.Value.SGui.RecallBar.SizeSideBar.Width, enemy.Value.SGui.RecallBar.SizeSideBar.Height, _recSize.Width, _recSize.Height);
                                DirectXDrawer.DrawSprite(S, _overlayRecall, enemy.Value.SGui.RecallBar.SizeSideBar, new ColorBGRA(Color3.White, 0.80f), rec);
                            }
                            else if (time < 30.0f && (info.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportAbort || info.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallAborted))
                            {
                                Rectangle rec = new Rectangle(enemy.Value.SGui.RecallBar.SizeSideBar.Width, enemy.Value.SGui.RecallBar.SizeSideBar.Height, _recSize.Width, _recSize.Height);
                                DirectXDrawer.DrawSprite(S, _overlayRecall, enemy.Value.SGui.RecallBar.SizeSideBar, new ColorBGRA(Color3.White, 0.80f), rec);
                            }
                        }
                    }
                }
                S.End();
                foreach (var enemy in Enemies)
                {
                    DirectXDrawer.DrawText(SpellF, enemy.Value.SGui.SHealth, (int)enemy.Value.SGui.HealthBar.CoordsSideBar.X, (int)enemy.Value.SGui.HealthBar.CoordsSideBar.Y, Color.Orange);
                    DirectXDrawer.DrawText(SpellF, enemy.Value.SGui.SMana, (int)enemy.Value.SGui.ManaBar.CoordsSideBar.X, (int)enemy.Value.SGui.ManaBar.CoordsSideBar.Y, Color.Orange);
                    if (enemy.Value.SGui.SpellQ.Cd > 0.0f)
                    {
                        DirectXDrawer.DrawText(SpellF, enemy.Value.SGui.SpellQ.Cd.ToString(), (int)enemy.Value.SGui.SpellQ.CoordsSideBar.X, (int)enemy.Value.SGui.SpellQ.CoordsSideBar.Y, Color.Orange);
                    }
                    if (enemy.Value.SGui.SpellW.Cd > 0.0f)
                    {
                        DirectXDrawer.DrawText(SpellF, enemy.Value.SGui.SpellW.Cd.ToString(), (int)enemy.Value.SGui.SpellW.CoordsSideBar.X, (int)enemy.Value.SGui.SpellW.CoordsSideBar.Y, Color.Orange);
                    }
                    if (enemy.Value.SGui.SpellE.Cd > 0.0f)
                    {
                        DirectXDrawer.DrawText(SpellF, enemy.Value.SGui.SpellE.Cd.ToString(), (int)enemy.Value.SGui.SpellE.CoordsSideBar.X, (int)enemy.Value.SGui.SpellE.CoordsSideBar.Y, Color.Orange);
                    }
                    if (enemy.Value.SGui.SpellR.Cd > 0.0f)
                    {
                        DirectXDrawer.DrawText(SpellF, enemy.Value.SGui.SpellR.Cd.ToString(), (int)enemy.Value.SGui.SpellR.CoordsSideBar.X, (int)enemy.Value.SGui.SpellR.CoordsSideBar.Y, Color.Orange);
                    }
                    if (enemy.Value.SGui.DeathTime > 0.0f && enemy.Key.IsDead)
                    {
                        DirectXDrawer.DrawText(ChampF, enemy.Value.SGui.DeathTime.ToString(), (int)enemy.Value.SGui.Champ.CoordsSideBar.X, (int)enemy.Value.SGui.Champ.CoordsSideBar.Y, Color.Orange);
                    }
                    else if (enemy.Value.SGui.InvisibleTime > 0.0f && !enemy.Key.IsVisible)
                    {
                        DirectXDrawer.DrawText(ChampF, enemy.Value.SGui.InvisibleTime.ToString(), (int)enemy.Value.SGui.Champ.CoordsSideBar.X, (int)enemy.Value.SGui.Champ.CoordsSideBar.Y, Color.Red);
                    }
                    if (enemy.Value.SGui.SpellSum1.Cd > 0.0f)
                    {
                        DirectXDrawer.DrawText(SumF, enemy.Value.SGui.SpellSum1.Cd.ToString(), (int)enemy.Value.SGui.SpellSum1.CoordsSideBar.X, (int)enemy.Value.SGui.SpellSum1.CoordsSideBar.Y, Color.Orange);
                    }
                    if (enemy.Value.SGui.SpellSum2.Cd > 0.0f)
                    {
                        DirectXDrawer.DrawText(SumF, enemy.Value.SGui.SpellSum2.Cd.ToString(), (int)enemy.Value.SGui.SpellSum2.CoordsSideBar.X, (int)enemy.Value.SGui.SpellSum2.CoordsSideBar.Y, Color.Orange);
                    }
                    if (Menu.RecallDetector.GetActive())
                    {
                        Recall.RecallInfo info = GetRecall(enemy.Key.NetworkId);
                        if (info != null && info.StartTime != 0)
                        {
                            float time = Game.Time + info.Recall.Duration / 1000 - info.StartTime;
                            if (time > 0.0f && (info.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportStart || info.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallStarted))
                            {
                                DirectXDrawer.DrawText(RecF, "Porting", (int)enemy.Value.SGui.Champ.CoordsSideBar.X, (int)enemy.Value.SGui.Champ.CoordsSideBar.Y +
                                    (int)_champSize.Height / 2 + 5, Color.Chartreuse);
                            }
                            else if (time < 30.0f && (info.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportEnd || info.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallFinished))
                            {
                                DirectXDrawer.DrawText(RecF, "Ported", (int)enemy.Value.SGui.Champ.CoordsSideBar.X, (int)enemy.Value.SGui.Champ.CoordsSideBar.Y +
                                    (int)_champSize.Height / 2 + 5, Color.Chartreuse);
                            }
                            else if (time < 30.0f && (info.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportAbort || info.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallAborted))
                            {
                                DirectXDrawer.DrawText(RecF, "Canceled", (int)enemy.Value.SGui.Champ.CoordsSideBar.X, (int)enemy.Value.SGui.Champ.CoordsSideBar.Y +
                                    (int)_champSize.Height / 2 + 5, Color.Chartreuse);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    class WaypointTracker
    {
        public WaypointTracker()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~WaypointTracker()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Menu.WaypointTracker.GetActive();
        }

        void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                float arrivalTime = 0.0f;
                if (enemy.IsValid && enemy.IsVisible && !enemy.IsDead && enemy.IsEnemy)
                {
                    var waypoints = enemy.GetWaypoints();
                    for (int i = 0; i < waypoints.Count - 1; i++)
                    {
                        float[] oWp;
                        float[] nWp;
                        float time = 0;
                        oWp = Drawing.WorldToScreen(waypoints[i].To3D());
                        nWp = Drawing.WorldToScreen(waypoints[i + 1].To3D());
                        Drawing.DrawLine(oWp[0], oWp[1], nWp[0], nWp[1], 1, System.Drawing.Color.White);
                        time =
                                ((Vector3.Distance(waypoints[i].To3D(), waypoints[i + 1].To3D()) /
                                  (ObjectManager.Player.MoveSpeed/1000)) /1000);
                        time = (float)Math.Round(time, 2);
                        arrivalTime += time;
                        if (i == enemy.Path.Length - 1)
                        {
                            DrawCross(nWp[0], nWp[1], 1.0f, 3.0f, System.Drawing.Color.Red);
                            Drawing.DrawText(nWp[0] - 15, nWp[1] + 10, System.Drawing.Color.Red, arrivalTime.ToString());
                        }
                    }
                }                
            }           
        }

        void DrawCross(float x, float y, float size, float thickness, System.Drawing.Color color)
        {
            var topLeft = new Vector2(x - 10 * size, y - 10 * size);
            var topRight = new Vector2(x + 10 * size, y - 10 * size);
            var botLeft = new Vector2(x - 10 * size, y + 10 * size);
            var botRight = new Vector2(x + 10 * size, y + 10 * size);

            Drawing.DrawLine(topLeft.X, topLeft.Y, botRight.X, botRight.Y, thickness, color);
            Drawing.DrawLine(topRight.X, topRight.Y, botLeft.X, botLeft.Y, thickness, color);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CreateMenu();
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
                LeagueSharp.Common.Menu menu = new LeagueSharp.Common.Menu("SAwareness", "SAwareness", true);

                Menu.Timers.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Timers", "SAwarenessTimers"));
                Menu.Timers.MenuItems.Add(Menu.Timers.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTimersPingTimes", "Ping Times").SetValue(new Slider(0, 5, 0))));
                Menu.Timers.MenuItems.Add(Menu.Timers.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTimersRemindTime", "Remind Time").SetValue(new Slider(0, 50, 0))));
                Menu.Timers.MenuItems.Add(Menu.Timers.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTimersLocalPing", "Local Ping | Not implemented").SetValue(false)));
                Menu.Timers.MenuItems.Add(Menu.Timers.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTimersLocalChat", "Local Chat").SetValue(false)));
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
                Menu.CdPanel.Menu = Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("CDTracker", "SAwarenessCDTracker"));
                Menu.CdPanel.MenuItems.Add(Menu.CdPanel.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessItemPanelActive", "ItemPanel").SetValue(true)));
                Menu.CdPanel.MenuItems.Add(Menu.CdPanel.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessCDTrackerActive", "Active").SetValue(true)));
                Menu.SsCaller.Menu = Menu.Tracker.Menu.AddSubMenu(new LeagueSharp.Common.Menu("SSCaller", "SAwarenessSSCaller"));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerPingTimes", "Ping Times").SetValue(new Slider(0, 5, 0))));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerPingType", "Ping Type").SetValue(new StringList(new string[] { "Normal", "Danger", "EnemyMissing", "OnMyWay", "Fallback", "AssistMe" }))));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerLocalPing", "Local Ping | Not implemented").SetValue(false)));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerLocalChat", "Local Chat").SetValue(false)));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerDisableTime", "Disable Time").SetValue(new Slider(20, 180, 1))));
                Menu.SsCaller.MenuItems.Add(Menu.SsCaller.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSSCallerActive", "Active").SetValue(true)));
                Menu.Tracker.MenuItems.Add(Menu.Tracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTrackerActive", "Active").SetValue(true)));

                Menu.Detector.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Detector", "SAwarenessDetector"));
                Menu.VisionDetector.Menu = Menu.Detector.Menu.AddSubMenu(new LeagueSharp.Common.Menu("VisionDetector", "SAwarenessVisionDetector"));
                Menu.VisionDetector.MenuItems.Add(Menu.VisionDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessVisionDetectorActive", "Active").SetValue(true)));
                Menu.RecallDetector.Menu = Menu.Detector.Menu.AddSubMenu(new LeagueSharp.Common.Menu("RecallDetector", "SAwarenessRecallDetector"));
                Menu.RecallDetector.MenuItems.Add(Menu.RecallDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRecallDetectorPingTimes", "Ping Times").SetValue(new Slider(0, 5, 0))));
                Menu.RecallDetector.MenuItems.Add(Menu.RecallDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRecallDetectorLocalPing", "Local Ping | Not implemented").SetValue(false)));
                Menu.RecallDetector.MenuItems.Add(Menu.RecallDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRecallDetectorLocalChat", "Local Chat").SetValue(false)));
                Menu.RecallDetector.MenuItems.Add(Menu.RecallDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRecallDetectorMode", "Mode").SetValue(new StringList(new string[] { "Chat", "CDTracker", "Both" }))));
                Menu.RecallDetector.MenuItems.Add(Menu.RecallDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessRecallDetectorActive", "Active").SetValue(true)));
                Menu.Detector.MenuItems.Add(Menu.Detector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessDetectorActive", "Active").SetValue(true)));

                Menu.Ganks.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Ganks | Not implemented", "SAwarenessGanks"));
                Menu.GankTracker.Menu = Menu.Ganks.Menu.AddSubMenu(new LeagueSharp.Common.Menu("GankTracker", "SAwarenessGankTracker"));
                Menu.GankTracker.MenuItems.Add(Menu.GankTracker.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGankTrackerActive", "Active").SetValue(false)));
                Menu.GankDetector.Menu = Menu.Ganks.Menu.AddSubMenu(new LeagueSharp.Common.Menu("GankDetector", "SAwarenessGankDetector"));
                Menu.GankDetector.MenuItems.Add(Menu.GankDetector.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGankDetectorActive", "Active").SetValue(false)));
                Menu.Ganks.MenuItems.Add(Menu.Ganks.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessGanksActive", "Active").SetValue(false)));

                Menu.Health.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("Object Health", "SAwarenessObjectHealth"));
                Menu.TowerHealth.Menu = Menu.Health.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Tower Health", "SAwarenessTowerHealth"));
                Menu.TowerHealth.MenuItems.Add(Menu.TowerHealth.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessTowerHealthActive", "Active").SetValue(true)));
                Menu.InhibitorHealth.Menu = Menu.Health.Menu.AddSubMenu(new LeagueSharp.Common.Menu("Inhibitor Health", "SAwarenessInhibitorHealth"));
                Menu.InhibitorHealth.MenuItems.Add(Menu.InhibitorHealth.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessInhibitorHealthActive", "Active").SetValue(false)));
                Menu.Health.MenuItems.Add(Menu.Health.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessHealthMode", "Mode").SetValue(new StringList(new string[] { "Percent", "Normal" }))));
                Menu.Health.MenuItems.Add(Menu.Health.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessHealthActive", "Active").SetValue(true)));

                //Maybe in Misc together
                Menu.AutoLevler.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoLevler", "SAwarenessAutoLevler"));
                tempSettings = Menu.AutoLevler.AddMenuItemSettings("Priority",
                    "SAwarenessAutoLevlerPriority");
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerPrioritySliderQ", "Q").SetValue(new Slider(0, 3, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerPrioritySliderW", "W").SetValue(new Slider(0, 3, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerPrioritySliderE", "E").SetValue(new Slider(0, 3, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerPrioritySliderR", "R").SetValue(new Slider(0, 3, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerPriorityActive", "Active").SetValue(false).DontSave()));
                tempSettings = Menu.AutoLevler.AddMenuItemSettings("Sequence",
                    "SAwarenessAutoLevlerSequence");
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerSequenceLoadChampion", "Load Champion").SetValue(false)));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerSequenceActive", "Active").SetValue(false)));
                Menu.AutoLevler.MenuItems.Add(Menu.AutoLevler.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerMode", "Mode").SetValue(new StringList(new string[] { "Sequence", "Priority" }))));
                Menu.AutoLevler.MenuItems.Add(Menu.AutoLevler.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoLevlerActive", "Active").SetValue(true)));
                Menu.AutoSmite.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoSmite", "SAwarenessAutoSmite"));
                Menu.AutoSmite.MenuItems.Add(Menu.AutoSmite.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoSmiteActive", "Active").SetValue(true)));
                Menu.Ward.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("WardPlacer", "SAwarenessWardPlacer"));
                Menu.Ward.MenuItems.Add(Menu.Ward.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessWardPlacerActive", "Active").SetValue(true)));
                Menu.SkinChanger.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("SkinChanger", "SAwarenessSkinChanger"));
                Menu.SkinChanger.MenuItems.Add(Menu.SkinChanger.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSkinChangerSlider", "Skin").SetValue(new Slider(0, 2, 0))));
                Menu.SkinChanger.MenuItems.Add(Menu.SkinChanger.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessSkinChangerActive", "Active").SetValue(false)));
                Menu.AutoPot.Menu = menu.AddSubMenu(new LeagueSharp.Common.Menu("AutoPot", "SAwarenessAutoPot"));
                tempSettings = Menu.AutoPot.AddMenuItemSettings("HealthPot",
                    "SAwarenessAutoPotHealthPot");
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoPotHealthPotPercent", "Health Percent").SetValue(new Slider(20, 99, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoPotHealthPotActive", "Active").SetValue(false)));
                tempSettings = Menu.AutoPot.AddMenuItemSettings("ManaPot",
                    "SAwarenessAutoPotManaPot");
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoPotManaPotPercent", "Mana Percent").SetValue(new Slider(20, 99, 0))));
                tempSettings.MenuItems.Add(tempSettings.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoPotManaPotActive", "Active").SetValue(false)));
                Menu.AutoPot.MenuItems.Add(Menu.AutoPot.Menu.AddItem(new LeagueSharp.Common.MenuItem("SAwarenessAutoPotActive", "Active").SetValue(true)));

                menu.AddItem(new LeagueSharp.Common.MenuItem("By Mariopart", "By Mariopart V0.2 Bugged Shit!"));
                menu.AddToMainMenu();
            }
            catch (Exception)
            {
                
                throw;
            }            
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                //Game.Say("TestGui");

                Game.OnGameUpdate += GameOnOnGameUpdate;
                //SkinChanger.GenAndSendModelPacket(ObjectManager.Player.BaseSkinName, 1);
                //GamePacket gPacketT = Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(100, 100, 0, (Packet.PingType) 6));
                //gPacketT.Send();
                //Device device = Drawing.Direct3DDevice;
                //if (device != null)
                //{
                //    Menu.CdPanel = new Menu.MenuItemSettings(new CdTracker());
                //}
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
            }
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            Type classType = typeof(Menu);
            //BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            //foreach (PropertyInfo p in GetPublicProperties(classType)/*classType.GetProperties(flags)*/)
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
                        item.Item = Activator.CreateInstance(item.Type);
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
