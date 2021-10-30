using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;

using Mutagen.Bethesda.Skyrim;
using MiscPatcherUtil;
using Loqui;

namespace MiscPatcherUtil
{
	public static class EqualityMasks
	{
		public static void GetMasks(this Light patched, HashSet<ILightGetter> vanillas, ILightGetter modded,
			out Light.Mask<bool> moddedEquals, out HashSet<Light.Mask<bool>> vanillasEqual, out Light.TranslationMask doCopy)
		{
			moddedEquals = modded.GetEqualsMask(patched);
			AddMoreMasks(moddedEquals, modded, patched);

			vanillasEqual = new();
			foreach (var vanilla in vanillas)
			{
				var vanillaMask = vanilla.GetEqualsMask(patched);
				AddMoreMasks(vanillaMask, vanilla, patched);

				vanillasEqual.Add(vanillaMask);
			}

			doCopy = new(false);

			// Add masks for things that are wanted but not set by GetEqualsMask
			void AddMoreMasks(Light.Mask<bool> mask, ILightGetter first, ILightGetter second)
			{
				// Floating point equality
				mask.NearClip = Util.NearlyEquals(first.NearClip, second.NearClip, 0.000001f);
				mask.FadeValue = Util.NearlyEquals(first.FadeValue, second.FadeValue, 0.000001f);

				// Fields that require masks of their own
				mask.ObjectBounds = new(true, new(false));
				mask.ObjectBounds.Specific!.First = first.ObjectBounds.First.Equals(second.ObjectBounds.First);
				mask.ObjectBounds.Specific!.Second = first.ObjectBounds.Second.Equals(second.ObjectBounds.Second);
			}
		}

		public static void GetMasks(this Worldspace patched, HashSet<IWorldspaceGetter> vanillas, IWorldspaceGetter modded,
			out Worldspace.Mask<bool> moddedEquals, out HashSet<Worldspace.Mask<bool>> vanillasEqual, out Worldspace.TranslationMask doCopy)
		{
			moddedEquals = modded.GetEqualsMask(patched);

			vanillasEqual = new();
			foreach (var vanilla in vanillas)
				vanillasEqual.Add(vanilla.GetEqualsMask(patched));

			doCopy = new(false);
		}
		public static void GetMasks(this Cell patched, HashSet<ICellGetter> vanillas, ICellGetter modded,
			out Cell.Mask<bool> moddedEquals, out HashSet<Cell.Mask<bool>> vanillasEqual, out Cell.TranslationMask doCopy)
		{
			moddedEquals = modded.GetEqualsMask(patched);
			AddMoreMasks(moddedEquals, modded, patched);

			vanillasEqual = new();
			foreach (var vanilla in vanillas)
			{
				var vanillaMask = vanilla.GetEqualsMask(patched);
				AddMoreMasks(vanillaMask, vanilla, patched);

				vanillasEqual.Add(vanillaMask);
			}

			doCopy = new(false);

			// Add masks for things that are wanted but not set (properly) by GetEqualsMask
			void AddMoreMasks(Cell.Mask<bool> mask, ICellGetter first, ICellGetter second)
			{
				// Requires its own mask & floating point precision
				mask.Lighting = new(true, new(false));
				if (first.Lighting == null || second.Lighting == null)
					mask.Lighting = new(true, new(Equals(first.Lighting, second.Lighting)));
				else
				{
					var firstL = first.Lighting;
					var secondL = second.Lighting;

					mask.Lighting = new(true, first.Lighting.GetEqualsMask(second.Lighting));
					mask.Lighting.Specific!.DirectionalFade = Util.NearlyEquals(firstL.DirectionalFade, secondL.DirectionalFade, 0.000001f);
					mask.Lighting.Specific!.FogClipDistance = Util.NearlyEquals(firstL.FogClipDistance, secondL.FogClipDistance, 0.000001f);
					mask.Lighting.Specific!.FogPower = Util.NearlyEquals(firstL.FogPower, secondL.FogPower, 0.000001f);
					mask.Lighting.Specific!.FogMax = Util.NearlyEquals(firstL.FogMax, secondL.FogMax, 0.000001f);
					mask.Lighting.Specific!.LightFadeBegin = Util.NearlyEquals(firstL.LightFadeBegin, secondL.LightFadeBegin, 0.000001f);
					mask.Lighting.Specific!.LightFadeEnd = Util.NearlyEquals(firstL.LightFadeEnd, secondL.LightFadeEnd, 0.000001f);
					mask.Lighting.Specific!.FogNear = Util.NearlyEquals(firstL.FogNear, secondL.FogNear, 0.000001f);
					mask.Lighting.Specific!.FogFar = Util.NearlyEquals(firstL.FogFar, secondL.FogFar, 0.000001f);
					/*mask.Lighting.Specific!.Scale = Util.FloatsNearlyEqual(firstL.Scale, secondL.Scale, 0.000001f);*/
				}

				// Null:Skyrim.esm to Null
				var firstLightingTemplate = first.DeepCopy().LightingTemplate.WithFixedNull();
				var secondLightingTemplate = second.DeepCopy().LightingTemplate.WithFixedNull();
				mask.LightingTemplate = Equals(firstLightingTemplate, secondLightingTemplate);
			}
		}
		public static void GetMasks(this PlacedObject patched, HashSet<IPlacedObjectGetter> vanillas, IPlacedObjectGetter modded,
			out PlacedObject.Mask<bool> moddedEquals, out HashSet<PlacedObject.Mask<bool>> vanillasEqual, out PlacedObject.TranslationMask doCopy)
		{
			moddedEquals = modded.GetEqualsMask(patched);
			AddMoreMasks(moddedEquals, modded, patched);

			vanillasEqual = new();
			foreach (var vanilla in vanillas)
			{
				var vanillaMask = vanilla.GetEqualsMask(patched);
				AddMoreMasks(vanillaMask, vanilla, patched);

				vanillasEqual.Add(vanillaMask);
			}

			doCopy = new(false);

			// Add masks for things that are wanted but not set (properly) by GetEqualsMask
			void AddMoreMasks(PlacedObject.Mask<bool> mask, IPlacedObjectGetter first, IPlacedObjectGetter second)
			{
				// Floating point precision
				mask.BoundHalfExtents = Util.NearlyEquals(first.BoundHalfExtents, second.BoundHalfExtents, 0.000001f);

				// Require their own mask & floating point precision
				var firstP = first.Placement;
				var secondP = second.Placement;
				if (firstP == null || secondP == null)
					mask.Placement = new(true, new(Equals(firstP, secondP)));
				else
				{
					mask.Placement = new(true, firstP.GetEqualsMask(secondP));
					mask.Placement.Specific!.Position = Util.NearlyEquals(firstP.Position, secondP.Position, 0.000001f);
					mask.Placement.Specific!.Rotation = Util.NearlyEquals(Util.NormalizeRadians2Pi(firstP.Rotation), Util.NormalizeRadians2Pi(secondP.Rotation), 0.0000017453293f);
				}
			}
		}
	}
	public static class MaskField
	{
		#region Light specific
		public static void MaskObjectBounds(this Light.TranslationMask doCopy, HashSet<Light.Mask<bool>> vanillasEqual, Light.Mask<bool> moddedEquals)
		{
			bool moddedBoundedEquals = moddedEquals.ObjectBounds?.Specific?.All(x => x) ?? false;
			bool anyVanillaBoundedEquals = vanillasEqual.Any(x => x.ObjectBounds?.Specific?.All(y => y) ?? false);

			if (!moddedBoundedEquals && anyVanillaBoundedEquals)
				doCopy.ObjectBounds = new(true);
		}
		public static void MaskRadius(this Light.TranslationMask doCopy, HashSet<Light.Mask<bool>> vanillasEqual, Light.Mask<bool> moddedEquals)
		{
			doCopy.Radius = !moddedEquals.Radius && vanillasEqual.Any(x => x.Radius);
		}
		public static void MaskColor(this Light.TranslationMask doCopy, HashSet<Light.Mask<bool>> vanillasEqual, Light.Mask<bool> moddedEquals)
		{
			doCopy.Color = !moddedEquals.Color && vanillasEqual.Any(x => x.Color);
		}
		public static void MaskNearClip(this Light.TranslationMask doCopy, HashSet<Light.Mask<bool>> vanillasEqual, Light.Mask<bool> moddedEquals)
		{
			doCopy.NearClip = !moddedEquals.NearClip && vanillasEqual.Any(x => x.NearClip);
		}
		public static void MaskFadeValue(this Light.TranslationMask doCopy, HashSet<Light.Mask<bool>> vanillasEqual, Light.Mask<bool> moddedEquals)
		{
			doCopy.FadeValue = !moddedEquals.FadeValue && vanillasEqual.Any(x => x.FadeValue);
		}
		#endregion

		#region Worldspace specific
		public static void MaskInteriorLighting(this Worldspace.TranslationMask doCopy, HashSet<Worldspace.Mask<bool>> vanillasEqual, Worldspace.Mask<bool> moddedEquals)
		{
			doCopy.InteriorLighting = !moddedEquals.InteriorLighting && vanillasEqual.Any(x => x.InteriorLighting);
		}

		#endregion
		#region Cell specific
		public static void MaskLighting(this Cell.TranslationMask doCopy, HashSet<Cell.Mask<bool>> vanillasEqual, Cell.Mask<bool> moddedEquals)
		{
			bool moddedLightingEquals = moddedEquals.Lighting?.Specific?.All(x => x) ?? false;
			bool anyVanillaLightingEquals = vanillasEqual.Any(x => x.Lighting?.Specific?.All(y => y) ?? false);

			if (!moddedLightingEquals && anyVanillaLightingEquals)
				doCopy.Lighting = new(true);
		}
		public static void MaskLightingTemplate(this Cell.TranslationMask doCopy, HashSet<Cell.Mask<bool>> vanillasEqual, Cell.Mask<bool> moddedEquals)
		{
			doCopy.LightingTemplate = !moddedEquals.LightingTemplate && vanillasEqual.Any(x => x.LightingTemplate);
		}
		public static void MaskWaterHeight(this Cell.TranslationMask doCopy, HashSet<Cell.Mask<bool>> vanillasEqual, Cell.Mask<bool> moddedEquals)
		{
			doCopy.WaterHeight = !moddedEquals.WaterHeight && vanillasEqual.Any(x => x.WaterHeight);
		}
		public static void MaskWaterNoiseTexture(this Cell.TranslationMask doCopy, HashSet<Cell.Mask<bool>> vanillasEqual, Cell.Mask<bool> moddedEquals)
		{
			doCopy.WaterNoiseTexture = !moddedEquals.WaterNoiseTexture && vanillasEqual.Any(x => x.WaterNoiseTexture);
		}
		public static void MaskSkyAndWeather(this Cell.TranslationMask doCopy, HashSet<Cell.Mask<bool>> vanillasEqual, Cell.Mask<bool> moddedEquals)
		{
			doCopy.SkyAndWeatherFromRegion = !moddedEquals.SkyAndWeatherFromRegion && vanillasEqual.Any(x => x.SkyAndWeatherFromRegion);
		}
		public static void MaskImageSpace(this Cell.TranslationMask doCopy, HashSet<Cell.Mask<bool>> vanillasEqual, Cell.Mask<bool> moddedEquals)
		{
			doCopy.ImageSpace = !moddedEquals.ImageSpace && vanillasEqual.Any(x => x.ImageSpace);
		}
		#endregion
		#region Placed object specific
		public static void MaskBoundHalfExtents(this PlacedObject.TranslationMask doCopy, HashSet<PlacedObject.Mask<bool>> vanillasEqual, PlacedObject.Mask<bool> moddedEquals)
		{
			doCopy.BoundHalfExtents = !moddedEquals.BoundHalfExtents && vanillasEqual.Any(x => x.BoundHalfExtents);
		}
		public static void MaskUnknown(this PlacedObject.TranslationMask doCopy, HashSet<PlacedObject.Mask<bool>> vanillasEqual, PlacedObject.Mask<bool> moddedEquals)
		{
			doCopy.Unknown = !moddedEquals.Unknown && vanillasEqual.Any(x => x.Unknown);
		}
		public static void MaskLightingTemplate(this PlacedObject.TranslationMask doCopy, HashSet<PlacedObject.Mask<bool>> vanillasEqual, PlacedObject.Mask<bool> moddedEquals)
		{
			doCopy.LightingTemplate = !moddedEquals.LightingTemplate && vanillasEqual.Any(x => x.LightingTemplate);
		}
		public static void MaskImageSpace(this PlacedObject.TranslationMask doCopy, HashSet<PlacedObject.Mask<bool>> vanillasEqual, PlacedObject.Mask<bool> moddedEquals)
		{
			doCopy.ImageSpace = !moddedEquals.ImageSpace && vanillasEqual.Any(x => x.ImageSpace);
		}
		public static void MaskLocationRef(this PlacedObject.TranslationMask doCopy, HashSet<PlacedObject.Mask<bool>> vanillasEqual, PlacedObject.Mask<bool> moddedEquals)
		{
			doCopy.LocationReference = !moddedEquals.LocationReference && vanillasEqual.Any(x => x.LocationReference);
		}
		public static void MaskPlacement(this PlacedObject.TranslationMask doCopy, HashSet<PlacedObject.Mask<bool>> vanillasEqual, PlacedObject.Mask<bool> moddedEquals)
		{
			bool moddedPlacementEquals = moddedEquals.Placement?.Specific?.All(x => x) ?? false;
			bool anyVanillaPlacementEquals = vanillasEqual.Any(x => x.Placement?.Specific?.All(y => y) ?? false);

			if (!moddedPlacementEquals && anyVanillaPlacementEquals)
				doCopy.Placement = new(true);
		}
		#endregion
	}
}