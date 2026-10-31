using System.Text;
using UnityEngine;

namespace Facens.Test
{
    public sealed class TestStatusOverlay : MonoBehaviour
    {
        // Todas as medidas abaixo estao em "dp" (independentes de densidade, como no Android).
        const float MarginDp = 10f;
        const float PadDp = 14f;
        const float GapDp = 6f;
        const float MaxPanelWidthDp = 640f;   // limita a largura em paisagem/tablet
        const float ButtonHeightDp = 56f;     // alvo de toque confortavel (>= 48dp)
        const float ProgressHeightDp = 6f;

        const int TitleSp = 20, TextSp = 16, SmallSp = 13, ButtonSp = 16;

        readonly StringBuilder _anchorsLine = new();
        readonly GUIContent _content = new();
        TestProcedureController _controller;

        GUIStyle _titleStyle, _textStyle, _smallStyle, _buttonStyle, _primaryButtonStyle;
        Texture2D _btnNormal, _btnHover, _btnPrimary, _btnPrimaryHover;
        float _dp;
        int _lastW, _lastH;

        // Retangulos (coordenadas de GUI) usados para bloquear toques no AR
        static Rect s_topRect, s_bottomRect;

        void Start() => _controller = FindAnyObjectByType<TestProcedureController>();

        void OnDestroy()
        {
            Destroy(_btnNormal); Destroy(_btnHover); Destroy(_btnPrimary); Destroy(_btnPrimaryHover);
        }

        // ---------- Escala ----------
        float Dp(float v) => v * _dp;
        int Sp(int v) => Mathf.RoundToInt(v * _dp);

        static float ComputeDp()
        {
            var shortSide = Mathf.Min(Screen.width, Screen.height);
            var dp = Screen.dpi > 0f ? Screen.dpi / 160f : shortSide / 360f;
            return Mathf.Clamp(dp, 1f, 4.5f);
        }

        static Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        void EnsureStyles()
        {
            if (_titleStyle != null && _lastW == Screen.width && _lastH == Screen.height) return;
            _lastW = Screen.width; _lastH = Screen.height;
            _dp = ComputeDp();

            if (_btnNormal == null)
            {
                _btnNormal = MakeTex(new Color(0.22f, 0.30f, 0.38f, 1f));
                _btnHover = MakeTex(new Color(0.30f, 0.40f, 0.50f, 1f));
                _btnPrimary = MakeTex(new Color(0.20f, 0.60f, 0.25f, 1f));
                _btnPrimaryHover = MakeTex(new Color(0.28f, 0.72f, 0.33f, 1f));
            }

            _titleStyle = new GUIStyle(GUI.skin.label)
            { fontSize = Sp(TitleSp), fontStyle = FontStyle.Bold, wordWrap = true, normal = { textColor = Color.white } };
            _textStyle = new GUIStyle(GUI.skin.label)
            { fontSize = Sp(TextSp), wordWrap = true, normal = { textColor = Color.white } };
            _smallStyle = new GUIStyle(_textStyle)
            { fontSize = Sp(SmallSp), normal = { textColor = new Color(0.80f, 0.86f, 0.92f, 1f) } };

            _buttonStyle = MakeButton(_btnNormal, _btnHover);
            _primaryButtonStyle = MakeButton(_btnPrimary, _btnPrimaryHover);
        }

        GUIStyle MakeButton(Texture2D normal, Texture2D hover)
        {
            var st = new GUIStyle(GUI.skin.button)
            {
                fontSize = Sp(ButtonSp),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            st.normal.background = normal;   st.normal.textColor = Color.white;
            st.hover.background = hover;     st.hover.textColor = Color.white;
            st.active.background = hover;    st.active.textColor = Color.white;
            st.focused.background = normal;  st.focused.textColor = Color.white;
            st.padding = new RectOffset((int)Dp(6), (int)Dp(6), (int)Dp(4), (int)Dp(4));
            return st;
        }

        // ---------- Helpers de desenho ----------
        static Rect SafeRect()
        {
            var sa = Screen.safeArea; // origem embaixo-esquerda (pixels)
            return Rect.MinMaxRect(sa.xMin, Screen.height - sa.yMax, sa.xMax, Screen.height - sa.yMin);
        }

        static void Fill(Rect r, Color c)
        {
            var old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = old;
        }

        float TextHeight(GUIStyle st, string text, float width)
        {
            _content.text = text ?? string.Empty;
            return st.CalcHeight(_content, width);
        }

        // Texto com sombra para manter contraste sobre a imagem da camera
        void Label(Rect r, string text, GUIStyle st)
        {
            var original = st.normal.textColor;
            var o = Mathf.Max(1f, Dp(1f));
            st.normal.textColor = new Color(0f, 0f, 0f, 0.85f);
            GUI.Label(new Rect(r.x + o, r.y + o, r.width, r.height), text, st);
            st.normal.textColor = original;
            GUI.Label(r, text, st);
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

        // ---------- GUI ----------
        void OnGUI()
        {
            EnsureStyles();
            if (_controller == null) { _controller = FindAnyObjectByType<TestProcedureController>(); return; }

            var s = _controller.Active; // pode ser null enquanto nenhuma ancora foi reconhecida

            var safe = SafeRect();
            var margin = Dp(MarginDp);
            var pad = Dp(PadDp);
            var gap = Dp(GapDp);
            var panelW = Mathf.Min(safe.width - margin * 2f, Dp(MaxPanelWidthDp));
            var panelX = safe.x + (safe.width - panelW) * 0.5f;
            var innerX = panelX + pad;
            var innerW = panelW - pad * 2f;

            // ===== Painel superior =====
            const string appTitle = "WL03 · Contextual / Procedural AR";
            var status = s?.StatusMessage
                ?? (_controller.UnmatchedImage != null
                    ? $"Imagem '{_controller.UnmatchedImage}' reconhecida, mas sem procedimento no JSON (campo target)."
                    : "Aponte a camera para um alvo.");
            var anchors = BuildAnchorsLine();

            var hTitle = TextHeight(_titleStyle, appTitle, innerW);
            var hStatus = TextHeight(_textStyle, status, innerW);
            var hAnchors = TextHeight(_smallStyle, anchors, innerW);
            var topH = pad + hTitle + gap + hStatus + gap + hAnchors + pad;

            s_topRect = new Rect(panelX, safe.y + margin, panelW, topH);
            Fill(s_topRect, new Color(0f, 0f, 0f, 0.80f));
            var y = s_topRect.y + pad;
            Label(new Rect(innerX, y, innerW, hTitle), appTitle, _titleStyle); y += hTitle + gap;
            Label(new Rect(innerX, y, innerW, hStatus), status, _textStyle); y += hStatus + gap;
            Label(new Rect(innerX, y, innerW, hAnchors), anchors, _smallStyle);

            // ===== Painel inferior =====
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
            const string hint = "Tambem e possivel tocar no componente destacado em RA.";

            var barH = Dp(ProgressHeightDp);
            var btnH = Dp(ButtonHeightDp);
            var hHeader = TextHeight(_smallStyle, header, innerW);
            var hStepTitle = TextHeight(_titleStyle, title, innerW);
            var hInstr = TextHeight(_textStyle, instruction, innerW);
            var hHint = TextHeight(_smallStyle, hint, innerW);
            var bottomH = pad + barH + gap + hHeader + hStepTitle + gap + hInstr + gap + btnH + gap + hHint + pad;

            s_bottomRect = new Rect(panelX, safe.yMax - margin - bottomH, panelW, bottomH);
            Fill(s_bottomRect, new Color(0.04f, 0.07f, 0.11f, 0.94f));

            y = s_bottomRect.y + pad;
            var progress = Mathf.Clamp01(s?.Progress ?? 0f);
            Fill(new Rect(innerX, y, innerW, barH), new Color(0.20f, 0.26f, 0.31f, 1f));
            Fill(new Rect(innerX, y, innerW * progress, barH), new Color(0.30f, 0.75f, 0.33f, 1f));
            y += barH + gap;

            Label(new Rect(innerX, y, innerW, hHeader), header, _smallStyle); y += hHeader;
            Label(new Rect(innerX, y, innerW, hStepTitle), title, _titleStyle); y += hStepTitle + gap;
            Label(new Rect(innerX, y, innerW, hInstr), instruction, _textStyle); y += hInstr + gap;

            var bw = (innerW - gap * 2f) / 3f;
            GUI.enabled = s != null && !s.IsComplete && s.ActiveStepIndex > 0;
            if (GUI.Button(new Rect(innerX, y, bw, btnH), "Anterior", _buttonStyle)) _controller.PreviousStep();
            GUI.enabled = s != null && s.TargetFound && !s.IsComplete;
            if (GUI.Button(new Rect(innerX + bw + gap, y, bw, btnH), "Confirmar", _primaryButtonStyle)) _controller.ConfirmCurrentStep();
            GUI.enabled = s != null;
            if (GUI.Button(new Rect(innerX + (bw + gap) * 2f, y, bw, btnH), "Reiniciar", _buttonStyle)) _controller.ResetProcedure();
            GUI.enabled = true;
            y += btnH + gap;

            Label(new Rect(innerX, y, innerW, hHint), hint, _smallStyle);
        }

        /// <summary>Retorna true se o toque (em pixels de tela) cai sobre algum painel do overlay.</summary>
        public static bool IsTouchOverOverlay(Vector2 screenPosition)
        {
            var p = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
            const float slack = 6f;
            var top = s_topRect; top.xMin -= slack; top.xMax += slack; top.yMin -= slack; top.yMax += slack;
            var bottom = s_bottomRect; bottom.xMin -= slack; bottom.xMax += slack; bottom.yMin -= slack; bottom.yMax += slack;
            return top.Contains(p) || bottom.Contains(p);
        }
    }
}