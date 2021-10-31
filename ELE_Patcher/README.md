# Enhanced Lighting for ENB Synthesis patcher

## Description

Carries over all changes from [Enhanced Lighting for ENB](https://www.nexusmods.com/skyrimspecialedition/mods/1377).

Includes changes:
- Image Spaces: HDR, cinematic, tint
- Lights: record flags, flags, object bounds, radius, color, near clip, fade value
- Worldspaces: interior lighting
- Cells: lighting, lighting template, water height, water noise texture, sky and weather from region, image space
- Placed objects: record flags, primitive, light data, bound half extents, unknown, lighting template, image space, location reference, placement

## Installation

### Synthesis

If you have Synthesis, there are 3 options:
- In Synthesis, click on Git repository, and choose ELE Patcher from the list of patchers. <span style="color:red">(not available at the time of writing due to an issue on Synthesis' side)</span>
- In Synthesis, click on Git repository, click on Input, and paste in `https://github.com/Benna96/Synthesis-Misc-Compatibility-Patchers`. Then choose ELE_Patcher from the projects. This will cause the name of the patcher to get stuck on Synthesis-Misc-Patchers, at the time of writing there's no way to change the name.
- [Grab the exe](https://github.com/Benna96/Synthesis-Misc-Compatibility-Patchers/releases/latest/download/ELE_Patcher.exe), then in Synthesis, click on External Program, and browse for the exe. Synthesis doesn't recommend this, but with the issue at the time of writing of the browser not populating with new patchers and not being able to change nicknames, this is the option that gives the patcher a descriptive name in Synthesis' list.

### Standalone

The patcher does run without Synthesis as well. Just [grab the exe](https://github.com/Benna96/Synthesis-Misc-Compatibility-Patchers/releases/latest/download/ELE_Patcher.exe) and run it. The generated plugin is called `Synthesis ELE patch.esp`.

If you're an MO2 user, as with all things, remember to run through MO2!