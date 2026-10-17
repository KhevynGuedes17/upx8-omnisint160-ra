using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Facens.Test
{
    [RequireComponent(typeof(ARTrackedImageManager))]
    public sealed class TestProcedureController : MonoBehaviour
    {
        const string TargetName = "TestEquipmentTarget"; // Nesse caso, este nome deve ser IGUAL ao ReferenceImage
        readonly Dictionary<string, Renderer> _componentRenderers = new();
        readonly Dictionary<string, Material> _componentMaterials = new();

        ARTrackedImageManager _imageManager;
        TestProcedureDefinition _procedure;
        GameObject _contentRoot;
        ARTrackedImage _trackedImage;
        bool[] _completed;
        int _activeStepIndex;
        float _startedAt = -1f;
        float _finishedAt = -1f;

        public bool TargetFound { get; private set; }
        public bool IsComplete => _finishedAt >= 0f;
        public int ActiveStepIndex => _activeStepIndex;
        public int StepCount => _procedure?.steps?.Length ?? 0;
        public TestProcedureStep CurrentStep => StepCount == 0 ? null : _procedure.steps[Mathf.Clamp(_activeStepIndex, 0, StepCount - 1)];
        public string ProcedureTitle => _procedure?.title ?? "Procedimento Test";
        public string StatusMessage { get; private set; } = "Aponte a camera para o alvo Test.";
        public float Progress => StepCount == 0 ? 0f : CompletedCount / (float)StepCount;
        public int CompletedCount { get { var count = 0; if (_completed != null) foreach (var value in _completed) if (value) count++; return count; } }
        public float ElapsedSeconds => _startedAt < 0f ? 0f : Mathf.Max(0f, (_finishedAt >= 0f ? _finishedAt : Time.realtimeSinceStartup) - _startedAt);

        void Awake()
        {
            _imageManager = GetComponent<ARTrackedImageManager>();
            LoadProcedure();
            ResetProcedure();
        }

        void OnEnable() => _imageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
        void OnDisable() => _imageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);

        void LoadProcedure()
        {
            var asset = Resources.Load<TextAsset>("Test_procedure");
            if (asset == null) throw new InvalidOperationException("Assets/Resources/Test_procedure.json nao foi encontrado.");
            _procedure = JsonUtility.FromJson<TestProcedureDefinition>(asset.text);
            if (_procedure?.steps == null || _procedure.steps.Length == 0) throw new InvalidOperationException("Procedimento Test vazio ou invalido.");
        }

        void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
        {
            foreach (var image in args.added) HandleTrackedImage(image);
            foreach (var image in args.updated) HandleTrackedImage(image);
            foreach (var removed in args.removed)
            {
                if (_trackedImage != null && removed.Key == _trackedImage.trackableId)
                    SetTargetFound(false, null);
            }
        }

        void HandleTrackedImage(ARTrackedImage image)
        {
            if (image.referenceImage.name != TargetName) return;
            var tracking = image.trackingState == TrackingState.Tracking;
            SetTargetFound(tracking, image);
        }

        void SetTargetFound(bool found, ARTrackedImage image)
        {
            if (found && image != null)
            {
                _trackedImage = image;
                if (_startedAt < 0f) _startedAt = Time.realtimeSinceStartup;
                EnsureContextualContent(image.transform);
                _contentRoot.SetActive(true);
                StatusMessage = IsComplete ? "Procedimento concluido." : $"Alvo reconhecido. Etapa {CurrentStep.id}: {CurrentStep.title}";
            }
            else
            {
                if (_contentRoot != null) _contentRoot.SetActive(false);
                StatusMessage = "Alvo perdido. Reposicione a camera para visualizar o equipamento.";
            }
            TargetFound = found;
        }

        void EnsureContextualContent(Transform target)
        {
            if (_contentRoot == null)
            {
                _contentRoot = new GameObject("Test_ContextualContent");
                CreateComponent("A", new Vector3(-0.055f, 0.018f, 0f));
                CreateComponent("B", new Vector3(0f, 0.018f, 0f));
                CreateComponent("C", new Vector3(0.055f, 0.018f, 0f));
            }
            _contentRoot.transform.SetParent(target, false);
            _contentRoot.transform.localPosition = Vector3.zero;
            _contentRoot.transform.localRotation = Quaternion.identity;
            RefreshVisuals();
        }

        void CreateComponent(string id, Vector3 position)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"Test_Component_{id}";
            cube.transform.SetParent(_contentRoot.transform, false);
            cube.transform.localPosition = position;
            cube.transform.localScale = new Vector3(0.035f, 0.016f, 0.035f);
            var tag = cube.AddComponent<TestComponentTag>();
            tag.componentId = id;
            var renderer = cube.GetComponent<Renderer>();
            var material = renderer.material ? renderer.material : new Material(Shader.Find("Standard"));
            renderer.material = material;
            _componentRenderers[id] = renderer;
            _componentMaterials[id] = material;

            var labelObject = new GameObject($"Label_{id}");
            labelObject.transform.SetParent(cube.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            labelObject.transform.localScale = Vector3.one * 0.7f;
            var label = labelObject.AddComponent<TextMesh>();
            label.text = id;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 42;
            label.characterSize = 0.1f;
            label.color = Color.white;
        }

        void Update()
        {
            if (!TargetFound || IsComplete || Input.touchCount != 1) return;
            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Ended || TestStatusOverlay.IsTouchOverOverlay(touch.position)) return;
            var camera = Camera.main;
            if (camera == null) return;
            var ray = camera.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out var hit, 10f))
            {
                var tag = hit.collider.GetComponent<TestComponentTag>();
                if (tag != null && CurrentStep != null && tag.componentId == CurrentStep.component)
                    ConfirmCurrentStep();
            }
        }

        public void ConfirmCurrentStep()
        {
            if (!TargetFound || IsComplete || CurrentStep == null) return;
            _completed[_activeStepIndex] = true;
            Debug.Log($"[Test] Etapa {CurrentStep.id} concluida: componente {CurrentStep.component}");
            if (_activeStepIndex >= StepCount - 1)
            {
                _finishedAt = Time.realtimeSinceStartup;
                StatusMessage = $"Procedimento concluido em {ElapsedSeconds:F0} s.";
                Debug.Log($"[Test] Procedimento concluido em {ElapsedSeconds:F1} s.");
            }
            else
            {
                _activeStepIndex++;
                StatusMessage = $"Etapa {CurrentStep.id}: {CurrentStep.title}";
            }
            RefreshVisuals();
        }

        public void PreviousStep()
        {
            if (IsComplete || _activeStepIndex <= 0) return;
            _activeStepIndex--;
            StatusMessage = $"Etapa {CurrentStep.id}: {CurrentStep.title}";
            RefreshVisuals();
        }

        public void ResetProcedure()
        {
            _activeStepIndex = 0;
            _completed = new bool[_procedure.steps.Length];
            _finishedAt = -1f;
            _startedAt = TargetFound ? Time.realtimeSinceStartup : -1f;
            StatusMessage = TargetFound ? $"Etapa {CurrentStep.id}: {CurrentStep.title}" : "Aponte a camera para o alvo Test.";
            RefreshVisuals();
            Debug.Log("[Test] Procedimento reiniciado.");
        }

        void RefreshVisuals()
        {
            if (_procedure == null || _componentRenderers.Count == 0) return;
            for (var i = 0; i < _procedure.steps.Length; i++)
            {
                var step = _procedure.steps[i];
                if (!_componentRenderers.TryGetValue(step.component, out var renderer)) continue;
                var active = !IsComplete && i == _activeStepIndex;
                var done = _completed[i];
                var color = done ? new Color(0.26f, 0.63f, 0.28f) : active ? new Color(0.95f, 0.49f, 0.18f) : new Color(0.32f, 0.38f, 0.43f);
                _componentMaterials[step.component].color = color;
                renderer.transform.localScale = active ? new Vector3(0.044f, 0.021f, 0.044f) : new Vector3(0.035f, 0.016f, 0.035f);
            }
        }

        void OnDestroy()
        {
            foreach (var material in _componentMaterials.Values) if (material != null) Destroy(material);
        }
    }
}
