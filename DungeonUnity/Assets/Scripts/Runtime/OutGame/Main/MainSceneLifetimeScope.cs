using VContainer;
using VContainer.Unity;

namespace Dungeon.Runtime.OutGame.Main
{
    public class MainSceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<MainSceneController>().AsImplementedInterfaces();
        }
    }
}
