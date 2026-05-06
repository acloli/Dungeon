using Dungeon.Infrastructure.MasterData;
using NUnit.Framework;

namespace Dungeon.Tests.Editor
{
    public class DungeonBootstrapTests
    {
        [Test]
        public void MasterDataBootstrap_UsesExpectedAssetRoot()
        {
            Assert.That(DungeonMasterDataBootstrap.MasterDataAssetRoot, Is.EqualTo("Assets/ScriptableObjects/MasterData"));
        }

        [Test]
        public void MasterDataBootstrap_UsesExpectedRuntimeRoot()
        {
            Assert.That(DungeonMasterDataBootstrap.RuntimeScriptRoot, Is.EqualTo("Assets/Scripts"));
        }
    }
}
