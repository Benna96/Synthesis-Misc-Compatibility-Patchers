using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;

using Mutagen.Bethesda.Skyrim;

namespace MiscPatcherUtil
{
	public static class PatchField
	{
		#region Applicable to multiple types
		public static void PatchRecordFlags<TMajor, TMajorGetter>(this TMajor patched, HashSet<TMajorGetter> vanillas, TMajorGetter modded, ref bool changed)
			where TMajor : SkyrimMajorRecord, TMajorGetter
			where TMajorGetter : ISkyrimMajorRecordGetter
		{
			var oldPatchedValue = patched.MajorRecordFlagsRaw;
			var moddedValue = modded.MajorRecordFlagsRaw;
			var vanillaValues = vanillas.Select(x => x.MajorRecordFlagsRaw);

			patched.MajorRecordFlagsRaw = Util.GetPatchedFlags(oldPatchedValue, vanillaValues, moddedValue, ref changed);
		}

		#endregion

		#region Image space specific
		public static void PatchHdr(this ImageSpace patched, HashSet<IImageSpaceGetter> vanillas, IImageSpaceGetter modded, ref bool changed)
		{
			var patchedValue = patched.Hdr;
			var moddedValue = modded.Hdr;
			var vanillaValues = vanillas.Select(x => x.Hdr);

			bool membersRevertedToVanilla = vanillaValues.Any(x =>
			{
				var reverted = new bool[9];
				var isVanillaList = new List<bool>();

				reverted[0] = Util.RevertedToVanilla(patchedValue?.EyeAdaptSpeed, x?.EyeAdaptSpeed, moddedValue?.EyeAdaptSpeed, 0.000001f, isVanillaList: isVanillaList);
				reverted[1] = Util.RevertedToVanilla(patchedValue?.BloomBlurRadius, x?.BloomBlurRadius, moddedValue?.BloomBlurRadius, 0.000001f, isVanillaList: isVanillaList);
				reverted[2] = Util.RevertedToVanilla(patchedValue?.BloomThreshold, x?.BloomThreshold, moddedValue?.BloomThreshold, 0.000001f, isVanillaList: isVanillaList);
				reverted[3] = Util.RevertedToVanilla(patchedValue?.BloomScale, x?.BloomScale, moddedValue?.BloomScale, 0.000001f, isVanillaList: isVanillaList);
				reverted[4] = Util.RevertedToVanilla(patchedValue?.ReceiveBloomThreshold, x?.ReceiveBloomThreshold, moddedValue?.ReceiveBloomThreshold, 0.000001f, isVanillaList: isVanillaList);
				reverted[5] = Util.RevertedToVanilla(patchedValue?.White, x?.White, moddedValue?.White, 0.000001f, isVanillaList: isVanillaList);
				reverted[6] = Util.RevertedToVanilla(patchedValue?.SunlightScale, x?.SunlightScale, moddedValue?.SunlightScale, 0.000001f, isVanillaList: isVanillaList);
				reverted[7] = Util.RevertedToVanilla(patchedValue?.SkyScale, x?.SkyScale, moddedValue?.SkyScale, 0.000001f, isVanillaList: isVanillaList);
				reverted[8] = Util.RevertedToVanilla(patchedValue?.EyeAdaptStrength, x?.EyeAdaptStrength, moddedValue?.EyeAdaptStrength, 0.000001f, isVanillaList: isVanillaList);

				return isVanillaList.All(x => x) && reverted.Any(x => x);
			});

			if (membersRevertedToVanilla)
			{
				changed = true;
				patched.Hdr = moddedValue?.DeepCopy();
			}
		}
		public static void PatchCinematic(this ImageSpace patched, HashSet<IImageSpaceGetter> vanillas, IImageSpaceGetter modded, ref bool changed)
		{
			var patchedValue = patched.Cinematic;
			var moddedValue = modded.Cinematic;
			var vanillaValues = vanillas.Select(x => x.Cinematic);

			bool membersRevertedToVanilla = vanillaValues.Any(x =>
			{
				var reverted = new bool[3];
				var isVanillaList = new List<bool>();

				reverted[0] = Util.RevertedToVanilla(patchedValue?.Saturation, x?.Saturation, moddedValue?.Saturation, 0.000001f, isVanillaList: isVanillaList);
				reverted[1] = Util.RevertedToVanilla(patchedValue?.Brightness, x?.Brightness, moddedValue?.Brightness, 0.000001f, isVanillaList: isVanillaList);
				reverted[2] = Util.RevertedToVanilla(patchedValue?.Contrast, x?.Contrast, moddedValue?.Contrast, 0.000001f, isVanillaList: isVanillaList);

				return isVanillaList.All(x => x) && reverted.Any(x => x);
			});

			if (membersRevertedToVanilla)
			{
				changed = true;
				patched.Cinematic = moddedValue?.DeepCopy();
			}
		}
		public static void PatchTint(this ImageSpace patched, HashSet<IImageSpaceGetter> vanillas, IImageSpaceGetter modded, ref bool changed)
		{
			var patchedValue = patched.Tint;
			var moddedValue = modded.Tint;
			var vanillaValues = vanillas.Select(x => x.Tint);

			bool membersRevertedToVanilla = vanillaValues.Any(x =>
			{
				var reverted = new bool[2];
				var isVanillaList = new List<bool>();

				reverted[0] = Util.RevertedToVanilla(patchedValue?.Amount, x?.Amount, moddedValue?.Amount, 0.000001f, isVanillaList: isVanillaList);
				reverted[1] = Util.RevertedToVanilla(patchedValue?.Color, x?.Color, moddedValue?.Color, isVanillaList);

				return isVanillaList.All(x => x) && reverted.Any(x => x);
			});

			if (membersRevertedToVanilla)
			{
				changed = true;
				patched.Tint = moddedValue?.DeepCopy();
			}
		}

		#endregion
		#region Light specific
		public static void PatchFlags(this Light patched, HashSet<ILightGetter> vanillas, ILightGetter modded, ref bool changed)
		{
			var patchedValue = (int)patched.Flags;
			var moddedValue = (int)modded.Flags;
			var vanillaValues = vanillas.Select(x => (int)x.Flags);

			patched.Flags = (Light.Flag)Util.GetPatchedFlags(patchedValue, vanillaValues, moddedValue, ref changed);
		}
		#endregion

		#region Cell specific
		public static void PatchFlags(this Cell patched, HashSet<ICellGetter> vanillas, ICellGetter modded, ref bool changed)
		{
			var patchedValue = (int)patched.Flags;
			var moddedValue = (int)modded.Flags;
			var vanillaValues = vanillas.Select(x => (int)x.Flags);

			patched.Flags = (Cell.Flag)Util.GetPatchedFlags(patchedValue, vanillaValues, moddedValue, ref changed);
		}

		#endregion
		#region Placed object specific
		public static void PatchPrimitive(this PlacedObject patched, HashSet<IPlacedObjectGetter> vanillas, IPlacedObjectGetter modded, ref bool changed)
		{
			var patchedValue = patched.Primitive;
			var moddedValue = modded.Primitive;
			var vanillaValues = vanillas.Select(x => x.Primitive);

			var oldPatchedBounds = patched.Primitive?.Bounds;
			var moddedBounds = modded.Primitive?.Bounds;
			var vanillaBounds = vanillas.Select(x => x.Primitive?.Bounds);
			var newPatchedBounds = Util.PatchReverted(oldPatchedBounds, vanillaBounds, moddedBounds, 0.0001f);

			if (patchedValue == null || moddedValue == null)
				patched.Primitive = moddedValue?.DeepCopy();
			else
				patchedValue.Bounds = newPatchedBounds!.Value;

			changed = newPatchedBounds != oldPatchedBounds ? true : changed;
		}
		public static void PatchLightData(this PlacedObject patched, HashSet<IPlacedObjectGetter> vanillas, IPlacedObjectGetter modded, ref bool changed)
		{
			var patchedValue = patched.LightData;
			var moddedValue = modded.LightData;
			var vanillaValues = vanillas.Select(x => x.LightData);

			bool membersgRevertedToVanilla = vanillaValues.Any(x =>
			{
				var reverted = new bool[5];
				var isVanillaList = new List<bool>();

				reverted[0] = Util.RevertedToVanilla(patchedValue?.FovOffset, x?.FovOffset, moddedValue?.FovOffset, 0.000001f, isVanillaList: isVanillaList);
				reverted[1] = Util.RevertedToVanilla(patchedValue?.FadeOffset, x?.FadeOffset, moddedValue?.FadeOffset, 0.000001f, isVanillaList: isVanillaList);
				reverted[2] = Util.RevertedToVanilla(patchedValue?.ShadowDepthBias, x?.ShadowDepthBias, moddedValue?.ShadowDepthBias, 0.000001f, isVanillaList: isVanillaList);
				reverted[3] = Util.RevertedToVanilla(patchedValue?.Versioning, x?.Versioning, moddedValue?.Versioning, isVanillaList);

					// Only check unknown if versioning matches
					if (isVanillaList[3])
					reverted[4] = Util.RevertedToVanilla(patchedValue?.Unknown, x?.Unknown, moddedValue?.Unknown);

				return isVanillaList.All(x => x) && reverted.Any(x => x);
			});

			if (membersgRevertedToVanilla)
			{
				changed = true;
				patched.LightData = moddedValue?.DeepCopy();
			}
		}
		#endregion
	}

}