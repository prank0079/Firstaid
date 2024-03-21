using Rocket.API;
using System.Collections.Generic;
using UnityEngine;

namespace Firstaid
{
    public class FAPlayerModel
    {
        public bool Isdown;
        public Transform Deathtransform;
    }
    public class FAConfig : IRocketPluginConfiguration
    {
        public ushort Down_UI;
        public ushort Down_Effect_World;
        public bool Down_Message;
        public float Kill_Time;
        public float Down_Armor;
        public float Down_Heal;
        public bool Bleeding_Heal;
        public List<ushort> KItems = new List<ushort>();
        public List<ulong> DPlayers = new List<ulong>();
        public List<ushort> RItems = new List<ushort>();

        public void LoadDefaults()
        {
            Down_UI = 13890;
            Down_Effect_World = 13891;
            Down_Message = true;
            Down_Armor = 0.3f;
            Down_Heal = 0;
            Kill_Time = 0;
            Bleeding_Heal = true;
            RItems = new List<ushort>()
            {
                387,
            };
            KItems = new List<ushort>()
            {
                388,
            };
            DPlayers = new List<ulong>()
            {
            };
        }
    }
}
