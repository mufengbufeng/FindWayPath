using System;
using System.Collections.Generic;

namespace ET.Client
{
    [EntitySystemOf(typeof(PathfindingComponent))]
    [FriendOfAttribute(typeof(ET.Client.PathfindingComponent))]
    [FriendOfAttribute(typeof(ET.Client.FindPathDataComponent))]
    [FriendOfAttribute(typeof(ET.Client.BFSComponent))]
    [FriendOfAttribute(typeof(ET.Client.DFSComponent))]
    [FriendOfAttribute(typeof(ET.Client.AStarComponent))]
    [FriendOfAttribute(typeof(ET.Client.JPSComponent))]
    public static partial class PathfindingComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.PathfindingComponent self)
        {

        }

        /// <summary>
        /// 初始化网格大小
        /// </summary>
        public static void InitGrid(this PathfindingComponent self, int width, int height)
        {
            // 初始化所有算法组件的网格大小
            var scene = self.Scene();
            
            var bfsComponent = scene.GetComponent<BFSComponent>();
            bfsComponent?.InitGrid(width, height);
            
            var dfsComponent = scene.GetComponent<DFSComponent>();
            dfsComponent?.InitGrid(width, height);
            
            var astarComponent = scene.GetComponent<AStarComponent>();
            astarComponent?.InitGrid(width, height);
            
            var jpsComponent = scene.GetComponent<JPSComponent>();
            jpsComponent?.InitGrid(width, height);
        }

        /// <summary>
        /// 设置寻路算法
        /// </summary>
        public static void SetAlgorithm(this PathfindingComponent self, PathfindingAlgorithmType algorithm)
        {
            self.CurrentAlgorithm = algorithm;
            self.Config.AlgorithmType = algorithm;
        }

        /// <summary>
        /// 设置动画速度
        /// </summary>
        public static void SetAnimSpeed(this PathfindingComponent self, int speed)
        {
            self.Config.AnimSpeed = speed;
            
            // 同步更新所有算法组件的动画速度
            var scene = self.Scene();
            
            var bfsComponent = scene.GetComponent<BFSComponent>();
            bfsComponent?.SetAnimSpeed(speed);
            
            var dfsComponent = scene.GetComponent<DFSComponent>();
            dfsComponent?.SetAnimSpeed(speed);
            
            var astarComponent = scene.GetComponent<AStarComponent>();
            astarComponent?.SetAnimSpeed(speed);
            
            var jpsComponent = scene.GetComponent<JPSComponent>();
            jpsComponent?.SetAnimSpeed(speed);
        }

        /// <summary>
        /// 使用选定算法进行寻路
        /// </summary>
        public static bool FindPath(this PathfindingComponent self, CubeState[,] cubeStatesArr, GridPosition startPos, GridPosition endPos)
        {
            var scene = self.Scene();
            
            switch (self.CurrentAlgorithm)
            {
                case PathfindingAlgorithmType.BFS:
                    var bfsComponent = scene.GetComponent<BFSComponent>();
                    return bfsComponent?.FindPath(cubeStatesArr, startPos, endPos) ?? false;
                    
                case PathfindingAlgorithmType.DFS:
                    var dfsComponent = scene.GetComponent<DFSComponent>();
                    return dfsComponent?.FindPath(cubeStatesArr, startPos, endPos) ?? false;
                    
                case PathfindingAlgorithmType.AStar:
                    var astarComponent = scene.GetComponent<AStarComponent>();
                    return astarComponent?.FindPath(cubeStatesArr, startPos, endPos) ?? false;
                    
                case PathfindingAlgorithmType.JPS:
                    var jpsComponent = scene.GetComponent<JPSComponent>();
                    return jpsComponent?.FindPath(cubeStatesArr, startPos, endPos) ?? false;
                    
                default:
                    var defaultBfsComponent = scene.GetComponent<BFSComponent>();
                    return defaultBfsComponent?.FindPath(cubeStatesArr, startPos, endPos) ?? false;
            }
        }

        /// <summary>
        /// 动画寻路
        /// </summary>
        public static async ETTask<bool> AnimatedFindPath(this PathfindingComponent self, CubeState[,] cubeStatesArr, GridPosition startPos, GridPosition endPos, OperaComponent operaComponent)
        {
            var scene = self.Scene();
            
            switch (self.CurrentAlgorithm)
            {
                case PathfindingAlgorithmType.BFS:
                    var bfsComponent = scene.GetComponent<BFSComponent>();
                    return bfsComponent != null ? await bfsComponent.AnimatedFindPath(cubeStatesArr, startPos, endPos, operaComponent) : false;
                    
                case PathfindingAlgorithmType.DFS:
                    var dfsComponent = scene.GetComponent<DFSComponent>();
                    return dfsComponent != null ? await dfsComponent.AnimatedFindPath(cubeStatesArr, startPos, endPos, operaComponent) : false;
                    
                case PathfindingAlgorithmType.AStar:
                    var astarComponent = scene.GetComponent<AStarComponent>();
                    return astarComponent != null ? await astarComponent.AnimatedFindPath(cubeStatesArr, startPos, endPos, operaComponent) : false;
                    
                case PathfindingAlgorithmType.JPS:
                    var jpsComponent = scene.GetComponent<JPSComponent>();
                    return jpsComponent != null ? await jpsComponent.AnimatedFindPath(cubeStatesArr, startPos, endPos, operaComponent) : false;
                    
                default:
                    var defaultBfsComponent = scene.GetComponent<BFSComponent>();
                    return defaultBfsComponent != null ? await defaultBfsComponent.AnimatedFindPath(cubeStatesArr, startPos, endPos, operaComponent) : false;
            }
        }

        /// <summary>
        /// 获取当前算法的路径结果
        /// </summary>
        public static List<GridPosition> GetPathResult(this PathfindingComponent self)
        {
            var scene = self.Scene();
            
            switch (self.CurrentAlgorithm)
            {
                case PathfindingAlgorithmType.BFS:
                    return scene.GetComponent<BFSComponent>()?.GetPathResult() ?? new List<GridPosition>();
                    
                case PathfindingAlgorithmType.DFS:
                    return scene.GetComponent<DFSComponent>()?.GetPathResult() ?? new List<GridPosition>();
                    
                case PathfindingAlgorithmType.AStar:
                    return scene.GetComponent<AStarComponent>()?.GetPathResult() ?? new List<GridPosition>();
                    
                case PathfindingAlgorithmType.JPS:
                    return scene.GetComponent<JPSComponent>()?.GetPathResult() ?? new List<GridPosition>();
                    
                default:
                    return scene.GetComponent<BFSComponent>()?.GetPathResult() ?? new List<GridPosition>();
            }
        }
        
        /// <summary>
        /// 清除所有算法组件的路径结果
        /// </summary>
        public static void ClearPath(this PathfindingComponent self)
        {
            var scene = self.Scene();
            var findPathDataComponent = scene.GetComponent<FindPathDataComponent>();
            
            Log.Info($"[PathfindingComponent.ClearPath开始] StartIndex={findPathDataComponent?.StartIndex}, EndIndex={findPathDataComponent?.EndIndex}");
            
            scene.GetComponent<BFSComponent>()?.ClearPath();
            scene.GetComponent<DFSComponent>()?.ClearPath();
            scene.GetComponent<AStarComponent>()?.ClearPath();
            scene.GetComponent<JPSComponent>()?.ClearPath();
            
            Log.Info($"[PathfindingComponent.ClearPath完成] StartIndex={findPathDataComponent?.StartIndex}, EndIndex={findPathDataComponent?.EndIndex}");
        }
        
        /// <summary>
        /// 停止所有动画寻路
        /// </summary>
        public static void StopAnimPathfinding(this PathfindingComponent self)
        {
            var scene = self.Scene();
            
            scene.GetComponent<BFSComponent>()?.StopAnimPathfinding();
            scene.GetComponent<DFSComponent>()?.StopAnimPathfinding();
            scene.GetComponent<AStarComponent>()?.StopAnimPathfinding();
            scene.GetComponent<JPSComponent>()?.StopAnimPathfinding();
        }
    }
}