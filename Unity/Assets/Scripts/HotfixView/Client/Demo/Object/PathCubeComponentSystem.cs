using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(PathCubeComponent))]
    [FriendOfAttribute(typeof(ET.Client.PathCubeComponent))]
    public static partial class PathCubeComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.PathCubeComponent self)
        {

        }

        public static void SetData(this ET.Client.PathCubeComponent self, GameObject cube, CubeState state, int index)
        {
            self.Cube = cube;
            self.State = state;
            self.Index = index;

            // 设置初始状态颜色
            self.ChangeState(state);
        }

        public static void ChangeState(this ET.Client.PathCubeComponent self, CubeState newState)
        {
            if (self.Cube == null)
                return;

            self.State = newState;

            Renderer renderer = self.Cube.GetComponent<Renderer>();
            if (renderer == null)
                return;

            // 根据状态设置颜色
            Color cubeColor = newState switch
            {
                CubeState.None => Color.gray,      // 灰色
                CubeState.Start => Color.green,    // 绿色
                CubeState.End => Color.red,        // 红色
                CubeState.Obstacle => Color.black, // 黑色
                CubeState.Query => Color.yellow,   // 黄色
                CubeState.Path => Color.blue,      // 蓝色
                _ => Color.gray
            };

            renderer.material.color = cubeColor;
        }

        public static CubeState GetState(this ET.Client.PathCubeComponent self)
        {
            return self.State;
        }

        public static GameObject RecycleCube(this ET.Client.PathCubeComponent self)
        {
            if (self.Cube == null)
                return null;

            // 停用立方体并重置父对象
            self.Cube.SetActive(false);
            self.Cube.transform.SetParent(null);

            GameObject cube = self.Cube;
            self.Cube = null;
            self.State = CubeState.None;

            return cube;
        }




    }
}