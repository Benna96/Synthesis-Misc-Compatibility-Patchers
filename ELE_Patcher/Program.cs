using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using System.Threading.Tasks;

using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Exceptions;

using Noggog;
using MiscPatcherUtil;

namespace ELE_Patcher
{
	public class Program
	{
		static Lazy<ModKey> key = null!;

		public static async Task<int> Main(string[] args)
		{
			return await SynthesisPipeline.Instance
				.AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
				.SetTypicalOpen(GameRelease.SkyrimSE, "Synthesis ELE patch.esp")
				.AddRunnabilityCheck(state =>
				{
					key = new(ModKey.FromNameAndExtension("ELE_SSE.esp"));
					state.LoadOrder.AssertHasMod(key.Value, true, "\n\nELE plugin missing, not active, or inaccessible to patcher!\n\n");
				})
				.Run(args);
		}

		public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
		{
			var startTime = DateTime.Now;
			using var mod = key.Value.GetModAndMasters(state, out var masters);

			Util.WriteLineProgress(true, "Patching ELE image spaces...");
			foreach (var modded in mod.ImageSpaces)
			{
				if (!modded.CheckAndGetRecordsToWorkWith(state, key.Value, masters, out var vanillas, out var winnerContext, out ImageSpace? patched))
					continue;

				patched.PatchHdr(vanillas, modded);
				patched.PatchCinematic(vanillas, modded);
				patched.PatchTint(vanillas, modded);

				if (!patched.Equals(winnerContext.Record))
					state.PatchMod.ImageSpaces.Set(patched);
			}

			Util.WriteLineProgress(true, "Patching ELE lights...");
			foreach (var modded in mod.Lights)
			{
				if (!modded.CheckAndGetRecordsToWorkWith(state, key.Value, masters, out var vanillas, out var winnerContext, out Light? patched))
					continue;

				patched.GetMasks(vanillas, modded, out var moddedEquals, out var vanillasEqual, out var doCopy);

				patched.PatchRecordFlags(vanillas, modded);
				patched.PatchFlags(vanillas, modded);

				doCopy.MaskObjectBounds(vanillasEqual, moddedEquals);
				doCopy.MaskRadius(vanillasEqual, moddedEquals);
				doCopy.MaskColor(vanillasEqual, moddedEquals);
				doCopy.MaskNearClip(vanillasEqual, moddedEquals);
				doCopy.MaskFadeValue(vanillasEqual, moddedEquals);
				patched.DeepCopyIn(modded, doCopy);

				if (!patched.Equals(winnerContext.Record))
					state.PatchMod.Lights.Set(patched);
			}

			Util.WriteLineProgress(true, "Patching ELE worldspaces...");
			var worldspaces = mod.AsEnumerable().Worldspace().WinningOverrides();
			foreach (var modded in worldspaces)
			{
				if (!modded.CheckAndGetRecordsToWorkWith(state, key.Value, masters, out var vanillas, out var winnerContext, out Worldspace? patched))
					continue;

				patched.GetMasks(vanillas, modded, out var moddedEquals, out var vanillasEqual, out var doCopy);

				doCopy.MaskInteriorLighting(vanillasEqual, moddedEquals);
				patched.DeepCopyIn(modded, doCopy);

				if (!patched.Equals(winnerContext.Record))
					state.PatchMod.Worldspaces.Set(patched);
			}

			Util.WriteLineProgress(true, "Patching ELE cells...");
			var cells = mod.AsEnumerable().Cell().WinningOverrides();
			foreach (var modded in cells)
			{
				if (!modded.CheckAndGetRecordsToWorkWith(state, key.Value, masters, out var vanillas, out var winnerContext, out Cell? _))
					continue;

				var patched = winnerContext.GetOrAddAsOverride(state.PatchMod);
				patched.GetMasks(vanillas, modded, out var moddedEquals, out var vanillasEqual, out var doCopy);

				patched.PatchFlags(vanillas, modded);

				doCopy.MaskLighting(vanillasEqual, moddedEquals);
				doCopy.MaskLightingTemplate(vanillasEqual, moddedEquals);
				doCopy.MaskWaterHeight(vanillasEqual, moddedEquals);
				doCopy.MaskWaterNoiseTexture(vanillasEqual, moddedEquals);
				doCopy.MaskSkyAndWeather(vanillasEqual, moddedEquals);
				doCopy.MaskImageSpace(vanillasEqual, moddedEquals);
				patched.DeepCopyIn(modded, doCopy);

				if (winnerContext.ModKey != state.PatchMod.ModKey && patched.Equals(winnerContext.Record))
					state.PatchMod.Remove<Cell>(modded.FormKey);
			}

			Util.WriteLineProgress(true, "Patching ELE placed objects...");
			var placedObjects = mod.AsEnumerable().PlacedObject().WinningOverrides();
			foreach (var modded in placedObjects)
			{
				if (!modded.CheckAndGetRecordsToWorkWith(state, key.Value, masters, out var vanillas, out var winnerContext, out PlacedObject? _))
					continue;

				var patched = winnerContext.GetOrAddAsOverride(state.PatchMod);
				patched.GetMasks(vanillas, modded, out var moddedEquals, out var vanillasEqual, out var doCopy);

				patched.PatchRecordFlags(vanillas, modded);
				patched.PatchPrimitive(vanillas, modded);
				patched.PatchLightData(vanillas, modded);

				doCopy.MaskBoundHalfExtents(vanillasEqual, moddedEquals);
				doCopy.MaskUnknown(vanillasEqual, moddedEquals);
				doCopy.MaskLightingTemplate(vanillasEqual, moddedEquals);
				doCopy.MaskImageSpace(vanillasEqual, moddedEquals);
				doCopy.MaskLocationRef(vanillasEqual, moddedEquals);
				doCopy.MaskPlacement(vanillasEqual, moddedEquals);
				patched.DeepCopyIn(modded, doCopy);

				if (winnerContext.ModKey != state.PatchMod.ModKey && patched.Equals(winnerContext.Record))
					state.PatchMod.Remove<PlacedObject>(patched.FormKey);
			}

			var endTime = DateTime.Now;
			Util.WriteLineProgress(true, $"Took this long: {endTime - startTime}");
		}
	}
}

/* Progress bar to perhaps implement one day... 
 * Doesn't work with Synthesis' console, seem it can't rewrite a line */
/*decimal newPercentage = 100m * (index + 1) / cells.Count();
if ((int)newPercentage > percentage)
{
	percentage = (int)newPercentage;
	string percentsString = $"{percentage}%";
	//percentage = 2.0695m;
	Util.WriteProgress(true, $"\rPatching ELE cells... {percentsString,-4}");
}*/