using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness
{
    class MoveToMouse
    {
         public MoveToMouse()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

         ~MoveToMouse()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Menu.Misc.GetActive() && Menu.MoveToMouse.GetActive();
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || !Menu.MoveToMouse.GetMenuItem("SAwarenessMoveToMouseKey").GetValue<KeyBind>().Active)
                return;

            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
        }
    }
}
