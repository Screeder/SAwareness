using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness
{
    internal class RecallDetector
    {
        public List<RecallInfo> Recalls = new List<RecallInfo>();

        public RecallDetector()
        {
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsEnemy)
                {
                    Recalls.Add(new RecallInfo(enemy.NetworkId));
                }
            }
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
        }

        ~RecallDetector()
        {
            Game.OnGameProcessPacket -= Game_OnGameProcessPacket;
            Recalls = null;
        }

        public bool IsActive()
        {
            return Menu.Detector.GetActive() && Menu.RecallDetector.GetActive();
        }

        private void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte packetId = reader.ReadByte(); //PacketId
                if (packetId != Recall.Header) //OLD 215
                    return;
                //Log.LogPacket(args.PacketData);
                Recall.Struct recall = RecallDecode(args.PacketData);//Packet.S2C.Recall.Decoded(args.PacketData);
                HandleRecall(recall);
            }
            catch (Exception ex)
            {
                Console.WriteLine("RecallProcess: " + ex);
            }
        }

        private void HandleRecall(Recall.Struct recallEx)
        {
            int time = Environment.TickCount - Game.Ping;

            foreach (RecallInfo recall in Recalls)
            {
                if (recall == null) continue;

                if (recallEx.Type == Recall.ObjectType.Player)
                {
                    var obj = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(recall.NetworkId);
                    var objEx = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(recallEx.UnitNetworkId);
                    if (obj == null)
                        continue;
                    if (obj.NetworkId == objEx.NetworkId) //already existing
                    {
                        recall.Recall = recallEx;
                        recall.Recall2 = new Recall.Struct();
                        var t = Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorMode").GetValue<StringList>();
                        if (t.SelectedIndex == 0 || t.SelectedIndex == 2)
                        {
                            var percentHealth = (int) ((obj.Health/obj.MaxHealth)*100);
                            String sColor = "<font color='#FFFFFF'>";
                            String color = (percentHealth > 50
                                ? "<font color='#00FF00'>"
                                : (percentHealth > 30 ? "<font color='#FFFF00'>" : "<font color='#FF0000'>"));
                            if (recallEx.Status == Recall.RecallStatus.TeleportStart ||
                                recallEx.Status == Recall.RecallStatus.RecallStarted)
                            {
                                String text = (recallEx.Status == Recall.RecallStatus.TeleportStart
                                    ? "porting"
                                    : "recalling");
                                recall.StartTime = (int) Game.Time;
                                if (
                                    Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorChatChoice")
                                        .GetValue<StringList>()
                                        .SelectedIndex == 1)
                                {
                                    Game.PrintChat(obj.ChampionName + " {0} with {1} hp {2}({3})", text,
                                        (int) obj.Health, color, percentHealth);
                                }
                                else if (
                                    Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorChatChoice")
                                        .GetValue<StringList>()
                                        .SelectedIndex == 2 &&
                                    Menu.GlobalSettings.GetMenuItem("SAwarenessGlobalSettingsServerChatPingActive")
                                        .GetValue<bool>())
                                {
                                    Game.Say(obj.ChampionName + " {0} with {1} hp {2}({3})", text, (int) obj.Health,
                                        color, percentHealth);
                                }
                            }
                            else if (recallEx.Status == Recall.RecallStatus.TeleportEnd ||
                                     recallEx.Status == Recall.RecallStatus.RecallFinished)
                            {
                                String text = (recallEx.Status == Recall.RecallStatus.TeleportStart
                                    ? "ported"
                                    : "recalled");
                                if (
                                    Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorChatChoice")
                                        .GetValue<StringList>()
                                        .SelectedIndex == 1)
                                {
                                    Game.PrintChat(obj.ChampionName + " {0} with {1} hp {2}({3})", text,
                                        (int) obj.Health, color, percentHealth);
                                }
                                else if (
                                    Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorChatChoice")
                                        .GetValue<StringList>()
                                        .SelectedIndex == 2 &&
                                    Menu.GlobalSettings.GetMenuItem(
                                        "SAwarenessGlobalSettingsServerChatPingActive").GetValue<bool>())
                                {
                                    Game.Say(obj.ChampionName + " {0} with {1} hp {2}({3})", text,
                                        (int) obj.Health, color, percentHealth);
                                }
                            }
                            else
                            {
                                if (
                                    Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorChatChoice")
                                        .GetValue<StringList>()
                                        .SelectedIndex == 1)
                                {
                                    Game.PrintChat(obj.ChampionName + " canceled with {0} hp", (int) obj.Health);
                                }
                                else if (
                                    Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorChatChoice")
                                        .GetValue<StringList>()
                                        .SelectedIndex == 2 &&
                                    Menu.GlobalSettings.GetMenuItem(
                                        "SAwarenessGlobalSettingsServerChatPingActive").GetValue<bool>())
                                {
                                    Game.Say(obj.ChampionName + " canceled with {0} hp", (int) obj.Health);
                                }
                            }
                        }
                        return;
                    }
                }
                else if (recallEx.Status == Recall.RecallStatus.TeleportStart ||
                         recallEx.Status == Recall.RecallStatus.TeleportEnd)
                {
                    if (recall.Recall.Status == Recall.RecallStatus.TeleportStart)
                        recall.Recall2 = recallEx;

                    var obj = ObjectManager.GetUnitByNetworkId<GameObject>(recallEx.UnitNetworkId);
                    Vector3 pos = obj.Position;
                    for (int i = 0;
                        i <
                        Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorPingTimes")
                            .GetValue<Slider>()
                            .Value;
                        i++)
                    {
                        GamePacket gPacketT;
                        if (Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorLocalPing").GetValue<bool>())
                        {
                            gPacketT =
                                Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(pos[0], pos[1], 0, 0,
                                    Packet.PingType.Danger));
                            gPacketT.Process();
                        }
                        else
                        {
                            gPacketT =
                                Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(pos.X, pos.Y, 0,
                                    Packet.PingType.Danger));
                            //gPacketT.Send();
                        }
                    }
                }
            }
        }

        //By Lexxes
        public static Dictionary<int, int> RecallT = new Dictionary<int, int>();

        public static Recall.Struct RecallDecode(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            var recall = new Recall.Struct();

            reader.ReadByte(); //PacketId
            reader.ReadInt32();
            recall.UnitNetworkId = reader.ReadInt32();
            reader.ReadBytes(66);

            recall.Status = Recall.RecallStatus.Unknown;

            var teleport = false;

            if (BitConverter.ToString(reader.ReadBytes(6)) != "00-00-00-00-00-00")
            {
                if (BitConverter.ToString(reader.ReadBytes(3)) != "00-00-00")
                {
                    recall.Status = Recall.RecallStatus.TeleportStart;
                    teleport = true;
                }
                else
                    recall.Status = Recall.RecallStatus.RecallStarted;
            }

            reader.Close();

            var champ = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(recall.UnitNetworkId);

            if (champ == null)
                return recall;
            if (teleport)
                recall.Duration = 3500;
            else
            //use masteries to detect recall duration, because spelldata is not initialized yet when enemy has not been seen
            {
                recall.Duration = Utility.Map.GetMap()._MapType == Utility.Map.MapType.CrystalScar ? 4500 : 8000;

                if (champ.Masteries.Any(x => x.Page == MasteryPage.Utility && x.Id == 65 && x.Points == 1))
                    recall.Duration -= Utility.Map.GetMap()._MapType == Utility.Map.MapType.CrystalScar ? 500 : 1000;
                //phasewalker mastery
            }

            var time = Environment.TickCount - Game.Ping;

            if (!RecallT.ContainsKey(recall.UnitNetworkId))
                RecallT.Add(recall.UnitNetworkId, time);
            //will result in status RecallStarted, which would be wrong if the assembly was to be loaded while somebody recalls
            else
            {
                if (RecallT[recall.UnitNetworkId] == 0)
                    RecallT[recall.UnitNetworkId] = time;
                else
                {
                    if (time - RecallT[recall.UnitNetworkId] > recall.Duration - 175)
                        recall.Status = teleport
                            ? Recall.RecallStatus.TeleportEnd
                            : Recall.RecallStatus.RecallFinished;
                    else
                        recall.Status = teleport
                            ? Recall.RecallStatus.TeleportAbort
                            : Recall.RecallStatus.RecallAborted;

                    RecallT[recall.UnitNetworkId] = 0; //recall aborted or finished, reset status
                }
            }

            return recall;
        }

        public class RecallInfo
        {
            public int NetworkId;
            public Recall.Struct Recall;
            public Recall.Struct Recall2;
            public int StartTime;

            public RecallInfo(int networkId)
            {
                NetworkId = networkId;
            }
        }
    }

    public static class Recall
    {
        public enum ObjectType
        {
            Player,
            Turret,
            Minion,
            Ward,
            Object
        }

        public enum RecallStatus
        {
            RecallStarted,
            RecallAborted,
            RecallFinished,
            Unknown,
            TeleportStart,
            TeleportAbort,
            TeleportEnd,
        }

        private const int ErrorGap = 100; //in ticks

        public static byte Header = 0xD8;

        private static readonly IDictionary<string, Type> TypeByString = new Dictionary<string, Type>
                {
                    { "Recall", Type.Recall },
                    { "Teleport", Type.Teleport }
                };

        private static readonly Dictionary<int, RecallData> RecallDataByNetworkId =
            new Dictionary<int, RecallData>();

        public static GamePacket Encoded(Struct packetStruct)
        {
            //TODO when the packet is fully decoded.
            return new GamePacket(Header);
        }

        public static Struct Decoded(byte[] data)
        {
            var packet = new GamePacket(data);
            var result = new Struct();

            result.UnitNetworkId = packet.ReadInteger(5);
            result.Status = RecallStatus.Unknown;

            var typeAsString = packet.ReadString(75);
            var gObject = ObjectManager.GetUnitByNetworkId<GameObject>(result.UnitNetworkId);

            if (gObject == null || !gObject.IsValid)
            {
                return result;
            }

            if (gObject is Obj_AI_Hero)
            {
                var unit = (Obj_AI_Hero)gObject;

                if (!unit.IsValid || unit.Spellbook.GetSpell(SpellSlot.Recall) == null)
                {
                    return result;
                }

                result.Type = ObjectType.Player;

                if (!RecallDataByNetworkId.ContainsKey(result.UnitNetworkId))
                {
                    RecallDataByNetworkId[result.UnitNetworkId] = new RecallData { Type = Type.Unknown };
                }

                if (string.IsNullOrEmpty(typeAsString))
                {
                    switch (RecallDataByNetworkId[result.UnitNetworkId].Type)
                    {
                        case Type.Recall:
                            if (Environment.TickCount - RecallDataByNetworkId[result.UnitNetworkId].Start <
                                RecallDataByNetworkId[result.UnitNetworkId].Duration - ErrorGap)
                            {
                                result.Status = RecallStatus.RecallAborted;
                            }
                            else
                            {
                                result.Status = RecallStatus.RecallFinished;
                            }
                            break;
                        case Type.Teleport:
                            if (Environment.TickCount - RecallDataByNetworkId[result.UnitNetworkId].Start <
                                RecallDataByNetworkId[result.UnitNetworkId].Duration - ErrorGap)
                            {
                                result.Status = RecallStatus.TeleportAbort;
                            }
                            else
                            {
                                result.Status = RecallStatus.TeleportEnd;
                            }
                            break;
                    }
                }
                else
                {
                    if (TypeByString.ContainsKey(typeAsString))
                    {
                        switch (TypeByString[typeAsString])
                        {
                            case Type.Recall:
                                result.Status = RecallStatus.RecallStarted;
                                result.Duration = Utility.GetRecallTime(packet.ReadString(139));
                                RecallDataByNetworkId[result.UnitNetworkId] = new RecallData
                                {
                                    Type = Type.Recall,
                                    Duration = result.Duration,
                                    Start = Environment.TickCount
                                };
                                break;
                            case Type.Teleport:
                                result.Status = RecallStatus.TeleportStart;
                                result.Duration = 3500;
                                RecallDataByNetworkId[result.UnitNetworkId] = new RecallData
                                {
                                    Type = Type.Teleport,
                                    Duration = result.Duration,
                                    Start = Environment.TickCount
                                };
                                break;
                        }
                    }
                }
            }
            else if (gObject is Obj_AI_Turret)
            {
                result.Type = ObjectType.Turret;
                result.Status = string.IsNullOrEmpty(typeAsString)
                    ? RecallStatus.TeleportEnd
                    : RecallStatus.TeleportStart;
            }
            else if (gObject is Obj_AI_Minion)
            {
                result.Type = ObjectType.Object;

                if (gObject.Name.Contains("Minion"))
                {
                    result.Type = ObjectType.Minion;
                }
                if (gObject.Name.Contains("Ward"))
                {
                    result.Type = ObjectType.Ward;
                }

                result.Status = string.IsNullOrEmpty(typeAsString)
                    ? RecallStatus.TeleportEnd
                    : RecallStatus.TeleportStart;
            }
            else
            {
                result.Type = ObjectType.Object;
            }

            return result;
        }

        internal struct RecallData
        {
            public Type Type { get; set; }
            public int Start { get; set; }
            public int Duration { get; set; }
        }

        public struct Struct
        {
            public int Duration;
            public RecallStatus Status;
            public ObjectType Type;
            public int UnitNetworkId;

            public Struct(int unitNetworkId, RecallStatus status, ObjectType type, int duration)
            {
                UnitNetworkId = unitNetworkId;
                Status = status;
                Type = type;
                Duration = duration;
            }
        }

        internal enum Type
        {
            Recall,
            Teleport,
            Unknown
        }
    }
}
