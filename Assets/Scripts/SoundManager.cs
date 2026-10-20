using System.Collections.Generic;
using UnityEngine;

/// <summary>Efeitos sonoros da tabela "Som" do GDD.</summary>
public enum Sfx
{
    Flip, PairOk, PairBad, ChallengeOpen, Success, Fail, Defeat, Victory,
    Click, Hover, Ability, EventGood, EventBad, SmallOk, SmallBad, Countdown
}

/// <summary>
/// Toca os efeitos e a música, todos gerados por código (SoundBank). A GameUI cria este componente
/// sozinha, então não precisa montar nada na cena. Volume, música e mudo ficam salvos em PlayerPrefs;
/// a tecla M liga e desliga o som. Use SoundManager.Play(Sfx.X): se não houver som, nada acontece.
/// </summary>
public class SoundManager : MonoBehaviour
{
    const string KeyVolume = "som_volume";
    const string KeyMusic = "som_musica";
    const string KeyMute = "som_mudo";
    const float DefaultVolume = 0.8f;
    const float VolumeStep = 0.1f;
    const float MusicShare = 0.3f;  // música = 30% do volume geral
    const float DuckFactor = 0.5f;  // durante desafios a música cai para 50%
    const float PitchVariation = 0.02f;

    public static SoundManager Instance { get; private set; }

    /// <summary>Avisa a interface quando volume, música ou mudo mudam (para atualizar os botões).</summary>
    public static event System.Action Changed;

    public float Volume { get; private set; }
    public bool MusicOn { get; private set; }
    public bool Muted { get; private set; }

    AudioSource sfx, varied, music;
    readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();
    bool ducked;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        // Sem AudioListener na cena não sai som; cria um para garantir
        if (FindAnyObjectByType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();

        sfx = NewSource();
        varied = NewSource(); // efeitos com leve variação de tom (virar carta e botão)
        music = NewSource();
        music.loop = true;

        Volume = Mathf.Clamp01(PlayerPrefs.GetFloat(KeyVolume, DefaultVolume));
        MusicOn = PlayerPrefs.GetInt(KeyMusic, 1) == 1;
        Muted = PlayerPrefs.GetInt(KeyMute, 0) == 1;

        // Gera os clipes uma vez só
        foreach (Sfx s in System.Enum.GetValues(typeof(Sfx))) clips[s] = SoundBank.Make(s);
        music.clip = SoundBank.Music();
        Apply();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    AudioSource NewSource()
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0; // 2D
        return src;
    }

    void Update()
    {
        if (MutePressed()) ToggleMute();
    }

    static bool MutePressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.mKey.wasPressedThisFrame) return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.M)) return true;
#endif
        return false;
    }

    // ------------------------------------------------------------ Atalhos estáticos (seguros sem som)

    public static void Play(Sfx s)
    {
        if (Instance != null) Instance.PlaySfx(s);
    }

    public static void StartMusic()
    {
        if (Instance != null && Instance.music.clip != null && !Instance.music.isPlaying) Instance.music.Play();
    }

    public static void StopMusic()
    {
        if (Instance != null) Instance.music.Stop();
    }

    /// <summary>Música mais baixa durante desafios.</summary>
    public static void Duck(bool on)
    {
        if (Instance == null || Instance.ducked == on) return;
        Instance.ducked = on;
        Instance.Apply();
    }

    // ------------------------------------------------------------ Controles

    public void ToggleMute()
    {
        Muted = !Muted;
        SaveAndApply();
    }

    public void ToggleMusic()
    {
        MusicOn = !MusicOn;
        SaveAndApply();
    }

    /// <summary>dir = +1 ou -1: sobe ou desce um passo de 0,1.</summary>
    public void StepVolume(int dir)
    {
        Volume = Mathf.Clamp01(Mathf.Round((Volume + dir * VolumeStep) * 10) / 10);
        SaveAndApply();
    }

    void SaveAndApply()
    {
        PlayerPrefs.SetFloat(KeyVolume, Volume);
        PlayerPrefs.SetInt(KeyMusic, MusicOn ? 1 : 0);
        PlayerPrefs.SetInt(KeyMute, Muted ? 1 : 0);
        PlayerPrefs.Save();
        Apply();
        if (Changed != null) Changed();
    }

    void Apply()
    {
        float fx = Muted ? 0 : Volume;
        sfx.volume = fx;
        varied.volume = fx;
        music.volume = Muted || !MusicOn ? 0 : Volume * MusicShare * (ducked ? DuckFactor : 1);
    }

    void PlaySfx(Sfx s)
    {
        if (Muted || Volume <= 0) return;
        AudioClip clip;
        if (!clips.TryGetValue(s, out clip) || clip == null) return;

        if (s == Sfx.Flip || s == Sfx.Click)
        {
            varied.pitch = Random.Range(1 - PitchVariation, 1 + PitchVariation);
            varied.PlayOneShot(clip);
        }
        else
        {
            sfx.PlayOneShot(clip);
        }
    }
}

/// <summary>
/// Gera os AudioClips por código (sem arquivo): soma de notas com envelope de ataque linear
/// e queda exponencial (τ = D/5), pico máximo 0,8, como manda a seção "Som" do GDD.
/// </summary>
public static class SoundBank
{
    const int Rate = 44100;
    const float Peak = 0.8f;
    const float FadeOutMs = 3;

    enum Wave { Sine, Triangle, Square, Saw }

    /// <summary>Buffer de um clipe. wrap = true faz a cauda das notas voltar ao começo (laço sem emenda).</summary>
    class Synth
    {
        public readonly float[] data;
        readonly bool wrap;

        public Synth(float ms, bool wrap = false)
        {
            data = new float[Mathf.CeilToInt(ms / 1000f * Rate)];
            this.wrap = wrap;
        }

        /// <summary>Uma nota que começa em startMs e desliza de f0 para f1 Hz ao longo de durMs.</summary>
        public Synth Note(float startMs, float durMs, float f0, float f1, Wave wave, float vol, float attackMs, float decayMs)
        {
            int start = Samples(startMs);
            int len = Samples(durMs);
            int total = Mathf.Max(len, Samples(attackMs + decayMs)); // a cauda pode soar por cima da próxima nota
            float a = attackMs / 1000f;
            float tau = decayMs / 1000f / 5f;
            double phase = 0;

            for (int i = 0; i < total; i++)
            {
                int idx = start + i;
                if (idx >= data.Length)
                {
                    if (!wrap) break;
                    idx %= data.Length;
                }
                float t = i / (float)Rate;
                float f = Mathf.Lerp(f0, f1, Mathf.Clamp01(i / (float)len));
                phase += f / Rate;
                float env = t < a ? t / a : Mathf.Exp(-(t - a) / tau);
                data[idx] += Shape(wave, (float)(phase - System.Math.Floor(phase))) * env * vol * Peak;
            }
            return this;
        }

        /// <summary>Ruído branco que some linearmente ao longo de durMs.</summary>
        public Synth Noise(float startMs, float durMs, float vol)
        {
            var rnd = new System.Random(7);
            int start = Samples(startMs);
            int len = Samples(durMs);
            for (int i = 0; i < len && start + i < data.Length; i++)
                data[start + i] += ((float)rnd.NextDouble() * 2 - 1) * vol * Peak * (1 - i / (float)len);
            return this;
        }

        public AudioClip Build(string name, bool normalize = false)
        {
            float max = 0;
            foreach (float v in data) max = Mathf.Max(max, Mathf.Abs(v));
            if (max > 0 && (normalize || max > Peak))
                for (int i = 0; i < data.Length; i++) data[i] *= Peak / max;

            if (!wrap)
            {
                int fade = Mathf.Min(Samples(FadeOutMs), data.Length);
                for (int i = 0; i < fade; i++) data[data.Length - 1 - i] *= i / (float)fade;
            }

            var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }

    static int Samples(float ms)
    {
        return Mathf.CeilToInt(ms / 1000f * Rate);
    }

    static float Shape(Wave wave, float p)
    {
        switch (wave)
        {
            case Wave.Triangle: return 4 * Mathf.Abs(p - 0.5f) - 1;
            case Wave.Square: return (p < 0.5f ? 1 : -1) * 0.5f; // metade da amplitude para não ficar áspera
            case Wave.Saw: return 2 * p - 1;
            default: return Mathf.Sin(2 * Mathf.PI * p);
        }
    }

    /// <summary>Várias notas seguidas, cada uma com a duração dada (a última pode ter outra).</summary>
    static Synth Notes(Synth s, float[] freqs, float stepMs, float lastMs, Wave wave, float vol,
        float attackMs, float decayMs, float lastDecayMs, float startMs = 0)
    {
        for (int i = 0; i < freqs.Length; i++)
        {
            bool last = i == freqs.Length - 1;
            s.Note(startMs + i * stepMs, last ? lastMs : stepMs, freqs[i], freqs[i], wave, vol,
                attackMs, last ? lastDecayMs : decayMs);
        }
        return s;
    }

    public static AudioClip Make(Sfx s)
    {
        switch (s)
        {
            case Sfx.Flip:
                return new Synth(80).Note(0, 80, 900, 500, Wave.Sine, 1, 2, 70).Build("virar");
            case Sfx.PairOk:
                return Notes(new Synth(300), new float[] { 659, 880 }, 150, 150, Wave.Triangle, 1, 5, 120, 120).Build("par_certo");
            case Sfx.PairBad:
                return Notes(new Synth(260), new float[] { 220, 165 }, 130, 130, Wave.Square, 1, 3, 110, 110).Build("par_errado");
            case Sfx.ChallengeOpen:
                return Notes(new Synth(320), new float[] { 660, 880, 660, 880 }, 80, 80, Wave.Square, 0.3f, 3, 70, 70).Build("desafio");
            case Sfx.Success:
            {
                var arp = new float[] { 523, 659, 784, 1047 };
                var syn = Notes(new Synth(480), arp, 70, 270, Wave.Sine, 1, 4, 90, 200);
                return Notes(syn, arp, 70, 270, Wave.Triangle, 0.5f, 4, 90, 200).Build("acerto");
            }
            case Sfx.Fail:
                return new Synth(300).Note(0, 300, 120, 60, Wave.Sine, 1, 2, 220).Noise(0, 60, 0.1f).Build("erro");
            case Sfx.Defeat:
                return Notes(new Synth(1500), new float[] { 440, 349, 294, 220 }, 300, 600, Wave.Square, 0.4f, 5, 280, 600).Build("derrota");
            case Sfx.Victory:
            {
                var syn = Notes(new Synth(1300), new float[] { 523, 659, 784 }, 120, 120, Wave.Triangle, 1, 5, 100, 100);
                foreach (float f in new float[] { 523, 659, 784, 1047 })
                    syn.Note(360, 940, f, f, Wave.Sine, 0.5f, 10, 600);
                return syn.Build("vitoria");
            }
            case Sfx.Click:
                return new Synth(50).Note(0, 50, 880, 880, Wave.Triangle, 1, 2, 40).Build("botao");
            case Sfx.Hover:
                return new Synth(20).Note(0, 20, 1200, 1200, Wave.Sine, 0.15f, 2, 15).Build("hover");
            case Sfx.Ability:
                return new Synth(140).Note(0, 140, 600, 1200, Wave.Sine, 1, 5, 130).Build("habilidade");
            case Sfx.EventGood:
                return new Synth(450).Note(0, 450, 784, 784, Wave.Sine, 0.6f, 5, 400)
                    .Note(0, 450, 1175, 1175, Wave.Sine, 0.6f, 5, 400).Build("evento_bom");
            case Sfx.EventBad:
                return new Synth(350).Note(0, 350, 110, 55, Wave.Sine, 1, 3, 300)
                    .Note(0, 100, 220, 220, Wave.Square, 0.3f, 3, 100).Build("evento_ruim");
            case Sfx.SmallOk:
                return new Synth(50).Note(0, 50, 1568, 1568, Wave.Sine, 1, 2, 40).Build("passo_certo");
            case Sfx.SmallBad:
                return new Synth(150).Note(0, 150, 150, 150, Wave.Saw, 0.3f, 3, 100).Build("passo_errado");
            case Sfx.Countdown:
                return new Synth(60).Note(0, 60, 880, 880, Wave.Sine, 1, 2, 50).Build("contagem");
            default:
                return null;
        }
    }

    /// <summary>Música de 8 s em laço: Lá menor, Fá, Dó, Sol (2 s cada) com baixo e arpejo.</summary>
    public static AudioClip Music()
    {
        const float bar = 2000;
        float[] bass = { 110, 87, 131, 98 };
        float[][] chords =
        {
            new float[] { 220, 262, 330 }, // Lá menor
            new float[] { 175, 220, 262 }, // Fá
            new float[] { 262, 330, 392 }, // Dó
            new float[] { 196, 247, 294 }, // Sol
        };
        int[] pattern = { 0, 1, 2, 1, 0, 1, 2, 1 };

        var syn = new Synth(4 * bar, true);
        for (int b = 0; b < 4; b++)
        {
            for (int k = 0; k < 4; k++)
                syn.Note(b * bar + k * 500, 500, bass[b], bass[b], Wave.Triangle, 0.10f, 5, 350);
            for (int k = 0; k < 8; k++)
            {
                float f = chords[b][pattern[k]];
                syn.Note(b * bar + k * 250, 250, f, f, Wave.Sine, 0.06f, 5, 220);
            }
        }
        return syn.Build("musica", true);
    }
}
