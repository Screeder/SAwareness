using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Evade;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness
{
    class AutoShield
    {

        class Shield
        {
            public bool OnlyMagic;
            public bool Instant;
            public bool Skillshot;
            public Spell Spell;

            public Shield(Spell spell, bool instant = true, bool skillshot = false, bool onlyMagic = false)
            {
                Spell = spell;
                Instant = instant;
                Skillshot = skillshot;
                OnlyMagic = onlyMagic;
            }
        }

        class Spell : LeagueSharp.Common.Spell
        {
            public Spell(SpellSlot slot, float range, float delay = 0, float width = 0, float speed = 0) : base(slot, range)
            {
                this.SetSkillshot(delay, width, speed, false, SkillshotType.SkillshotLine);
            }
        }

        private Shield shield;

        public AutoShield()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Janna":
                    shield = new Shield(new Spell(SpellSlot.E, 900, 0, 0, 0));
                    break;

                case "Morgana":
                    shield = new Shield(new Spell(SpellSlot.E, 850, 0, 0, 0), true, false, true);
                    break;

                case "Lux":
                    shield = new Shield(new Spell(SpellSlot.W, 1175, 0.5f, 150, 1200), false, true);
                    break;

                case "Orianna":
                    shield = new Shield(new Spell(SpellSlot.E, 1295, 0.5f, 0, 1200), false);
                    break;

                case "Karma":
                    shield = new Shield(new Spell(SpellSlot.E, 900, 0, 0, 0));
                    break;

                case "Lulu":
                    shield = new Shield(new Spell(SpellSlot.E, 750, 0, 0, 0));
                    break;

                default:
                    return;
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoShield()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Menu.Activator.GetActive() && Menu.AutoShield.GetActive();
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;

            Dictionary<Obj_AI_Hero, List<Activator.IncomingDamage>> tempDamages =
                new Dictionary<Obj_AI_Hero, List<Activator.IncomingDamage>>(Activator.damages);
            foreach (KeyValuePair<Obj_AI_Hero, List<Activator.IncomingDamage>> damage in Activator.damages)
            {
                Obj_AI_Hero hero = damage.Key;

                foreach (var tDamage in tempDamages[hero].ToArray())
                {
                    foreach (var spell in Database.GetSpellList())
                    {
                        if (spell.Name.Contains(tDamage.SpellName))
                        {
                            if (shield.OnlyMagic)
                            {
                                if (!IsDamageType((Obj_AI_Hero)tDamage.Source, tDamage.SpellName, Damage.DamageType.Magical))
                                {
                                    tempDamages[hero].Remove(tDamage);
                                    continue;
                                }
                                if (
                                    Menu.AutoShield.GetMenuItem("SAwarenessAutoShieldBlockCC")
                                        .GetValue<bool>() &&
                                    !ContainsCC(tDamage.SpellName))
                                {
                                    tempDamages[hero].Remove(tDamage);
                                    continue;
                                }
                            }
                            if (!CheckDamagelevel(tDamage.SpellName) && !shield.OnlyMagic)
                            {
                                tempDamages[hero].Remove(tDamage);
                                continue;
                            }                            
                        }
                        if (!Menu.AutoShield.GetMenuItem("SAwarenessAutoShieldBlockAA")
                                        .GetValue<bool>() && IsAutoAttack(tDamage.SpellName))
                        {
                            tempDamages[hero].Remove(tDamage);
                            continue;
                        }
                    }
                }
            }

            foreach (var damage in tempDamages)
            {
                Vector2 d2 = Drawing.WorldToScreen(damage.Key.ServerPosition);
                Drawing.DrawText(d2.X, d2.Y, System.Drawing.Color.Aquamarine, Activator.CalcMaxDamage(damage.Key).ToString());

                if (Activator.CalcMaxDamage(damage.Key) > 0 && damage.Key.Distance(ObjectManager.Player.ServerPosition) < shield.Spell.Range)
                {
                    if (shield.Skillshot)
                    {
                        PredictionOutput predOutput = shield.Spell.GetPrediction(damage.Key);
                        if (predOutput.Hitchance > HitChance.Medium)
                            ObjectManager.Player.Spellbook.CastSpell(shield.Spell.Slot, predOutput.CastPosition);
                        break;
                    }
                    if (shield.Instant)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(shield.Spell.Slot, damage.Key);
                        break;
                    }
                }
            }
        }

        private static bool ContainsCC(String spellName)
        {
            foreach (var spell in Database.GetSpellList())
            {
                if (spellName.Contains(spell.Name))
                {
                    if (spell.CCType != Database.Spell.CCtype.NoCC)
                        return true;
                }
            }
            return false;
        }

        private static bool CheckDamagelevel(String spellName)
        {
            foreach (var spell in Database.GetSpellList())
            {
                if (spell.Name.Contains(spellName))
                {
                    if (Menu.AutoShield.GetMenuItem("SAwarenessAutoShieldBlockDamageAmount").GetValue<StringList>().SelectedIndex == 0)
                    {
                        if (spell.Damagelvl == Database.Spell.DamageLevel.Medium ||
                            spell.Damagelvl == Database.Spell.DamageLevel.High ||
                            spell.Damagelvl == Database.Spell.DamageLevel.Extrem)
                            return true;
                    }
                    else if (Menu.AutoShield.GetMenuItem("SAwarenessAutoShieldBlockDamageAmount").GetValue<StringList>().SelectedIndex == 1)
                    {
                        if (spell.Damagelvl == Database.Spell.DamageLevel.High ||
                            spell.Damagelvl == Database.Spell.DamageLevel.Extrem)
                            return true;
                    }
                    if (Menu.AutoShield.GetMenuItem("SAwarenessAutoShieldBlockDamageAmount").GetValue<StringList>().SelectedIndex == 2)
                    {
                        if (spell.Damagelvl == Database.Spell.DamageLevel.Extrem)
                            return true;
                    }
                }
            }
            return false;
        }

        private static bool IsAutoAttack(String spellName)
        {
            if(spellName.ToLower().Contains("attack"))
                return true;
            return false;
        }

        private static bool IsDamageType(Obj_AI_Hero hero, String spellName, Damage.DamageType damageType)
        {
            DamageSpell damageSpell = null;
            foreach (SpellDataInst spellDataInst in hero.Spellbook.Spells)
            {
                if (string.Equals(spellDataInst.Name, spellName,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    damageSpell = Enumerable.FirstOrDefault<DamageSpell>((IEnumerable<DamageSpell>)Damage.Spells[hero.ChampionName], (Func<DamageSpell, bool>)(s =>
                    {
                        if (s.Slot == spellDataInst.Slot)
                            return 0 == s.Stage;
                        else
                            return false;
                    })) ?? Enumerable.FirstOrDefault<DamageSpell>((IEnumerable<DamageSpell>)Damage.Spells[hero.ChampionName], (Func<DamageSpell, bool>)(s => s.Slot == spellDataInst.Slot));
                    if (damageSpell != null)
                        break;
                }
            }
            if (damageSpell == null || damageSpell.DamageType != damageType)
                return false;
            return true;
        }
       
    }
}
