using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 简单的二维坐标结构体
    /// </summary>
    public struct GridPosition
    {
        public int x;
        public int z;

        public GridPosition(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public static bool operator ==(GridPosition a, GridPosition b)
        {
            return a.x == b.x && a.z == b.z;
        }

        public static bool operator !=(GridPosition a, GridPosition b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is GridPosition pos)
                return this == pos;
            return false;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ z.GetHashCode();
        }

        public override string ToString()
        {
            return $"({x}, {z})";
        }
    }

    /// <summary>
    /// 网格节点，用于寻路
    /// </summary>
    public struct PathNode
    {
        public GridPosition Position;
        public GridPosition Parent;
        public int Distance;

        public PathNode(GridPosition position, GridPosition parent, int distance)
        {
            Position = position;
            Parent = parent;
            Distance = distance;
        }
    }

    /// <summary>
    /// BFS（广度优先搜索）算法组件
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class BFSComponent : Entity, IAwake
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