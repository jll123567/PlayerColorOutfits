using ABI_RC.Systems.PlayerColors;
using HarmonyLib;
using System.Text;
using MelonLoader;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using ABI_RC.Core.Player;
using System.Text.RegularExpressions;
using System;


namespace PlayerColorOutfits
{
    /// <summary>
    /// Pretty much the whole mod here.
    /// 
    /// Read in the avatar_pallete and prop_pallete to figure out what pallet to change to when.
    /// Then do the changing when the user chages avatars or spawns/drops a prop.
    /// </summary>
    internal class Main : MelonMod
    {
        // Associate a content guid to a pallet.
        public static Dictionary<string,PlayerColorPallet> PropIds = new Dictionary<string, PlayerColorPallet>();
        public static Dictionary<string, PlayerColorPallet> AvatarIds = new Dictionary<string, PlayerColorPallet>();
        
        // Paths for files.
        private static readonly string ModDataFolder = Path.Combine("UserData", nameof(PlayerColorOutfits));
        private static readonly string AvatarConfigPath = Path.Combine(ModDataFolder, "avatar_pallete.ini");
        private static readonly string PropConfigPath = Path.Combine(ModDataFolder, "prop_pallete.ini");

        /// <summary>
        /// Just handle a funny thing.
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Input.GetKeyDown(KeyCode.Pause))
            {
                MelonLogger.Msg("Enjoy the forbidden pallet!");
                PlayerColorManager.ChangePlayerColor(PlayerColorPallet.TotalPallets);
                MelonLogger.Msg($"Your pallet is now: {PlayerColorManager.CurrentPallet.ToString()}");
            }
        }

        /// <summary>
        /// Create the template config files or read existing ones in.
        /// </summary>
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

        /// <summary>
        /// Create a config file template
        /// 
        /// Config files are ini format
        /// Each section is the pallet and can have a list of content guids.
        /// </summary>
        private void CreateConfigTemplate()
        {
            MelonLogger.Warning($"Config files are missing, making some in <your game's install folder>\\{ModDataFolder}");
            Directory.CreateDirectory(ModDataFolder);
            if (!File.Exists(AvatarConfigPath))
            {
                StringBuilder template_content = new StringBuilder();
                foreach (string pallet_name in Enum.GetNames(typeof(PlayerColorPallet)))
                {
                    if (!pallet_name.Equals("TotalPallets")) // While I appreciate it... this one does not help *me* in particular.
                    {
                        template_content.AppendLine($"[{pallet_name}]");
                        template_content.AppendLine("");
                    }
                }
                File.WriteAllText(AvatarConfigPath, template_content.ToString());
            }
            if (!File.Exists(PropConfigPath))
            {
                StringBuilder template_content = new StringBuilder();
                foreach (string pallet_name in Enum.GetNames(typeof(PlayerColorPallet)))
                {
                    if (!pallet_name.Equals("TotalPallets"))
                    {
                        template_content.AppendLine($"[{pallet_name}]");
                        template_content.AppendLine("");
                    }
                }
                File.WriteAllText(PropConfigPath, template_content.ToString());
            }
            
        }

        /// <summary>
        /// Loads the content guids and their associated pallets from the configs.
        /// Does some validation, and warns if lines in config are invalid.
        /// </summary>
        private void LoadConfig()
        {
            string header_pattern = "^\\[([A-Za-z]+)\\]";
            string guid_pattern = "^([ap]\\+)?([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})";

            int avatar_tally = 0;
            int prop_tally = 0;

            PlayerColorPallet pallete = (PlayerColorPallet)(-1); // -1 indicates an invalid pallete.
            foreach (string line in File.ReadLines(AvatarConfigPath))
            {

                if (Regex.IsMatch(line,header_pattern))  // Palletes are ini format headers.
                {
                    string pallete_name = Regex.Match(line, header_pattern).Groups[1].Value;
                    try
                    {
                        if (pallete_name.Equals("TotalPallets")) throw new ArgumentException();  // While it is valid to the enum, its not a real pallete.
                        pallete = Enum.Parse<PlayerColorPallet>(pallete_name);

                    }
                    catch (ArgumentException)
                    {

                        MelonLogger.Warning($"The pallet header [{pallete_name}] is not valid.\n Fix your {AvatarConfigPath} file.");
                    }
                    
                }
                else if (Regex.IsMatch(line, guid_pattern))
                {
                    string guid = Regex.Match(line, guid_pattern).Groups[2].Value;
                    if (((int)pallete) != -1) {
                        AvatarIds.Add(guid, pallete);
                        avatar_tally++;
                    }
                    else
                    {
                        MelonLogger.Warning($"The guid {guid} was not added to a pallete, it should be under a \"[<pallet name>]\" line in your {AvatarConfigPath}.");
                    }
                }
                else if (Regex.IsMatch(line, "^default") && ! AvatarIds.ContainsKey("default"))
                {
                    AvatarIds.Add("default", pallete);
                }
            }

            pallete = (PlayerColorPallet)(-1);
            foreach (string line in File.ReadLines(PropConfigPath))
            {

                if (Regex.IsMatch(line, header_pattern))  // Palletes are ini format headers.
                {
                    string pallete_name = Regex.Match(line, header_pattern).Groups[1].Value;
                    try
                    {
                        if (pallete_name.Equals("TotalPallets")) throw new ArgumentException();  // While it is valid to the enum, its not a real pallete.
                        pallete = Enum.Parse<PlayerColorPallet>(pallete_name);
                    }
                    catch (ArgumentException)
                    {

                        MelonLogger.Warning($"The pallet header [{pallete_name}] is not valid.\n Fix your {PropConfigPath} file.");
                    }
                }
                else if (Regex.IsMatch(line, guid_pattern))
                {
                    string guid = Regex.Match(line, guid_pattern).Groups[2].Value;
                    if (((int)pallete) != -1)
                    {
                        PropIds.Add(guid, pallete);
                        prop_tally++;
                    }
                    else
                    {
                        MelonLogger.Warning($"The guid {guid} was not added to a pallete, it should be under a \"[<pallet name>]\" line in your {PropConfigPath}.");
                    }
                }
                else if (Regex.IsMatch(line, "^default") && !PropIds.ContainsKey("default"))
                {
                    PropIds.Add("default", pallete);
                }
            }
            MelonLogger.Msg($"Loaded pallet changes for {avatar_tally} avatars and {prop_tally} props.");
        }

        [HarmonyPatch]
        internal class HarmonyPatches
        {
            /// <summary>
            /// Patch to SetupAvatar to change the pallet.
            /// 
            /// Only checks the local avatar, but may run for remote avatar changes as well.
            /// For some reason if this patched method takes to long, SetupAvatar will be re-run, creating an infinite loop.
            /// </summary>
            /// <param name="avatarObject">unused</param>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(PlayerBase), nameof(PlayerBase.SetupAvatar))]
            internal static void SetupAvatar(GameObject avatarObject)
            {
                string local_avatar_id = PlayerSetup.Instance.AvatarMetadata.AssetId;
                local_avatar_id = local_avatar_id.StartsWith("a+") ? local_avatar_id.Substring(2) : local_avatar_id;  // Strip the a+ thing.
                PlayerColorPallet pallet;
                if (AvatarIds.TryGetValue(local_avatar_id, out pallet))
                {
                    PlayerColorManager.ChangePlayerColor(pallet);
                    //MelonLogger.Msg($"Changing pallete to {pallet.ToString()} because of Avatar {local_avatar_id}");
                }
                else if(AvatarIds.TryGetValue("default", out pallet))
                {
                    PlayerColorManager.ChangePlayerColor(pallet);
                    //MelonLogger.Msg($"Changing pallete to {pallet.ToString()} because of default.");
                }
            }

            /// <summary>
            /// Patch to DropProp to change the pallete.
            /// 
            /// Only for the local player.
            /// </summary>
            /// <param name="propGuid">Prop guid to test for.</param>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(PlayerSetup), nameof(PlayerSetup.DropProp))]
            internal static void DropProp(string propGuid)
            {
                string safe_prop_id = propGuid.StartsWith("p+") ? propGuid.Substring(2) : propGuid; // Strip the p+ thing.
                PlayerColorPallet pallet;
                if (PropIds.TryGetValue(safe_prop_id, out pallet))
                {
                    PlayerColorManager.ChangePlayerColor(pallet);
                    //MelonLogger.Msg($"Changing pallete to {pallet.ToString()} because of Prop {safe_prop_id}");
                }
                else if (PropIds.TryGetValue("default", out pallet))
                {
                    PlayerColorManager.ChangePlayerColor(pallet);
                    //MelonLogger.Msg($"Changing pallete to {pallet.ToString()} because of default.");
                }
            }

            /// <summary>
            /// Patch to SpawnProp to change the pallet.
            /// 
            /// Only for the local player.
            /// </summary>
            /// <param name="propGuid">Prop guid to test for.</param>
            /// <param name="spawnPos">unused</param>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(PlayerSetup), nameof(PlayerSetup.SpawnProp))]
            internal static void SpawnProp(string propGuid, Vector3 spawnPos)
            {
                string safe_prop_id = propGuid.StartsWith("p+") ? propGuid.Substring(2) : propGuid; // Strip the p+ thing.
                PlayerColorPallet pallet;
                if (PropIds.TryGetValue(safe_prop_id, out pallet))
                {
                    PlayerColorManager.ChangePlayerColor(pallet);
                    //MelonLogger.Msg($"Changing pallete to {pallet.ToString()} because of Prop {safe_prop_id}");
                }
                else if (PropIds.TryGetValue("default", out pallet))
                {
                    PlayerColorManager.ChangePlayerColor(pallet);
                    //MelonLogger.Msg($"Changing pallete to {pallet.ToString()} because of default.");
                }
            }

        }
    }
}
