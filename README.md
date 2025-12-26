# Player Color Outfits

Chillout VR mod for changing your player color automatically when using certain avatars or spawning certian props.

## Installation

Install the [latest release](https://github.com/jll123567/PlayerColorOutfits/releases) and put the dll in your `<CVR installation folder>/mods` folder.

Currently Player Colors are only available on the nightly branch,
use the [nightly release](https://github.com/jll123567/PlayerColorOutfits/releases/tag/nightly-1.0.1) for the nightly branch.

## Configuration

To set what avatars/props change your color, open 

`<CVR installation folder>/UserData/PlayerColorOutfits/outfits_avatar.txt` for avatars

or

`<CVR installation folder>/UserData/PlayerColorOutfits/prop_avatar.txt` for props

after you've run the mod at least once so it can generate those files.

Each outfit is a guid to match against, primary color swatch, secondary color swatch, and weather to use the primary color as the emission (true or false).
Alternatively you can list a guid, a color preset, and weather to use the primary color as the emission.
You can also use "default" in place of a guid to change to a pallet for object you didn't specify otherwise.
Each field is seperated by commas.

For example: `aaaaa-bbbb-cccc-dddd-1234567890ef, Blue, LightGray, true`
or: `default, Kaffee, false`

## Building

1. Clone this repo.
2. Download [NStrip](https://github.com/bbepis/NStrip) into the repo directory, or have it in you `PATH`.
3. If CVR isn't installed in `C:\Program Files (x86)\Steam\steamapps\common\ChilloutVR`, [set your CVR folder environment variable](https://github.com/kafeijao/Kafe_CVR_Mods#set-cvr-folder-environment-variable).
4. Run `copy_and_nstrip_dll.ps1` in powershell.

(Instructions pulled from [https://github.com/kafeijao/Kafe_CVR_Mods](https://github.com/kafeijao/Kafe_CVR_Mods))

## Disclaimer

This modification is not affiliated, approved, or verified by the ChilloutVR team in any way whatsoever.
