using System;
using System.Collections.Generic;
using System.Text;

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using Noggog;
using Loqui;

using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;

namespace MiscPatcherUtil
{
	public static class Util
	{
		#region General
		/// <summary>
		/// Writes line to console if doWrite is true.<br/>
		/// Use like Console.Write, except add a bool to the beginning.
		/// </summary>
		/// <param name="doWrite">Specifies whether or not to write. For easy setting-dependent printing.</param>
		/// <param name="format">The string or format to write.</param>
		/// <param name="args">Array of objects to write using format.</param>
		public static void WriteProgress(bool doWrite, string? format = null, params object[] args)
		{
			if (doWrite)
			{
				if (format == null) Console.WriteLine();
				else Console.Write(format, args);
			}
		}
		/// <summary>
		/// Writes line to console if doWrite is true.<br/>
		/// Use like Console.WriteLine, except add a bool to the beginning.
		/// </summary>
		/// <param name="doWrite">Specifies whether or not to write. For easy setting-dependent printing.</param>
		/// <param name="format">The string or format to write.</param>
		/// <param name="args">Array of objects to write using format.</param>
		public static void WriteLineProgress(bool doWrite, string? format = null, params object[] args)
		{
			if (doWrite)
			{
				if (format == null) Console.WriteLine();
				else Console.WriteLine(format, args);
			}
		}

		public static ISkyrimModDisposableGetter GetModAndMasters(this ModKey key, IPatcherState<ISkyrimMod, ISkyrimModGetter> state, out HashSet<ModKey> masters)
		{
			var mod = GetDisposableMod();
			masters = GetModMasters(mod);
			return mod;

			ISkyrimModDisposableGetter GetDisposableMod()
			{
				var fileName = key.FileName;
				var path = ModPath.FromPath(Path.Combine(state.DataFolderPath, fileName));
				var skyrimVersion = state.GameRelease.ToSkyrimRelease();
				return SkyrimMod.CreateFromBinaryOverlay(path, skyrimVersion);
			}
			HashSet<ModKey> GetModMasters(ISkyrimModDisposableGetter leMod)
			{
				HashSet<ModKey> leMasters = new();
				foreach (var masterRef in leMod.MasterReferences)
					leMasters.Add(masterRef.Master);
				return leMasters;
			}
		}

		/// <summary>
		/// Initializes variables related to the record.<br/>
		/// Returns false if record doesn't need to be patched.<br/>
		/// Checks that the record doesn't originate from mod, and that the winning override of the record isn't from mod or a known patch to mod.<br/>
		/// This overload is for records that can be added to PatchMod using Set.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TGetter"></typeparam>
		/// <param name="modRecord"></param>
		/// <param name="state"></param>
		/// <param name="modKey"></param>
		/// <param name="masters"></param>
		/// <param name="vanillaRecords"></param>
		/// <param name="patched"></param>
		/// <param name="changed"></param>
		/// <param name="knownPatches"></param>
		/// <returns></returns>
		public static bool InitializeRecordVars<T, TGetter>(this TGetter modRecord, IPatcherState<ISkyrimMod, ISkyrimModGetter> state, ModKey modKey, HashSet<ModKey> masters, out HashSet<TGetter> vanillaRecords, [NotNullWhen(true)] out T? patched, out bool changed, HashSet<string>? knownPatches = null)
			where T : SkyrimMajorRecord, TGetter
			where TGetter : class, ISkyrimMajorRecordGetter
		{
			vanillaRecords = new();
			patched = null;
			changed = false;
			var contexts = state.LinkCache.ResolveAllContexts<T, TGetter>(modRecord.FormKey);

			// Don't patch if record originates from modded, or modded is winning override
			var origin = contexts.Last();
			var winner = contexts.First();
			if (origin.ModKey == modKey || winner.ModKey == modKey)
				return false;

			// Don't patch if winner is a known patch to the mod.
			if (knownPatches != null)
			{
				var winnerModFile = winner.ModKey.FileName.String;
				if (knownPatches.Contains(winnerModFile))
					return false;
			}

			// Add vanilla records & patched
			foreach (var context in contexts)
				if (masters.Contains(context.ModKey))
					vanillaRecords.Add(context.Record);

			patched = (T)winner.Record.DeepCopy();
			return true;
		}
		/// <summary>
		/// Initializes variables related to the record.<br/>
		/// Returns false if record doesn't need to be patched.<br/>
		/// Checks that the record doesn't originate from mod, and that the winning override of the record isn't from mod or a known patch to mod.<br/>
		/// This overload is for records that need to be added as override through IModContext.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TGetter"></typeparam>
		/// <param name="modRecord"></param>
		/// <param name="state"></param>
		/// <param name="modKey"></param>
		/// <param name="masters"></param>
		/// <param name="vanillaRecords"></param>
		/// <param name="patched"></param>
		/// <param name="safeToRemove"></param>
		/// <param name="changed"></param>
		/// <param name="knownPatches"></param>
		/// <returns></returns>
		public static bool InitializeRecordVars<T, TGetter>(this TGetter modRecord, IPatcherState<ISkyrimMod, ISkyrimModGetter> state, ModKey modKey, HashSet<ModKey> masters, out HashSet<TGetter> vanillaRecords, [NotNullWhen(true)] out T? patched, out bool safeToRemove, out bool changed, HashSet<string>? knownPatches = null)
			where T : SkyrimMajorRecord, TGetter
			where TGetter : class, ISkyrimMajorRecordGetter
		{
			vanillaRecords = new();
			patched = null;
			safeToRemove = changed = false;
			var contexts = state.LinkCache.ResolveAllContexts<T, TGetter>(modRecord.FormKey);

			// Don't patched if record originates from modded, or modded is winning override
			var origin = contexts.Last();
			var winner = contexts.First();
			if (origin.ModKey == modKey || winner.ModKey == modKey)
				return false;

			// Don't allow removal of record in case of ITPO if PatchMod was latest override before patching
			safeToRemove = winner.ModKey != state.PatchMod.ModKey;

			// Don't patch if winner is a known patch to the mod.
			if (knownPatches != null)
			{
				var winnerModFile = winner.ModKey.FileName.String;
				if (knownPatches.Contains(winnerModFile))
					return false;
			}

			// Add vanilla records & patched
			foreach (var context in contexts)
				if (masters.Contains(context.ModKey))
					vanillaRecords.Add(context.Record);

			patched = winner.GetOrAddAsOverride(state.PatchMod);
			return true;
		}
		/// <summary>
		/// Initializes variables related to the record.<br/>
		/// Returns false if record doesn't need to be patched.<br/>
		/// Checks that the record doesn't originate from mod, and that the winning override of the record isn't from mod or a known patch to mod.<br/>
		/// This overload is for records that need access to a context in order to GetOrAddAsOverride.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TGetter"></typeparam>
		/// <param name="modRecord"></param>
		/// <param name="state"></param>
		/// <param name="modKey"></param>
		/// <param name="masters"></param>
		/// <param name="vanillaRecords"></param>
		/// <param name="patched"></param>
		/// <param name="safeToRemove"></param>
		/// <param name="changed"></param>
		/// <param name="knownPatches"></param>
		/// <returns></returns>
		public static bool InitializeRecordVars<T, TGetter>(this TGetter modRecord, IPatcherState<ISkyrimMod, ISkyrimModGetter> state, ModKey modKey, HashSet<ModKey> masters, out HashSet<TGetter> vanillaRecords, IModContext<ISkyrimMod, ISkyrimModGetter, T, TGetter> winnerContext, [NotNullWhen(true)] out T? patched, out bool changed, HashSet<string>? knownPatches = null)
			where T : SkyrimMajorRecord, TGetter
			where TGetter : class, ISkyrimMajorRecordGetter
		{
			vanillaRecords = new();
			patched = null;
			changed = false;
			var contexts = state.LinkCache.ResolveAllContexts<T, TGetter>(modRecord.FormKey);

			// Don't patched if record originates from modded, or modded is winning override
			var origin = contexts.Last();
			var winner = contexts.First();
			if (origin.ModKey == modKey || winner.ModKey == modKey)
				return false;

			// Don't patch if winner is a known patch to the mod.
			if (knownPatches != null)
			{
				var winnerModFile = winner.ModKey.FileName.String;
				if (knownPatches.Contains(winnerModFile))
					return false;
			}

			// Add vanilla records & patched
			foreach (var context in contexts)
				if (masters.Contains(context.ModKey))
					vanillaRecords.Add(context.Record);

			patched = (T)winner.Record.DeepCopy();
			return true;
		}
		#endregion

		#region Internal helpers
		private static float NormalizeRadians2Pi(float radians)
		{
			float TwoPI = 2f * MathF.PI;
			float result = radians % TwoPI;
			return result > 0 ? result : result + TwoPI;
		}
		internal static P3Float NormalizeRadians2Pi(P3Float radiansStruct)
		{
			float x = NormalizeRadians2Pi(radiansStruct.X);
			float y = NormalizeRadians2Pi(radiansStruct.Y);
			float z = NormalizeRadians2Pi(radiansStruct.Z);

			return new(x, y, z);
		}

		internal static void FixNull<TFormLink>(this TFormLink linkToFix)
			where TFormLink : IFormLink<IMajorRecordCommonGetter>
		{
			if (linkToFix.IsNull)
				linkToFix.SetToNull();
		}
		internal static TFormLink WithFixedNull<TFormLink>(this TFormLink linkToFix)
			where TFormLink : IFormLink<IMajorRecordCommonGetter>
		{
			linkToFix.FixNull();
			return linkToFix;
		}

		internal static bool NearlyEquals(float? first, float? second, float allowedDiff, (float min, float max)? range = null)
		{
			if (first == null || second == null)
				return Equals(first, second);
			else
			{
				bool equal = MathF.Abs((float)first - (float)second) < allowedDiff;
				if (!equal && range != null)
				{
					float allowedEdgeDiff = allowedDiff / 2f;

					float firstMinDiff = MathF.Abs((float)first - range.Value.min);
					float firstMaxDiff = MathF.Abs((float)first - range.Value.max);
					float secondMinDiff = MathF.Abs((float)second - range.Value.min);
					float secondMaxDiff = MathF.Abs((float)second - range.Value.max);

					//bool edge1 = (firstMinDiff + secondMaxDiff) < allowedDiff;
					//bool edge2 = (firstMaxDiff + secondMinDiff) < allowedDiff;

					bool firstIsEdge = firstMinDiff < allowedEdgeDiff || firstMaxDiff < allowedEdgeDiff;
					bool secondIsEdge = secondMinDiff < allowedEdgeDiff || secondMaxDiff < allowedEdgeDiff;

					equal = firstIsEdge && secondIsEdge;
				}

				return equal;
			}
		}
		internal static bool NearlyEquals(P3Float? first, P3Float? second, float allowedDiff, (float min, float max)? range = null)
		{
			bool xEquals = NearlyEquals(first?.X, second?.X, allowedDiff, range);
			bool yEquals = NearlyEquals(first?.Y, second?.Y, allowedDiff, range);
			bool zEquals = NearlyEquals(first?.Z, second?.Z, allowedDiff, range);

			return xEquals && yEquals && zEquals;
		}

		internal static bool RevertedToVanilla(float? patched, float? vanilla, float? modded, float allowedDiff, (float min, float max)? range = null, List<bool>? isVanillaList = null)
		{
			bool isVanilla = NearlyEquals(patched, vanilla, allowedDiff, range);
			bool isModded = NearlyEquals(patched, modded, allowedDiff, range);

			isVanillaList?.Add(isVanilla);

			return !isModded && isVanilla;
		}
		private static bool RevertedToVanilla(P3Float? patched, P3Float? vanilla, P3Float? modded, float allowedDiff, (float min, float max)? range = null)
		{
			bool vanillaEquals = NearlyEquals(patched, vanilla, allowedDiff, range);
			bool moddedEquals = NearlyEquals(patched, modded, allowedDiff, range);

			return !moddedEquals && vanillaEquals;
		}
		internal static bool RevertedToVanilla<T>(T? patched, T? vanilla, T? modded, List<bool>? isVanillaList = null)
		{
			bool isVanilla = Equals(patched, vanilla);
			bool isModded = Equals(patched, modded);

			isVanillaList?.Add(isVanilla);

			return !isModded && isVanilla;
		}

		internal static float? PatchReverted(float? patched, IEnumerable<float?> vanillas, float? modded, float allowedDiff, (float min, float max)? range = null)
		{
			if (vanillas.Any(x => RevertedToVanilla(patched, x, modded, allowedDiff, range)))
				return modded;
			else
				return patched;
		}
		internal static P3Float? PatchReverted(P3Float? patched, IEnumerable<P3Float?> vanillas, P3Float? modded, float allowedDiff, (float min, float max)? range = null)
		{
			if (vanillas.Any(x => RevertedToVanilla(patched, x, modded, allowedDiff, range)))
				return modded;
			else
				return patched;
		}
		internal static TGetter? PatchReverted<TGetter>(TGetter? patched, IEnumerable<TGetter?> vanillas, TGetter? modded)
			where TGetter : ILoquiObjectGetter
		{
			if (vanillas.Any(x => RevertedToVanilla(patched, x, modded)))
				return modded;
			else
				return patched;
		}

		internal static int GetPatchedFlags(int patchedValue, IEnumerable<int> vanillaValues, int moddedValue, ref bool changed)
		{
			foreach (var vanillaValue in vanillaValues)
				PatchEachFlag(ref patchedValue, vanillaValue, moddedValue, ref changed);

			return patchedValue;

			void PatchEachFlag(ref int patchedFlags, int vanillaFlags, int moddedFlags, ref bool changed)
			{
				// These return 00111000 etc where if bit is 1 it's been changed
				var moddedChangedFlags = moddedFlags ^ vanillaFlags;
				var patchedChangedFlags = patchedFlags ^ vanillaFlags;

				// These ones for convenience checking bits
				BitArray moddedChangedBits = new(new int[] { moddedChangedFlags });
				BitArray patchedChangedBits = new(new int[] { patchedChangedFlags });

				for (int i = 0; i < moddedChangedBits.Length; ++i)
				{
					// Change the corresponding bit if modded changes it but winner doesn't
					if (moddedChangedBits.Get(i) && !patchedChangedBits.Get(i))
					{
						changed = true;
						patchedFlags ^= (1 << i);
					}
				}
			}

		}

		#endregion
	}

}
