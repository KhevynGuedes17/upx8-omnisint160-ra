using UnityEngine;

namespace Facens.Test
{
    public sealed class TestStatusOverlay : MonoBehaviour
    {
        TestProcedureController _controller;
        GUIStyle _titleStyle, _textStyle, _smallStyle, _buttonStyle;

        void Start() => _controller = FindAnyObjectByType<TestProcedureController>();

        void EnsureStyles()
        {
            if (_titleStyle != null) return;
            _titleStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(18, Screen.width / 24), fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            _textStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(13, Screen.width / 34), wordWrap = true, normal = { textColor = Color.white } };
            _smallStyle = new GUIStyle(_textStyle) { fontSize = Mathf.Max(11, Screen.width / 42) };
            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = Mathf.Max(12, Screen.width / 38), fontStyle = FontStyle.Bold };
        }

        void OnGUI()
        {
            EnsureStyles();
            if (_controller == null) { _controller = FindAnyObjectByType<TestProcedureController>(); return; }
            var margin = 16f;
            var width = Screen.width - margin * 2f;
            var oldColor = GUI.color;

            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.Box(new Rect(margin, margin, width, 118f), GUIContent.none);
            GUI.color = oldColor;
            GUI.Label(new Rect(margin + 12f, margin + 8f, width - 24f, 34f), "WL03 · Contextual / Procedural AR", _titleStyle);
            GUI.Label(new Rect(margin + 12f, margin + 45f, width - 24f, 60f), _controller.StatusMessage, _textStyle);

            var panelH = 205f;
            var panelY = Screen.height - panelH - margin;
            GUI.color = new Color(0.04f, 0.07f, 0.11f, 0.90f);
            GUI.Box(new Rect(margin, panelY, width, panelH), GUIContent.none);
            GUI.color = oldColor;

            var progressWidth = (width - 24f) * _controller.Progress;
            GUI.color = new Color(0.20f, 0.26f, 0.31f, 1f); GUI.Box(new Rect(margin + 12f, panelY + 12f, width - 24f, 8f), GUIContent.none);
            GUI.color = new Color(0.26f, 0.63f, 0.28f, 1f); GUI.Box(new Rect(margin + 12f, panelY + 12f, progressWidth, 8f), GUIContent.none); GUI.color = oldColor;

            var step = _controller.CurrentStep;
            var header = _controller.IsComplete ? "Procedimento finalizado" : step == null ? "Carregando..." : $"Etapa {step.id} de {_controller.StepCount} · Componente {step.component}";
            GUI.Label(new Rect(margin + 12f, panelY + 28f, width - 24f, 26f), header, _smallStyle);
            var title = _controller.IsComplete ? "Verificacao concluida" : step?.title ?? "";
            var instruction = _controller.IsComplete ? $"Tempo total: {_controller.ElapsedSeconds:F0} s. Use Reiniciar para repetir." : step?.instruction ?? "";
            GUI.Label(new Rect(margin + 12f, panelY + 52f, width - 24f, 28f), title, _titleStyle);
            GUI.Label(new Rect(margin + 12f, panelY + 82f, width - 24f, 46f), instruction, _textStyle);

            var y = panelY + 138f; var gap = 8f; var bw = (width - 24f - gap * 2f) / 3f;
            var previousEnabled = !_controller.IsComplete && _controller.ActiveStepIndex > 0;
            GUI.enabled = previousEnabled;
            if (GUI.Button(new Rect(margin + 12f, y, bw, 48f), "Anterior", _buttonStyle)) _controller.PreviousStep();
            GUI.enabled = _controller.TargetFound && !_controller.IsComplete;
            if (GUI.Button(new Rect(margin + 12f + bw + gap, y, bw, 48f), "Confirmar", _buttonStyle)) _controller.ConfirmCurrentStep();
            GUI.enabled = true;
            if (GUI.Button(new Rect(margin + 12f + (bw + gap) * 2f, y, bw, 48f), "Reiniciar", _buttonStyle)) _controller.ResetProcedure();
            GUI.Label(new Rect(margin + 12f, panelY + 188f, width - 24f, 20f), "Tambem e possivel tocar no componente destacado em RA.", _smallStyle);
        }

        public static bool IsTouchOverOverlay(Vector2 screenPosition)
        {
            var guiY = Screen.height - screenPosition.y;
            return guiY <= 145f || guiY >= Screen.height - 225f;
        }
    }
}
