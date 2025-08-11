using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// JPS（Jump Point Search）算法组件
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class JPSComponent : Entity, IAwake
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
        /// 跳跃点列表（用于动画显示）
        /// </summary>
        public List<GridPosition> JumpPoints = new List<GridPosition>();
    }

    /// <summary>
    /// JPS算法专用节点
    /// </summary>
    public struct JPSNode
    {
        public GridPosition Position;
        public GridPosition Parent;
        public GridPosition Direction;  // 移动方向
        public float GCost;
        public float HCost;
        public float FCost => GCost + HCost;

        public JPSNode(GridPosition position, GridPosition parent, GridPosition direction, float gCost, float hCost)
        {
            Position = position;
            Parent = parent;
            Direction = direction;
            GCost = gCost;
            HCost = hCost;
        }
    }
}