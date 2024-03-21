using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Collections;
using UnityEngine;

namespace Firstaid
{
    public class FADown : MonoBehaviour
    {
        public Player Player;
        public UnturnedPlayer downplayer;
        public float Ktime;
        public bool istimekill = true;
        private void Awake()
        {
            Player = GetComponent<Player>();
            downplayer = UnturnedPlayer.FromCSteamID(Player.channel.owner.playerID.steamID);
            if (Player == null)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
        }
        public void ConnectedOndown()
        {
            if (Player.movement.isSafe)
                return;
            istimekill = true;
            FACore.Instance.FAplayer[downplayer.CSteamID].Isdown = true;
            EffectManager.sendUIEffect(FACore.Instance.Configuration.Instance.Down_UI, (short)FACore.Instance.Configuration.Instance.Down_UI, Provider.findTransportConnection(Player.channel.owner.playerID.steamID), true);
            if (FACore.Instance.Configuration.Instance.Kill_Time == 0)
            {
                Ktime = 1f;
            }
            else
            {
                Ktime = FACore.Instance.Configuration.Instance.Kill_Time;
            }
            StartCoroutine(Onlocked());
            ItemBarricadeAsset itemBarricadeAsset = Assets.find(EAssetType.ITEM, FACore.Instance.Configuration.Instance.Down_Effect_World) as ItemBarricadeAsset;
            if (itemBarricadeAsset == null)
                return;
            FACore.Instance.FAplayer[downplayer.CSteamID].Deathtransform = BarricadeManager.dropBarricade(new Barricade(itemBarricadeAsset), null, downplayer.Position, 0f, 0f, 0f, 0uL, 0uL);
        }
        public void Ondown()
        {
            if (Player.movement.isSafe)
                return;
            Player.life.serverModifyHealth(100);
            istimekill = true;
            FACore.Instance.FAplayer[downplayer.CSteamID].Isdown = true;
            EffectManager.sendUIEffect(FACore.Instance.Configuration.Instance.Down_UI, (short)FACore.Instance.Configuration.Instance.Down_UI, Provider.findTransportConnection(Player.channel.owner.playerID.steamID), true);
            if (FACore.Instance.Configuration.Instance.Kill_Time == 0)
            {
                Ktime = 1f;
            }
            else
            {
                Ktime = FACore.Instance.Configuration.Instance.Kill_Time;
            }
            StartCoroutine(Onlocked());
            ItemBarricadeAsset itemBarricadeAsset = Assets.find(EAssetType.ITEM, FACore.Instance.Configuration.Instance.Down_Effect_World) as ItemBarricadeAsset;
            if (itemBarricadeAsset == null)
                return;
            FACore.Instance.FAplayer[downplayer.CSteamID].Deathtransform = BarricadeManager.dropBarricade(new Barricade(itemBarricadeAsset), null, downplayer.Position, 0f, 0f, 0f, 0uL, 0uL);
        }
        public void Nontdown()
        {
            FACore.Instance.FAplayer[downplayer.CSteamID].Isdown = false; 
            BarricadeManager.damage(FACore.Instance.FAplayer[Player.channel.owner.playerID.steamID].Deathtransform, 65000f, 1f, armor: false, default(CSteamID), EDamageOrigin.Carepackage_Timeout);
        }

        public IEnumerator Onlocked()
        {
            while (Ktime > 0)
            {
                if (FACore.Instance.Configuration.Instance.Kill_Time != 0)
                {
                    Ktime -= 1f;
                    EffectManager.sendUIEffectText((short)FACore.Instance.Configuration.Instance.Down_UI, Provider.findTransportConnection(Player.channel.owner.playerID.steamID), true, "Time", (Ktime).ToString("0"));
                }
                if (!FACore.Instance.FAplayer[downplayer.CSteamID].Isdown)
                {
                    istimekill = false;
                    Ktime = 0;
                }
                else
                {
                    Player.life.serverModifyHealth(FACore.Instance.Configuration.Instance.Down_Heal);
                }
                VehicleManager.forceRemovePlayer(Player.channel.owner.playerID.steamID);
                Player.equipment.dequip();
                Player.stance.stance = EPlayerStance.PRONE;
                Player.animator.sendGesture(EPlayerGesture.SURRENDER_START, true);
                Player.stance.checkStance(EPlayerStance.PRONE);
                Player.movement.sendPluginSpeedMultiplier(0);
                Player.movement.sendPluginJumpMultiplier(0);
                yield return (object)new WaitForSeconds(1f);
            }
            if (istimekill)
            {
                Player.life.askDamage(101, Vector3.up * 101f, EDeathCause.INFECTION, ELimb.SKULL, Player.channel.owner.playerID.steamID, out var _);
                FACore.Instance.FAplayer[downplayer.CSteamID].Isdown = false;
            }
            Player.movement.sendPluginSpeedMultiplier(1f);
            Player.movement.sendPluginJumpMultiplier(1f);
            EffectManager.askEffectClearByID(FACore.Instance.Configuration.Instance.Down_UI, Provider.findTransportConnection(Player.channel.owner.playerID.steamID));
            if (transform == null)
                yield break;
            BarricadeManager.damage(FACore.Instance.FAplayer[Player.channel.owner.playerID.steamID].Deathtransform, 65000f, 1f, armor: false, default(CSteamID), EDamageOrigin.Carepackage_Timeout);
        }
    }
}