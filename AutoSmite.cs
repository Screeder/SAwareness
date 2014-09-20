using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness
{
    class AutoSmite
    {
        private String[] monsters = { "GreatWraith", "Wraith", "AncientGolem", "GiantWolf", "LizardElder", "Golem", "Worm", "Dragon", "Wight", "TT_Spiderboss" };
        private String[] usefulMonsters = { "AncientGolem", "LizardElder", "Worm", "Dragon", "TT_Spiderboss" };

        public AutoSmite()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        ~AutoSmite()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Drawing.OnDraw -= Drawing_OnDraw;
        }

        void Drawing_OnDraw(EventArgs args)
        {
            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
            {
                int smiteDamage = GetSmiteDamage();
                if (Vector3.Distance(ObjectManager.Player.ServerPosition, minion.ServerPosition) < 1500)
                {
                    foreach (var monster in monsters)
                    {
                        if (minion.SkinName == monster && minion.IsVisible)
                        {
                            Vector2 pos = Drawing.WorldToScreen(minion.ServerPosition);
                            String health = minion.Health != 0 ? (((int) minion.Health - smiteDamage)).ToString() : "";
                            health =
                                !Menu.AutoSmite.GetMenuItem("SAwarenessAutoSmiteKeyActive")
                                    .GetValue<KeyBind>()
                                    .Active
                                    ? health + " Disabled"
                                    : health;
                            Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.SkyBlue, health);
                        }
                    }
                }
            }
        }

        public bool IsActive()
        {
            return Menu.Activator.GetActive() && Menu.AutoSmite.GetActive();
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive() || !Menu.AutoSmite.GetMenuItem("SAwarenessAutoSmiteKeyActive").GetValue<KeyBind>().Active)
                return;
            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
            {
                List<Obj_AI_Minion> min = ObjectManager.Get<Obj_AI_Minion>().ToList();
                if (Vector3.Distance(ObjectManager.Player.ServerPosition, minion.ServerPosition) < 750)
                {
                    int smiteDamage = GetSmiteDamage();
                    if (minion.Health <= smiteDamage && minion.Health > 0)
                    {
                        if (!Menu.AutoSmite.GetMenuItem("SAwarenessAutoSmiteSmallCampsActive").GetValue<bool>())
                        {
                            foreach (var monster in usefulMonsters)
                            {
                                if (minion.SkinName == monster && minion.IsVisible)
                                {
                                    SmiteIt(minion.NetworkId);
                                }
                            }
                        }
                        else
                        {
                            foreach (var monster in monsters)
                            {
                                if (minion.SkinName == monster && minion.IsVisible)
                                {
                                    SmiteIt(minion.NetworkId);
                                }
                            }
                        }                        
                    }
                }
            }
        }

        private void SmiteIt(int networkId)
        {
            SpellSlot spellSlot = GetSmiteSlot();
            int slot = -1;
            if (spellSlot == SpellSlot.Q)
                slot = 64;
            else if (spellSlot == SpellSlot.W)
                slot = 65;
            if (slot != -1)
            {
                GamePacket gPacketT = Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(networkId, (SpellSlot)slot));
                gPacketT.Send();
            }
        }

        private int GetSmiteDamage()
        {
            int level = ObjectManager.Player.Level;
            int smiteDamage = 390 +
                                (level < 5 ? 20 * (level - 1) :
                                (level < 10 ? 60 + 30 * (level - 4) :
                                (level < 15 ? 210 + 40 * (level - 9) :
                                              410 + 50 * (level - 14))));
            return smiteDamage;
        }

        private SpellSlot GetSmiteSlot()
        {
            foreach (var spell in ObjectManager.Player.SummonerSpellbook.Spells)
            {
                if (spell.Name.ToLower().Contains("smite") && spell.State == SpellState.Ready)
                    return spell.Slot;
            }
            return SpellSlot.Unknown;
        }
    }
}
