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
using ABI.CCK.Components;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.GameEventSystem;
using ABI_RC.Core.Util;


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

            // Add listners so stuff happens.
            CVRGameEventSystem.Avatar.OnLocalAvatarLoad.AddListener(OnLocalAvatarLoaded);
            CVRGameEventSystem.Spawnable.OnPropSpawned.AddListener(OnLocalPropLoaded);
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

        /// <summary>
        /// Change the pallet based on the new avatar's guid.
        /// 
        /// This should only be run on the local avatar.
        /// </summary>
        /// <param name="guid">guid to search for the pallet of.</param>
        /// <param name="is_avatar">Weather the object is an avatar or prop.</param>
        private static void ChangePalletFromObject(string guid, string? spanwer)
        {
            PlayerColorPallet pallet;
            if (spanwer == null)
            {
                guid = guid.StartsWith("a+") ? guid.Substring(2) : guid;  // Strip the a+ thing.
                if (AvatarIds.TryGetValue(guid, out pallet))
                {
                    PlayerColorManager.ChangePlayerColor(pallet);
                    MelonLogger.Msg($"Changing pallete to {pallet.ToString()} because of Avatar {guid}");
                }
                else if (AvatarIds.TryGetValue("default", out pallet))
                {
                    PlayerColorManager.ChangePlayerColor(pallet);
                    MelonLogger.Msg($"Changing pallete to {pallet.ToString()} because of avatar default.");
                }
                else
                {
                    MelonLogger.Msg("Would change pallet from avatar but theres nothing to do...");
                }
            }
            else if(MetaPort.Instance.ownerId == spanwer)  // Need to validate prop spawner
            {
                guid = guid.StartsWith("p+") ? guid.Substring(2) : guid;  // Strip the p+ thing.
                if (PropIds.TryGetValue(guid, out pallet))
                {
                    PlayerColorManager.ChangePlayerColor(pallet);
                    MelonLogger.Msg($"Changing pallete to {pallet.ToString()} because of prop {guid}");
                }
                else if (PropIds.TryGetValue("default", out pallet))
                {
                    PlayerColorManager.ChangePlayerColor(pallet);
                    MelonLogger.Msg($"Changing pallete to {pallet.ToString()} because of prop default.");
                }
                else
                {
                    MelonLogger.Msg("Would change pallet from prop but theres nothing to do...");
                }
            }
            
        }

        /// <summary>
        /// Event listener for OnLocalAvatarLoad
        /// 
        /// Calls ChangePalletFromAvatar
        /// </summary>
        /// <param name="avatar">The avatar that was loaded.</param>
        private void OnLocalAvatarLoaded(CVRAvatar avatar)
            => ChangePalletFromObject(avatar.AssetInfo.objectId, null);

        /// <summary>
        /// Event listener for OnPropSpawned.
        /// </summary>
        /// <param name="s">unused</param>
        /// <param name="prop">A prop data about the prop.</param>
        private void OnLocalPropLoaded(string s, CVRSyncHelper.PropData prop)
            => ChangePalletFromObject(prop.ObjectId, prop.SpawnedBy);

    }
}
