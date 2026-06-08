using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon.Runtime.InGame.Battle.Model;
using Game.MasterData.Generated;
using TFramework.Debug;
using TFramework.MasterData;

namespace Dungeon.Runtime.InGame.Battle.Services
{
    /// <summary>
    /// Event用のMasterData組み立てクラス
    /// </summary>
    public sealed class EventMasterDataFacade
    {
        private const int SupportedEventPoolId = 1;

        private readonly IMasterDataService _masterDataService;

        public EventMasterDataFacade(IMasterDataService masterDataService)
        {
            _masterDataService = masterDataService;
        }

        /// <summary>
        /// イベント定義一覧を構築する
        /// </summary>
        public IReadOnlyList<RuntimeEvent> BuildEvents(int eventPoolId)
        {
            if (eventPoolId != SupportedEventPoolId)
            {
                TLogger.Warning($"EventPoolId is not supported yet. id={eventPoolId}", "Battle");
                return Array.Empty<RuntimeEvent>();
            }

            IReadOnlyList<EventChoiceMaster> choiceMasters = _masterDataService.GetAll<EventChoiceMaster>();
            ILookup<int, EventChoiceMaster> choicesByEventId = choiceMasters.ToLookup(choice => choice.EventId);

            List<RuntimeEvent> events = new List<RuntimeEvent>();
            IReadOnlyList<EventMaster> eventMasters = _masterDataService.GetAll<EventMaster>();
            foreach (EventMaster master in eventMasters.OrderBy(master => master.Id))
            {
                List<RuntimeEventChoice> choices = new List<RuntimeEventChoice>();
                foreach (EventChoiceMaster choice in choicesByEventId[master.Id].OrderBy(choice => choice.ChoiceId))
                {
                    choices.Add(new RuntimeEventChoice(
                        choice.ChoiceId,
                        choice.LocalizationKey,
                        choice.EffectType,
                        choice.EffectValue));
                }

                events.Add(new RuntimeEvent(
                    master.Id,
                    master.EventName,
                    master.LocalizationKey,
                    master.ImageId,
                    choices));
            }

            return events;
        }
    }
}
