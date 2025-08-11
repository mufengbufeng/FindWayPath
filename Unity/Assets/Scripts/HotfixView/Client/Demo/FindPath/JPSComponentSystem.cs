using System;
using System.Collections.Generic;

namespace ET.Client
{
    [EntitySystemOf(typeof(JPSComponent))]
    [FriendOfAttribute(typeof(ET.Client.JPSComponent))]
    [FriendOfAttribute(typeof(ET.Client.FindPathDataComponent))]
    public static partial class JPSComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.JPSComponent self)
        {

        }

        /// <summary>
        /// 初始化网格大小
        /// </summary>
        public static void InitGrid(this JPSComponent self, int width, int height)
        {
            self.Width = width;
            self.Height = height;
        }

        /// <summary>
        /// 使用JPS算法寻找路径（简化版本，基于A*）
        /// </summary>
        public static bool FindPath(this JPSComponent self, CubeState[,] cubeStatesArr, GridPosition startPos, GridPosition endPos)
        {
            if (self.IsPathfinding)
            {
                Log.Warning("正在寻路中，请稍后再试");
                return false;
            }

            Log.Info("JPS算法：使用简化版本（基于A*）");
            
            self.IsPathfinding = true;
            self.PathResult.Clear();

            // JPS简化版本：使用A*算法逻辑，但会识别关键跳跃点
            List<JPSNode> openList = new List<JPSNode>();
            HashSet<GridPosition> closedSet = new HashSet<GridPosition>();
            Dictionary<GridPosition, JPSNode> nodeMap = new Dictionary<GridPosition, JPSNode>();

            // 八个方向：上下左右 + 对角线
            GridPosition[] directions = new GridPosition[]
            {
                new GridPosition(0, 1),   // 上
                new GridPosition(0, -1),  // 下
                new GridPosition(-1, 0),  // 左
                new GridPosition(1, 0),   // 右
                new GridPosition(-1, 1),  // 左上
                new GridPosition(1, 1),   // 右上
                new GridPosition(-1, -1), // 左下
                new GridPosition(1, -1)   // 右下
            };

            // 初始化起始节点
            JPSNode startNode = new JPSNode(startPos, new GridPosition(-1, -1), new GridPosition(0, 0), 0, self.CalculateHeuristic(startPos, endPos));
            openList.Add(startNode);
            nodeMap[startPos] = startNode;

            Log.Info($"开始JPS寻路：从 {startPos} 到 {endPos}");

            bool pathFound = false;

            // JPS主循环（简化版本）
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

                JPSNode currentNode = openList[currentIndex];
                openList.RemoveAt(currentIndex);
                closedSet.Add(currentNode.Position);

                // 到达目标点
                if (currentNode.Position == endPos)
                {
                    pathFound = true;
                    Log.Info("JPS找到路径！");
                    break;
                }

                // 检查方向（简化版本：检查所有方向）
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

                    // 计算移动代价（对角线移动代价更高）
                    float moveCost = (Math.Abs(directions[i].x) + Math.Abs(directions[i].z) == 2) ? 1.414f : 1.0f;
                    float newGCost = currentNode.GCost + moveCost;
                    bool inOpenList = nodeMap.ContainsKey(neighborPos) && openList.Exists(n => n.Position == neighborPos);

                    if (!inOpenList || newGCost < nodeMap[neighborPos].GCost)
                    {
                        JPSNode neighborNode = new JPSNode(neighborPos, currentNode.Position, directions[i], newGCost, self.CalculateHeuristic(neighborPos, endPos));
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
                Log.Info($"JPS路径构建完成，路径长度：{self.PathResult.Count}");
            }
            else
            {
                Log.Warning("JPS未找到有效路径");
            }

            self.IsPathfinding = false;
            return pathFound;
        }

        /// <summary>
        /// 动画JPS寻路算法
        /// </summary>
        public static async ETTask<bool> AnimatedFindPath(this JPSComponent self, CubeState[,] cubeStatesArr, GridPosition startPos, GridPosition endPos, OperaComponent operaComponent)
        {
            Log.Info("JPS动画寻路：使用简化版本（基于A*）");
            
            if (self.IsAnimPathfinding || self.IsPathfinding)
            {
                Log.Warning("正在寻路中，请稍后再试");
                return false;
            }

            self.IsAnimPathfinding = true;
            self.PathResult.Clear();
            self.VisitedNodes.Clear();
            self.JumpPoints.Clear();

            // 创建取消令牌
            self.AnimCancellationToken = new ETCancellationToken();

            try
            {
                // 使用A*逻辑的简化JPS
                List<JPSNode> openList = new List<JPSNode>();
                HashSet<GridPosition> closedSet = new HashSet<GridPosition>();
                Dictionary<GridPosition, JPSNode> nodeMap = new Dictionary<GridPosition, JPSNode>();

                // 八个方向
                GridPosition[] directions = new GridPosition[]
                {
                    new GridPosition(0, 1),   // 上
                    new GridPosition(0, -1),  // 下
                    new GridPosition(-1, 0),  // 左
                    new GridPosition(1, 0),   // 右
                    new GridPosition(-1, 1),  // 左上
                    new GridPosition(1, 1),   // 右上
                    new GridPosition(-1, -1), // 左下
                    new GridPosition(1, -1)   // 右下
                };

                // 初始化起始节点
                JPSNode startNode = new JPSNode(startPos, new GridPosition(-1, -1), new GridPosition(0, 0), 0, self.CalculateHeuristic(startPos, endPos));
                openList.Add(startNode);
                nodeMap[startPos] = startNode;

                Log.Info($"开始JPS动画寻路：从 {startPos} 到 {endPos}");

                bool pathFound = false;

                // JPS主循环 - 动画版本
                while (openList.Count > 0)
                {
                    // 检查是否取消
                    if (self.AnimCancellationToken.IsCancel())
                    {
                        Log.Info("JPS动画寻路被取消");
                        return false;
                    }

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

                    JPSNode currentNode = openList[currentIndex];
                    openList.RemoveAt(currentIndex);
                    closedSet.Add(currentNode.Position);

                    // 记录跳跃点（除了起点和终点）
                    if (!(currentNode.Position == startPos) && !(currentNode.Position == endPos))
                    {
                        self.JumpPoints.Add(currentNode.Position);

                        // 动画显示当前跳跃点
                        operaComponent.SetCubeState(currentNode.Position.x, currentNode.Position.z, CubeState.Query);
                        var findPathDataComponent = operaComponent.Scene().GetComponent<FindPathDataComponent>();
                        findPathDataComponent.CubeStatesArr[currentNode.Position.x, currentNode.Position.z] = CubeState.Query;

                        // 等待动画时间（JPS应该比A*快一些）
                        await self.Root().GetComponent<TimerComponent>().WaitAsync(self.AnimSpeed , self.AnimCancellationToken);
                    }

                    // 到达目标点
                    if (currentNode.Position == endPos)
                    {
                        pathFound = true;
                        Log.Info("JPS动画寻路找到路径！");
                        break;
                    }

                    // 检查方向
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

                        // 计算移动代价
                        float moveCost = (Math.Abs(directions[i].x) + Math.Abs(directions[i].z) == 2) ? 1.414f : 1.0f;
                        float newGCost = currentNode.GCost + moveCost;
                        bool inOpenList = nodeMap.ContainsKey(neighborPos) && openList.Exists(n => n.Position == neighborPos);

                        if (!inOpenList || newGCost < nodeMap[neighborPos].GCost)
                        {
                            JPSNode neighborNode = new JPSNode(neighborPos, currentNode.Position, directions[i], newGCost, self.CalculateHeuristic(neighborPos, endPos));
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

                // 如果找到路径，构建并显示路径结果
                if (pathFound)
                {
                    self.BuildPath(nodeMap, startPos, endPos);

                    // 动画显示最终路径
                    await self.AnimatePathDisplay(operaComponent);

                    Log.Info($"JPS动画寻路完成，路径长度：{self.PathResult.Count}");
                }
                else
                {
                    Log.Warning("JPS动画寻路未找到有效路径");
                }

                self.IsAnimPathfinding = false;
                return pathFound;
            }
            catch (System.Exception e)
            {
                Log.Error($"JPS动画寻路出错：{e.Message}");
                self.IsAnimPathfinding = false;
                return false;
            }
        }

        /// <summary>
        /// 计算启发式距离（欧几里得距离）
        /// </summary>
        private static float CalculateHeuristic(this JPSComponent self, GridPosition from, GridPosition to)
        {
            float dx = from.x - to.x;
            float dz = from.z - to.z;
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }

        /// <summary>
        /// 检查位置是否有效
        /// </summary>
        private static bool IsValidPosition(this JPSComponent self, GridPosition pos)
        {
            return pos.x >= 0 && pos.x < self.Width && pos.z >= 0 && pos.z < self.Height;
        }

        /// <summary>
        /// 构建JPS路径结果
        /// </summary>
        private static void BuildPath(this JPSComponent self, Dictionary<GridPosition, JPSNode> nodeMap, GridPosition startPos, GridPosition endPos)
        {
            List<GridPosition> path = new List<GridPosition>();
            GridPosition currentPos = endPos;

            // 从终点往起点回溯
            while (!(currentPos == startPos))
            {
                path.Add(currentPos);
                if (nodeMap.TryGetValue(currentPos, out JPSNode node))
                {
                    currentPos = node.Parent;
                }
                else
                {
                    Log.Error("JPS路径构建失败：找不到父节点");
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
        private static async ETTask AnimatePathDisplay(this JPSComponent self, OperaComponent operaComponent)
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
        /// 获取路径结果
        /// </summary>
        public static List<GridPosition> GetPathResult(this JPSComponent self)
        {
            return self.PathResult;
        }

        /// <summary>
        /// 清除路径结果
        /// </summary>
        public static void ClearPath(this JPSComponent self)
        {
            self.PathResult.Clear();
            self.IsPathfinding = false;
            self.VisitedNodes.Clear();
            self.JumpPoints.Clear();

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
        public static void SetAnimSpeed(this JPSComponent self, int speed)
        {
            self.AnimSpeed = speed;
        }

        /// <summary>
        /// 停止动画寻路
        /// </summary>
        public static void StopAnimPathfinding(this JPSComponent self)
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