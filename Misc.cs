using System;
using System.Collections.Generic;
using System.IO;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness
{
    internal class Misc
    {
    }

    internal class DisconnectDetector
    {
        private readonly Dictionary<Obj_AI_Hero, bool> _disconnects = new Dictionary<Obj_AI_Hero, bool>();

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

        private void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte packetId = reader.ReadByte(); //PacketId
                if (packetId != Packet.S2C.PlayerDisconnect.Header)
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
                if (
                    Menu.DisconnectDetector.GetMenuItem("SAwarenessDisconnectDetectorChatChoice")
                        .GetValue<StringList>()
                        .SelectedIndex == 1)
                {
                    Game.PrintChat("Champion " + disconnect.Player.ChampionName + " has disconnected!");
                }
                else if (
                    Menu.DisconnectDetector.GetMenuItem("SAwarenessDisconnectDetectorChatChoice")
                        .GetValue<StringList>()
                        .SelectedIndex == 2 &&
                    Menu.GlobalSettings.GetMenuItem("SAwarenessGlobalSettingsServerChatPingActive").GetValue<bool>())
                {
                    Game.Say("Champion " + disconnect.Player.ChampionName + " has disconnected!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DisconnectProcess: " + ex);
            }
        }
    }

    internal class AutoLatern
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
            return Menu.Misc.GetActive() && Menu.AutoLatern.GetActive();
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || !Menu.AutoLatern.GetMenuItem("SAwarenessAutoLaternKey").GetValue<KeyBind>().Active)
                return;

            foreach (GameObject gObject in ObjectManager.Get<GameObject>())
            {
                if (gObject.Name.Contains("ThreshLantern") && gObject.IsAlly &&
                    gObject.Position.Distance(ObjectManager.Player.ServerPosition) < 400 &&
                    !ObjectManager.Player.ChampionName.Contains("Thresh"))
                {
                    GamePacket gPacket =
                        Packet.C2S.InteractObject.Encoded(
                            new Packet.C2S.InteractObject.Struct(ObjectManager.Player.NetworkId,
                                gObject.NetworkId));
                    gPacket.Send();
                }
            }
        }
    }

    internal class TurnAround
    {
        private Vector2 _lastMove = ObjectManager.Player.ServerPosition.To2D();
        private float _lastTime = Game.Time;

        public TurnAround()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Game.OnGameSendPacket += Game_OnGameSendPacket;
        }

        ~TurnAround()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast -= Obj_AI_Hero_OnProcessSpellCast;
            Game.OnGameSendPacket -= Game_OnGameSendPacket;
        }

        public bool IsActive()
        {
            return Menu.Misc.GetActive() && Menu.TurnAround.GetActive();
        }

        private void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!IsActive())
                return;
            if (!sender.IsEnemy)
                return;
            if (args.SData.Name.Contains("CassiopeiaPetrifyingGaze"))
            {
                if (ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition) <= 750)
                {
                    var pos =
                        new Vector2(
                            ObjectManager.Player.ServerPosition.X +
                            ((sender.ServerPosition.X - ObjectManager.Player.ServerPosition.X)*(-100)/
                             ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition)),
                            ObjectManager.Player.ServerPosition.
                                Y +
                            ((sender.ServerPosition.Y - ObjectManager.Player.ServerPosition.Y)*(-100)/
                             ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition)));
                    Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(pos.X, pos.Y)).Send();
                    _lastTime = Game.Time;
                    Utility.DelayAction.Add(750,
                        () => Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(_lastMove.X, _lastMove.Y)).Send());
                }
            }
            else if (args.SData.Name.Contains("MockingShout"))
            {
                if (ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition) <= 850)
                {
                    var pos =
                        new Vector2(
                            ObjectManager.Player.ServerPosition.X +
                            ((sender.ServerPosition.X - ObjectManager.Player.ServerPosition.X)*(100)/
                             ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition)),
                            ObjectManager.Player.ServerPosition.
                                Y +
                            ((sender.ServerPosition.Y - ObjectManager.Player.ServerPosition.Y)*(100)/
                             ObjectManager.Player.ServerPosition.Distance(sender.ServerPosition)));
                    Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(pos.X, pos.Y)).Send();
                    _lastTime = Game.Time;
                    Utility.DelayAction.Add(750,
                        () => Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(_lastMove.X, _lastMove.Y)).Send());
                }
            }
        }

        private void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;

            try
            {
                decimal milli = DateTime.Now.Ticks/(decimal) TimeSpan.TicksPerMillisecond;
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte packetId = reader.ReadByte();
                if (packetId != Packet.C2S.Move.Header)
                    return;
                Packet.C2S.Move.Struct move = Packet.C2S.Move.Decoded(args.PacketData);
                if (move.MoveType == 2)
                {
                    if (move.SourceNetworkId == ObjectManager.Player.NetworkId)
                    {
                        _lastMove = new Vector2(move.X, move.Y);
                        if (_lastTime + 1 > Game.Time)
                            args.Process = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("MovementSend: " + ex);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
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

    internal class AutoJump //DONT PLACE COUR CURSOR ON A WALL IT WILL FAIL
    {
        private readonly Spell _jumpSpell;
        private readonly bool _onlyAlly;
        private readonly bool _onlyEnemy;
        private readonly bool _useWard = true;
        private float _lastCast = Game.Time;

        public AutoJump()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Katarina":
                    _jumpSpell = new Spell(SpellSlot.E, 790);
                    break;

                case "Jax":
                    _jumpSpell = new Spell(SpellSlot.Q, 790);
                    break;

                case "LeeSin":
                    _jumpSpell = new Spell(SpellSlot.W, 790);
                    _onlyAlly = true;
                    break;

                case "Talon":
                    _jumpSpell = new Spell(SpellSlot.E, 790);
                    _onlyEnemy = true;
                    _useWard = false;
                    break;

                default:
                    return;
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
        }

        ~AutoJump()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Menu.Misc.GetActive() && Menu.AutoJump.GetActive();
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || !Menu.AutoJump.GetMenuItem("SAwarenessAutoJumpKey").GetValue<KeyBind>().Active ||
                !_jumpSpell.IsReady())
                return;

            foreach (GameObject gObject in ObjectManager.Get<GameObject>())
            {
                if ((_useWard && (gObject.Name.Contains("SightWard") || gObject.Name.Contains("VisionWard"))) ||
                    gObject.Type == GameObjectType.obj_AI_Minion)
                {
                    if (!_onlyAlly && !_onlyEnemy || (_onlyAlly && gObject.IsAlly) || (_onlyEnemy && gObject.IsEnemy))
                    {
                        if (!gObject.IsValid || ((Obj_AI_Base) gObject).Health < 1)
                            continue;
                        if (Game.CursorPos.Distance(gObject.Position) > 150)
                            continue;
                        if (_lastCast + 1 > Game.Time)
                            continue;
                        Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(gObject.Position.X, gObject.Position.Y))
                            .Send();
                        _jumpSpell.Cast((Obj_AI_Base) gObject, true);
                        _lastCast = Game.Time;
                        return;
                    }
                }
            }
            if (_jumpSpell.IsReady() && _useWard)
            {
                if (_lastCast + 1 > Game.Time)
                    return;
                InventorySlot slot = Wards.GetWardSlot();
                slot.UseItem(Game.CursorPos);
                _jumpSpell.Cast(Game.CursorPos, true);
                _lastCast = Game.Time;
            }
        }

        private void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (!IsActive() || !Menu.AutoJump.GetMenuItem("SAwarenessAutoJumpKey").GetValue<KeyBind>().Active ||
                !_jumpSpell.IsReady())
                return;
            if (sender.Name.Contains("SightWard") || sender.Name.Contains("VisionWard"))
            {
                if (Game.CursorPos.Distance(sender.Position) > 150)
                    return;
                Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(sender.Position.X, sender.Position.Y)).Send();
                _jumpSpell.Cast((Obj_AI_Base) sender, true);
                _lastCast = Game.Time;
            }
        }
    }

    internal class MinionBars
    {
        public MinionBars()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~MinionBars()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Menu.Misc.GetActive() && Menu.MinionBars.GetActive();
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;

            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
            {
                if(!minion.IsVisible || minion.IsDead || minion.IsAlly)
                    continue;
                Vector2 hpPos = minion.HPBarPosition;
                //hpPos.Y -= 3;
                double damageMinion = ObjectManager.Player.GetAutoAttackDamage(minion);
                double hitsToKill = Math.Ceiling(minion.MaxHealth/damageMinion);
                double barsToDraw = Math.Floor(minion.MaxHealth / 100.0);
                double barDistance = 100.0 / (minion.MaxHealth / 62.0);
                double myDamageDistance = damageMinion / (minion.MaxHealth / 62.0);
	            double barsDrawn = 0;
	            int heightOffset = 1;
	            int barSize = 2;
                int barWidth = 1;
                //hpPos.X = hpPos.X - 32;
                hpPos.Y = hpPos.Y + heightOffset;
                if (minion.BaseSkinName == "Dragon" || minion.BaseSkinName == "Worm" ||
                    minion.BaseSkinName == "TT_Spiderboss")
                {
                    double healthDraw = 500.0;
                    if (minion.BaseSkinName == "Dragon")
                    {
                        hpPos.X -= 31;
                        hpPos.Y -= 7;
                    }
                    else if (minion.BaseSkinName == "Worm")
                    {
                        hpPos.X -= 31;
                        healthDraw = 1000.0;
                    }
                    else if (minion.BaseSkinName == "TT_Spiderboss")
                        hpPos.X -= 3;
                    barsToDraw = Math.Floor(minion.MaxHealth/healthDraw);
                    barDistance = healthDraw/(minion.MaxHealth/124.0);
                    double drawDistance = 0;
                    while (barsDrawn != barsToDraw && barsToDraw != 0 && barsToDraw < 200)
                    {
                        drawDistance = drawDistance + barDistance;
                        if (barsDrawn%2 == 1)
                        {
                            DrawRectangleAL(hpPos.X + 43 + drawDistance, hpPos.Y + 19, barWidth + 1, barSize,
                                System.Drawing.Color.Black);
                        }
                        else
                            DrawRectangleAL(hpPos.X + 43 + drawDistance, hpPos.Y + 19, barWidth, barSize,
                                System.Drawing.Color.Black);
                        barsDrawn = barsDrawn + 1;
                    }
                    DrawRectangleAL(hpPos.X + 43 + myDamageDistance, hpPos.Y + 19, barWidth, barSize, System.Drawing.Color.GreenYellow);
                    if (damageMinion > minion.Health)
                    {
                        OutLineBar(hpPos.X + 43, hpPos.Y + 20, System.Drawing.Color.GreenYellow);
                    }
                }
                else
                {
                    double drawDistance = 0;
                    while (barsDrawn != barsToDraw && barsToDraw != 0 && barsToDraw < 50)
                    {
                        drawDistance = drawDistance + barDistance;
                        if (barsToDraw > 20)
                        {
                            if (barsDrawn % 5 == 4)
                                if (barsDrawn % 10 == 9)
                                    DrawRectangleAL(hpPos.X + 43 + drawDistance, hpPos.Y + 19, barWidth + 1, barSize,
                                        System.Drawing.Color.Black);
                                else
                                    DrawRectangleAL(hpPos.X + 43 + drawDistance, hpPos.Y + 19, barWidth, barSize,
                                        System.Drawing.Color.Black);
                        }
                        else
                            DrawRectangleAL(hpPos.X + 43 + drawDistance, hpPos.Y + 19, barWidth, barSize,
                                System.Drawing.Color.Black);
                        barsDrawn = barsDrawn + 1;

                    }
                    DrawRectangleAL(hpPos.X + 43 + myDamageDistance, hpPos.Y + 19, barWidth, barSize, System.Drawing.Color.GreenYellow);
                    if (damageMinion > minion.Health && Menu.MinionBars.GetMenuItem("SAwarenessMinionBarsGlowActive").GetValue<bool>())
                    {
                        OutLineBar(hpPos.X + 43, hpPos.Y + 20, System.Drawing.Color.GreenYellow);
                    }			
                }
            }
        }

        private void DrawRectangleAL(double x, double y, double w, double h, System.Drawing.Color color)
        {
            Vector2[] points = new Vector2[4];
            points[0] = new Vector2((float)Math.Floor(x), (float)Math.Floor(y));
            points[1] = new Vector2((float) Math.Floor(x + w), (float) Math.Floor(y));
            points[2] = new Vector2((float)Math.Floor(x), (float)Math.Floor(y + h));
            points[3] = new Vector2((float)Math.Floor(x + w), (float)Math.Floor(y + h));
            if (Common.IsOnScreen(points[0]) && Common.IsOnScreen(points[1]))
                Drawing.DrawLine(points[0], points[1], 1, color);
            if (Common.IsOnScreen(points[0]) && Common.IsOnScreen(points[2]))
                Drawing.DrawLine(points[0], points[2], 1, color);
            if (Common.IsOnScreen(points[1]) && Common.IsOnScreen(points[3]))
                Drawing.DrawLine(points[1], points[3], 1, color);
            if (Common.IsOnScreen(points[2]) && Common.IsOnScreen(points[3]))
                Drawing.DrawLine(points[2], points[3], 1, color);
        }

        private void OutLineBar(double x, double y, System.Drawing.Color color)
        {
            DrawRectangleAL(x, y - 3, 64, 1, color);
	        DrawRectangleAL(x, y + 2, 64, 1, color);
	
	        DrawRectangleAL(x, y, 1, 5, color);
	        DrawRectangleAL(x + 63, y, 1, 5, color);
        }
    }
}