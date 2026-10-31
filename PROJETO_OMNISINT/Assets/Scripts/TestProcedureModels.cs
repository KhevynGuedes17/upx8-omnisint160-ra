using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Facens.Test
{
    // ---------- Dados (JSON) ----------

    [Serializable]
    public sealed class TestProcedureCollection
    {
        public TestProcedureDefinition[] procedures;
    }

    [Serializable]
    public sealed class TestProcedureDefinition
    {
        public string id;
        public string title;
        public string target;                         // Nome da ReferenceImage (deve ser IGUAL ao da library)
        public TestComponentDefinition[] components;  // Opcional: se vazio, o layout e gerado automaticamente
        public TestProcedureStep[] steps;
    }

    [Serializable]
    public sealed class TestComponentDefinition
    {
        public string id;
        public Vector3 position; // Posicao local em relacao a imagem (metros)
        public string primitive; // Opcional: "sphere" cria uma esfera em vez de um cubo
    }

    [Serializable]
    public sealed class TestProcedureStep
    {
        public int id;
        public string component;
        public string title;
        public string instruction;
    }

    public sealed class TestComponentTag : MonoBehaviour
    {
        public string target;      // ReferenceImage a que o componente pertence
        public string componentId;
    }

    // ---------- Estado + visual de UMA ancora ----------

    public sealed class TestProcedureSession
    {
        static readonly Vector3 NormalScale = new(0.035f, 0.016f, 0.035f);
        static readonly Vector3 ActiveScale = new(0.044f, 0.021f, 0.044f);
        static readonly Color DoneColor = new(0.26f, 0.63f, 0.28f);
        static readonly Color ActiveColor = new(0.95f, 0.49f, 0.18f);
        static readonly Color IdleColor = new(0.32f, 0.38f, 0.43f);

        readonly Transform _holder; // onde o conteudo fica "estacionado" quando a ancora some
        readonly Dictionary<string, Renderer> _renderers = new();
        readonly Dictionary<string, Material> _materials = new();
        GameObject _contentRoot;
        bool[] _completed;
        float _startedAt = -1f;
        float _finishedAt = -1f;

        public TestProcedureSession(TestProcedureDefinition definition, Transform holder)
        {
            Definition = definition;
            _holder = holder;
            Reset();
        }

        public TestProcedureDefinition Definition { get; }
        public string Target => Definition.target;
        public string Title => string.IsNullOrEmpty(Definition.title) ? Definition.target : Definition.title;
        public bool TargetFound { get; private set; }
        public float LastFoundAt { get; private set; } = -1f;
        public int ActiveStepIndex { get; private set; }
        public string StatusMessage { get; private set; }
        public bool IsComplete => _finishedAt >= 0f;
        public int StepCount => Definition.steps.Length;
        public TestProcedureStep CurrentStep => Definition.steps[Mathf.Clamp(ActiveStepIndex, 0, StepCount - 1)];
        public float Progress => StepCount == 0 ? 0f : CompletedCount / (float)StepCount;
        public float ElapsedSeconds => _startedAt < 0f ? 0f : Mathf.Max(0f, (_finishedAt >= 0f ? _finishedAt : Time.realtimeSinceStartup) - _startedAt);

        public int CompletedCount
        {
            get
            {
                var count = 0;
                if (_completed != null) foreach (var value in _completed) if (value) count++;
                return count;
            }
        }

        // ---------- Rastreamento ----------

        public void SetTracking(bool found, Transform anchor)
        {
            var wasFound = TargetFound;
            if (found && anchor != null)
            {
                EnsureContent();
                var t = _contentRoot.transform;
                if (t.parent != anchor)
                {
                    t.SetParent(anchor, false);
                    t.localPosition = Vector3.zero;
                    t.localRotation = Quaternion.identity;
                }
                _contentRoot.SetActive(true);
                TargetFound = true;
                if (!wasFound)
                {
                    LastFoundAt = Time.realtimeSinceStartup;
                    if (_startedAt < 0f) _startedAt = LastFoundAt;
                    StatusMessage = IsComplete ? $"{Title}: procedimento concluido." : $"Alvo reconhecido. {StepStatus()}";
                    RefreshVisuals();
                }
            }
            else
            {
                if (_contentRoot != null)
                {
                    _contentRoot.SetActive(false);
                    // Desacopla da imagem: o ARTrackedImage pode ser destruido pelo ARFoundation.
                    _contentRoot.transform.SetParent(_holder, false);
                }
                TargetFound = false;
                if (wasFound) StatusMessage = $"{Title}: alvo perdido. Reposicione a camera.";
            }
        }

        // ---------- Procedimento ----------

        public bool Confirm()
        {
            if (!TargetFound || IsComplete) return false;
            var step = CurrentStep;
            _completed[ActiveStepIndex] = true;
            Debug.Log($"[Test] {Title}: etapa {step.id} concluida (componente {step.component})");
            if (ActiveStepIndex >= StepCount - 1)
            {
                _finishedAt = Time.realtimeSinceStartup;
                StatusMessage = $"{Title}: procedimento concluido em {ElapsedSeconds:F0} s.";
                Debug.Log($"[Test] {Title}: concluido em {ElapsedSeconds:F1} s.");
            }
            else
            {
                ActiveStepIndex++;
                StatusMessage = StepStatus();
            }
            RefreshVisuals();
            return true;
        }

        public void Previous()
        {
            if (IsComplete || ActiveStepIndex <= 0) return;
            ActiveStepIndex--;
            StatusMessage = StepStatus();
            RefreshVisuals();
        }

        public void Reset()
        {
            ActiveStepIndex = 0;
            _completed = new bool[StepCount];
            _finishedAt = -1f;
            _startedAt = TargetFound ? Time.realtimeSinceStartup : -1f;
            StatusMessage = TargetFound ? StepStatus() : $"{Title}: aponte a camera para o alvo.";
            RefreshVisuals();
            Debug.Log($"[Test] {Title}: procedimento reiniciado.");
        }

        string StepStatus() => $"{Title} - Etapa {CurrentStep.id}: {CurrentStep.title}";

        // ---------- Visual ----------

        void EnsureContent()
        {
            if (_contentRoot != null) return;
            _contentRoot = new GameObject($"Test_Content_{Target}");
            foreach (var component in ResolveComponents()) {
                Debug.Log("AIIIII " + component.id);
                if (component.id != "Monitor de oxigênio" && component.id != "Emergência") CreateComponent(component.id, component.position);
                else CreateModel(component.id, component.position);
            }
        }

        IEnumerable<TestComponentDefinition> ResolveComponents()
        {
            if (Definition.components != null && Definition.components.Length > 0)
                return Definition.components;

            // Layout automatico: componentes distintos dos passos, lado a lado no eixo X.
            var ids = new List<string>();
            foreach (var step in Definition.steps) if (!ids.Contains(step.component)) ids.Add(step.component);
            var list = new List<TestComponentDefinition>();
            for (var i = 0; i < ids.Count; i++)
                list.Add(new TestComponentDefinition { id = ids[i], position = new Vector3((i - (ids.Count - 1) / 2f) * 0.055f, 0.018f, 0f) });
            return list;
        }

        void CreateModel(string id, Vector3 position)
        {
            Debug.Log("AIIIII " + id);
            var definition = FindComponentDefinition(id);
            GameObject prefab;
            if (id == "Monitor de oxigênio")
            {
                prefab = Resources.Load<GameObject>("Momentary Switch Red");
                if (prefab == null)
                {
                    Debug.LogError("Prefab 'Momentary Switch Red' não encontrado em Resources.");
                    return;
                }    
            }
            else if (id == "Emergência")
            {
                prefab = Resources.Load<GameObject>("Emergency Switch");
                if (prefab == null)
                {
                    Debug.LogError("Prefab 'Emergency Switch' não encontrado em Resources.");
                    return;
                }    
            }
            else
            {
                prefab = Resources.Load<GameObject>("Emergency Switch");
                if (prefab == null)
                {
                    Debug.LogError("Prefab 'Emergency Switch' não encontrado em Resources.");
                    return;
                }    
            }

            // Cria a instância na cena, já como filha do contentRoot
            var component = UnityEngine.Object.Instantiate(prefab, _contentRoot.transform, false);
            component.name = $"Test_Component_{id}";
            component.transform.localPosition = position;

            var tag = component.AddComponent<TestComponentTag>();
            tag.target = Target;
            tag.componentId = id;

            // O Renderer pode estar em um filho do prefab
            var renderer = component.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                Debug.LogError("Nenhum Renderer encontrado no prefab.");
                return;
            }
            _renderers[id] = renderer;
            _materials[id] = renderer.material; // instância própria

            var labelObject = new GameObject($"Label_{id}");
            labelObject.transform.SetParent(component.transform, false);
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

        void CreateComponent(string id, Vector3 position)
        {
            var definition = FindComponentDefinition(id);
            var primitive = string.Equals(definition?.primitive, "sphere", StringComparison.OrdinalIgnoreCase)
                ? PrimitiveType.Sphere
                : PrimitiveType.Cube;
            var component = GameObject.CreatePrimitive(primitive);
            component.name = $"Test_Component_{id}";
            component.transform.SetParent(_contentRoot.transform, false);
            component.transform.localPosition = position;
            component.transform.localScale = primitive == PrimitiveType.Sphere
                ? new Vector3(0.055f, 0.055f, 0.055f)
                : NormalScale;
            var tag = component.AddComponent<TestComponentTag>();
            tag.target = Target;
            tag.componentId = id;
            var renderer = component.GetComponent<Renderer>();
            _renderers[id] = renderer;
            _materials[id] = renderer.material; // instancia propria

            var labelObject = new GameObject($"Label_{id}");
            labelObject.transform.SetParent(component.transform, false);
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

        TestComponentDefinition FindComponentDefinition(string id)
        {
            if (Definition.components == null) return null;
            foreach (var component in Definition.components)
                if (string.Equals(component.id, id, StringComparison.OrdinalIgnoreCase)) return component;
            return null;
        }

        void RefreshVisuals()
        {
            if (_renderers.Count == 0) return;
            for (var i = 0; i < Definition.steps.Length; i++)
            {
                var step = Definition.steps[i];
                if (!_renderers.TryGetValue(step.component, out var renderer)) continue;
                var active = !IsComplete && i == ActiveStepIndex;
                var done = _completed[i];
                _materials[step.component].color = done ? DoneColor : active ? ActiveColor : IdleColor;
                var sphere = string.Equals(FindComponentDefinition(step.component)?.primitive, "sphere", StringComparison.OrdinalIgnoreCase);
                renderer.transform.localScale = sphere
                    ? (active ? Vector3.one * 0.066f : Vector3.one * 0.055f)
                    : active ? ActiveScale : NormalScale;
            }
        }

        public void Dispose()
        {
            foreach (var material in _materials.Values) if (material != null) UnityEngine.Object.Destroy(material);
            _materials.Clear();
            _renderers.Clear();
            if (_contentRoot != null) UnityEngine.Object.Destroy(_contentRoot);
        }
    }
}
