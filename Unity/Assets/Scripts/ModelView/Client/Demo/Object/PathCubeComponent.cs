using UnityEngine;

namespace ET.Client
{
    [ChildOf(typeof(OperaComponent))]
    public class PathCubeComponent : Entity, IAwake
    {
        public GameObject Cube;
        public CubeState State;
        public int Index;
    }
}