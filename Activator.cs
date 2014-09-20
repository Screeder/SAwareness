using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SAwareness
{
    class Activator
    {
        private float LastItemCleanseUse = 0;
        List<BuffType> buffs = new List<BuffType>();

        public Activator()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }      

        void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            UseOffensiveItems_OnProcessSpellCast(sender, args);
        }

        ~Activator()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
            Obj_AI_Hero.OnProcessSpellCast -= Obj_AI_Hero_OnProcessSpellCast;
        }

        public bool IsActive()
        {
            return Menu.Activator.GetActive();
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            UseOffensiveItems_OnGameUpdate();
            UseDefensiveItems_OnGameUpdate();
            UseSummonerSpells_OnGameUpdate();
        }

        private void UseDefensiveItems_OnGameUpdate()
        {
            if (!Menu.ActivatorDefensive.GetActive())
                return;

            buffs.Clear();
            if (Menu.ActivatorDefensiveCleanseConfig.GetMenuItem("SAwarenessActivatorDefensiveCleanseConfigStun").GetValue<bool>())
                buffs.Add(BuffType.Stun);
            if (Menu.ActivatorDefensiveCleanseConfig.GetMenuItem("SAwarenessActivatorDefensiveCleanseConfigSilence").GetValue<bool>())
                buffs.Add(BuffType.Silence);
            if (Menu.ActivatorDefensiveCleanseConfig.GetMenuItem("SAwarenessActivatorDefensiveCleanseConfigTaunt").GetValue<bool>())
                buffs.Add(BuffType.Taunt);
            if (Menu.ActivatorDefensiveCleanseConfig.GetMenuItem("SAwarenessActivatorDefensiveCleanseConfigFear").GetValue<bool>())
                buffs.Add(BuffType.Fear);
            if (Menu.ActivatorDefensiveCleanseConfig.GetMenuItem("SAwarenessActivatorDefensiveCleanseConfigCharm").GetValue<bool>())
                buffs.Add(BuffType.Charm);
            if (Menu.ActivatorDefensiveCleanseConfig.GetMenuItem("SAwarenessActivatorDefensiveCleanseConfigBlind").GetValue<bool>())
                buffs.Add(BuffType.Blind);
            if (Menu.ActivatorDefensiveCleanseConfig.GetMenuItem("SAwarenessActivatorDefensiveCleanseConfigDisarm").GetValue<bool>())
                buffs.Add(BuffType.Disarm);
            if (Menu.ActivatorDefensiveCleanseConfig.GetMenuItem("SAwarenessActivatorDefensiveCleanseConfigSuppress").GetValue<bool>())
                buffs.Add(BuffType.Suppression);
            if (Menu.ActivatorDefensiveCleanseConfig.GetMenuItem("SAwarenessActivatorDefensiveCleanseConfigSlow").GetValue<bool>())
                buffs.Add(BuffType.Slow);
            if (Menu.ActivatorDefensiveCleanseConfig.GetMenuItem("SAwarenessActivatorDefensiveCleanseConfigCombatDehancer").GetValue<bool>())
                buffs.Add(BuffType.CombatDehancer);
            if (Menu.ActivatorDefensiveCleanseConfig.GetMenuItem("SAwarenessActivatorDefensiveCleanseConfigSnare").GetValue<bool>())
                buffs.Add(BuffType.Snare);
            if (Menu.ActivatorDefensiveCleanseConfig.GetMenuItem("SAwarenessActivatorDefensiveCleanseConfigPoison").GetValue<bool>())
                buffs.Add(BuffType.Poison);

            UseQSS();
            UseMS();
            UseDB();
            UseRanduins();
            UseFrostQueensClaim();
        }

        private void UseQSS()
        {
            if (!Menu.ActivatorDefensiveCleanseSelf.GetActive())
                return;

            List<BuffInstance> buffList = GetActiveCCBuffs();

            if (buffList.Count() >=
                Menu.ActivatorDefensiveCleanseSelf.GetMenuItem("SAwarenessActivatorDefensiveCleanseSelfConfigMinSpells").GetValue<Slider>().Value &&
                Menu.ActivatorDefensiveCleanseSelf.GetMenuItem("SAwarenessActivatorDefensiveCleanseSelfQSS").GetValue<bool>() &&
                LastItemCleanseUse + 1 < Game.Time)
            {
                Items.Item qss = new Items.Item(3140, 0);
                if (qss.IsReady())
                {
                    qss.Cast();
                    LastItemCleanseUse = Game.Time;
                }
            }
        }

        private void UseMS()
        {
            if (!Menu.ActivatorDefensiveCleanseSelf.GetActive())
                return;

            List<BuffInstance> buffList = GetActiveCCBuffs();

            if (buffList.Count() >=
                Menu.ActivatorDefensiveCleanseSelf.GetMenuItem("SAwarenessActivatorDefensiveCleanseSelfConfigMinSpells").GetValue<Slider>().Value &&
                Menu.ActivatorDefensiveCleanseSelf.GetMenuItem("SAwarenessActivatorDefensiveCleanseSelfMercurialScimitar").GetValue<bool>() &&
                LastItemCleanseUse + 1 < Game.Time)
            {
                Items.Item ms = new Items.Item(3139, 0);
                if (ms.IsReady())
                {
                    foreach (var instance in buffList)
                    {
                        Console.WriteLine(instance.Name);
                    }
                    ms.Cast();
                    LastItemCleanseUse = Game.Time;
                }
            }
        }

        private void UseDB()
        {
            if (!Menu.ActivatorDefensiveCleanseSelf.GetActive())
                return;

            List<BuffInstance> buffList = GetActiveCCBuffs();

            if (buffList.Count() >=
                Menu.ActivatorDefensiveCleanseSelf.GetMenuItem("SAwarenessActivatorDefensiveCleanseSelfConfigMinSpells").GetValue<Slider>().Value &&
                Menu.ActivatorDefensiveCleanseSelf.GetMenuItem("SAwarenessActivatorDefensiveCleanseSelfDervishBlade").GetValue<bool>() &&
                LastItemCleanseUse + 1 < Game.Time)
            {
                Items.Item db = new Items.Item(3137, 0);
                if (db.IsReady())
                {
                    db.Cast();
                    LastItemCleanseUse = Game.Time;
                }
            }
        }

        private void UseRanduins()
        {
            if (!Menu.ActivatorDefensiveDebuffSlow.GetActive())
                return;

            Obj_AI_Hero hero = GetHighestAdEnemy();
            int count = Utility.CountEnemysInRange(400);
            if (hero == null || !hero.IsValid || hero.IsDead)
                return;

            if (Menu.ActivatorDefensiveDebuffSlow.GetMenuItem("SAwarenessActivatorDefensiveDebuffSlowRanduins").GetValue<bool>() &&
                Menu.ActivatorDefensiveDebuffSlow.GetMenuItem("SAwarenessActivatorDefensiveDebuffSlowConfigRanduins").GetValue<Slider>().Value >= count &&
                ImFleeing(hero) || IsFleeing(hero) && !ImFleeing(hero))
            {
                Items.Item randuins = new Items.Item(3143, 0);
                if (randuins.IsReady())
                {
                    randuins.Cast();
                }
            }
        }

        private void UseFrostQueensClaim()
        {
            if (!Menu.ActivatorDefensiveDebuffSlow.GetActive())
                return;

            Obj_AI_Hero enemy = null;
            int count = 0;
            int nCount = 0;

            foreach (var hero1 in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero1.IsEnemy && hero1.IsVisible)
                {
                    if (hero1.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 750)
                    {
                        foreach (var hero2 in ObjectManager.Get<Obj_AI_Hero>())
                        {
                            if (hero2.IsEnemy && hero2.IsVisible)
                            {
                                if (hero2.ServerPosition.Distance(hero1.ServerPosition) < 200)
                                {
                                    count++;
                                }
                            }
                        }
                        if (count == 0)
                        {
                            enemy = hero1;
                        }
                        else if (nCount < count)
                        {
                            nCount = count;
                            enemy = hero1;
                        }
                    }
                }
            }

            if (enemy == null || !enemy.IsValid || enemy.IsDead || Menu.ActivatorDefensiveDebuffSlow.GetMenuItem("SAwarenessActivatorDefensiveDebuffSlowConfigFrostQueensClaim").GetValue<Slider>().Value > nCount)
                return;

            if (Menu.ActivatorDefensiveDebuffSlow.GetMenuItem("SAwarenessActivatorDefensiveDebuffSlowFrostQueensClaim").GetValue<bool>())
            {
                Items.Item fqc = new Items.Item(3092, 850);
                if (fqc.IsReady())
                {
                    fqc.Cast(enemy.ServerPosition);
                }
            }
        }

        private List<BuffInstance> GetActiveCCBuffs()
        {
            List<BuffInstance> nBuffs = new List<BuffInstance>();
            foreach (var buff in ObjectManager.Player.Buffs)
            {
                foreach (var buffType in buffs)
                {
                    if(buff.Type == buffType)
                        nBuffs.Add(buff);
                }
            }
            return nBuffs;
        }

        void UseOffensiveItems_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Menu.ActivatorOffensive.GetActive())
                return;
            if (sender.NetworkId != ObjectManager.Player.NetworkId)
                return;
            if (!args.SData.Name.ToLower().Contains("attack") || args.Target.Type != GameObjectType.obj_AI_Hero)
                return;

            if (Menu.ActivatorOffensiveAd.GetActive())
            {
                var target = SimpleTs.GetTarget(1000, SimpleTs.DamageType.Physical);
                if (target == null || !target.IsValid)
                    return;
                Items.Item entropy = new Items.Item(3184, 400);
                Items.Item hydra = new Items.Item(3074, 400);
                Items.Item botrk = new Items.Item(3153, 450);
                Items.Item tiamat = new Items.Item(3077, 450);
                Items.Item devinesword = new Items.Item(3131, 900);
                Items.Item youmuus = new Items.Item(3142, 900);

                if (entropy.IsReady() && Menu.ActivatorOffensiveAd.GetMenuItem("SAwarenessActivatorOffensiveAdEntropy").GetValue<bool>())
                {
                    entropy.Cast(target);
                }
                if (hydra.IsReady() && Menu.ActivatorOffensiveAd.GetMenuItem("SAwarenessActivatorOffensiveAdRavenousHydra").GetValue<bool>())
                {
                    hydra.Cast(target);
                }
                if (botrk.IsReady() && Menu.ActivatorOffensiveAd.GetMenuItem("SAwarenessActivatorOffensiveAdBOTRK").GetValue<bool>())
                {
                    botrk.Cast(target);
                }
                if (tiamat.IsReady() && Menu.ActivatorOffensiveAd.GetMenuItem("SAwarenessActivatorOffensiveAdTiamat").GetValue<bool>())
                {
                    tiamat.Cast(target);
                }
                if (devinesword.IsReady() && Menu.ActivatorOffensiveAd.GetMenuItem("SAwarenessActivatorOffensiveAdSwordOfTheDevine").GetValue<bool>())
                {
                    devinesword.Cast(target);
                }
                if (youmuus.IsReady() && Menu.ActivatorOffensiveAd.GetMenuItem("SAwarenessActivatorOffensiveAdYoumuusGhostblade").GetValue<bool>())
                {
                    youmuus.Cast(target);
                }
            }
        }

        void UseOffensiveItems_OnGameUpdate()
        {
            if (!Menu.ActivatorOffensive.GetActive() || !Menu.ActivatorOffensive.GetMenuItem("SAwarenessActivatorOffensiveKey").GetValue<KeyBind>().Active)
                return;
            if (Menu.ActivatorOffensiveAd.GetActive())
            {
                var target = SimpleTs.GetTarget(1000, SimpleTs.DamageType.Physical);
                if (target == null || !target.IsValid)
                    return;
                Items.Item botrk = new Items.Item(3153, 450);
                if (botrk.IsReady() && Menu.ActivatorOffensiveAd.GetMenuItem("SAwarenessActivatorOffensiveAdBOTRK").GetValue<bool>())
                {
                    botrk.Cast(target);
                }
            }
            if (Menu.ActivatorOffensiveAp.GetActive())
            {
                var target = SimpleTs.GetTarget(1000, SimpleTs.DamageType.Physical);
                if (target == null || !target.IsValid)
                    return;
                Items.Item bilgewater = new Items.Item(3144, 450);
                Items.Item hextech = new Items.Item(3146, 700);
                Items.Item blackfire = new Items.Item(3188, 750);
                Items.Item dfg = new Items.Item(3128, 750);
                Items.Item twinshadows = new Items.Item(3023, 1000);
                if(Utility.Map.GetMap() == Utility.Map.MapType.CrystalScar)
                    twinshadows = new Items.Item(3290, 1000);
                if (bilgewater.IsReady() && Menu.ActivatorOffensive.GetMenuItem("SAwarenessActivatorOffensiveApBilgewaterCutlass").GetValue<bool>())
                {
                    bilgewater.Cast(target);
                }
                if (hextech.IsReady() && Menu.ActivatorOffensive.GetMenuItem("SAwarenessActivatorOffensiveApHextechGunblade").GetValue<bool>())
                {
                    hextech.Cast(target);
                }
                if (blackfire.IsReady() && Menu.ActivatorOffensive.GetMenuItem("SAwarenessActivatorOffensiveApBlackfireTorch").GetValue<bool>())
                {
                    blackfire.Cast(target);
                }
                if (dfg.IsReady() && Menu.ActivatorOffensive.GetMenuItem("SAwarenessActivatorOffensiveApDFG").GetValue<bool>())
                {
                    dfg.Cast(target);
                }
                if (twinshadows.IsReady() && Menu.ActivatorOffensive.GetMenuItem("SAwarenessActivatorOffensiveApTwinShadows").GetValue<bool>())
                {
                    twinshadows.Cast(target);
                }
            }           
        }

        public static bool IsCCd(Obj_AI_Hero hero)
        {
            var cc = new List<BuffType>
            {
                BuffType.Taunt,
                BuffType.Blind,
                BuffType.Charm,
                BuffType.Fear,
                BuffType.Polymorph,
                BuffType.Stun,
                BuffType.Silence,
                BuffType.Snare
            };

            return cc.Any(hero.HasBuffOfType);
        }

        private SpellSlot GetIgniteSlot()
        {
            foreach (var spell in ObjectManager.Player.SummonerSpellbook.Spells)
            {
                if (spell.Name.ToLower().Contains("dot") && spell.State == SpellState.Ready)
                    return spell.Slot;
            }
            return SpellSlot.Unknown;
        }

        private SpellSlot GetHealSlot()
        {
            foreach (var spell in ObjectManager.Player.SummonerSpellbook.Spells)
            {
                if (spell.Name.ToLower().Contains("heal") && spell.State == SpellState.Ready)
                    return spell.Slot;
            }
            return SpellSlot.Unknown;
        }

        private SpellSlot GetBarrierSlot()
        {
            foreach (var spell in ObjectManager.Player.SummonerSpellbook.Spells)
            {
                if (spell.Name.ToLower().Contains("barrier") && spell.State == SpellState.Ready)
                    return spell.Slot;
            }
            return SpellSlot.Unknown;
        }

        private SpellSlot GetExhaustSlot()
        {
            foreach (var spell in ObjectManager.Player.SummonerSpellbook.Spells)
            {
                if (spell.Name.ToLower().Contains("exhaust") && spell.State == SpellState.Ready)
                    return spell.Slot;
            }
            return SpellSlot.Unknown;
        }

        private SpellSlot GetCleanseSlot()
        {
            foreach (var spell in ObjectManager.Player.SummonerSpellbook.Spells)
            {
                if (spell.Name.ToLower().Contains("boost") && spell.State == SpellState.Ready)
                    return spell.Slot;
            }
            return SpellSlot.Unknown;
        }

        private SpellSlot GetPacketSlot(SpellSlot nSpellSlot)
        {
            SpellSlot spellSlot = nSpellSlot;
            int slot = -1;
            if (spellSlot == SpellSlot.Q)
                slot = 64;
            else if (spellSlot == SpellSlot.W)
                slot = 65;
            if (slot != -1)
            {
                return (SpellSlot) slot;
            }
            return SpellSlot.Unknown;
        }

        void UseSummonerSpells_OnGameUpdate()
        {
            if (!Menu.ActivatorAutoSummonerSpell.GetActive())
                return;

            UseIgnite();
            UseHealth();
            UseBarrier();
            UseExhaust();
            UseCleanse();
        }

        private void UseCleanse()
        {
            if (!Menu.ActivatorAutoSummonerSpellCleanse.GetActive())
                return;
            var sumCleanse = GetCleanseSlot();
            buffs.Clear();
            if (Menu.ActivatorAutoSummonerSpellCleanse.GetMenuItem("SAwarenessActivatorAutoSummonerSpellCleanseStun").GetValue<bool>())
                buffs.Add(BuffType.Stun);
            if (Menu.ActivatorAutoSummonerSpellCleanse.GetMenuItem("SAwarenessActivatorAutoSummonerSpellCleanseSilence").GetValue<bool>())
                buffs.Add(BuffType.Silence);
            if (Menu.ActivatorAutoSummonerSpellCleanse.GetMenuItem("SAwarenessActivatorAutoSummonerSpellCleanseTaunt").GetValue<bool>())
                buffs.Add(BuffType.Taunt);
            if (Menu.ActivatorAutoSummonerSpellCleanse.GetMenuItem("SAwarenessActivatorAutoSummonerSpellCleanseFear").GetValue<bool>())
                buffs.Add(BuffType.Fear);
            if (Menu.ActivatorAutoSummonerSpellCleanse.GetMenuItem("SAwarenessActivatorAutoSummonerSpellCleanseCharm").GetValue<bool>())
                buffs.Add(BuffType.Charm);
            if (Menu.ActivatorAutoSummonerSpellCleanse.GetMenuItem("SAwarenessActivatorAutoSummonerSpellCleanseBlind").GetValue<bool>())
                buffs.Add(BuffType.Blind);
            if (Menu.ActivatorAutoSummonerSpellCleanse.GetMenuItem("SAwarenessActivatorAutoSummonerSpellCleanseDisarm").GetValue<bool>())
                buffs.Add(BuffType.Disarm);
            if (Menu.ActivatorAutoSummonerSpellCleanse.GetMenuItem("SAwarenessActivatorAutoSummonerSpellCleanseSlow").GetValue<bool>())
                buffs.Add(BuffType.Slow);
            if (Menu.ActivatorAutoSummonerSpellCleanse.GetMenuItem("SAwarenessActivatorAutoSummonerSpellCleanseCombatDehancer").GetValue<bool>())
                buffs.Add(BuffType.CombatDehancer);
            if (Menu.ActivatorAutoSummonerSpellCleanse.GetMenuItem("SAwarenessActivatorAutoSummonerSpellCleanseSnare").GetValue<bool>())
                buffs.Add(BuffType.Snare);
            if (Menu.ActivatorAutoSummonerSpellCleanse.GetMenuItem("SAwarenessActivatorAutoSummonerSpellCleansePoison").GetValue<bool>())
                buffs.Add(BuffType.Poison);

            List<BuffInstance> buffList = GetActiveCCBuffs();

            if (buffList.Count() >=
                Menu.ActivatorAutoSummonerSpellCleanse.GetMenuItem("SAwarenessActivatorAutoSummonerSpellCleanseMinSpells").GetValue<Slider>().Value &&
                LastItemCleanseUse + 1 < Game.Time)
            {
                SpellSlot spellSlot = GetPacketSlot(sumCleanse);
                if (spellSlot != SpellSlot.Unknown)
                {
                    GamePacket gPacketT = Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(0, spellSlot));
                    gPacketT.Send();
                    LastItemCleanseUse = Game.Time;
                }
            }
        }

        void UseIgnite()
        {
            if (!Menu.ActivatorAutoSummonerSpellIgnite.GetActive())
                return;
            var sumIgnite = GetIgniteSlot();
            var target = SimpleTs.GetTarget(600, SimpleTs.DamageType.True);            
            if (target != null && sumIgnite != SpellSlot.Unknown)
            {
                var igniteDmg = DamageLib.getDmg(target, DamageLib.SpellType.IGNITE);
                if (igniteDmg > target.Health)
                {
                    SpellSlot spellSlot = GetPacketSlot(sumIgnite);
                    if (spellSlot != SpellSlot.Unknown)
                    {
                        GamePacket gPacketT = Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(target.NetworkId, spellSlot));
                        gPacketT.Send();
                    }
                }
            }
        }

        void UseHealth()
        {
            if (!Menu.ActivatorAutoSummonerSpellHeal.GetActive())
                return;

            var sumHeal = GetHealSlot();
            if (
                Menu.ActivatorAutoSummonerSpellHeal.GetMenuItem("SAwarenessActivatorAutoSummonerSpellHealAllyActive")
                    .GetValue<bool>())
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (!hero.IsEnemy && !hero.IsDead && hero.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 700)
                    {
                        if (((hero.Health / hero.MaxHealth) * 100) <
                            Menu.ActivatorAutoSummonerSpellHeal.GetMenuItem(
                                "SAwarenessActivatorAutoSummonerSpellHealPercent").GetValue<Slider>().Value)
                        {
                            SpellSlot spellSlot = GetPacketSlot(sumHeal);
                            if (spellSlot != SpellSlot.Unknown)
                            {
                                GamePacket gPacketT = Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(0, spellSlot));
                                gPacketT.Send();
                            }
                        }
                    }
                }
            }
            if (((ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) * 100) < Menu.ActivatorAutoSummonerSpellHeal.GetMenuItem("SAwarenessActivatorAutoSummonerSpellHealPercent").GetValue<Slider>().Value)
            {
                SpellSlot spellSlot = GetPacketSlot(sumHeal);
                if (spellSlot != SpellSlot.Unknown)
                {
                    GamePacket gPacketT = Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(0, spellSlot));
                    gPacketT.Send();
                }
            }
        }

        private void UseBarrier()
        {
            if (!Menu.ActivatorAutoSummonerSpellBarrier.GetActive())
                return;

            var sumBarrier = GetBarrierSlot();
            if (((ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) * 100) < Menu.ActivatorAutoSummonerSpellBarrier.GetMenuItem("SAwarenessActivatorAutoSummonerSpellBarrierPercent").GetValue<Slider>().Value)
            {
                SpellSlot spellSlot = GetPacketSlot(sumBarrier);
                if (spellSlot != SpellSlot.Unknown)
                {
                    GamePacket gPacketT = Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(0, spellSlot));
                    gPacketT.Send();
                }
            }
        }

        private void UseExhaust()
        {
            if (!Menu.ActivatorAutoSummonerSpellExhaust.GetActive())
                return;

            var sumExhaust = GetExhaustSlot();
            var enemy = GetHighestAdEnemy();
            if(enemy == null || !enemy.IsValid)
                return;
            var countE = Utility.CountEnemysInRange(750);
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (!hero.IsMe && !hero.IsEnemy && hero.IsValid && hero.ServerPosition.Distance(ObjectManager.Player.ServerPosition) <= 900 && countE >= Menu.ActivatorAutoSummonerSpellExhaust.GetMenuItem("SAwarenessActivatorAutoSummonerSpellExhaustMinEnemies").GetValue<Slider>().Value)
                {
                    var countA = ObjectManager.Get<Obj_AI_Hero>().Where(units => !units.IsEnemy).Count(units => Vector2.Distance(ObjectManager.Player.Position.To2D(), units.Position.To2D()) <= 750);
                    var healthA = hero.Health / hero.MaxHealth * 100;
                    var healthE = enemy.Health / enemy.MaxHealth * 100;
                    SpellSlot spellSlot = GetPacketSlot(sumExhaust);
                    if (spellSlot != SpellSlot.Unknown)
                    {
                        GamePacket gPacketT = Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(enemy.NetworkId, spellSlot));
                        if (Menu.ActivatorAutoSummonerSpellExhaust.GetMenuItem("SAwarenessActivatorAutoSummonerSpellExhaustAutoCast").GetValue<KeyBind>().Active && IsFleeing(enemy) && !ImFleeing(enemy) && countA > 0)
                        {
                            gPacketT.Send();
                        }
                        else if (Menu.ActivatorAutoSummonerSpellExhaust.GetMenuItem("SAwarenessActivatorAutoSummonerSpellExhaustAutoCast").GetValue<KeyBind>().Active && !IsFleeing(enemy) && healthA < 25)
                        {
                            gPacketT.Send();
                        }
                        else
                        if (!IsFleeing(enemy) && healthA <= Menu.ActivatorAutoSummonerSpellExhaust.GetMenuItem("SAwarenessActivatorAutoSummonerSpellExhaustAllyPercent").GetValue<Slider>().Value)
                        {
                            gPacketT.Send();
                        } 
                        else if (!ImFleeing(enemy) && countA > 0 && IsFleeing(enemy) && healthE >= 10 &&
                                 healthE <=
                                 Menu.ActivatorAutoSummonerSpellExhaust.GetMenuItem(
                                     "SAwarenessActivatorAutoSummonerSpellExhaustSelfPercent").GetValue<Slider>().Value)
                        {
                            gPacketT.Send();
                        }                        
                    } 
                }
            }
        }

        private Obj_AI_Hero GetHighestAdEnemy()
        {
            Obj_AI_Hero highestAd = null;
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    if (hero.IsValidTarget() && hero.Distance(ObjectManager.Player.ServerPosition) <= 650)
                    {
                        if (highestAd == null)
                        {
                            highestAd = hero;
                        } else if (highestAd.BaseAttackDamage + highestAd.FlatPhysicalDamageMod <
                                   hero.BaseAttackDamage + hero.FlatPhysicalDamageMod)
                        {
                            highestAd = hero;
                        }
                    }
                }
            }
            return highestAd;
        }

        private bool IsFleeing(Obj_AI_Hero hero)
        {
            if (hero.IsValid &&
                hero.ServerPosition.Distance(ObjectManager.Player.Position) >
                hero.Position.Distance(ObjectManager.Player.Position))
            {
                return true;
            }
            return false;
        }

        private bool ImFleeing(Obj_AI_Hero hero)
        {
            if (hero.IsValid &&
                hero.Position.Distance(ObjectManager.Player.ServerPosition) >
                hero.Position.Distance(ObjectManager.Player.Position))
            {
                return true;
            }
            return false;
        }

    }
}
