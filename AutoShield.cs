using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;

namespace SAwareness
{
    class AutoShield
    {
        public AutoShield()
        {
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

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;

            
        }
    }
}
