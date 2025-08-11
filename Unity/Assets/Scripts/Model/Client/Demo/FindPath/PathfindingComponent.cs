using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 寻路算法管理组件，统一管理多种寻路算法
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class PathfindingComponent : Entity, IAwake
    {
        /// <summary>
        /// 当前选择的寻路算法
        /// </summary>
        public PathfindingAlgorithmType CurrentAlgorithm = PathfindingAlgorithmType.BFS;
        
        /// <summary>
        /// 寻路配置
        /// </summary>
        public PathfindingConfig Config = new PathfindingConfig(PathfindingAlgorithmType.BFS, 200, true);
    }

}