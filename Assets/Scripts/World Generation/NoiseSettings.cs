using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "_NoiseSettings", menuName = "Custom / Noise Settings", order = 1)]
public class NoiseSettings : ScriptableObject
{
	[Range(1, 100)]
	public float Amplitude = 1f;

	[Range(1, 800)]
	public float Frequency = 10f;

	[Range(1, 8)]
	public int Octaves = 1;

	[Range(0, 5)]
	public float Lacunarity = 1;

	[Range(0, 5)]
	public float Persistance = 1;

	[Range(0, 5)]
	public float WeightedStrength = 1;

	public bool Invert = false;

	public FastNoiseLite.NoiseType NoiseType = FastNoiseLite.NoiseType.OpenSimplex2;
	public FastNoiseLite.FractalType FractalType = FastNoiseLite.FractalType.FBm;
	public ValueType DiscardValues = ValueType.Neither;

	[Header("Modifier noise settings")]
	//public NoiseSettings ModifierNoise = null;
	//public bool AffectsAmplitude = false;
	//public ModifierType amplitudeModType = ModifierType.Additive;
	public ModifierSettings AmplitudeModSettings;
	//public bool AffectsFrequency = false;
	//public ModifierType frequencyModType = ModifierType.Additive;
	public ModifierSettings FrequencyModSettings;
	//public bool AffectsOctaves = false;
	//public ModifierType octaveModType = ModifierType.Additive;
	public ModifierSettings OctaveModSettings;
	//public bool AffectsLacunarity = false;
	//public ModifierType lacunarityModType = ModifierType.Additive;
	public ModifierSettings LacunarityModSettings;
	//public bool AffectsPersistance = false;
	//public ModifierType persistanceModType = ModifierType.Additive;
	public ModifierSettings PersistanceModSettings;
	//public bool AffectsWeightedStrength = false;
	//public ModifierType weightedStrengthModType = ModifierType.Additive;
	public ModifierSettings WeightedStrengthModSettings;

	int seed;
	FastNoiseLite fnl;

	void SetFNL(Vector2 samplePos)
	{
		if (fnl == null)
		{
			fnl = new FastNoiseLite(seed);
		}

		fnl.SetSeed(seed);

		fnl.SetNoiseType(NoiseType);
		fnl.SetFractalType(FractalType);
		fnl.SetFrequency(Frequency / 1000);
		fnl.SetFractalOctaves(Octaves);
		fnl.SetFractalLacunarity(Lacunarity);
		fnl.SetFractalGain(Persistance);
		fnl.SetFractalWeightedStrength(WeightedStrength);

		// Frequency modifier
		if (FrequencyModSettings.ModifierNoise != null)
		{
			float modifierNoise = FrequencyModSettings.ModifierNoise.GetNoiseAt(samplePos, seed, FrequencyModSettings.IgnoreAmplitude);
			bool apply = true;

            switch (FrequencyModSettings.DiscardValues)
            {
                case ValueType.Negative:
					apply = modifierNoise > 0;
                    break;
                case ValueType.Positive:
					apply = modifierNoise < 0;
					break;
                case ValueType.Both:
					apply = false;
					break;
                case ValueType.Neither:
					apply = true;
                    break;
                default:
                    break;
            }

            if (modifierNoise != 0 && apply)
			{
				switch (FrequencyModSettings.ModifierType)
				{
					case ModifierType.Additive:
						fnl.SetFrequency(Frequency + modifierNoise / 1000);
						break;
					case ModifierType.Subtractive:
						fnl.SetFrequency(Frequency - modifierNoise / 1000);
						break;
					case ModifierType.Multiplicative:
						fnl.SetFrequency(Frequency * modifierNoise / 1000);
						break;
					case ModifierType.Divisive:
						fnl.SetFrequency(Frequency / modifierNoise / 1000);
						break;
					default:
						break;
				}
			}
		}

		// Octaves modifier
		if (OctaveModSettings.ModifierNoise != null)
		{
			float modifierNoise = OctaveModSettings.ModifierNoise.GetNoiseAt(samplePos, seed, OctaveModSettings.IgnoreAmplitude);
			bool apply = true;

			switch (OctaveModSettings.DiscardValues)
			{
				case ValueType.Negative:
					apply = modifierNoise > 0;
					break;
				case ValueType.Positive:
					apply = modifierNoise < 0;
					break;
				case ValueType.Both:
					apply = false;
					break;
				case ValueType.Neither:
					apply = true;
					break;
				default:
					break;
			}
			if (modifierNoise != 0 && apply)
			{
				switch (OctaveModSettings.ModifierType)
				{
					case ModifierType.Additive:
						fnl.SetFractalOctaves(Octaves + (int)modifierNoise);
						break;
					case ModifierType.Subtractive:
						fnl.SetFractalOctaves(Octaves - (int)modifierNoise);
						break;
					case ModifierType.Multiplicative:
						fnl.SetFractalOctaves(Octaves * (int)modifierNoise);
						break;
					case ModifierType.Divisive:
						fnl.SetFractalOctaves(Octaves / (int)modifierNoise);
						break;
					default:
						break;
				}
			}
		}

		// Lacunarity modifier
		if (LacunarityModSettings.ModifierNoise != null)
		{
			float modifierNoise = LacunarityModSettings.ModifierNoise.GetNoiseAt(samplePos, seed, LacunarityModSettings.IgnoreAmplitude);
			bool apply = true;

			switch (LacunarityModSettings.DiscardValues)
			{
				case ValueType.Negative:
                    apply = modifierNoise > 0;
                    break;
                case ValueType.Positive:
                    apply = modifierNoise < 0;
                    break;
                case ValueType.Both:
                    apply = false;
                    break;
                case ValueType.Neither:
                    apply = true;
                    break;
				default:
					break;
			}
			if (modifierNoise != 0 && apply)
			{
				switch (LacunarityModSettings.ModifierType)
				{
					case ModifierType.Additive:
						fnl.SetFractalLacunarity(Lacunarity + modifierNoise);
						break;
					case ModifierType.Subtractive:
						fnl.SetFractalLacunarity(Lacunarity - modifierNoise);
						break;
					case ModifierType.Multiplicative:
						fnl.SetFractalLacunarity(Lacunarity * modifierNoise);
						break;
					case ModifierType.Divisive:
						fnl.SetFractalLacunarity(Lacunarity / modifierNoise);
						break;
					default:
						break;
				}
			}
		}

		// Persistance modifier
		if (PersistanceModSettings.ModifierNoise != null)
		{
			float modifierNoise = PersistanceModSettings.ModifierNoise.GetNoiseAt(samplePos, seed, PersistanceModSettings.IgnoreAmplitude);
			bool apply = true;

			switch (PersistanceModSettings.DiscardValues)
			{
				case ValueType.Negative:
                    apply = modifierNoise > 0;
                    break;
                case ValueType.Positive:
                    apply = modifierNoise < 0;
                    break;
                case ValueType.Both:
                    apply = false;
                    break;
                case ValueType.Neither:
                    apply = true;
                    break;
				default:
					break;
			}
			if (modifierNoise != 0 && apply)
			{
				switch (PersistanceModSettings.ModifierType)
				{
					case ModifierType.Additive:
						fnl.SetFractalGain(Persistance + modifierNoise);
						break;
					case ModifierType.Subtractive:
						fnl.SetFractalGain(Persistance - modifierNoise);
						break;
					case ModifierType.Multiplicative:
						fnl.SetFractalGain(Persistance * modifierNoise);
						break;
					case ModifierType.Divisive:
						fnl.SetFractalGain(Persistance / modifierNoise);
						break;
					default:
						break;
				}
			}
		}

		// WeightedStrength modifier
		if (WeightedStrengthModSettings.ModifierNoise != null)
		{
			float modifierNoise = WeightedStrengthModSettings.ModifierNoise.GetNoiseAt(samplePos, seed, WeightedStrengthModSettings.IgnoreAmplitude);
			bool apply = true;

			switch (WeightedStrengthModSettings.DiscardValues)
			{
				case ValueType.Negative:
                    apply = modifierNoise > 0;
                    break;
                case ValueType.Positive:
                    apply = modifierNoise < 0;
                    break;
                case ValueType.Both:
                    apply = false;
                    break;
                case ValueType.Neither:
                    apply = true;
                    break;
				default:
					break;
			}
			if (modifierNoise != 0 && apply)
			{
				switch (WeightedStrengthModSettings.ModifierType)
				{
					case ModifierType.Additive:
						fnl.SetFractalWeightedStrength(WeightedStrength + modifierNoise);
						break;
					case ModifierType.Subtractive:
						fnl.SetFractalWeightedStrength(WeightedStrength - modifierNoise);
						break;
					case ModifierType.Multiplicative:
						fnl.SetFractalWeightedStrength(WeightedStrength * modifierNoise);
						break;
					case ModifierType.Divisive:
						fnl.SetFractalWeightedStrength(WeightedStrength / modifierNoise);
						break;
					default:
						break;
				}
			}
		}
	}

	/// <summary>
	/// Output range -1, 1.
	/// </summary>
	/// <param name="samplePosition"></param>
	/// <returns>The noise value at samplePosition.</returns>
	public float GetNoiseAt(Vector2 samplePosition, int seed, bool ignoreAmplitude)
	{
		this.seed = seed;
		float result;
		float amplitude = Amplitude;

		SetFNL(samplePosition);

		// Output range -1 ... 1
		if (Invert)
		{
			result = -fnl.GetNoise(samplePosition.x, samplePosition.y);
		}
		else
		{
			result = fnl.GetNoise(samplePosition.x, samplePosition.y);
		}

		// Amplitude modifier
		if (AmplitudeModSettings.ModifierNoise != null)
        {
			float modifierNoise = AmplitudeModSettings.ModifierNoise.GetNoiseAt(samplePosition, seed, AmplitudeModSettings.IgnoreAmplitude);
			bool apply = true;

			switch (AmplitudeModSettings.DiscardValues)
			{
				case ValueType.Negative:
                    apply = modifierNoise > 0;
                    break;
                case ValueType.Positive:
                    apply = modifierNoise < 0;
                    break;
                case ValueType.Both:
                    apply = false;
                    break;
                case ValueType.Neither:
                    apply = true;
                    break;
				default:
					break;
			}
			if (modifierNoise != 0 && apply)
			{
				switch (AmplitudeModSettings.ModifierType)
				{
					case ModifierType.Additive:
						amplitude += modifierNoise;
						break;
					case ModifierType.Subtractive:
						amplitude -= modifierNoise;
						break;
					case ModifierType.Multiplicative:
						amplitude *= modifierNoise;
						break;
					case ModifierType.Divisive:
						amplitude /= modifierNoise;
						break;
					default:
						break;
				}
			}
        }




		/*
		if(ModifierNoise != null)
        {
			float modNoise = ModifierNoise.GetNoiseAt(samplePosition, seed, false);

            if (AffectsAmplitude)
            {
				if(modNoise != 0)
                {
                    switch (amplitudeModType)
                    {
                        case ModifierType.Additive:
                            break;
                        case ModifierType.Subtractive:
                            break;
                        case ModifierType.Multiplicative:
                            break;
                        case ModifierType.Divisive:
                            break;
                        default:
                            break;
                    }
                }
            }
		}












		// Handle modifiers
		if (ModifierNoise != null && !ignoreAmplitude)
		{
			if (AffectsAmplitude)
			{
                switch (ModifierNoise.DiscardValues)
                {
                    case ValueType.Negative:
						if(result < 0)
                        {
                            switch (amplitudeModType)
                            {
                                case ModifierType.Additive:
									amplitude += ModifierNoise.GetNoiseAt(samplePosition, seed, false);
									break;
                                case ModifierType.Subtractive:
									amplitude -= ModifierNoise.GetNoiseAt(samplePosition, seed, false);
									break;
                                case ModifierType.Multiplicative:
									amplitude *= ModifierNoise.GetNoiseAt(samplePosition, seed, false);
									break;
                                case ModifierType.Divisive:
									amplitude /= ModifierNoise.GetNoiseAt(samplePosition, seed, false);
									break;
                                default:
                                    break;
                            }
                        }
						break;

                    case ValueType.Positive:
						if (result > 0)
                        {
							switch (amplitudeModType)
							{
								case ModifierType.Additive:
									amplitude += ModifierNoise.GetNoiseAt(samplePosition, seed, false);
									break;
								case ModifierType.Subtractive:
									amplitude -= ModifierNoise.GetNoiseAt(samplePosition, seed, false);
									break;
								case ModifierType.Multiplicative:
									amplitude *= ModifierNoise.GetNoiseAt(samplePosition, seed, false);
									break;
								case ModifierType.Divisive:
									amplitude /= ModifierNoise.GetNoiseAt(samplePosition, seed, false);
									break;
								default:
									break;
							}
						}
						break;

                    case ValueType.Both:

						switch (amplitudeModType)
						{
							case ModifierType.Additive:
								amplitude += ModifierNoise.GetNoiseAt(samplePosition, seed, false);
								break;
							case ModifierType.Subtractive:
								amplitude -= ModifierNoise.GetNoiseAt(samplePosition, seed, false);
								break;
							case ModifierType.Multiplicative:
								amplitude *= ModifierNoise.GetNoiseAt(samplePosition, seed, false);
								break;
							case ModifierType.Divisive:
								amplitude /= ModifierNoise.GetNoiseAt(samplePosition, seed, false);
								break;
							default:
								break;
						}
						break;

                    default:
                        break;
                }
			}
		}*/

        switch (DiscardValues)
        {
            case ValueType.Negative:
				if (result < 0)
					result = 0;
                break;
            case ValueType.Positive:
				if (result > 0)
					result = 0;
				break;
            case ValueType.Both:
				result = 0;
				break;
            case ValueType.Neither:
                break;
            default:
                break;
        }

        if (!ignoreAmplitude)
			return result * amplitude;
		else
			return result;
	}

	#region PREVIEW STUFF

	TerrainGenerator preview = null;
	private void OnValidate()
	{
		if (preview == null)
		{
			preview = FindObjectOfType<TerrainGenerator>();
			return;
		}

		preview.UpdatePreview();
	}

	#endregion
}

public enum ValueType
{
	Negative,
	Positive,
	Both,
	Neither,
}

public enum ModifierType
{
	Additive,
	Subtractive,
	Multiplicative,
	Divisive,
}

[System.Serializable]
public class ModifierSettings
{
	public NoiseSettings ModifierNoise = null;
	public ModifierType ModifierType = ModifierType.Additive;
	public ValueType DiscardValues = ValueType.Both;
	public bool IgnoreAmplitude = false;
}