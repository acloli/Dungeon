using Dungeon.Runtime.InGame.Battle.Model;
using Dungeon.Runtime.SceneFlow;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleRunProfileResolverのEditモードテストクラス
    /// </summary>
    [TestFixture]
    public sealed class BattleRunProfileResolverTests
    {
        [Test]
        public void ResolveRunProfileId_ValidBridgeData_ReturnsBridgeValue()
        {
            BattleRunBridgeData bridgeData = new BattleRunBridgeData(6601);

            int runProfileId = BattleRunProfileResolver.ResolveRunProfileId(bridgeData, 5501);

            Assert.That(runProfileId, Is.EqualTo(6601));
        }

        [Test]
        public void ResolveRunProfileId_MissingBridgeData_ReturnsFallbackValue()
        {
            int runProfileId = BattleRunProfileResolver.ResolveRunProfileId(null, 5501);

            Assert.That(runProfileId, Is.EqualTo(5501));
        }

        [Test]
        public void ResolveRunProfileId_InvalidBridgeData_ReturnsFallbackValue()
        {
            BattleRunBridgeData bridgeData = new BattleRunBridgeData(0);

            int runProfileId = BattleRunProfileResolver.ResolveRunProfileId(bridgeData, 5501);

            Assert.That(runProfileId, Is.EqualTo(5501));
        }

        [Test]
        public void ResolveRunProfileId_InvalidBridgeDataAndFallback_ReturnsInvalidValue()
        {
            BattleRunBridgeData bridgeData = new BattleRunBridgeData(0);

            int runProfileId = BattleRunProfileResolver.ResolveRunProfileId(bridgeData, 0);

            Assert.That(runProfileId, Is.EqualTo(0));
        }
    }
}
