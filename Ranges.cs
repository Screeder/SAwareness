using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness
{
    class Ranges
    {

        StringList mode = new StringList();
        
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

        void Drawing_OnDraw(EventArgs args)
        {
            if (!IsActive())
                return;
            mode = Menu.Range.GetMenuItem("SAwarenessRangesMode").GetValue<StringList>();
            DrawExperienceRanges();
            DrawAttackRanges();
            DrawTurretRanges();
            DrawQ();
            DrawW();
            DrawE();
            DrawR();
        }

        public void DrawExperienceRanges()
        {
            if (!Menu.ExperienceRange.GetActive())
                return;
            switch (mode.SelectedIndex)
            {
                case 0:
                    Utility.DrawCircle(ObjectManager.Player.Position, 1400, System.Drawing.Color.LawnGreen);
                break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (!enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, 1400, System.Drawing.Color.IndianRed);
                        }
                    }
                break;
                case 2:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, 1400, System.Drawing.Color.IndianRed);
                        }
                    }
                break;
            }
        }

        public void DrawAttackRanges()
        {
            if (!Menu.AttackRange.GetActive())
                return;
            switch (mode.SelectedIndex)
            {
                case 0:
                    Utility.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.AttackRange, System.Drawing.Color.LawnGreen);
                break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (!enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, enemy.AttackRange, System.Drawing.Color.IndianRed);
                        }
                    }
                break;
                case 2:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, enemy.AttackRange, System.Drawing.Color.IndianRed);
                        }
                    }
                break;
            }
        }

        public void DrawTurretRanges()
        {
            if (!Menu.TowerRange.GetActive())
                return;
            switch (mode.SelectedIndex)
            {
                case 0:
                case 1:
                    foreach (Obj_AI_Turret Turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (Turret.IsVisible && !Turret.IsDead && !Turret.IsEnemy && Turret.IsValid && Common.IsOnScreen(Turret.ServerPosition))
                        {
                            Utility.DrawCircle(Turret.Position, 900f, System.Drawing.Color.LawnGreen);
                        }
                    }
                break;
                case 2:
                    foreach (Obj_AI_Turret Turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (Turret.IsVisible && !Turret.IsDead && Turret.IsEnemy && Turret.IsValid && Common.IsOnScreen(Turret.ServerPosition))
                        {
                            Utility.DrawCircle(Turret.Position, 900f, System.Drawing.Color.DarkRed);
                        }
                    }
                break;
            }
        }

        public void DrawQ()
        {
            if (!Menu.SpellQRange.GetActive())
                return;
            switch (mode.SelectedIndex)
            {
                case 0:
                    Utility.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange[0], System.Drawing.Color.LawnGreen);
                break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (!enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange[0], System.Drawing.Color.IndianRed);
                        }
                    }
                break;
                case 2:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange[0], System.Drawing.Color.IndianRed);
                        }
                    }
                break;
            }
        }

        public void DrawW()
        {
            if (!Menu.SpellWRange.GetActive())
                return;
            switch (mode.SelectedIndex)
            {
                case 0:
                    Utility.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRange[0], System.Drawing.Color.LawnGreen);
                break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (!enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRange[0], System.Drawing.Color.IndianRed);
                        }
                    }
                break;
                case 2:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.CastRange[0], System.Drawing.Color.IndianRed);
                        }
                    }
                break;
            }
        }

        public void DrawE()
        {
            if (!Menu.SpellERange.GetActive())
                return;
            switch (mode.SelectedIndex)
            {
                case 0:
                    Utility.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange[0], System.Drawing.Color.LawnGreen);
                break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (!enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange[0], System.Drawing.Color.IndianRed);
                        }
                    }
                break;
                case 2:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange[0], System.Drawing.Color.IndianRed);
                        }
                    }
                break;
            }
        }

        public void DrawR()
        {
            if (!Menu.SpellRRange.GetActive())
                return;
            switch (mode.SelectedIndex)
            {
                case 0:
                    Utility.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange[0], System.Drawing.Color.LawnGreen);
                break;
                case 1:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (!enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange[0], System.Drawing.Color.IndianRed);
                        }
                    }
                break;
                case 2:
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange[0], System.Drawing.Color.IndianRed);
                        }
                    }
                break;
            }
        }
    }
}
