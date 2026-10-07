using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon.Runtime.OutGame.Common
{
    public class Badge : MonoBehaviour
    {
        private List<Func<bool>> _badgeConditionFuncs = new();

        public void RegistBadgeConditionFuncs(Func<bool> badgeConditionFunc)
        {
            _badgeConditionFuncs.Add(badgeConditionFunc);
        }

        public void UnregistBadgeConditionFuncs(Func<bool> badgeConditionFunc)
        {
            _badgeConditionFuncs.Remove(badgeConditionFunc);
        }

        public void UpdateBadge()
        {
            bool isActive = false;
            foreach (var func in _badgeConditionFuncs)
            {
                if (func())
                {
                    isActive = true;
                    break;
                }
            }

            gameObject.SetActive(isActive);
        }
    }
}