# Miscellaneous Synthesis compatibility patchers

This is where I plan to keep any compatibility patchers I make for Synthesis.

Currently there are patchers for
- Enhanced Lighting for ENB

All patchers are for Skyrim SE only, unless specified otherwise.

## Technical

Each of these patchers carries over its respective mod's changes if another mod causes a change to revert. However, if another mod makes a change of its own, the patcher will leave it alone.

A change refers to the difference of a value between a mod's master and the mod itself. Each master is taken into account separately.

Value can refer to a single field, or a bunch of grouped together fields. For example, a mod might change an armor's alternate texture; in this case, value would include the armor's file path as well, since if another mod changes the armor to a whole new model, it wouldn't make sense to merge the old model's alternate textures into the new model.