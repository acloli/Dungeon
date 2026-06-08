using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dungeon.Runtime.InGame.Battle.Services;
using Game.MasterData.Generated;
using NUnit.Framework;
using R3;
using TFramework.Localization;

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

        [Test]
        public void GetNames_WithLocalization_ReturnsLocalizedName()
        {
            FakeLocalizationService localizationService = new FakeLocalizationService();
            localizationService.Set("battle.intent.attack_defend", "攻防");
            localizationService.Set("battle.status.weak", "脱力");
            localizationService.Set("battle.buff.ritual", "儀式");
            BattleDisplayTextService service = new BattleDisplayTextService(localizationService);

            Assert.That(service.GetIntentName(IntentType.AttackDefend), Is.EqualTo("攻防"));
            Assert.That(service.GetStatusName(StatusType.Weak), Is.EqualTo("脱力"));
            Assert.That(service.GetBuffName(BuffType.Ritual), Is.EqualTo("儀式"));
        }

        /// <summary>
        /// テスト用LocalizationService
        /// </summary>
        private sealed class FakeLocalizationService : ILocalizationService
        {
            private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

            public LanguageCode CurrentLanguage { get; set; } = LanguageCode.Japanese;
            public LanguageCode[] SupportedLanguages { get; } = { LanguageCode.Japanese };
            public Observable<LanguageCode> OnLanguageChanged => null;

            public void Set(string key, string value)
            {
                _values[key] = value;
            }

            public string Get(string key)
            {
                return _values.TryGetValue(key, out string value) ? value : key;
            }

            public string Get(string key, params object[] args)
            {
                return string.Format(Get(key), args);
            }

            public bool HasKey(string key)
            {
                return _values.ContainsKey(key);
            }

            public UniTask LoadLanguageAsync(LanguageCode language, CancellationToken ct)
            {
                CurrentLanguage = language;
                return UniTask.CompletedTask;
            }
        }
    }
}
