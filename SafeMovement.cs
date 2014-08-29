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
    class SafeMovement
    {
        private decimal lastSend = 0;

        public SafeMovement()
        {
            Game.OnGameSendPacket += Game_OnGameSendPacket;
        }

        ~SafeMovement()
        {
            Game.OnGameSendPacket -= Game_OnGameSendPacket;
        }

        public bool IsActive()
        {
            return Menu.SafeMovement.GetActive();
        }

        void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;

            try
            {
                decimal milli = DateTime.Now.Ticks/(decimal) TimeSpan.TicksPerMillisecond;
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte PacketId = reader.ReadByte();
                if (PacketId != Packet.C2S.Move.Header)
                    return;
                Packet.C2S.Move.Struct move = Packet.C2S.Move.Decoded(args.PacketData);
                if (move.MoveType == 2)
                {
                    if (move.SourceNetworkId == ObjectManager.Player.NetworkId)
                    {
                        if (milli - lastSend < Menu.SafeMovement.GetMenuItem("SAwarenessSafeMovementBlockIntervall").GetValue<Slider>().Value)
                        {
                            args.Process = false;
                        }
                        else
                        {
                            lastSend = milli;
                        }
                    }
                    
                }
                else if (move.MoveType == 3)
                {
                    lastSend = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("MovementProcess: " + ex.ToString());
                return;
            }
        }
    }
}
