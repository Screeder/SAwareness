using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SAwareness
{
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
            return Menu.Activator.GetActive() && Menu.AutoPot.GetActive();
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || ObjectManager.Player.IsDead || Utility.InFountain() || ObjectManager.Player.HasBuff("Recall"))
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
                        if (ObjectManager.Player.Health / ObjectManager.Player.MaxHealth * 100 <=
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
                        if (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana * 100 <=
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
}
