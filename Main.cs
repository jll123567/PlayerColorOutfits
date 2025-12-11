using ABI_RC.Systems.PlayerColors;
using ABI.CCK.Components;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.GameEventSystem;
using ABI_RC.Core.Util;
using MelonLoader;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
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
        public static Dictionary<string,PlayerColors> AvatarOutfits = new Dictionary<string, PlayerColors>();
        public static Dictionary<string, PlayerColors> PropOutfits = new Dictionary<string, PlayerColors>();

        // Debugging
        public static bool Debug = false;

        // Paths for files.
        private static readonly string ModDataFolder = Path.Combine("UserData", nameof(PlayerColorOutfits));
        private static readonly string AvatarConfigPath = Path.Combine(ModDataFolder, "outfits_avatar.txt");
        private static readonly string PropConfigPath = Path.Combine(ModDataFolder, "outfits_props.txt");

        /// <summary>
        /// Just handle a funny thing.
        /// </summary>
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Input.GetKeyDown(KeyCode.Pause))
            {

                LoadConfig();
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
        /// </summary>
        private void CreateConfigTemplate()
        {
            MelonLogger.Warning($"Config files are missing, making some in <your game's install folder>\\{ModDataFolder}");
            Directory.CreateDirectory(ModDataFolder);
            if (!File.Exists(AvatarConfigPath))
            {
                string template_content =
                    "# List outfits for avatars in this file.\n" +
                    "# Each outfit is a guid to match against, primary color swatch, secondary color swatch, and weather to use the primary color as the emission (true or false).\n" +
                    "# Alternatively you can list a guid, a color preset, and weather to use the primary color as the emission.\n" +
                    "# You can also use \"default\" in place of a guid to change to a pallet for object you didn't specify otherwise.\n" +
                    "# For example: aaaaa-bbbb-cccc-dddd-1234567890ef, Blue, LightGray, true\n" +
                    "# or:          default, Kaffee, false\n\n" +
                    "# Below are the different swatches and presets you can use. Exact spelling and capitalization is required.\n" +
                    "# Swatches: White Gray Red Orange Yellow Green DarkCyan Cyan Blue Magenta Purple Black LightGray RustyRed Brown LightBrown LightGreen GreenerTeal LightTeal SkyBlue FlamingoPink Amethyst\n" +
                    "# Presets:  OceanMist SunsetGlow ForestWhisper RoyalDusk DesertBloom BlossomVeil TitaniumWhite WhereIsMyShader CottonCandy ChilloutVR Basis Unit01 Spooky Cherry Kaffee Bongo";
                
                File.WriteAllText(AvatarConfigPath, template_content);
            }
            if (!File.Exists(PropConfigPath))
            {
                string template_content =
                    "# List outfits for props in this file.\n" +
                    "# Each outfit is a guid to match against, primary color swatch, secondary color swatch, and weather to use the primary color as the emission (true or false).\n" +
                    "# Alternatively you can list a guid, a color preset, and weather to use the primary color as the emission.\n" +
                    "# You can also use \"default\" in place of a guid to change to a pallet for object you didn't specify otherwise.\n" +
                    "# For example: aaaaa-bbbb-cccc-dddd-1234567890ef, Blue, LightGray, true\n" +
                    "# or:          default, Kaffee, false\n\n" +
                    "# Below are the different swatches and presets you can use. Exact spelling and capitalization is required.\n" +
                    "# Swatches: White Gray Red Orange Yellow Green DarkCyan Cyan Blue Magenta Purple Black LightGray RustyRed Brown LightBrown LightGreen GreenerTeal LightTeal SkyBlue FlamingoPink Amethyst\n" +
                    "# Presets:  OceanMist SunsetGlow ForestWhisper RoyalDusk DesertBloom BlossomVeil TitaniumWhite WhereIsMyShader CottonCandy ChilloutVR Basis Unit01 Spooky Cherry Kaffee Bongo";

                File.WriteAllText(PropConfigPath, template_content);
            }

        }

        /// <summary>
        /// Loads the content guids and their associated pallets from the configs.
        /// Does some validation, and warns if lines in config are invalid.
        /// </summary>
        private void LoadConfig()
        {
            string swatch_outfit_pattern = "^([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}|default), *([A-Za-z]+), *([A-Za-z]+), *(true|false) *$";
            string preset_outfit_pattern = "^([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}|default), *([A-Za-z]+), *(true|false) *$";
            
            int item_tally = 0;

            foreach (string line in File.ReadLines(AvatarConfigPath))
            {
                if (Regex.IsMatch(line, swatch_outfit_pattern))
                {
                    var matches = Regex.Match(line, swatch_outfit_pattern).Groups;
                    string guid = matches[1].Value;
                    string swatch_1 = matches[2].Value;
                    string swatch_2 = matches[3].Value;
                    string primary_as_emission = matches[4].Value;

                    if (Debug) MelonLogger.Msg($"Pattern: {guid}, {swatch_1}, {swatch_2}, {primary_as_emission}");

                    Color color_1;
                    PlayerColorsUtility.ColorSwatchToColor.TryGetValue((PlayerColorSwatches)Enum.Parse(typeof(PlayerColorSwatches), swatch_1), out color_1);
                    Color color_2;
                    PlayerColorsUtility.ColorSwatchToColor.TryGetValue((PlayerColorSwatches)Enum.Parse(typeof(PlayerColorSwatches), swatch_2), out color_2);

                    PlayerColors imported_color = new PlayerColors(color_1, color_2, Boolean.Parse(primary_as_emission));
                    if (Debug) MelonLogger.Msg($"  Color: {imported_color.PrimaryColor}, {imported_color.SecondaryColor}, {imported_color.UsePrimaryAsEmission}");

                    if (AvatarOutfits.ContainsKey(guid)) AvatarOutfits.Remove(guid);
                    AvatarOutfits.TryAdd(guid, imported_color);

                    item_tally++;
                }
                else if (Regex.IsMatch(line, preset_outfit_pattern)){
                    var matches = Regex.Match(line, preset_outfit_pattern).Groups;
                    string guid = matches[1].Value;
                    string preset = matches[2].Value;
                    string primary_as_emission = matches[3].Value;

                    if (Debug) MelonLogger.Msg($"Pattern: {guid}, {preset}, {primary_as_emission}");

                    ValueTuple<Color, Color> colors;
                    PlayerColorsUtility.ColorPresetToColor.TryGetValue((PlayerColorPresets)Enum.Parse(typeof(PlayerColorPresets), preset), out colors);

                    PlayerColors imported_color = new PlayerColors(colors.Item1, colors.Item2, Boolean.Parse(primary_as_emission));
                    if (Debug) MelonLogger.Msg($"  Color: {imported_color.PrimaryColor}, {imported_color.SecondaryColor}, {imported_color.UsePrimaryAsEmission}");

                    if (AvatarOutfits.ContainsKey(guid)) AvatarOutfits.Remove(guid);
                    AvatarOutfits.TryAdd(guid, imported_color);

                    item_tally++;
                }
                else if (Regex.IsMatch(line, "^#| *"))
                {
                    // Dont error for comments.
                }
                else
                {
                    MelonLogger.Error($"Bad config line: {line}");
                }
            }
            foreach (string line in File.ReadLines(PropConfigPath))
            {
                if (Regex.IsMatch(line, swatch_outfit_pattern))
                {
                    var matches = Regex.Match(line, swatch_outfit_pattern).Groups;
                    string guid = matches[1].Value;
                    string swatch_1 = matches[2].Value;
                    string swatch_2 = matches[3].Value;
                    string primary_as_emission = matches[4].Value;

                    if (Debug) MelonLogger.Msg($"Pattern: {guid}, {swatch_1}, {swatch_2}, {primary_as_emission}");

                    Color color_1;
                    PlayerColorsUtility.ColorSwatchToColor.TryGetValue((PlayerColorSwatches)Enum.Parse(typeof(PlayerColorSwatches), swatch_1), out color_1);
                    Color color_2;
                    PlayerColorsUtility.ColorSwatchToColor.TryGetValue((PlayerColorSwatches)Enum.Parse(typeof(PlayerColorSwatches), swatch_2), out color_2);

                    PlayerColors imported_color = new PlayerColors(color_1, color_2, Boolean.Parse(primary_as_emission));
                    if (Debug) MelonLogger.Msg($"  Color: {imported_color.PrimaryColor}, {imported_color.SecondaryColor}, {imported_color.UsePrimaryAsEmission}");

                    if (PropOutfits.ContainsKey(guid)) PropOutfits.Remove(guid);
                    PropOutfits.TryAdd(guid, imported_color);

                    item_tally++;
                }
                else if (Regex.IsMatch(line, preset_outfit_pattern))
                {
                    var matches = Regex.Match(line, preset_outfit_pattern).Groups;
                    string guid = matches[1].Value;
                    string preset = matches[2].Value;
                    string primary_as_emission = matches[3].Value;

                    if (Debug) MelonLogger.Msg($"Pattern: {guid}, {preset}, {primary_as_emission}");

                    ValueTuple<Color, Color> colors;
                    PlayerColorsUtility.ColorPresetToColor.TryGetValue((PlayerColorPresets)Enum.Parse(typeof(PlayerColorPresets), preset), out colors);

                    PlayerColors imported_color = new PlayerColors(colors.Item1, colors.Item2, Boolean.Parse(primary_as_emission));
                    if (Debug) MelonLogger.Msg($"  Color: {imported_color.PrimaryColor}, {imported_color.SecondaryColor}, {imported_color.UsePrimaryAsEmission}");

                    if (PropOutfits.ContainsKey(guid)) PropOutfits.Remove(guid);
                    PropOutfits.TryAdd(guid, imported_color);

                    item_tally++;
                }
                else if (Regex.IsMatch(line, "^#| *"))
                {
                    // Dont error for comments.
                }
                else
                {
                    MelonLogger.Error($"Bad config line: {line}");
                }
            }

            MelonLogger.Msg($"Loaded pallet changes for {item_tally} items.");
        }

        /// <summary>
        /// Change the pallet based on the new avatar's guid.
        /// 
        /// This should only be run on the local avatar or local prop spawn.
        /// </summary>
        /// <param name="guid">guid to search for the pallet of.</param>
        /// <param name="spanwer">For props, who spawned the prop. For avatars, null.</param>
        private static void ChangePalletFromObject(string guid, string? spanwer)
        {
            if (Debug) MelonLogger.Msg($"Trying to change outfit: {guid}, {spanwer}");
            PlayerColors pallet;
            if (spanwer == null)
            {
                if (AvatarOutfits.TryGetValue(guid, out pallet))
                {
                    PlayerColorsManager.ChangePlayerColor(pallet);
                    if (Debug) MelonLogger.Msg($"Changing pallete to {pallet.ToString()} because of Avatar {guid}");
                }
                else if (AvatarOutfits.TryGetValue("default", out pallet))
                {
                    PlayerColorsManager.ChangePlayerColor(pallet);
                    if (Debug) MelonLogger.Msg($"Changing pallete to {pallet.ToString()} because of avatar default.");
                }
            }
            else if(MetaPort.Instance.ownerId == spanwer)  // Need to validate prop spawner
            {
                if (PropOutfits.TryGetValue(guid, out pallet))
                {
                    PlayerColorsManager.ChangePlayerColor(pallet);
                    if (Debug) MelonLogger.Msg($"Changing pallete to {pallet.ToString()} because of prop {guid}");
                }
                else if (PropOutfits.TryGetValue("default", out pallet))
                {
                    PlayerColorsManager.ChangePlayerColor(pallet);
                    if (Debug) MelonLogger.Msg($"Changing pallete to {pallet.ToString()} because of prop default.");
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
