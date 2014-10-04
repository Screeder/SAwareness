using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

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
                else if (Menu.DisconnectDetector.GetMenuItem("SAwarenessDisconnectDetectorChatChoice").GetValue<StringList>().SelectedIndex == 2 &&
                            Menu.GlobalSettings.GetMenuItem("SAwarenessGlobalSettingsServerChatPingActive").GetValue<bool>())
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

    class TurnAround
    {
        private float lastTime = Game.Time;
        private Vector2 lastMove = ObjectManager.Player.ServerPosition.To2D();
        
        public TurnAround()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Game.OnGameSendPacket += Game_OnGameSendPacket;
        }

        ~TurnAround()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Obj_AI_Hero.OnProcessSpellCast -= Obj_AI_Hero_OnProcessSpellCast;
            Game.OnGameSendPacket -= Game_OnGameSendPacket;
        }

        public bool IsActive()
        {
            return Menu.TurnAround.GetActive() && Menu.TurnAround.GetActive();
        }

        void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!IsActive())
                return;
            if(!sender.IsEnemy)
                return;
            if (args.SData.Name.Contains("CassiopeiaPetrifyingGaze"))
            {
                if (ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition) <= 750)
                {
                    Vector2 pos = new Vector2(ObjectManager.Player.ServerPosition.X + ((sender.ServerPosition.X - ObjectManager.Player.ServerPosition.X) * (-100) / ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition)), ObjectManager.Player.ServerPosition.
                    Y + ((sender.ServerPosition.Y - ObjectManager.Player.ServerPosition.Y)*(-100)/ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition)));
                    Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(pos.X, pos.Y)).Send();
                    lastTime = Game.Time;
                    Utility.DelayAction.Add(750, () => Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(lastMove.X, lastMove.Y)).Send());
                }
            } else if (args.SData.Name.Contains("MockingShout"))
            {
                if (ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition) <= 850)
                {
                    Vector2 pos = new Vector2(ObjectManager.Player.ServerPosition.X + ((sender.ServerPosition.X - ObjectManager.Player.ServerPosition.X) * (100) / ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition)), ObjectManager.Player.ServerPosition.
                    Y + ((sender.ServerPosition.Y - ObjectManager.Player.ServerPosition.Y) * (100) / ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition)));
                    Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(pos.X, pos.Y)).Send();
                    lastTime = Game.Time;
                    Utility.DelayAction.Add(750, () => Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(lastMove.X, lastMove.Y)).Send());
                }
            }
        }

        void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;

            try
            {
                decimal milli = DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerMillisecond;
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte PacketId = reader.ReadByte();
                if (PacketId != Packet.C2S.Move.Header)
                    return;
                Packet.C2S.Move.Struct move = Packet.C2S.Move.Decoded(args.PacketData);
                if (move.MoveType == 2)
                {
                    if (move.SourceNetworkId == ObjectManager.Player.NetworkId)
                    {
                        lastMove = new Vector2(move.X, move.Y);
                        if (lastTime + 1 > Game.Time)
                            args.Process = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("MovementSend: " + ex.ToString());
                return;
            }
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;

            //foreach (var gObject in ObjectManager.Get<GameObject>())
            //{
            //    if (lastTime + 2 < Game.Time &&
            //        lastMove.X != 0)
            //    {
            //        Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(lastMove.X, lastMove.Y)).Send();
            //        lastMove.X = 0;
            //    }
            //}
        }
    }

    class AutoJump //DONT PLACE COUR CURSOR ON A WALL IT WILL FAIL
    {
        private Spell jumpSpell;
        private bool onlyAlly = false;
        private bool onlyEnemy = false;
        private bool useWard = true;
        private float lastCast = Game.Time;

        public AutoJump()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Katarina":
                    jumpSpell = new Spell(SpellSlot.E, 790);
                    break;

                case "Jax":
                    jumpSpell = new Spell(SpellSlot.Q, 790);
                    break;

                case "LeeSin":
                    jumpSpell = new Spell(SpellSlot.W, 790);
                    onlyAlly = true;
                    break;

                case "Talon":
                    jumpSpell = new Spell(SpellSlot.E, 790);
                    onlyEnemy = true;
                    useWard = false;
                    break;

                default:
                    return;
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
        }

        ~AutoJump()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Menu.AutoJump.GetActive() && Menu.AutoJump.GetActive();
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || !Menu.AutoJump.GetMenuItem("SAwarenessAutoJumpKey").GetValue<KeyBind>().Active || !jumpSpell.IsReady())
                return;           

            foreach (var gObject in ObjectManager.Get<GameObject>())
            {
                if ((useWard && (gObject.Name.Contains("SightWard") || gObject.Name.Contains("VisionWard"))) || gObject.Type == GameObjectType.obj_AI_Minion)
                {
                    if (!onlyAlly && !onlyEnemy || (onlyAlly && gObject.IsAlly) || (onlyEnemy && gObject.IsEnemy))
                    {
                        if(!gObject.IsValid || ((Obj_AI_Base)gObject).Health < 1)
                            continue;
                        if(Game.CursorPos.Distance(gObject.Position) > 150)
                            continue;
                        if (lastCast + 1 > Game.Time)
                            continue;
                        Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(gObject.Position.X, gObject.Position.Y)).Send();
                        jumpSpell.Cast((Obj_AI_Base)gObject, true);
                        lastCast = Game.Time;
                        return;
                    }
                }
            }
            if (jumpSpell.IsReady() && useWard)
            {
                if (lastCast + 1 > Game.Time)
                    return;
                InventorySlot slot = Wards.GetWardSlot();
                slot.UseItem(Game.CursorPos);
                jumpSpell.Cast(Game.CursorPos, true);
                lastCast = Game.Time;
            }
        }

        void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (!IsActive() || !Menu.AutoJump.GetMenuItem("SAwarenessAutoJumpKey").GetValue<KeyBind>().Active || !jumpSpell.IsReady())
                return; 
            if (sender.Name.Contains("SightWard") || sender.Name.Contains("VisionWard"))
            {
                if (Game.CursorPos.Distance(sender.Position) > 150)
                    return;
                Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(sender.Position.X, sender.Position.Y)).Send();
                jumpSpell.Cast((Obj_AI_Base)sender, true);
                lastCast = Game.Time;
            }
        }
    }
}
