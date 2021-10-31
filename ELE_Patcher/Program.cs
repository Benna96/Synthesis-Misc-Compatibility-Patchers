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
			using var mod = key.Value.GetModAndMasters(state, out var masters);

			var devKey = FormKey.Factory("021EF3:Skyrim.esm");
			var devKey2 = FormKey.Factory("002F82:Dawnguard.esm");

			Util.WriteLineProgress(true, "Patching ELE image spaces...");
			foreach (var modded in mod.ImageSpaces)
			{
				if (!modded.InitializeRecordVars(state, key.Value, masters, out var vanillas, out ImageSpace? patched, out var changed))
					continue;

				patched.PatchHdr(vanillas, modded, ref changed);
				patched.PatchCinematic(vanillas, modded, ref changed);
				patched.PatchTint(vanillas, modded, ref changed);

				if (changed)
					state.PatchMod.ImageSpaces.Set(patched);
			}

			Util.WriteLineProgress(true, "Patching ELE lights...");
			foreach (var modded in mod.Lights)
			{
				if (!modded.InitializeRecordVars(state, key.Value, masters, out var vanillas, out Light? patched, out var changed))
					continue;

				patched.PatchRecordFlags(vanillas, modded, ref changed);
				patched.PatchFlags(vanillas, modded, ref changed);

				patched.GetMasks(vanillas, modded, out var moddedEquals, out var vanillasEqual, out var doCopy);
				doCopy.MaskObjectBounds(vanillasEqual, moddedEquals, ref changed);
				doCopy.MaskRadius(vanillasEqual, moddedEquals, ref changed);
				doCopy.MaskColor(vanillasEqual, moddedEquals, ref changed);
				doCopy.MaskNearClip(vanillasEqual, moddedEquals, ref changed);
				doCopy.MaskFadeValue(vanillasEqual, moddedEquals, ref changed);
				patched.DeepCopyIn(modded, doCopy);

				if (changed)
					state.PatchMod.Lights.Set(patched);
			}

			Util.WriteLineProgress(true, "Patching ELE worldspaces...");
			var worldspaces = mod.AsEnumerable().Worldspace().WinningContextOverrides();
			foreach (var moddedContext in worldspaces)
			{
				var modded = moddedContext.Record;

				if (!modded.InitializeRecordVars(state, key.Value, masters, out var vanillas, out Worldspace? patched, out var safeToRemove, out var changed))
					continue;

				patched.GetMasks(vanillas, modded, out var moddedEquals, out var vanillasEqual, out var doCopy);
				doCopy.MaskInteriorLighting(vanillasEqual, moddedEquals, ref changed);

				if (changed)
					moddedContext
						.GetOrAddAsOverride(state.PatchMod)
						.DeepCopyIn(modded, doCopy);
			}

			Util.WriteLineProgress(true, "Patching ELE cells...");
			var cellMask = new Cell.TranslationMask(false)
			{
				Flags = true
			};
			var cells = mod.AsEnumerable().Cell().WinningContextOverrides(state.LinkCache);
			foreach (var moddedContext in cells)
			{
				var modded = moddedContext.Record;

				if (!modded.InitializeRecordVars(state, key.Value, masters, out var vanillas, out Cell? patched, out var changed))
					continue;

				patched.PatchFlags(vanillas, modded, ref changed);

				patched.GetMasks(vanillas, modded, out var moddedEquals, out var vanillasEqual, out var doCopy);
				doCopy.MaskLighting(vanillasEqual, moddedEquals, ref changed);
				doCopy.MaskLightingTemplate(vanillasEqual, moddedEquals, ref changed);
				doCopy.MaskWaterHeight(vanillasEqual, moddedEquals, ref changed);
				doCopy.MaskWaterNoiseTexture(vanillasEqual, moddedEquals, ref changed);
				doCopy.MaskSkyAndWeather(vanillasEqual, moddedEquals, ref changed);
				doCopy.MaskImageSpace(vanillasEqual, moddedEquals, ref changed);

				if (changed)
				{
					var patchedIntoMod = moddedContext.GetOrAddAsOverride(state.PatchMod);
					patchedIntoMod.DeepCopyIn(patched, cellMask);
					patchedIntoMod.DeepCopyIn(modded, doCopy);
				}
			}

			Util.WriteLineProgress(true, "Patching ELE placed objects...");
			var placedObjects = mod.AsEnumerable().PlacedObject().WinningContextOverrides(state.LinkCache);
			var placedObjectMask = new PlacedObject.TranslationMask(false)
			{
				MajorRecordFlagsRaw = true,
				Primitive = new(true),
				LightData = new(true)
			};
			foreach (var moddedContext in placedObjects)
			{
				var modded = moddedContext.Record;

				if (!modded.InitializeRecordVars(state, key.Value, masters, out var vanillas, out PlacedObject? patched, out var changed))
					continue;

				patched.PatchRecordFlags(vanillas, modded, ref changed);
				patched.PatchPrimitive(vanillas, modded, ref changed);
				patched.PatchLightData(vanillas, modded, ref changed);

				patched.GetMasks(vanillas, modded, out var moddedEquals, out var vanillasEqual, out var doCopy);
				doCopy.MaskBoundHalfExtents(vanillasEqual, moddedEquals, ref changed);
				doCopy.MaskUnknown(vanillasEqual, moddedEquals, ref changed);
				doCopy.MaskLightingTemplate(vanillasEqual, moddedEquals, ref changed);
				doCopy.MaskImageSpace(vanillasEqual, moddedEquals, ref changed);
				doCopy.MaskLocationRef(vanillasEqual, moddedEquals, ref changed);
				doCopy.MaskPlacement(vanillasEqual, moddedEquals, ref changed);

				if (changed)
				{
					var patchedIntoMod = moddedContext.GetOrAddAsOverride(state.PatchMod);
					patchedIntoMod.DeepCopyIn(patched, placedObjectMask);
					patchedIntoMod.DeepCopyIn(modded, doCopy);
				}
			}
		}
	}
}