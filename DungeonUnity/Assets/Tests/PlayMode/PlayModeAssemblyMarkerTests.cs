using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Dungeon.Tests.PlayMode
{
    public sealed class PlayModeAssemblyMarkerTests
    {
        [UnityTest]
        public IEnumerator AssemblyMarker_IsDiscoverable()
        {
            yield return null;
            Assert.Pass();
        }
    }
}
