using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UILobbyComponent))]
    [FriendOf(typeof(UILobbyComponent))]
    public static partial class UILobbyComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UILobbyComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

            self.enterMap = rc.Get<GameObject>("EnterMap");
            self.enterMap.GetComponent<Button>().onClick.AddListener(() => { self.EnterMap().Coroutine(); });
        }

        public static async ETTask EnterMap(this UILobbyComponent self)
        {
            Scene root = self.Root();



            // await LSSceneChangeHelper.SceneChangeTo(root, "Map1", 0);
            // await root.GetComponent<ObjectWait>().Wait<Wait_SceneChangeFinish>();
            // EventSystem.Instance.Publish(root, new EnterMapFinish());
            await SceneChangeHelper.SceneChangeTo(root, "Map1", 0);
            await UIHelper.Remove(root, UIType.UILobby);
            return;
            await EnterMapHelper.EnterMapAsync(root);
        }
    }
}