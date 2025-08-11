using System;
using System.Collections.Generic;

namespace ET.Client
{
    [EntitySystemOf(typeof(AStarComponent))]
    [FriendOfAttribute(typeof(ET.Client.AStarComponent))]
    [FriendOfAttribute(typeof(ET.Client.FindPathDataComponent))]
    public static partial class AStarComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.AStarComponent self)
        {

        }

        /// <summary>
        /// 初始化网格大小
        /// </summary>
        public static void InitGrid(this AStarComponent self, int width, int height)
        {
            self.Width = width;
            self.Height = height;
        }

        /// <summary>
        /// 使用A*算法寻找路径
        /// </summary>
        public static bool FindPath(this AStarComponent self, CubeState[,] cubeStatesArr, GridPosition startPos, GridPosition endPos)
        {
            if (self.IsPathfinding)
            {
                Log.Warning("正在寻路中，请稍后再试");
                return false;
            }

            self.IsPathfinding = true;
            self.PathResult.Clear();

            // A*寻路算法
            List<AStarNode> openList = new List<AStarNode>();
            HashSet<GridPosition> closedSet = new HashSet<GridPosition>();
            Dictionary<GridPosition, AStarNode> nodeMap = new Dictionary<GridPosition, AStarNode>();

            // 四个方向：上下左右
            GridPosition[] directions = new GridPosition[]
            {
                new GridPosition(0, 1),   // 上
                new GridPosition(0, -1),  // 下
                new GridPosition(-1, 0),  // 左
                new GridPosition(1, 0)    // 右
            };

            // 初始化起始节点
            AStarNode startNode = new AStarNode(startPos, new GridPosition(-1, -1), 0, self.CalculateHeuristic(startPos, endPos));
            openList.Add(startNode);
            nodeMap[startPos] = startNode;

            Log.Info($"开始A*寻路：从 {startPos} 到 {endPos}");

            bool pathFound = false;

            // A*主循环
            while (openList.Count > 0)
            {
                // 找到F值最小的节点
                int currentIndex = 0;
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].FCost < openList[currentIndex].FCost ||
                        (openList[i].FCost == openList[currentIndex].FCost && openList[i].HCost < openList[currentIndex].HCost))
                    {
                        currentIndex = i;
                    }
                }

                AStarNode currentNode = openList[currentIndex];
                openList.RemoveAt(currentIndex);
                closedSet.Add(currentNode.Position);

                // 到达目标点
                if (currentNode.Position == endPos)
                {
                    pathFound = true;
                    Log.Info("A*找到路径！");
                    break;
                }

                // 检查四个方向的邻居
                for (int i = 0; i < directions.Length; i++)
                {
                    GridPosition neighborPos = new GridPosition(
                        currentNode.Position.x + directions[i].x,
                        currentNode.Position.z + directions[i].z
                    );

                    // 检查边界和关闭列表
                    if (!self.IsValidPosition(neighborPos) || closedSet.Contains(neighborPos))
                        continue;

                    // 检查是否是障碍物
                    CubeState neighborState = cubeStatesArr[neighborPos.x, neighborPos.z];
                    if (neighborState == CubeState.Obstacle)
                        continue;

                    float newGCost = currentNode.GCost + 1;
                    bool inOpenList = nodeMap.ContainsKey(neighborPos) && openList.Exists(n => n.Position == neighborPos);

                    if (!inOpenList || newGCost < nodeMap[neighborPos].GCost)
                    {
                        AStarNode neighborNode = new AStarNode(neighborPos, currentNode.Position, newGCost, self.CalculateHeuristic(neighborPos, endPos));
                        nodeMap[neighborPos] = neighborNode;

                        if (!inOpenList)
                        {
                            openList.Add(neighborNode);
                        }
                        else
                        {
                            // 更新已存在的节点
                            for (int j = 0; j < openList.Count; j++)
                            {
                                if (openList[j].Position == neighborPos)
                                {
                                    openList[j] = neighborNode;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // 如果找到路径，构建路径结果
            if (pathFound)
            {
                self.BuildPath(nodeMap, startPos, endPos);
                Log.Info($"A*路径构建完成，路径长度：{self.PathResult.Count}");
            }
            else
            {
                Log.Warning("A*未找到有效路径");
            }

            self.IsPathfinding = false;
            return pathFound;
        }

        /// <summary>
        /// 动画A*寻路算法
        /// </summary>
        public static async ETTask<bool> AnimatedFindPath(this AStarComponent self, CubeState[,] cubeStatesArr, GridPosition startPos, GridPosition endPos, OperaComponent operaComponent)
        {
            if (self.IsAnimPathfinding || self.IsPathfinding)
            {
                Log.Warning("正在寻路中，请稍后再试");
                return false;
            }

            self.IsAnimPathfinding = true;
            self.PathResult.Clear();
            self.VisitedNodes.Clear();
            self.OpenNodes.Clear();
            self.ClosedNodes.Clear();

            // 创建取消令牌
            self.AnimCancellationToken = new ETCancellationToken();

            try
            {
                // A*寻路算法
                List<AStarNode> openList = new List<AStarNode>();
                HashSet<GridPosition> closedSet = new HashSet<GridPosition>();
                Dictionary<GridPosition, AStarNode> nodeMap = new Dictionary<GridPosition, AStarNode>();

                // 四个方向：上下左右
                GridPosition[] directions = new GridPosition[]
                {
                    new GridPosition(0, 1),   // 上
                    new GridPosition(0, -1),  // 下
                    new GridPosition(-1, 0),  // 左
                    new GridPosition(1, 0)    // 右
                };

                // 初始化起始节点
                AStarNode startNode = new AStarNode(startPos, new GridPosition(-1, -1), 0, self.CalculateHeuristic(startPos, endPos));
                openList.Add(startNode);
                nodeMap[startPos] = startNode;

                Log.Info($"开始动画A*寻路：从 {startPos} 到 {endPos}");

                bool pathFound = false;

                // A*主循环 - 动画版本
                while (openList.Count > 0)
                {
                    // 检查是否取消
                    if (self.AnimCancellationToken.IsCancel())
                    {
                        Log.Info("A*动画寻路被取消");
                        return false;
                    }

                    // // 找到F值最小的节点 - 动画显示查询过程
                    int currentIndex = 0;
                    for (int i = 1; i < openList.Count; i++)
                    {
                        // 动画显示当前正在比较的节点
                        GridPosition comparePos = openList[i].Position;
                        if (!(comparePos == startPos) && !(comparePos == endPos))
                        {
                            // 短暂显示正在比较的节点
                            operaComponent.SetCubeState(comparePos.x, comparePos.z, CubeState.Query);
                            var findPathDataComponent = operaComponent.Scene().GetComponent<FindPathDataComponent>();
                            findPathDataComponent.CubeStatesArr[comparePos.x, comparePos.z] = CubeState.Query;

                        }
                        // await self.Root().GetComponent<TimerComponent>().WaitAsync(self.AnimSpeed, self.AnimCancellationToken);

                        if (openList[i].FCost < openList[currentIndex].FCost ||
                            (openList[i].FCost == openList[currentIndex].FCost && openList[i].HCost < openList[currentIndex].HCost))
                        {
                            currentIndex = i;
                        }
                    }

                    AStarNode currentNode = openList[currentIndex];
                    openList.RemoveAt(currentIndex);
                    closedSet.Add(currentNode.Position);

                    // 记录访问的节点（除了起点和终点）
                    if (!(currentNode.Position == startPos) && !(currentNode.Position == endPos))
                    {
                        self.ClosedNodes.Add(currentNode.Position);

                        // 动画显示当前处理的节点（从开放列表移到关闭列表）
                        operaComponent.SetCubeState(currentNode.Position.x, currentNode.Position.z, CubeState.Query);
                        var findPathDataComponent = operaComponent.Scene().GetComponent<FindPathDataComponent>();
                        findPathDataComponent.CubeStatesArr[currentNode.Position.x, currentNode.Position.z] = CubeState.Query;

                        // 等待动画时间
                        await self.Root().GetComponent<TimerComponent>().WaitAsync(self.AnimSpeed, self.AnimCancellationToken);
                    }

                    // 到达目标点
                    if (currentNode.Position == endPos)
                    {
                        pathFound = true;
                        Log.Info("A*动画寻路找到路径！");
                        break;
                    }

                    // 检查四个方向的邻居
                    for (int i = 0; i < directions.Length; i++)
                    {
                        GridPosition neighborPos = new GridPosition(
                            currentNode.Position.x + directions[i].x,
                            currentNode.Position.z + directions[i].z
                        );

                        // 检查边界和关闭列表
                        if (!self.IsValidPosition(neighborPos) || closedSet.Contains(neighborPos))
                            continue;

                        // 检查是否是障碍物
                        CubeState neighborState = cubeStatesArr[neighborPos.x, neighborPos.z];
                        if (neighborState == CubeState.Obstacle)
                            continue;

                        float newGCost = currentNode.GCost + 1;
                        bool inOpenList = nodeMap.ContainsKey(neighborPos) && openList.Exists(n => n.Position == neighborPos);

                        if (!inOpenList || newGCost < nodeMap[neighborPos].GCost)
                        {
                            AStarNode neighborNode = new AStarNode(neighborPos, currentNode.Position, newGCost, self.CalculateHeuristic(neighborPos, endPos));
                            nodeMap[neighborPos] = neighborNode;

                            if (!inOpenList)
                            {
                                openList.Add(neighborNode);

                                // 记录开放列表节点（用于统计，不显示动画）
                                if (!(neighborPos == startPos) && !(neighborPos == endPos))
                                {
                                    self.OpenNodes.Add(neighborPos);
                                }
                            }
                            else
                            {
                                // 更新已存在的节点
                                for (int j = 0; j < openList.Count; j++)
                                {
                                    if (openList[j].Position == neighborPos)
                                    {
                                        openList[j] = neighborNode;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                // 如果找到路径，构建并显示路径结果
                if (pathFound)
                {
                    self.BuildPath(nodeMap, startPos, endPos);

                    // 动画显示最终路径
                    await self.AnimatePathDisplay(operaComponent);

                    Log.Info($"A*动画寻路完成，路径长度：{self.PathResult.Count}");
                }
                else
                {
                    Log.Warning("A*动画寻路未找到有效路径");
                }

                self.IsAnimPathfinding = false;
                return pathFound;
            }
            catch (System.Exception e)
            {
                Log.Error($"A*动画寻路出错：{e.Message}");
                self.IsAnimPathfinding = false;
                return false;
            }
        }

        /// <summary>
        /// 计算启发式距离（曼哈顿距离）
        /// </summary>
        private static float CalculateHeuristic(this AStarComponent self, GridPosition from, GridPosition to)
        {
            return Math.Abs(from.x - to.x) + Math.Abs(from.z - to.z);
        }

        /// <summary>
        /// 检查位置是否有效
        /// </summary>
        private static bool IsValidPosition(this AStarComponent self, GridPosition pos)
        {
            return pos.x >= 0 && pos.x < self.Width && pos.z >= 0 && pos.z < self.Height;
        }

        /// <summary>
        /// 构建A*路径结果
        /// </summary>
        private static void BuildPath(this AStarComponent self, Dictionary<GridPosition, AStarNode> nodeMap, GridPosition startPos, GridPosition endPos)
        {
            List<GridPosition> path = new List<GridPosition>();
            GridPosition currentPos = endPos;

            // 从终点往起点回溯
            while (!(currentPos == startPos))
            {
                path.Add(currentPos);
                if (nodeMap.TryGetValue(currentPos, out AStarNode node))
                {
                    currentPos = node.Parent;
                }
                else
                {
                    Log.Error("A*路径构建失败：找不到父节点");
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
        /// 动画显示最终路径
        /// </summary>
        private static async ETTask AnimatePathDisplay(this AStarComponent self, OperaComponent operaComponent)
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

                    await self.Root().GetComponent<TimerComponent>().WaitAsync(self.AnimSpeed, self.AnimCancellationToken);
                }
            }
        }

        /// <summary>
        /// 获取路径结果
        /// </summary>
        public static List<GridPosition> GetPathResult(this AStarComponent self)
        {
            return self.PathResult;
        }

        /// <summary>
        /// 清除路径结果
        /// </summary>
        public static void ClearPath(this AStarComponent self)
        {
            self.PathResult.Clear();
            self.IsPathfinding = false;
            self.VisitedNodes.Clear();
            self.OpenNodes.Clear();
            self.ClosedNodes.Clear();

            // 取消动画协程
            if (self.AnimCancellationToken != null)
            {
                self.AnimCancellationToken.Cancel();
                self.AnimCancellationToken = null;
            }
            self.IsAnimPathfinding = false;
        }

        /// <summary>
        /// 设置动画速度
        /// </summary>
        public static void SetAnimSpeed(this AStarComponent self, int speed)
        {
            self.AnimSpeed = speed;
        }

        /// <summary>
        /// 停止动画寻路
        /// </summary>
        public static void StopAnimPathfinding(this AStarComponent self)
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