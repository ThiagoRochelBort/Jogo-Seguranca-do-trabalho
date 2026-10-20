# Como abrir o projeto na Unity

A pasta `Jogo` já é o projeto Unity (Unity 6.3 LTS, 6000.3.25f1).

1. No Unity Hub, clique em **Add > Add project from disk** e escolha a pasta `Jogo`.
2. Abra a cena `Assets/Scenes/Jogo`.
3. Na aba Game, escolha a resolução **1920x1080** ou **16:9**.
4. Aperte **Play**.

A cena é montada automaticamente na primeira vez que o projeto abre. Se ela sumir ou quebrar, use o menu **Turno Seguro > Recriar cena**. A interface é gerada toda por código, então não é preciso montar Canvas nem botões. As regras, como vida, quantidade de pares e frequência dos eventos, podem ser ajustadas no Inspector do `GameManager` (objeto `Jogo`).

## Se algo der errado

- **Erro de Input** (InvalidOperationException envolvendo `UnityEngine.Input`): vá em Edit > Project Settings > Player > Active Input Handling e selecione **Both**.
- **Textos não aparecem:** confira se o pacote **Unity UI** (com.unity.ugui) está instalado em Window > Package Manager.
- **"Faltam pares ou desafios":** o arquivo precisa estar exatamente em `Assets/Resources/cards.json` e ter pelo menos 2 pares e 1 desafio.

## Adicionar cartas

Edite `Assets/Resources/cards.json`. O grupo pode fazer isso sem abrir a Unity.

Os **pares da memória** ficam na lista `"pairs"`. Use `"kind": "nr"` para norma ↔ tema e `"kind": "epi"` para risco ↔ EPI:

```json
{ "id": "p99", "kind": "nr", "a": "NR-18", "b": "Construção civil",
  "explanation": "Aparece quando a equipe encontra o par." }

{ "id": "p98", "kind": "epi", "a": "Calor intenso", "b": "Vestimenta aluminizada",
  "explanation": "..." }
```

Os **desafios** e **eventos** ficam na lista `"cards"`:

```json
{ "id": "q99", "type": "question", "nr": "NR-6",
  "prompt": "Pergunta?", "options": ["A", "B", "C", "D"], "correctIndex": 0,
  "explanation": "Por que a resposta é essa." }

{ "id": "s99", "type": "sequence", "nr": "NR-10", "title": "Título",
  "prompt": "Enunciado", "steps": ["passo 1", "passo 2", "passo 3"],
  "explanation": "..." }

{ "id": "e99", "type": "epi", "nr": "NR-6", "title": "Título",
  "prompt": "Enunciado", "items": ["EPI 1", "EPI 2", "EPI 3", "EPI 4"], "correctItems": [0, 2],
  "explanation": "..." }

{ "id": "ev99", "type": "event", "title": "Título", "prompt": "O que aconteceu",
  "healthDelta": -1, "auditDelta": 0 }
```

Algumas regras de preenchimento:

- Mantenha os textos dos pares curtos (até umas 4 palavras), porque eles precisam caber na carta.
- Nas sequências, escreva os passos já na **ordem correta**. O jogo embaralha sozinho.
- Os índices em `correctIndex` e `correctItems` começam em 0.
- Nos eventos, `auditDelta` positivo abre um par escondido.
- Depois de editar, valide o JSON (em jsonlint.com, por exemplo). Uma vírgula sobrando quebra o carregamento.

## Gerar o executável para a apresentação

Vá em File > Build Profiles (ou Build Settings), escolha **Windows** e clique em **Build**.
