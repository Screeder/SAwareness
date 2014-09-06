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
    class SurrenderVote
    {
        private int lastNoVoteCount = 0;

        public SurrenderVote()
        {
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
        }

        ~SurrenderVote()
        {
            Game.OnGameProcessPacket -= Game_OnGameProcessPacket;
        }

        public bool IsActive()
        {
            return Menu.SurrenderVote.GetActive();
        }

        void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;

            try
            {
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte PacketId = reader.ReadByte(); //PacketId
                if (PacketId != 201)
                    return;
                GamePacket gamePacket = new GamePacket(args.PacketData);
                gamePacket.Position = 6;
                var networkId = gamePacket.ReadInteger();
                gamePacket.Position = 11;
                var noVote = gamePacket.ReadByte();
                var allVote = gamePacket.ReadByte();
                var team = gamePacket.ReadByte();

                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (hero.NetworkId == networkId)
                    {
                        if (noVote > lastNoVoteCount)
                        {
                            if (Menu.SurrenderVote.GetMenuItem("SAwarenessSurrenderVoteChatChoice").GetValue<StringList>().SelectedIndex == 1)
                            {
                                Game.PrintChat("{0} voted NO", hero.ChampionName);
                            }
                            else if (Menu.SurrenderVote.GetMenuItem("SAwarenessSurrenderVoteChatChoice").GetValue<StringList>().SelectedIndex == 2)
                            {
                                Game.Say("{0} voted NO", hero.ChampionName);
                            }
                        }
                        else
                        {
                            if (Menu.SurrenderVote.GetMenuItem("SAwarenessSurrenderVoteChatChoice").GetValue<StringList>().SelectedIndex == 1)
                            {
                                Game.PrintChat("{0} voted YES", hero.ChampionName);
                            }
                            else if (Menu.SurrenderVote.GetMenuItem("SAwarenessSurrenderVoteChatChoice").GetValue<StringList>().SelectedIndex == 2)
                            {
                                Game.Say("{0} voted YES", hero.ChampionName);
                            }
                        }
                        break;
                    }
                }
                lastNoVoteCount = noVote;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SurrenderProcess: " + ex.ToString());
                return;
            }
        }
    }
}
