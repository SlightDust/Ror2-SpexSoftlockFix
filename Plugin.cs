// Copyright (C) 2026 SlightDust
// SPDX-License-Identifier: GPL-3.0-or-later

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using RoR2;

namespace SpexSoftlockFix
{
    [BepInPlugin(Guid, Name, Version)]
    public sealed class SpexSoftlockFixPlugin : BaseUnityPlugin
    {
        public const string Guid = "pipichen.SpexSoftlockFix";
        public const string Name = "SPEX Softlock Fix";
        public const string Version = "0.1.3";

        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> PreventSpexAnger;
        internal static ConfigEntry<bool> RefundExposedCerebellum;
        internal static ConfigEntry<bool> AnnounceRefund;
        internal static ConfigEntry<bool> GrantMissingCerebellum;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            PreventSpexAnger = Config.Bind(
                "Server",
                "PreventSpexAnger",
                true,
                "SPEX 不再因受击进入偷取状态, 也就不会把玩家踢出计算交易所。 | SPEX can no longer be angered into the steal state, so it never ejects players from the Computational Exchange.");

            RefundExposedCerebellum = Config.Bind(
                "Server",
                "RefundExposedCerebellum",
                true,
                "玩家供奉暴露小脑后仍被踢出时, 把暴露小脑返还给最初提交它的玩家。 | If the player is ejected after offering the Exposed Cerebellum, refund it to the player who offered it.");

            AnnounceRefund = Config.Bind(
                "Server",
                "AnnounceRefund",
                true,
                "返还暴露小脑时向全队广播一条提示。 | Broadcast a chat message to the team when a refund happens.");

            GrantMissingCerebellum = Config.Bind(
                "Server",
                "GrantMissingCerebellum",
                false,
                "进入计算交易所时, 若玩家既没有加密小脑也没有暴露小脑, 直接补发一个加密小脑(每局一次)。默认关闭: 这等于抹掉“忘记拾取”的惩罚。 | When entering the Computational Exchange, grant an Encrypted Cerebellum to players who have neither the Encrypted nor the Exposed Cerebellum (once per run). Off by default: it removes the penalty for forgetting to pick it up.");

            _harmony = new Harmony(Guid);
            _harmony.PatchAll(typeof(SpexSoftlockFixPlugin).Assembly);

            Run.onRunStartGlobal += OnRunStart;
            Stage.onServerStageBegin += OnStageBegin;

            Log.LogInfo(Name + " " + Version + " loaded.");
        }

        private void OnDestroy()
        {
            Run.onRunStartGlobal -= OnRunStart;
            Stage.onServerStageBegin -= OnStageBegin;
            _harmony?.UnpatchSelf();
        }

        private static void OnRunStart(Run run)
        {
            ExchangeRescue.ResetForNewRun();
        }

        private static void OnStageBegin(Stage stage)
        {
            ExchangeRescue.OnStageBegin(stage);
        }
    }
}
