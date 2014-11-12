using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness
{
    internal class AutoPot
    {
        private readonly List<Pot> _pots = new List<Pot>();

        public AutoPot()
        {
            _pots.Add(new Pot(2037, "PotionOfGiantStrengt", Pot.PotType.Health)); //elixirOfFortitude
            _pots.Add(new Pot(2039, "PotionOfBrilliance", Pot.PotType.Mana)); //elixirOfBrilliance            
            _pots.Add(new Pot(2041, "ItemCrystalFlask", Pot.PotType.Both)); //crystalFlask
            _pots.Add(new Pot(2009, "ItemMiniRegenPotion", Pot.PotType.Both)); //biscuit
            _pots.Add(new Pot(2010, "ItemMiniRegenPotion", Pot.PotType.Both)); //biscuit
            _pots.Add(new Pot(2003, "RegenerationPotion", Pot.PotType.Health)); //healthPotion
            _pots.Add(new Pot(2004, "FlaskOfCrystalWater", Pot.PotType.Mana)); //manaPotion
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoPot()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Menu.Activator.GetActive() && Menu.AutoPot.GetActive();
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || ObjectManager.Player.IsDead || Utility.InFountain() ||
                ObjectManager.Player.HasBuff("Recall") || Utility.CountEnemysInRange(1500) > 0)
                return;
            Pot myPot = null;
            if (
                Menu.AutoPot.GetMenuSettings("SAwarenessAutoPotHealthPot")
                    .GetMenuItem("SAwarenessAutoPotHealthPotActive")
                    .GetValue<bool>())
            {
                foreach (Pot pot in _pots)
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
                foreach (Pot pot in _pots)
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
            if (myPot != null)
                UsePot(myPot);
        }

        private void UsePot(Pot pot)
        {
            foreach (BuffInstance buff in ObjectManager.Player.Buffs)
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

        public class Pot
        {
            public enum PotType
            {
                None,
                Health,
                Mana,
                Both
            }

            public String Buff;
            public int Id;
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
        }
    }
}