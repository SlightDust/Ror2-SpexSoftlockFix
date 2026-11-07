// Copyright (C) 2026 SlightDust
// SPDX-License-Identifier: GPL-3.0-or-later

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace SpexSoftlockFix
{
    [BepInPlugin(Guid, Name, Version)]
    public sealed class SpexSoftlockFixPlugin : BaseUnityPlugin
    {
        public const string Guid = "pipichen.SpexSoftlockFix";
        public const string Name = "SPEX Softlock Fix";
        public const string Version = "0.1.1";

        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> PreventSpexAnger;
        internal static ConfigEntry<bool> RefundExposedCerebellum;
        internal static ConfigEntry<bool> AnnounceRefund;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            PreventSpexAnger = Config.Bind(
                "Server",
                "PreventSpexAnger",
                true,
                "SPEX 不再因受击进入偷取状态, 也就不会把玩家踢出计算交易所。");

            RefundExposedCerebellum = Config.Bind(
                "Server",
                "RefundExposedCerebellum",
                true,
                "玩家供奉暴露小脑后仍被踢出时, 把暴露小脑返还给最初提交它的玩家。");

            AnnounceRefund = Config.Bind(
                "Server",
                "AnnounceRefund",
                true,
                "返还暴露小脑时向全队广播一条提示。");

            _harmony = new Harmony(Guid);
            _harmony.PatchAll(typeof(SpexSoftlockFixPlugin).Assembly);
            Log.LogInfo(Name + " " + Version + " loaded.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
