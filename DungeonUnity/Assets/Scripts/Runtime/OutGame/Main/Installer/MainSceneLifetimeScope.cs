using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace Dungeon.Runtime.OutGame.Main
{
    public class MainSceneLifetimeScope : LifetimeScope
    {
        [SerializeField] private MainMenuPreviewContent _previewContent;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<MainRunProfileService>(Lifetime.Scoped).As<IMainRunProfileService>();
            builder.Register<Dungeon.Runtime.InGame.Save.Services.RunSaveService>(Lifetime.Scoped).As<Dungeon.Runtime.InGame.Save.Services.IRunSaveService>();
            builder.RegisterInstance(_previewContent);
            builder.Register<MainMenuPreviewService>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<MainSceneUiCoordinator>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<MainSceneController>().AsImplementedInterfaces();
        }
    }
}
