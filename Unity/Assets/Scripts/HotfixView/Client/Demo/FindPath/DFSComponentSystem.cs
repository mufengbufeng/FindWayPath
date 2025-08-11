using System.Collections.Generic;

namespace ET.Client
{
    [EntitySystemOf(typeof(DFSComponent))]
    [FriendOfAttribute(typeof(ET.Client.DFSComponent))]
    [FriendOfAttribute(typeof(ET.Client.FindPathDataComponent))]
    public static partial class DFSComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.DFSComponent self)
        {

        }

        /// <summary>
        /// 初始化网格大小
        /// </summary>
        public static void InitGrid(this DFSComponent self, int width, int height)
        {
            self.Width = width;
            self.Height = height;
        }

        /// <summary>
        /// 使用DFS算法寻找路径
        /// </summary>
        /// <param name="self"></param>
        /// <param name="cubeStatesArr">网格状态数组</param>
        /// <param name="startPos">起始位置</param>
        /// <param name="endPos">目标位置</param>
        /// <returns>是否找到路径</returns>
        public static bool FindPath(this DFSComponent self, CubeState[,] cubeStatesArr, GridPosition startPos, GridPosition endPos)
        {
            if (self.IsPathfinding)
            {
                Log.Warning("正在寻路中，请稍后再试");
                return false;
            }

            self.IsPathfinding = true;
            self.PathResult.Clear();

            // DFS寻路算法
            Stack<PathNode> openStack = new Stack<PathNode>();
            HashSet<GridPosition> visited = new HashSet<GridPosition>();
            Dictionary<GridPosition, PathNode> nodeMap = new Dictionary<GridPosition, PathNode>();

            // 四个方向：上下左右
            GridPosition[] directions = new GridPosition[]
            {
                new GridPosition(0, 1),   // 上
                new GridPosition(0, -1),  // 下
                new GridPosition(-1, 0),  // 左
                new GridPosition(1, 0)    // 右
            };

            // 初始化起始节点
            PathNode startNode = new PathNode(startPos, new GridPosition(-1, -1), 0);
            openStack.Push(startNode);
            visited.Add(startPos);
            nodeMap[startPos] = startNode;

            Log.Info($"开始DFS寻路：从 {startPos} 到 {endPos}");

            bool pathFound = false;

            // DFS主循环
            while (openStack.Count > 0)
            {
                PathNode currentNode = openStack.Pop();
                GridPosition currentPos = currentNode.Position;

                // 到达目标点
                if (currentPos == endPos)
                {
                    pathFound = true;
                    Log.Info("找到路径！");
                    break;
                }

                // 检查四个方向的邻居
                for (int i = 0; i < directions.Length; i++)
                {
                    GridPosition neighborPos = new GridPosition(
                        currentPos.x + directions[i].x,
                        currentPos.z + directions[i].z
                    );

                    // 检查边界
                    if (!self.IsValidPosition(neighborPos) || visited.Contains(neighborPos))
                        continue;

                    // 检查是否是障碍物
                    CubeState neighborState = cubeStatesArr[neighborPos.x, neighborPos.z];
                    if (neighborState == CubeState.Obstacle)
                        continue;

                    // 添加到堆栈
                    PathNode neighborNode = new PathNode(neighborPos, currentPos, currentNode.Distance + 1);
                    openStack.Push(neighborNode);
                    visited.Add(neighborPos);
                    nodeMap[neighborPos] = neighborNode;
                }
            }

            // 如果找到路径，构建路径结果
            if (pathFound)
            {
                self.BuildPath(nodeMap, startPos, endPos);
                Log.Info($"路径构建完成，路径长度：{self.PathResult.Count}");
            }
            else
            {
                Log.Warning("未找到有效路径");
            }

            self.IsPathfinding = false;
            return pathFound;
        }

        /// <summary>
        /// 检查位置是否有效
        /// </summary>
        private static bool IsValidPosition(this DFSComponent self, GridPosition pos)
        {
            return pos.x >= 0 && pos.x < self.Width && pos.z >= 0 && pos.z < self.Height;
        }

        /// <summary>
        /// 构建路径结果
        /// </summary>
        private static void BuildPath(this DFSComponent self, Dictionary<GridPosition, PathNode> nodeMap, GridPosition startPos, GridPosition endPos)
        {
            List<GridPosition> path = new List<GridPosition>();
            GridPosition currentPos = endPos;

            // 从终点往起点回溯
            while (!(currentPos == startPos))
            {
                path.Add(currentPos);
                if (nodeMap.TryGetValue(currentPos, out PathNode node))
                {
                    currentPos = node.Parent;
                }
                else
                {
                    Log.Error("路径构建失败：找不到父节点");
                    break;
                }
            }

            // 添加起始点
            path.Add(startPos);

            // 反转路径（从起点到终点）
            path.Reverse();

            self.PathResult = path;
        }

        /// <summary>
        /// 获取路径结果
        /// </summary>
        public static List<GridPosition> GetPathResult(this DFSComponent self)
        {
            return self.PathResult;
        }

        /// <summary>
        /// 清除路径结果
        /// </summary>
        public static void ClearPath(this DFSComponent self)
        {
            self.PathResult.Clear();
            self.IsPathfinding = false;
            self.VisitedNodes.Clear();

            // 取消动画协程
            if (self.AnimCancellationToken != null)
            {
                self.AnimCancellationToken.Cancel();
                self.AnimCancellationToken = null;
            }
            self.IsAnimPathfinding = false;
        }

        /// <summary>
        /// 动画寻路算法
        /// </summary>
        public static async ETTask<bool> AnimatedFindPath(this DFSComponent self, CubeState[,] cubeStatesArr, GridPosition startPos, GridPosition endPos, OperaComponent operaComponent)
        {
            if (self.IsAnimPathfinding || self.IsPathfinding)
            {
                Log.Warning("正在寻路中，请稍后再试");
                return false;
            }

            self.IsAnimPathfinding = true;
            self.PathResult.Clear();
            self.VisitedNodes.Clear();

            // 创建取消令牌
            self.AnimCancellationToken = new ETCancellationToken();

            try
            {
                // DFS寻路算法
                Stack<PathNode> openStack = new Stack<PathNode>();
                HashSet<GridPosition> visited = new HashSet<GridPosition>();
                Dictionary<GridPosition, PathNode> nodeMap = new Dictionary<GridPosition, PathNode>();

                // 四个方向：上下左右
                GridPosition[] directions = new GridPosition[]
                {
                    new GridPosition(0, 1),   // 上
                    new GridPosition(0, -1),  // 下
                    new GridPosition(-1, 0),  // 左
                    new GridPosition(1, 0)    // 右
                };

                // 初始化起始节点
                PathNode startNode = new PathNode(startPos, new GridPosition(-1, -1), 0);
                openStack.Push(startNode);
                visited.Add(startPos);
                nodeMap[startPos] = startNode;

                Log.Info($"开始动画DFS寻路：从 {startPos} 到 {endPos}");

                bool pathFound = false;

                // DFS主循环 - 动画版本
                while (openStack.Count > 0)
                {
                    // 检查是否取消
                    if (self.AnimCancellationToken.IsCancel())
                    {
                        Log.Info("动画寻路被取消");
                        return false;
                    }

                    PathNode currentNode = openStack.Pop();
                    GridPosition currentPos = currentNode.Position;

                    // 记录访问的节点（除了起点和终点）
                    if (!(currentPos == startPos) && !(currentPos == endPos))
                    {
                        self.VisitedNodes.Add(currentPos);

                        // 动画显示当前检查的节点
                        operaComponent.SetCubeState(currentPos.x, currentPos.z, CubeState.Query);
                        var findPathDataComponent = operaComponent.Scene().GetComponent<FindPathDataComponent>();
                        findPathDataComponent.CubeStatesArr[currentPos.x, currentPos.z] = CubeState.Query;

                        // 等待动画时间
                        await self.Root().GetComponent<TimerComponent>().WaitAsync(self.AnimSpeed, self.AnimCancellationToken);
                    }

                    // 到达目标点
                    if (currentPos == endPos)
                    {
                        pathFound = true;
                        Log.Info("动画寻路找到路径！");
                        break;
                    }

                    // 检查四个方向的邻居
                    for (int i = 0; i < directions.Length; i++)
                    {
                        GridPosition neighborPos = new GridPosition(
                            currentPos.x + directions[i].x,
                            currentPos.z + directions[i].z
                        );

                        // 检查边界
                        if (!self.IsValidPosition(neighborPos) || visited.Contains(neighborPos))
                            continue;

                        // 检查是否是障碍物
                        CubeState neighborState = cubeStatesArr[neighborPos.x, neighborPos.z];
                        if (neighborState == CubeState.Obstacle)
                            continue;

                        // 添加到堆栈
                        PathNode neighborNode = new PathNode(neighborPos, currentPos, currentNode.Distance + 1);
                        openStack.Push(neighborNode);
                        visited.Add(neighborPos);
                        nodeMap[neighborPos] = neighborNode;
                    }
                }

                // 如果找到路径，构建并显示路径结果
                if (pathFound)
                {
                    self.BuildPath(nodeMap, startPos, endPos);

                    // 动画显示最终路径
                    await self.AnimatePathDisplay(operaComponent);

                    Log.Info($"动画寻路完成，路径长度：{self.PathResult.Count}");
                }
                else
                {
                    Log.Warning("动画寻路未找到有效路径");
                }

                self.IsAnimPathfinding = false;
                return pathFound;
            }
            catch (System.Exception e)
            {
                Log.Error($"动画寻路出错：{e.Message}");
                self.IsAnimPathfinding = false;
                return false;
            }
        }

        /// <summary>
        /// 动画显示最终路径
        /// </summary>
        private static async ETTask AnimatePathDisplay(this DFSComponent self, OperaComponent operaComponent)
        {
            var findPathDataComponent = operaComponent.Scene().GetComponent<FindPathDataComponent>();

            for (int i = 0; i < self.PathResult.Count; i++)
            {
                // 检查是否取消
                if (self.AnimCancellationToken.IsCancel())
                    return;

                GridPosition pos = self.PathResult[i];
                CubeState currentState = findPathDataComponent.CubeStatesArr[pos.x, pos.z];

                // 不覆盖起点和终点的状态
                if (currentState != CubeState.Start && currentState != CubeState.End)
                {
                    operaComponent.SetCubeState(pos.x, pos.z, CubeState.Path);
                    findPathDataComponent.CubeStatesArr[pos.x, pos.z] = CubeState.Path;

                    // 路径显示动画稍快一些
                    await self.Root().GetComponent<TimerComponent>().WaitAsync(self.AnimSpeed , self.AnimCancellationToken);
                }
            }
        }

        /// <summary>
        /// 设置动画速度
        /// </summary>
        public static void SetAnimSpeed(this DFSComponent self, int speed)
        {
            self.AnimSpeed = speed;
        }

        /// <summary>
        /// 停止动画寻路
        /// </summary>
        public static void StopAnimPathfinding(this DFSComponent self)
        {
            if (self.AnimCancellationToken != null)
            {
                self.AnimCancellationToken.Cancel();
                self.AnimCancellationToken = null;
            }
            self.IsAnimPathfinding = false;
        }
    }
}