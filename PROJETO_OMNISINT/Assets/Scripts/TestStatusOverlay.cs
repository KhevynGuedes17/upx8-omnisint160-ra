using System.Text;
using UnityEngine;

namespace Facens.Test
{
    public sealed class TestStatusOverlay : MonoBehaviour
    {
        const float Margin = 16f;
        const float TopHeight = 140f;
        const float BottomHeight = 205f;

        readonly StringBuilder _anchorsLine = new();
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

        // Ex.: "*Motor 2/4 [vista]   Painel 0/3   Bomba 3/3 OK"  (* = em foco)
        string BuildAnchorsLine()
        {
            _anchorsLine.Clear();
            foreach (var s in _controller.Sessions)
            {
                if (_anchorsLine.Length > 0) _anchorsLine.Append("   ");
                if (s == _controller.Active) _anchorsLine.Append('*');
                _anchorsLine.Append(s.Title).Append(' ').Append(s.CompletedCount).Append('/').Append(s.StepCount);
                if (s.IsComplete) _anchorsLine.Append(" OK");
                else if (s.TargetFound) _anchorsLine.Append(" [vista]");
            }
            return _anchorsLine.ToString();
        }

        void OnGUI()
        {
            EnsureStyles();
            if (_controller == null) { _controller = FindAnyObjectByType<TestProcedureController>(); return; }
            var s = _controller.Active; // pode ser null enquanto nenhuma ancora foi reconhecida
            var width = Screen.width - Margin * 2f;
            var oldColor = GUI.color;

            // ---- Painel superior ----
            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.Box(new Rect(Margin, Margin, width, TopHeight), GUIContent.none);
            GUI.color = oldColor;
            GUI.Label(new Rect(Margin + 12f, Margin + 8f, width - 24f, 34f), "WL03 · Contextual / Procedural AR", _titleStyle);
            var status = s?.StatusMessage
                ?? (_controller.UnmatchedImage != null
                    ? $"Imagem '{_controller.UnmatchedImage}' reconhecida, mas sem procedimento no JSON (campo target)."
                    : "Aponte a camera para um alvo.");
            GUI.Label(new Rect(Margin + 12f, Margin + 45f, width - 24f, 60f), status, _textStyle);
            GUI.Label(new Rect(Margin + 12f, Margin + TopHeight - 28f, width - 24f, 24f), BuildAnchorsLine(), _smallStyle);

            // ---- Painel inferior ----
            var panelY = Screen.height - BottomHeight - Margin;
            GUI.color = new Color(0.04f, 0.07f, 0.11f, 0.90f);
            GUI.Box(new Rect(Margin, panelY, width, BottomHeight), GUIContent.none);
            GUI.color = oldColor;

            var progressWidth = (width - 24f) * (s?.Progress ?? 0f);
            GUI.color = new Color(0.20f, 0.26f, 0.31f, 1f); GUI.Box(new Rect(Margin + 12f, panelY + 12f, width - 24f, 8f), GUIContent.none);
            GUI.color = new Color(0.26f, 0.63f, 0.28f, 1f); GUI.Box(new Rect(Margin + 12f, panelY + 12f, progressWidth, 8f), GUIContent.none); GUI.color = oldColor;

            string header, title, instruction;
            if (s == null)
            {
                header = "Nenhum alvo reconhecido";
                title = "Aguardando alvo";
                instruction = "Aponte a camera para uma das imagens de referencia.";
            }
            else if (s.IsComplete)
            {
                header = $"{s.Title} · finalizado";
                title = "Verificacao concluida";
                instruction = $"Tempo total: {s.ElapsedSeconds:F0} s. Use Reiniciar para repetir.";
            }
            else
            {
                var step = s.CurrentStep;
                header = $"{s.Title} · Etapa {step.id} de {s.StepCount} · Componente {step.component}";
                title = step.title;
                instruction = step.instruction;
            }
            GUI.Label(new Rect(Margin + 12f, panelY + 28f, width - 24f, 26f), header, _smallStyle);
            GUI.Label(new Rect(Margin + 12f, panelY + 52f, width - 24f, 28f), title, _titleStyle);
            GUI.Label(new Rect(Margin + 12f, panelY + 82f, width - 24f, 46f), instruction, _textStyle);

            var y = panelY + 138f; var gap = 8f; var bw = (width - 24f - gap * 2f) / 3f;
            GUI.enabled = s != null && !s.IsComplete && s.ActiveStepIndex > 0;
            if (GUI.Button(new Rect(Margin + 12f, y, bw, 48f), "Anterior", _buttonStyle)) _controller.PreviousStep();
            GUI.enabled = s != null && s.TargetFound && !s.IsComplete;
            if (GUI.Button(new Rect(Margin + 12f + bw + gap, y, bw, 48f), "Confirmar", _buttonStyle)) _controller.ConfirmCurrentStep();
            GUI.enabled = s != null;
            if (GUI.Button(new Rect(Margin + 12f + (bw + gap) * 2f, y, bw, 48f), "Reiniciar", _buttonStyle)) _controller.ResetProcedure();
            GUI.enabled = true;
            GUI.Label(new Rect(Margin + 12f, panelY + 188f, width - 24f, 20f), "Tambem e possivel tocar no componente destacado em RA.", _smallStyle);
        }

        public static bool IsTouchOverOverlay(Vector2 screenPosition)
        {
            var guiY = Screen.height - screenPosition.y;
            return guiY <= Margin + TopHeight + 10f || guiY >= Screen.height - BottomHeight - Margin - 4f;
        }
    }
}
