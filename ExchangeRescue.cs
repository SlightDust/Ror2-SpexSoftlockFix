// Copyright (C) 2026 SlightDust
// SPDX-License-Identifier: GPL-3.0-or-later

using RoR2;
using UnityEngine.Networking;

namespace SpexSoftlockFix
{
    internal static class ExchangeRescue
    {
        private static bool _grantedThisRun;

        internal static void ResetForNewRun()
        {
            _grantedThisRun = false;
        }

        internal static void OnStageBegin(Stage stage)
        {
            if (!SpexSoftlockFixPlugin.GrantMissingCerebellum.Value || _grantedThisRun)
                return;

            if (!NetworkServer.active || !Run.instance)
                return;

            SceneDef sceneDef = SceneCatalog.GetSceneDefForCurrentScene();
            if (!sceneDef || !string.Equals(sceneDef.baseSceneName, "computationalexchange", System.StringComparison.OrdinalIgnoreCase))
                return;

            ItemIndex encrypted = DLC3Content.Items.MasterBattery.itemIndex;
            ItemIndex exposed = DLC3Content.Items.MasterCore.itemIndex;

            int granted = 0;
            foreach (PlayerCharacterMasterController controller in PlayerCharacterMasterController.instances)
            {
                CharacterMaster master = controller ? controller.master : null;
                if (!master || !master.inventory)
                    continue;

                if (master.inventory.GetItemCountEffective(encrypted) > 0)
                    continue;
                if (master.inventory.GetItemCountEffective(exposed) > 0)
                    continue;

                master.inventory.GiveItemPermanent(encrypted, 1);
                granted++;
                SpexSoftlockFixPlugin.Log.LogInfo("granted Encrypted Cerebellum to " + master.name + " on entering Computational Exchange");
            }

            if (granted <= 0)
                return;

            _grantedThisRun = true;

            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = IsChineseLanguage()
                    ? "<color=#c8ffc8>[SPEX Fix] 背包里没有加密小脑，已补发一个（本局一次）</color>"
                    : "<color=#c8ffc8>[SPEX Fix] No Encrypted Cerebellum in inventory - granted one (once per run)</color>"
            });
        }

        private static bool IsChineseLanguage()
        {
            string languageName = Language.currentLanguageName;
            return !string.IsNullOrEmpty(languageName) && languageName.StartsWith("zh");
        }
    }
}
