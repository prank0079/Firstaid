using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Firstaid
{
    public class FACore : RocketPlugin<FAConfig>
    {
        public static Firstaid.FACore Instance;
        public List<ushort> RItems;
        public List<ushort> KItems;
        public List<ulong> DPlayers;
        public Dictionary<CSteamID, FAPlayerModel> FAplayer;
        public CSteamID Displayer;
        protected override void Load()
        {
            Firstaid.FACore.Instance = this;
            Firstaid.FACore.Instance.RItems = new List<ushort>();
            Firstaid.FACore.Instance.KItems = new List<ushort>();
            Firstaid.FACore.Instance.FAplayer = new Dictionary<CSteamID, FAPlayerModel>();
            Rocket.Core.Logging.Logger.Log("Firstaid has been loaded...");
            Player.onPlayerCreated += Player_onPlayerCreated;
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected += Events_OnPlayerDisconnected;
            UseableConsumeable.onPerformingAid += UseableConsumeable_onPerformingAid;
            PlayerLife.onPlayerDied += PlayerLife_onPlayerDied;
            DamageTool.damagePlayerRequested += damagePlayerRequested;
            UnturnedPlayerEvents.OnPlayerUpdatePosition += Player_Move;
        }
        protected override void Unload()
        {
            Player.onPlayerCreated -= Player_onPlayerCreated;
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= Events_OnPlayerDisconnected;
            UseableConsumeable.onPerformingAid -= UseableConsumeable_onPerformingAid;
            PlayerLife.onPlayerDied -= PlayerLife_onPlayerDied;
            DamageTool.damagePlayerRequested -= damagePlayerRequested;
            UnturnedPlayerEvents.OnPlayerUpdatePosition -= Player_Move;
            Configuration.Save();
        }
        private void Player_onPlayerCreated(Player player)
        {
            if (player.GetComponent<FADown>() == null)
            {
                player.gameObject.AddComponent<FADown>();
            }
        }        
        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            player.Player.life.OnFallDamageRequested += OnFallDamageRequested;
            player.Player.life.onHurt += On_Hurt;
            FAplayer.Add(player.CSteamID, new FAPlayerModel { Deathtransform = null, Isdown = false });
            if (!(FACore.Instance.Configuration.Instance.DPlayers.Contains((ulong)player.CSteamID)))
                return;
            if (player.GetComponent<FADown>() == null)
            {
                player.Player.gameObject.AddComponent<FADown>();
                FADown component1 = player.GetComponent<FADown>();
                component1.ConnectedOndown();
            }
            else
            {
                FADown component = player.GetComponent<FADown>();
                component.ConnectedOndown();
            }
            FACore.Instance.Configuration.Instance.DPlayers.Remove((ulong)player.CSteamID);
            Configuration.Save();
        }
        private void Player_Move(UnturnedPlayer player, Vector3 position)
        {
            if (!FAplayer[player.CSteamID].Isdown)
                return;
            if (FAplayer[player.CSteamID].Deathtransform == null)
                return;
            if (FAplayer[player.CSteamID].Deathtransform.position != position)
            {

                BarricadeManager.damage(FAplayer[player.CSteamID].Deathtransform, 65000f, 1f, armor: false, default(CSteamID), EDamageOrigin.Carepackage_Timeout);
                ItemBarricadeAsset itemBarricadeAsset = Assets.find(EAssetType.ITEM, FACore.Instance.Configuration.Instance.Down_Effect_World) as ItemBarricadeAsset;
                if (itemBarricadeAsset == null)
                    return;
                FAplayer[player.CSteamID].Deathtransform = BarricadeManager.dropBarricade(new Barricade(itemBarricadeAsset), null, position, 0f, 0f, 0f, 0uL, 0uL);
            }
        }
        private void Events_OnPlayerDisconnected(UnturnedPlayer player)
        {
            if (FAplayer[player.CSteamID].Isdown)
            {
                FACore.Instance.Configuration.Instance.DPlayers.Add((ulong)player.CSteamID);
                Configuration.Save();
            }
            if (FAplayer[player.CSteamID].Deathtransform != null)
            {
                BarricadeManager.damage(FAplayer[player.CSteamID].Deathtransform, 65000f, 1f, armor: false, default(CSteamID), EDamageOrigin.Carepackage_Timeout);
            }
            FAplayer.Remove(player.CSteamID);
            player.Player.life.OnFallDamageRequested -= OnFallDamageRequested;
            player.Player.life.onHurt -= On_Hurt;
        }        
        private void PlayerLife_onPlayerDied(PlayerLife sender, EDeathCause cause, ELimb limb, CSteamID instigator)
        {
            UnturnedPlayer unplayer = UnturnedPlayer.FromCSteamID(sender.player.channel.owner.playerID.steamID);
            if (!FAplayer[unplayer.CSteamID].Isdown)
                return;
            FADown component = sender.GetComponent<FADown>();
            if (component == null)
                return;
            component.Nontdown();
        }
        public IEnumerator BleedingDown(Player player)
        {
            yield return (object)new WaitForSeconds(0.8f);
            if (player.life.health == 1 && player.life.isBleeding && !FACore.Instance.FAplayer[player.channel.owner.playerID.steamID].Isdown)
            {
                FADown component = player.GetComponent<FADown>();
                component.Ondown();
            }
        }
        public IEnumerator BurningDown(Player player)
        {
            yield return (object)new WaitForSeconds(0.8f);
            if (player.life.health > 0 && player.life.health <= 10 && player.life.temperature == EPlayerTemperature.BURNING && !FACore.Instance.FAplayer[player.channel.owner.playerID.steamID].Isdown)
            {
                FADown component = player.GetComponent<FADown>();
                component.Ondown();
            }
        }
        public IEnumerator RadiatedDown(Player player)
        {
            yield return (object)new WaitForSeconds(0.8f);
            if (player.life.health > 0 && player.life.health <= 11 && player.life.virus == 0 && player.movement.isRadiated && !FACore.Instance.FAplayer[player.channel.owner.playerID.steamID].Isdown)
            {
                FADown component = player.GetComponent<FADown>();
                component.Ondown();
            }
        }
        public IEnumerator AcidDown(Player player)
        {
            yield return (object)new WaitForSeconds(0.8f);
            if (player.life.health > 0 && player.life.health <= 10 && player.life.temperature == EPlayerTemperature.ACID && !FACore.Instance.FAplayer[player.channel.owner.playerID.steamID].Isdown)
            {
                FADown component = player.GetComponent<FADown>();
                component.Ondown();
            }
        }
        public IEnumerator OxygenDown(Player player)
        {
            yield return (object)new WaitForSeconds(0.8f);
            if (player.life.health > 0 && player.life.health <= 10 && player.life.oxygen == 0 && !FACore.Instance.FAplayer[player.channel.owner.playerID.steamID].Isdown)
            {
                FADown component = player.GetComponent<FADown>();
                component.Ondown();
            }
        }
        private void On_Hurt(Player player, byte damage, Vector3 force, EDeathCause cause, ELimb limb, CSteamID killer)
        {
            if (player.life.health > 11)
                return;
            FADown component = player.GetComponent<FADown>();
            if (component == null)
                return;
            if (player.life.health == 1 && player.life.isBleeding && !FACore.Instance.FAplayer[player.channel.owner.playerID.steamID].Isdown)
            {
                StartCoroutine(BleedingDown(player));
                return;
            }
            if (player.life.health > 0 && player.life.health <= 10 && player.life.temperature == EPlayerTemperature.BURNING && !FACore.Instance.FAplayer[player.channel.owner.playerID.steamID].Isdown)
            {
                StartCoroutine(BurningDown(player));
                return;
            }
            if (player.life.health == 1 && player.life.temperature == EPlayerTemperature.FREEZING && !FACore.Instance.FAplayer[player.channel.owner.playerID.steamID].Isdown)
            {
                component.Ondown();
                return;
            }
            if (player.life.health == 1 && player.life.food == 0 && !FACore.Instance.FAplayer[player.channel.owner.playerID.steamID].Isdown)
            {
                component.Ondown();
                return;
            }
            if (player.life.health == 1 && player.life.water == 0 && !FACore.Instance.FAplayer[player.channel.owner.playerID.steamID].Isdown)
            {
                component.Ondown();
                return;
            }
            if (player.life.health == 1 && player.life.virus == 0 &&!player.movement.isRadiated && !FACore.Instance.FAplayer[player.channel.owner.playerID.steamID].Isdown)
            {
                component.Ondown();
                return;
            }
            if (player.life.health >0 && player.life.health <= 11 && player.life.virus == 0 && player.movement.isRadiated && !FACore.Instance.FAplayer[player.channel.owner.playerID.steamID].Isdown)
            {
                StartCoroutine(RadiatedDown(player));
                return;
            }
            if (player.life.health > 0 && player.life.health <= 10 && player.life.temperature == EPlayerTemperature.ACID && !FACore.Instance.FAplayer[player.channel.owner.playerID.steamID].Isdown)
            {
                StartCoroutine(AcidDown(player));
                return;
            }
            if (player.life.health > 0 && player.life.health <= 10 && player.life.oxygen == 0 && !FACore.Instance.FAplayer[player.channel.owner.playerID.steamID].Isdown)
            {
                StartCoroutine(OxygenDown(player));
                return;
            }
        }
        private void OnFallDamageRequested(PlayerLife component, float velocity, ref float damage, ref bool shouldBreakLegs)
        {
            UnturnedPlayer unplayer = UnturnedPlayer.FromCSteamID(component.player.channel.owner.playerID.steamID);            
            if (damage == component.health -1 && FACore.Instance.Configuration.Instance.Bleeding_Heal)
            {
                component.serverSetBleeding(false);
            }
            if (damage >= component.health)
            {
                damage = 0;
                FADown component1 = component.GetComponent<FADown>();
                if (component1 == null)
                    return;
                component1.Ondown();
            }
        }
        private void damagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            FADown component = parameters.player.GetComponent<FADown>();
            if (component == null)
                return;
            if (FACore.Instance.FAplayer[parameters.player.channel.owner.playerID.steamID].Isdown)
            {
                parameters.damage *= FACore.Instance.Configuration.Instance.Down_Armor;
                parameters.trackKill = true;
                return;
            }
            if (FACore.Instance.Configuration.Instance.Bleeding_Heal && parameters.player.life.health - 1 == parameters.damage)
            {
                parameters.bleedingModifier = DamagePlayerParameters.Bleeding.Heal;
            }
            if (parameters.cause == EDeathCause.BOULDER && parameters.player.life.health <= 60)
            {
                parameters.player.life.serverModifyHealth(100);
                component.Ondown();                
                return;
            }
            if (parameters.damage* parameters.times >= parameters.player.life.health)
            {
                shouldAllow = false;
                component.Ondown();
                if (FACore.Instance.Configuration.Instance.Down_Message == true)
                {
                    UnturnedPlayer unplayer = UnturnedPlayer.FromCSteamID(parameters.player.channel.owner.playerID.steamID);
                    if (FAplayer.ContainsKey(parameters.killer))
                    {
                        UnturnedPlayer unkiller = UnturnedPlayer.FromCSteamID(parameters.killer); 
                        string KillItemName;
                        if(unkiller.Player.equipment.asset == null)
                        {
                            KillItemName = "null";
                        }
                        else
                        {
                            KillItemName = unkiller.Player.equipment.asset.itemName;
                        }
                        UnturnedChat.Say(unplayer.CSteamID, Translate("command_Player", unkiller.CharacterName, KillItemName, Math.Round((double)Vector3.Distance(unplayer.Position, unkiller.Position)).ToString()));
                        UnturnedChat.Say(unkiller.CSteamID, Translate("command_Killer", unplayer.CharacterName, KillItemName, Math.Round((double)Vector3.Distance(unplayer.Position, unkiller.Position)).ToString()));
                    }
                    else
                    {
                        UnturnedChat.Say(unplayer.CSteamID, Translate("command_NotKiller"));
                    }
                }
            }
        }
        private void UseableConsumeable_onPerformingAid(Player instigator, Player target, ItemConsumeableAsset asset, ref bool shouldAllow)
        {
            try 
            {
                FADown component = target.GetComponent<FADown>();
                if (component == null)
                    return;
                if (!FAplayer[target.channel.owner.playerID.steamID].Isdown)
                    return;
                if (FACore.Instance.Configuration.Instance.RItems.Contains(asset.id))
                {
                    target.life.serverModifyHealth(-(target.life.health - 1));
                    target.life.serverModifyStamina(-(target.life.stamina));
                    target.life.serverSetBleeding(false);
                    component.Nontdown();
                    return;
                }
                if (FACore.Instance.Configuration.Instance.KItems.Contains(asset.id))
                {
                    target.life.askDamage(101, Vector3.up * 101f, EDeathCause.INFECTION, ELimb.SKULL, instigator.channel.owner.playerID.steamID, out var _);
                }
            }
            catch { }
                  
        }
        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    {"command_Player","你被 {0} 用 {1} 击倒，距离 {2} m。"},
                    {"command_Killer","你用 {1} 将 {0} 击倒，距离 {2} m。"},
                    {"command_NotKiller","你倒下了。"},
                };
            }
        }
    }
}
