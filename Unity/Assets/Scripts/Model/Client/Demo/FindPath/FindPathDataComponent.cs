using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ET.Client
{
    /// <summary>
    /// 寻路方块状态 
    /// </summary>
    public enum CubeState
    {

        None,   // 灰色
        Start,  // 绿色
        End,    // 红色
        Obstacle, // 黑色
        Query,  // 黄色
        Path    // 蓝色
    }

    [ComponentOf(typeof(Scene))]
    public class FindPathDataComponent : Entity, IAwake
    {
        public CubeState[,] CubeStatesArr;
        public int StartIndex;
        public int EndIndex;
    }
}