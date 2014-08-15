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
}
