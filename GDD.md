# Turno Seguro — Documento de Design (GDD)

Jogo da memória cooperativo sobre as Normas Regulamentadoras (NRs), feito para a disciplina de Segurança do Trabalho. Entre 2 e 4 pessoas jogam na mesma tela, passando a vez entre si, como uma equipe de segurança de uma empresa.

## Objetivo

Na mesa ficam 20 cartas viradas para baixo, formando 10 pares. A equipe vence quando encontra **todos os pares**: cada par encontrado soma 1 na **Auditoria**. A equipe perde se o **Índice de Segurança** (vida compartilhada, começando em 5) chegar a zero, o que representa um acidente grave. Todos ganham ou todos perdem, então vale ajudar o colega a lembrar onde estão as cartas.

## Pares

- **Norma ↔ Tema:** por exemplo, NR-35 ↔ Trabalho em altura e NR-10 ↔ Segurança em eletricidade.
- **Risco ↔ EPI:** por exemplo, Ruído excessivo ↔ Protetor auricular e Queda de altura ↔ Cinturão tipo paraquedista.

A cada partida o jogo sorteia 5 pares de cada tipo de um banco maior. Ao achar um par, a tela mostra uma explicação curta.

## Turno

1. O jogador da vez pode usar sua habilidade, se estiver disponível.
2. Ele vira duas cartas.
3. **Se formarem par:** as cartas ficam abertas e a Auditoria sobe 1.
4. **Se não formarem par** (a memória falhou): a equipe encara um **desafio**, que é uma pergunta ou um puzzle. Acertando, não perde nada. Errando, perde 1 do Índice de Segurança. As cartas voltam a ficar viradas.
5. A vez passa para o próximo jogador.

Não há eventos aleatórios: a partida é só memória e desafios. (As cartas de evento continuam no `cards.json`, dormentes, e voltam se `eventEveryRounds` for maior que 0.)

## Papéis

Cada papel tem uma habilidade. Depois de usada, ela fica um turno do próprio jogador recarregando.

| Papel | Habilidade | Efeito |
|---|---|---|
| Técnico(a) de Segurança | Dica Técnica | No desafio: elimina 2 alternativas erradas, fixa o 1º passo de uma sequência ou revela 1 EPI correto |
| Membro da CIPA | Reunião da CIPA | No desafio: escudo, e se errar a equipe não perde Índice |
| Brigadista | Primeiros Socorros | +1 no Índice de Segurança, usado antes de virar as cartas |
| Engenheiro(a) de Segurança | Projeto de Engenharia | Descarta o desafio atual sem penalidade |

## Tipos de desafio

- **Pergunta:** múltipla escolha sobre uma NR.
- **Puzzle de Sequência:** ordenar os passos de um procedimento, como a desenergização da NR-10 ou a hierarquia de controle da NR-1.
- **Puzzle de EPI:** selecionar exatamente os EPIs necessários para uma atividade.

## Conteúdo inicial

São 18 pares no banco (10 de norma e 8 de EPI) e 37 desafios (17 perguntas, 4 sequências, 3 de EPI e 13 práticos: 3 wire, 4 lockout, 3 hunt e 3 match). Há também 6 cartas de evento, dormentes (os eventos estão desligados). Eles cobrem as NR-1, 5, 6, 7, 10, 12, 17, 23, 33 e 35. Antes de apresentar, o grupo deve conferir cada carta no texto vigente da norma (gov.br), porque as NRs passam por revisões.

## Balanceamento (ajustável no Inspector do GameManager)

- `maxHealth` = 5
- `pairsPerGame` = 10 (a mesa tem o dobro de cartas)
- `abilityRecharge` = 1
- `eventEveryRounds` = 0 (eventos aleatórios desligados por decisão da equipe; valor maior que 0 religa, a cada N rodadas)
- `previewSeconds` = 8 (prévia das cartas antes do turno 1; 0 desliga)
- `maxMistakes` = 2 (erros permitidos nos desafios práticos)
- Dano por desafio errado = 1. Um desafio pode definir um dano maior com o campo `damage`.

## Direção visual

Visão: painel de sinalização de obra. Fundo escuro, faixas amarelo e preto, verde para "seguro", vermelho só quando há perigo ou erro. Tudo em unidades do canvas 1920x1080 (referência). Valores prontos para copiar em `Theme.cs`. O que mudou em relação ao rascunho está na última tabela desta seção.

### 1. Paleta (5 cores de identidade + texto)

| Nome no Theme.cs | Hex | Uso | Contraste (WCAG) |
|---|---|---|---|
| `Background` (preto de sinalização) | `14181D` | Fundo da tela, texto escuro sobre amarelo e sobre verde | Texto `F5F5F0` sobre ele: 16:1 |
| `Surface` (fundo escuro neutro) | `222A33` | HUD, faixa de baixo, painéis, botão em recarga | Texto `F5F5F0`: 13:1; `A9B1BA`: 6,7:1 |
| `Yellow` (amarelo de sinalização) | `FFC72C` | Botão principal, faixas de alerta, borda da carta ativa, destaque do jogador da vez | Texto `14181D` sobre ele: 11:1 |
| `Green` (verde de segurança) | `2FB36D` | Índice cheio, Auditoria, par certo, habilidade pronta | Texto `14181D` sobre ele: 6,6:1. **Nunca texto branco sobre verde (2,7:1)** |
| `Red` (perigo/erro) | `E5484D` | Índice <= 2, resposta errada, dano, evento ruim | Sobre `Background`: 4,6:1 (ok para ícone, barra e texto grande) |
| `Text` | `F5F5F0` | Texto principal sobre fundo escuro | 16:1 |
| `TextMuted` | `A9B1BA` | Legendas, rótulos do HUD | 6,7:1 sobre `Surface` |
| `OnYellow` | `14181D` | Texto sobre amarelo e verde | 11:1 / 6,6:1 |
| `CardFace` | `F5F5F0` | Frente da carta (texto `14181D`) | 16:1 |

Neutros de apoio (não contam como cor de identidade): `SurfaceHigh` = `33404D` (segmento vazio, borda de botão desativado, carta virada para baixo com padrão), `RedDeep` = `C0353A` (fundo de botão ou faixa vermelha com texto branco: 5,6:1; usar no lugar de `Red` quando o texto estiver DENTRO do vermelho).

Regras de uso: amarelo é a ação que o jogador deve fazer agora; verde é bom; vermelho é só ruim. Nada decorativo em vermelho. Nunca usar cinza escuro sobre preto para texto lido.

### 2. Tipografia (canvas 1920x1080, nada abaixo de 24)

| Nome no Theme.cs | Tamanho | Uso |
|---|---|---|
| `FontTitle` | 80 | Nome do jogo, "Vitória" / "Derrota" |
| `FontHeading` | 48 | Título de painel (desafio, evento, par encontrado) |
| `FontSubheading` | 40 | Nome do jogador da vez no HUD, subtítulos |
| `FontBody` | 32 | Pergunta, alternativas, explicações |
| `FontButton` (novo) | 36 | Texto de botão (negrito) |
| `FontSmall` | 28 | Texto dentro das cartas, mensagens da faixa de baixo |
| `FontTag` | 24 | Legendas e rótulos do HUD ("ÍNDICE DE SEGURANÇA", "Rodada") |
| `FontMin` | 24 | Piso: se o texto não couber, reduzir até aqui e depois quebrar linha |

Negrito em título, botão e números do HUD. Texto do corpo alinhado à esquerda, no máximo 4 linhas por bloco.

### 3. HUD (topo) e faixa de baixo

Fundo do HUD: `Surface`. Linha amarela de 6 de altura na borda inferior do HUD (e na borda superior da faixa de baixo), `Yellow`.

| Item | Valor |
|---|---|
| `HudHeight` | 120 |
| `BottomHeight` | 130 |
| `BarPadding` (lateral) | 40 |
| Divisão do HUD | 3 blocos: esquerda Índice (largura 420), centro Auditoria (largura 520, centralizado), direita Jogador (alinhado à direita) |
| Índice de Segurança | rótulo `FontTag` `TextMuted` em cima; abaixo, 5 segmentos (`maxHealth`), cada um 64x28, espaço 8 (`HealthSegmentWidth` 64, `HealthSegmentHeight` 28, `HealthSegmentSpacing` 8) |
| Segmento cheio | `Green`; se Índice <= 2 (`LowHealth`): `Red` |
| Segmento vazio | `SurfaceHigh` |
| Ao perder 1 | o segmento pisca `Red` por 0,4 s antes de esvaziar (Dev decide a animação na etapa 2) |
| Auditoria | rótulo `FontTag` `TextMuted` em cima ("AUDITORIA"); abaixo, barra 460x28 (`AuditBarWidth` 460, `AuditBarHeight` 28), trilho `SurfaceHigh`, preenchimento `Green` proporcional a N/10; número "3/10" `FontSubheading` negrito `Text` à direita da barra |
| Jogador da vez | linha 1 `FontSubheading`: nome, cor `Yellow`; linha 2 `FontTag`: papel, cor `TextMuted`; abaixo do papel, "Rodada 4" `FontTag` `TextMuted` (ou na mesma linha do papel, separado por " · ") |
| Faixa de baixo (`BottomHeight` 130) | esquerda: botão da habilidade (largura 420, altura 80); direita da habilidade: mensagem/dica em `FontSmall` `Text`, até 2 linhas, alinhada à esquerda |

### 4. Mesa centralizada

A mesa fica no espaço entre HUD e faixa de baixo (1080 - 120 - 130 = 830 de altura), centralizada na horizontal e na vertical. Grade de 5 colunas x 4 linhas (20 cartas).

| Nome no Theme.cs | Valor |
|---|---|
| `BoardMaxWidth` | 1480 |
| `BoardMaxHeight` | 720 |
| `CardMaxWidth` | 280 |
| `CardMaxHeight` | 160 |
| `BoardSpacing` | (16, 16) |
| `SideMargin` | 0,08 (154 de cada lado, só se a tela for menor; na referência a mesa ocupa 1464 x 688) |
| `ContentMarginY` | 16 |
| `CornerRadius` | 14 |

Conta: 5 x 280 + 4 x 16 = 1464 de largura; 4 x 160 + 3 x 16 = 688 de altura, sobrando 71 acima e abaixo. Texto da carta: `FontSmall` (28), centralizado, cor `14181D`, margem interna 16, até 3 linhas (cabe "Gerenciamento de riscos (GRO/PGR)"); se não couber, reduz até 24. Carta virada para baixo: `SurfaceHigh` com borda `Yellow` de 3 e símbolo "?" em `Yellow`.

### 5. Botões

Altura 80 (`ButtonHeight`), grande 100 (`ButtonBigHeight`), largura de rodapé 420 (`FooterButtonWidth`), texto `FontButton` 36 negrito, margem interna do texto 20, cantos `CornerRadius` 14.

| Estado | Fundo | Texto | Observação |
|---|---|---|---|
| Normal (principal) | `FFC72C` | `14181D` | |
| Hover | `FFD95E` | `14181D` | 11,9:1 |
| Pressionado | `D9A600` | `14181D` | 8,2:1; reduzir escala para 0,97 |
| Desativado | `33404D` | `9AA4AF` | 3,9:1 (desativado pode ser mais fraco, mas ainda legível); sem hover |
| Secundário (normal) | `222A33` com borda `FFC72C` de 3 | `F5F5F0` | hover fundo `2D3742`; pressionado `14181D` |
| Habilidade pronta | `2FB36D` | `14181D` | hover `4CC985`; pressionado `249A5A` (texto escuro mantém 5,5:1) |
| Habilidade em recarga | `222A33` com borda `33404D` de 3 | `A9B1BA` | sem hover; rótulo "Recarregando (volta em 1 turno)" |
| Perigo (só para ação destrutiva, ex.: sair da partida) | `C0353A` | `F5F5F0` | hover `D4484D` |

### 6. Textos curtos do HUD e da faixa de baixo

| Onde | Texto |
|---|---|
| Rótulo do Índice | `ÍNDICE DE SEGURANÇA` |
| Rótulo da Auditoria | `AUDITORIA` e número `3/10` |
| Jogador da vez | `Vez de {nome}` e abaixo `{papel} · Rodada {n}` |
| Dica: início do turno | `Vire duas cartas e ache o par.` |
| Dica: depois da 1ª carta | `Agora vire a segunda carta.` |
| Dica: par certo | `Par certo! Auditoria +1.` |
| Dica: par errado | `Não formou par. Prepare-se para o desafio!` |
| Dica: desafio certo | `Acertou! O Índice está protegido.` |
| Dica: desafio errado | `Errou. O Índice caiu 1. Leia a explicação.` |
| Dica: Índice <= 2 (em vermelho `Red`) | `Cuidado: o Índice está baixo!` |
| Dica: vez trocando | `Passando a vez para {nome}.` |
| Botão da habilidade pronta | `Usar: {habilidade}` |
| Botão da habilidade em recarga | `{habilidade} · volta em {n} turno` |
| Habilidade já usada no turno | `{habilidade} · já usada` |

### O que mudou em relação ao rascunho de `Theme.cs`

| Item | Rascunho | Novo | Motivo |
|---|---|---|---|
| Cores | `14181D`, `222A33`, `FFC72C`, `2FB36D`, `E5484D`, `F5F5F0`, `A9B1BA` | iguais | Todos passam no contraste (mín. 4,6:1) |
| Novas cores | n/a | `SurfaceHigh` `33404D`, `RedDeep` `C0353A` | Segmento vazio e texto branco sobre vermelho (branco sobre `E5484D` dá só 3,9:1) |
| Regra do verde | n/a | texto sempre `14181D` sobre verde | Branco sobre `2FB36D` dá 2,7:1 |
| `FontTitle` | 72 | 80 | Mais presença no projetor |
| `FontHeading` | 44 | 48 | Hierarquia mais clara |
| `FontSubheading` | 36 | 40 | Nome do jogador legível ao fundo da sala |
| `FontBody` | 30 | 32 | Leitura da pergunta no projetor |
| `FontSmall` | 24 | 28 | Texto das cartas e dicas |
| `FontTag` | 20 | 24 | 20 fica abaixo do mínimo |
| `FontMin` | 20 | 24 | Idem |
| `FontButton` | n/a | 36 | Botão com tamanho próprio |
| `HudHeight` | 110 | 120 | Cabe rótulo + segmentos + papel/rodada |
| `BottomHeight` | 110 | 130 | Cabe botão de 80 com folga e 2 linhas de dica |
| `HealthSegmentHeight`, `AuditBarHeight` | 26 | 28 | Mais legível |
| `BoardMaxHeight` | 560 | 720 | 4 linhas de cartas de 160 não cabiam em 560 (o rascunho já forçava cartas menores que 140) |
| `CardMaxHeight` | 140 | 160 | Texto da carta a 28 em até 3 linhas |
| `BoardSpacing` | (16, 12) | (16, 16) | Espaçamento igual nos dois eixos |
| `ContentMarginY` | 12 | 16 | Respiro |
| `FooterHeight`, `ButtonHeight`, `ButtonBigHeight`, `FooterButtonWidth`, `CornerRadius`, `LowHealth` | 110, 80, 100, 420, 14, 2 | iguais | Sem motivo para mudar |

## Fase de memorização (prévia)

Antes do turno 1, as 20 cartas ficam viradas para cima para a equipe decorar. É regra nova; o Dev implementa.

| Item | Regra |
|---|---|
| `previewSeconds` | 8 (ajustável no Inspector do GameManager; 0 desliga a prévia) |
| O que o jogador vê | As 20 cartas abertas, o HUD normal, e no centro da faixa de baixo a contagem grande: `Memorize! 8` (número em `Yellow`, `FontHeading`, caindo de 1 em 1). Não há botão para pular |
| Fim da prévia | Quando o tempo zera, as cartas viram sozinhas, juntas (som de virar, uma vez só) e a dica passa para `Vire duas cartas e ache o par.` |
| Durante a prévia | Cartas não podem ser clicadas, habilidades ficam desativadas, o Índice e a Auditoria não mudam, não conta rodada |
| Pular a prévia | Não pode: todos têm os mesmos 8 s e a prévia sempre vai até o fim |
| Últimos 3 segundos | A contagem fica `Red`, com beep curto a cada segundo (ver Som) |
| Quando acontece | Uma vez por partida, ao começar (e de novo ao jogar outra partida) |

Balanceamento: com a prévia, achar pares fica bem mais fácil. Para não zerar a tensão, o plano é manter 8 s para 20 cartas (é pouco para decorar tudo) e deixar os desafios práticos mais frequentes. Conferir no playtest: se a equipe errar menos de 6 pares por partida, baixar `previewSeconds` para 6.

## Desafios práticos (mini-jogos)

Além de pergunta, sequência e EPI, o desafio de par errado também pode ser um **desafio prático**: uma mini-cena em que o jogador faz a tarefa. São 4 tipos novos, todos com Unity UI simples (botão, clique, formas e texto), sem arte externa.

### Regras comuns

- **Sorteio do desafio** (campo `challengeWeights`, ajustável): pergunta 35%, sequência 10%, EPI 10%, e os 4 práticos 10% cada (45% no total). Nunca o mesmo tipo duas vezes seguidas. Meta de duração: pergunta 20 s; prático 45 a 60 s. Uma partida de 10 a 15 min tem em média 4 práticos.
- **Erros:** cada tipo prático tem `maxMistakes` (padrão 2). Cada ação errada soma 1 erro, mostra na hora uma frase curta de por que está errado (`why`) e a cena continua. Ao chegar em `maxMistakes`, o desafio falha.
- **Resultado:** completar a tarefa com menos de `maxMistakes` erros é acerto (sem dano, mesmo com 1 erro, mas a explicação aparece). Falhar tira `damage` do Índice (padrão 1; a carta pode definir outro valor).
- **Sem relógio por padrão** (a equipe discute junto). Campo opcional `seconds` (0 = sem tempo) só no tipo `hunt`; ao acabar o tempo, é falha.
- **Habilidades dos papéis** (todos os tipos práticos):
  - **Dica Técnica:** ajuda específica de cada tipo (ver tabelas abaixo). Uma vez por desafio.
  - **Reunião da CIPA (escudo):** se o desafio falhar, não tira Índice. Os erros ainda aparecem na tela e a explicação é mostrada.
  - **Projeto de Engenharia:** descarta o desafio sem penalidade e sem explicação extra, igual aos desafios atuais.
  - **Primeiros Socorros:** continua sendo usada antes de virar as cartas, sem efeito no desafio.
- **Explicação ao final:** campo `explanation` (até 2 frases curtas), mostrado em `FontBody`, com o botão `Continuar`.
- **Campos comuns em todo JSON prático:** `id`, `type`, `nr`, `title` (até 4 palavras), `prompt` (1 frase), `explanation`, e opcionais `damage` e `maxMistakes`.
- **Ids:** `w01..` (wire), `h01..` (hunt), `m01..` (match), `l01..` (lockout).
- Tudo que depende de norma abaixo vem com `// conferir no gov.br` e entra na nota Playtest antes de apresentar. Os exemplos são só para definir o formato e **não estão no cards.json**; as cartas novas entram depois que o Dev implementar os tipos.

### Tipo 1: `wire` — Ligar e proteger (NR-10 e NR-6)

| Item | Regra |
|---|---|
| O que o jogador vê | Uma bancada simples: à esquerda 3 bornes de uma fonte (retângulos coloridos com texto), à direita 3 bornes de um equipamento. Acima, a lista de EPIs (6 botões de alternar, no mesmo estilo do desafio de EPI atual) e o botão `Pronto para mexer` |
| Passo 1: escolher o EPI | Marcar os EPIs certos e apertar `Pronto para mexer`. Se o conjunto estiver errado: +1 erro, aparece `why` geral ("falta ou sobra EPI") e a pessoa pode ajustar. Só depois de acertar o conjunto os bornes ficam ativos |
| Passo 2: ligar os fios | Clicar em um borne da esquerda e depois no borne certo da direita. Acerto: uma linha verde liga os dois. Erro: +1 erro e linha vermelha por 1 s, que some |
| Como acerta | EPIs certos e os 3 fios ligados com menos de `maxMistakes` erros |
| Como erra | `maxMistakes` erros somados (EPI + fios) |
| Dica Técnica | No passo 1: desativa 2 EPIs errados. No passo 2: liga sozinha 1 fio certo |
| Cenário da regra | O jogador aprende que o EPI vem ANTES de mexer, e que cada fio tem seu lugar. Sem texto sobre desenergizar na cena; a explicação lembra que o ideal é desenergizar antes (NR-10) |

Formato:

```json
{
  "id": "w01", "type": "wire", "nr": "NR-10",
  "title": "Trocar uma tomada",
  "prompt": "Antes de ligar os fios, escolha seus EPIs.",
  "epiItems": ["Luva isolante de borracha", "Óculos de proteção", "Calçado sem partes metálicas", "Protetor auricular", "Luva de vaqueta", "Colete salva-vidas"],
  "epiCorrect": [0, 1, 2],
  "left": ["Fase (preto)", "Neutro (azul)", "Terra (verde-amarelo)"],
  "right": ["Borne N", "Borne T", "Borne F"],
  "links": [2, 0, 1],
  "epiWhy": "Choque e fagulha: precisa de luva isolante, óculos e calçado sem metal.",
  "linkWhy": "Cada fio vai no borne do mesmo tipo: fase, neutro e terra.",
  "maxMistakes": 2,
  "explanation": "Primeiro o EPI, depois o fio. E o ideal é desenergizar o circuito antes de mexer."
}
```

`links` é uma lista simples do mesmo tamanho de `left`: a posição é o fio da esquerda e o valor é o borne da direita (`"links": [2, 0, 1]` quer dizer left[0]→right[2], left[1]→right[0], left[2]→right[1]; o JsonUtility da Unity não lê lista dentro de lista); `epiCorrect` são índices em `epiItems`; `epiWhy` e `linkWhy` são as frases mostradas a cada erro. `// conferir no gov.br` (cores dos fios e EPIs para baixa tensão; NR-10 e NR-6).

### Tipo 2: `hunt` — Caça aos riscos (NR-1, NR-12, NR-23, NR-17)

| Item | Regra |
|---|---|
| O que o jogador vê | Uma cena feita de formas (retângulos e círculos com texto curto dentro, por exemplo `Fio descascado`) numa área de 1300 x 500. Cada objeto tem no mínimo 160 x 72 (círculo: 160 de diâmetro), label de até 3 palavras (texto 28, encolhe até 24, em 2 linhas) e distância mínima entre centros de 15% na horizontal e 17% na vertical. De 8 a 10 objetos, de 3 a 5 deles são riscos. A dica da faixa de baixo diz `Ache 4 riscos: 0/4` |
| O que faz | Clica nos objetos que são riscos |
| Clique em risco | O objeto ganha borda `Red` e aparece sua frase `why`; o contador sobe |
| Clique em item seguro | +1 erro, borda amarela por 1 s e frase `why` ("isso está certo porque...") |
| Como acerta | Achar todos os riscos com menos de `maxMistakes` erros |
| Como erra | `maxMistakes` erros, ou o tempo (`seconds`, opcional) acabar |
| Dica Técnica | Faz 1 risco ainda não achado piscar em `Yellow` por 2 s |
| Limites de texto | `title` e `prompt` em 1 linha cada (`prompt` até uns 60 caracteres); com 2 linhas a tela passa da altura e rola. Label do objeto cabe em 2 linhas no botão de 182x80 (até ~16 caracteres por linha; evite 3 palavras longas) |

Formato (os objetos da cena ficam em `huntItems`; `x`, `y` são a posição do centro e `w`, `h` o tamanho, em % da área da cena, de 0 a 100; `shape` é `rect` ou `circle`):

```json
{
  "id": "h01", "type": "hunt", "nr": "NR-1",
  "title": "Oficina com riscos",
  "prompt": "Clique nos 4 riscos da oficina.",
  "huntItems": [
    { "label": "Fio descascado", "shape": "rect", "x": 18, "y": 22, "w": 14, "h": 16, "risk": true, "why": "Fio sem isolamento causa choque." },
    { "label": "Placa de saída", "shape": "rect", "x": 50, "y": 22, "w": 14, "h": 16, "risk": false, "why": "A sinalização de saída está correta." },
    { "label": "Extintor bloqueado", "shape": "rect", "x": 82, "y": 22, "w": 14, "h": 16, "risk": true, "why": "O extintor precisa estar livre e sinalizado." },
    { "label": "Fio na canaleta", "shape": "rect", "x": 18, "y": 50, "w": 14, "h": 16, "risk": false, "why": "Fio organizado na canaleta é o certo." },
    { "label": "Piso molhado", "shape": "rect", "x": 50, "y": 50, "w": 14, "h": 16, "risk": true, "why": "Piso molhado sem sinal causa queda." },
    { "label": "Capacete no gancho", "shape": "rect", "x": 82, "y": 50, "w": 14, "h": 16, "risk": false, "why": "Guardar o EPI no lugar é correto." },
    { "label": "Máquina sem proteção", "shape": "rect", "x": 34, "y": 78, "w": 14, "h": 16, "risk": true, "why": "Partes que se movem precisam de proteção." },
    { "label": "Extintor sinalizado", "shape": "rect", "x": 66, "y": 78, "w": 14, "h": 16, "risk": false, "why": "Extintor livre e sinalizado está certo." }
  ],
  "maxMistakes": 2,
  "seconds": 0,
  "explanation": "Enxergar o risco antes do acidente é o primeiro passo do gerenciamento de riscos (NR-1)."
}
```

`// conferir no gov.br` em cada `why` que cite norma.

### Tipo 3: `match` — Situação e ação certa (NR-23, NR-33, NR-12, NR-5)

| Item | Regra |
|---|---|
| O que o jogador vê | Coluna esquerda com 4 situações (cartões com texto de até 5 palavras, uma linha) e coluna direita com as 4 ações certas mais 1 ou 2 ações erradas (no máximo 6 itens), misturadas, também de até 5 palavras |
| O que faz | Clica numa situação e depois na ação que ela pede (ou arrasta, se o Dev preferir; clique é obrigatório) |
| Acerto | O par fica verde e travado, com a palavra `Certo` (sem símbolos especiais, que a fonte padrão pode não ter) |
| Erro | +1 erro, os dois cartões piscam `Red`, aparece o `why` da situação |
| Como acerta | Ligar as 4 situações com menos de `maxMistakes` erros |
| Como erra | `maxMistakes` erros |
| Dica Técnica | Liga sozinha 1 par certo |
| Limite de itens | No máximo 6 em cada coluna (4 pares + 2 extras); com 7 os botões ficam baixos demais para o texto |

Formato (`matchPairs` na ordem certa; o jogo embaralha as duas colunas; `extras` são ações erradas, 1 ou 2):

```json
{
  "id": "m01", "type": "match", "nr": "NR-23",
  "title": "O que fazer agora?",
  "prompt": "Ligue cada situação à ação certa.",
  "matchPairs": [
    { "s": "Fogo em painel elétrico", "a": "Extintor de CO2 ou pó", "why": "Água conduz eletricidade." },
    { "s": "Máquina travou", "a": "Desligar e bloquear antes", "why": "Energia residual machuca." },
    { "s": "Alarme de incêndio toca", "a": "Sair pela rota de fuga", "why": "Siga a sinalização de saída." },
    { "s": "Colega passou mal", "a": "Chamar a brigada ou socorro", "why": "Peça ajuda treinada." }
  ],
  "extras": ["Jogar água", "Continuar trabalhando"],
  "maxMistakes": 2,
  "explanation": "Agir certo na hora do perigo é o que a NR ensina em treinamento."
}
```

`// conferir no gov.br` (cada ação sobre incêndio, bloqueio e primeiros socorros).

### Tipo 4: `lockout` — Bloqueio e etiquetagem (NR-10 e NR-12)

| Item | Regra |
|---|---|
| O que o jogador vê | Uma máquina desenhada com 1 retângulo grande e, embaixo, de 5 a 7 botões de ação misturados (de 3 a 5 são passos certos e 1 ou 2 são armadilhas). Acima, a dica `Clique os passos na ordem certa` |
| O que faz | Clica nas ações na ordem em que fariam na prática |
| Passo certo na vez | O botão fica verde com o número `1`, `2`, `3`... e trava |
| Passo fora da ordem ou armadilha | +1 erro, o botão pisca `Red` e aparece o `why` (a armadilha explica por que é perigosa) |
| Como acerta | Clicar todos os passos certos na ordem com menos de `maxMistakes` erros |
| Como erra | `maxMistakes` erros |
| Dica Técnica | Destaca em amarelo o próximo passo certo |
| Diferença para a sequência | A sequência só reordena frases; aqui há botões armadilha que NÃO devem ser clicados, e o feedback é na hora, a cada clique |

Formato (`steps` na ordem certa, de 3 a 5; `traps` de 1 a 2):

```json
{
  "id": "l01", "type": "lockout", "nr": "NR-12",
  "title": "Bloqueio e etiqueta",
  "prompt": "Prepare a máquina para manutenção.",
  "steps": ["Avisar a equipe", "Desligar a máquina", "Bloquear com cadeado", "Colocar a etiqueta", "Testar se não parte"],
  "traps": [
    { "label": "Tirar o cadeado para testar", "why": "O bloqueio só sai no fim, por quem bloqueou." },
    { "label": "Só encostar a chave", "why": "Sem cadeado e etiqueta, alguém pode ligar." }
  ],
  "maxMistakes": 2,
  "explanation": "Avise, desligue, bloqueie, etiquete e teste, nessa ordem, antes de mexer na máquina."
}
```

`// conferir no gov.br` (ordem exata dos passos de bloqueio e etiquetagem na NR-10 e na NR-12; incluir a dissipação de energia residual se a norma exigir).

### Regras para o Dev (resumo)

| O que | Valor |
|---|---|
| Novos valores de `type` | `wire`, `hunt`, `match`, `lockout` |
| Campos novos em `Card` | `epiItems`, `epiCorrect`, `left`, `right`, `links`, `epiWhy`, `linkWhy`, `huntItems` (objetos da cena hunt; o campo `items` continua sendo só a lista de textos do desafio de EPI), `matchPairs` (cada item tem `s`, `a`, `why`; não confundir com a lista `pairs` das cartas de memória), `extras`, `steps`, `traps`, `maxMistakes`, `seconds` |
| Cores | Acerto `Green`, erro `Red`, dica da Dica Técnica `Yellow`, texto seguindo a tabela de botões em "Direção visual" |
| Texto mínimo | 24 (a regra de `FontMin`); objetos da cena `hunt` usam `FontSmall` 28 |
| Cartas | O designer cria as cartas novas no `cards.json` depois que o Dev avisar que os tipos estão prontos |

## Desafios práticos — visual

Objetivo: os 4 desafios práticos deixam de ser caixas com texto e viram pequenas cenas desenhadas. Tudo é feito por código com Unity UI: retângulos arredondados (`Theme.Rounded`), círculos, linhas (retângulo fino girado), listras, triângulos (sprite procedural) e o texto padrão. Sem arte externa e sem pacote. **A jogabilidade, as regras, o dano, as habilidades e o som não mudam**: só o que o jogador vê. Os sons já existentes (acerto pequeno, erro pequeno, etc.) continuam nos mesmos momentos.

### Regras visuais comuns

- **Cores:** só a paleta de "Direção visual" (`Background`, `Surface`, `SurfaceHigh`, `Yellow`, `Green`, `Red`, `Text`, `TextMuted`, `CardFace`). Verde = certo, vermelho = erro ou perigo, amarelo = selecionado ou dica. Exceção funcional: as cores dos fios do tipo `wire` (tabela abaixo), porque a cor é parte da lição. Todo objeto tem contorno de 2 em `Text` ou `Background` para se destacar do fundo. `// conferir no gov.br` (cores dos fios).
- **Texto:** continua sendo o de "Direção visual" (mínimo 24). O ícone ajuda, mas o nome do objeto sempre aparece escrito.
- **Animações curtas** (duração total até 0,4 s, sem bloquear o clique seguinte além disso): *acerto* = brilho verde (contorno `Green` de 6 que cresce de escala 0,9 para 1,1 e volta a 1,0 em 0,25 s) e, em alguns tipos, faíscas; *erro* = tremida horizontal de ±10 em 3 idas e vindas (0,3 s) com contorno `Red`. Dica Técnica = pulso `Yellow` (escala 1,0 a 1,08 a 1,0, duas vezes em 2 s).
- **Campos novos no JSON são opcionais.** Se um campo faltar ou o nome do ícone não estiver na lista, o jogo mostra só texto, como hoje. As cartas atuais continuam funcionando.

### Lista fixa de ícones

Cada ícone cabe num quadrado de 96 x 96 (o jogo reduz para 64 ou 48 quando precisa) e é desenhado com as formas abaixo. O nome é o valor usado nos campos `icon`, `epiIcons`, `stepIcons`, `sIcon`, `aIcon` e `extraIcons`.

| Nome | Desenho (formas) | Cores |
|---|---|---|
| `capacete` | Semicírculo, aba (retângulo fino) e faixa central | `Yellow`, contorno `Background` |
| `oculos` | Dois círculos só com contorno ligados por uma ponte e duas hastes | contorno `Text`, lentes `Text` a 25% |
| `luva` | Palma (retângulo arredondado), 4 dedos (retângulos) e o punho | `Yellow`, punho `SurfaceHigh` |
| `calcado` | Bota (retângulo arredondado com bico) e sola | `CardFace`, sola `Background` |
| `protetor_auricular` | Arco de cabeça (semicírculo contorno) e 2 círculos nas pontas | `Text`, círculos `Yellow` |
| `respirador` | Retângulo arredondado e 2 círculos de filtro | `CardFace`, filtros `Yellow` |
| `cinturao` | 2 faixas cruzadas em X (retângulos girados), argola (círculo contorno) no cruzamento | `Yellow`, argola `Text` |
| `mascara_solda` | Retângulo escuro com visor retangular | `SurfaceHigh`, visor `Yellow` |
| `colete` | 2 retângulos (peito) e 2 listras horizontais | `Yellow`, listras `Text` |
| `extintor` | Cilindro (retângulo arredondado), bocal (retângulo fino) e gatilho | `Red`, detalhes `Text` |
| `cadeado` | Corpo (retângulo arredondado) e arco (semicírculo contorno) | `Yellow`, arco `Text` |
| `etiqueta` | Retângulo com furo (círculo) no canto e um triângulo de alerta | `Yellow`, `Background` |
| `disjuntor` | Caixa (retângulo arredondado) e alavanca (retângulo); 2 estados: ligado (alavanca para cima, luz `Green`) e desligado (para baixo, luz `Red`) | `Surface`, contorno `Text` |
| `plugue` | Corpo (retângulo) e 2 pinos (retângulos finos) | `CardFace`, pinos `TextMuted` |
| `tomada` | Retângulo arredondado com 2 furos (retângulos escuros) | `CardFace`, furos `Background` |
| `fio` | Linha grossa (10) reta, curvada em 3 trechos | cor do fio (ver wire) |
| `alerta` | Triângulo com "!" (texto em negrito) | `Yellow`, "!" `Background` |
| `chama` | Gota de círculo com triângulo em cima, e outra menor dentro | `Red`, miolo `Yellow` |
| `gota` | Círculo com triângulo em cima (poça: elipse achatada embaixo) | `Text` a 60% |
| `escada` | 2 retângulos verticais e 4 degraus | `CardFace` |
| `maquina` | Corpo (retângulo), engrenagem (círculo com 6 retângulos em volta) e luz | `SurfaceHigh`, engrenagem `Yellow`, luz `Green` ou `Red` |
| `painel` | Caixa com 3 luzes e 2 botões | `Surface`, luzes `Green`/`Yellow`/`Red` |
| `placa_saida` | Retângulo com seta (triângulo + retângulo) | `Green`, seta `Text` |
| `cruz` | Cruz de 2 retângulos sobre círculo | cruz `Green`, círculo `CardFace` |
| `documento` | Folha (retângulo) com 3 linhas e um canto dobrado (triângulo) | `CardFace`, linhas `TextMuted` |
| `som` | Círculo central e 3 arcos de onda de cada lado | `Text`, ondas `Yellow` |
| `pessoa` | Cabeça (círculo) e corpo (retângulo arredondado) | cabeça `CardFace`, corpo `Yellow` |
| `tanque` | Cilindro vertical (retângulo arredondado), tampa (retângulo fino) e boca (círculo) | `SurfaceHigh`, boca `Background` |
| `gas` | Nuvem de 3 círculos sobrepostos | `TextMuted` a 70% |
| `multimetro` | Retângulo com display (retângulo) mostrando `0 V` e 2 pontas (linhas) | `Yellow`, display `Background`, texto `Green` |
| `cone` | Triângulo com base (retângulo) e faixa | `Yellow`, faixa `Background` |
| `botao_emergencia` | Círculo grande sobre base quadrada | círculo `Red`, base `Yellow` |
| `lupa` | Círculo contorno e cabo girado | `Text` |
| `ferramenta` | Cabo (retângulo) e cabeça (retângulo) em T | `TextMuted`, cabeça `CardFace` |

Total: 34 ícones. O `// conferir no gov.br` não se aplica aos desenhos, só às frases e cores de fio das cartas.

### Tipo `wire` — bancada elétrica e boneco com EPI

| Item | O que o jogador vê |
|---|---|
| Layout (1300 x 500 útil) | À esquerda, a **fonte**: caixa de luz (retângulo `Surface`) com 3 terminais redondos (círculos `CardFace` de 40 de diâmetro) nas cores dos fios. À direita, o **equipamento** (campo `device`): 3 bornes redondos rotulados com a letra (`F`, `N`, `T`). No centro, o espaço dos cabos. Embaixo à esquerda, o **boneco** (ícone `pessoa` em 140) |
| `device` | `tomada`, `bomba`, `luminaria` ou `painel`: o equipamento é um retângulo grande com o ícone dele (tomada/maquina/painel; luminária é um círculo amarelo com base) e os bornes na lateral esquerda |
| Passo 1 (EPI) | Os EPIs viram **cartões com ícone** (150 x 130, grade de 3 x 2 acima do boneco): ícone de 64 e nome em `FontTag` abaixo. Selecionado = contorno `Yellow` de 4 e canto com bolinha `Green` |
| Ao acertar o EPI | Cada EPI certo "veste" o boneco com uma animação de 0,3 s (desliza e encaixa): capacete na cabeça, óculos no rosto, luvas nas mãos (2 círculos), calçado nos pés, protetor nas orelhas, colete no corpo. Brilho `Green` no boneco. EPIs errados não vestem |
| Ao errar o EPI | Tremida no boneco e no cartão errado (contorno `Red`), mais o texto `epiWhy` como hoje |
| Passo 2 (fios) | Clicar num terminal da esquerda destaca o terminal (anel `Yellow`). Ao clicar no borne certo, o **cabo estica** da esquerda para a direita em 0,25 s (linha da cor do fio, espessura 10, 3 trechos), com **faísca**: 4 quadradinhos `Yellow` de 8 que voam 24 para fora do encaixe e somem em 0,3 s. O terminal e o borne ganham aro `Green` |
| Ao errar o fio | O cabo estica até o borne errado em `Red`, vibra (tremida) e se recolhe em 0,3 s; o borne errado pisca `Red`; faísca `Red` |
| Final certo | A luz do equipamento (círculo no canto) passa de `SurfaceHigh` para `Green` e o ícone dele "liga" (brilho). Final de falha: faísca `Red` grande e o equipamento fica apagado |
| Dica Técnica | No passo 1: os 2 cartões errados esmaecem a 30%. No passo 2: o cabo certo se liga sozinho com faísca amarela |

Cores dos fios (campo `wireColors`): `preto` = `14181D` com contorno `Text` de 2; `azul` = `4A90E2`; `marrom` = `8B5A2B` com contorno `Text` de 2; `verde_amarelo` = `Green` com listras `Yellow` (retângulos girados a cada 16). `// conferir no gov.br`.

Campos novos (opcionais):

| Campo | Tipo | Regra |
|---|---|---|
| `epiIcons` | lista de nomes de ícone | Mesmo tamanho de `epiItems` (ex.: `["luva", "oculos", "calcado", "protetor_auricular", "luva", "colete"]`) |
| `wireColors` | lista de `preto`, `azul`, `marrom`, `verde_amarelo` | Mesmo tamanho de `left` |
| `device` | `tomada`, `bomba`, `luminaria` ou `painel` | Qual equipamento aparece à direita |

### Tipo `hunt` — cena montada de formas

| Item | O que o jogador vê |
|---|---|
| Cena (campo `scene`) | `oficina`, `telhado` ou `obra`: o fundo da área 1300 x 500 é montado de formas. **Oficina:** parede `Surface`, 2 janelas (retângulos `Background` com contorno `Text`), piso `SurfaceHigh` nos 22% de baixo, com faixa de listras `Yellow` e `Background` na borda do piso. **Telhado:** céu `Background`, telhado (retângulo grande girado 12 graus em `SurfaceHigh`), chaminé e linha do horizonte. **Obra:** fundo `Background`, piso `SurfaceHigh` com listras, 3 vigas (retângulos `Surface`) e um andaime de linhas |
| Objetos (campo `icon` em cada item de `huntItems`) | Cada objeto é um cartão translúcido (`Surface` a 80%) com o **ícone** de 56 em cima e o nome em `FontTag` (24) embaixo, em até 2 linhas. Tamanho com ícone: no mínimo 200 x 128 (no JSON, `w` 16 e `h` 26, em %). Sem `icon`, continua a caixa de texto de hoje |
| Ao achar o risco | Círculo `Red` de 6 (anel) cresce em volta do objeto (0,25 s), um triângulo `alerta` surge acima com um "pop" (escala 0 a 1,2 a 1) e o objeto trava com contorno `Red`. O motivo aparece como hoje |
| Ao clicar item seguro | Tremida horizontal (±10, 0,3 s), contorno `Yellow` por 1 s, depois o cartão esmaece a 40% |
| Dica Técnica | O risco escolhido pulsa em `Yellow` por 2 s |
| Final | Se ganhou, todos os riscos achados viram aros `Green` por 0,4 s. Se perdeu, os que faltam ganham anel `Red` pulsando |

Campos novos: `scene` (lista acima) no card e `icon` em cada item de `huntItems`. Posições `x` e `y` continuam em % do centro; com ícone use `w` 16 e `h` 26 (200 x 130), centros a no mínimo 18% na horizontal e 28% na vertical (grade já usada: `x` 18, 50, 82 e 34, 66; `y` 22, 50, 78). O texto de `title` e `prompt` continua em 1 linha cada (até uns 60 caracteres).

### Tipo `lockout` — máquina, disjuntor, cadeado e etiqueta

| Item | O que o jogador vê |
|---|---|
| Cena (campo `scene`) | Acima, uma ilustração central de 560 x 260 com o objeto principal: `maquina` (ícone `maquina` em 200 com luz), `painel` (ícone `painel` e o `disjuntor` ao lado), `altura` (ícone `escada` + `pessoa` no alto + ponto de ancoragem: círculo `Yellow` na parede) ou `tanque` (ícone `tanque` em 200 e a `pessoa` do lado de fora como vigia) |
| Botões de passo | Em linha embaixo, 5 a 7 cartões de 180 x 120: ícone (campo `stepIcons`) de 56 e nome em `FontTag`. Armadilhas têm o mesmo visual, com ícone próprio (`icon` em cada item de `traps`) |
| Passo certo | O cartão ganha o número e trava em `Green`, e a **cena reage** conforme `stepFx` (ver tabela): a alavanca do `disjuntor` desce e a luz da máquina vira `Red` (apagada), o `cadeado` desce do alto e "fecha" na alavanca (0,3 s), a `etiqueta` cai pendurada e balança, o `multimetro` aparece com `0 V` em `Green`, etc. |
| Passo errado ou armadilha | Tremida do cartão e da cena (±10, 0,3 s), contorno `Red` e brilho `Red` na cena. Nada se move |
| Final certo | A luz da máquina fica `Red` (parada segura), com o cadeado e a etiqueta visíveis, e um check `Green` (círculo com a palavra `Seguro`) |
| Dica Técnica | O próximo cartão certo pulsa em `Yellow` |

Valores de `stepFx` (um por passo, mesma ordem de `steps`; valor desconhecido = só destaca o cartão):

| `stepFx` | Efeito na cena |
|---|---|
| `avisar` | Balão de fala (retângulo arredondado `CardFace` com `alerta`) sobe da `pessoa` |
| `desligar` | Alavanca do `disjuntor` desce, luz da máquina de `Green` para `Red`, engrenagem para de girar |
| `bloquear` | `cadeado` entra de cima e fecha na alavanca |
| `etiquetar` | `etiqueta` cai presa ao cadeado e balança 2 vezes |
| `medir` | `multimetro` aparece com `0 V` em `Green` |
| `aterrar` | Linha `verde_amarelo` liga a caixa ao chão com faísca `Yellow` |
| `testar` | Botão de partida (círculo `Green`) é apertado e a máquina continua parada: a luz continua `Red` e aparece um check `Green` |
| `documento` | `documento` surge ao lado da cena com um check `Green` |
| `inspecionar` | `lupa` passa sobre o equipamento (desliza 0,4 s) e fica um check `Green` |
| `ancorar` | Linha liga a `pessoa` ao ponto de ancoragem, com faísca `Yellow` |
| `isolar` | 2 `cone` aparecem nas pontas da entrada, com linha de fita `Yellow` entre eles |
| `entrar` | A `pessoa` de trabalho entra pelo `tanque` e a de fora (vigia) fica |

Campos novos: `scene` (`maquina`, `painel`, `altura`, `tanque`), `stepIcons` (mesmo tamanho de `steps`), `stepFx` (mesmo tamanho de `steps`) e `icon` em cada item de `traps`.

### Tipo `match` — ícones nos dois lados e linha de ligação

| Item | O que o jogador vê |
|---|---|
| Layout | Coluna esquerda com as situações e coluna direita com as ações, cada cartão (largura 520, altura 88 a 96) com **ícone de 56 à esquerda** e o texto à direita. No centro, 120 de espaço livre para as linhas |
| Seleção | Clicar na situação: contorno `Yellow` e pino redondo `Yellow` na borda direita do cartão, que pulsa |
| Ao acertar | A **linha** (retângulo fino de 8) cresce do pino da situação até o pino da ação em 0,25 s, em `Green`, com ponto de brilho que corre pela linha; os dois cartões travam em `Green` com a palavra `Certo` |
| Ao errar | A linha `Red` cresce até a metade do caminho, treme e se desfaz em 0,3 s; os dois cartões tremem com contorno `Red` |
| Final | Todas as linhas verdes piscam juntas uma vez (0,3 s) |
| Dica Técnica | O par certo ganha a linha automaticamente em `Yellow` por 1 s e fica verde |

Campos novos (opcionais): `sIcon` e `aIcon` em cada item de `matchPairs` e `extraIcons` (mesmo tamanho de `extras`). Textos continuam em até 5 palavras e no máximo 6 itens por coluna.

### Resumo dos campos novos para o Dev

| Tipo | Campos | Obrigatório |
|---|---|---|
| `wire` | `epiIcons`, `wireColors`, `device` | Não (sem eles, aparece o visual de hoje) |
| `hunt` | `scene`, `icon` em cada item de `huntItems` | Não |
| `lockout` | `scene`, `stepIcons`, `stepFx`, `icon` em cada item de `traps` | Não |
| `match` | `sIcon` e `aIcon` em cada item de `matchPairs`, `extraIcons` | Não |

As cartas atuais (w01 a w03, l01 a l04, h01 a h03, m01 a m03) ganham esses campos depois que o Dev avisar que o visual está pronto.

## Som

Todos os sons são gerados por código (`AudioClip` procedural, sem arquivo e sem pacote). Taxa de amostragem 44100 Hz, 1 canal. Cada som é soma de notas com envelope. Fórmulas para o Dev:

- Ondas: `sine` = sen(2πft); `triangle` e `square` (onda quadrada, amplitude pela metade para não ficar áspera) e `saw` (dente de serra).
- Envelope `A/D`: ataque linear de A ms, depois queda exponencial até quase zero (e^(-t/τ) com τ = D/5). Sempre 3 ms de fade-out no fim para não estalar.
- Volume de pico do clipe: no máximo 0,8 antes do volume geral.
- Nota = frequência fixa em Hz. Uma sequência de notas toca uma depois da outra, sem pausa, a menos que a tabela diga.

### Efeitos

| Som | Quando toca | Notas / frequência | Onda | Envelope | Duração |
|---|---|---|---|---|---|
| Virar carta | Ao virar uma carta | 900 Hz caindo para 500 Hz | sine | A 2 ms, D 70 ms | 80 ms |
| Par certo | Ao formar par | E5 659 Hz depois A5 880 Hz | triangle | A 5 ms, D 120 ms cada nota | 300 ms |
| Par errado | Ao virar duas cartas diferentes | 220 Hz depois 165 Hz | square | A 3 ms, D 110 ms cada | 260 ms |
| Desafio abre | Quando o painel de desafio aparece | Alerta: 660, 880, 660, 880 Hz, 80 ms cada | square (volume 0,3) | A 3 ms, D 70 ms | 320 ms |
| Acerto | Desafio concluído sem falha | Arpejo C5 523, E5 659, G5 784, C6 1047 (70 ms cada, última 200 ms) | sine mais triangle (metade do volume) | A 4 ms, D 90 ms (última D 200 ms) | 480 ms |
| Erro / perde Índice | Desafio falhou, ou evento ruim, ou dano | 120 Hz caindo para 60 Hz, mais ruído branco fraco (0,1) nos primeiros 60 ms | sine | A 2 ms, D 220 ms | 300 ms |
| Derrota | Índice chegou a 0 | A4 440, F4 349, D4 294, A3 220 (300 ms cada, última 600 ms) | square (volume 0,4) | A 5 ms, D 280 ms (última D 600 ms) | 1,5 s |
| Vitória | Todos os pares achados | C5 523, E5 659, G5 784 (120 ms cada), depois acorde C5+E5+G5 juntos por 600 ms e C6 1047 em cima | triangle (notas) e sine (acorde) | A 5 ms, D 100 ms; acorde A 10 ms, D 600 ms | 1,3 s |
| Botão | Clique em qualquer botão | 880 Hz | triangle | A 2 ms, D 40 ms | 50 ms |
| Passar o mouse | Hover em botão ativo | 1200 Hz | sine (volume 0,15) | A 2 ms, D 15 ms | 20 ms |
| Habilidade usada | Ao usar habilidade do papel | Desliza de 600 Hz para 1200 Hz | sine | A 5 ms, D 130 ms | 140 ms |
| Evento bom | Fiscalização, SIPAT, DDS etc. com efeito positivo | G5 784 e D6 1175 juntos | sine | A 5 ms, D 400 ms | 450 ms |
| Evento ruim | Evento com dano | 110 Hz caindo para 55 Hz (350 ms) e um beep de 220 Hz de 100 ms no início | sine mais square (volume 0,3) | A 3 ms, D 300 ms | 350 ms |
| Acerto pequeno | Passo ou clique certo num mini-jogo | 1568 Hz | sine | A 2 ms, D 40 ms | 50 ms |
| Erro pequeno | Passo ou clique errado num mini-jogo | 150 Hz | saw (volume 0,3) | A 3 ms, D 100 ms | 150 ms |
| Contagem da prévia | Últimos 3 segundos da memorização | 880 Hz | sine | A 2 ms, D 50 ms | 60 ms |

Variação: ao tocar, sortear ±2% na frequência de "Virar carta" e "Botão" para não soar repetido.

### Música de ambiente (opcional, ligada por padrão com volume baixo)

| Item | Valor |
|---|---|
| Duração e laço | 8 s, em laço contínuo (volta ao início sem pausa) |
| Andamento | 120 bpm, 4 compassos de 2 s cada |
| Acordes | Lá menor, Fá, Dó, Sol (um por compasso) |
| Baixo | Nota fundamental uma oitava abaixo (A2 110, F2 87, C3 131, G2 98 Hz), 4 notas por compasso, onda triangle, A 5 ms, D 350 ms, volume 0,10 |
| Arpejo | Notas do acorde (por exemplo Lá menor: A3 220, C4 262, E4 330), 8 por compasso, onda sine, A 5 ms, D 220 ms, volume 0,06 |
| Clima | Calma e constante, como música de suspense leve; sem melodia chamativa |
| Efeitos sobre a música | Durante desafios, o volume da música cai para 50%; na vitória e na derrota, a música para e o efeito toca |

### Controles de som

| Controle | Regra |
|---|---|
| Volume geral | De 0 a 1, em passos de 0,1, padrão 0,8; controle no canto da tela com texto de pelo menos 24 |
| Música | Liga/desliga, padrão ligada com volume 0,3 do volume geral |
| Mudo | Botão `Som: ligado` / `Som: mudo` no HUD (canto superior direito, botão de 64 de altura) e tecla `M`. O mudo vale para efeitos e música |
| Lembrar | O volume, a música e o mudo ficam salvos entre partidas (`PlayerPrefs`) |
| Ao abrir o jogo | Respeita o último ajuste; na primeira vez, som ligado |

## Ideias para depois do MVP

- Puzzle de "caça aos riscos": clicar nos riscos de uma imagem de ambiente de trabalho.
- Nomes personalizados e escolha de papel pelos jogadores.
- Sons e arte temática nas cartas (assets gratuitos do Kenney.nl, por exemplo).
- Níveis de dificuldade, com mais pares na mesa ou menos Índice.
