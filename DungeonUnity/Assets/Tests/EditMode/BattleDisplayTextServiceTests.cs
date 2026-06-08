using Dungeon.Runtime.InGame.Battle.Services;
using Game.MasterData.Generated;
using NUnit.Framework;

namespace Dungeon.Tests.EditMode
{
    /// <summary>
    /// BattleDisplayTextServiceのEditモードテストクラス
    /// </summary>
    [TestFixture]
    public sealed class BattleDisplayTextServiceTests
    {
        [Test]
        public void GetNames_WithoutLocalization_ReturnsEnumNameFallback()
        {
            BattleDisplayTextService service = new BattleDisplayTextService();

            Assert.That(service.GetIntentName(IntentType.AttackDefend), Is.EqualTo(nameof(IntentType.AttackDefend)));
            Assert.That(service.GetStatusName(StatusType.Weak), Is.EqualTo(nameof(StatusType.Weak)));
            Assert.That(service.GetBuffName(BuffType.Ritual), Is.EqualTo(nameof(BuffType.Ritual)));
        }
    }
}
