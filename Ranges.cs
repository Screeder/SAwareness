﻿using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness
{
    internal class Ranges
    {
        public Ranges()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~Ranges()
        {
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        public bool IsActive()
        {
            return Menu.Range.GetActive();
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            try
            {
                DrawExperienceRanges();
                DrawAttackRanges();
                DrawTurretRanges();
                DrawQ();
                DrawW();
                DrawE();
                DrawR();
            }
            catch (Exception ex)
            {
                Console.WriteLine("SAwareness: " + ex);
                throw;
            }
        }

        public void DrawExperienceRanges()
        {
            if (!Menu.ExperienceRange.GetActive())
                return;
            var mode = Menu.ExperienceRange.GetMenuItem("SAwarenessExperienceRangeMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    Utility.DrawCircle(ObjectManager.Player.Position, 1400, Color.LawnGreen);
                    break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, 1400, Color.IndianRed);
                        }
                    }
                    break;
                case 2:
                    Utility.DrawCircle(ObjectManager.Player.Position, 1400, Color.LawnGreen);
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, 1400, Color.IndianRed);
                        }
                    }
                    break;
            }
        }

        public void DrawAttackRanges()
        {
            if (!Menu.AttackRange.GetActive())
                return;
            var mode = Menu.AttackRange.GetMenuItem("SAwarenessAttackRangeMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    Utility.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.AttackRange, Color.LawnGreen);
                    break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, enemy.AttackRange, Color.IndianRed);
                        }
                    }
                    break;
                case 2:
                    Utility.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.AttackRange, Color.LawnGreen);
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, enemy.AttackRange, Color.IndianRed);
                        }
                    }
                    break;
            }
        }

        public void DrawTurretRanges()
        {
            if (!Menu.TowerRange.GetActive())
                return;
            var mode = Menu.TowerRange.GetMenuItem("SAwarenessTowerRangeMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret.IsVisible && !turret.IsDead && !turret.IsEnemy && turret.IsValid &&
                            Common.IsOnScreen(turret.ServerPosition))
                        {
                            Utility.DrawCircle(turret.Position, 900f, Color.LawnGreen);
                        }
                    }
                    break;
                case 1:
                    foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret.IsVisible && !turret.IsDead && turret.IsEnemy && turret.IsValid &&
                            Common.IsOnScreen(turret.ServerPosition))
                        {
                            Utility.DrawCircle(turret.Position, 900f, Color.DarkRed);
                        }
                    }
                    break;
                case 2:
                    foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret.IsVisible && !turret.IsDead && !turret.IsEnemy && turret.IsValid &&
                            Common.IsOnScreen(turret.ServerPosition))
                        {
                            Utility.DrawCircle(turret.Position, 900f, Color.LawnGreen);
                        }
                    }
                    foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret.IsVisible && !turret.IsDead && turret.IsEnemy && turret.IsValid &&
                            Common.IsOnScreen(turret.ServerPosition))
                        {
                            Utility.DrawCircle(turret.Position, 900f, Color.DarkRed);
                        }
                    }
                    break;
            }
        }

        public void DrawQ()
        {
            if (!Menu.SpellQRange.GetActive())
                return;
            var mode = Menu.SpellQRange.GetMenuItem("SAwarenessSpellQRangeMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    Utility.DrawCircle(ObjectManager.Player.Position,
                        ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange[0], Color.LawnGreen);
                    break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange[0], Color.IndianRed);
                        }
                    }
                    break;
                case 2:
                    Utility.DrawCircle(ObjectManager.Player.Position,
                        ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange[0], Color.LawnGreen);
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange[0], Color.IndianRed);
                        }
                    }
                    break;
            }
        }

        public void DrawW()
        {
            if (!Menu.SpellWRange.GetActive())
                return;
            var mode = Menu.SpellWRange.GetMenuItem("SAwarenessSpellWRangeMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    Utility.DrawCircle(ObjectManager.Player.Position,
                        ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRange[0], Color.LawnGreen);
                    break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRange[0], Color.IndianRed);
                        }
                    }
                    break;
                case 2:
                    Utility.DrawCircle(ObjectManager.Player.Position,
                        ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRange[0], Color.LawnGreen);
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRange[0], Color.IndianRed);
                        }
                    }
                    break;
            }
        }

        public void DrawE()
        {
            if (!Menu.SpellERange.GetActive())
                return;
            var mode = Menu.SpellERange.GetMenuItem("SAwarenessSpellERangeMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    Utility.DrawCircle(ObjectManager.Player.Position,
                        ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange[0], Color.LawnGreen);
                    break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange[0], Color.IndianRed);
                        }
                    }
                    break;
                case 2:
                    Utility.DrawCircle(ObjectManager.Player.Position,
                        ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange[0], Color.LawnGreen);
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange[0], Color.IndianRed);
                        }
                    }
                    break;
            }
        }

        public void DrawR()
        {
            if (!Menu.SpellRRange.GetActive())
                return;
            var mode = Menu.SpellRRange.GetMenuItem("SAwarenessSpellRRangeMode").GetValue<StringList>();
            switch (mode.SelectedIndex)
            {
                case 0:
                    Utility.DrawCircle(ObjectManager.Player.Position,
                        ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange[0], Color.LawnGreen);
                    break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange[0], Color.IndianRed);
                        }
                    }
                    break;
                case 2:
                    Utility.DrawCircle(ObjectManager.Player.Position,
                        ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange[0], Color.LawnGreen);
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position,
                                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange[0], Color.IndianRed);
                        }
                    }
                    break;
            }
        }
    }
}