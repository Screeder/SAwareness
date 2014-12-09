using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SAwareness.Spectator;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Font = SharpDX.Direct3D9.Font;
using Packet = LeagueSharp.Common.Packet;

//Tracker: originale Texture muss immer das richtige pic sein, sonst unschärfe. Fragezeichen kommt weg

namespace SAwareness
{
    internal class CloneTracker
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
            return Menu.Tracker.GetActive() && Menu.CloneTracker.GetActive();
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy && !hero.IsDead && hero.IsVisible)
                {
                    if (hero.ChampionName.Contains("Shaco") ||
                        hero.ChampionName.Contains("Leblanc") ||
                        hero.ChampionName.Contains("MonkeyKing") ||
                        hero.ChampionName.Contains("Yorick"))
                    {
                        Utility.DrawCircle(hero.ServerPosition, 100, Color.Red);
                        Utility.DrawCircle(hero.ServerPosition, 110, Color.Red);
                    }
                    
                }
            }
        }
    }

    internal class HiddenObject
    {
        public enum ObjectType
        {
            Vision,
            Sight,
            Trap,
            Unknown
        }

        private const int WardRange = 1200;
        private const int TrapRange = 300;
        public List<ObjectData> HidObjects = new List<ObjectData>();
        public List<Object> Objects = new List<Object>();

        public HiddenObject()
        {
            Objects.Add(new Object(ObjectType.Vision, "Vision Ward", "VisionWard", "VisionWard", float.MaxValue, 8,
                6424612, Color.BlueViolet));
            Objects.Add(new Object(ObjectType.Sight, "Stealth Ward", "SightWard", "SightWard", 180.0f, 161, 234594676,
                Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Warding Totem (Trinket)", "YellowTrinket", "TrinketTotemLvl1", 60.0f,
                56, 263796881, Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Warding Totem (Trinket)", "YellowTrinketUpgrade", "TrinketTotemLvl2", 120.0f,
                56, 263796882, Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Greater Stealth Totem (Trinket)", "SightWard", "TrinketTotemLvl3",
                180.0f, 56, 263796882, Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Greater Vision Totem (Trinket)", "VisionWard", "TrinketTotemLvl3B",
                9999.9f, 137, 194218338, Color.BlueViolet));
            Objects.Add(new Object(ObjectType.Sight, "Wriggle's Lantern", "SightWard", "wrigglelantern", 180.0f, 73,
                177752558, Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Quill Coat", "SightWard", "", 180.0f, 73, 135609454, Color.Green));
            Objects.Add(new Object(ObjectType.Sight, "Ghost Ward", "SightWard", "ItemGhostWard", 180.0f, 229, 101180708,
                Color.Green));

            Objects.Add(new Object(ObjectType.Trap, "Yordle Snap Trap", "Cupcake Trap", "CaitlynYordleTrap", 240.0f, 62,
                176176816, Color.Red));
            Objects.Add(new Object(ObjectType.Trap, "Jack In The Box", "Jack In The Box", "JackInTheBox", 60.0f, 2,
                44637032, Color.Red));
            Objects.Add(new Object(ObjectType.Trap, "Bushwhack", "Noxious Trap", "Bushwhack", 240.0f, 9, 167611995,
                Color.Red));
            Objects.Add(new Object(ObjectType.Trap, "Noxious Trap", "Noxious Trap", "BantamTrap", 600.0f, 48, 176304336,
                Color.Red));

            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            GameObject.OnDelete += Obj_AI_Base_OnDelete;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;
            Game.OnGameUpdate += Game_OnGameUpdate;
            foreach (var obj in ObjectManager.Get<GameObject>())
            {
                GameObject_OnCreate(obj, new EventArgs());
            }
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            List<ObjectData> objects = HidObjects.FindAll(x => x.ObjectBase.Name == "Unknown");
            foreach (var obj1 in HidObjects.ToArray())
            {
                if(obj1.ObjectBase.Name.Contains("Unknown"))
                    continue;
                foreach (var obj2 in objects)
                {
                    if (Geometry.ProjectOn(obj1.EndPosition.To2D(), obj2.StartPosition.To2D(), obj2.EndPosition.To2D()).IsOnSegment)
                    {
                        HidObjects.Remove(obj2);
                    }
                }
            }
            
        }

        ~HiddenObject()
        {
            Game.OnGameProcessPacket -= Game_OnGameProcessPacket;
            Obj_AI_Base.OnProcessSpellCast -= Obj_AI_Base_OnProcessSpellCast;
            GameObject.OnCreate -= GameObject_OnCreate;
            GameObject.OnDelete -= Obj_AI_Base_OnDelete;
            Drawing.OnDraw -= Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            
            HidObjects = null;
            Objects = null;
        }

        public bool IsActive()
        {
            return Menu.Tracker.GetActive() && Menu.VisionDetector.GetActive();
        }

        private void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                if(!sender.IsValid)
                    return;
                if (sender is Obj_AI_Base && ObjectManager.Player.Team != sender.Team)
                {   
                    foreach (Object obj in Objects)
                    {
                        if (((Obj_AI_Base)sender).BaseSkinName == obj.ObjectName && !ObjectExist(sender.Position))
                        {
                            HidObjects.Add(new ObjectData(obj, sender.Position, Game.Time + ((Obj_AI_Base)sender).Mana, sender.Name,
                                null, sender.NetworkId));
                            break;
                        }
                    }
                }
                
                if (sender is Obj_SpellLineMissile && ObjectManager.Player.Team != ((Obj_SpellMissile)sender).SpellCaster.Team)
                {
                    if (((Obj_SpellMissile)sender).SData.Name.Contains("itemplacementmissile"))
                    {
                        Utility.DelayAction.Add(10, () =>
                        {
                            if (!ObjectExist(((Obj_SpellMissile)sender).EndPosition))
                            {

                                HidObjects.Add(new ObjectData(new Object(ObjectType.Unknown, "Unknown", "Unknown", "Unknown", 180.0f, 0, 0, Color.Yellow), ((Obj_SpellMissile)sender).EndPosition, Game.Time + 180.0f, sender.Name, null,
                                    sender.NetworkId, ((Obj_SpellMissile)sender).StartPosition));
                            }
                        });
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HiddenObjectCreate: " + ex);
            }
        }

        private void Drawing_OnDraw(EventArgs args)
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
                    Vector2 objMPos = Drawing.WorldToMinimap(obj.EndPosition);
                    Vector2 objPos = Drawing.WorldToScreen(obj.EndPosition);
                    var posList = new List<Vector3>();
                    switch (obj.ObjectBase.Type)
                    {
                        case ObjectType.Sight:
                            if (Menu.VisionDetector.GetMenuItem("SAwarenessVisionDetectorDrawRange").GetValue<bool>())
                            {
                                Utility.DrawCircle(obj.EndPosition, WardRange, obj.ObjectBase.Color);
                            }
                            posList = GetVision(obj.EndPosition, WardRange);
                            for (int j = 0; j < posList.Count; j++)
                            {
                                Vector2 visionPos1 = Drawing.WorldToScreen(posList[i]);
                                Vector2 visionPos2 = Drawing.WorldToScreen(posList[i]);
                                Drawing.DrawLine(visionPos1[0], visionPos1[1], visionPos2[0], visionPos2[1], 1.0f,
                                    obj.ObjectBase.Color);
                            }
                            Drawing.DrawText(objMPos[0], objMPos[1], obj.ObjectBase.Color, "S");
                            break;

                        case ObjectType.Trap:
                            if (Menu.VisionDetector.GetMenuItem("SAwarenessVisionDetectorDrawRange").GetValue<bool>())
                            {
                                Utility.DrawCircle(obj.EndPosition, TrapRange, obj.ObjectBase.Color);
                            }
                            posList = GetVision(obj.EndPosition, TrapRange);
                            for (int j = 0; j < posList.Count; j++)
                            {
                                Vector2 visionPos1 = Drawing.WorldToScreen(posList[i]);
                                Vector2 visionPos2 = Drawing.WorldToScreen(posList[i]);
                                Drawing.DrawLine(visionPos1[0], visionPos1[1], visionPos2[0], visionPos2[1], 1.0f,
                                    obj.ObjectBase.Color);
                            }
                            Drawing.DrawText(objMPos[0], objMPos[1], obj.ObjectBase.Color, "T");
                            break;

                        case ObjectType.Vision:
                            if (Menu.VisionDetector.GetMenuItem("SAwarenessVisionDetectorDrawRange").GetValue<bool>())
                            {
                                Utility.DrawCircle(obj.EndPosition, WardRange, obj.ObjectBase.Color);
                            }
                            posList = GetVision(obj.EndPosition, WardRange);
                            for (int j = 0; j < posList.Count; j++)
                            {
                                Vector2 visionPos1 = Drawing.WorldToScreen(posList[i]);
                                Vector2 visionPos2 = Drawing.WorldToScreen(posList[i]);
                                Drawing.DrawLine(visionPos1[0], visionPos1[1], visionPos2[0], visionPos2[1], 1.0f,
                                    obj.ObjectBase.Color);
                            }
                            Drawing.DrawText(objMPos[0], objMPos[1], obj.ObjectBase.Color, "V");
                            break;

                        case ObjectType.Unknown:
                            Drawing.DrawLine(Drawing.WorldToScreen(obj.StartPosition), Drawing.WorldToScreen(obj.EndPosition), 1, obj.ObjectBase.Color);
                            break;
                    }
                    Utility.DrawCircle(obj.EndPosition, 50, obj.ObjectBase.Color);
                    float endTime = obj.EndTime - Game.Time;
                    if (!float.IsInfinity(endTime) && !float.IsNaN(endTime) && endTime.CompareTo(float.MaxValue) != 0)
                    {
                        var m = (float) Math.Floor(endTime/60);
                        var s = (float) Math.Ceiling(endTime%60);
                        String ms = (s < 10 ? m + ":0" + s : m + ":" + s);
                        Drawing.DrawText(objPos[0], objPos[1], obj.ObjectBase.Color, ms);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HiddenObjectDraw: " + ex);
            }
        }

        private List<Vector3> GetVision(Vector3 viewPos, float range) //TODO: ADD IT
        {
            var list = new List<Vector3>();
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

        private Object HiddenObjectById(int id)
        {
            return Objects.FirstOrDefault(vision => id == vision.Id2);
        }

        private bool ObjectExist(Vector3 pos)
        {
            return HidObjects.Any(obj => pos.Distance(obj.EndPosition) < 30);
        }

        private void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                var reader = new BinaryReader(new MemoryStream(args.PacketData));
                byte packetId = reader.ReadByte(); //PacketId
                if (packetId == 181) //OLD 180
                {
                    int networkId = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                    var creator = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(networkId);
                    if (creator != null && creator.Team != ObjectManager.Player.Team)
                    {
                        reader.ReadBytes(7);
                        int id = reader.ReadInt32();
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
                            if (obj.Type == ObjectType.Trap)
                                pos = new Vector3(x, z, y);
                            networkId = networkId + 2;
                            Utility.DelayAction.Add(1, () =>
                            {
                                for (int i = 0; i < HidObjects.Count; i++)
                                {
                                    ObjectData objectData = HidObjects[i];
                                    if (objectData != null && objectData.NetworkId == networkId)
                                    {
                                        var objNew = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(networkId);
                                        if (objNew != null && objNew.IsValid)
                                            objectData.EndPosition = objNew.Position;
                                    }
                                }
                            });
                            HidObjects.Add(new ObjectData(obj, pos, Game.Time + obj.Duration, creator.Name, null,
                                networkId));
                        }
                    }
                }
                else if (packetId == 178)
                {
                    int networkId = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                    var gObject = ObjectManager.GetUnitByNetworkId<GameObject>(networkId);
                    if (gObject != null)
                    {
                        for (int i = 0; i < HidObjects.Count; i++)
                        {
                            ObjectData objectData = HidObjects[i];
                            if (objectData != null && objectData.NetworkId == networkId)
                            {
                                objectData.EndPosition = gObject.Position;
                            }
                        }
                    }
                }
                else if (packetId == 50) //OLD 49
                {
                    int networkId = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                    for (int i = 0; i < HidObjects.Count; i++)
                    {
                        ObjectData objectData = HidObjects[i];
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
                Console.WriteLine("HiddenObjectProcess: " + ex);
            }
        }

        private void Obj_AI_Base_OnDelete(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                if (!sender.IsValid)
                    return;
                for (int i = 0; i < HidObjects.Count; i++)
                {
                    ObjectData obj = HidObjects[i];
                    if ((obj.ObjectBase != null && sender.Name == obj.ObjectBase.ObjectName) ||
                        sender.Name.Contains("Ward") && sender.Name.Contains("Death"))
                        if (sender.Position.Distance(obj.EndPosition) < 30 || sender.Position.Distance(obj.StartPosition) < 30)
                        {
                            HidObjects.RemoveAt(i);
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HiddenObjectDelete: " + ex);
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                if (!sender.IsValid)
                    return;
                if (ObjectManager.Player.Team != sender.Team)
                {
                    foreach (Object obj in Objects)
                    {
                        if (args.SData.Name == obj.SpellName && !ObjectExist(args.End))
                        {
                            HidObjects.Add(new ObjectData(obj, args.End, Game.Time + obj.Duration, sender.Name, null,
                                sender.NetworkId));
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HiddenObjectSpell: " + ex);
            }
        }

        public class Object
        {
            public Color Color;
            public float Duration;
            public int Id;
            public int Id2;
            public String Name;
            public String ObjectName;
            public String SpellName;
            public ObjectType Type;

            public Object(ObjectType type, String name, String objectName, String spellName, float duration, int id,
                int id2, Color color)
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
            public String Creator;
            public float EndTime;
            public int NetworkId;
            public Object ObjectBase;
            public List<Vector2> Points;
            public Vector3 EndPosition;
            public Vector3 StartPosition;

            public ObjectData(Object objectBase, Vector3 endPosition, float endTime, String creator, List<Vector2> points,
                int networkId, Vector3 startPosition = new Vector3())
            {
                ObjectBase = objectBase;
                EndPosition = endPosition;
                EndTime = endTime;
                Creator = creator;
                Points = points;
                NetworkId = networkId;
                StartPosition = startPosition;
            }
        }
    }

    internal class DestinationTracker
    {
        private static Dictionary<Obj_AI_Hero, List<Ability>> Enemies =
            new Dictionary<Obj_AI_Hero, List<Ability>>();

        public DestinationTracker()
        {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    var abilities = new List<Ability>();
                    foreach (SpellDataInst spell in hero.SummonerSpellbook.Spells)
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
                    if (abilities.Count > 0)
                        AddObject(hero, abilities);
                }
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private bool AddObject(Obj_AI_Hero hero, List<Ability> abilities)
        {
            if (Enemies.ContainsKey(hero))
                return false;
            Enemies.Add(hero, abilities);
            return true;
            //TODO:Add
        }

        ~DestinationTracker()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast -= Obj_AI_Hero_OnProcessSpellCast;
            GameObject.OnCreate -= Obj_AI_Base_OnCreate;
            Drawing.OnDraw -= Drawing_OnDraw;
            
            Enemies = null;
        }

        public bool IsActive()
        {
            return Menu.Tracker.GetActive() && Menu.DestinationTracker.GetActive();
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var enemy in Enemies)
            {
                foreach (Ability ability in enemy.Value)
                {
                    if (ability.Casted)
                    {
                        Vector2 startPos = Drawing.WorldToScreen(ability.StartPos);
                        Vector2 endPos = Drawing.WorldToScreen(ability.EndPos);

                        if (ability.OutOfBush)
                        {
                            Utility.DrawCircle(ability.EndPos, ability.Range, Color.Red);
                        }
                        else
                        {
                            Utility.DrawCircle(ability.EndPos, ability.Range, Color.Red);
                            Drawing.DrawLine(startPos[0], startPos[1], endPos[0], endPos[1], 1.0f, Color.Red);
                        }
                        Drawing.DrawText(endPos[0], endPos[1], Color.Bisque,
                            enemy.Key.ChampionName + " " + ability.SpellName);
                    }
                }
            }
        }

        private void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var enemy in Enemies)
            {
                if (enemy.Key.ChampionName == "Shaco")
                {
                    if (sender.Type != GameObjectType.obj_LampBulb && sender.Name == "JackintheboxPoof2.troy" && !enemy.Value[0].Casted)
                    {
                        enemy.Value[0].StartPos = sender.Position;
                        enemy.Value[0].EndPos = sender.Position;
                        enemy.Value[0].Casted = true;
                        enemy.Value[0].TimeCasted = (int) Game.Time;
                        enemy.Value[0].OutOfBush = true;
                    }
                }
            }
        }

        private Vector3 CalculateEndPos(Ability ability, GameObjectProcessSpellCastEventArgs args)
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
                Vector3 endPos = args.Start - norm*ability.Range;

                //endPos = FindNearestNonWall(); TODO: Add FindNearestNonWall

                ability.EndPos = endPos;
            }
            return ability.EndPos;
        }

        private void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!IsActive())
                return;
            if (sender.GetType() == typeof (Obj_AI_Hero))
            {
                var hero = (Obj_AI_Hero) sender;
                if (hero.IsEnemy)
                {
                    Obj_AI_Hero enemy = hero;
                    foreach (var abilities in Enemies)
                    {
                        if (abilities.Key.NetworkId != enemy.NetworkId)
                            continue;
                        int index = 0;
                        foreach (Ability ability in abilities.Value)
                        {
                            if (args.SData.Name == "vayneinquisition")
                            {
                                if (ability.ExtraTicks > 0)
                                {
                                    ability.ExtraTicks = (int) Game.Time + 6 + 2*args.Level;
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
                                ability.TimeCasted = (int) Game.Time;
                                return;
                            }
                            index++;
                        }
                    }
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var abilities in Enemies)
            {
                foreach (Ability ability in abilities.Value)
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

        public class Ability
        {
            public bool Casted;
            public float Delay;
            public Vector3 EndPos;
            public int ExtraTicks;
            public bool OutOfBush;
            public Obj_AI_Hero Owner;
            public int Range;
            public String SpellName;
            public Vector3 StartPos;
            public Obj_AI_Hero Target;
            public bool TargetDead;
            public int TimeCasted;

            public Ability(string spellName, int range, float delay, Obj_AI_Hero owner)
            {
                SpellName = spellName;
                Range = range;
                Delay = delay;
                Owner = owner;
            }
        }
    }

    internal class SsCaller
    {
        public static Dictionary<Obj_AI_Hero, Time> Enemies = new Dictionary<Obj_AI_Hero, Time>();

        public SsCaller()
        {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
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
            Enemies = null;
        }

        public bool IsActive()
        {
            return Menu.Tracker.GetActive() && Menu.SsCaller.GetActive() &&
                   Game.Time < (Menu.SsCaller.GetMenuItem("SAwarenessSSCallerDisableTime").GetValue<Slider>().Value*60);
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var enemy in Enemies)
            {
                UpdateTime(enemy);
                HandleSs(enemy);
            }
        }

        private void HandleSs(KeyValuePair<Obj_AI_Hero, Time> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            if (enemy.Value.InvisibleTime > 5 && !enemy.Value.Called && Game.Time - enemy.Value.LastTimeCalled > 30)
            {
                var pos = new Vector2(hero.Position.X, hero.Position.Y);
                var pingType = Packet.PingType.Normal;
                var t = Menu.SsCaller.GetMenuItem("SAwarenessSSCallerPingType").GetValue<StringList>();
                pingType = (Packet.PingType) t.SelectedIndex + 1;
                GamePacket gPacketT;
                for (int i = 0;
                    i < Menu.SsCaller.GetMenuItem("SAwarenessSSCallerPingTimes").GetValue<Slider>().Value;
                    i++)
                {
                    if (Menu.SsCaller.GetMenuItem("SAwarenessSSCallerLocalPing").GetValue<bool>())
                    {
                        gPacketT =
                            Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(pos[0], pos[1], 0, 0, pingType));
                        gPacketT.Process();
                    }
                    else if (!Menu.SsCaller.GetMenuItem("SAwarenessSSCallerLocalPing").GetValue<bool>() &&
                             Menu.GlobalSettings.GetMenuItem("SAwarenessGlobalSettingsServerChatPingActive")
                                 .GetValue<bool>())
                    {
                        gPacketT =
                            Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(enemy.Value.LastPosition.X,
                                enemy.Value.LastPosition.Y, 0, pingType));
                        gPacketT.Send();
                    }
                }
                if (Menu.SsCaller.GetMenuItem("SAwarenessSSCallerChatChoice").GetValue<StringList>().SelectedIndex == 1)
                {
                    Game.PrintChat("ss {0}", hero.ChampionName);
                }
                else if (
                    Menu.SsCaller.GetMenuItem("SAwarenessSSCallerChatChoice").GetValue<StringList>().SelectedIndex ==
                    2 &&
                    Menu.GlobalSettings.GetMenuItem("SAwarenessGlobalSettingsServerChatPingActive").GetValue<bool>())
                {
                    Game.Say("ss {0}", hero.ChampionName);
                }
                enemy.Value.LastTimeCalled = (int) Game.Time;
                enemy.Value.Called = true;
            }
        }

        private void UpdateTime(KeyValuePair<Obj_AI_Hero, Time> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            if (hero.IsVisible)
            {
                Enemies[hero].InvisibleTime = 0;
                Enemies[hero].VisibleTime = (int) Game.Time;
                enemy.Value.Called = false;
                Enemies[hero].LastPosition = hero.ServerPosition;
            }
            else
            {
                if (Enemies[hero].VisibleTime != 0)
                {
                    Enemies[hero].InvisibleTime = (int) (Game.Time - Enemies[hero].VisibleTime);
                }
                else
                {
                    Enemies[hero].InvisibleTime = 0;
                }
            }
        }

        public class Time
        {
            public bool Called;
            public int InvisibleTime;
            public Vector3 LastPosition;
            public int LastTimeCalled;
            public int VisibleTime;
        }
    }

    public class UiTracker
    {
        public static readonly Dictionary<Obj_AI_Hero, ChampInfos> _allies = new Dictionary<Obj_AI_Hero, ChampInfos>();
        public static readonly Dictionary<Obj_AI_Hero, ChampInfos> _enemies = new Dictionary<Obj_AI_Hero, ChampInfos>();
        /*private static Font _champF;
        private static Render.Rectangle _recB;
        private static Font _recF;
        private static Render.Rectangle _recS;
        private static Render.Rectangle _recNS;
        private static Sprite _s;
        private static Font _spellF;
        private static Font _sumF;
        private static Font _champIF;
        public static Render.Sprite _backBar;
        public static Render.Sprite _healthBar;
        public static Render.Sprite _manaBar;
        public static Render.Sprite _overlayEmptyItem;
        public static Render.Sprite _overlayRecall;
        public static Render.Sprite _overlaySpellItem;
        public static Render.Sprite _overlaySpellItemRed;
        public static Render.Sprite _overlaySpellItemGreen;
        public static Render.Sprite _overlaySummoner;
        public static Render.Sprite _overlaySummonerSpell;
        public static Render.Sprite _overlayGoldCsLvl;*/
        /*private Texture _backBar;
        private Texture _healthBar;
        private Texture _manaBar;
        private Texture _overlayEmptyItem;
        private Texture _overlayRecall;
        private Texture _overlaySpellItem;
        private Texture _overlaySpellItemRed;
        private Texture _overlaySpellItemGreen;
        private Texture _overlaySummoner;
        private Texture _overlaySummonerSpell;
        private Texture _overlayGoldCsLvl;*/
        //private static bool _drawActive = true;
        private static Size _backBarSize = new Size(96, 10);
        private static Size _champSize = new Size(64, 64);
        private static Size _healthManaBarSize = new Size(96, 5);
        private static Size _recSize = new Size(64, 12);
        private static Vector2 _screen = new Vector2(Drawing.Width, Drawing.Height/2);
        private static Size _spellSize = new Size(16, 16);
        private static Size _sumSize = new Size(32, 32);


        private Size _hudSize;
        private Vector2 _lastCursorPos;
        private bool _moveActive;
        private int _oldAx = 0;
        private int _oldAy = 0;
        private int _oldEx;
        private int _oldEy;
        private float _scalePc = 1.0f;
        private bool _shiftActive;

        public UiTracker()
        {
            UpdateItems(true);
            UpdateItems(false);
            CalculateSizes(true);
            CalculateSizes(false);
            //bool loaded = false;
            //int tries = 0;
            //while (!loaded)
            //{
            //    loaded = Init(tries >= 5);

            //    tries++;
            //    if (tries > 9)
            //    {
            //        Console.WriteLine("Couldn't load Interface. It got disabled.");
            //        Menu.UiTracker.ForceDisable = true;
            //        Menu.UiTracker.Item = null;
            //        return;
            //    }
            //    Thread.Sleep(10);
            //}
            ////new System.Threading.Thread(() =>
            ////{
            ////    SpecUtils.GetInfo();
            ////}).Start();
            Game.OnGameUpdate += Game_OnGameUpdate;
            //Game.OnGameProcessPacket += Game_OnGameProcessPacket; //TODO:Enable for Gold View currently bugged packet id never received
            Game.OnWndProc += Game_OnWndProc;
            //AppDomain.CurrentDomain.DomainUnload += delegate { Drawing_OnPreReset(new EventArgs()); };
            //AppDomain.CurrentDomain.ProcessExit += delegate { Drawing_OnPreReset(new EventArgs()); };
        }
        
         ~UiTracker()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Game.OnGameProcessPacket -= Game_OnGameProcessPacket;
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if (!IsActive())
                return;
            HandleInput((WindowsMessages) args.Msg, Utils.GetCursorPos(), args.WParam);
        }

        private void HandleInput(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            HandleUiMove(message, cursorPos, key);
            HandleChampClick(message, cursorPos, key);
        }

        private void HandleUiMove(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if (message != WindowsMessages.WM_LBUTTONDOWN && message != WindowsMessages.WM_MOUSEMOVE &&
                message != WindowsMessages.WM_LBUTTONUP || (!_moveActive && message == WindowsMessages.WM_MOUSEMOVE)
                )
            {
                return;
            }
            if (message == WindowsMessages.WM_LBUTTONDOWN)
            {
                _lastCursorPos = cursorPos;
            }
            if (message == WindowsMessages.WM_LBUTTONUP)
            {
                _lastCursorPos = new Vector2();
                _moveActive = false;
                return;
            }
            var firstEnemyHero = new KeyValuePair<Obj_AI_Hero, ChampInfos>();
            foreach (var enemy in _enemies.Reverse())
            {
                firstEnemyHero = enemy;
                break;
            }
            if (firstEnemyHero.Key != null &&
                Common.IsInside(cursorPos, firstEnemyHero.Value.SpellPassive.SizeSideBar,
                    _hudSize.Width, _hudSize.Height))
            {
                _moveActive = true;
                if (message == WindowsMessages.WM_MOUSEMOVE)
                {
                    var curSliderX =
                        Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                            .GetMenuItem("SAwarenessUITrackerEnemyTrackerXPos")
                            .GetValue<Slider>();
                    var curSliderY =
                        Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                            .GetMenuItem("SAwarenessUITrackerEnemyTrackerYPos")
                            .GetValue<Slider>();
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerXPos")
                        .SetValue(new Slider((int) (curSliderX.Value + cursorPos.X - _lastCursorPos.X),
                            curSliderX.MinValue, curSliderX.MaxValue));
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerYPos")
                        .SetValue(new Slider((int) (curSliderY.Value + cursorPos.Y - _lastCursorPos.Y),
                            curSliderY.MinValue, curSliderY.MaxValue));
                    _lastCursorPos = cursorPos;
                }
            }
            var firstAllyHero = new KeyValuePair<Obj_AI_Hero, ChampInfos>();
            foreach (var ally in _allies.Reverse())
            {
                firstAllyHero = ally;
                break;
            }
            if (firstAllyHero.Key != null &&
                Common.IsInside(cursorPos, firstAllyHero.Value.SpellPassive.SizeSideBar,
                    _hudSize.Width, _hudSize.Height))
            {
                _moveActive = true;
                if (message == WindowsMessages.WM_MOUSEMOVE)
                {
                    var curSliderX =
                        Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                            .GetMenuItem("SAwarenessUITrackerAllyTrackerXPos")
                            .GetValue<Slider>();
                    var curSliderY =
                        Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                            .GetMenuItem("SAwarenessUITrackerAllyTrackerYPos")
                            .GetValue<Slider>();
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerXPos")
                        .SetValue(new Slider((int) (curSliderX.Value + cursorPos.X - _lastCursorPos.X),
                            curSliderX.MinValue, curSliderX.MaxValue));
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerYPos")
                        .SetValue(new Slider((int) (curSliderY.Value + cursorPos.Y - _lastCursorPos.Y),
                            curSliderY.MinValue, curSliderY.MaxValue));
                    _lastCursorPos = cursorPos;
                }
            }
        }

        private void HandleChampClick(WindowsMessages message, Vector2 cursorPos, uint key)
        {
            if ((message != WindowsMessages.WM_KEYDOWN && key == 16) && message != WindowsMessages.WM_LBUTTONDOWN &&
                (message != WindowsMessages.WM_KEYUP && key == 16) ||
                (!_shiftActive && message == WindowsMessages.WM_LBUTTONDOWN))
            {
                return;
            }
            if (message == WindowsMessages.WM_KEYDOWN && key == 16)
            {
                _shiftActive = true;
            }
            if (message == WindowsMessages.WM_KEYUP && key == 16)
            {
                _shiftActive = false;
            }
            if (message == WindowsMessages.WM_LBUTTONDOWN)
            {
                foreach (var enemy in _enemies.Reverse())
                {
                    if (Common.IsInside(cursorPos, enemy.Value.Champ.SizeSideBar, _champSize.Width,
                        _champSize.Height))
                    {
                        //TODO: Add Camera move
                        if (Menu.UiTracker.GetMenuItem("SAwarenessUITrackerPingActive").GetValue<bool>())
                        {
                            Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(enemy.Key.ServerPosition.X,
                                enemy.Key.ServerPosition.Y, 0, 0, Packet.PingType.Normal)).Process();
                        }
                    }
                }
            }
        }

        //private void Drawing_OnPostReset(EventArgs args)
        //{
        //    if (Drawing.Direct3DDevice == null || Drawing.Direct3DDevice.IsDisposed)
        //        return;
        //    //_s.OnResetDevice();
        //    //_champF.OnResetDevice();
        //    //_spellF.OnResetDevice();
        //    //_sumF.OnResetDevice();
        //    //_recF.OnResetDevice();
        //    //_recS.OnPostReset();
        //    //_recB.OnPreReset();
        //    //_recNS.OnPreReset();
        //    _drawActive = true;
        //}

        //private void Drawing_OnPreReset(EventArgs args)
        //{
        //    //_s.OnLostDevice();
        //    //_champF.OnLostDevice();
        //    //_spellF.OnLostDevice();
        //    //_sumF.OnLostDevice();
        //    //_recF.OnLostDevice();
        //    //_recS.OnPreReset();
        //    //_recB.OnPreReset();
        //    //_recNS.OnPostReset();
        //    _drawActive = false;
        //}
        public bool IsActive()
        {
            return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive();
        }

        public async static Task Init()
        {
            try
            {
                //_s = new Sprite(Drawing.Direct3DDevice);
                //_recF = new Font(Drawing.Direct3DDevice, new System.Drawing.Font("Times New Roman", 12));
                //_spellF = new Font(Drawing.Direct3DDevice, new System.Drawing.Font("Times New Roman", 8));
                //_champF = new Font(Drawing.Direct3DDevice, new System.Drawing.Font("Times New Roman", 24));
                //_sumF = new Font(Drawing.Direct3DDevice, new System.Drawing.Font("Times New Roman", 16));
                //_recS = new Render.Rectangle(0, 0, 16, 16, SharpDX.Color.Green);
                //_recB = new Render.Rectangle(0, 0, (int)(16 * 1.7), (int)(16 * 1.7), SharpDX.Color.Green);
                //_recNS = new Render.Rectangle(0, 0, 32, 16, SharpDX.Color.Green);
            }
            catch (Exception)
            {
                //return false;
                //throw;
            }
            if (
                Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                    .GetMenuItem("SAwarenessUITrackerEnemyTrackerXPos")
                    .GetValue<Slider>()
                    .Value == 0)
            {
                Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                    .GetMenuItem("SAwarenessUITrackerEnemyTrackerXPos")
                    .SetValue(new Slider((int) _screen.X, Drawing.Width, 0));
            }
            if (
                Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                    .GetMenuItem("SAwarenessUITrackerEnemyTrackerYPos")
                    .GetValue<Slider>()
                    .Value == 0)
            {
                Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                    .GetMenuItem("SAwarenessUITrackerEnemyTrackerYPos")
                    .SetValue(new Slider((int) _screen.Y, Drawing.Height, 0));
            }
            if (
                Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                    .GetMenuItem("SAwarenessUITrackerAllyTrackerXPos")
                    .GetValue<Slider>()
                    .Value == 0)
            {
                Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                    .GetMenuItem("SAwarenessUITrackerAllyTrackerXPos")
                    .SetValue(new Slider((int) _screen.X, Drawing.Width, 0));
            }
            if (
                Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                    .GetMenuItem("SAwarenessUITrackerAllyTrackerYPos")
                    .GetValue<Slider>()
                    .Value == 0)
            {
                Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                    .GetMenuItem("SAwarenessUITrackerAllyTrackerYPos")
                    .SetValue(new Slider((int) _screen.Y, Drawing.Height, 0));
            }

            //SpriteHelper.LoadTexture("SummonerTint", ref _overlaySummoner, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("SummonerSpellTint", ref _overlaySummonerSpell, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("SpellTint", ref _overlaySpellItem, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("SpellTintRed", ref _overlaySpellItemRed, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("SpellTintGreen", ref _overlaySpellItemGreen, SpriteHelper.TextureType.Default);

            //SpriteHelper.LoadTexture("BarBackground", ref _backBar, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("HealthBar", ref _healthBar, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("ManaBar", ref _manaBar, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("ItemSlotEmpty", ref _overlayEmptyItem, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("RecallBar", ref _overlayRecall, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("GoldCsLvlBar", ref _overlayGoldCsLvl, SpriteHelper.TextureType.Default);

            //muss in champinfos nun rein da nur einmal verwendbar
            //nur sidebar wird angezeigt kein overhead, wahrscheinlich 2 instanzen von nöten.
            //SpriteHelper.LoadTexture("SummonerTint", ref _overlaySummoner, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("SummonerSpellTint", ref _overlaySummonerSpell, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("SpellTint", ref _overlaySpellItem, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("SpellTintRed", ref _overlaySpellItemRed, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("SpellTintGreen", ref _overlaySpellItemGreen, SpriteHelper.TextureType.Default);

            //SpriteHelper.LoadTexture("BarBackground", ref _backBar, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("HealthBar", ref _healthBar, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("ManaBar", ref _manaBar, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("ItemSlotEmpty", ref _overlayEmptyItem, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("RecallBar", ref _overlayRecall, SpriteHelper.TextureType.Default);
            //SpriteHelper.LoadTexture("GoldCsLvlBar", ref _overlayGoldCsLvl, SpriteHelper.TextureType.Default);

            float percentScale =
                    (float)Menu.UiTracker.GetMenuItem("SAwarenessUITrackerScale").GetValue<Slider>().Value / 100;

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if(hero.IsMe)
                    continue;
                var champ = new ChampInfos();

                Task<ChampInfos> champInfos = CreateSideHud(hero, champ, percentScale);
                champ = await champInfos;
                champInfos = CreateOverHeadHud(hero, champ, percentScale);
                champ = await champInfos;

                if (hero.IsEnemy)
                {
                  _enemies.Add(hero, champ);
                }
                if (!hero.IsEnemy)
                {
                    _allies.Add(hero, champ);
                }
            }      
            //return true;
        }

        private async static Task<ChampInfos> CreateSideHud(Obj_AI_Hero hero, ChampInfos champ, float percentScale)
        {
            float percentHealth = CalcHpBar(hero);
            float percentMana = CalcManaBar(hero);
            //SpriteHelper.LoadTexture("ItemSlotEmpty", ref _overlayEmptyItem, SpriteHelper.TextureType.Default);
            Console.WriteLine(hero.ChampionName);
            Console.WriteLine("Champ");
            Task<SpriteHelper.SpriteInfo> taskInfo = null;
            taskInfo = SpriteHelper.LoadTextureAsync(hero.ChampionName, champ.Champ.Sprite[0], SpriteHelper.DownloadType.Champion);
            champ.Champ.Sprite[0] = await taskInfo;
            if (!champ.Champ.Sprite[0].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateChampImage(hero, champ.Champ.SizeSideBar, champ.Champ.Sprite[0], UpdateMethod.Side));
            }
            else
            {
                SetScale(ref champ.Champ.Sprite[0].Sprite, _champSize, percentScale);
                champ.Champ.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.Champ.SizeSideBar.Width, champ.Champ.SizeSideBar.Height);
                };
                champ.Champ.Sprite[0].Sprite.VisibleCondition = delegate
                {
                    return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                };
                champ.Champ.Sprite[0].Sprite.Add();
            }

            SpellDataInst[] s1 = hero.Spellbook.Spells;
            //SpriteHelper.LoadTexture(s1[0].Name + ".dds", "PASSIVE/", loc + "PASSIVE\\" + s1[0].Name + ".dds", ref champ.Passive.Texture);
            Console.WriteLine("SpellQ");
            taskInfo = SpriteHelper.LoadTextureAsync(s1[0].Name, champ.SpellQ.Sprite[0], SpriteHelper.DownloadType.Spell);
            champ.SpellQ.Sprite[0] = await taskInfo;
            if (!champ.SpellQ.Sprite[0].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, champ.SpellQ.SizeSideBar, champ.SpellQ.Sprite[0], SpellSlot.Q, UpdateMethod.Side));
            }
            else
            {
                SetScale(ref champ.SpellQ.Sprite[0].Sprite, _spellSize, percentScale);
                champ.SpellQ.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellQ.SizeSideBar.Width, champ.SpellQ.SizeSideBar.Height);
                };
                champ.SpellQ.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                };
                champ.SpellQ.Sprite[0].Sprite.Add();
            }

            Console.WriteLine("SpellW");
            taskInfo = SpriteHelper.LoadTextureAsync(s1[1].Name, champ.SpellW.Sprite[0], SpriteHelper.DownloadType.Spell);
            champ.SpellW.Sprite[0] = await taskInfo;
            if (!champ.SpellW.Sprite[0].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, champ.SpellW.SizeSideBar, champ.SpellW.Sprite[0], SpellSlot.W, UpdateMethod.Side));
            }
            else
            {
                SetScale(ref champ.SpellW.Sprite[0].Sprite, _spellSize, percentScale);
                champ.SpellW.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellW.SizeSideBar.Width, champ.SpellW.SizeSideBar.Height);
                };
                champ.SpellW.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                };
                champ.SpellW.Sprite[0].Sprite.Add();
            }

            Console.WriteLine("SpellE");
            taskInfo = SpriteHelper.LoadTextureAsync(s1[2].Name, champ.SpellE.Sprite[0], SpriteHelper.DownloadType.Spell);
            champ.SpellE.Sprite[0] = await taskInfo;
            if (!champ.SpellE.Sprite[0].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, champ.SpellE.SizeSideBar, champ.SpellE.Sprite[0], SpellSlot.E, UpdateMethod.Side));
            }
            else
            {
                SetScale(ref champ.SpellE.Sprite[0].Sprite, _spellSize, percentScale);
                champ.SpellE.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellE.SizeSideBar.Width, champ.SpellE.SizeSideBar.Height);
                };
                champ.SpellE.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                };
                champ.SpellE.Sprite[0].Sprite.Add();
            }

            Console.WriteLine("SpellR");
            taskInfo = SpriteHelper.LoadTextureAsync(s1[3].Name, champ.SpellR.Sprite[0], SpriteHelper.DownloadType.Spell);
            champ.SpellR.Sprite[0] = await taskInfo;
            if (!champ.SpellR.Sprite[0].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, champ.SpellR.SizeSideBar, champ.SpellR.Sprite[0], SpellSlot.R, UpdateMethod.Side));
            }
            else
            {
                SetScale(ref champ.SpellR.Sprite[0].Sprite, _spellSize, percentScale);
                champ.SpellR.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellR.SizeSideBar.Width, champ.SpellR.SizeSideBar.Height);
                };
                champ.SpellR.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                };
                champ.SpellR.Sprite[0].Sprite.Add();
            }

            SpellDataInst[] s2 = hero.SummonerSpellbook.Spells;
            Console.WriteLine("Spell1");
            taskInfo = SpriteHelper.LoadTextureAsync(s2[0].Name, champ.SpellSum1.Sprite[0], SpriteHelper.DownloadType.Summoner);
            champ.SpellSum1.Sprite[0] = await taskInfo;
            if (!champ.SpellSum1.Sprite[0].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSummonerSpellImage(hero, champ.SpellSum1.SizeSideBar, champ.SpellSum1.Sprite[0], SpellSlot.Summoner1, UpdateMethod.Side));
            }
            else
            {
                SetScale(ref champ.SpellSum1.Sprite[0].Sprite, _sumSize, percentScale);
                champ.SpellSum1.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellSum1.SizeSideBar.Width, champ.SpellSum1.SizeSideBar.Height);
                };
                champ.SpellSum1.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                };
                champ.SpellSum1.Sprite[0].Sprite.Add();
            }

            Console.WriteLine("Spell2");
            taskInfo = SpriteHelper.LoadTextureAsync(s2[1].Name, champ.SpellSum2.Sprite[0], SpriteHelper.DownloadType.Summoner);
            champ.SpellSum2.Sprite[0] = await taskInfo;
            if (!champ.SpellSum2.Sprite[0].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSummonerSpellImage(hero, champ.SpellSum2.SizeSideBar, champ.SpellSum2.Sprite[0], SpellSlot.Summoner2, UpdateMethod.Side));
            }
            else
            {
                SetScale(ref champ.SpellSum2.Sprite[0].Sprite, _sumSize, percentScale);
                champ.SpellSum2.Sprite[0].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellSum2.SizeSideBar.Width, champ.SpellSum2.SizeSideBar.Height);
                };
                champ.SpellSum2.Sprite[0].Sprite.VisibleCondition = sender =>
                {
                    return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                };
                champ.SpellSum2.Sprite[0].Sprite.Add();
            }

            Console.WriteLine("Backbar");
            champ.BackBar.Sprite[0] = new SpriteHelper.SpriteInfo();
            SpriteHelper.LoadTexture("BarBackground", ref champ.BackBar.Sprite[0], SpriteHelper.TextureType.Default);
            SetScale(ref champ.BackBar.Sprite[0].Sprite, _backBarSize, percentScale);
            champ.BackBar.Sprite[0].Sprite.PositionUpdate = delegate
            {
                return new Vector2(champ.BackBar.SizeSideBar.Width, champ.BackBar.SizeSideBar.Height);
            };
            champ.BackBar.Sprite[0].Sprite.VisibleCondition = delegate
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
            };
            champ.BackBar.Sprite[0].Sprite.Add();

            Console.WriteLine("Healthbar");
            champ.HealthBar.Sprite[0] = new SpriteHelper.SpriteInfo();
            SpriteHelper.LoadTexture("HealthBar", ref champ.HealthBar.Sprite[0], SpriteHelper.TextureType.Default);
            SetScale(ref champ.HealthBar.Sprite[0].Sprite, _healthManaBarSize, percentScale);
            //SetScaleX(ref champ.HealthBar.Sprite[0].Sprite, _healthManaBarSize, percentScale * percentHealth);
            champ.HealthBar.Sprite[0].Sprite.PositionUpdate = delegate
            {
                //SetScaleX(ref champ.HealthBar.Sprite[0].Sprite, _healthManaBarSize, percentScale * CalcHpBar(hero));
                return new Vector2(champ.HealthBar.SizeSideBar.Width, champ.HealthBar.SizeSideBar.Height);
            };
            champ.HealthBar.Sprite[0].Sprite.VisibleCondition = delegate
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
            };
            champ.HealthBar.Sprite[0].Sprite.Add();

            Console.WriteLine("Manabar");
            champ.ManaBar.Sprite[0] = new SpriteHelper.SpriteInfo();
            SpriteHelper.LoadTexture("ManaBar", ref champ.ManaBar.Sprite[0], SpriteHelper.TextureType.Default);
            SetScale(ref champ.ManaBar.Sprite[0].Sprite, _healthManaBarSize, percentScale);
            //SetScaleX(ref champ.ManaBar.Sprite[0].Sprite, _healthManaBarSize, percentScale * percentMana);
            champ.ManaBar.Sprite[0].Sprite.PositionUpdate = delegate
            {
                //SetScaleX(ref champ.ManaBar.Sprite[0].Sprite, _healthManaBarSize, percentScale * CalcManaBar(hero));
                return new Vector2(champ.ManaBar.SizeSideBar.Width, champ.ManaBar.SizeSideBar.Height);
            };
            champ.ManaBar.Sprite[0].Sprite.VisibleCondition = delegate
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
            };
            champ.ManaBar.Sprite[0].Sprite.Add();

            Console.WriteLine("Recallbar");
            champ.RecallBar.Sprite[0] = new SpriteHelper.SpriteInfo();
            SpriteHelper.LoadTexture("RecallBar", ref champ.RecallBar.Sprite[0], SpriteHelper.TextureType.Default);
            SetScale(ref champ.RecallBar.Sprite[0].Sprite, _recSize, percentScale);
            champ.RecallBar.Sprite[0].Sprite.PositionUpdate = delegate
            {
                return new Vector2(champ.RecallBar.SizeSideBar.Width, champ.RecallBar.SizeSideBar.Height);
            };
            champ.RecallBar.Sprite[0].Sprite.VisibleCondition = delegate
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 && Menu.RecallDetector.GetActive();
            };
            champ.RecallBar.Sprite[0].Sprite.Color = new ColorBGRA(Color3.White, 0.55f);
            champ.RecallBar.Sprite[0].Sprite.Add();

            Console.WriteLine("Goldbar");
            champ.GoldCsLvlBar.Sprite[0] = new SpriteHelper.SpriteInfo();
            SpriteHelper.LoadTexture("GoldCsLvlBar", ref champ.GoldCsLvlBar.Sprite[0], SpriteHelper.TextureType.Default);
            SetScale(ref champ.GoldCsLvlBar.Sprite[0].Sprite, _recSize, percentScale);
            champ.GoldCsLvlBar.Sprite[0].Sprite.PositionUpdate = delegate
            {
                return new Vector2(champ.Champ.SizeSideBar.Width, champ.Champ.SizeSideBar.Height);
            };
            champ.GoldCsLvlBar.Sprite[0].Sprite.VisibleCondition = delegate
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
            };
            champ.GoldCsLvlBar.Sprite[0].Sprite.Color = new ColorBGRA(Color3.White, 0.55f);
            champ.GoldCsLvlBar.Sprite[0].Sprite.Add();

            ///////

            champ.HealthBar.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.HealthBar.Text[0].TextUpdate = delegate
            {
                return champ.SHealth ?? "";
            };
            champ.HealthBar.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.HealthBar.CoordsSideBar.Width, champ.HealthBar.CoordsSideBar.Height);
            };
            champ.HealthBar.Text[0].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
            };
            champ.HealthBar.Text[0].OutLined = true;
            champ.HealthBar.Text[0].Centered = true;
            champ.HealthBar.Text[0].Add();

            champ.ManaBar.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.ManaBar.Text[0].TextUpdate = delegate
            {
                return champ.SMana ?? "";
            };
            champ.ManaBar.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.ManaBar.CoordsSideBar.Width, champ.ManaBar.CoordsSideBar.Height);
            };
            champ.ManaBar.Text[0].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
            };
            champ.ManaBar.Text[0].OutLined = true;
            champ.ManaBar.Text[0].Centered = true;
            champ.ManaBar.Text[0].Add();

            champ.SpellQ.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellQ.Text[0].TextUpdate = delegate
            {
                return champ.SpellQ.Value.ToString();
            };
            champ.SpellQ.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellQ.CoordsSideBar.Width, champ.SpellQ.CoordsSideBar.Height);
            };
            champ.SpellQ.Text[0].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellQ.Value > 0.0f;
            };
            champ.SpellQ.Text[0].OutLined = true;
            champ.SpellQ.Text[0].Centered = true;
            champ.SpellQ.Text[0].Add();

            champ.SpellW.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellW.Text[0].TextUpdate = delegate
            {
                return champ.SpellW.Value.ToString();
            };
            champ.SpellW.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellW.CoordsSideBar.Width, champ.SpellW.CoordsSideBar.Height);
            };
            champ.SpellW.Text[0].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellW.Value > 0.0f;
            };
            champ.SpellW.Text[0].OutLined = true;
            champ.SpellW.Text[0].Centered = true;
            champ.SpellW.Text[0].Add();

            champ.SpellE.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellE.Text[0].TextUpdate = delegate
            {
                return champ.SpellE.Value.ToString();
            };
            champ.SpellE.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellE.CoordsSideBar.Width, champ.SpellE.CoordsSideBar.Height);
            };
            champ.SpellE.Text[0].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellE.Value > 0.0f;
            };
            champ.SpellE.Text[0].OutLined = true;
            champ.SpellE.Text[0].Centered = true;
            champ.SpellE.Text[0].Add();

            champ.SpellR.Text[0] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellR.Text[0].TextUpdate = delegate
            {
                return champ.SpellR.Value.ToString();
            };
            champ.SpellR.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellR.CoordsSideBar.Width, champ.SpellR.CoordsSideBar.Height);
            };
            champ.SpellR.Text[0].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellR.Value > 0.0f;
            };
            champ.SpellR.Text[0].OutLined = true;
            champ.SpellR.Text[0].Centered = true;
            champ.SpellR.Text[0].Add();

            champ.Champ.Text[0] = new Render.Text(0, 0, "", 30, SharpDX.Color.Orange);
            champ.Champ.Text[0].TextUpdate = delegate
            {
                if (champ.DeathTimeDisplay > 0.0f && hero.IsDead)
                    return champ.DeathTimeDisplay.ToString();
                else if (champ.InvisibleTime > 0.0f && !hero.IsVisible)
                    return champ.InvisibleTime.ToString();
                return "";
            };
            champ.Champ.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.Champ.CoordsSideBar.Width, champ.Champ.CoordsSideBar.Height);
            };
            champ.Champ.Text[0].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    ((champ.DeathTimeDisplay > 0.0f && hero.IsDead) || (champ.InvisibleTime > 0.0f && !hero.IsVisible));
            };
            champ.Champ.Text[0].OutLined = true;
            champ.Champ.Text[0].Centered = true;
            champ.Champ.Text[0].Add();

            champ.SpellSum1.Text[0] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.SpellSum1.Text[0].TextUpdate = delegate
            {
                return champ.SpellSum1.Value.ToString();
            };
            champ.SpellSum1.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellSum1.CoordsSideBar.Width, champ.SpellSum1.CoordsSideBar.Height);
            };
            champ.SpellSum1.Text[0].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellSum1.Value > 0.0f;
            };
            champ.SpellSum1.Text[0].OutLined = true;
            champ.SpellSum1.Text[0].Centered = true;
            champ.SpellSum1.Text[0].Add();

            champ.SpellSum2.Text[0] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.SpellSum2.Text[0].TextUpdate = delegate
            {
                return champ.SpellSum2.Value.ToString();
            };
            champ.SpellSum2.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellSum2.CoordsSideBar.Width, champ.SpellSum2.CoordsSideBar.Height);
            };
            champ.SpellSum2.Text[0].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellSum2.Value > 0.0f;
            };
            champ.SpellSum2.Text[0].OutLined = true;
            champ.SpellSum2.Text[0].Centered = true;
            champ.SpellSum2.Text[0].Add();

            foreach (var item in champ.Item)
            {
                if (item == null)
                    continue;
                item.Text[0] = new Render.Text(0, 0, "", 12, SharpDX.Color.Orange);
                item.Text[0].TextUpdate = delegate
                {
                    return item.Value.ToString();
                };
                item.Text[0].PositionUpdate = delegate
                {
                    return new Vector2(item.CoordsSideBar.Width, item.CoordsSideBar.Height);
                };
                item.Text[0].VisibleCondition = sender =>
                {
                    return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                        item.Value > 0.0f && Menu.UiTracker.GetMenuItem("SAwarenessItemPanelActive").GetValue<bool>();
                };
                item.Text[0].OutLined = true;
                item.Text[0].Centered = true;
                item.Text[0].Add();
            }

            champ.Level.Text[0] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.Level.Text[0].TextUpdate = delegate
            {
                return champ.Level.Value.ToString();
            };
            champ.Level.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.Level.CoordsSideBar.Width, champ.Level.CoordsSideBar.Height);
            };
            champ.Level.Text[0].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
            };
            champ.Level.Text[0].OutLined = true;
            champ.Level.Text[0].Centered = true;
            champ.Level.Text[0].Add();

            champ.Cs.Text[0] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.Cs.Text[0].TextUpdate = delegate
            {
                return champ.Cs.Value.ToString();
            };
            champ.Cs.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.Cs.CoordsSideBar.Width, champ.Cs.CoordsSideBar.Height);
            };
            champ.Cs.Text[0].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
            };
            champ.Cs.Text[0].OutLined = true;
            champ.Cs.Text[0].Centered = true;
            champ.Cs.Text[0].Add();

            champ.RecallBar.Text[0] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.RecallBar.Text[0].TextUpdate = delegate
            {
                RecallDetector.RecallInfo info = GetRecall(hero.NetworkId);
                if (info != null && info.StartTime != 0)
                {
                    float time = Game.Time + info.Recall.Duration / 1000 - info.StartTime;
                    if (time > 0.0f &&
                        (info.Recall.Status == Recall.RecallStatus.TeleportStart ||
                         info.Recall.Status == Recall.RecallStatus.RecallStarted))
                    {
                        return "Porting";
                    }
                    else if (time < 30.0f &&
                             (info.Recall.Status == Recall.RecallStatus.TeleportEnd ||
                              info.Recall.Status == Recall.RecallStatus.RecallFinished))
                    {
                        return "Ported";
                    }
                    else if (time < 30.0f &&
                             (info.Recall.Status == Recall.RecallStatus.TeleportAbort ||
                              info.Recall.Status == Recall.RecallStatus.RecallAborted))
                    {
                        return "Canceled";
                    }
                }
                return "";
            };
            champ.RecallBar.Text[0].PositionUpdate = delegate
            {
                return new Vector2(champ.RecallBar.CoordsSideBar.Width, champ.RecallBar.CoordsSideBar.Height);
            };
            champ.RecallBar.Text[0].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    Menu.RecallDetector.GetActive();
            };
            champ.RecallBar.Text[0].OutLined = true;
            champ.RecallBar.Text[0].Centered = true;
            champ.RecallBar.Text[0].Add();

            //champ.Champ.Text[0].TextFontDescription = new FontDescription()
            //{
            //    FaceName = "Calibri",
            //    Height = 24,
            //    OutputPrecision = FontPrecision.Default,
            //    Quality = FontQuality.Default
            //};

            return champ;
        }

        private async static Task<ChampInfos> CreateOverHeadHud(Obj_AI_Hero hero, ChampInfos champ, float percentScale)
        {
            float scaleSpell = GetHeadMode(hero.IsEnemy).SelectedIndex == 1 ? 1.7f : 1.0f;
            float scaleSum = GetHeadMode(hero.IsEnemy).SelectedIndex == 1 ? 1.0f : 0.8f;

            SpellDataInst[] s1 = hero.Spellbook.Spells;
            SpellDataInst[] s2 = hero.SummonerSpellbook.Spells;

            Task<SpriteHelper.SpriteInfo> taskInfo = null;
            taskInfo = SpriteHelper.LoadTextureAsync(s2[0].Name, champ.SpellSum1.Sprite[1], SpriteHelper.DownloadType.Summoner);
            champ.SpellSum1.Sprite[1] = await taskInfo;
            if (!champ.SpellSum1.Sprite[1].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSummonerSpellImage(hero, champ.SpellSum1.SizeHpBar, champ.SpellSum1.Sprite[1], SpellSlot.Summoner1, UpdateMethod.Hp));
            }
            else
            {
                SetScale(ref champ.SpellSum1.Sprite[1].Sprite, _sumSize, scaleSum * percentScale);
                champ.SpellSum1.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellSum1.SizeHpBar.Width, champ.SpellSum1.SizeHpBar.Height);
                };
                champ.SpellSum1.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 0 && hero.IsVisible && !hero.IsDead;
                };
                champ.SpellSum1.Sprite[1].Sprite.Add();
            }

            taskInfo = SpriteHelper.LoadTextureAsync(s2[1].Name, champ.SpellSum2.Sprite[1], SpriteHelper.DownloadType.Summoner);
            champ.SpellSum2.Sprite[1] = await taskInfo;
            if (!champ.SpellSum2.Sprite[1].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSummonerSpellImage(hero, champ.SpellSum2.SizeHpBar, champ.SpellSum2.Sprite[1], SpellSlot.Summoner2, UpdateMethod.Hp));
            }
            else
            {
                SetScale(ref champ.SpellSum2.Sprite[1].Sprite, _sumSize, scaleSum * percentScale);
                champ.SpellSum2.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellSum2.SizeHpBar.Width, champ.SpellSum2.SizeHpBar.Height);
                };
                champ.SpellSum2.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() &&
                           GetMode(hero.IsEnemy).SelectedIndex != 0 &&
                           hero.IsVisible && !hero.IsDead;
                };
                champ.SpellSum2.Sprite[1].Sprite.Add();
            }      

            //SpriteHelper.LoadTexture(s1[1].Name + ".dds", "PASSIVE/", loc + "PASSIVE\\" + s1[1].Name + ".dds", ref champ.Passive.Texture);
            taskInfo = SpriteHelper.LoadTextureAsync(s1[0].Name, champ.SpellQ.Sprite[1], SpriteHelper.DownloadType.Spell);
            champ.SpellQ.Sprite[1] = await taskInfo;
            if (!champ.SpellQ.Sprite[1].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, champ.SpellQ.SizeHpBar, champ.SpellQ.Sprite[1], SpellSlot.Q, UpdateMethod.Hp));
            }
            else
            {
                SetScale(ref champ.SpellQ.Sprite[1].Sprite, _spellSize, scaleSum * percentScale);
                champ.SpellQ.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellQ.SizeHpBar.Width, champ.SpellQ.SizeHpBar.Height);
                };
                champ.SpellQ.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() &&
                            GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 0 &&
                            hero.IsVisible && !hero.IsDead;
                };
                champ.SpellQ.Sprite[1].Sprite.Add();
            }

            taskInfo = SpriteHelper.LoadTextureAsync(s1[1].Name, champ.SpellW.Sprite[1], SpriteHelper.DownloadType.Spell);
            champ.SpellW.Sprite[1] = await taskInfo;
            if (!champ.SpellW.Sprite[1].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, champ.SpellW.SizeHpBar, champ.SpellW.Sprite[1], SpellSlot.W, UpdateMethod.Hp));
            }
            else
            {
                SetScale(ref champ.SpellW.Sprite[1].Sprite, _spellSize, scaleSum * percentScale);
                champ.SpellW.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellW.SizeHpBar.Width, champ.SpellW.SizeHpBar.Height);
                };
                champ.SpellW.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() &&
                            GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 0 &&
                            hero.IsVisible && !hero.IsDead;
                };
                champ.SpellW.Sprite[1].Sprite.Add();
            }

            taskInfo = SpriteHelper.LoadTextureAsync(s1[2].Name, champ.SpellE.Sprite[1], SpriteHelper.DownloadType.Spell);
            champ.SpellE.Sprite[1] = await taskInfo;
            if (!champ.SpellE.Sprite[1].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, champ.SpellE.SizeHpBar, champ.SpellE.Sprite[1], SpellSlot.E, UpdateMethod.Hp));
            }
            else
            {
                SetScale(ref champ.SpellE.Sprite[1].Sprite, _spellSize, scaleSum * percentScale);
                champ.SpellE.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellE.SizeHpBar.Width, champ.SpellE.SizeHpBar.Height);
                };
                champ.SpellE.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() &&
                            GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 0 &&
                            hero.IsVisible && !hero.IsDead;
                };
                champ.SpellE.Sprite[1].Sprite.Add();
            }

            taskInfo = SpriteHelper.LoadTextureAsync(s1[3].Name, champ.SpellR.Sprite[1], SpriteHelper.DownloadType.Spell);
            champ.SpellR.Sprite[1] = await taskInfo;
            if (!champ.SpellR.Sprite[1].LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, champ.SpellR.SizeHpBar, champ.SpellR.Sprite[1], SpellSlot.R, UpdateMethod.Hp));
            }
            else
            {
                SetScale(ref champ.SpellR.Sprite[1].Sprite, _spellSize, scaleSum * percentScale);
                champ.SpellR.Sprite[1].Sprite.PositionUpdate = delegate
                {
                    return new Vector2(champ.SpellR.SizeHpBar.Width, champ.SpellR.SizeHpBar.Height);
                };
                champ.SpellR.Sprite[1].Sprite.VisibleCondition = sender =>
                {
                    return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() &&
                            GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 0 &&
                            hero.IsVisible && !hero.IsDead;
                };
                champ.SpellR.Sprite[1].Sprite.Add();
            }                

            //////

            champ.SpellQ.Rectangle[0] = new Render.Rectangle(champ.SpellQ.SizeHpBar.Width, champ.SpellQ.SizeHpBar.Height,
                _spellSize.Width, _spellSize.Height, SharpDX.Color.Red);
            champ.SpellQ.Rectangle[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellQ.SizeHpBar.Width, champ.SpellQ.SizeHpBar.Height);
            };
            champ.SpellQ.Rectangle[0].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 1 &&
                        hero.IsVisible && !hero.IsDead;
            };
            champ.SpellQ.Rectangle[0].Add();
            champ.SpellW.Rectangle[0] = new Render.Rectangle(champ.SpellW.SizeHpBar.Width, champ.SpellW.SizeHpBar.Height,
                _spellSize.Width, _spellSize.Height, SharpDX.Color.Red);
            champ.SpellW.Rectangle[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellW.SizeHpBar.Width, champ.SpellW.SizeHpBar.Height);
            };
            champ.SpellW.Rectangle[0].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 1 &&
                        hero.IsVisible && !hero.IsDead;
            };
            champ.SpellW.Rectangle[0].Add();
            champ.SpellE.Rectangle[0] = new Render.Rectangle(champ.SpellE.SizeHpBar.Width, champ.SpellE.SizeHpBar.Height,
                _spellSize.Width, _spellSize.Height, SharpDX.Color.Red);
            champ.SpellE.Rectangle[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellE.SizeHpBar.Width, champ.SpellE.SizeHpBar.Height);
            };
            champ.SpellE.Rectangle[0].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 1 &&
                        hero.IsVisible && !hero.IsDead;
            };
            champ.SpellE.Rectangle[0].Add();
            champ.SpellR.Rectangle[0] = new Render.Rectangle(champ.SpellR.SizeHpBar.Width, champ.SpellR.SizeHpBar.Height,
                _spellSize.Width, _spellSize.Height, SharpDX.Color.Red);
            champ.SpellR.Rectangle[0].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellR.SizeHpBar.Width, champ.SpellR.SizeHpBar.Height);
            };
            champ.SpellR.Rectangle[0].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() &&
                        GetMode(hero.IsEnemy).SelectedIndex != 0 && GetHeadDisplayMode(hero.IsEnemy).SelectedIndex == 1 &&
                        hero.IsVisible && !hero.IsDead;
            };
            champ.SpellR.Rectangle[0].Add();

            ///////
             
            champ.SpellQ.Text[1] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellQ.Text[1].TextUpdate = delegate
            {
                return champ.SpellQ.Value.ToString();
            };
            champ.SpellQ.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellQ.CoordsHpBar.Width, champ.SpellQ.CoordsHpBar.Height);
            };
            champ.SpellQ.Text[1].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() &&
                    champ.SpellQ.Value > 0.0f && hero.IsVisible && !hero.IsDead;
            };
            champ.SpellQ.Text[1].OutLined = true;
            champ.SpellQ.Text[1].Centered = true;
            champ.SpellQ.Text[1].Add();

            champ.SpellW.Text[1] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellW.Text[1].TextUpdate = delegate
            {
                return champ.SpellW.Value.ToString();
            };
            champ.SpellW.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellW.CoordsHpBar.Width, champ.SpellW.CoordsHpBar.Height);
            };
            champ.SpellW.Text[1].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellW.Value > 0.0f && hero.IsVisible && !hero.IsDead;
            };
            champ.SpellW.Text[1].OutLined = true;
            champ.SpellW.Text[1].Centered = true;
            champ.SpellW.Text[1].Add();

            champ.SpellE.Text[1] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellE.Text[1].TextUpdate = delegate
            {
                return champ.SpellE.Value.ToString();
            };
            champ.SpellE.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellE.CoordsHpBar.Width, champ.SpellE.CoordsHpBar.Height);
            };
            champ.SpellE.Text[1].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellE.Value > 0.0f && hero.IsVisible && !hero.IsDead;
            };
            champ.SpellE.Text[1].OutLined = true;
            champ.SpellE.Text[1].Centered = true;
            champ.SpellE.Text[1].Add();

            champ.SpellR.Text[1] = new Render.Text(0, 0, "", 14, SharpDX.Color.Orange);
            champ.SpellR.Text[1].TextUpdate = delegate
            {
                return champ.SpellR.Value.ToString();
            };
            champ.SpellR.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellR.CoordsHpBar.Width, champ.SpellR.CoordsHpBar.Height);
            };
            champ.SpellR.Text[1].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellR.Value > 0.0f && hero.IsVisible && !hero.IsDead;
            };
            champ.SpellR.Text[1].OutLined = true;
            champ.SpellR.Text[1].Centered = true;
            champ.SpellR.Text[1].Add();

            champ.SpellSum1.Text[1] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.SpellSum1.Text[1].TextUpdate = delegate
            {
                return champ.SpellSum1.Value.ToString();
            };
            champ.SpellSum1.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellSum1.CoordsHpBar.Width, champ.SpellSum1.CoordsHpBar.Height);
            };
            champ.SpellSum1.Text[1].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellSum1.Value > 0.0f && hero.IsVisible && !hero.IsDead;
            };
            champ.SpellSum1.Text[1].OutLined = true;
            champ.SpellSum1.Text[1].Centered = true;
            champ.SpellSum1.Text[1].Add();

            champ.SpellSum2.Text[1] = new Render.Text(0, 0, "", 16, SharpDX.Color.Orange);
            champ.SpellSum2.Text[1].TextUpdate = delegate
            {
                return champ.SpellSum2.Value.ToString();
            };
            champ.SpellSum2.Text[1].PositionUpdate = delegate
            {
                return new Vector2(champ.SpellSum2.CoordsHpBar.Width, champ.SpellSum2.CoordsHpBar.Height);
            };
            champ.SpellSum2.Text[1].VisibleCondition = sender =>
            {
                return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1 &&
                    champ.SpellSum2.Value > 0.0f && hero.IsVisible && !hero.IsDead;
            };
            champ.SpellSum2.Text[1].OutLined = true;
            champ.SpellSum2.Text[1].Centered = true;
            champ.SpellSum2.Text[1].Add();

            return champ;
        }

        private static StringList GetMode(bool enemy)
        {
            if (enemy)
            {
                return Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerMode")
                        .GetValue<StringList>();
            }
            else
            {
                return Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerMode")
                        .GetValue<StringList>();
            }
        }

        private static StringList GetSideDisplayMode(bool enemy)
        {
            if (enemy)
            {
                return Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerSideDisplayMode")
                        .GetValue<StringList>();
            }
            else
            {
                return Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerSideDisplayMode")
                        .GetValue<StringList>();
            }
        }

        private static StringList GetHeadMode(bool enemy)
        {
            if (enemy)
            {
                return Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerHeadMode")
                        .GetValue<StringList>();
            }
            else
            {
                return Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerHeadMode")
                        .GetValue<StringList>();
            }
        }

        private static StringList GetHeadDisplayMode(bool enemy)
        {
            if (enemy)
            {
                return Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerHeadDisplayMode")
                        .GetValue<StringList>();
            }
            else
            {
                return Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerHeadDisplayMode")
                        .GetValue<StringList>();
            }
        }

        private void CalculateSizes(bool calcEenemy)
        {
            Dictionary<Obj_AI_Hero, ChampInfos> heroes;
            float percentScale;
            StringList mode;
            StringList modeHead;
            StringList modeDisplay;
            int count;
            int xOffset;
            int yOffset;
            int yOffsetAdd;
            if (calcEenemy)
            {
                heroes = _enemies;
                percentScale = (float) Menu.UiTracker.GetMenuItem("SAwarenessUITrackerScale").GetValue<Slider>().Value/
                               100;
                mode =
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerMode")
                        .GetValue<StringList>();
                modeHead =
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerHeadMode")
                        .GetValue<StringList>();
                modeDisplay =
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerSideDisplayMode")
                        .GetValue<StringList>();
                count = 0;
                xOffset =
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerXPos")
                        .GetValue<Slider>()
                        .Value;
                _oldEx = xOffset;
                yOffset =
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerYPos")
                        .GetValue<Slider>()
                        .Value;
                _oldEy = yOffset;
                yOffsetAdd = (int) (20*percentScale);
            }
            else
            {
                heroes = _allies;
                percentScale = (float) Menu.UiTracker.GetMenuItem("SAwarenessUITrackerScale").GetValue<Slider>().Value/
                               100;
                mode =
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerMode")
                        .GetValue<StringList>();
                modeHead =
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerHeadMode")
                        .GetValue<StringList>();
                modeDisplay =
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerSideDisplayMode")
                        .GetValue<StringList>();
                count = 0;
                xOffset =
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerXPos")
                        .GetValue<Slider>()
                        .Value;
                _oldAx = xOffset;
                yOffset =
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerYPos")
                        .GetValue<Slider>()
                        .Value;
                _oldAy = yOffset;
                yOffsetAdd = (int) (20*percentScale);
            }

            _hudSize = new Size();
            foreach (var hero in heroes)
            {
                float scaleSpell = GetHeadMode(hero.Key.IsEnemy).SelectedIndex == 1 ? 1.7f : 1.0f;
                float scaleSum = GetHeadMode(hero.Key.IsEnemy).SelectedIndex == 1 ? 1.0f : 0.8f;
                if (mode.SelectedIndex == 0 || mode.SelectedIndex == 2)
                {
                    if (modeDisplay.SelectedIndex == 0)
                    {
                        hero.Value.SpellPassive.SizeSideBar =
                        new Size(
                            xOffset - (int)(_champSize.Width * percentScale) - (int)(_sumSize.Width * percentScale) -
                            (int)(_spellSize.Width * percentScale),
                            yOffset - (int)(_spellSize.Height * percentScale) * (count * 4 - 0) -
                            count * (int)(_backBarSize.Height * percentScale) -
                            count * (int)(_spellSize.Height * percentScale) - yOffsetAdd);
                        hero.Value.SpellQ.SizeSideBar = new Size(hero.Value.SpellPassive.SizeSideBar.Width,
                            hero.Value.SpellPassive.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) * 1);
                        hero.Value.SpellW.SizeSideBar = new Size(hero.Value.SpellPassive.SizeSideBar.Width,
                            hero.Value.SpellPassive.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) * 2);
                        hero.Value.SpellE.SizeSideBar = new Size(hero.Value.SpellPassive.SizeSideBar.Width,
                            hero.Value.SpellPassive.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) * 3);
                        hero.Value.SpellR.SizeSideBar = new Size(hero.Value.SpellPassive.SizeSideBar.Width,
                            hero.Value.SpellPassive.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) * 4);

                        hero.Value.Champ.SizeSideBar =
                            new Size(
                                hero.Value.SpellPassive.SizeSideBar.Width + (int)(_spellSize.Width * percentScale),
                                hero.Value.SpellPassive.SizeSideBar.Height);
                        hero.Value.SpellSum1.SizeSideBar =
                            new Size(hero.Value.Champ.SizeSideBar.Width + (int)(_champSize.Width * percentScale),
                                hero.Value.SpellPassive.SizeSideBar.Height);
                        hero.Value.SpellSum2.SizeSideBar = new Size(hero.Value.SpellSum1.SizeSideBar.Width,
                            hero.Value.SpellPassive.SizeSideBar.Height + (int)(_sumSize.Height * percentScale));

                        if (hero.Value.Item[0] == null)
                            hero.Value.Item[0] = new ChampInfos.SpriteInfos();
                        hero.Value.Item[0].SizeSideBar = new Size(hero.Value.SpellR.SizeSideBar.Width,
                            hero.Value.SpellR.SizeSideBar.Height + (int)(_spellSize.Height * percentScale));
                        for (int i = 1; i < hero.Value.Item.Length; i++)
                        {
                            if (hero.Value.Item[i] == null)
                                hero.Value.Item[i] = new ChampInfos.SpriteInfos();
                            hero.Value.Item[i].SizeSideBar =
                                new Size(
                                    hero.Value.Item[0].SizeSideBar.Width + (int)(_spellSize.Width * percentScale) * i,
                                    hero.Value.Item[0].SizeSideBar.Height);
                        }

                        hero.Value.SpellSum1.CoordsSideBar =
                            new Size(hero.Value.SpellSum1.SizeSideBar.Width + (int)(_sumSize.Width * percentScale) / 2,
                                hero.Value.SpellSum1.SizeSideBar.Height + (int)(_sumSize.Height * percentScale) / 2);
                        hero.Value.SpellSum2.CoordsSideBar =
                            new Size(hero.Value.SpellSum2.SizeSideBar.Width + (int)(_sumSize.Width * percentScale) / 2,
                                hero.Value.SpellSum2.SizeSideBar.Height + (int)(_sumSize.Height * percentScale) / 2);
                        hero.Value.Champ.CoordsSideBar =
                            new Size(hero.Value.Champ.SizeSideBar.Width + (int)(_champSize.Width * percentScale) / 2,
                                hero.Value.Champ.SizeSideBar.Height + (int)(_champSize.Height * percentScale) / 2);
                        hero.Value.SpellPassive.CoordsSideBar =
                            new Size(
                                hero.Value.SpellPassive.SizeSideBar.Width + (int)(_spellSize.Width * percentScale) / 2,
                                hero.Value.SpellPassive.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) / 2);
                        hero.Value.SpellQ.CoordsSideBar =
                            new Size(hero.Value.SpellQ.SizeSideBar.Width + (int)(_spellSize.Width * percentScale) / 2,
                                hero.Value.SpellQ.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) / 2);
                        hero.Value.SpellW.CoordsSideBar =
                            new Size(hero.Value.SpellW.SizeSideBar.Width + (int)(_spellSize.Width * percentScale) / 2,
                                hero.Value.SpellW.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) / 2);
                        hero.Value.SpellE.CoordsSideBar =
                            new Size(hero.Value.SpellE.SizeSideBar.Width + (int)(_spellSize.Width * percentScale) / 2,
                                hero.Value.SpellE.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) / 2);
                        hero.Value.SpellR.CoordsSideBar =
                            new Size(hero.Value.SpellR.SizeSideBar.Width + (int)(_spellSize.Width * percentScale) / 2,
                                hero.Value.SpellR.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) / 2);

                        hero.Value.BackBar.SizeSideBar = new Size(hero.Value.Champ.SizeSideBar.Width,
                            hero.Value.SpellSum2.SizeSideBar.Height + (int)(_sumSize.Height * percentScale));
                        hero.Value.HealthBar.SizeSideBar = new Size(hero.Value.BackBar.SizeSideBar.Width,
                            hero.Value.BackBar.SizeSideBar.Height);
                        hero.Value.ManaBar.SizeSideBar = new Size(hero.Value.BackBar.SizeSideBar.Width,
                            hero.Value.BackBar.SizeSideBar.Height + (int)(_healthManaBarSize.Height * percentScale) + 3);
                        hero.Value.SHealth = ((int)hero.Key.Health) + "/" + ((int)hero.Key.MaxHealth);
                        hero.Value.SMana = ((int)hero.Key.Mana) + "/" + ((int)hero.Key.MaxMana);
                        hero.Value.HealthBar.CoordsSideBar =
                            new Size(
                                hero.Value.HealthBar.SizeSideBar.Width +
                                (int)(_healthManaBarSize.Width * percentScale) / 2,
                                hero.Value.HealthBar.SizeSideBar.Height -
                                (int)(_healthManaBarSize.Height * percentScale) / 4);
                        hero.Value.ManaBar.CoordsSideBar =
                            new Size(
                                hero.Value.ManaBar.SizeSideBar.Width + (int)(_healthManaBarSize.Width * percentScale) / 2,
                                hero.Value.ManaBar.SizeSideBar.Height -
                                (int)(_healthManaBarSize.Height * percentScale) / 4);

                        if (hero.Value.Item[0] == null)
                            hero.Value.Item[0] = new ChampInfos.SpriteInfos();
                        hero.Value.Item[0].CoordsSideBar = new Size(hero.Value.SpellR.CoordsSideBar.Width,
                            hero.Value.SpellR.CoordsSideBar.Height + (int)(_spellSize.Height * percentScale));
                        for (int i = 1; i < hero.Value.Item.Length; i++)
                        {
                            if (hero.Value.Item[i] == null)
                                hero.Value.Item[i] = new ChampInfos.SpriteInfos();
                            hero.Value.Item[i].CoordsSideBar =
                                new Size(
                                    hero.Value.Item[0].CoordsSideBar.Width + (int)(_spellSize.Width * percentScale) * i,
                                    hero.Value.Item[0].CoordsSideBar.Height);
                        }

                        hero.Value.RecallBar.SizeSideBar = new Size(hero.Value.Champ.SizeSideBar.Width,
                            hero.Value.BackBar.SizeSideBar.Height - (int)(_champSize.Height * percentScale) / 4);
                        hero.Value.RecallBar.CoordsSideBar =
                            new Size(hero.Value.RecallBar.SizeSideBar.Width + (int)(_recSize.Width * percentScale) / 2,
                                hero.Value.RecallBar.SizeSideBar.Height + (int)(_recSize.Height * percentScale) / 4);

                        hero.Value.Level.SizeSideBar = new Size(hero.Value.Champ.SizeSideBar.Width,
                            hero.Value.Champ.SizeSideBar.Height);
                        hero.Value.Level.CoordsSideBar =
                            new Size(hero.Value.Champ.SizeSideBar.Width + (int)(_champSize.Width * percentScale) / 8,
                                hero.Value.Champ.SizeSideBar.Height + (int)(_recSize.Height * percentScale) / 2);

                        hero.Value.Cs.SizeSideBar = new Size(hero.Value.Champ.SizeSideBar.Width,
                            hero.Value.Champ.SizeSideBar.Height);
                        hero.Value.Cs.CoordsSideBar =
                            new Size(hero.Value.Champ.SizeSideBar.Width + (int)((_champSize.Width * percentScale) / 1.2),
                                hero.Value.Champ.SizeSideBar.Height + (int)(_recSize.Height * percentScale) / 2);

                        hero.Value.Gold.SizeSideBar = new Size(hero.Value.Champ.SizeSideBar.Width,
                            hero.Value.Champ.SizeSideBar.Height);
                        hero.Value.Gold.CoordsSideBar =
                            new Size(hero.Value.Champ.SizeSideBar.Width + (int)(_champSize.Width * percentScale) / 2,
                                hero.Value.Champ.SizeSideBar.Height + (int)(_recSize.Height * percentScale) / 2);

                        yOffsetAdd += (int)(5 * percentScale);
                        Size nSize = (hero.Value.Item[hero.Value.Item.Length - 1].SizeSideBar) -
                                     (hero.Value.SpellPassive.SizeSideBar);
                        nSize.Height += (int)(8 * percentScale);
                        _hudSize += nSize;
                        _hudSize.Width = nSize.Width;
                        _hudSize.Width += _spellSize.Width;
                        _hudSize.Height += (int)(20 * percentScale);
                        count++;
                    }
                    else
                    {
                        //yOffsetAdd = (int) (20*percentScale);
                        hero.Value.Champ.SizeSideBar =
                            new Size(
                                xOffset - (int)(_champSize.Width * percentScale),
                                yOffset - count * (int)(_champSize.Height * percentScale) -
                            count * (int)(_backBarSize.Height * percentScale) - yOffsetAdd);
                        hero.Value.SpellSum1.SizeSideBar =
                            new Size(hero.Value.Champ.SizeSideBar.Width - (int)(_sumSize.Width * percentScale),
                                hero.Value.Champ.SizeSideBar.Height);
                        hero.Value.SpellSum2.SizeSideBar = new Size(hero.Value.SpellSum1.SizeSideBar.Width,
                            hero.Value.Champ.SizeSideBar.Height + (int)(_sumSize.Height * percentScale));
                        hero.Value.SpellR.SizeSideBar = new Size(xOffset - (int)(_sumSize.Width * percentScale),
                            hero.Value.Champ.SizeSideBar.Height);

                        hero.Value.SpellSum1.CoordsSideBar =
                            new Size(hero.Value.SpellSum1.SizeSideBar.Width + (int)(_sumSize.Width * percentScale) / 2,
                                hero.Value.SpellSum1.SizeSideBar.Height + (int)(_sumSize.Height * percentScale) / 2);
                        hero.Value.SpellSum2.CoordsSideBar =
                            new Size(hero.Value.SpellSum2.SizeSideBar.Width + (int)(_sumSize.Width * percentScale) / 2,
                                hero.Value.SpellSum2.SizeSideBar.Height + (int)(_sumSize.Height * percentScale) / 2);
                        hero.Value.Champ.CoordsSideBar =
                            new Size(hero.Value.Champ.SizeSideBar.Width + (int)(_champSize.Width * percentScale) / 2,
                                hero.Value.Champ.SizeSideBar.Height + (int)(_champSize.Height * percentScale) / 2);
                        hero.Value.SpellR.CoordsSideBar =
                            new Size(hero.Value.SpellR.SizeSideBar.Width + (int)(_spellSize.Width * percentScale),
                                hero.Value.SpellR.SizeSideBar.Height + (int)(_spellSize.Height * percentScale) / 2);

                        hero.Value.BackBar.SizeSideBar = new Size(hero.Value.SpellSum1.SizeSideBar.Width,
                            hero.Value.SpellSum2.SizeSideBar.Height + (int)(_sumSize.Height * percentScale));
                        hero.Value.HealthBar.SizeSideBar = new Size(hero.Value.BackBar.SizeSideBar.Width,
                            hero.Value.BackBar.SizeSideBar.Height);
                        hero.Value.ManaBar.SizeSideBar = new Size(hero.Value.BackBar.SizeSideBar.Width,
                            hero.Value.BackBar.SizeSideBar.Height + (int)(_healthManaBarSize.Height * percentScale) + 3);
                        hero.Value.SHealth = ((int)hero.Key.Health) + "/" + ((int)hero.Key.MaxHealth);
                        hero.Value.SMana = ((int)hero.Key.Mana) + "/" + ((int)hero.Key.MaxMana);
                        hero.Value.HealthBar.CoordsSideBar =
                            new Size(
                                hero.Value.HealthBar.SizeSideBar.Width +
                                (int)(_healthManaBarSize.Width * percentScale) / 2,
                                hero.Value.HealthBar.SizeSideBar.Height -
                                (int)(_healthManaBarSize.Height * percentScale) / 4);
                        hero.Value.ManaBar.CoordsSideBar =
                            new Size(
                                hero.Value.ManaBar.SizeSideBar.Width + (int)(_healthManaBarSize.Width * percentScale) / 2,
                                hero.Value.ManaBar.SizeSideBar.Height -
                                (int)(_healthManaBarSize.Height * percentScale) / 4);

                        //For champ click/move
                        hero.Value.SpellPassive.SizeSideBar = hero.Value.SpellSum1.SizeSideBar;

                        yOffsetAdd += (int)(5 * percentScale);
                        Size nSize = (hero.Value.ManaBar.SizeSideBar) -
                                     (hero.Value.SpellSum1.SizeSideBar);
                        nSize.Height += (int)(8 * percentScale);
                        _hudSize += nSize;
                        _hudSize.Width = nSize.Width;
                        _hudSize.Width += _spellSize.Width;
                        _hudSize.Height += (int)(20 * percentScale);
                        count++;
                    }
                }
                if (mode.SelectedIndex == 1 || mode.SelectedIndex == 2)
                {
                    if (modeHead.SelectedIndex == 0)
                    {
                        const float hpPosScale = 0.8f;
                        Vector2 hpPos = hero.Key.HPBarPosition;
                        hero.Value.SpellSum1.SizeHpBar = new Size((int) hpPos.X - 20, (int) hpPos.Y);
                        SetScale(ref hero.Value.SpellSum1.Sprite[1].Sprite, _sumSize, scaleSum * percentScale, true, hero.Value.SpellSum1.Sprite[1].Bitmap);
                        hero.Value.SpellSum2.SizeHpBar = new Size(hero.Value.SpellSum1.SizeHpBar.Width,
                            hero.Value.SpellSum1.SizeHpBar.Height + (int) (_sumSize.Height*hpPosScale));
                        SetScale(ref hero.Value.SpellSum2.Sprite[1].Sprite, _sumSize, scaleSum * percentScale, true, hero.Value.SpellSum2.Sprite[1].Bitmap);
                        hero.Value.SpellPassive.SizeHpBar =
                            new Size(hero.Value.SpellSum1.SizeHpBar.Width + _sumSize.Width,
                                hero.Value.SpellSum2.SizeHpBar.Height + (int) ((_spellSize.Height*hpPosScale)/1.5));
                        //SetScale(ref hero.Value.SpellPassive.Sprite[1].Sprite, _spellSize, scaleSum * percentScale);
                        //hero.Value.SpellPassive.Sprite[1].Sprite.Scale = new Vector2(((float)_spellSize.Width / hero.Value.SpellPassive.Sprite[1].Sprite.Width) * scaleSum * percentScale,
                        //    ((float)_spellSize.Height / hero.Value.SpellPassive.Sprite[1].Sprite.Height) * scaleSum * percentScale);
                        hero.Value.SpellQ.SizeHpBar =
                            new Size(hero.Value.SpellPassive.SizeHpBar.Width + _spellSize.Width,
                                hero.Value.SpellPassive.SizeHpBar.Height);
                        SetScale(ref hero.Value.SpellQ.Sprite[1].Sprite, _spellSize, scaleSpell * percentScale, true, hero.Value.SpellQ.Sprite[1].Bitmap);
                        hero.Value.SpellW.SizeHpBar =
                            new Size(hero.Value.SpellQ.SizeHpBar.Width + _spellSize.Width,
                                hero.Value.SpellQ.SizeHpBar.Height);
                        SetScale(ref hero.Value.SpellW.Sprite[1].Sprite, _spellSize, scaleSpell * percentScale, true, hero.Value.SpellW.Sprite[1].Bitmap);
                        hero.Value.SpellE.SizeHpBar =
                            new Size(hero.Value.SpellW.SizeHpBar.Width + _spellSize.Width,
                                hero.Value.SpellW.SizeHpBar.Height);
                        SetScale(ref hero.Value.SpellE.Sprite[1].Sprite, _spellSize, scaleSpell * percentScale, true, hero.Value.SpellE.Sprite[1].Bitmap);
                        hero.Value.SpellR.SizeHpBar =
                            new Size(hero.Value.SpellE.SizeHpBar.Width + _spellSize.Width,
                                hero.Value.SpellE.SizeHpBar.Height);
                        SetScale(ref hero.Value.SpellR.Sprite[1].Sprite, _spellSize, scaleSpell * percentScale, true, hero.Value.SpellR.Sprite[1].Bitmap);

                        hero.Value.SpellSum1.CoordsHpBar =
                            new Size(hero.Value.SpellSum1.SizeHpBar.Width + _sumSize.Width/2,
                                hero.Value.SpellSum1.SizeHpBar.Height + _sumSize.Height/2);
                        hero.Value.SpellSum2.CoordsHpBar =
                            new Size(hero.Value.SpellSum2.SizeHpBar.Width + _sumSize.Width/2,
                                hero.Value.SpellSum2.SizeHpBar.Height + _sumSize.Height/2);
                        hero.Value.SpellPassive.CoordsHpBar =
                            new Size(hero.Value.SpellPassive.SizeHpBar.Width + _spellSize.Width/2,
                                hero.Value.SpellPassive.SizeHpBar.Height + _spellSize.Height/2);
                        hero.Value.SpellQ.CoordsHpBar =
                            new Size(hero.Value.SpellQ.SizeHpBar.Width + _spellSize.Width/2,
                                hero.Value.SpellQ.SizeHpBar.Height + _spellSize.Height/2);
                        hero.Value.SpellW.CoordsHpBar =
                            new Size(hero.Value.SpellW.SizeHpBar.Width + _spellSize.Width/2,
                                hero.Value.SpellW.SizeHpBar.Height + _spellSize.Height/2);
                        hero.Value.SpellE.CoordsHpBar =
                            new Size(hero.Value.SpellE.SizeHpBar.Width + _spellSize.Width/2,
                                hero.Value.SpellE.SizeHpBar.Height + _spellSize.Height/2);
                        hero.Value.SpellR.CoordsHpBar =
                            new Size(hero.Value.SpellR.SizeHpBar.Width + _spellSize.Width/2,
                                hero.Value.SpellR.SizeHpBar.Height + _spellSize.Height/2);
                    }
                    else
                    {
                        const float hpPosScale = 1.7f;
                        Vector2 hpPos = hero.Key.HPBarPosition;
                        hero.Value.SpellSum1.SizeHpBar = new Size((int) hpPos.X - 25, (int) hpPos.Y + 2);
                        SetScale(ref hero.Value.SpellSum1.Sprite[1].Sprite, _sumSize, scaleSum * percentScale, true, hero.Value.SpellSum1.Sprite[1].Bitmap);
                        hero.Value.SpellSum2.SizeHpBar = new Size(hero.Value.SpellSum1.SizeHpBar.Width,
                            hero.Value.SpellSum1.SizeHpBar.Height + (int) (_sumSize.Height*1.0f));
                        SetScale(ref hero.Value.SpellSum2.Sprite[1].Sprite, _sumSize, scaleSum * percentScale, true, hero.Value.SpellSum2.Sprite[1].Bitmap);
                        hero.Value.SpellPassive.SizeHpBar =
                            new Size(hero.Value.SpellSum1.SizeHpBar.Width + (int) (_spellSize.Width*hpPosScale),
                                hero.Value.SpellSum2.SizeHpBar.Height);
                        //SetScale(ref hero.Value.SpellPassive.Sprite[1].Sprite, _spellSize, scaleSum * percentScale);
                        //hero.Value.SpellPassive.Sprite[1].Sprite.Scale = new Vector2(((float)_spellSize.Width / hero.Value.SpellPassive.Sprite[1].Sprite.Width) * scaleSum * percentScale,
                        //    ((float)_spellSize.Height / hero.Value.SpellPassive.Sprite[1].Sprite.Height) * scaleSum * percentScale);
                        hero.Value.SpellQ.SizeHpBar =
                            new Size(
                                hero.Value.SpellPassive.SizeHpBar.Width + (int) (_spellSize.Width*hpPosScale),
                                hero.Value.SpellPassive.SizeHpBar.Height);
                        SetScale(ref hero.Value.SpellQ.Sprite[1].Sprite, _spellSize, scaleSpell * percentScale, true, hero.Value.SpellQ.Sprite[1].Bitmap);
                        hero.Value.SpellW.SizeHpBar =
                            new Size(hero.Value.SpellQ.SizeHpBar.Width + (int) (_spellSize.Width*hpPosScale),
                                hero.Value.SpellQ.SizeHpBar.Height);
                        SetScale(ref hero.Value.SpellW.Sprite[1].Sprite, _spellSize, scaleSpell * percentScale, true, hero.Value.SpellW.Sprite[1].Bitmap);
                        hero.Value.SpellE.SizeHpBar =
                            new Size(hero.Value.SpellW.SizeHpBar.Width + (int) (_spellSize.Width*hpPosScale),
                                hero.Value.SpellW.SizeHpBar.Height);
                        SetScale(ref hero.Value.SpellE.Sprite[1].Sprite, _spellSize, scaleSpell * percentScale, true, hero.Value.SpellE.Sprite[1].Bitmap);
                        hero.Value.SpellR.SizeHpBar =
                            new Size(hero.Value.SpellE.SizeHpBar.Width + (int) (_spellSize.Width*hpPosScale),
                                hero.Value.SpellE.SizeHpBar.Height);
                        SetScale(ref hero.Value.SpellR.Sprite[1].Sprite, _spellSize, scaleSpell * percentScale, true, hero.Value.SpellR.Sprite[1].Bitmap);

                        hero.Value.SpellSum1.CoordsHpBar =
                            new Size(hero.Value.SpellSum1.SizeHpBar.Width + _sumSize.Width/2,
                                hero.Value.SpellSum1.SizeHpBar.Height + _sumSize.Height/8);
                        hero.Value.SpellSum2.CoordsHpBar =
                            new Size(hero.Value.SpellSum2.SizeHpBar.Width + _sumSize.Width/2,
                                hero.Value.SpellSum2.SizeHpBar.Height + _sumSize.Height/8);
                        hero.Value.SpellPassive.CoordsHpBar =
                            new Size(hero.Value.SpellPassive.SizeHpBar.Width + (int) (_spellSize.Width/1.7),
                                hero.Value.SpellPassive.SizeHpBar.Height + _spellSize.Height/2);
                        hero.Value.SpellQ.CoordsHpBar =
                            new Size(hero.Value.SpellQ.SizeHpBar.Width + (int) (_spellSize.Width/1.3),
                                hero.Value.SpellQ.SizeHpBar.Height + _spellSize.Height/2);
                        hero.Value.SpellW.CoordsHpBar =
                            new Size(hero.Value.SpellW.SizeHpBar.Width + (int) (_spellSize.Width/1.3),
                                hero.Value.SpellW.SizeHpBar.Height + _spellSize.Height/2);
                        hero.Value.SpellE.CoordsHpBar =
                            new Size(hero.Value.SpellE.SizeHpBar.Width + (int) (_spellSize.Width/1.3),
                                hero.Value.SpellE.SizeHpBar.Height + _spellSize.Height/2);
                        hero.Value.SpellR.CoordsHpBar =
                            new Size(hero.Value.SpellR.SizeHpBar.Width + (int) (_spellSize.Width/1.3),
                                hero.Value.SpellR.SizeHpBar.Height + _spellSize.Height/2);
                    }
                }
            }
        }

        private static void SetScale(ref Render.Sprite sprite, Size size, float scale, bool needUpdate = false, Bitmap bitmap = null)
        {
            if (sprite != null && size.Width != 0 && size.Height != 0 && scale.CompareTo(0.0f) != 0)
            {
                var nScale = new Vector2(
                                ((float)size.Width / sprite.Width) * scale,
                                ((float)size.Height / sprite.Height) * scale);
                if (sprite.Scale.X.CompareTo(nScale.X) == 0 && sprite.Scale.Y.CompareTo(nScale.X) == 0)
                    return;
                if (needUpdate && bitmap != null)
                {
                    nScale = new Vector2(
                                ((float)size.Width / bitmap.Width) * scale,
                                ((float)size.Height / bitmap.Height) * scale);
                    if (sprite.Scale.X.CompareTo(nScale.X) == 0 && sprite.Scale.Y.CompareTo(nScale.X) == 0)
                        return;
                    sprite.UpdateTextureBitmap((Bitmap) bitmap.Clone());
                }
                //sprite.Reset();
                sprite.Scale = nScale;
            }
        }

        private static void SetScaleX(ref Render.Sprite sprite, Size size, float scale)
        {
            if (sprite != null && size.Width != 0 && size.Height != 0 && scale.CompareTo(0.0f) != 0)
            {
                var nScale = new Vector2(
                                ((float)size.Width / sprite.Width) * scale,
                                1);
                if (sprite.Scale.X.CompareTo(nScale.X) == 0)
                    return;
                //sprite.Reset();
                sprite.Scale = nScale;
            }
        }

        public enum UpdateMethod
        {
            Side,
            Hp,
            MiniMap
        }

        public async static void UpdateChampImage(Obj_AI_Hero hero, Size size, SpriteHelper.SpriteInfo sprite, UpdateMethod method)
        {
            Task<SpriteHelper.SpriteInfo> taskInfo = null;
            taskInfo = SpriteHelper.LoadTextureAsync(hero.ChampionName, sprite, SpriteHelper.DownloadType.Champion);
            sprite = await taskInfo;
            if (sprite.LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateChampImage(hero, size, sprite, method));
            }
            else
            {
                float percentScale =
                    (float)Menu.UiTracker.GetMenuItem("SAwarenessUITrackerScale").GetValue<Slider>().Value / 100;
                if (method == UpdateMethod.Side)
                {
                    SetScale(ref sprite.Sprite, _champSize, percentScale);
                    sprite.Sprite.PositionUpdate = delegate
                    {
                        return new Vector2(size.Width, size.Height);
                    };
                    sprite.Sprite.VisibleCondition = delegate
                    {
                        return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                    };
                }
                else if (method == UpdateMethod.MiniMap)
                {
                    if (sprite.Sprite.Bitmap != null)
                        sprite.Sprite.UpdateTextureBitmap(UimTracker.CropImage(sprite.Sprite.Bitmap, sprite.Sprite.Width));
                    sprite.Sprite.GrayScale();
                    SetScale(ref sprite.Sprite, size, percentScale);
                    sprite.Sprite.PositionUpdate = delegate
                    {
                        Vector2 serverPos = Drawing.WorldToMinimap(hero.ServerPosition);
                        var mPos = new Size((int)(serverPos[0] - 32 * 0.3f), (int)(serverPos[1] - 32 * 0.3f));
                        return new Vector2(mPos.Width, mPos.Height);
                    };
                    sprite.Sprite.VisibleCondition = delegate
                    {
                        return Menu.Tracker.GetActive() && Menu.UimTracker.GetActive() && !hero.IsVisible;
                    };
                }
                sprite.Sprite.Add();
            }
        }

        private async static void UpdateSpellImage(Obj_AI_Hero hero, Size size, SpriteHelper.SpriteInfo sprite, SpellSlot slot, UpdateMethod method)
        {
            SpellDataInst[] s1 = hero.Spellbook.Spells;
            Task<SpriteHelper.SpriteInfo> taskInfo = null;
            switch (slot)
            {
                case SpellSlot.Q:
                    taskInfo = SpriteHelper.LoadTextureAsync(s1[0].Name, sprite, SpriteHelper.DownloadType.Spell);
                break;

                case SpellSlot.W:
                    taskInfo = SpriteHelper.LoadTextureAsync(s1[1].Name, sprite, SpriteHelper.DownloadType.Spell);
                break;

                case SpellSlot.E:
                    taskInfo = SpriteHelper.LoadTextureAsync(s1[2].Name, sprite, SpriteHelper.DownloadType.Spell);
                break;

                case SpellSlot.R:
                    taskInfo = SpriteHelper.LoadTextureAsync(s1[3].Name, sprite, SpriteHelper.DownloadType.Spell);
                break;
            }
            if (taskInfo == null)
                return;
            sprite = await taskInfo;
            if (sprite.LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSpellImage(hero, size, sprite, slot, method));
            }
            else
            {
                float percentScale =
                    (float)Menu.UiTracker.GetMenuItem("SAwarenessUITrackerScale").GetValue<Slider>().Value / 100;
                SetScale(ref sprite.Sprite, _champSize, percentScale);
                if (method == UpdateMethod.Side)
                {
                    sprite.Sprite.PositionUpdate = delegate
                    {
                        return new Vector2(size.Width, size.Height);
                    };
                    sprite.Sprite.VisibleCondition = delegate
                    {
                        return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                    };
                }
                else if (method == UpdateMethod.Hp)
                {
                    sprite.Sprite.PositionUpdate = delegate
                    {
                        return new Vector2(size.Width, size.Height);
                    };
                    sprite.Sprite.VisibleCondition = delegate
                    {
                        return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                    };
                }
                else if (method == UpdateMethod.MiniMap)
                {
                    sprite.Sprite.PositionUpdate = delegate
                    {
                        return new Vector2(size.Width, size.Height);
                    };
                    sprite.Sprite.VisibleCondition = delegate
                    {
                        return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                    };
                }
                sprite.Sprite.Add();
            }
        }

        private async static void UpdateSummonerSpellImage(Obj_AI_Hero hero, Size size, SpriteHelper.SpriteInfo sprite, SpellSlot slot, UpdateMethod method)
        {
            SpellDataInst[] s2 = hero.SummonerSpellbook.Spells;
            Task<SpriteHelper.SpriteInfo> taskInfo = null;
            switch (slot)
            {
                case SpellSlot.Summoner1:
                    taskInfo = SpriteHelper.LoadTextureAsync(s2[0].Name, sprite, SpriteHelper.DownloadType.Summoner);
                    break;

                case SpellSlot.Summoner2:
                    taskInfo = SpriteHelper.LoadTextureAsync(s2[1].Name, sprite, SpriteHelper.DownloadType.Summoner);
                    break;
            }
            if (taskInfo == null)
                return;
            sprite = await taskInfo;
            if (sprite.LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UpdateSummonerSpellImage(hero, size, sprite, slot, method));
            }
            else
            {
                float percentScale =
                    (float)Menu.UiTracker.GetMenuItem("SAwarenessUITrackerScale").GetValue<Slider>().Value / 100;
                SetScale(ref sprite.Sprite, _champSize, percentScale);
                sprite.Sprite.PositionUpdate = delegate
                {
                    return new Vector2(size.Width, size.Height);
                };
                sprite.Sprite.VisibleCondition = sender =>
                {
                    return Menu.Tracker.GetActive() && Menu.UiTracker.GetActive() && GetMode(hero.IsEnemy).SelectedIndex != 1;
                };
                sprite.Sprite.Add();
            }
        }

        private void UpdateItems(bool enemy)
        {
            //if (!Menu.UiTracker.GetMenuItem("SAwarenessItemPanelActive").GetValue<bool>())
            //    return;
            ////var loc = Assembly.GetExecutingAssembly().Location;
            ////loc = loc.Remove(loc.LastIndexOf("\\", StringComparison.Ordinal));
            ////loc = loc + "\\Sprites\\SAwareness\\";

            //Dictionary<Obj_AI_Hero, ChampInfos> heroes;

            //if (enemy)
            //{
            //    heroes = _enemies;
            //}
            //else
            //{
            //    heroes = _allies;
            //}

            //foreach (var hero in heroes)
            //{
            //    InventorySlot[] i1 = hero.Key.InventoryItems;
            //    ChampInfos champ = hero.Value;
            //    var slot = new List<int>();
            //    var unusedId = new List<int> {0, 1, 2, 3, 4, 5, 6};
            //    foreach (InventorySlot inventorySlot in i1)
            //    {
            //        slot.Add(inventorySlot.Slot);
            //        if (inventorySlot.Slot >= 0 && inventorySlot.Slot <= 6)
            //        {
            //            unusedId.Remove(inventorySlot.Slot);
            //            if (champ.Item[inventorySlot.Slot] == null)
            //                champ.Item[inventorySlot.Slot] = new ChampInfos.SpriteInfos();
            //            if (champ.Item[inventorySlot.Slot].Sprite == null ||
            //                champ.ItemId[inventorySlot.Slot] != inventorySlot.Id)
            //            {
            //                //SpriteHelper.LoadTexture(inventorySlot.Id + ".dds", "ITEMS/",
            //                //    loc + "ITEMS\\" + inventorySlot.Id + ".dds",
            //                //    ref champ.Item[inventorySlot.Slot].Texture, true);
            //                SpriteHelper.LoadTexture(inventorySlot.Id.ToString(),
            //                    ref champ.Item[inventorySlot.Slot].Sprite[0].Sprite, SpriteHelper.DownloadType.Item);
            //                if (champ.Item[inventorySlot.Slot].Sprite != null)
            //                    champ.ItemId[inventorySlot.Slot] = inventorySlot.Id;
            //            }
            //        }
            //    }

            //    for (int i = 0; i < unusedId.Count; i++)
            //    {
            //        int id = unusedId[i];
            //        champ.ItemId[id] = 0;
            //        if (champ.Item[id] == null)
            //            champ.Item[id] = new ChampInfos.SpriteInfos();
            //        champ.Item[id].Sprite = null;
            //        if ( /*id == i*/champ.Item[id].Sprite == null &&
            //                        champ.Item[id].Sprite[0].Sprite != _overlayEmptyItem)
            //        {
            //            champ.Item[id].Sprite[0].Sprite = _overlayEmptyItem;
            //        }
            //    }
            //}
        }

        private void UpdateCds(Dictionary<Obj_AI_Hero, ChampInfos> heroes)
        {
            try
            {
                UpdateItems(true);
                UpdateItems(false);

                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                    foreach (var enemy in heroes)
                    {
                        if (enemy.Key == null)
                            continue;
                        enemy.Value.SHealth = ((int) enemy.Key.Health) + "/" + ((int) enemy.Key.MaxHealth);
                        enemy.Value.SMana = ((int) enemy.Key.Mana) + "/" + ((int) enemy.Key.MaxMana);
                        if (enemy.Key.NetworkId == hero.NetworkId)
                        {
                            foreach (var spell in hero.Spellbook.Spells)
                            {
                                if (spell.Slot == SpellSlot.Item1 && enemy.Value.Item[0] != null)
                                {
                                    if (spell.CooldownExpires - Game.Time > 0.0f)
                                    {
                                        enemy.Value.Item[0].Value = (int)(spell.CooldownExpires - Game.Time);
                                    }
                                    else if (spell.CooldownExpires - Game.Time <= 0.0f && enemy.Value.Item[0].Value != 0)
                                    {
                                        enemy.Value.Item[0].Value = 0;
                                    }
                                }
                                else if (spell.Slot == SpellSlot.Item2 && enemy.Value.Item[1] != null)
                                {
                                    if (spell.CooldownExpires - Game.Time > 0.0f)
                                    {
                                        enemy.Value.Item[1].Value = (int)(spell.CooldownExpires - Game.Time);
                                    }
                                    else if (spell.CooldownExpires - Game.Time <= 0.0f && enemy.Value.Item[1].Value != 0)
                                    {
                                        enemy.Value.Item[1].Value = 0;
                                    }
                                }
                                else if (spell.Slot == SpellSlot.Item3 && enemy.Value.Item[2] != null)
                                {
                                    if (spell.CooldownExpires - Game.Time > 0.0f)
                                    {
                                        enemy.Value.Item[2].Value = (int)(spell.CooldownExpires - Game.Time);
                                    }
                                    else if (spell.CooldownExpires - Game.Time <= 0.0f && enemy.Value.Item[2].Value != 0)
                                    {
                                        enemy.Value.Item[2].Value = 0;
                                    }
                                }
                                else if (spell.Slot == SpellSlot.Item4 && enemy.Value.Item[3] != null)
                                {
                                    if (spell.CooldownExpires - Game.Time > 0.0f)
                                    {
                                        enemy.Value.Item[3].Value = (int)(spell.CooldownExpires - Game.Time);
                                    }
                                    else if (spell.CooldownExpires - Game.Time <= 0.0f && enemy.Value.Item[3].Value != 0)
                                    {
                                        enemy.Value.Item[3].Value = 0;
                                    }
                                }
                                else if (spell.Slot == SpellSlot.Item5 && enemy.Value.Item[4] != null)
                                {
                                    if (spell.CooldownExpires - Game.Time > 0.0f)
                                    {
                                        enemy.Value.Item[4].Value = (int)(spell.CooldownExpires - Game.Time);
                                    }
                                    else if (spell.CooldownExpires - Game.Time <= 0.0f && enemy.Value.Item[4].Value != 0)
                                    {
                                        enemy.Value.Item[4].Value = 0;
                                    }
                                }
                                else if (spell.Slot == SpellSlot.Item6 && enemy.Value.Item[5] != null)
                                {
                                    if (spell.CooldownExpires - Game.Time > 0.0f)
                                    {
                                        enemy.Value.Item[5].Value = (int)(spell.CooldownExpires - Game.Time);
                                    }
                                    else if (spell.CooldownExpires - Game.Time <= 0.0f && enemy.Value.Item[5].Value != 0)
                                    {
                                        enemy.Value.Item[5].Value = 0;
                                    }
                                }
                                else if (spell.Slot == SpellSlot.Trinket && enemy.Value.Item[6] != null)
                                {
                                    if (spell.CooldownExpires - Game.Time > 0.0f)
                                    {
                                        enemy.Value.Item[6].Value = (int)(spell.CooldownExpires - Game.Time);
                                    }
                                    else if (spell.CooldownExpires - Game.Time <= 0.0f && enemy.Value.Item[6].Value != 0)
                                    {
                                        enemy.Value.Item[6].Value = 0;
                                    }
                                }
                            }

                            SpellDataInst[] s1 = hero.Spellbook.Spells;
                            if (s1[0].CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SpellQ.Value = (int)(s1[0].CooldownExpires - Game.Time);
                            }
                            else if (s1[0].CooldownExpires - Game.Time <= 0.0f && enemy.Value.SpellQ.Value != 0)
                            {
                                enemy.Value.SpellQ.Value = 0;
                            }
                            if (s1[1].CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SpellW.Value = (int)(s1[1].CooldownExpires - Game.Time);
                            }
                            else if (s1[1].CooldownExpires - Game.Time <= 0.0f && enemy.Value.SpellW.Value != 0)
                            {
                                enemy.Value.SpellW.Value = 0;
                            }
                            if (s1[2].CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SpellE.Value = (int)(s1[2].CooldownExpires - Game.Time);
                            }
                            else if (s1[2].CooldownExpires - Game.Time <= 0.0f && enemy.Value.SpellE.Value != 0)
                            {
                                enemy.Value.SpellE.Value = 0;
                            }
                            if (s1[3].CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SpellR.Value = (int)(s1[3].CooldownExpires - Game.Time);
                            }
                            else if (s1[3].CooldownExpires - Game.Time <= 0.0f && enemy.Value.SpellR.Value != 0)
                            {
                                enemy.Value.SpellR.Value = 0;
                            }
                            SpellDataInst[] s2 = hero.SummonerSpellbook.Spells;
                            if (s2[0].CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SpellSum1.Value = (int)(s2[0].CooldownExpires - Game.Time);
                            }
                            else if (s2[0].CooldownExpires - Game.Time <= 0.0f && enemy.Value.SpellSum1.Value != 0)
                            {
                                enemy.Value.SpellSum1.Value = 0;
                            }
                            if (s2[1].CooldownExpires - Game.Time > 0.0f)
                            {
                                enemy.Value.SpellSum2.Value = (int)(s2[1].CooldownExpires - Game.Time);
                            }
                            else if (s2[1].CooldownExpires - Game.Time <= 0.0f && enemy.Value.SpellSum2.Value != 0)
                            {
                                enemy.Value.SpellSum2.Value = 0;
                            }
                            if (hero.IsVisible)
                            {
                                enemy.Value.InvisibleTime = 0;
                                enemy.Value.VisibleTime = (int)Game.Time;
                            }
                            else
                            {
                                if (enemy.Value.VisibleTime != 0)
                                {
                                    enemy.Value.InvisibleTime = (int)(Game.Time - enemy.Value.VisibleTime);
                                }
                                else
                                {
                                    enemy.Value.InvisibleTime = 0;
                                }
                            }

                            //Death
                            if (hero.IsDead && !enemy.Value.Dead)
                            {
                                enemy.Value.Dead = true;
                                float temp = enemy.Key.Level * 2.5f + 5 + 2;
                                if (Math.Floor(Game.Time / 60) >= 25)
                                {
                                    enemy.Value.DeathTime = (int)(temp + ((temp / 50) * (Math.Floor(Game.Time / 60) - 25))) + (int)Game.Time;
                                }
                                else
                                {
                                    enemy.Value.DeathTime = (int)temp + (int)Game.Time;
                                }
                                if (enemy.Key.ChampionName.Contains("KogMaw"))
                                {
                                    enemy.Value.DeathTime -= 4;
                                }
                            }
                            else if (!hero.IsDead && enemy.Value.Dead)
                            {
                                enemy.Value.Dead = false;
                                enemy.Value.DeathTime = 0;
                            }
                            if (enemy.Value.DeathTime - Game.Time > 0.0f)
                            {
                                enemy.Value.DeathTimeDisplay = (int)(enemy.Value.DeathTime - Game.Time);
                            }
                            else if (enemy.Value.DeathTime - Game.Time <= 0.0f &&
                                     enemy.Value.DeathTimeDisplay != 0)
                            {
                                enemy.Value.DeathTimeDisplay = 0;
                            }
                            enemy.Value.Gold.Value = (int)enemy.Key.GoldEarned;//TODO: enable to get gold
                            enemy.Value.Cs.Value = enemy.Key.MinionsKilled;
                            enemy.Value.Level.Value = enemy.Key.Level;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UITrackerUpdate: " + ex);
                throw;
            }
        }

        void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == 0xC1 || args.PacketData[0] == 0xC2)
            {
                new System.Threading.Thread(() =>
                {
                    GetGold();
                }).Start();
            }
        }

        private void GetGold()
        {
            List<Spectator.Packet> packets = new List<Spectator.Packet>();
            if(SpecUtils.GameId == null)
                return;
            List<Byte[]> fullGameBytes = SpectatorDownloader.DownloadGameFiles(SpecUtils.GameId, SpecUtils.PlatformId, SpecUtils.Key, "KeyFrame");
            foreach (Byte[] chunkBytes in fullGameBytes)
            {
                packets.AddRange(SpectatorDecoder.DecodeBytes(chunkBytes));
            }
            foreach (Spectator.Packet p in packets)
            {
                if (p.header == (Byte)Spectator.HeaderId.PlayerStats)
                {
                    Spectator.PlayerStats playerStats = new Spectator.PlayerStats(p);
                    if (playerStats.GoldEarned <= 0.0f)
                        continue;
                    foreach (var ally in _allies)
                    {
                        if (ally.Key.NetworkId == playerStats.NetId)
                        {
                            //ally.Value.Gold = playerStats.GoldEarned;
                        }
                    }
                    foreach (var enemy in _enemies)
                    {
                        if (enemy.Key.NetworkId == playerStats.NetId)
                        {
                            //enemy.Value.Gold = playerStats.GoldEarned;
                        }
                    }
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;

            UpdateCds(_enemies);
            UpdateCds(_allies);
            CalculateSizes(true);
            CalculateSizes(false);
            UpdateOHRec(_allies);
            UpdateOHRec(_enemies);
        }

        private void UpdateOHRec(Dictionary<Obj_AI_Hero, ChampInfos> heroes)
        {
            foreach (var champ in heroes)
            {
                if (champ.Value.SpellQ.Rectangle[0] != null && 
                    champ.Value.SpellQ.Sprite[0] != null && champ.Value.SpellQ.Sprite[0].Sprite != null &&
                    champ.Value.SpellQ.Sprite[1] != null && champ.Value.SpellQ.Sprite[1].Sprite != null)
                {
                    if (champ.Value.SpellQ.Value > 0.0f ||
                        champ.Key.Spellbook.GetSpell(SpellSlot.Q).Level < 1)
                    {
                        //champ.Value.SpellQ.Sprite[0].Sprite.GrayScale();
                        //champ.Value.SpellQ.Sprite[1].Sprite.GrayScale();
                        champ.Value.SpellQ.Rectangle[0].Color = SharpDX.Color.Red;
                    }
                    else
                    {
                        //champ.Value.SpellQ.Sprite[0].Sprite.Complement();
                        //champ.Value.SpellQ.Sprite[1].Sprite.Complement();
                        champ.Value.SpellQ.Rectangle[0].Color = SharpDX.Color.Green;
                    }
                }
                if (champ.Value.SpellW.Rectangle[0] != null &&
                    champ.Value.SpellW.Sprite[0] != null && champ.Value.SpellW.Sprite[0].Sprite != null &&
                    champ.Value.SpellW.Sprite[1] != null && champ.Value.SpellW.Sprite[1].Sprite != null)
                {
                    if (champ.Value.SpellW.Value > 0.0f ||
                        champ.Key.Spellbook.GetSpell(SpellSlot.W).Level < 1)
                    {
                        //champ.Value.SpellW.Sprite[0].Sprite.GrayScale();
                        //champ.Value.SpellW.Sprite[1].Sprite.GrayScale();
                        champ.Value.SpellW.Rectangle[0].Color = SharpDX.Color.Red;
                    }
                    else
                    {
                        //champ.Value.SpellW.Sprite[0].Sprite.SetSaturation(1.0f);
                        //champ.Value.SpellW.Sprite[1].Sprite.SetSaturation(1.0f);
                        champ.Value.SpellW.Rectangle[0].Color = SharpDX.Color.Green;
                    }
                }
                if (champ.Value.SpellE.Rectangle[0] != null &&
                    champ.Value.SpellE.Sprite[0] != null && champ.Value.SpellE.Sprite[0].Sprite != null &&
                    champ.Value.SpellE.Sprite[1] != null && champ.Value.SpellE.Sprite[1].Sprite != null)
                {
                    if (champ.Value.SpellE.Value > 0.0f ||
                        champ.Key.Spellbook.GetSpell(SpellSlot.E).Level < 1)
                    {
                        //champ.Value.SpellE.Sprite[0].Sprite.GrayScale();
                        //champ.Value.SpellE.Sprite[1].Sprite.GrayScale();
                        champ.Value.SpellE.Rectangle[0].Color = SharpDX.Color.Red;
                    }
                    else
                    {
                        //champ.Value.SpellE.Sprite[0].Sprite.SetSaturation(1.0f);
                        //champ.Value.SpellE.Sprite[1].Sprite.SetSaturation(1.0f);
                        champ.Value.SpellE.Rectangle[0].Color = SharpDX.Color.Green;
                    }
                }
                if (champ.Value.SpellR.Rectangle[0] != null &&
                    champ.Value.SpellR.Sprite[0] != null && champ.Value.SpellR.Sprite[0].Sprite != null &&
                    champ.Value.SpellR.Sprite[1] != null && champ.Value.SpellR.Sprite[1].Sprite != null)
                {
                    if (champ.Value.SpellR.Value > 0.0f ||
                        champ.Key.Spellbook.GetSpell(SpellSlot.R).Level < 1)
                    {
                        //champ.Value.SpellR.Sprite[0].Sprite.GrayScale();
                        //champ.Value.SpellR.Sprite[1].Sprite.GrayScale();
                        champ.Value.SpellR.Rectangle[0].Color = SharpDX.Color.Red;
                    }
                    else
                    {
                        //champ.Value.SpellR.Sprite[0].Sprite.SetSaturation(1.0f);
                        //champ.Value.SpellR.Sprite[1].Sprite.SetSaturation(1.0f);
                        champ.Value.SpellR.Rectangle[0].Color = SharpDX.Color.Green;
                    }
                }
            }
        }

        private static float CalcHpBar(Obj_AI_Hero hero)
        {
            float percent = (100/hero.MaxHealth*hero.Health);
            return (percent <= 0 || Single.IsNaN(percent) ? 0.1f : percent / 100);
        }

        private static float CalcManaBar(Obj_AI_Hero hero)
        {
            float percent = (100/hero.MaxMana*hero.Mana);
            return (percent <= 0 || Single.IsNaN(percent) ? 0.1f : percent/100);
        }

        private static float CalcRecallBar(RecallDetector.RecallInfo recall)
        {
            if (recall == null)
                return 0.1f;
            float maxTime = (recall.Recall.Duration/1000);
            float percent = (100/maxTime*(Game.Time - recall.StartTime));
            return (percent <= 100 ? percent/100 : 1f);
        }

        private System.Drawing.Font CalcFont(int size, float scale)
        {
            double calcSize = (int) (size*scale);
            var newSize = (int) Math.Ceiling(calcSize);
            if (newSize%2 == 0 && newSize != 0)
                return new System.Drawing.Font("Times New Roman", (int) (size*scale));
            return null;
        }

        private void CheckValidSprite(ref Sprite sprite)
        {
            if (sprite.Device != Drawing.Direct3DDevice)
            {
                sprite = new Sprite(Drawing.Direct3DDevice);
            }
        }

        private void CheckValidFont(ref Font font)
        {
            if (font.Device != Drawing.Direct3DDevice)
            {
                AssingFonts(_scalePc, true);
            }
        }

        private void AssingFonts(float percentScale, bool force = false)
        {
            //System.Drawing.Font font = CalcFont(12, percentScale);
            //if (font != null || force)
            //    _recF = new Font(Drawing.Direct3DDevice, font);
            //font = CalcFont(8, percentScale);
            //if (font != null || force)
            //    _spellF = new Font(Drawing.Direct3DDevice, font);
            //font = CalcFont(30, percentScale);
            //if (font != null || force)
            //    _champF = new Font(Drawing.Direct3DDevice, font);
            //font = CalcFont(16, percentScale);
            //if (font != null || force)
            //    _sumF = new Font(Drawing.Direct3DDevice, font);
        }

        private static RecallDetector.RecallInfo GetRecall(int networkId)
        {
            try
            {
                var t = Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorMode").GetValue<StringList>();
                if (t.SelectedIndex == 1 || t.SelectedIndex == 2)
                {
                    var recallDetector = (RecallDetector)Menu.RecallDetector.Item;
                    if (recallDetector == null)
                        return null;
                    foreach (RecallDetector.RecallInfo info in recallDetector.Recalls)
                    {
                        if (info.NetworkId == networkId)
                        {
                            return info;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SAwareness: Could not get Recall. Message: " + ex.ToString());
                throw;
            }
        }

        private void DrawSideBarSimple(Dictionary<Obj_AI_Hero, ChampInfos> heroes, float percentScale)
        {
            foreach (var hero in heroes)
            {
                float percentHealth = CalcHpBar(hero.Key);
                float percentMana = CalcManaBar(hero.Key);

                //DirectXDrawer.DrawSprite(_s, hero.Value.Champ.Texture,
                //    hero.Value.Champ.SizeSideBar,
                //    new[] { 1.0f * percentScale, 1.0f * percentScale });
                //DirectXDrawer.DrawSprite(_s, hero.Value.SpellSum1.Texture,
                //    hero.Value.SpellSum1.SizeSideBar,
                //    new[] { 1.0f * percentScale, 1.0f * percentScale });
                //DirectXDrawer.DrawSprite(_s, hero.Value.SpellSum2.Texture,
                //    hero.Value.SpellSum2.SizeSideBar,
                //    new[] { 1.0f * percentScale, 1.0f * percentScale });

                //DirectXDrawer.DrawSprite(_s, _backBar,
                //    hero.Value.BackBar.SizeSideBar,
                //    new[] { 1.0f * percentScale * 0.75f, 1.0f * percentScale });
                //DirectXDrawer.DrawSprite(_s, _healthBar,
                //    hero.Value.HealthBar.SizeSideBar,
                //    new[] { 1.0f * percentHealth * percentScale * 0.75f, 1.0f * percentScale });
                //DirectXDrawer.DrawSprite(_s, _manaBar,
                //    hero.Value.ManaBar.SizeSideBar,
                //    new[] { 1.0f * percentMana * percentScale * 0.75f, 1.0f * percentScale });

                //if (hero.Value.DeathTimeDisplay > 0.0f)
                //{
                //    DirectXDrawer.DrawSprite(_s, _overlaySummoner,
                //        hero.Value.Champ.SizeSideBar,
                //        new ColorBGRA(Color3.White, 0.55f), new[] { 1.0f * percentScale, 1.0f * percentScale });
                //}
                //if (hero.Value.SpellSum1.Value > 0.0f)
                //{
                //    DirectXDrawer.DrawSprite(_s, _overlaySummonerSpell,
                //        hero.Value.SpellSum1.SizeSideBar,
                //        new ColorBGRA(Color3.White, 0.55f), new[] { 1.0f * percentScale, 1.0f * percentScale });
                //}
                //if (hero.Value.SpellSum2.Value > 0.0f)
                //{
                //    DirectXDrawer.DrawSprite(_s, _overlaySummonerSpell,
                //        hero.Value.SpellSum2.SizeSideBar,
                //        new ColorBGRA(Color3.White, 0.55f), new[] { 1.0f * percentScale, 1.0f * percentScale });
                //}

                //if (hero.Value.SpellR.Value > 0.0f || hero.Key.Spellbook.GetSpell(SpellSlot.R).Level < 1)
                //{
                //    DirectXDrawer.DrawSprite(_s, _overlaySpellItemRed,
                //        hero.Value.SpellR.SizeSideBar,
                //        new ColorBGRA(Color3.White, 0.65f), new[] { 2.0f * percentScale, 1.0f * percentScale });
                //}
                //else
                //{
                //    DirectXDrawer.DrawSprite(_s, _overlaySpellItemGreen,
                //        hero.Value.SpellR.SizeSideBar,
                //        new ColorBGRA(Color3.White, 0.65f), new[] { 2.0f * percentScale, 1.0f * percentScale });
                //}
            }

            foreach (var hero in heroes)
            {
                //if (hero.Value.SpellR.Value > 0.0f)
                //{
                //    DirectXDrawer.DrawText(_spellF, hero.Value.SpellR.Value.ToString(),
                //        hero.Value.SpellR.CoordsSideBar, SharpDX.Color.Orange);
                //}
                //if (hero.Value.DeathTimeDisplay > 0.0f && hero.Key.IsDead)
                //{
                //    DirectXDrawer.DrawText(_champF, hero.Value.DeathTimeDisplay.ToString(),
                //        hero.Value.Champ.CoordsSideBar, SharpDX.Color.Orange);
                //}
                //else if (hero.Value.InvisibleTime > 0.0f && !hero.Key.IsVisible)
                //{
                //    DirectXDrawer.DrawText(_champF, hero.Value.InvisibleTime.ToString(),
                //        hero.Value.Champ.CoordsSideBar, SharpDX.Color.Red);
                //}
                //if (hero.Value.SpellSum1.Value > 0.0f)
                //{
                //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellSum1.Value.ToString(),
                //        hero.Value.SpellSum1.CoordsSideBar, SharpDX.Color.Orange);
                //}
                //if (hero.Value.SpellSum2.Value > 0.0f)
                //{
                //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellSum2.Value.ToString(),
                //        hero.Value.SpellSum2.CoordsSideBar, SharpDX.Color.Orange);
                //}
            }
        }

        private void DrawSideBarDefault(Dictionary<Obj_AI_Hero, ChampInfos> heroes, float percentScale)
        {
            foreach (var hero in heroes)
            {
                //float percentHealth = CalcHpBar(hero.Key);
                //float percentMana = CalcManaBar(hero.Key);

                //DrawSprite(S, enemy.Value.PassiveTexture, nPassiveSize, Color.White);
                //DirectXDrawer.DrawSprite(_s, hero.Value.SpellQ.Texture,
                //    hero.Value.SpellQ.SizeSideBar,
                //    new[] { 1.0f * percentScale, 1.0f * percentScale });
                //DirectXDrawer.DrawSprite(_s, hero.Value.SpellW.Texture,
                //    hero.Value.SpellW.SizeSideBar,
                //    new[] { 1.0f * percentScale, 1.0f * percentScale });
                //DirectXDrawer.DrawSprite(_s, hero.Value.SpellE.Texture,
                //    hero.Value.SpellE.SizeSideBar,
                //    new[] { 1.0f * percentScale, 1.0f * percentScale });
                //DirectXDrawer.DrawSprite(_s, hero.Value.SpellR.Texture,
                //    hero.Value.SpellR.SizeSideBar,
                //    new[] { 1.0f * percentScale, 1.0f * percentScale });

                //DirectXDrawer.DrawSprite(_s, hero.Value.Champ.Texture,
                //    hero.Value.Champ.SizeSideBar,
                //    new[] { 1.0f * percentScale, 1.0f * percentScale });
                //DirectXDrawer.DrawSprite(_s, hero.Value.SpellSum1.Texture,
                //    hero.Value.SpellSum1.SizeSideBar,
                //    new[] { 1.0f * percentScale, 1.0f * percentScale });
                //DirectXDrawer.DrawSprite(_s, hero.Value.SpellSum2.Texture,
                //    hero.Value.SpellSum2.SizeSideBar,
                //    new[] { 1.0f * percentScale, 1.0f * percentScale });

                //DirectXDrawer.DrawSprite(_s, _backBar,
                //    hero.Value.BackBar.SizeSideBar,
                //    new[] { 1.0f * percentScale * 0.75f, 1.0f * percentScale });
                //DirectXDrawer.DrawSprite(_s, _healthBar,
                //    hero.Value.HealthBar.SizeSideBar,
                //    new[] { 1.0f * percentHealth * percentScale * 0.75f, 1.0f * percentScale });
                //DirectXDrawer.DrawSprite(_s, _manaBar,
                //    hero.Value.ManaBar.SizeSideBar,
                //    new[] { 1.0f * percentMana * percentScale * 0.75f, 1.0f * percentScale });

                //if (Menu.UiTracker.GetMenuItem("SAwarenessItemPanelActive").GetValue<bool>())
                //{
                //    foreach (ChampInfos.Gui.SpriteInfos spriteInfo in hero.Value.Item)
                //    {
                //        DirectXDrawer.DrawSprite(_s, spriteInfo.Texture,
                //            spriteInfo.SizeSideBar,
                //            new[] { 1.0f * percentScale, 1.0f * percentScale });
                //    }
                //}

                //if (hero.Value.SpellQ.Value > 0.0f || hero.Key.Spellbook.GetSpell(SpellSlot.Q).Level < 1)
                //{
                //    DirectXDrawer.DrawSprite(_s, _overlaySpellItem,
                //        hero.Value.SpellQ.SizeSideBar, new SharpDX.Color(255, 255, 255, 200),
                //        /*new ColorBGRA(Color3.White, 0.55f),*/ new[] { 1.0f * percentScale, 1.0f * percentScale });
                //}
                //if (hero.Value.SpellW.Value > 0.0f || hero.Key.Spellbook.GetSpell(SpellSlot.W).Level < 1)
                //{
                //    DirectXDrawer.DrawSprite(_s, _overlaySpellItem,
                //        hero.Value.SpellW.SizeSideBar,
                //        new ColorBGRA(Color3.White, 0.55f), new[] { 1.0f * percentScale, 1.0f * percentScale });
                //}
                //if (hero.Value.SpellE.Value > 0.0f || hero.Key.Spellbook.GetSpell(SpellSlot.E).Level < 1)
                //{
                //    DirectXDrawer.DrawSprite(_s, _overlaySpellItem,
                //        hero.Value.SpellE.SizeSideBar,
                //        new ColorBGRA(Color3.White, 0.55f), new[] { 1.0f * percentScale, 1.0f * percentScale });
                //}
                //if (hero.Value.SpellR.Value > 0.0f || hero.Key.Spellbook.GetSpell(SpellSlot.R).Level < 1)
                //{
                //    DirectXDrawer.DrawSprite(_s, _overlaySpellItem,
                //        hero.Value.SpellR.SizeSideBar,
                //        new ColorBGRA(Color3.White, 0.55f), new[] { 1.0f * percentScale, 1.0f * percentScale });
                //}
                //if (hero.Value.DeathTimeDisplay > 0.0f)
                //{
                //    DirectXDrawer.DrawSprite(_s, _overlaySummoner,
                //        hero.Value.Champ.SizeSideBar,
                //        new ColorBGRA(Color3.White, 0.55f), new[] { 1.0f * percentScale, 1.0f * percentScale });
                //}
                //if (hero.Value.SpellSum1.Value > 0.0f)
                //{
                //    DirectXDrawer.DrawSprite(_s, _overlaySummonerSpell,
                //        hero.Value.SpellSum1.SizeSideBar,
                //        new ColorBGRA(Color3.White, 0.55f), new[] { 1.0f * percentScale, 1.0f * percentScale });
                //}
                //if (hero.Value.SpellSum2.Value > 0.0f)
                //{
                //    DirectXDrawer.DrawSprite(_s, _overlaySummonerSpell,
                //        hero.Value.SpellSum2.SizeSideBar,
                //        new ColorBGRA(Color3.White, 0.55f), new[] { 1.0f * percentScale, 1.0f * percentScale });
                //}
                //if (Menu.UiTracker.GetMenuItem("SAwarenessItemPanelActive").GetValue<bool>())
                //{
                //    foreach (ChampInfos.Gui.SpriteInfos spriteInfo in hero.Value.Item)
                //    {
                //        if (spriteInfo.Value > 0.0f)
                //        {
                //            DirectXDrawer.DrawSprite(_s, _overlaySpellItem,
                //                spriteInfo.SizeSideBar,
                //                new ColorBGRA(Color3.White, 0.55f), new[] { 1.0f * percentScale, 1.0f * percentScale });
                //        }
                //    }
                //}
                //DirectXDrawer.DrawSprite(_s, _overlayGoldCsLvl,
                //    hero.Value.Champ.SizeSideBar,
                //    new ColorBGRA(Color3.White, 0.55f), new[] { 1.0f * percentScale, 1.0f * percentScale });
                //if (Menu.RecallDetector.GetActive())
                //{
                //    RecallDetector.RecallInfo info = GetRecall(hero.Key.NetworkId);
                //    if (info != null)
                //    {
                //        float percentRecall = CalcRecallBar(info);
                //        if (info != null && info.StartTime != 0)
                //        {
                //            float time = Game.Time + info.Recall.Duration / 1000 - info.StartTime;
                //            if (time > 0.0f &&
                //                (info.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportStart ||
                //                 info.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallStarted))
                //            {
                //                DirectXDrawer.DrawSprite(_s, _overlayRecall,
                //                    hero.Value.RecallBar.SizeSideBar,
                //                    new ColorBGRA(Color3.White, 0.80f),
                //                    new[] { 1.0f * percentRecall * percentScale, 1.0f * percentScale });
                //            }
                //            else if (time < 30.0f &&
                //                     (info.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportEnd ||
                //                      info.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallFinished))
                //            {
                //                DirectXDrawer.DrawSprite(_s, _overlayRecall,
                //                    hero.Value.RecallBar.SizeSideBar,
                //                    new ColorBGRA(Color3.White, 0.80f),
                //                    new[] { 1.0f * percentScale, 1.0f * percentScale });
                //            }
                //            else if (time < 30.0f &&
                //                     (info.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportAbort ||
                //                      info.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallAborted))
                //            {
                //                DirectXDrawer.DrawSprite(_s, _overlayRecall,
                //                    hero.Value.RecallBar.SizeSideBar,
                //                    new ColorBGRA(Color3.White, 0.80f),
                //                    new[] { 1.0f * percentScale, 1.0f * percentScale });
                //            }
                //        }
                //    }
                //}
            }

            foreach (var hero in heroes)
            {
            //    DirectXDrawer.DrawText(_spellF, hero.Value.SHealth,
            //        hero.Value.HealthBar.CoordsSideBar, SharpDX.Color.Orange);
            //    DirectXDrawer.DrawText(_spellF, hero.Value.SMana,
            //        hero.Value.ManaBar.CoordsSideBar, SharpDX.Color.Orange);
            //    if (hero.Value.SpellQ.Value > 0.0f)
            //    {
            //        DirectXDrawer.DrawText(_spellF, hero.Value.SpellQ.Value.ToString(),
            //            hero.Value.SpellQ.CoordsSideBar, SharpDX.Color.Orange);
            //    }
            //    if (hero.Value.SpellW.Value > 0.0f)
            //    {
            //        DirectXDrawer.DrawText(_spellF, hero.Value.SpellW.Value.ToString(),
            //            hero.Value.SpellW.CoordsSideBar, SharpDX.Color.Orange);
            //    }
            //    if (hero.Value.SpellE.Value > 0.0f)
            //    {
            //        DirectXDrawer.DrawText(_spellF, hero.Value.SpellE.Value.ToString(),
            //            hero.Value.SpellE.CoordsSideBar, SharpDX.Color.Orange);
            //    }
            //    if (hero.Value.SpellR.Value > 0.0f)
            //    {
            //        DirectXDrawer.DrawText(_spellF, hero.Value.SpellR.Value.ToString(),
            //            hero.Value.SpellR.CoordsSideBar, SharpDX.Color.Orange);
            //    }
            //    if (hero.Value.DeathTimeDisplay > 0.0f && hero.Key.IsDead)
            //    {
            //        DirectXDrawer.DrawText(_champF, hero.Value.DeathTimeDisplay.ToString(),
            //            hero.Value.Champ.CoordsSideBar, SharpDX.Color.Orange);
            //    }
            //    else if (hero.Value.InvisibleTime > 0.0f && !hero.Key.IsVisible)
            //    {
            //        DirectXDrawer.DrawText(_champF, hero.Value.InvisibleTime.ToString(),
            //            hero.Value.Champ.CoordsSideBar, SharpDX.Color.Red);
            //    }
            //    if (hero.Value.SpellSum1.Value > 0.0f)
            //    {
            //        DirectXDrawer.DrawText(_sumF, hero.Value.SpellSum1.Value.ToString(),
            //            hero.Value.SpellSum1.CoordsSideBar, SharpDX.Color.Orange);
            //    }
            //    if (hero.Value.SpellSum2.Value > 0.0f)
            //    {
            //        DirectXDrawer.DrawText(_sumF, hero.Value.SpellSum2.Value.ToString(),
            //            hero.Value.SpellSum2.CoordsSideBar, SharpDX.Color.Orange);
            //    }
            //    if (Menu.UiTracker.GetMenuItem("SAwarenessItemPanelActive").GetValue<bool>())
            //    {
            //        foreach (ChampInfos.SpriteInfos spriteInfo in hero.Value.Item)
            //        {
            //            if (spriteInfo.Value > 0.0f)
            //            {
            //                DirectXDrawer.DrawText(_spellF, spriteInfo.Value.ToString(),
            //                    spriteInfo.CoordsSideBar, SharpDX.Color.Orange);
            //            }
            //        }
            //    }
            //    DirectXDrawer.DrawText(_recF, hero.Value.Level.Value.ToString(),
            //            hero.Value.Level.CoordsSideBar, SharpDX.Color.Orange);
            //    //DirectXDrawer.DrawText(_recF, hero.Value.Gold.Value.ToString(),
            //    //        hero.Value.Gold.CoordsSideBar, SharpDX.Color.Orange);
            //    DirectXDrawer.DrawText(_recF, hero.Value.Cs.Value.ToString(),
            //            hero.Value.Cs.CoordsSideBar, SharpDX.Color.Orange);
            //    if (Menu.RecallDetector.GetActive())
            //    {
            //        RecallDetector.RecallInfo info = GetRecall(hero.Key.NetworkId);
            //        if (info != null && info.StartTime != 0)
            //        {
            //            float time = Game.Time + info.Recall.Duration / 1000 - info.StartTime;
            //            if (time > 0.0f &&
            //                (info.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportStart ||
            //                 info.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallStarted))
            //            {
            //                DirectXDrawer.DrawText(_recF, "Porting",
            //                    hero.Value.RecallBar.CoordsSideBar,
            //                    SharpDX.Color.Chartreuse);
            //            }
            //            else if (time < 30.0f &&
            //                     (info.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportEnd ||
            //                      info.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallFinished))
            //            {
            //                DirectXDrawer.DrawText(_recF, "Ported",
            //                    hero.Value.RecallBar.CoordsSideBar,
            //                    SharpDX.Color.Chartreuse);
            //            }
            //            else if (time < 30.0f &&
            //                     (info.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportAbort ||
            //                      info.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallAborted))
            //            {
            //                DirectXDrawer.DrawText(_recF, "Canceled",
            //                    hero.Value.RecallBar.CoordsSideBar,
            //                    SharpDX.Color.Chartreuse);
            //            }
            //        }
            //    }
            }
        }

        private void DrawSideBar(Dictionary<Obj_AI_Hero, ChampInfos> heroes, float percentScale, StringList modeSideDisplayChoice)
        {
            if (modeSideDisplayChoice.SelectedIndex == 0)
            {
                DrawSideBarDefault(heroes, percentScale);
            }
            else
            {
                DrawSideBarSimple(heroes, percentScale);
            }
        }

        private void DrawOverHeadSimple(Dictionary<Obj_AI_Hero, ChampInfos> heroes, float percentScale, StringList modeHeadChoice)
        {
            if (modeHeadChoice.SelectedIndex == 0)
            {
                foreach (var hero in heroes)
                {
                    if (!hero.Key.IsDead && hero.Key.IsVisible)
                    {
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellSum1.Texture,
                        //    hero.Value.SpellSum1.SizeHpBar,
                        //    new[] { 0.8f * percentScale, 0.8f * percentScale });
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellSum2.Texture,
                        //    hero.Value.SpellSum2.SizeHpBar,
                        //    new[] { 0.8f * percentScale, 0.8f * percentScale });

                        //if (hero.Value.SpellQ.Value > 0.0f ||
                        //    hero.Key.Spellbook.GetSpell(SpellSlot.Q).Level < 1)
                        //{
                        //    _recS.Color = SharpDX.Color.Red;
                        //}
                        //else
                        //{
                        //    _recS.Color = SharpDX.Color.Green;
                        //}
                        //_recS.X = hero.Value.SpellQ.SizeHpBar.Width;
                        //_recS.Y = hero.Value.SpellQ.SizeHpBar.Height;
                        //_recS.OnEndScene();
                        //if (hero.Value.SpellW.Value > 0.0f ||
                        //    hero.Key.Spellbook.GetSpell(SpellSlot.W).Level < 1)
                        //{
                        //    _recS.Color = SharpDX.Color.Red;
                        //}
                        //else
                        //{
                        //    _recS.Color = SharpDX.Color.Green;
                        //}
                        //_recS.X = hero.Value.SpellW.SizeHpBar.Width;
                        //_recS.Y = hero.Value.SpellW.SizeHpBar.Height;
                        //_recS.OnEndScene();
                        //if (hero.Value.SpellE.Value > 0.0f ||
                        //    hero.Key.Spellbook.GetSpell(SpellSlot.E).Level < 1)
                        //{
                        //    _recS.Color = SharpDX.Color.Red;
                        //}
                        //else
                        //{
                        //    _recS.Color = SharpDX.Color.Green;
                        //}
                        //_recS.X = hero.Value.SpellE.SizeHpBar.Width;
                        //_recS.Y = hero.Value.SpellE.SizeHpBar.Height;
                        //_recS.OnEndScene();
                        //if (hero.Value.SpellR.Value > 0.0f ||
                        //    hero.Key.Spellbook.GetSpell(SpellSlot.R).Level < 1)
                        //{
                        //    _recS.Color = SharpDX.Color.Red;
                        //}
                        //else
                        //{
                        //    _recS.Color = SharpDX.Color.Green;
                        //}
                        //_recS.X = hero.Value.SpellR.SizeHpBar.Width;
                        //_recS.Y = hero.Value.SpellR.SizeHpBar.Height;
                        //_recS.OnEndScene();
                        //if (hero.Value.SpellSum1.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawSprite(_s, _overlaySummonerSpell,
                        //        hero.Value.SpellSum1.SizeHpBar,
                        //        new ColorBGRA(Color3.White, 0.55f),
                        //        new[] { 0.8f * percentScale, 0.8f * percentScale });
                        //}
                        //if (hero.Value.SpellSum2.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawSprite(_s, _overlaySummonerSpell,
                        //        hero.Value.SpellSum2.SizeHpBar,
                        //        new ColorBGRA(Color3.White, 0.55f),
                        //        new[] { 0.8f * percentScale, 0.8f * percentScale });
                        //}
                    }
                }
                foreach (var hero in heroes)
                {
                    if (!hero.Key.IsDead && hero.Key.IsVisible)
                    {
                        //if (hero.Value.SpellQ.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_spellF, hero.Value.SpellQ.Value.ToString(),
                        //        hero.Value.SpellQ.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //else
                        //{
                        //    DirectXDrawer.DrawText(_spellF, "Q",
                        //        hero.Value.SpellQ.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellW.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_spellF, hero.Value.SpellW.Value.ToString(),
                        //        hero.Value.SpellW.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //else
                        //{
                        //    DirectXDrawer.DrawText(_spellF, "W",
                        //        hero.Value.SpellW.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellE.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_spellF, hero.Value.SpellE.Value.ToString(),
                        //        hero.Value.SpellE.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //else
                        //{
                        //    DirectXDrawer.DrawText(_spellF, "E",
                        //        hero.Value.SpellE.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellR.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_spellF, hero.Value.SpellR.Value.ToString(),
                        //        hero.Value.SpellR.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //else
                        //{
                        //    DirectXDrawer.DrawText(_spellF, "R",
                        //        hero.Value.SpellR.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellSum1.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellSum1.Value.ToString(),
                        //        hero.Value.SpellSum1.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellSum2.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellSum2.Value.ToString(),
                        //        hero.Value.SpellSum2.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                    }
                }
            }
            else
            {
                foreach (var hero in heroes)
                {
                    if (!hero.Key.IsDead && hero.Key.IsVisible)
                    {
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellSum1.Texture,
                        //    hero.Value.SpellSum1.SizeHpBar,
                        //    new[] { 1.0f * percentScale, 1.0f * percentScale });
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellSum2.Texture,
                        //    hero.Value.SpellSum2.SizeHpBar,
                        //    new[] { 1.0f * percentScale, 1.0f * percentScale });

                        //if (hero.Value.SpellQ.Value > 0.0f ||
                        //    hero.Key.Spellbook.GetSpell(SpellSlot.Q).Level < 1)
                        //{
                        //    _recB.Color = SharpDX.Color.Red;
                        //}
                        //else
                        //{
                        //    _recB.Color = SharpDX.Color.Green;
                        //}
                        //_recB.X = hero.Value.SpellQ.SizeHpBar.Width;
                        //_recB.Y = hero.Value.SpellQ.SizeHpBar.Height;
                        //_recB.OnEndScene();
                        //if (hero.Value.SpellW.Value > 0.0f ||
                        //    hero.Key.Spellbook.GetSpell(SpellSlot.W).Level < 1)
                        //{
                        //    _recB.Color = SharpDX.Color.Red;
                        //}
                        //else
                        //{
                        //    _recB.Color = SharpDX.Color.Green;
                        //}
                        //_recB.X = hero.Value.SpellW.SizeHpBar.Width;
                        //_recB.Y = hero.Value.SpellW.SizeHpBar.Height;
                        //_recB.OnEndScene();
                        //if (hero.Value.SpellE.Value > 0.0f ||
                        //    hero.Key.Spellbook.GetSpell(SpellSlot.E).Level < 1)
                        //{
                        //    _recB.Color = SharpDX.Color.Red;
                        //}
                        //else
                        //{
                        //    _recB.Color = SharpDX.Color.Green;
                        //}
                        //_recB.X = hero.Value.SpellE.SizeHpBar.Width;
                        //_recB.Y = hero.Value.SpellE.SizeHpBar.Height;
                        //_recB.OnEndScene();
                        //if (hero.Value.SpellR.Value > 0.0f ||
                        //    hero.Key.Spellbook.GetSpell(SpellSlot.R).Level < 1)
                        //{
                        //    _recB.Color = SharpDX.Color.Red;
                        //}
                        //else
                        //{
                        //    _recB.Color = SharpDX.Color.Green;
                        //}
                        //_recB.X = hero.Value.SpellR.SizeHpBar.Width;
                        //_recB.Y = hero.Value.SpellR.SizeHpBar.Height;
                        //_recB.OnEndScene();
                    }
                }
                foreach (var hero in heroes)
                {
                    if (!hero.Key.IsDead && hero.Key.IsVisible)
                    {
                        //if (hero.Value.SpellQ.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellQ.Value.ToString(),
                        //        hero.Value.SpellQ.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //else
                        //{
                        //    DirectXDrawer.DrawText(_sumF, "Q",
                        //        hero.Value.SpellQ.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellW.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellW.Value.ToString(),
                        //        hero.Value.SpellW.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //else
                        //{
                        //    DirectXDrawer.DrawText(_sumF, "W",
                        //        hero.Value.SpellW.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellE.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellE.Value.ToString(),
                        //        hero.Value.SpellE.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //else
                        //{
                        //    DirectXDrawer.DrawText(_sumF, "E",
                        //        hero.Value.SpellE.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellR.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellR.Value.ToString(),
                        //        hero.Value.SpellR.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //else
                        //{
                        //    DirectXDrawer.DrawText(_sumF, "R",
                        //        hero.Value.SpellR.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellSum1.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellSum1.Value.ToString(),
                        //        hero.Value.SpellSum1.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellSum2.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellSum2.Value.ToString(),
                        //        hero.Value.SpellSum2.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                    }
                }
            }
        }

        private void DrawOverHeadDefault(Dictionary<Obj_AI_Hero, ChampInfos> heroes, float percentScale, StringList modeHeadChoice)
        {
            if (modeHeadChoice.SelectedIndex == 0)
            {
                foreach (var hero in heroes)
                {
                    if (!hero.Key.IsDead && hero.Key.IsVisible)
                    {
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellPassive.Texture,
                        //    hero.Value.SpellPassive.SizeHpBar,
                        //    new[] { 1.0f * percentScale, 1.0f * percentScale });
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellQ.Texture,
                        //    hero.Value.SpellQ.SizeHpBar,
                        //    new[] { 1.0f * percentScale, 1.0f * percentScale });
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellW.Texture,
                        //    hero.Value.SpellW.SizeHpBar,
                        //    new[] { 1.0f * percentScale, 1.0f * percentScale });
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellE.Texture,
                        //    hero.Value.SpellE.SizeHpBar,
                        //    new[] { 1.0f * percentScale, 1.0f * percentScale });
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellR.Texture,
                        //    hero.Value.SpellR.SizeHpBar,
                        //    new[] { 1.0f * percentScale, 1.0f * percentScale });
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellSum1.Texture,
                        //    hero.Value.SpellSum1.SizeHpBar,
                        //    new[] { 0.8f * percentScale, 0.8f * percentScale });
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellSum2.Texture,
                        //    hero.Value.SpellSum2.SizeHpBar,
                        //    new[] { 0.8f * percentScale, 0.8f * percentScale });

                        //if (hero.Value.SpellQ.Value > 0.0f ||
                        //    hero.Key.Spellbook.GetSpell(SpellSlot.Q).Level < 1)
                        //{
                        //    DirectXDrawer.DrawSprite(_s, _overlaySpellItem,
                        //        hero.Value.SpellQ.SizeHpBar,
                        //        new ColorBGRA(Color3.White, 0.55f),
                        //        new[] { 1.0f * percentScale, 1.0f * percentScale });
                        //}
                        //if (hero.Value.SpellW.Value > 0.0f ||
                        //    hero.Key.Spellbook.GetSpell(SpellSlot.W).Level < 1)
                        //{
                        //    DirectXDrawer.DrawSprite(_s, _overlaySpellItem,
                        //        hero.Value.SpellW.SizeHpBar,
                        //        new ColorBGRA(Color3.White, 0.55f),
                        //        new[] { 1.0f * percentScale, 1.0f * percentScale });
                        //}
                        //if (hero.Value.SpellE.Value > 0.0f ||
                        //    hero.Key.Spellbook.GetSpell(SpellSlot.E).Level < 1)
                        //{
                        //    DirectXDrawer.DrawSprite(_s, _overlaySpellItem,
                        //        hero.Value.SpellE.SizeHpBar,
                        //        new ColorBGRA(Color3.White, 0.55f),
                        //        new[] { 1.0f * percentScale, 1.0f * percentScale });
                        //}
                        //if (hero.Value.SpellR.Value > 0.0f ||
                        //    hero.Key.Spellbook.GetSpell(SpellSlot.R).Level < 1)
                        //{
                        //    DirectXDrawer.DrawSprite(_s, _overlaySpellItem,
                        //        hero.Value.SpellR.SizeHpBar,
                        //        new ColorBGRA(Color3.White, 0.55f),
                        //        new[] { 1.0f * percentScale, 1.0f * percentScale });
                        //}
                        //if (hero.Value.SpellSum1.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawSprite(_s, _overlaySummonerSpell,
                        //        hero.Value.SpellSum1.SizeHpBar,
                        //        new ColorBGRA(Color3.White, 0.55f),
                        //        new[] { 0.8f * percentScale, 0.8f * percentScale });
                        //}
                        //if (hero.Value.SpellSum2.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawSprite(_s, _overlaySummonerSpell,
                        //        hero.Value.SpellSum2.SizeHpBar,
                        //        new ColorBGRA(Color3.White, 0.55f),
                        //        new[] { 0.8f * percentScale, 0.8f * percentScale });
                        //}
                    }
                }
                foreach (var hero in heroes)
                {
                    if (!hero.Key.IsDead && hero.Key.IsVisible)
                    {
                        //if (hero.Value.SpellQ.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_spellF, hero.Value.SpellQ.Value.ToString(),
                        //        hero.Value.SpellQ.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellW.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_spellF, hero.Value.SpellW.Value.ToString(),
                        //        hero.Value.SpellW.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellE.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_spellF, hero.Value.SpellE.Value.ToString(),
                        //        hero.Value.SpellE.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellR.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_spellF, hero.Value.SpellR.Value.ToString(),
                        //        hero.Value.SpellR.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellSum1.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellSum1.Value.ToString(),
                        //        hero.Value.SpellSum1.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellSum2.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellSum2.Value.ToString(),
                        //        hero.Value.SpellSum2.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                    }
                }
            }
            else
            {
                foreach (var hero in heroes)
                {
                    if (!hero.Key.IsDead && hero.Key.IsVisible)
                    {
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellPassive.Texture,
                        //    hero.Value.SpellPassive.SizeHpBar,
                        //    new[] { 1.7f * percentScale, 1.7f * percentScale });
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellQ.Texture,
                        //    hero.Value.SpellQ.SizeHpBar,
                        //    new[] { 1.7f * percentScale, 1.7f * percentScale });
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellW.Texture,
                        //    hero.Value.SpellW.SizeHpBar,
                        //    new[] { 1.7f * percentScale, 1.7f * percentScale });
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellE.Texture,
                        //    hero.Value.SpellE.SizeHpBar,
                        //    new[] { 1.7f * percentScale, 1.7f * percentScale });
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellR.Texture,
                        //    hero.Value.SpellR.SizeHpBar,
                        //    new[] { 1.7f * percentScale, 1.7f * percentScale });
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellSum1.Texture,
                        //    hero.Value.SpellSum1.SizeHpBar,
                        //    new[] { 1.0f * percentScale, 1.0f * percentScale });
                        //DirectXDrawer.DrawSprite(_s, hero.Value.SpellSum2.Texture,
                        //    hero.Value.SpellSum2.SizeHpBar,
                        //    new[] { 1.0f * percentScale, 1.0f * percentScale });

                        //if (hero.Value.SpellQ.Value > 0.0f ||
                        //    hero.Key.Spellbook.GetSpell(SpellSlot.Q).Level < 1)
                        //{
                        //    DirectXDrawer.DrawSprite(_s, _overlaySpellItem,
                        //        hero.Value.SpellQ.SizeHpBar,
                        //        new ColorBGRA(Color3.White, 0.55f),
                        //        new[] { 1.7f * percentScale, 1.7f * percentScale });
                        //}
                        //if (hero.Value.SpellW.Value > 0.0f ||
                        //    hero.Key.Spellbook.GetSpell(SpellSlot.W).Level < 1)
                        //{
                        //    DirectXDrawer.DrawSprite(_s, _overlaySpellItem,
                        //        hero.Value.SpellW.SizeHpBar,
                        //        new ColorBGRA(Color3.White, 0.55f),
                        //        new[] { 1.7f * percentScale, 1.7f * percentScale });
                        //}
                        //if (hero.Value.SpellE.Value > 0.0f ||
                        //    hero.Key.Spellbook.GetSpell(SpellSlot.E).Level < 1)
                        //{
                        //    DirectXDrawer.DrawSprite(_s, _overlaySpellItem,
                        //        hero.Value.SpellE.SizeHpBar,
                        //        new ColorBGRA(Color3.White, 0.55f),
                        //        new[] { 1.7f * percentScale, 1.7f * percentScale });
                        //}
                        //if (hero.Value.SpellR.Value > 0.0f ||
                        //    hero.Key.Spellbook.GetSpell(SpellSlot.R).Level < 1)
                        //{
                        //    DirectXDrawer.DrawSprite(_s, _overlaySpellItem,
                        //        hero.Value.SpellR.SizeHpBar,
                        //        new ColorBGRA(Color3.White, 0.55f),
                        //        new[] { 1.7f * percentScale, 1.7f * percentScale });
                        //}
                        //if (hero.Value.SpellSum1.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawSprite(_s, _overlaySummonerSpell,
                        //        hero.Value.SpellSum1.SizeHpBar,
                        //        new ColorBGRA(Color3.White, 0.55f),
                        //        new[] { 1.0f * percentScale, 1.0f * percentScale });
                        //}
                        //if (hero.Value.SpellSum2.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawSprite(_s, _overlaySummonerSpell,
                        //        hero.Value.SpellSum2.SizeHpBar,
                        //        new ColorBGRA(Color3.White, 0.55f),
                        //        new[] { 1.0f * percentScale, 1.0f * percentScale });
                        //}
                    }
                }
                foreach (var hero in heroes)
                {
                    if (!hero.Key.IsDead && hero.Key.IsVisible)
                    {
                        //if (hero.Value.SpellQ.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellQ.Value.ToString(),
                        //        hero.Value.SpellQ.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellW.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellW.Value.ToString(),
                        //        hero.Value.SpellW.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellE.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellE.Value.ToString(),
                        //        hero.Value.SpellE.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellR.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellR.Value.ToString(),
                        //        hero.Value.SpellR.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellSum1.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellSum1.Value.ToString(),
                        //        hero.Value.SpellSum1.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                        //if (hero.Value.SpellSum2.Value > 0.0f)
                        //{
                        //    DirectXDrawer.DrawText(_sumF, hero.Value.SpellSum2.Value.ToString(),
                        //        hero.Value.SpellSum2.CoordsHpBar, SharpDX.Color.Orange);
                        //}
                    }
                }
            }
        }

        private void DrawOverHead(Dictionary<Obj_AI_Hero, ChampInfos> heroes, float percentScale, StringList modeHeadChoice, StringList modeHeadDisplayChoice)
        {           
            if (modeHeadDisplayChoice.SelectedIndex == 0)
            {
                DrawOverHeadDefault(heroes, percentScale, modeHeadChoice);
            }
            else
            {
                DrawOverHeadSimple(heroes, percentScale, modeHeadChoice);
            }
        }

        private void DrawInterface(bool enemy)
        {
            try
            {
                StringList modeChoice;
                StringList modeSideDisplayChoice;
                StringList modeHeadChoice;
                StringList modeHeadDisplayChoice;
                Dictionary<Obj_AI_Hero, ChampInfos> heroes;
                if (enemy)
                {
                    heroes = _enemies;
                    modeChoice =
                        Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                            .GetMenuItem("SAwarenessUITrackerEnemyTrackerMode")
                            .GetValue<StringList>();
                    modeSideDisplayChoice =
                        Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                            .GetMenuItem("SAwarenessUITrackerEnemyTrackerSideDisplayMode")
                            .GetValue<StringList>();
                    modeHeadChoice =
                        Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                            .GetMenuItem("SAwarenessUITrackerEnemyTrackerHeadMode")
                            .GetValue<StringList>();
                    modeHeadDisplayChoice =
                        Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                            .GetMenuItem("SAwarenessUITrackerEnemyTrackerHeadDisplayMode")
                            .GetValue<StringList>();
                }
                else
                {
                    heroes = _allies;
                    modeChoice =
                        Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                            .GetMenuItem("SAwarenessUITrackerAllyTrackerMode")
                            .GetValue<StringList>();
                    modeSideDisplayChoice =
                        Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                            .GetMenuItem("SAwarenessUITrackerAllyTrackerSideDisplayMode")
                            .GetValue<StringList>();
                    modeHeadChoice =
                        Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                            .GetMenuItem("SAwarenessUITrackerAllyTrackerHeadMode")
                            .GetValue<StringList>();
                    modeHeadDisplayChoice =
                        Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                            .GetMenuItem("SAwarenessUITrackerAllyTrackerHeadDisplayMode")
                            .GetValue<StringList>();
                }

                float percentScale =
                    (float) Menu.UiTracker.GetMenuItem("SAwarenessUITrackerScale").GetValue<Slider>().Value/100;
                if (
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerXPos")
                        .GetValue<Slider>()
                        .Value != _oldEx ||
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker")
                        .GetMenuItem("SAwarenessUITrackerEnemyTrackerYPos")
                        .GetValue<Slider>()
                        .Value != _oldEy
                    ||
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerXPos")
                        .GetValue<Slider>()
                        .Value != _oldAx ||
                    Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker")
                        .GetMenuItem("SAwarenessUITrackerAllyTrackerYPos")
                        .GetValue<Slider>()
                        .Value != _oldAy
                    || percentScale != _scalePc)
                {
                    CalculateSizes(true);
                    CalculateSizes(false);
                }

                if (percentScale != _scalePc)
                {
                    _scalePc = percentScale;
                    AssingFonts(percentScale);
                }

                StringList mode = modeChoice;              
                if (mode.SelectedIndex == 0 || mode.SelectedIndex == 2)
                {
                    DrawSideBar(heroes, percentScale, modeSideDisplayChoice);                    
                }
                if (mode.SelectedIndex == 1 || mode.SelectedIndex == 2)
                {
                    DrawOverHead(heroes, percentScale, modeHeadChoice, modeHeadDisplayChoice);
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if (ex.GetType() == typeof (SharpDXException))
                {
                    Menu.UiTracker.SetActive(false);
                    Game.PrintChat("UITracker: An error occured. Please activate CDPanel in your menu again.");
                }
            }
        }

        //private void Drawing_OnEndScene(EventArgs args)
        //{
        //    if (!IsActive() || !_drawActive)
        //        return;

        //    foreach (var enemy in _enemies)
        //    {
        //        //foreach (var sprite in enemy.Value.Champ.Sprite)
        //        //{
        //        //    if(sprite.Visible) sprite.OnEndScene();
        //        //}
        //        ////enemy.Value.Champ.Sprite[0].OnEndScene();
        //        //foreach (var sprite in enemy.Value.SpellQ.Sprite)
        //        //{
        //        //    if (sprite.Visible) sprite.OnEndScene();
        //        //}
        //        ////enemy.Value.SpellQ.Sprite[0].OnEndScene();
        //        //enemy.Value.SpellW.Sprite[0].OnEndScene();
        //        //enemy.Value.SpellE.Sprite[0].OnEndScene();
        //        //enemy.Value.SpellR.Sprite[0].OnEndScene();
        //        //enemy.Value.SpellSum1.Sprite[0].OnEndScene();
        //        //enemy.Value.SpellSum2.Sprite[0].OnEndScene();
        //    }

        //    //if (Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerEnemyTracker").GetActive())
        //    //{
        //    //    DrawInterface(true);
        //    //    int teamGoldEnemy = 0;
        //    //    foreach (var enemy in _enemies)
        //    //    {
        //    //        teamGoldEnemy += enemy.Value.Gold.Value;
        //    //    }
        //    //    //DirectXDrawer.DrawText(_sumF, teamGoldEnemy.ToString(),
        //    //    //                _enemies.Last().Value.Champ.SizeSideBar, SharpDX.Color.Orange);
        //    //}
        //    //if (Menu.UiTracker.GetMenuSettings("SAwarenessUITrackerAllyTracker").GetActive())
        //    //{
        //    //    DrawInterface(false);
        //    //    int teamGoldAlly = 0;
        //    //    foreach (var ally in ObjectManager.Get<Obj_AI_Hero>())
        //    //    {
        //    //        if(ally.IsAlly)
        //    //            teamGoldAlly += (int)ally.GoldEarned;
        //    //    }
        //    //    //DirectXDrawer.DrawText(_sumF, teamGoldAlly.ToString(),
        //    //    //                _allies.Last().Value.Champ.SizeSideBar, SharpDX.Color.Orange);
        //    //}
        //}

        public class ChampInfos
        {
            public SpriteInfos BackBar = new SpriteInfos();
            public SpriteInfos Champ = new SpriteInfos();
            public SpriteInfos HealthBar = new SpriteInfos();
            public SpriteInfos[] Item = new SpriteInfos[7];
            public ItemId[] ItemId = new ItemId[7];
            public SpriteInfos ManaBar = new SpriteInfos();
            public SpriteInfos RecallBar = new SpriteInfos();
            public SpriteInfos SpellPassive = new SpriteInfos();
            public SpriteInfos SpellQ = new SpriteInfos();
            public SpriteInfos SpellW = new SpriteInfos();
            public SpriteInfos SpellE = new SpriteInfos();
            public SpriteInfos SpellR = new SpriteInfos();
            public SpriteInfos SpellSum1 = new SpriteInfos();
            public SpriteInfos SpellSum2 = new SpriteInfos();
            public SpriteInfos GoldCsLvlBar = new SpriteInfos();
            public SpriteInfos Gold = new SpriteInfos();
            public SpriteInfos Level = new SpriteInfos();
            public SpriteInfos Cs = new SpriteInfos();
            public int DeathTime;
            public int DeathTimeDisplay;
            public bool Dead;
            public int InvisibleTime;
            public Vector2 Pos = new Vector2();
            public String SHealth;
            public String SMana;
            public int VisibleTime;

            public class SpriteInfos
            {
                public SpriteHelper.SpriteInfo[] Sprite = new SpriteHelper.SpriteInfo[10];
                public Render.Rectangle[] Rectangle = new Render.Rectangle[10];
                public Render.Text[] Text = new Render.Text[10];
                public int Value;
                public Size CoordsHpBar;
                public Size CoordsSideBar;
                public Size SizeHpBar;
                public Size SizeSideBar;
                //public Texture Texture;
            }
        }
    }

    public class UimTracker
    {
        private static Dictionary<Obj_AI_Hero, SpriteHelper.SpriteInfo> _enemies = new Dictionary<Obj_AI_Hero, SpriteHelper.SpriteInfo>();

        public UimTracker()
        {
            if (!IsActive())
                return;
        }

        ~UimTracker()
        {
            
            _enemies = null;
        }

        public bool IsActive()
        {
            return Menu.Tracker.GetActive() && Menu.UimTracker.GetActive();
        }

        public async static Task Init()
        {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    SpriteHelper.SpriteInfo champ = new SpriteHelper.SpriteInfo();

                    Task<SpriteHelper.SpriteInfo> champInfos = CreateImage(hero, champ);
                    champ = await champInfos;

                    _enemies.Add(hero, champ);
                }
            }
        }

        //public static void Init()
        //{
        //    foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
        //    {
        //        if (hero.IsEnemy)
        //        {
        //            SpriteHelper.SpriteInfo champ = new SpriteHelper.SpriteInfo();
        //            _enemies.Add(hero, champ);
        //        }
        //    }
        //}

        //public async static Task DownloadImages()
        //{
        //    foreach (var enemy in _enemies)
        //    {
        //        if (enemy.Key.IsEnemy)
        //        {
        //            Task<Bitmap> map = SpriteHelper.DownloadImageAsync(enemy.Key.ChampionName, SpriteHelper.DownloadType.Champion);
        //            enemy.Value.Bitmap = await map;
        //            enemy.Value.DownloadFinished = true;
        //        }
        //    }
        //}

        //public static void CreateImages()
        //{
        //    float percentScale =
        //            (float)Menu.UimTracker.GetMenuItem("SAwarenessUIMTrackerScale").GetValue<Slider>().Value / 100;
        //    foreach (var enemy in _enemies)
        //    {
        //        if (enemy.Value.DownloadFinished && !enemy.Value.LoadingFinished)
        //        {
        //            SpriteHelper.LoadTexture(enemy.Value.Bitmap, ref enemy.Value.Sprite);
        //            enemy.Value.LoadingFinished = true;
        //            if (enemy.Value.Sprite.Bitmap != null)
        //                enemy.Value.Sprite.UpdateTextureBitmap(CropImage(enemy.Value.Sprite.Bitmap, enemy.Value.Sprite.Width));
        //            enemy.Value.Sprite.GrayScale();
        //            enemy.Value.Sprite.Scale = new Vector2(((float)24 / enemy.Value.Sprite.Width) * percentScale, ((float)24 / enemy.Value.Sprite.Height) * percentScale);
        //            enemy.Value.Sprite.PositionUpdate = delegate
        //            {
        //                Vector2 serverPos = Drawing.WorldToMinimap(enemy.Key.ServerPosition);
        //                var mPos = new Size((int)(serverPos[0] - 32 * 0.3f), (int)(serverPos[1] - 32 * 0.3f));
        //                return new Vector2(mPos.Width, mPos.Height);
        //            };
        //            enemy.Value.Sprite.VisibleCondition = delegate
        //            {
        //                return Menu.Tracker.GetActive() && Menu.UimTracker.GetActive() && !enemy.Key.IsVisible;
        //            };
        //            enemy.Value.Sprite.Add();
        //        }
        //    }
        //}

        private async static Task<SpriteHelper.SpriteInfo> CreateImage(Obj_AI_Hero hero, SpriteHelper.SpriteInfo champ)
        {
            float percentScale =
                    (float)Menu.UimTracker.GetMenuItem("SAwarenessUIMTrackerScale").GetValue<Slider>().Value / 100;
            Task<SpriteHelper.SpriteInfo> taskInfo = SpriteHelper.LoadTextureAsync(
                hero.ChampionName, champ, SpriteHelper.DownloadType.Champion);
            champ = await taskInfo;
            if (!champ.LoadingFinished)
            {
                Utility.DelayAction.Add(5000, () => UiTracker.UpdateChampImage(hero, new Size(champ.Sprite.Width, champ.Sprite.Width), champ, UiTracker.UpdateMethod.MiniMap));
            }
            else
            {
                if (champ.Sprite.Bitmap != null)
                    champ.Sprite.UpdateTextureBitmap(CropImage(champ.Sprite.Bitmap, champ.Sprite.Width));
                champ.Sprite.GrayScale();
                champ.Sprite.Scale = new Vector2(((float)24 / champ.Sprite.Width) * percentScale, ((float)24 / champ.Sprite.Height) * percentScale);
                champ.Sprite.PositionUpdate = delegate
                {
                    Vector2 serverPos = Drawing.WorldToMinimap(hero.ServerPosition);
                    var mPos = new Size((int)(serverPos[0] - 32 * 0.3f), (int)(serverPos[1] - 32 * 0.3f));
                    return new Vector2(mPos.Width, mPos.Height);
                };
                champ.Sprite.VisibleCondition = delegate
                {
                    return Menu.Tracker.GetActive() && Menu.UimTracker.GetActive() && !hero.IsVisible;
                };
                champ.Sprite.Add();
            }

            return champ;
        }

        public static Bitmap CropImage(Bitmap srcBitmap, int imageWidth)
        {
            Bitmap finalImage = new Bitmap(imageWidth, imageWidth);
            System.Drawing.Rectangle cropRect = new System.Drawing.Rectangle(0, 0,
                imageWidth, imageWidth);

            using (Bitmap sourceImage = srcBitmap)
            using (Bitmap croppedImage = sourceImage.Clone(cropRect, sourceImage.PixelFormat))
            using (TextureBrush tb = new TextureBrush(croppedImage))
            using (Graphics g = Graphics.FromImage(finalImage))
            {
                g.FillEllipse(tb, 0, 0, imageWidth, imageWidth);
                Pen p = new Pen(System.Drawing.Color.Black, 10) { Alignment = PenAlignment.Inset };
                g.DrawEllipse(p, 0, 0, imageWidth, imageWidth);
            }
            return finalImage;
        }

        private RecallDetector.RecallInfo GetRecall(int networkId)
        {
            var t = Menu.RecallDetector.GetMenuItem("SAwarenessRecallDetectorMode").GetValue<StringList>();
            if (t.SelectedIndex == 1 || t.SelectedIndex == 2)
            {
                var recallDetector = (RecallDetector) Menu.RecallDetector.Item;
                if (recallDetector == null)
                    return null;
                foreach (RecallDetector.RecallInfo info in recallDetector.Recalls)
                {
                    if (info.NetworkId == networkId)
                    {
                        return info;
                    }
                }
            }
            return null;
        }
    }

    internal class WaypointTracker
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
            return Menu.Tracker.GetActive() && Menu.WaypointTracker.GetActive();
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                float arrivalTime = 0.0f;
                if (enemy.IsValid && enemy.IsVisible && !enemy.IsDead && enemy.IsEnemy)
                {
                    List<Vector2> waypoints = enemy.GetWaypoints();
                    for (int i = 0; i < waypoints.Count - 1; i++)
                    {
                        Vector2 oWp;
                        Vector2 nWp;
                        float time = 0;
                        oWp = Drawing.WorldToScreen(waypoints[i].To3D());
                        nWp = Drawing.WorldToScreen(waypoints[i + 1].To3D());
                        Drawing.DrawLine(oWp[0], oWp[1], nWp[0], nWp[1], 1, Color.White);
                        time =
                            ((Vector3.Distance(waypoints[i].To3D(), waypoints[i + 1].To3D())/
                              (ObjectManager.Player.MoveSpeed/1000))/1000);
                        time = (float) Math.Round(time, 2);
                        arrivalTime += time;
                        if (i == enemy.Path.Length - 1)
                        {
                            DrawCross(nWp[0], nWp[1], 1.0f, 3.0f, Color.Red);
                            Drawing.DrawText(nWp[0] - 15, nWp[1] + 10, Color.Red, arrivalTime.ToString());
                        }
                    }
                }
            }
        }

        private void DrawCross(float x, float y, float size, float thickness, Color color)
        {
            var topLeft = new Vector2(x - 10*size, y - 10*size);
            var topRight = new Vector2(x + 10*size, y - 10*size);
            var botLeft = new Vector2(x - 10*size, y + 10*size);
            var botRight = new Vector2(x + 10*size, y + 10*size);

            Drawing.DrawLine(topLeft.X, topLeft.Y, botRight.X, botRight.Y, thickness, color);
            Drawing.DrawLine(topRight.X, topRight.Y, botLeft.X, botLeft.Y, thickness, color);
        }
    }

    internal class Killable //TODO: Add more option for e.g. most damage first, add ignite spell
    {
        Dictionary<Obj_AI_Hero, InternalKillable> _enemies = new Dictionary<Obj_AI_Hero, InternalKillable>();

        public Killable()
        {
            int index = 0;
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsEnemy)
                {
                    Render.Text text = new Render.Text(new Vector2(0,0), "", 28, SharpDX.Color.Red);
                    text.Centered = true;
                    text.OutLined = true;
                    text.VisibleCondition = sender =>
                    {
                        return CalculateKillable(enemy).Killable && enemy.IsVisible && !enemy.IsDead;
                    };
                    text.PositionUpdate = delegate
                    {
                        return new Vector2(Drawing.Width / 2, Drawing.Height * 0.80f - (17 * index));
                    };
                    text.TextUpdate = delegate
                    {
                        String killText = "Killable " + enemy.ChampionName + ": ";
                        if (CalculateKillable(enemy).Spells != null && CalculateKillable(enemy).Spells.Count > 0)
                            CalculateKillable(enemy).Spells.ForEach(x => killText += x.Name + "/");
                        if (CalculateKillable(enemy).Items != null && CalculateKillable(enemy).Items.Count > 0)
                            CalculateKillable(enemy).Items.ForEach(x => killText += x.Name + "/");
                        if (killText.Contains("/"))
                            killText = killText.Remove(killText.LastIndexOf("/"));
                        return killText;
                    };
                    InternalKillable killable = new InternalKillable(CalculateKillable(enemy), text);
                    _enemies.Add(enemy, killable);
                }
                index++;
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~Killable()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            _enemies = null;
        }

        public bool IsActive()
        {
            return Menu.Tracker.GetActive() && Menu.Killable.GetActive();
        }

        private void CalculateKillable()
        {
            foreach (var enemy in _enemies.ToArray())
            {
                _enemies[enemy.Key].Combo = (CalculateKillable(enemy.Key));
            }
        }

        private Combo CalculateKillable(Obj_AI_Hero enemy)
        {
            var creationItemList = new Dictionary<Item, Damage.DamageItems>();
            var creationSpellList = new List<LeagueSharp.Common.Spell>();
            var tempSpellList = new List<Spell>();
            var tempItemList = new List<Item>();

            var ignite = new LeagueSharp.Common.Spell(Activator.GetIgniteSlot(), 1000);

            var q = new LeagueSharp.Common.Spell(SpellSlot.Q, 1000);
            var w = new LeagueSharp.Common.Spell(SpellSlot.W, 1000);
            var e = new LeagueSharp.Common.Spell(SpellSlot.E, 1000);
            var r = new LeagueSharp.Common.Spell(SpellSlot.R, 1000);
            creationSpellList.Add(q);
            creationSpellList.Add(w);
            creationSpellList.Add(e);
            creationSpellList.Add(r);

            var dfg = new Item(3128, 1000, "Dfg");
            var bilgewater = new Item(3144, 1000, "Bilgewater");
            var hextechgun = new Item(3146, 1000, "Hextech");
            var blackfire = new Item(3188, 1000, "Blackfire");
            var botrk = new Item(3153, 1000, "Botrk");
            creationItemList.Add(dfg, Damage.DamageItems.Dfg);
            creationItemList.Add(bilgewater, Damage.DamageItems.Bilgewater);
            creationItemList.Add(hextechgun, Damage.DamageItems.Hexgun);
            creationItemList.Add(blackfire, Damage.DamageItems.BlackFireTorch);
            creationItemList.Add(botrk, Damage.DamageItems.Botrk);

            double enoughDmg = 0;
            double enoughMana = 0;

            foreach (var item in creationItemList)
            {
                if (item.Key.IsReady())
                {
                    enoughDmg += ObjectManager.Player.GetItemDamage(enemy, item.Value);
                    tempItemList.Add(item.Key);
                }
                if (enemy.Health < enoughDmg)
                {
                    return new Combo(null, tempItemList, true);
                }
            }

            foreach (LeagueSharp.Common.Spell spell in creationSpellList)
            {
                if (spell.IsReady())
                {
                    double spellDamage = spell.GetDamage(enemy, 0);
                    if (spellDamage > 0)
                    {
                        enoughDmg += spellDamage;
                        enoughMana += spell.Instance.ManaCost;
                        tempSpellList.Add(new Spell(spell.Slot.ToString(), spell.Slot));
                    }
                }
                if (enemy.Health < enoughDmg)
                {
                    if (ObjectManager.Player.Mana >= enoughMana)
                        return new Combo(tempSpellList, tempItemList, true);
                    return new Combo(null, null, false);
                }
            }

            if (Activator.GetIgniteSlot() != SpellSlot.Unknown && enemy.Health > enoughDmg)
            {
                enoughDmg += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
                tempSpellList.Add(new Spell("Ignite", ignite.Slot));
            }
            if (enemy.Health < enoughDmg)
            {
                return new Combo(tempSpellList, tempItemList, true);
            }

            return new Combo();
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            CalculateKillable();
        }

        public class InternalKillable
        {
            public Combo Combo;
            public Render.Text Text;

            public InternalKillable(Combo combo, Render.Text text)
            {
                Combo = combo;
                Text = text;
            }
        }

        public class Combo
        {
            public List<Item> Items = new List<Item>();

            public bool Killable = false;
            public List<Spell> Spells = new List<Spell>();


            public Combo(List<Spell> spells, List<Item> items, bool killable)
            {
                Spells = spells;
                Items = items;
                Killable = killable;
            }

            public Combo()
            {
            }
        }

        public class Item : Items.Item
        {
            public String Name;

            public Item(int id, float range, String name) : base(id, range)
            {
                Name = name;
            }
        }

        public class Spell
        {
            public String Name;
            public SpellSlot SpellSlot;

            public Spell(String name, SpellSlot spellSlot)
            {
                Name = name;
                SpellSlot = spellSlot;
            }
        }
    }
}
