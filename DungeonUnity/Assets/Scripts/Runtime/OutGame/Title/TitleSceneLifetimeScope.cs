using VContainer;
using VContainer.Unity;

namespace Dungeon.Runtime.OutGame.Title
{
    public class TitleSceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<TitleSceneController>().AsImplementedInterfaces();
        }
    }
}
