using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

// ReSharper disable RequiredBaseTypesConflict
// ReSharper disable InconsistentNaming

// ----------------------------------------------------------------------------
// Author: Alexandre Brull
// https://brullalex.itch.io/
// ----------------------------------------------------------------------------

//TODO: Setup all the rules also for using RandomAnimation (also add sprites to TileAnimations array!)

[ExecuteInEditMode]
[CreateAssetMenu(fileName = "New Auto Rule Tile", menuName = "Tiles/Auto Rule Tile")]
public class AutoRuleTile : ScriptableObject
{
    [SerializeField] private List<Texture2D> SpriteSheets;
    [SerializeField] private AdvancedRuleTile RuleTileTemplate;
    [SerializeField] private RuleTile.TilingRuleOutput.OutputSprite OutputType;
    
    private AdvancedRuleTile ruleTileTemplate_Default;

    private void Awake()
    {
        // If there is a default template, load it when the asset is created.
        ruleTileTemplate_Default = Resources.Load("JoiningAutoRuleTile_default") as AdvancedRuleTile;
        if (ruleTileTemplate_Default != null)
        {
            RuleTileTemplate = ruleTileTemplate_Default;
        }
    }

    private void Reset()
    {
        // Load all the selected textures to the sprite sheets array.
        List<Texture2D> selections = Selection.objects.OfType<Texture2D>().ToList();
        SpriteSheets = selections;
    }

#if (UNITY_EDITOR)
    public void OverrideRuleTile()
    {
        // Make a copy of the Rule Tile Template from a new asset.
        AdvancedRuleTile _new = CreateInstance<AdvancedRuleTile>();
        EditorUtility.CopySerialized(RuleTileTemplate, _new);

        // List of all the spriteSheets as Sprite[]:s
        List<Sprite[]> sprites = new List<Sprite[]>();
        
        for (int i = 0; i < SpriteSheets.Count; i++)
        {
            sprites.Add(new Sprite[SpriteSheets.Count]);
            string path = AssetDatabase.GetAssetPath(SpriteSheets[i]);
            sprites[i] = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        }

        string path1 = AssetDatabase.GetAssetPath(SpriteSheets[0]);
        Debug.Log("Loading at path " + path1);
        Sprite[] assets = AssetDatabase.LoadAllAssetsAtPath(path1).OfType<Sprite>().ToArray();
        
        foreach (var sprite in assets)
        {
            Debug.Log(sprite.name);
        }

        if (sprites[0].Length != RuleTileTemplate.m_TilingRules.Count)
        {
            Debug.LogWarning("The Spritesheet doesn't have the same number of sprites than the Rule Tile template has rules.");
        }

        // Create all the tile rules
        for (int ruleIndex = 0; ruleIndex < RuleTileTemplate.m_TilingRules.Count; ruleIndex++)
        {
            if (SpriteSheets.Count > 1)
            {
                _new.m_TilingRules[ruleIndex].m_Output = OutputType;
            }

            // Initialize a array for all the animation sprites
            _new.m_TilingRules[ruleIndex].m_Sprites = new Sprite[SpriteSheets.Count];
            
            // Add all the animation sprites to the m_Sprites array
            for (int animSheetIndex = 0; animSheetIndex < SpriteSheets.Count; animSheetIndex++)
            {
                _new.m_TilingRules[ruleIndex].m_Sprites[animSheetIndex] = sprites[animSheetIndex][ruleIndex];
                
                //Debug.Log("Rule Index " + ruleIndex + " is now " + spriteSheets[animSheetIndex][ruleIndex] + " (" + animSheetIndex + ")");
            }
            
            // Set the default sprite
            _new.m_DefaultSprite = sprites[0][24];
        }

        // Replace this Asset with the new one.
        AssetDatabase.CreateAsset(_new, AssetDatabase.GetAssetPath(this));
    }
#endif
}

