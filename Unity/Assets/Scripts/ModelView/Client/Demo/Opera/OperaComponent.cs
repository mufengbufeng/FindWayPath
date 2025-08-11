using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    public enum EControlState
    {
        // 默认状态
        None,
        // 起点状态
        Start,
        // 终点状态
        End,
        // 障碍状态
        Obstacle,
        // 寻路状态
        Query,
        // 教学状态
        Teach
    }

    [ComponentOf(typeof(Scene))]
    public class OperaComponent : Entity, IAwake, IUpdate
    {
        public EControlState ControlState = EControlState.None;
        public Vector3 ClickPoint;

        public int mapMask;

        public GameObject World;
        public Terrain Terrain;
        public GameObject PathCubeRoot;

        public Dictionary<int, EntityRef<PathCubeComponent>> PathCubeDic = new();

        // GameObject对象池
        public Queue<GameObject> CubePool = new Queue<GameObject>();
    }
}
