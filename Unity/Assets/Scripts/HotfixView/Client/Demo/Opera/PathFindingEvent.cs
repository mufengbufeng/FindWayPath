using System.Collections.Generic;

namespace ET.Client
{
    [FriendOfAttribute(typeof(ET.Client.OperaComponent))]
    [Event(SceneType.Demo)]
    public class PathFindingEvent_StartQuery : AEvent<Scene, ChangeFindPathStateEvent>
    {
        protected override async ETTask Run(Scene scene, ChangeFindPathStateEvent args)
        {
            if (args.state != EControlState.Query)
                return;
            
            var currScene = scene.CurrentScene();
            var operaComponent = currScene.GetComponent<OperaComponent>();
            
            // 开始寻路
            operaComponent.StartPathfinding();
            
            await ETTask.CompletedTask;
        }
    }
    
    [FriendOfAttribute(typeof(ET.Client.OperaComponent))]
    [Event(SceneType.Demo)]
    public class PathResetEvent_Reset : AEvent<Scene, PathResetEvent>
    {
        protected override async ETTask Run(Scene scene, PathResetEvent args)
        {
            var currScene = scene.CurrentScene();
            var operaComponent = currScene.GetComponent<OperaComponent>();
            
            // 重置路径
            operaComponent.ResetPathfinding();
            
            await ETTask.CompletedTask;
        }
    }
    
    [FriendOfAttribute(typeof(ET.Client.OperaComponent))]
    [Event(SceneType.Demo)]
    public class AnimPathFindingEvent_StartAnimPathFinding : AEvent<Scene, AnimPathFindingEvent>
    {
        protected override async ETTask Run(Scene scene, AnimPathFindingEvent args)
        {
            var currScene = scene.CurrentScene();
            var operaComponent = currScene.GetComponent<OperaComponent>();
            
            // 开始动画寻路
            await operaComponent.StartAnimatedPathfinding();
            
            await ETTask.CompletedTask;
        }
    }
}