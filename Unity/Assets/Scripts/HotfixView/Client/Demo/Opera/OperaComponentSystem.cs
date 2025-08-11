using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ET.Client
{
    [EntitySystemOf(typeof(OperaComponent))]
    [FriendOf(typeof(OperaComponent))]
    [FriendOfAttribute(typeof(FindPathDataComponent))]
    [FriendOfAttribute(typeof(ET.Client.PathfindingComponent))]
    [FriendOfAttribute(typeof(ET.Client.BFSComponent))]
    [FriendOfAttribute(typeof(ET.Client.DFSComponent))]
    [FriendOfAttribute(typeof(ET.Client.AStarComponent))]
    [FriendOfAttribute(typeof(ET.Client.JPSComponent))]
    public static partial class OperaComponentSystem
    {
        [EntitySystem]
        private static void Awake(this OperaComponent self)
        {
            self.mapMask = LayerMask.GetMask("Map");
            self.World = GameObject.Find("World");
            var rc = self.World.GetComponent<ReferenceCollector>();

            self.Terrain = rc.Get<GameObject>("Terrain").GetComponent<Terrain>();
            self.PathCubeRoot = rc.Get<GameObject>("PathCubeRoot");

            if (self.Scene().GetComponent<FindPathDataComponent>() == null)
            {
                var findPathDataComponent = self.Scene().AddComponent<FindPathDataComponent>();
                // 初始化Index为-1，表示未设置起点和终点
                findPathDataComponent.StartIndex = -1;
                findPathDataComponent.EndIndex = -1;
            }

            if (self.Scene().GetComponent<PathfindingComponent>() == null)
            {
                self.Scene().AddComponent<PathfindingComponent>();
            }

            // 添加所有算法组件
            if (self.Scene().GetComponent<BFSComponent>() == null)
            {
                self.Scene().AddComponent<BFSComponent>();
            }

            if (self.Scene().GetComponent<DFSComponent>() == null)
            {
                self.Scene().AddComponent<DFSComponent>();
            }

            if (self.Scene().GetComponent<AStarComponent>() == null)
            {
                self.Scene().AddComponent<AStarComponent>();
            }

            if (self.Scene().GetComponent<JPSComponent>() == null)
            {
                self.Scene().AddComponent<JPSComponent>();
            }
        }

        [EntitySystem]
        private static void Update(this OperaComponent self)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                // 根据Terrain的长宽初始化立方体 terrain width
                int width = (int)self.Terrain.terrainData.size.x;
                int height = (int)self.Terrain.terrainData.size.z;
                self.LoadCubes(width, height, 1);
                Log.Info("Cubes Loaded");
            }

            // 鼠标左键点击检测 // 没有
            if (Input.GetMouseButtonDown(0))
            {
                self.HandleMouseClick();
            }
        }

        // 加载立方体 Assets\Scripts\Core\World\Module\ObjectPool\ObjectPool.cs
        private static void LoadCubes(this OperaComponent self, int xCount, int yCount, float cellSize)
        {
            // 清理之前的立方体并回收到对象池
            if (self.PathCubeRoot.transform.childCount > 0)
            {
                foreach (var kvp in self.PathCubeDic)
                {
                    PathCubeComponent pathCube = kvp.Value;
                    if (pathCube != null)
                    {
                        GameObject cube = pathCube.RecycleCube();
                        if (cube != null)
                        {
                            self.CubePool.Enqueue(cube);
                        }
                        pathCube.Dispose();
                    }
                }
                self.PathCubeDic.Clear();
            }

            var findPathDataComponent = self.Scene().GetComponent<FindPathDataComponent>();
            var pathfindingComponent = self.Scene().GetComponent<PathfindingComponent>();

            // 异步加载立方体预制体
            var cubeTask = ResourcesComponent.Instance.LoadAssetAsync<GameObject>("Cube");
            cubeTask.GetAwaiter().OnCompleted(() =>
            {
                GameObject cubePrefab = cubeTask.GetAwaiter().GetResult();
                findPathDataComponent.CubeStatesArr = new CubeState[xCount, yCount];

                // 初始化寻路组件网格大小
                pathfindingComponent.InitGrid(xCount, yCount);
                var size = new Vector3(0.8f, 1f, 0.8f);
                // 生成立方体网格 - 使用对象池
                for (int x = 0; x < xCount; x++)
                {
                    for (int z = 0; z < yCount; z++)
                    {
                        GameObject cubeInstance;
                        // 从对象池获取立方体，如果池中没有则新建
                        if (self.CubePool.Count > 0)
                        {
                            cubeInstance = self.CubePool.Dequeue();
                        }
                        else
                        {
                            cubeInstance = GameObject.Instantiate(cubePrefab);
                        }
                        cubeInstance.tag = "Cube";
                        // 设置父对象和激活状态
                        cubeInstance.transform.SetParent(self.PathCubeRoot.transform);
                        cubeInstance.SetActive(true);

                        // 计算世界位置
                        Vector3 worldPos = new Vector3(x * cellSize, 0, z * cellSize);
                        cubeInstance.transform.position = worldPos;
                        cubeInstance.transform.localScale = size;

                        var component = self.AddChild<PathCubeComponent>(cubeInstance);
                        int index = x * yCount + z;
                        component.SetData(cubeInstance, CubeState.None, index);

                        var v2 = new Vector2(x, z);
                        self.PathCubeDic[index] = component;

                        findPathDataComponent.CubeStatesArr[x, z] = CubeState.None;
                        // 设置默认状态
                        self.SetCubeState(x, z, CubeState.None);
                    }
                }
            });
        }

        // 处理鼠标点击
        private static void HandleMouseClick(this OperaComponent self)
        {
            // 如果鼠标在UI上则不进行射线检测
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Camera camera = Camera.main;
            if (camera == null) return;

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 射线检测，只检测Cube层
            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitObject = hit.collider.gameObject;

                // 检查是否点击的是Cube
                if (hitObject.CompareTag("Cube"))
                {
                    Vector3 cubePos = hitObject.transform.position;
                    // 根据立方体的世界坐标计算网格坐标
                    int x = Mathf.RoundToInt(cubePos.x);
                    int z = Mathf.RoundToInt(cubePos.z);

                    var findPathDataComponent = self.Scene().GetComponent<FindPathDataComponent>();
                    CubeState currentState = findPathDataComponent.CubeStatesArr[x, z];
                    CubeState targetState;

                    // 根据当前控制状态设置立方体状态
                    switch (self.ControlState)
                    {
                        case EControlState.Start:
                            targetState = CubeState.Start;
                            // 起点只能有一个
                            self.ClearStateInGrid(CubeState.Start);
                            findPathDataComponent.StartIndex = self.GetCubeIndex(x, z);
                            break;

                        case EControlState.End:
                            targetState = CubeState.End;
                            // 终点只能有一个
                            self.ClearStateInGrid(CubeState.End);
                            findPathDataComponent.EndIndex = self.GetCubeIndex(x, z);
                            break;

                        case EControlState.Obstacle:
                            // 障碍物切换模式：如果当前是障碍物，则切换为None；否则设为障碍物
                            if (currentState == CubeState.Obstacle)
                            {
                                targetState = CubeState.None;
                                Log.Info($"点击立方体 ({x}, {z})，移除障碍物");
                            }
                            else
                            {
                                targetState = CubeState.Obstacle;
                                Log.Info($"点击立方体 ({x}, {z})，设置为障碍物");
                            }
                            break;

                        case EControlState.None:
                            targetState = CubeState.None;
                            break;

                        default:
                            targetState = CubeState.None;
                            break;
                    }

                    self.SetCubeState(x, z, targetState);
                    findPathDataComponent.CubeStatesArr[x, z] = targetState;
                }
            }
        }

        // 清除网格中指定状态的立方体
        private static void ClearStateInGrid(this OperaComponent self, CubeState stateToClear)
        {
            var findPathDataComponent = self.Scene().GetComponent<FindPathDataComponent>();
            if (findPathDataComponent.CubeStatesArr == null) return;

            int width = findPathDataComponent.CubeStatesArr.GetLength(0);
            int height = findPathDataComponent.CubeStatesArr.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    if (findPathDataComponent.CubeStatesArr[x, z] == stateToClear)
                    {
                        self.SetCubeState(x, z, CubeState.None);
                        findPathDataComponent.CubeStatesArr[x, z] = CubeState.None;
                        // 注意：这里不重置Index，Index由设置新位置的代码管理
                    }
                }
            }
        }

        // 获取立方体索引
        private static int GetCubeIndex(this OperaComponent self, int x, int z)
        {
            int yCount = (int)self.Terrain.terrainData.size.z;
            return x * yCount + z;
        }

        /** 设置指定 x,y 位置立方体 状态
        *  1. 默认状态 ： None 灰色 
        *  2. 起点状态 ： Start 绿色
        *  3. 终点状态 ： End 红色
        *  4. 障碍状态 ： Obstacle 黑色
        *  5. 查询状态 ： Query 黄色
        *  6. 路径状态 ： Path 蓝色
        */
        public static void SetCubeState(this OperaComponent self, int x, int y, CubeState state)
        {
            // 计算立方体在字典中的索引
            int yCount = (int)self.Terrain.terrainData.size.z;
            int cubeIndex = x * yCount + y;

            if (self.PathCubeDic.TryGetValue(cubeIndex, out var cubeEntityRef))
            {
                PathCubeComponent pathCube = cubeEntityRef;
                pathCube?.ChangeState(state);
            }
        }

        // 获取指定位置立方体状态
        // private static CubeState GetCubeState(this OperaComponent self, int x, int y)
        // {
        //     int yCount = (int)self.Terrain.terrainData.size.z;
        //     int cubeIndex = x * yCount + y;

        //     if (self.PathCubeDic.TryGetValue(cubeIndex, out var cubeEntityRef))
        //     {
        //         PathCubeComponent pathCube = cubeEntityRef;
        //         return pathCube?.GetState() ?? CubeState.None;
        //     }

        //     return CubeState.None;
        // }

        /// <summary>
        /// 开始寻路
        /// </summary>
        public static void StartPathfinding(this OperaComponent self)
        {
            var findPathDataComponent = self.Scene().GetComponent<FindPathDataComponent>();
            var pathfindingComponent = self.Scene().GetComponent<PathfindingComponent>();

            if (findPathDataComponent.CubeStatesArr == null)
            {
                Log.Warning("请先初始化网格（按Q键）");
                return;
            }

            // 查找起点和终点
            GridPosition startPos = new GridPosition(-1, -1);
            GridPosition endPos = new GridPosition(-1, -1);

            int width = findPathDataComponent.CubeStatesArr.GetLength(0);
            int height = findPathDataComponent.CubeStatesArr.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    CubeState state = findPathDataComponent.CubeStatesArr[x, z];
                    if (state == CubeState.Start)
                    {
                        startPos = new GridPosition(x, z);
                    }
                    else if (state == CubeState.End)
                    {
                        endPos = new GridPosition(x, z);
                    }
                }
            }

            // 验证起点和终点
            if (startPos.x == -1 || endPos.x == -1)
            {
                Log.Warning("请先设置起点和终点");
                return;
            }

            Log.Info($"开始寻路：起点{startPos} -> 终点{endPos}");

            // 清除之前的路径显示
            self.ClearPathDisplay();

            // 执行寻路算法
            bool pathFound = pathfindingComponent.FindPath(findPathDataComponent.CubeStatesArr, startPos, endPos);

            if (pathFound)
            {
                // 显示路径
                self.DisplayPath(pathfindingComponent.GetPathResult());
            }
            else
            {
                Log.Warning("未找到有效路径");
            }
        }

        /// <summary>
        /// 重置寻路
        /// </summary>
        public static void ResetPathfinding(this OperaComponent self)
        {
            Log.Info("重置寻路");

            var findPathDataComponent = self.Scene().GetComponent<FindPathDataComponent>();
            var pathfindingComponent = self.Scene().GetComponent<PathfindingComponent>();

            if (findPathDataComponent.CubeStatesArr == null)
                return;

            // 清除路径显示
            self.ClearPathDisplay();

            // 清除寻路组件的路径结果
            pathfindingComponent.ClearPath();

            // 重置所有立方体状态为None（包括清空障碍物，但保留起点、终点）
            int width = findPathDataComponent.CubeStatesArr.GetLength(0);
            int height = findPathDataComponent.CubeStatesArr.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    CubeState currentState = findPathDataComponent.CubeStatesArr[x, z];
                    // 清空路径、查询状态和障碍物，保留起点和终点
                    if (currentState == CubeState.Path || currentState == CubeState.Query || currentState == CubeState.Obstacle)
                    {
                        self.SetCubeState(x, z, CubeState.None);
                        findPathDataComponent.CubeStatesArr[x, z] = CubeState.None;
                    }
                }
            }
        }

        /// <summary>
        /// 显示路径
        /// </summary>
        private static void DisplayPath(this OperaComponent self, List<GridPosition> pathResult)
        {
            var findPathDataComponent = self.Scene().GetComponent<FindPathDataComponent>();

            for (int i = 0; i < pathResult.Count; i++)
            {
                GridPosition pos = pathResult[i];
                CubeState currentState = findPathDataComponent.CubeStatesArr[pos.x, pos.z];

                // 不覆盖起点和终点的状态
                if (currentState != CubeState.Start && currentState != CubeState.End)
                {
                    self.SetCubeState(pos.x, pos.z, CubeState.Path);
                    findPathDataComponent.CubeStatesArr[pos.x, pos.z] = CubeState.Path;
                }
            }

            Log.Info($"路径显示完成，路径长度：{pathResult.Count}");
        }

        /// <summary>
        /// 清除路径显示
        /// </summary>
        private static void ClearPathDisplay(this OperaComponent self)
        {
            var findPathDataComponent = self.Scene().GetComponent<FindPathDataComponent>();

            if (findPathDataComponent.CubeStatesArr == null)
                return;

            int width = findPathDataComponent.CubeStatesArr.GetLength(0);
            int height = findPathDataComponent.CubeStatesArr.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    CubeState currentState = findPathDataComponent.CubeStatesArr[x, z];
                    if (currentState == CubeState.Path || currentState == CubeState.Query)
                    {
                        self.SetCubeState(x, z, CubeState.None);
                        findPathDataComponent.CubeStatesArr[x, z] = CubeState.None;
                    }
                }
            }
        }

        /// <summary>
        /// 开始动画寻路
        /// </summary>
        public static async ETTask StartAnimatedPathfinding(this OperaComponent self)
        {
            var findPathDataComponent = self.Scene().GetComponent<FindPathDataComponent>();
            var pathfindingComponent = self.Scene().GetComponent<PathfindingComponent>();

            if (findPathDataComponent.CubeStatesArr == null)
            {
                Log.Warning("请先初始化网格（按Q键）");
                return;
            }

            // 查找起点和终点
            GridPosition startPos = new GridPosition(-1, -1);
            GridPosition endPos = new GridPosition(-1, -1);

            int width = findPathDataComponent.CubeStatesArr.GetLength(0);
            int height = findPathDataComponent.CubeStatesArr.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    CubeState state = findPathDataComponent.CubeStatesArr[x, z];
                    if (state == CubeState.Start)
                    {
                        startPos = new GridPosition(x, z);
                    }
                    else if (state == CubeState.End)
                    {
                        endPos = new GridPosition(x, z);
                    }
                }
            }

            // 验证起点和终点
            if (startPos.x == -1 || endPos.x == -1)
            {
                Log.Warning("请先设置起点和终点");
                return;
            }

            Log.Info($"开始动画寻路：起点{startPos} -> 终点{endPos}，算法：{pathfindingComponent.CurrentAlgorithm}，动画速度：{pathfindingComponent.Config.AnimSpeed}ms");

            // 清除之前的路径显示
            self.ClearPathDisplay();

            // 执行动画寻路算法
            bool pathFound = await pathfindingComponent.AnimatedFindPath(findPathDataComponent.CubeStatesArr, startPos, endPos, self);

            if (!pathFound)
            {
                Log.Warning("动画寻路未找到有效路径");
            }
        }
    }
}