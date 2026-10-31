using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Facens.Test
{
    [RequireComponent(typeof(ARTrackedImageManager))]
    public sealed class TestProcedureController : MonoBehaviour
    {
        // Usado apenas se o JSON estiver no formato antigo (um unico procedimento sem "target").
        const string LegacyTargetName = "TestEquipmentTarget";

        readonly Dictionary<string, TestProcedureSession> _sessions = new(StringComparer.OrdinalIgnoreCase); // chave = nome da ReferenceImage
        readonly List<TestProcedureSession> _ordered = new();
        readonly Dictionary<TrackableId, string> _trackableTargets = new();
        readonly HashSet<string> _warnedUnknown = new();
        readonly Dictionary<TrackableId, TrackingState> _lastStates = new();

        ARTrackedImageManager _imageManager;
        TestProcedureSession _active;

        /// <summary>Procedimento em foco (o ultimo reconhecido, ou o tocado). Null se nenhuma ancora foi vista.</summary>
        public TestProcedureSession Active => _active;
        /// <summary>Nome da ultima ReferenceImage reconhecida que nao tem procedimento no JSON (diagnostico).</summary>
        public string UnmatchedImage { get; private set; }
        /// <summary>Estado do rastreamento do DISPOSITIVO (ARCore). Sem "OK", nenhuma imagem e detectada.</summary>
        public string DeviceTrackingInfo => ARSession.state == ARSessionState.SessionTracking
            ? "OK"
            : $"{ARSession.state} ({ARSession.notTrackingReason})";
        public IReadOnlyList<TestProcedureSession> Sessions => _ordered;
        public bool AnyTargetFound { get { foreach (var s in _ordered) if (s.TargetFound) return true; return false; } }

        void Awake()
        {
            _imageManager = GetComponent<ARTrackedImageManager>();
            LoadProcedures();
        }

        void OnEnable()
        {
            _imageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
            ARSession.stateChanged += OnSessionStateChanged;
        }

        void OnDisable()
        {
            _imageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
            ARSession.stateChanged -= OnSessionStateChanged;
        }

        static void OnSessionStateChanged(ARSessionStateChangedEventArgs args) =>
            Debug.Log($"[Test] ARSession state={args.state} notTrackingReason={ARSession.notTrackingReason}");

        void OnDestroy()
        {
            foreach (var session in _ordered) session.Dispose();
        }

        void Start() => ValidateAgainstLibrary();

        // Cruza os nomes da ReferenceImageLibrary com os "target" do JSON e avisa no Console.
        void ValidateAgainstLibrary()
        {
            var library = _imageManager.referenceLibrary;
            if (library == null)
            {
                Debug.LogError("[Test] ARTrackedImageManager sem ReferenceImageLibrary atribuida.");
                return;
            }

            var libraryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < library.count; i++)
            {
                var libName = library[i].name;
                libraryNames.Add(libName);
                var matched = _sessions.ContainsKey(libName);
                var msg = $"[Test] Library[{i}] nome='{libName}' size={library[i].size} -> {(matched ? "procedimento OK" : "SEM procedimento no JSON")}";
                if (matched) Debug.Log(msg); else Debug.LogWarning(msg);
            }

            foreach (var session in _ordered)
                if (!libraryNames.Contains(session.Target))
                    Debug.LogWarning($"[Test] Procedimento '{session.Title}' usa target '{session.Target}', que NAO existe na ReferenceImageLibrary.");
        }

        // ---------- Carregamento ----------

        void LoadProcedures()
        {
            var asset = Resources.Load<TextAsset>("Test_procedure");
            if (asset == null) throw new InvalidOperationException("Assets/Resources/Test_procedure.json nao foi encontrado.");

            var definitions = new List<TestProcedureDefinition>();
            var collection = JsonUtility.FromJson<TestProcedureCollection>(asset.text);
            if (collection?.procedures != null && collection.procedures.Length > 0)
            {
                definitions.AddRange(collection.procedures);
            }
            else
            {
                // Compatibilidade com o formato antigo (um procedimento na raiz do JSON).
                var single = JsonUtility.FromJson<TestProcedureDefinition>(asset.text);
                if (single?.steps != null && single.steps.Length > 0)
                {
                    if (string.IsNullOrEmpty(single.target)) single.target = LegacyTargetName;
                    definitions.Add(single);
                }
            }

            foreach (var definition in definitions)
            {
                if (string.IsNullOrEmpty(definition.target))
                    throw new InvalidOperationException($"Procedimento '{definition.id}' sem campo 'target' (nome da ReferenceImage).");
                if (definition.steps == null || definition.steps.Length == 0)
                    throw new InvalidOperationException($"Procedimento '{definition.id}' sem etapas.");
                if (_sessions.ContainsKey(definition.target))
                    throw new InvalidOperationException($"Mais de um procedimento usa o target '{definition.target}'.");

                var session = new TestProcedureSession(definition, transform);
                _sessions.Add(definition.target, session);
                _ordered.Add(session);
            }

            if (_ordered.Count == 0) throw new InvalidOperationException("Nenhum procedimento valido em Test_procedure.json.");
        }

        // ---------- Rastreamento ----------

        void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
        {
            foreach (var image in args.added) HandleTrackedImage(image);
            foreach (var image in args.updated) HandleTrackedImage(image);
            foreach (var removed in args.removed)
            {
                if (_trackableTargets.TryGetValue(removed.Key, out var target) && _sessions.TryGetValue(target, out var session))
                    SetTargetFound(session, false, null);
                _trackableTargets.Remove(removed.Key);
                _lastStates.Remove(removed.Key);
            }
        }

        void HandleTrackedImage(ARTrackedImage image)
        {
            var imageName = image.referenceImage.name;
            if (!_lastStates.TryGetValue(image.trackableId, out var lastState) || lastState != image.trackingState)
            {
                _lastStates[image.trackableId] = image.trackingState;
                Debug.Log($"[Test] Imagem '{imageName}' trackingState={image.trackingState} (dispositivo: {DeviceTrackingInfo})");
            }
            if (!_sessions.TryGetValue(imageName, out var session))
            {
                UnmatchedImage = imageName;
                if (_warnedUnknown.Add(imageName))
                    Debug.LogWarning($"[Test] ReferenceImage '{imageName}' reconhecida, mas nao possui procedimento no JSON (campo 'target').");
                return;
            }
            _trackableTargets[image.trackableId] = imageName;
            SetTargetFound(session, image.trackingState == TrackingState.Tracking, image);
        }

        void SetTargetFound(TestProcedureSession session, bool found, ARTrackedImage image)
        {
            var wasFound = session.TargetFound;
            session.SetTracking(found, image != null ? image.transform : null);

            if (found && !wasFound)
                _active = session; // nova ancora reconhecida passa a ser o foco
            else if (!found && wasFound && _active == session)
                _active = MostRecentFound() ?? session; // cai para outra ancora visivel; se nao houver, mantem a atual
        }

        TestProcedureSession MostRecentFound()
        {
            TestProcedureSession best = null;
            foreach (var session in _ordered)
                if (session.TargetFound && (best == null || session.LastFoundAt > best.LastFoundAt)) best = session;
            return best;
        }

        // ---------- Interacao ----------

        void Update()
        {
            if (!AnyTargetFound || Input.touchCount != 1) return;
            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Ended || TestStatusOverlay.IsTouchOverOverlay(touch.position)) return;
            var camera = Camera.main;
            if (camera == null) return;

            var ray = camera.ScreenPointToRay(touch.position);
            if (!Physics.Raycast(ray, out var hit, 10f)) return;
            var tag = hit.collider.GetComponent<TestComponentTag>();
            if (tag == null || !_sessions.TryGetValue(tag.target, out var session)) return;
            if (!session.TargetFound || session.IsComplete || tag.componentId != session.CurrentStep.component) return;

            _active = session; // tocar em um componente de outra ancora visivel muda o foco
            session.Confirm();
        }

        // Os botoes do overlay atuam sobre o procedimento em foco.
        public void ConfirmCurrentStep() => _active?.Confirm();
        public void PreviousStep() => _active?.Previous();
        public void ResetProcedure() => _active?.Reset();
        public void ResetAll() { foreach (var session in _ordered) session.Reset(); }
    }
}