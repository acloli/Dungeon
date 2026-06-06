using Game.MasterData.Generated;
using TFramework.Localization;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// Battle表示名解決クラス
    /// </summary>
    public sealed class BattleDisplayTextService : IBattleDisplayTextService
    {
        private readonly ILocalizationService _localizationService;

        public BattleDisplayTextService(ILocalizationService localizationService = null)
        {
            _localizationService = localizationService;
        }

        /// <summary>
        /// 敵意図表示名取得
        /// </summary>
        public string GetIntentName(IntentType intentType)
        {
            return Resolve(GetIntentKey(intentType), intentType.ToString());
        }

        /// <summary>
        /// 状態表示名取得
        /// </summary>
        public string GetStatusName(StatusType statusType)
        {
            return Resolve(GetStatusKey(statusType), statusType.ToString());
        }

        /// <summary>
        /// buff表示名取得
        /// </summary>
        public string GetBuffName(BuffType buffType)
        {
            return Resolve(GetBuffKey(buffType), buffType.ToString());
        }

        /// <summary>
        /// localized文言解決
        /// </summary>
        private string Resolve(string key, string fallback)
        {
            if (_localizationService == null || string.IsNullOrEmpty(key))
            {
                return fallback;
            }

            if (!_localizationService.HasKey(key))
            {
                return fallback;
            }

            return _localizationService.Get(key);
        }

        /// <summary>
        /// 敵意図localization key取得
        /// </summary>
        private static string GetIntentKey(IntentType intentType)
        {
            return intentType switch
            {
                IntentType.Attack => "battle.intent.attack",
                IntentType.AttackDebuff => "battle.intent.attack_debuff",
                IntentType.AttackDefend => "battle.intent.attack_defend",
                IntentType.Buff => "battle.intent.buff",
                IntentType.Debuff => "battle.intent.debuff",
                IntentType.Idle => "battle.intent.idle",
                IntentType.Special => "battle.intent.special",
                _ => string.Empty
            };
        }

        /// <summary>
        /// 状態localization key取得
        /// </summary>
        private static string GetStatusKey(StatusType statusType)
        {
            return statusType switch
            {
                StatusType.Weak => "battle.status.weak",
                StatusType.Vulnerable => "battle.status.vulnerable",
                StatusType.Slimed => "battle.status.slimed",
                _ => string.Empty
            };
        }

        /// <summary>
        /// buff localization key取得
        /// </summary>
        private static string GetBuffKey(BuffType buffType)
        {
            return buffType switch
            {
                BuffType.Strength => "battle.buff.strength",
                BuffType.Ritual => "battle.buff.ritual",
                BuffType.Enrage => "battle.buff.enrage",
                _ => string.Empty
            };
        }
    }
}
