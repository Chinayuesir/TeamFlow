using System.Collections.Generic;
using RoslynCSharp;
using UnityEngine;

namespace TeamFlow
{
    [CreateAssetMenu(fileName = "Assemblies", menuName = "TeamFlow/Assemblies", order = 0)]
    public class AssemblyReferences_SO : ScriptableObject
    {
        public List<AssemblyReferenceAsset> AssemblyReferenceAssets;
    }
}