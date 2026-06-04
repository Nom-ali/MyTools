using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public sealed class AssetReferenceMaterial : AssetReferenceT<Material>
{
    public AssetReferenceMaterial(string guid) : base(guid) { }
}