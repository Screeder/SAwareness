using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace SAwareness
{
    class Health
    {
        Font font;
        private bool drawActive = true;

        public Health()
        {
            try
            {
                font = new Font(Drawing.Direct3DDevice, new System.Drawing.Font("Times New Roman", 8));
            }
            catch (Exception)
            {
                Menu.Health.ForceDisable = true;
                Console.WriteLine("Health: Cannot create Font");
                return;
            }
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += delegate { Drawing_OnPreReset(new EventArgs()); };
            AppDomain.CurrentDomain.ProcessExit += delegate { Drawing_OnPreReset(new EventArgs()); };
        }

        void Drawing_OnPostReset(EventArgs args)
        {
            if (Drawing.Direct3DDevice == null || Drawing.Direct3DDevice.IsDisposed)
                return;
            font.OnResetDevice();
            drawActive = true;
        }

        void Drawing_OnPreReset(EventArgs args)
        {
            font.OnLostDevice();
            drawActive = false;
        }

        void Drawing_OnEndScene(EventArgs args)
        {
            DrawTurrentHealth();
            DrawInhibitorHealth();
        }

        ~Health()
        {
            Drawing.OnPreReset -= Drawing_OnPreReset;
            Drawing.OnPostReset -= Drawing_OnPostReset;
            Drawing.OnEndScene -= Drawing_OnEndScene;
        }

        public bool IsActive()
        {
            return Menu.Health.GetActive();
        }

        private void DrawInhibitorHealth()
        {
            if (!IsActive() || !drawActive)
                return;
            if (!Menu.InhibitorHealth.GetActive())
                return;
            List<Obj_Barracks> _baseB = new List<Obj_Barracks>();
            List<Obj_BarracksDampener> _baseBD = ObjectManager.Get<Obj_BarracksDampener>().ToList();

            foreach (var inhibitor in _baseB)
            {
                if (!inhibitor.IsDead && inhibitor.IsValid && inhibitor.Health > 0.1f)
                {
                    Vector2 pos = Drawing.WorldToMinimap(inhibitor.Position);
                    int health = 0;
                    StringList mode =
                        Menu.Health.GetMenuItem("SAwarenessHealthMode")
                            .GetValue<StringList>();
                    switch (mode.SelectedIndex)
                    {
                        case 0:
                            health = (int)((inhibitor.Health / inhibitor.MaxHealth) * 100);
                            break;

                        case 1:
                            health = (int)inhibitor.Health;
                            break;
                    }
                    if (((inhibitor.Health / inhibitor.MaxHealth) * 100) > 75)
                        DirectXDrawer.DrawText(font, health.ToString(), (int)pos[0], (int)pos[1], Color.LightGreen);
                    else if (((inhibitor.Health / inhibitor.MaxHealth) * 100) <= 75)
                        DirectXDrawer.DrawText(font, health.ToString(), (int)pos[0], (int)pos[1], Color.LightYellow);
                    else if (((inhibitor.Health / inhibitor.MaxHealth) * 100) <= 50)
                        DirectXDrawer.DrawText(font, health.ToString(), (int)pos[0], (int)pos[1], Color.Orange);
                    else if (((inhibitor.Health / inhibitor.MaxHealth) * 100) <= 25)
                        DirectXDrawer.DrawText(font, health.ToString(), (int)pos[0], (int)pos[1], Color.IndianRed);
                    //Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.Green, ((int)turret.Health).ToString());
                    //Drawing.DrawText(turret.HealthBarPosition.X, turret.HealthBarPosition.Y + 20, System.Drawing.Color.Green, ((int)turret.Health).ToString());
                }
            }

            foreach (var inhibitor in _baseBD)
            {
                if (!inhibitor.IsDead && inhibitor.IsValid && inhibitor.Health > 0.1f)
                {
                    Vector2 pos = Drawing.WorldToMinimap(inhibitor.Position);
                    int health = 0;
                    StringList mode =
                        Menu.Health.GetMenuItem("SAwarenessHealthMode")
                            .GetValue<StringList>();
                    switch (mode.SelectedIndex)
                    {
                        case 0:
                            health = (int)((inhibitor.Health / inhibitor.MaxHealth) * 100);
                            break;

                        case 1:
                            health = (int)inhibitor.Health;
                            break;
                    }
                    if (((inhibitor.Health / inhibitor.MaxHealth) * 100) > 75)
                        DirectXDrawer.DrawText(font, health.ToString(), (int)pos[0], (int)pos[1], Color.LightGreen);
                    else if (((inhibitor.Health / inhibitor.MaxHealth) * 100) <= 75)
                        DirectXDrawer.DrawText(font, health.ToString(), (int)pos[0], (int)pos[1], Color.LightYellow);
                    else if (((inhibitor.Health / inhibitor.MaxHealth) * 100) <= 50)
                        DirectXDrawer.DrawText(font, health.ToString(), (int)pos[0], (int)pos[1], Color.Orange);
                    else if (((inhibitor.Health / inhibitor.MaxHealth) * 100) <= 25)
                        DirectXDrawer.DrawText(font, health.ToString(), (int)pos[0], (int)pos[1], Color.IndianRed);
                    //Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.Green, ((int)turret.Health).ToString());
                    //Drawing.DrawText(turret.HealthBarPosition.X, turret.HealthBarPosition.Y + 20, System.Drawing.Color.Green, ((int)turret.Health).ToString());
                }
            }
        }

        private void DrawTurrentHealth() //TODO: Draw HP above BarPos
        {
            if (!IsActive())
                return;
            if (!Menu.TowerHealth.GetActive())
                return;
            foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
            {
                if (!turret.IsDead && turret.IsValid && turret.Health != 9999)
                {
                    Vector2 pos = Drawing.WorldToMinimap(turret.Position);
                    int health = 0;
                    StringList mode =
                        Menu.Health.GetMenuItem("SAwarenessHealthMode")
                            .GetValue<StringList>();
                    switch (mode.SelectedIndex)
                    {
                        case 0:
                            health = (int)((turret.Health / turret.MaxHealth) * 100);
                            break;

                        case 1:
                            health = (int)turret.Health;
                            break;
                    }
                    if (((turret.Health / turret.MaxHealth) * 100) > 75)
                        DirectXDrawer.DrawText(font, health.ToString(), (int)pos[0], (int)pos[1], Color.LightGreen);
                    else if (((turret.Health / turret.MaxHealth) * 100) <= 75)
                        DirectXDrawer.DrawText(font, health.ToString(), (int)pos[0], (int)pos[1], Color.LightYellow);
                    else if (((turret.Health / turret.MaxHealth) * 100) <= 50)
                        DirectXDrawer.DrawText(font, health.ToString(), (int)pos[0], (int)pos[1], Color.Orange);
                    else if (((turret.Health / turret.MaxHealth) * 100) <= 25)
                        DirectXDrawer.DrawText(font, health.ToString(), (int)pos[0], (int)pos[1], Color.IndianRed);
                    //Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.Green, ((int)turret.Health).ToString());
                    //Drawing.DrawText(turret.HealthBarPosition.X, turret.HealthBarPosition.Y + 20, System.Drawing.Color.Green, ((int)turret.Health).ToString());
                }
            }
        }
    }
}
