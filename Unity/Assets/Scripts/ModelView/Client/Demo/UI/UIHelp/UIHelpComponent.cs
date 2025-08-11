using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UIHelpComponent : Entity, IAwake, IDestroy
	{
		public Button RunBtn;
		public Button ResetBtn;
		public Button ObstacleBtn;
		public Button EndBtn;
		public Button StartBtn;
		public Button AnimRunBtn;

		public ScrollRect FindPathAlgorithmScroll;
		public GameObject FindPathAlgorithmContent;
		public Button ScrollBtn;
		
		/// <summary>
		/// 当前选择的寻路算法
		/// </summary>
		public PathfindingAlgorithmType CurrentAlgorithm = PathfindingAlgorithmType.BFS;
		
		/// <summary>
		/// 算法选择按钮列表
		/// </summary>
		public List<Button> AlgorithmButtons = new List<Button>();
	}
}
