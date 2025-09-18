using ABI_RC.Systems.PlayerColors;
using HarmonyLib;
using System.Text;
using MelonLoader;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using ABI_RC.Core.Player;
using System.Runtime.CompilerServices;
using ABI.CCK.Components;


namespace PlayerColorOutfits
{
    internal class Main : MelonMod
    {
        private static readonly string ModDataFolder = Path.Combine("UserData", nameof(PlayerColorOutfits));
        private static readonly string AvatarConfigPath = Path.Combine(ModDataFolder, "avatar_pallete.ini");
        private static readonly string PropConfigPath = Path.Combine(ModDataFolder, "prop_pallete.ini");
        public static List<List<string>> prop_ids = new List<List<string>>();
        public static List<List<string>> avatar_ids = new List<List<string>>();

        public override void OnInitializeMelon()
        {
            // Make sure theres a config directory.
            if (
                ! Directory.Exists(ModDataFolder)    ||
                !      File.Exists(AvatarConfigPath) ||
                !      File.Exists(PropConfigPath)
               )
            {
                CreateConfigTemplate();
            }
            else
            {
                LoadConfig();
            }
        }

        private void CreateConfigTemplate()
        {
            Directory.CreateDirectory(ModDataFolder);
            if (!File.Exists(AvatarConfigPath))
            {
                StringBuilder template_content = new StringBuilder();
                for (int i = 0; i < 16; i++)
                {
                    template_content.AppendLine($"[{((PlayerColorPallet)i).ToString()}]");
                    template_content.AppendLine("");
                }
                File.WriteAllText(AvatarConfigPath, template_content.ToString());
            }
            if (!File.Exists(PropConfigPath))
            {
                StringBuilder template_content = new StringBuilder();
                for (int i = 0; i < 16; i++)
                {
                    template_content.AppendLine($"[{((PlayerColorPallet)i).ToString()}]");
                    template_content.AppendLine("");
                }
                File.WriteAllText(PropConfigPath, template_content.ToString());
            }
            
        }

        private void LoadConfig()
        {
            int pallete = -1;
            foreach (string line in File.ReadLines(AvatarConfigPath))
            {
                if (pallete > 15) break; // Only 16 palletes at the moment, dont go over.
                else if (line.Length == 0) continue; // Also skip blank lines.

                if (line[0] == '[')  // Palletes are ini format headers, that is: they start with a [
                {
                    avatar_ids.Add(new List<string>());
                    pallete++;
                }
                else  // Assume anything else is a vailid id (bad).
                {
                    avatar_ids[pallete].Add(line);
                }
            }

            pallete = -1;
            foreach (string line in File.ReadLines(PropConfigPath))
            {
                if (pallete > 15) break; // Only 16 palletes at the moment, dont go over.
                else if (line.Length == 0) continue; // Also skip blank lines.

                if (line[0] == '[')  // Palletes are ini format headers, that is: they start with a [
                {
                    prop_ids.Add(new List<string>());
                    pallete++;
                }
                else  // Assume anything else is a vailid id (bad).
                {
                    prop_ids[pallete].Add(line);
                }
            }
        }

        [HarmonyPatch]
        internal class HarmonyPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(PlayerBase), nameof(PlayerBase.SetupAvatar))]
            internal static void SetupAvatar(GameObject avatarObject)
            {
                string local_avatar_id = PlayerSetup.Instance.AvatarMetadata.AssetId;
                local_avatar_id = local_avatar_id.StartsWith("a+") ? local_avatar_id.Substring(2) : local_avatar_id;  // Strip the a+ thing.
                for (int i = 0; i < 16; i++) // Cycle through palletes
                {
                    foreach(string potential_id in Main.avatar_ids[i])
                    {   
                        string clean_potential_id = potential_id.StartsWith("a+") ? potential_id.Substring(2) : potential_id; // Strip the a+ thing.

                        if (local_avatar_id.Equals(clean_potential_id)){
                            PlayerColorManager.ChangePlayerColor((PlayerColorPallet)i);
                            return;
                        }
                    }
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch(typeof(PlayerSetup), nameof(PlayerSetup.DropProp))]
            internal static void DropProp(string propGuid)
            {
                string safe_prop_id = propGuid.StartsWith("p+") ? propGuid.Substring(2) : propGuid; // Strip the p+ thing.
                for (int i = 0; i < 16; i++) // Cycle through palletes
                {
                    foreach(string potential_id in Main.prop_ids[i])
                    {
                        string clean_potential_id = potential_id.StartsWith("a+") ? potential_id.Substring(2) : potential_id; // Strip the p+ thing.
                        if (safe_prop_id.Equals(potential_id))
                        {
                            PlayerColorManager.ChangePlayerColor((PlayerColorPallet)i);
                            return;
                        }
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(PlayerSetup), nameof(PlayerSetup.SpawnProp))]
            internal static void SpawnProp(string propGuid, Vector3 spawnPos)
            {
                string safe_prop_id = propGuid.StartsWith("p+") ? propGuid.Substring(2) : propGuid; // Strip the p+ thing.
                for (int i = 0; i < 16; i++) // Cycle through palletes
                {
                    foreach (string potential_id in Main.prop_ids[i])
                    {
                        string clean_potential_id = potential_id.StartsWith("a+") ? potential_id.Substring(2) : potential_id; // Strip the p+ thing.
                        if (safe_prop_id.Equals(potential_id))
                        {
                            PlayerColorManager.ChangePlayerColor((PlayerColorPallet)i);
                            return;
                        }
                    }
                }
            }

        }
    }
}
