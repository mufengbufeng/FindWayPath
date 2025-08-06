using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace ET
{
    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    public class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }

        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }

        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }

    public class ResourcesComponent : Singleton<ResourcesComponent>, ISingletonAwake
    {
        public void Awake()
        {
            YooAssets.Initialize();
        }

        protected override void Destroy()
        {
            YooAssets.Destroy();
        }

        public async ETTask CreatePackageAsync(string packageName, bool isDefault = false)
        {
            ResourcePackage package = YooAssets.CreatePackage(packageName);
            if (isDefault)
            {
                YooAssets.SetDefaultPackage(package);
            }

            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            EPlayMode ePlayMode = globalConfig.EPlayMode;

            // 编辑器下的模拟模式
            switch (ePlayMode)
            {
                case EPlayMode.EditorSimulateMode:
                    {
                        // EditorSimulateModeParameters createParameters = new();
                        // createParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild("ScriptableBuildPipeline", packageName);
                        // await package.InitializeAsync(createParameters).Task;
                        var buildResult = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
                        var packageRoot = buildResult.PackageRootDirectory;
                        var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                        var initParameters = new EditorSimulateModeParameters();
                        initParameters.EditorFileSystemParameters = editorFileSystemParams;
                        var initOperation = package.InitializeAsync(initParameters);
                        await initOperation.Task;

                        break;
                    }
                case EPlayMode.OfflinePlayMode:
                    {
                        // OfflinePlayModeParameters createParameters = new();
                        // await package.InitializeAsync(createParameters).Task;
                        var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                        var initParameters = new OfflinePlayModeParameters();
                        initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
                        var initOperation = package.InitializeAsync(initParameters);
                        await initOperation.Task;
                        break;
                    }
                case EPlayMode.HostPlayMode:
                    {
                        string defaultHostServer = GetHostServerURL();
                        string fallbackHostServer = GetHostServerURL();
                        // HostPlayModeParameters createParameters = new();
                        // createParameters.BuildinQueryServices = new GameQueryServices();
                        // createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);

                        IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                        var cacheFileSystemParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
                        var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

                        var initParameters = new HostPlayModeParameters();
                        initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
                        initParameters.CacheFileSystemParameters = cacheFileSystemParams;
                        var initOperation = package.InitializeAsync(initParameters);
                        await initOperation.Task;
                        break;
                    }
                case EPlayMode.WebPlayMode:
                    {
                        string defaultHostServer = GetHostServerURL();
                        string fallbackHostServer = GetHostServerURL();

                        IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                        var webServerFileSystemParams = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
                        var webRemoteFileSystemParams = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(remoteServices); //支持跨域下载

                        var initParameters = new WebPlayModeParameters();
                        initParameters.WebServerFileSystemParameters = webServerFileSystemParams;
                        initParameters.WebRemoteFileSystemParameters = webRemoteFileSystemParams;

                        var initOperation = package.InitializeAsync(initParameters);
                        await initOperation.Task;
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            await RequestPackageVersion(packageName);
        }

        static string GetHostServerURL()
        {
            //string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
            string hostServerIP = "http://127.0.0.1";
            string appVersion = "v1.0";

#if UNITY_EDITOR
            if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
            {
                return $"{hostServerIP}/CDN/Android/{appVersion}";
            }
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
            {
                return $"{hostServerIP}/CDN/IPhone/{appVersion}";
            }
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
            {
                return $"{hostServerIP}/CDN/WebGL/{appVersion}";
            }

            return $"{hostServerIP}/CDN/PC/{appVersion}";
#else
            if (Application.platform == RuntimePlatform.Android)
            {
                return $"{hostServerIP}/CDN/Android/{appVersion}";
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return $"{hostServerIP}/CDN/IPhone/{appVersion}";
            }
            else if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                return $"{hostServerIP}/CDN/WebGL/{appVersion}";
            }

            return $"{hostServerIP}/CDN/PC/{appVersion}";
#endif
        }

        public void DestroyPackage(string packageName)
        {
            ResourcePackage package = YooAssets.GetPackage(packageName);
            package.UnloadUnusedAssetsAsync();
        }

        /// <summary>
        /// 主要用来加载dll config aotdll，因为这时候纤程还没创建，无法使用ResourcesLoaderComponent。
        /// 游戏中的资源应该使用ResourcesLoaderComponent来加载
        /// </summary>
        public async ETTask<T> LoadAssetAsync<T>(string location) where T : UnityEngine.Object
        {
            AssetHandle handle = YooAssets.LoadAssetAsync<T>(location);
            await handle.Task;
            T t = (T)handle.AssetObject;
            handle.Release();
            return t;
        }

        /// <summary>
        /// 主要用来加载dll config aotdll，因为这时候纤程还没创建，无法使用ResourcesLoaderComponent。
        /// 游戏中的资源应该使用ResourcesLoaderComponent来加载
        /// </summary>
        public async ETTask<Dictionary<string, T>> LoadAllAssetsAsync<T>(string location) where T : UnityEngine.Object
        {
            AllAssetsHandle allAssetsOperationHandle = YooAssets.LoadAllAssetsAsync<T>(location);
            await allAssetsOperationHandle.Task;
            Dictionary<string, T> dictionary = new Dictionary<string, T>();
            foreach (UnityEngine.Object assetObj in allAssetsOperationHandle.AllAssetObjects)
            {
                T t = assetObj as T;
                dictionary.Add(t.name, t);
            }

            allAssetsOperationHandle.Release();
            return dictionary;
        }

        public async ETTask RequestPackageVersion(string packageName)
        {
            var package = YooAssets.GetPackage(packageName);
            var operation = package.RequestPackageVersionAsync();
            await operation.Task;
            var packageVersion = operation.PackageVersion;
            await UpdatePackageManifest(packageName, packageVersion);
        }

        public async ETTask UpdatePackageManifest(string packageName, string packageVersion)
        {
            var package = YooAssets.GetPackage(packageName);
            var operation = package.UpdatePackageManifestAsync(packageVersion);
            await operation.Task;
            await Download();
        }

        public async ETTask Download()
        {
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var package = YooAssets.GetPackage("DefaultPackage");
            var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);

            //没有需要下载的资源
            if (downloader.TotalDownloadCount == 0)
            {
                await ETTask.CompletedTask;
            }

            //需要下载的文件总数和总大小
            int totalDownloadCount = downloader.TotalDownloadCount;
            long totalDownloadBytes = downloader.TotalDownloadBytes;

            //注册回调方法
            downloader.DownloadFinishCallback = OnDownloadFinishFunction; //当下载器结束（无论成功或失败）
            downloader.DownloadErrorCallback = OnDownloadErrorFunction; //当下载器发生错误
            downloader.DownloadUpdateCallback = OnDownloadUpdateFunction; //当下载进度发生变化
            downloader.DownloadFileBeginCallback = OnDownloadFileBeginFunction; //当开始下载某个文件

            //开启下载
            downloader.BeginDownload();
            await downloader.Task;
        }

        private void OnDownloadFinishFunction(DownloaderFinishData data)
        {
            Log.Info($"下载器结束：{data.PackageName}");
        }

        private void OnDownloadErrorFunction(DownloadErrorData data)
        {
            Log.Error($"下载器发生错误：{data.PackageName}，错误信息：{data.ErrorInfo}");
        }

        private void OnDownloadUpdateFunction(DownloadUpdateData data)
        {
            Log.Info($"下载器下载进度发生变化，当前进度：{data.Progress:P2}，已下载大小：{data.CurrentDownloadBytes}/{data.TotalDownloadBytes}");
        }

        private void OnDownloadFileBeginFunction(DownloadFileData data)
        {
            Log.Info($"下载器开始下载文件，文件名：{data.FileName}，文件大小：{data.FileSize}");
        }
    }
}