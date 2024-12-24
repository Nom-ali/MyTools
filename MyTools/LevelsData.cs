
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "LevelsData", menuName = "Scriptables/LevelsData")]
public class LevelsData : ScriptableObject
{
    [SerializeField] private Data[] MobileCases;

    [MyBox.Separator("************** Sticker **************")]
    public AssetReferenceGameObject StickerPrefab; 
    
    [MyBox.Separator("************** Chian **************")]
    public AssetReferenceGameObject[] ChainPrefabs;

    public Data GetReference(int caseTypeID)
    {
        return System.Array.Find(MobileCases, item => item.CaseID == caseTypeID);
    }        

    public AssetReferenceGameObject GetChainReference(int chainID)
    {
        return ChainPrefabs[chainID];
    }

}

[System.Serializable]
public struct Data
{
    public int CaseID;
    public AssetReferenceGameObject CasePrefab;
    public AssetReferenceGameObject ColoredCasePrefab;
    public AssetReferenceSprite WhiteSprite;

    [Space]
    public AssetReferenceSprite[] SimpleSprites;

    [Space]
    public AssetReferenceSprite[] GradientSprites;

    [Space]
    public AssetReferenceSprite[] GlitterSprites;
}

[System.Serializable] 
public enum ColorScheme
{
    Simple, Gradient, Glitter
}