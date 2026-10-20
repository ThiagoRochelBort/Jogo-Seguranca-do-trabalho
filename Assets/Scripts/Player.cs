public enum Role { TecnicoSeguranca, MembroCIPA, Brigadista, Engenheiro }

[System.Serializable]
public class Player
{
    public string name;
    public Role role;
    public int cooldown; // turnos do próprio jogador até a habilidade voltar

    public Player(string name, Role role)
    {
        this.name = name;
        this.role = role;
    }

    public bool AbilityReady { get { return cooldown == 0; } }

    public void Tick()
    {
        if (cooldown > 0) cooldown--;
    }
}

public static class RoleInfo
{
    public static string Name(Role r)
    {
        switch (r)
        {
            case Role.TecnicoSeguranca: return "Técnico(a) de Segurança";
            case Role.MembroCIPA: return "Membro da CIPA";
            case Role.Brigadista: return "Brigadista";
            default: return "Engenheiro(a) de Segurança";
        }
    }

    public static string AbilityName(Role r)
    {
        switch (r)
        {
            case Role.TecnicoSeguranca: return "Dica Técnica";
            case Role.MembroCIPA: return "Reunião da CIPA";
            case Role.Brigadista: return "Primeiros Socorros";
            default: return "Projeto de Engenharia";
        }
    }

    public static string AbilityDesc(Role r)
    {
        switch (r)
        {
            case Role.TecnicoSeguranca: return "no desafio, elimina 2 alternativas erradas, fixa o 1º passo de uma sequência ou revela 1 EPI correto.";
            case Role.MembroCIPA: return "no desafio, ativa um escudo: se a equipe errar, não perde Índice.";
            case Role.Brigadista: return "recupera 1 ponto do Índice de Segurança (use antes de virar as cartas).";
            default: return "descarta o desafio atual sem penalidade.";
        }
    }
}
