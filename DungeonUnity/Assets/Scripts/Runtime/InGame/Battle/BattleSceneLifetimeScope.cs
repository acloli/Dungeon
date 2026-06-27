using VContainer;
using VContainer.Unity;

namespace Dungeon.Runtime.InGame.Battle
{
    /// <summary>
    /// BattleSceneの依存登録クラス
    /// </summary>
    public class BattleSceneLifetimeScope : LifetimeScope
    {
        /// <summary>
        /// BattleScene用依存登録
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<Save.Services.RunSaveService>(Lifetime.Scoped).As<Save.Services.IRunSaveService>();
            builder.Register<Services.BattleRandomProvider>(Lifetime.Singleton).As<Services.IBattleRandomProvider>();
            builder.Register<Services.EventMasterDataFacade>(Lifetime.Singleton);
            builder.Register<Services.ShopMasterDataFacade>(Lifetime.Singleton);
            builder.Register<Services.BattleMasterDataFacade>(Lifetime.Singleton).As<Services.IBattleMasterDataFacade>();
            builder.Register<Services.BattleDisplayTextService>(Lifetime.Singleton).As<Services.IBattleDisplayTextService>();
            builder.Register<Services.BattleRewardService>(Lifetime.Singleton).As<Services.IBattleRewardService>();
            builder.Register<Services.BattleRewardFlowService>(Lifetime.Singleton).As<Services.IBattleRewardFlowService>();
            builder.Register<Services.BattleRelicService>(Lifetime.Singleton).As<Services.IBattleRelicService>();
            builder.Register<Services.BattlePotionService>(Lifetime.Singleton).As<Services.IBattlePotionService>();
            builder.Register<Services.BattleCombatEventService>(Lifetime.Singleton).As<Services.IBattleCombatEventService>();
            builder.Register<Services.BattleEventService>(Lifetime.Singleton).As<Services.IBattleEventService>();
            builder.Register<Services.BattleEventFlowService>(Lifetime.Singleton).As<Services.IBattleEventFlowService>();
            builder.Register<Services.BattleShopService>(Lifetime.Singleton).As<Services.IBattleShopService>();
            builder.Register<Services.BattleRestShopFlowService>(Lifetime.Singleton).As<Services.IBattleRestShopFlowService>();
            builder.Register<Services.BattleCheckpointService>(Lifetime.Singleton).As<Services.IBattleCheckpointService>();
            builder.Register<Services.BattleSnapshotFactory>(Lifetime.Singleton).As<Services.IBattleSnapshotFactory>();
            builder.Register<Services.BattleDeckService>(Lifetime.Singleton).As<Services.IBattleDeckService>();
            builder.Register<Services.BattleEncounterSelector>(Lifetime.Singleton).As<Services.IBattleEncounterSelector>();
            builder.Register<Services.BattleRewardRollService>(Lifetime.Singleton).As<Services.IBattleRewardRollService>();
            builder.Register<Services.BattleSceneRules>(Lifetime.Singleton).As<Services.IBattleSceneRules>();
            builder.Register<Services.BattleSceneFlowService>(Lifetime.Scoped).As<Services.IBattleSceneFlowService>();
            builder.Register<BattlePagePresenter>(Lifetime.Scoped);
            builder.Register<BattleSceneUiCoordinator>(Lifetime.Scoped).As<IBattleSceneUiCoordinator>();
            builder.Register<BattleScenePresenter>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<BattleSceneController>().AsImplementedInterfaces();
        }
    }
}
