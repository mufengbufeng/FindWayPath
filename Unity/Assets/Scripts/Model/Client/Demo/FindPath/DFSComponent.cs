using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// DFS（深度优先搜索）算法组件
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class DFSComponent : Entity, IAwake
    {
        /// <summary>
        /// 网格宽度
        /// </summary>
        public int Width;

        /// <summary>
        /// 网格高度
        /// </summary>
        public int Height;

        /// <summary>
        /// 存储路径结果
        /// </summary>
        public List<GridPosition> PathResult = new List<GridPosition>();

        /// <summary>
        /// 是否正在寻路
        /// </summary>
        public bool IsPathfinding = false;

        /// <summary>
        /// 动画寻路相关字段
        /// </summary>
        public bool IsAnimPathfinding = false;

        /// <summary>
        /// 动画播放速度（毫秒）
        /// </summary>
        public int AnimSpeed = 50;

        /// <summary>
        /// 动画播放的取消令牌
        /// </summary>
        public ETCancellationToken AnimCancellationToken;

        /// <summary>
        /// 存储动画过程中访问的节点顺序
        /// </summary>
        public List<GridPosition> VisitedNodes = new List<GridPosition>();
    }
}