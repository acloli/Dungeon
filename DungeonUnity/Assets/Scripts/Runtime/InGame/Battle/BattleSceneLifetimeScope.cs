using VContainer;
using VContainer.Unity;

namespace Dungeon.Runtime.InGame.Battle
{
    public class BattleSceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<Services.BattleSceneRules>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<BattleSceneController>().AsImplementedInterfaces();
        }
    }
}
