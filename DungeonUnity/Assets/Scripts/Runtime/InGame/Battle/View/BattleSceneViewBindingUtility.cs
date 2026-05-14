using UnityEngine;

namespace Dungeon.Runtime.InGame.Battle.View
{
    /// <summary>
    /// BattleScene用参照探索補助クラス
    /// </summary>
    internal static class BattleSceneViewBindingUtility
    {
        /// <summary>
        /// 指定名GameObject上のComponent探索
        /// </summary>
        public static T FindComponent<T>(string gameObjectName) where T : Component
        {
            T[] components = Resources.FindObjectsOfTypeAll<T>();
            for (int i = 0; i < components.Length; i++)
            {
                T component = components[i];
                if (component == null)
                {
                    continue;
                }

                GameObject gameObject = component.gameObject;
                if (!gameObject.scene.IsValid() || gameObject.name != gameObjectName)
                {
                    continue;
                }

                return component;
            }

            return null;
        }

        /// <summary>
        /// 指定名Transform探索
        /// </summary>
        public static Transform FindTransform(string gameObjectName)
        {
            return FindComponent<Transform>(gameObjectName);
        }
    }
}
