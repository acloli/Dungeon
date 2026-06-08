using VContainer;
using VContainer.Unity;

namespace Dungeon.Runtime.OutGame.Main
{
    public class MainSceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<MainRunProfileService>(Lifetime.Scoped).As<IMainRunProfileService>();
            builder.Register<Dungeon.Runtime.InGame.Save.Services.RunSaveService>(Lifetime.Scoped).As<Dungeon.Runtime.InGame.Save.Services.IRunSaveService>();
            builder.RegisterComponentInHierarchy<MainSceneController>().AsImplementedInterfaces();
        }
    }
}
