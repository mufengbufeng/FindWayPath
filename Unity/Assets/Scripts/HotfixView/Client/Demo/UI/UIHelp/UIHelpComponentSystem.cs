using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIHelpComponent))]
    [FriendOfAttribute(typeof(ET.Client.UIHelpComponent))]
    [FriendOfAttribute(typeof(ET.Client.FindPathDataComponent))]
    public static partial class UIHelpComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.UIHelpComponent self)
        {
            var ui = self.GetParent<UI>();
            var rc = ui.GameObject.GetComponent<ReferenceCollector>();

            self.RunBtn = rc.Get<Button>("RunBtn");
            self.ResetBtn = rc.Get<Button>("ResetBtn");
            self.ObstacleBtn = rc.Get<Button>("ObstacleBtn");
            self.EndBtn = rc.Get<Button>("EndBtn");
            self.StartBtn = rc.Get<Button>("StartBtn");
            self.AnimRunBtn = rc.Get<Button>("AnimRunBtn");

            // ScrollRect相关组件可能不存在，使用安全获取
            try
            {
                self.FindPathAlgorithmScroll = rc.Get<ScrollRect>("FindPathAlgorithmScroll");
            }
            catch
            {
                Log.Warning("FindPathAlgorithmScroll未找到，跳过ScrollRect初始化");
                self.FindPathAlgorithmScroll = null;
            }

            try
            {
                self.FindPathAlgorithmContent = rc.Get<GameObject>("FindPathAlgorithmContent");
            }
            catch
            {
                Log.Warning("FindPathAlgorithmContent未找到，跳过算法按钮初始化");
                self.FindPathAlgorithmContent = null;
            }

            // 获取ScrollBtn作为按钮模板
            try
            {
                self.ScrollBtn = rc.Get<Button>("ScrollBtn");
            }
            catch
            {
                Log.Warning("ScrollBtn未找到，将使用完全动态创建方式");
                self.ScrollBtn = null;
            }

            // 绑定按钮事件
            self.RunBtn.onClick.AddListener(self.RunBtnClick);
            self.ResetBtn.onClick.AddListener(self.ResetBtnClick);
            self.ObstacleBtn.onClick.AddListener(self.ObstacleBtnClick);
            self.EndBtn.onClick.AddListener(self.EndBtnClick);
            self.StartBtn.onClick.AddListener(self.StartBtnClick);
            self.AnimRunBtn.onClick.AddListener(self.AnimRunBtnClick);

            // 初始化算法选择按钮
            self.InitAlgorithmButtons();

            // 隐藏模板按钮（ScrollBtn仅用作克隆模板）
            if (self.ScrollBtn != null)
            {
                self.ScrollBtn.gameObject.SetActive(false);
            }
        }

        private static void RunBtnClick(this UIHelpComponent self)
        {
            // 处理 Run 按钮点击事件
            Log.Info("Run button clicked");

            EventSystem.Instance.Publish(self.Root(), new ChangeFindPathStateEvent() { state = EControlState.Query });
        }

        private static void ResetBtnClick(this UIHelpComponent self)
        {
            // 处理 Reset 按钮点击事件
            Log.Info("Reset button clicked");
            EventSystem.Instance.Publish(self.Root(), new PathResetEvent());
        }

        private static void ObstacleBtnClick(this UIHelpComponent self)
        {
            // 处理 Obstacle 按钮点击事件
            Log.Info("Obstacle button clicked");
            EventSystem.Instance.Publish(self.Root(), new ChangeFindPathStateEvent() { state = EControlState.Obstacle });
        }

        private static void EndBtnClick(this UIHelpComponent self)
        {
            // 处理 End 按钮点击事件
            Log.Info("End button clicked");
            EventSystem.Instance.Publish(self.Root(), new ChangeFindPathStateEvent() { state = EControlState.End });
        }

        private static void StartBtnClick(this UIHelpComponent self)
        {
            // 处理 Start 按钮点击事件
            Log.Info("Start button clicked");
            EventSystem.Instance.Publish(self.Root(), new ChangeFindPathStateEvent() { state = EControlState.Start });
        }

        private static void AnimRunBtnClick(this UIHelpComponent self)
        {
            // 处理动画寻路按钮点击事件
            Log.Info($"AnimRun button clicked, Algorithm: {self.CurrentAlgorithm}");
            EventSystem.Instance.Publish(self.Root(), new AnimPathFindingEvent());
        }

        /// <summary>
        /// 初始化算法选择按钮
        /// </summary>
        private static void InitAlgorithmButtons(this UIHelpComponent self)
        {
            if (self.FindPathAlgorithmContent == null)
            {
                Log.Warning("FindPathAlgorithmContent is null, skipping algorithm buttons initialization");
                return;
            }

            // 清空之前的按钮
            self.ClearAlgorithmButtons();

            // 获取所有算法类型
            var algorithms = System.Enum.GetValues(typeof(PathfindingAlgorithmType));

            // 动态创建算法按钮
            foreach (PathfindingAlgorithmType algorithm in algorithms)
            {
                self.CreateAlgorithmButton(algorithm);
            }

            // 设置默认选择
            self.UpdateAlgorithmButtonSelection();
        }

        /// <summary>
        /// 动态创建算法按钮（优先克隆ScrollBtn，否则完全动态创建）
        /// </summary>
        private static void CreateAlgorithmButton(this UIHelpComponent self, PathfindingAlgorithmType algorithm)
        {
            Button button;
            GameObject buttonGO;

            if (self.ScrollBtn != null)
            {
                // 使用ScrollBtn作为模板克隆按钮
                buttonGO = UnityEngine.Object.Instantiate(self.ScrollBtn.gameObject, self.FindPathAlgorithmContent.transform);
                buttonGO.name = $"{algorithm}Btn";
                button = buttonGO.GetComponent<Button>();

                // 清除原有的点击事件
                button.onClick.RemoveAllListeners();

                Log.Info($"从ScrollBtn模板克隆算法按钮: {algorithm}");
            }
            else
            {
                // 完全动态创建按钮（备选方案）
                buttonGO = new GameObject($"{algorithm}Btn", typeof(RectTransform));
                buttonGO.transform.SetParent(self.FindPathAlgorithmContent.transform, false);

                // 添加Image组件作为按钮背景
                Image buttonImage = buttonGO.AddComponent<Image>();
                buttonImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);

                // 添加Button组件
                button = buttonGO.AddComponent<Button>();
                button.targetGraphic = buttonImage;

                // 设置按钮颜色状态
                ColorBlock colors = button.colors;
                colors.normalColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
                colors.selectedColor = new Color(0.6f, 0.8f, 1f, 1f);
                button.colors = colors;

                // 创建文本子对象
                GameObject textGO = new GameObject("Text", typeof(RectTransform));
                textGO.transform.SetParent(buttonGO.transform, false);

                RectTransform textRect = textGO.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;

                Text buttonText = textGO.AddComponent<Text>();
                buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                buttonText.fontSize = 14;
                buttonText.color = Color.black;
                buttonText.alignment = TextAnchor.MiddleCenter;

                Log.Info($"完全动态创建算法按钮: {algorithm}");
            }

            // 布局由Content中的布局组件自动处理，无需手动设置位置

            // 更新按钮文本
            Text text = buttonGO.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = GetAlgorithmDisplayName(algorithm);
            }

            // 添加到按钮列表
            self.AlgorithmButtons.Add(button);

            // 添加点击事件
            PathfindingAlgorithmType capturedAlgorithm = algorithm; // 避免闭包问题
            button.onClick.AddListener(() => self.OnAlgorithmButtonClick(capturedAlgorithm));
        }

        /// <summary>
        /// 获取算法显示名称
        /// </summary>
        private static string GetAlgorithmDisplayName(PathfindingAlgorithmType algorithm)
        {
            return algorithm switch
            {
                PathfindingAlgorithmType.BFS => "广度优先",
                PathfindingAlgorithmType.DFS => "深度优先",
                PathfindingAlgorithmType.AStar => "A*算法",
                PathfindingAlgorithmType.JPS => "JPS算法",
                _ => algorithm.ToString()
            };
        }

        /// <summary>
        /// 清空所有算法按钮
        /// </summary>
        private static void ClearAlgorithmButtons(this UIHelpComponent self)
        {
            // 清理现有按钮
            foreach (var button in self.AlgorithmButtons)
            {
                if (button != null && button.gameObject != null)
                {
                    button.onClick.RemoveAllListeners();
                    UnityEngine.Object.DestroyImmediate(button.gameObject);
                }
            }
            self.AlgorithmButtons.Clear();
        }

        /// <summary>
        /// 算法按钮点击事件
        /// </summary>
        private static void OnAlgorithmButtonClick(this UIHelpComponent self, PathfindingAlgorithmType algorithm)
        {
            Log.Info($"================== 开始切换寻路算法: {algorithm} ==================");
            self.CurrentAlgorithm = algorithm;

            // 清理当前算法的搜索结果，但保留起点终点和障碍物
            var scene = self.Root().CurrentScene();
            var operaComponent = scene.GetComponent<OperaComponent>();
            var findPathDataComponent = scene.GetComponent<FindPathDataComponent>();

            if (operaComponent != null)
            {
                self.ClearSearchResultsOnly(operaComponent);
            }

            // 更新算法组件
            var pathfindingComponent = scene.GetComponent<PathfindingComponent>();
            if (pathfindingComponent != null)
            {
                pathfindingComponent.SetAlgorithm(algorithm);
                // 清理所有算法组件的路径数据
                pathfindingComponent.ClearPath();
            
            }

            // 更新按钮视觉状态
            self.UpdateAlgorithmButtonSelection();
        }

        /// <summary>
        /// 仅清理搜索结果，保留起点、终点和障碍物
        /// </summary>
        private static void ClearSearchResultsOnly(this UIHelpComponent self, OperaComponent operaComponent)
        {
            var findPathDataComponent = operaComponent.Scene().GetComponent<FindPathDataComponent>();
            if (findPathDataComponent.CubeStatesArr == null)
            {
                Log.Warning("[ClearSearchResultsOnly] CubeStatesArr为null");
                return;
            }

            int width = findPathDataComponent.CubeStatesArr.GetLength(0);
            int height = findPathDataComponent.CubeStatesArr.GetLength(1);


            int clearedCount = 0;

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    CubeState currentState = findPathDataComponent.CubeStatesArr[x, z];
                    if (currentState == CubeState.Path || currentState == CubeState.Query)
                    {
                        operaComponent.SetCubeState(x, z, CubeState.None);
                        findPathDataComponent.CubeStatesArr[x, z] = CubeState.None;
                        clearedCount++;
                    }
                }
            }
        }

        /// <summary>
        /// 更新算法按钮选择状态
        /// </summary>
        private static void UpdateAlgorithmButtonSelection(this UIHelpComponent self)
        {
            // 获取所有算法类型
            var algorithms = System.Enum.GetValues(typeof(PathfindingAlgorithmType));
            int currentIndex = 0;
            int selectedIndex = 0;

            // 找到当前选择算法的索引
            foreach (PathfindingAlgorithmType algorithm in algorithms)
            {
                if (algorithm == self.CurrentAlgorithm)
                {
                    selectedIndex = currentIndex;
                    break;
                }
                currentIndex++;
            }

            // 更新所有按钮的视觉状态
            for (int i = 0; i < self.AlgorithmButtons.Count; i++)
            {
                Button button = self.AlgorithmButtons[i];
                if (button != null && button.targetGraphic != null)
                {
                    Image buttonImage = button.targetGraphic as Image;
                    if (buttonImage != null)
                    {
                        // 选中的按钮显示为蓝色，未选中的显示为灰色
                        if (i == selectedIndex)
                        {
                            buttonImage.color = new Color(0.6f, 0.8f, 1f, 1f); // 蓝色选中状态
                        }
                        else
                        {
                            buttonImage.color = new Color(0.8f, 0.8f, 0.8f, 1f); // 灰色默认状态
                        }
                    }

                    // 更新文本颜色
                    Text buttonText = button.GetComponentInChildren<Text>();
                    if (buttonText != null)
                    {
                        buttonText.color = i == selectedIndex ? Color.white : Color.black;
                    }
                }
            }

            Log.Info($"当前选择的算法: {self.CurrentAlgorithm}");
        }

        [EntitySystem]
        private static void Destroy(this ET.Client.UIHelpComponent self)
        {
            self.RunBtn.onClick.RemoveListener(self.RunBtnClick);
            self.ResetBtn.onClick.RemoveListener(self.ResetBtnClick);
            self.ObstacleBtn.onClick.RemoveListener(self.ObstacleBtnClick);
            self.EndBtn.onClick.RemoveListener(self.EndBtnClick);
            self.StartBtn.onClick.RemoveListener(self.StartBtnClick);
            self.AnimRunBtn.onClick.RemoveListener(self.AnimRunBtnClick);

            // 清理算法按钮
            self.ClearAlgorithmButtons();
        }
    }
}