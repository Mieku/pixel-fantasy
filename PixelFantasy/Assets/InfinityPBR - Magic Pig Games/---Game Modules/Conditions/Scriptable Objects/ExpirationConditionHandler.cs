using System;
using UnityEngine;


namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/conditions/expiration-condition-handler")]
    [Serializable]
    public abstract class ExpirationConditionHandler : ModulesScriptableObject
    {
        public abstract bool HandleExpiration(IHaveConditions owner, Condition conditionToAdd, IHaveStats source = null);
    }
}