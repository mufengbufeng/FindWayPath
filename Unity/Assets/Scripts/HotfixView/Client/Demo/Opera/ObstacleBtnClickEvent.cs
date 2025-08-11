
namespace ET.Client
{
    [FriendOfAttribute(typeof(ET.Client.OperaComponent))]
    [Event(SceneType.Demo)]
    public class ObstacleBtnClickEvent_ChangeState : AEvent<Scene, ChangeFindPathStateEvent>
    {
        protected override async ETTask Run(Scene scene, ChangeFindPathStateEvent args)
        {
            var currScene = scene.CurrentScene();
            var operaComponent = currScene.GetComponent<OperaComponent>();
            operaComponent.ControlState = args.state;
            Log.Info($"切换状态到{args.state}");
            // operaComponent.OnControlStateChanged();
            await ETTask.CompletedTask;

        }

    }
}