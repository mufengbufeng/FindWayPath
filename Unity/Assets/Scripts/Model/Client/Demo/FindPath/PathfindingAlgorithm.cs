namespace ET.Client
{
    /// <summary>
    /// 寻路算法类型枚举
    /// </summary>
    public enum PathfindingAlgorithmType
    {
        BFS,    // 广度优先搜索
        DFS,    // 深度优先搜索
        AStar,  // A*算法
        JPS     // Jump Point Search 跳点搜索
    }

    /// <summary>
    /// 寻路算法配置
    /// </summary>
    public struct PathfindingConfig
    {
        public PathfindingAlgorithmType AlgorithmType;
        public int AnimSpeed;
        public bool ShowSearchProcess;

        public PathfindingConfig(PathfindingAlgorithmType algorithmType, int animSpeed = 50, bool showSearchProcess = true)
        {
            AlgorithmType = algorithmType;
            AnimSpeed = animSpeed;
            ShowSearchProcess = showSearchProcess;
        }
    }
}