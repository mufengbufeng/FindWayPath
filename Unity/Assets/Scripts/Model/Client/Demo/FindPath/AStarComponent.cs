using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// A*算法组件
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class AStarComponent : Entity, IAwake
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

        /// <summary>
        /// 开放列表节点（用于动画显示）
        /// </summary>
        public List<GridPosition> OpenNodes = new List<GridPosition>();

        /// <summary>
        /// 关闭列表节点（用于动画显示）
        /// </summary>
        public List<GridPosition> ClosedNodes = new List<GridPosition>();
    }

    /// <summary>
    /// A*算法专用节点
    /// </summary>
    public struct AStarNode
    {
        public GridPosition Position;
        public GridPosition Parent;
        public float GCost;  // 从起点到当前点的实际代价
        public float HCost;  // 从当前点到终点的启发式代价
        public float FCost => GCost + HCost;  // 总代价

        public AStarNode(GridPosition position, GridPosition parent, float gCost, float hCost)
        {
            Position = position;
            Parent = parent;
            GCost = gCost;
            HCost = hCost;
        }
    }
}