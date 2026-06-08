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
            builder.Register<Services.BattleMasterDataFacade>(Lifetime.Singleton).As<Services.IBattleMasterDataFacade>();
            builder.Register<Services.BattleDisplayTextService>(Lifetime.Singleton).As<Services.IBattleDisplayTextService>();
            builder.Register<Services.BattleSceneRules>(Lifetime.Singleton).As<Services.IBattleSceneRules>();
            builder.Register<Services.BattleSceneFlowService>(Lifetime.Scoped).As<Services.IBattleSceneFlowService>();
            builder.Register<BattlePagePresenter>(Lifetime.Scoped);
            builder.Register<BattleSceneUiCoordinator>(Lifetime.Scoped).As<IBattleSceneUiCoordinator>();
            builder.Register<BattleScenePresenter>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<BattleSceneController>().AsImplementedInterfaces();
        }
    }
}
