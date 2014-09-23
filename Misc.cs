using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness
{
    class Misc
    {
    }

    class DisconnectDetector
    {

        Dictionary<Obj_AI_Hero, bool> _disconnects = new Dictionary<Obj_AI_Hero, bool>();  

        public DisconnectDetector()
        {
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
        }

        ~DisconnectDetector()
        {
            Game.OnGameProcessPacket -= Game_OnGameProcessPacket;
        }

        public bool IsActive()
        {
            return Menu.Misc.GetActive() && Menu.DisconnectDetector.GetActive();
        }

        void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte PacketId = reader.ReadByte(); //PacketId
                if (PacketId != Packet.S2C.PlayerDisconnect.Header)
                    return;
                Packet.S2C.PlayerDisconnect.Struct disconnect = Packet.S2C.PlayerDisconnect.Decoded(args.PacketData);
                if (disconnect.Player == null)
                    return;
                if (_disconnects.ContainsKey(disconnect.Player))
                {
                    _disconnects[disconnect.Player] = true;
                }
                else
                {
                    _disconnects.Add(disconnect.Player, true);
                }
                if (Menu.DisconnectDetector.GetMenuItem("SAwarenessDisconnectDetectorChatChoice").GetValue<StringList>().SelectedIndex == 1)
                {
                    Game.PrintChat("Champion " + disconnect.Player.ChampionName + " has disconnected!");
                }
                else if (Menu.DisconnectDetector.GetMenuItem("SAwarenessDisconnectDetectorChatChoice").GetValue<StringList>().SelectedIndex == 2)
                {
                    Game.Say("Champion " + disconnect.Player.ChampionName + " has disconnected!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DisconnectProcess: " + ex.ToString());
                return;
            }

        }

    }

    class AutoLatern
    {
        public AutoLatern()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoLatern()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Menu.AutoLatern.GetActive() && Menu.AutoLatern.GetActive();
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || !Menu.AutoLatern.GetMenuItem("SAwarenessAutoLaternKey").GetValue<KeyBind>().Active)
                return;

            foreach (var gObject in ObjectManager.Get<GameObject>())
            {
                if (gObject.Name.Contains("ThreshLantern") && gObject.IsAlly && gObject.Position.Distance(ObjectManager.Player.ServerPosition) < 400 && !ObjectManager.Player.ChampionName.Contains("Thresh"))
                {
                    GamePacket gPacket = Packet.C2S.InteractObject.Encoded(new Packet.C2S.InteractObject.Struct(ObjectManager.Player.NetworkId,
                        gObject.NetworkId));
                    gPacket.Send();
                }
            }
        }
    }
}
