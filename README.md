# Player Color Outfits

Chillout VR mod for changing your player color automatically when using certain avatars or spawning certian props.

## Installation

Install the [latest release](https://github.com/jll123567/PlayerColorOutfits/releases) and put the dll in your `<CVR installation folder>/mods` folder.

Currently Player Colors are only available on the nightly branch,
use the [nightly release](https://github.com/jll123567/PlayerColorOutfits/releases/tag/nightly-1.0.1) for the nightly branch.

## Configuration

To set what avatars/props change your color, open 

`<CVR installation folder>/UserData/PlayerColorOutfits/avatar_pallete.ini` for avatars

or

`<CVR installation folder>/UserData/PlayerColorOutfits/prop_pallete.ini` for props

after you've run the mod at least once so it can generate those files.

Then put the guid (double-click the item's picture in its details to copy it) under the header for the color you want to have when using that item.

You can also add `default` the same as guids to use that color when you use an avatar/prop that isn't in the list.

If no default is specified, your color is not changed if the item isnt found in the config.

### Example

Avatars:

```ini
[ChilloutVR]
avatar1

[BaseV]
avatar2

[Unit01]
default
```

Will:
- Use ChilloutVR when in avatar1.
- Use BaseV when in avatar2.
- Use Unit01 when in anything else.

Props:
```ini
[OceanMist]
prop1

[Spooky]

[Cherry]
prop2
```

Will:
- Use OceanMist when you spawn prop1.
- Use Cherry when you spawn prop2.
- Not change your color when spawning other props (default wasn't set).
- Not change to Spooky for any prop (there arent any set).

## Building

1. Clone this repo.
2. Download [NStrip](https://github.com/bbepis/NStrip) into the repo directory, or have it in you `PATH`.
3. If CVR isn't installed in `C:\Program Files (x86)\Steam\steamapps\common\ChilloutVR`, [set your CVR folder environment variable](https://github.com/kafeijao/Kafe_CVR_Mods#set-cvr-folder-environment-variable).
4. Run `copy_and_nstrip_dll.ps1` in powershell.

(Instructions pulled from [https://github.com/kafeijao/Kafe_CVR_Mods](https://github.com/kafeijao/Kafe_CVR_Mods))

## Disclaimer

This modification is not affiliated, approved, or verified by the ChilloutVR team in any way whatsoever.
