using UnityEngine;

/// <summary>
/// Cores, tamanhos de texto, medidas e tempos da interface num lugar só (canvas de referência 1920x1080).
/// Valores da seção "Direção visual" do GDD.md. Para mudar o visual, mexa só aqui.
/// </summary>
public static class Theme
{
    // ------------------------------------------------------------ Paleta
    // Identidade: preto, neutro escuro, amarelo, verde e vermelho (só perigo e erro).
    public static readonly Color Background = Hex("14181D");
    public static readonly Color Surface = Hex("222A33");
    public static readonly Color Yellow = Hex("FFC72C");
    public static readonly Color Green = Hex("2FB36D");
    public static readonly Color Red = Hex("E5484D");
    public static readonly Color Text = Hex("F5F5F0");
    public static readonly Color TextMuted = Hex("A9B1BA");
    public static readonly Color OnYellow = Hex("14181D");   // texto sobre amarelo e sobre verde (nunca branco no verde)
    public static readonly Color CardFace = Hex("F5F5F0");

    // Neutros de apoio
    public static readonly Color SurfaceHigh = Hex("33404D"); // segmento vazio, desativado, carta virada
    public static readonly Color RedDeep = Hex("C0353A");     // fundo vermelho com texto claro dentro

    // Estados de botão
    public static readonly Color YellowHover = Hex("FFD95E");
    public static readonly Color YellowPressed = Hex("D9A600");
    public static readonly Color SurfaceHover = Hex("2D3742");
    public static readonly Color GreenHover = Hex("4CC985");
    public static readonly Color GreenPressed = Hex("249A5A");
    public static readonly Color TileBackHover = Hex("3D4B59");
    public static readonly Color DisabledText = Hex("9AA4AF");

    // Cores funcionais dos fios (exceção do GDD: a cor do fio faz parte da lição). Preto = Background, verde-amarelo = Green + Yellow
    public static readonly Color WireBlue = Hex("4A90E2");
    public static readonly Color WireBrown = Hex("8B5A2B");

    public static readonly Color Clear = new Color(0, 0, 0, 0);
    public static readonly Color TintBase = Color.white; // cor da Image dos botões; a cor real vem do ColorBlock

    // ------------------------------------------------------------ Tipografia (nada abaixo de 24)
    public const int FontTitle = 80;
    public const int FontHeading = 48;
    public const int FontSubheading = 40;
    public const int FontBody = 32;
    public const int FontButton = 36;
    public const int FontSmall = 28;
    public const int FontTag = 24;
    public const int FontMin = 24; // piso: se não couber, reduz até aqui

    // ------------------------------------------------------------ Canvas e faixas
    public static readonly Vector2 ReferenceResolution = new Vector2(1920, 1080);
    public const float CanvasMatch = 0;      // 0 = acompanha a largura: em 4:3 ou 16:10 sobra altura em vez de cortar a mesa
    public const float HudHeight = 120;
    public const float BottomHeight = 130;
    public const float BarPadding = 40;      // lateral, dentro do HUD e da faixa de baixo
    public const int BarPaddingY = 10;
    public const float StripeHeight = 6;     // linha amarela entre as faixas e a mesa
    public const float SideMargin = 0.08f;   // fração da largura, de cada lado da mesa
    public const float ContentMarginY = 16;

    // HUD
    public const float HudSideWidth = 610;       // blocos da esquerda e da direita iguais: Auditoria fica no centro
    public const float SoundButtonHeight = 64;   // botão "Som: ligado/mudo" no HUD
    public const float SoundButtonWidth = 180;
    public const float SoundToggleWidth = 320;   // tela inicial: Som e Música
    public const float SoundStepWidth = 80;      // tela inicial: botões - e +
    public const float SoundLabelWidth = 220;
    public const float AuditBlockWidth = 520;
    public const float HudLineSpacing = 4;
    public const float HealthSegmentWidth = 64;
    public const float HealthSegmentHeight = 28;
    public const float HealthSegmentSpacing = 8;
    public const float AuditBarWidth = 460;
    public const float AuditBarHeight = 28;
    public const int LowHealth = 2;          // até aqui o Índice fica vermelho

    // Faixa de baixo
    public const float AbilityButtonWidth = 420;
    public const float HintLineHeight = 36;  // linha de texto de altura fixa acima da mesa
    public const float MessageHeight = 90;   // até 2 linhas de FontSmall (3 se reduzir a 24)

    // ------------------------------------------------------------ Mesa
    public const float BoardMaxWidth = 1480;
    public const float BoardMaxHeight = 720;
    public const float CardMaxWidth = 280;
    public const float CardMaxHeight = 160;
    public static readonly Vector2 BoardSpacing = new Vector2(16, 16);
    public const float CardPadding = 16;
    public const float CardTagHeight = 30;

    // ------------------------------------------------------------ Botões e blocos
    public const float ButtonHeight = 80;
    public const float ButtonBigHeight = 100;
    public const float FooterButtonWidth = 420;
    public const float OptionHeight = 90;
    public const float CompactRowHeight = 64; // linhas da sequência: cabem 6 passos sem rolar
    public const float SmallButtonWidth = 180;
    public const float ButtonTextPadding = 20;
    public const float ButtonTextPaddingY = 6;
    public const int CornerRadius = 14;
    public const int BorderWidth = 3;
    public const float PressedScale = 0.97f;
    public const float ButtonFade = 0.08f;

    public const float ContentSpacing = 14;
    public const int ContentPadding = 20;
    public const float RowSpacing = 12;
    public const float ScrollSensitivity = 40;
    public const float SpaceS = 10;
    public const float SpaceM = 20;
    public const float SpaceL = 40;
    public static readonly Vector2 EpiCellSize = new Vector2(480, 90);
    public static readonly Vector2 EpiSpacing = new Vector2(20, 16);

    // Desafios práticos
    public const float WireAreaHeight = 280;     // bancada com os bornes
    public const float WireColumnWidth = 0.34f;  // fração da largura para cada coluna de bornes
    public const float WireRowGap = 16;
    public const float WireThickness = 6;
    public const float MachineHeight = 140;
    public const float HuntAreaWidth = 1300;     // cena da caça aos riscos (GDD: 1300 x 560; 500 para caber no 16:9)
    public const float HuntAreaHeight = 500;
    public const float HuntMinItemWidth = 160;   // objeto da cena nunca menor que isso, para o texto caber
    public const float HuntMinItemHeight = 72;   // 2 linhas de FontMin
    public const float HuntItemPadding = 10;
    public const float MatchAreaHeight = 480;    // colunas de situação e ação
    public const int CircleRadius = 64;          // resolução do sprite de círculo

    // Cenas desenhadas (Desafios práticos — visual)
    public const float IconBase = 96;            // ícones são desenhados num quadrado de 96 e reduzidos por escala
    public const float IconOutline = 2;
    public const int IconRingWidth = 10;         // espessura do anel no sprite de 128 (aro de ícone e de seleção)
    public static readonly Vector2 WireSceneSize = new Vector2(1300, 500);
    public static readonly Vector2 WireCardSize = new Vector2(180, 150);
    public const float WireCardGap = 10;
    public const int WireMaxEpiCards = 6;        // 3 por linha, 2 linhas, e o boneco embaixo
    public const float WireCardIcon = 48;
    public const float WireBodySize = 140;
    public const float WireBoxWidth = 300;       // caixa da fonte (rótulo do fio com 200 de largura)
    public const float WireDeviceWidth = 310;    // equipamento
    public const float WirePanelHeight = 420;
    public const float WireRowTop = 130;         // y do primeiro terminal (centro da cena = 0)
    public const float WireRowStep = 90;
    public const float WireRowHeight = 80;
    public const float WireTerminal = 40;
    public const float WireRing = 58;
    public const float WireDeviceIcon = 96;
    public const float WireLight = 24;
    public const float CableThickness = 10;
    public const float CableBend = 40;           // trecho reto na saída e na chegada do cabo
    public const float CableStripeEvery = 16;
    public const float MatchCardWidth = 520;
    public const float MatchCardMaxHeight = 96;
    public const float MatchRowGap = 8;          // com 6 linhas o cartão fica com 72 (2 linhas de texto em 24)
    public const float MatchGap = 120;           // espaço central para as linhas
    public const float MatchIconSize = 56;
    public const float MatchLineThickness = 8;
    public const float MatchPin = 18;
    public static readonly Vector2 HuntIconCard = new Vector2(200, 128); // objeto da caça com ícone (mínimo)
    public const float HuntIconSize = 56;
    public const float HuntRingPad = 20;
    public const float HuntAlertSize = 40;
    public const float HuntFadedAlpha = 0.4f;    // item seguro já conferido
    public const float HuntStripeHeight = 14;    // faixa zebrada na borda do piso
    public const float HuntStripeEvery = 28;
    public static readonly Vector2 LockSceneSize = new Vector2(560, 260);
    public static readonly Vector2 LockCardSize = new Vector2(200, 140);
    public const float LockCardIcon = 48;
    public const int LockCardsPerRow = 7;
    public const float LockMainSize = 180;       // objeto principal da cena (máquina, painel, escada, tanque)
    public static readonly Vector2 LockBreakerSize = new Vector2(70, 110);
    public static readonly Vector2 LockLeverSize = new Vector2(18, 34);
    public const float LockLeverTravel = 22;
    public const float LockPersonSize = 90;
    public const float LockFxIcon = 64;
    public const float LockLight = 28;
    public const float LockCheck = 44;
    public const float LockBadge = 100;
    public const float LockDropHeight = 150;     // de onde o cadeado cai
    public const float LockSwingAngle = 15;      // balanço da etiqueta
    public const float GlowWidth = 6;
    public const float SparkSize = 8;
    public const float SparkDistance = 24;
    public const float BigSparkSize = 14;
    public const float BigSparkDistance = 60;
    public const float DressRise = 40;           // quanto o EPI desce até encaixar no boneco
    public const float ShakeDistance = 10;
    public const float PulseScale = 1.08f;
    public const float FadedAlpha = 0.3f;        // cartão esmaecido pela Dica Técnica

    // ------------------------------------------------------------ Tempos (segundos)
    public const float FlipHalfTime = 0.12f;
    public const float RevealPause = 0.5f;
    public const float FlashTime = 1f;      // linha ou botão vermelho depois de um erro
    public const float ResultPause = 0.6f;  // pausa para ver o último acerto/erro antes do resultado
    public const float HintBlinkTime = 2f;  // Dica Técnica na caça aos riscos: quanto tempo o risco pisca
    public const float BlinkInterval = 0.25f;
    public const float GlowTime = 0.25f;    // brilho verde de acerto
    public const float GlowFade = 0.15f;
    public const float ShakeTime = 0.3f;    // tremida de erro
    public const float SparkTime = 0.3f;
    public const float CableTime = 0.25f;   // cabo/linha esticando
    public const float DressTime = 0.3f;    // EPI vestindo o boneco
    public const float PinPulseTime = 0.6f;
    public const float PinPulseScale = 1.3f;
    public const float HintLineTime = 1f;   // linha amarela da Dica Técnica antes de ficar verde
    public const float BlinkAllTime = 0.3f; // todas as linhas piscam juntas no fim do match
    public const float StepFxTime = 0.3f;   // reação da cena do lockout a cada passo
    public const float SlideTime = 0.4f;    // lupa passando, pessoa entrando
    public const float SwingTime = 0.6f;    // etiqueta balançando 2 vezes

    // ------------------------------------------------------------ Estilos de botão
    public static readonly ButtonStyle Primary = new ButtonStyle
    {
        normal = Yellow, hover = YellowHover, pressed = YellowPressed, disabled = SurfaceHigh,
        text = OnYellow, disabledText = DisabledText, border = Clear, disabledBorder = Clear
    };

    public static readonly ButtonStyle Secondary = new ButtonStyle
    {
        normal = Surface, hover = SurfaceHover, pressed = Background, disabled = SurfaceHigh,
        text = Text, disabledText = DisabledText, border = Yellow, disabledBorder = SurfaceHigh
    };

    /// <summary>Habilidade pronta (verde). Desativado = em recarga: apagado, com borda neutra.</summary>
    public static readonly ButtonStyle Ability = new ButtonStyle
    {
        normal = Green, hover = GreenHover, pressed = GreenPressed, disabled = Surface,
        text = OnYellow, disabledText = TextMuted, border = Clear, disabledBorder = SurfaceHigh
    };

    public static readonly ButtonStyle TileBack = new ButtonStyle
    {
        normal = SurfaceHigh, hover = TileBackHover, pressed = SurfaceHigh, disabled = SurfaceHigh,
        text = Yellow, disabledText = Yellow, border = Yellow, disabledBorder = Yellow, pressScale = false, clickSound = false
    };

    public static readonly ButtonStyle TileFace = new ButtonStyle
    {
        normal = CardFace, hover = CardFace, pressed = CardFace, disabled = CardFace,
        text = OnYellow, disabledText = OnYellow, border = Clear, disabledBorder = Clear, pressScale = false, clickSound = false
    };

    public static readonly ButtonStyle TileMatched = new ButtonStyle
    {
        normal = Green, hover = Green, pressed = Green, disabled = Green,
        text = OnYellow, disabledText = OnYellow, border = Clear, disabledBorder = Clear, pressScale = false, clickSound = false
    };

    /// <summary>Item feito certo e travado (verde, sem hover).</summary>
    public static readonly ButtonStyle Correct = new ButtonStyle
    {
        normal = Green, hover = Green, pressed = Green, disabled = Green,
        text = OnYellow, disabledText = OnYellow, border = Clear, disabledBorder = Clear, pressScale = false
    };

    /// <summary>Piscada de erro (texto claro dentro do vermelho: RedDeep).</summary>
    public static readonly ButtonStyle Wrong = new ButtonStyle
    {
        normal = RedDeep, hover = RedDeep, pressed = RedDeep, disabled = RedDeep,
        text = Text, disabledText = Text, border = Clear, disabledBorder = Clear, pressScale = false
    };

    /// <summary>Objeto da cena da caça aos riscos. Desativado = item seguro já conferido.</summary>
    public static readonly ButtonStyle HuntItem = new ButtonStyle
    {
        normal = SurfaceHigh, hover = TileBackHover, pressed = SurfaceHigh, disabled = Surface,
        text = Text, disabledText = TextMuted, border = Clear, disabledBorder = SurfaceHigh, pressScale = false
    };

    /// <summary>Risco encontrado: borda vermelha.</summary>
    public static readonly ButtonStyle HuntFound = new ButtonStyle
    {
        normal = SurfaceHigh, hover = SurfaceHigh, pressed = SurfaceHigh, disabled = SurfaceHigh,
        text = Text, disabledText = Text, border = Red, disabledBorder = Red, pressScale = false
    };

    /// <summary>Clicou num item seguro: borda amarela por um instante.</summary>
    public static readonly ButtonStyle HuntSafe = new ButtonStyle
    {
        normal = SurfaceHigh, hover = SurfaceHigh, pressed = SurfaceHigh, disabled = SurfaceHigh,
        text = Text, disabledText = Text, border = Yellow, disabledBorder = Yellow, pressScale = false
    };

    /// <summary>Cartão com ícone (EPI do wire): neutro, borda discreta.</summary>
    public static readonly ButtonStyle Card = new ButtonStyle
    {
        normal = Surface, hover = SurfaceHover, pressed = Background, disabled = Surface,
        text = Text, disabledText = TextMuted, border = SurfaceHigh, disabledBorder = SurfaceHigh
    };

    /// <summary>Cartão com ícone selecionado: borda amarela.</summary>
    public static readonly ButtonStyle CardSelected = new ButtonStyle
    {
        normal = Surface, hover = SurfaceHover, pressed = Background, disabled = Surface,
        text = Text, disabledText = TextMuted, border = Yellow, disabledBorder = Yellow
    };

    /// <summary>Área clicável transparente (terminais e bornes desenhados).</summary>
    public static readonly ButtonStyle Ghost = new ButtonStyle
    {
        normal = Clear, hover = SurfaceHover, pressed = Background, disabled = Clear,
        text = Text, disabledText = TextMuted, border = Clear, disabledBorder = Clear, pressScale = false
    };

    /// <summary>Objeto da caça com ícone: cartão translúcido sobre a cena.</summary>
    public static readonly ButtonStyle HuntCard = new ButtonStyle
    {
        normal = Alpha(Surface, 0.8f), hover = Alpha(SurfaceHover, 0.9f), pressed = Alpha(Background, 0.9f),
        disabled = Alpha(Surface, 0.8f), text = Text, disabledText = TextMuted, border = Clear, disabledBorder = Clear,
        pressScale = false
    };

    /// <summary>A mesma cor com outra transparência (não cria cor nova).</summary>
    public static Color Alpha(Color c, float a)
    {
        c.a = a;
        return c;
    }

    public static Color Hex(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString("#" + hex, out c);
        return c;
    }

    // ------------------------------------------------------------ Sprites gerados por código

    static Sprite rounded, frame, circle, circleFrame, iconRing, glowFrame, triangle;

    /// <summary>Anel grosso (aro de ícones, seleção e brilho redondo). Image.Type.Simple.</summary>
    public static Sprite IconRing
    {
        get
        {
            if (iconRing == null) iconRing = MakeRounded(CircleRadius, IconRingWidth);
            return iconRing;
        }
    }

    /// <summary>Borda arredondada grossa (GlowWidth) para o brilho de acerto. Image.Type.Sliced.</summary>
    public static Sprite GlowFrame
    {
        get
        {
            if (glowFrame == null) glowFrame = MakeRounded(CornerRadius, (int)GlowWidth);
            return glowFrame;
        }
    }

    /// <summary>Triângulo apontando para cima. Image.Type.Simple.</summary>
    public static Sprite Triangle
    {
        get
        {
            if (triangle == null) triangle = MakeTriangle(CircleRadius);
            return triangle;
        }
    }

    static Sprite MakeTriangle(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        var a = new Vector2(0, 0);
        var b = new Vector2(size, 0);
        var c = new Vector2(size / 2f, size);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float d = Mathf.Min(EdgeDistance(a, b, p), Mathf.Min(EdgeDistance(b, c, p), EdgeDistance(c, a, p)));
                tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(d + 0.5f)));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
    }

    /// <summary>Distância com sinal até a aresta p->q (positiva do lado de dentro, sentido anti-horário).</summary>
    static float EdgeDistance(Vector2 p, Vector2 q, Vector2 x)
    {
        Vector2 e = q - p;
        return (e.x * (x.y - p.y) - e.y * (x.x - p.x)) / e.magnitude;
    }

    /// <summary>Círculo cheio (usar com Image.Type.Simple).</summary>
    public static Sprite Circle
    {
        get
        {
            if (circle == null) circle = MakeRounded(CircleRadius, 0);
            return circle;
        }
    }

    /// <summary>Só o contorno do círculo (usar com Image.Type.Simple).</summary>
    public static Sprite CircleFrame
    {
        get
        {
            if (circleFrame == null) circleFrame = MakeRounded(CircleRadius, BorderWidth);
            return circleFrame;
        }
    }

    /// <summary>Quadrado de cantos arredondados para usar com Image.Type.Sliced.</summary>
    public static Sprite Rounded
    {
        get
        {
            if (rounded == null) rounded = MakeRounded(CornerRadius, 0);
            return rounded;
        }
    }

    /// <summary>Só a borda arredondada (centro transparente), com BorderWidth de espessura.</summary>
    public static Sprite RoundedFrame
    {
        get
        {
            if (frame == null) frame = MakeRounded(CornerRadius, BorderWidth);
            return frame;
        }
    }

    static Sprite MakeRounded(int radius, int border)
    {
        int size = radius * 2 + 2;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float cx = Mathf.Clamp(x + 0.5f, radius, size - radius);
                float cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                if (border > 0) alpha *= 1 - Mathf.Clamp01(radius - border - dist + 0.5f);
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0,
            SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }
}

/// <summary>Cores de um botão em cada estado (normal, hover, pressionado e desativado).</summary>
public class ButtonStyle
{
    public Color normal, hover, pressed, disabled;
    public Color text, disabledText;
    public Color border, disabledBorder; // Theme.Clear = sem borda
    public bool pressScale = true;
    public bool clickSound = true; // cartas da mesa não tocam clique (já tocam o som de virar)
}
