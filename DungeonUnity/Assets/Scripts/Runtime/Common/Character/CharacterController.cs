using System.Threading;
using Cysharp.Threading.Tasks;
using Game.MasterData.Generated;
using TFramework.MasterData;
using UnityEngine;
using VContainer;

namespace Dungeon.Runtime.Common.Character
{
    public class CharacterController
    {
        [SerializeField] private CharacterView _view;

        private enum CharacterState
        {
            Stable,
            Idle,
            Jump,
        }

        private CharacterState _currentState = CharacterState.Stable;

        private readonly IMasterDataService _masterDataService;

        [Inject]
        public CharacterController(IMasterDataService masterDataService)
        {
            _masterDataService = masterDataService;
        }

        public UniTask SetCharacterAsync(int characterId, bool isClickable = true, bool isShowDaiza = true,
            CancellationToken token = default)
        {
            var characterMasterData = _masterDataService.Get<CardMaster, int>(characterId);


            return UniTask.CompletedTask;
        }
    }
}