using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace Dungeon.Runtime.OutGame.Main
{
    public class MainSceneLifetimeScope : LifetimeScope
    {
        [SerializeField] private MenuPresentationCatalog _presentationCatalog;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<MainRunProfileService>(Lifetime.Scoped).As<IMainRunProfileService>();
            builder.Register<Dungeon.Runtime.InGame.Save.Services.RunSaveService>(Lifetime.Scoped).As<Dungeon.Runtime.InGame.Save.Services.IRunSaveService>();
            builder.RegisterInstance(_presentationCatalog);
            builder.Register<Dungeon.Runtime.InGame.Save.Services.CollectionProgressService>(Lifetime.Scoped).As<Dungeon.Runtime.InGame.Save.Services.ICollectionProgressService>();
            builder.Register<MenuMasterDataFacade>(Lifetime.Scoped);
            builder.Register<MainMenuDataService>(Lifetime.Scoped).As<IMainMenuDataSource>();
            builder.Register<MainMenuActionHandler>(Lifetime.Scoped).As<IMainMenuActionHandler>();
            builder.Register<MainSceneUiCoordinator>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<MainSceneController>().AsImplementedInterfaces();
        }
    }
}
