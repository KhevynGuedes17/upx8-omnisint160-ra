#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEditor.XR.ARCore;
using UnityEditor.XR.ARSubsystems;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using Unity.XR.CoreUtils;

namespace Facens.Test
{
    [InitializeOnLoad]
    public static class ProjetoDeTesteSetup
    {
        const string ScenePath = "Assets/Scenes/ContextualAR.unity";
        const string MarkerPath = "Assets/Markers/equipment-target.png";
        const string LibraryPath = "Assets/ReferenceImages/ReferenceImageLibrary.asset";
        const string TargetName = "EquipmentTarget";
        const string SetupFlag = "Assets.Editor.SetupComplete.v1";

        static ProjetoDeTesteSetup() => EditorApplication.delayCall += AutoSetup;

        static void AutoSetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool(SetupFlag, false)) return;
            try { ConfigureProject(); SessionState.SetBool(SetupFlag, true); }
            catch (Exception ex) { Debug.LogError($"[RA OMNISINT160] Falha na configuracao automatica: {ex}"); }
        }

        [MenuItem("RA OMNISINT160/Configurar projeto completo")]
        public static void ConfigureProject()
        {
            ConfigureAndroid();
            var library = EnsureImageLibrary();
            EnsureScene(library, false);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("[RA OMNISINT160] Projeto configurado. Use Test > Validar projeto e depois Build and Run Android.");
        }

        [MenuItem("RA OMNISINT160/Validar projeto")]
        public static void ValidateProject()
        {
            ConfigureProject();
            EditorSceneManager.OpenScene(ScenePath);
            var errors = 0;

            errors += RequireObject<ARSession>("ARSession");
            errors += RequireObject<XROrigin>("XROrigin");
            errors += RequireObject<ARTrackedImageManager>("ARTrackedImageManager");
            errors += RequireObject<TestProcedureController>("TestProcedureController");
            errors += RequireObject<ARCameraManager>("ARCameraManager");
            errors += RequireObject<ARCameraBackground>("ARCameraBackground");
            errors += RequireObject<TestStatusOverlay>("TestStatusOverlay");

            var manager = UnityEngine.Object.FindAnyObjectByType<ARTrackedImageManager>();
            if (manager == null || manager.referenceLibrary == null || manager.referenceLibrary.count != 1) { Debug.LogError("[RA OMNISINT160] Reference Image Library ausente ou invalida."); errors++; }
            
            var xr = GetAndroidXRSettings(); var arCore = false;
            if (xr?.Manager != null) foreach (var loader in xr.Manager.activeLoaders) if (loader != null && loader.GetType().FullName == "UnityEngine.XR.ARCore.ARCoreLoader") arCore = true;
            
            if (!arCore) { Debug.LogError("[RA OMNISINT160] ARCore Loader nao habilitado."); errors++; }
            if (errors == 0) Debug.Log("[RA OMNISINT160] VALIDACAO OK: baseline estrutural pronto para Android ARCore.");
            else Debug.LogError($"[RA OMNISINT160] VALIDACAO FALHOU: {errors} problema(s).");
        }

        static int RequireObject<T>(string label) where T : UnityEngine.Object { if (UnityEngine.Object.FindAnyObjectByType<T>() != null) return 0; Debug.LogError($"[RA OMNISINT160] Componente ausente: {label}"); return 1; }

        [MenuItem("RA OMNISINT160/Build Android APK")] public static void BuildAndroidApk() => BuildAndroid(false);
        [MenuItem("RA OMNISINT160/Build and Run Android")] public static void BuildAndRunAndroid() => BuildAndroid(true);
        [MenuItem("RA OMNISINT160/Abrir cena principal")] public static void OpenMainScene() { ConfigureProject(); EditorSceneManager.OpenScene(ScenePath); }
        [MenuItem("RA OMNISINT160/Recriar cena baseline (sobrescreve)")]
        public static void RecreateScene()
        {
            if (!EditorUtility.DisplayDialog("RA OMNISINT160 - Recriar cena", "Sobrescrever a cena baseline?", "Recriar", "Cancelar")) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null) AssetDatabase.DeleteAsset(ScenePath);
            EnsureScene(EnsureImageLibrary(), true); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        }

        static XRReferenceImageLibrary EnsureImageLibrary()
        {
            var existing = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(LibraryPath);
            if (existing != null && existing.count == 1) return existing;
            Directory.CreateDirectory("Assets/ReferenceImages");
            if (existing != null) AssetDatabase.DeleteAsset(LibraryPath);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(MarkerPath);
            if (texture == null) throw new InvalidOperationException($"Marcador nao encontrado: {MarkerPath}");
            var library = ScriptableObject.CreateInstance<XRReferenceImageLibrary>();
            library.name = "TestReferenceImageLibrary";
            AssetDatabase.CreateAsset(library, LibraryPath);
            XRReferenceImageLibraryExtensions.Add(library);
            XRReferenceImageLibraryExtensions.SetName(library, 0, TargetName);
            XRReferenceImageLibraryExtensions.SetSpecifySize(library, 0, true);
            XRReferenceImageLibraryExtensions.SetSize(library, 0, new Vector2(0.20f, 0.133333f));
            XRReferenceImageLibraryExtensions.SetTexture(library, 0, texture, false);
            EditorUtility.SetDirty(library); AssetDatabase.SaveAssets();
            return library;
        }

        static void BuildAndroid(bool autoRun)
        {
            ConfigureProject();
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android)) throw new InvalidOperationException("Nao foi possivel selecionar Android.");
            Directory.CreateDirectory("Builds");
            var options = new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/Test-Contextual-AR.apk", target = BuildTarget.Android, targetGroup = BuildTargetGroup.Android, options = autoRun ? BuildOptions.AutoRunPlayer : BuildOptions.None };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new InvalidOperationException($"Build Android falhou: {report.summary.result}");
            Debug.Log($"[RA OMNISINT160] APK gerado em {options.locationPathName}");
        }

        static void BuildIOS(bool autoRun)
        {
            ConfigureProject();
        }


        static void ConfigureAndroid()
        {
            PlayerSettings.companyName = "Facens"; 
            PlayerSettings.productName = "AR OMNISINT 160";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "edu.facens.Testcontextualar");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
            var perTarget = EnsureXRGeneralSettings(); var android = perTarget.SettingsForBuildTarget(BuildTargetGroup.Android);
            if (android == null || android.Manager == null) throw new InvalidOperationException("Falha nas configuracoes XR Android.");
            android.InitManagerOnStart = true; EditorUtility.SetDirty(android);
            var assigned = XRPackageMetadataStore.AssignLoader(android.Manager, "UnityEngine.XR.ARCore.ARCoreLoader", BuildTargetGroup.Android);
            var present = false; foreach (var loader in android.Manager.activeLoaders) if (loader != null && loader.GetType().FullName == "UnityEngine.XR.ARCore.ARCoreLoader") present = true;
            if (!assigned && !present) throw new InvalidOperationException("Nao foi possivel habilitar ARCore Loader.");
            var arCore = ARCoreSettings.GetOrCreateSettings(); arCore.requirement = ARCoreSettings.Requirement.Required; EditorUtility.SetDirty(arCore);
        }

        static void ConfigureiOS()
        {
            PlayerSettings.companyName = "Facens";
            PlayerSettings.productName = "AR OMNISINT 160";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "edu.facens.Testcontextualar");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.OpenGLES3 });
            var perTarget = EnsureXRGeneralSettings(); 
            var ios = perTarget.SettingsForBuildTarget(BuildTargetGroup.iOS);
            if (ios == null || ios.Manager == null) throw new InvalidOperationException("Falha nas configuracoes XR iOS.");
            ios.InitManagerOnStart = true;
            EditorUtility.SetDirty(ios);
            var assigned = XRPackageMetadataStore.AssignLoader(ios.Manager, "UnityEngine.XR.ARCore.ARCoreLoader", BuildTargetGroup.iOS);
            var present = false;
            foreach (var loader in ios.Manager.activeLoaders)
            {
                if (loader != null && loader.GetType().FullName == "UnityEngine.XR.ARCore.ARCoreLoader")
                {
                    present = true;
                }
            }
            if (!assigned && !present) throw new InvalidOperationException("Nao foi possivel habilitar ARCore Loader.");
            var arCore = ARCoreSettings.GetOrCreateSettings(); 
            arCore.requirement = ARCoreSettings.Requirement.Required; 
            EditorUtility.SetDirty(arCore);
        }

        static XRGeneralSettingsPerBuildTarget EnsureXRGeneralSettings()
        {
            XRGeneralSettingsPerBuildTarget perTarget = null; EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.settingsKey, out perTarget);
            if (perTarget == null) { Directory.CreateDirectory("Assets/XR"); perTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>(); perTarget.name = "XRGeneralSettingsPerBuildTarget"; AssetDatabase.CreateAsset(perTarget, "Assets/XR/XRGeneralSettingsPerBuildTarget.asset"); EditorBuildSettings.AddConfigObject(XRGeneralSettings.settingsKey, perTarget, true); }
            if (!perTarget.HasSettingsForBuildTarget(BuildTargetGroup.Android)) perTarget.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Android);
            if (!perTarget.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android)) perTarget.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
            EditorUtility.SetDirty(perTarget); AssetDatabase.SaveAssets(); return perTarget;
        }
        static XRGeneralSettings GetAndroidXRSettings() { 
            XRGeneralSettingsPerBuildTarget p = null; 
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.settingsKey, out p); 
            return p?.SettingsForBuildTarget(BuildTargetGroup.Android); 
        }

        static void EnsureScene(XRReferenceImageLibrary library, bool force)
        {
            if (!force && AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null) { EnsureSceneInBuildSettings(); return; }
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var sessionObject = new GameObject("AR Session"); sessionObject.AddComponent<ARSession>(); sessionObject.AddComponent<ARInputManager>();
            var originObject = new GameObject("XR Origin (Mobile AR)"); var origin = originObject.AddComponent<XROrigin>();
            var cameraOffset = new GameObject("Camera Offset"); cameraOffset.transform.SetParent(originObject.transform, false);
            var cameraObject = new GameObject("Main Camera"); cameraObject.tag = "MainCamera"; cameraObject.transform.SetParent(cameraOffset.transform, false);
            var camera = cameraObject.AddComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black; camera.nearClipPlane = 0.01f; camera.farClipPlane = 30f;
            cameraObject.AddComponent<AudioListener>(); cameraObject.AddComponent<ARCameraManager>(); cameraObject.AddComponent<ARCameraBackground>();
#pragma warning disable CS0618
            cameraObject.AddComponent<ARPoseDriver>();
#pragma warning restore CS0618
            SetXROriginReference(origin, "Camera", "m_Camera", camera); SetXROriginReference(origin, "CameraFloorOffsetObject", "m_CameraFloorOffsetObject", cameraOffset);
            var imageManager = originObject.AddComponent<ARTrackedImageManager>(); imageManager.referenceLibrary = library; imageManager.requestedMaxNumberOfMovingImages = 1;
            originObject.AddComponent<TestProcedureController>();
            new GameObject("Test Status Overlay").AddComponent<TestStatusOverlay>();
            EditorSceneManager.SaveScene(scene, ScenePath); EnsureSceneInBuildSettings(); EditorSceneManager.OpenScene(ScenePath);
        }

        static void SetXROriginReference(XROrigin origin, string propertyName, string serializedFieldName, UnityEngine.Object value)
        {
            var property = typeof(XROrigin).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (property != null && property.CanWrite) { property.SetValue(origin, value); return; }
            var serialized = new SerializedObject(origin); var field = serialized.FindProperty(serializedFieldName);
            if (field == null) throw new InvalidOperationException($"Nao foi possivel configurar XROrigin.{propertyName}.");
            field.objectReferenceValue = value; serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        static void EnsureSceneInBuildSettings() { foreach (var entry in EditorBuildSettings.scenes) if (entry.path == ScenePath && entry.enabled) return; EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) }; }
    }
}
#endif

