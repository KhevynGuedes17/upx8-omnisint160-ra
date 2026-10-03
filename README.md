# Manual Interativo em Realidade Aumentada para a OMNISINT 160

> **UPX — Realidade Aumentada | 2026.2**  
> Centro Universitário Facens — Sorocaba/SP

![Status](https://img.shields.io/badge/status-em%20desenvolvimento-yellow)
![Projeto](https://img.shields.io/badge/projeto-UPX%20RA-blue)
![Equipamento](https://img.shields.io/badge/equipamento-OMNISINT%20160-lightgrey)

---

## 1. Identificação do Projeto

| Item | Informação |
|---|---|
| **Projeto** | Manual Interativo em Realidade Aumentada para Apoio à Operação da Impressora 3D Metálica OMNISINT 160 |
| **Instituição** | Centro Universitário Facens |
| **Disciplina** | UPX — Realidade Aumentada |
| **Semestre** | 2026.2 |
| **Objeto de estudo** | Impressora 3D metálica OMNISINT 160 — FabLab Facens |
| **Status atual** | Planejamento, configuração do ambiente e desenvolvimento inicial |

### Integrantes

| Nome | RA | Responsabilidade inicial |
|---|---:|---|
| Edson Taveira | 236746 | Desenvolvimento e testes |
| Felipe Mastromauro | 210186 | Desenvolvimento e testes |
| Jeovanni Conservani | 190691 | Testes e documentação |
| Khevyn Henrique | 223761 | Gestão e documentação |
| Livia Stingelin | 235797 | Gestão e documentação |
| Lucas Lima | 223929 | Testes e documentação |
| Patrick Vieira | 234868 | Desenvolvimento e testes |

> As responsabilidades podem evoluir conforme a divisão registrada no Trello durante o semestre.

---

## 2. Visão Geral

O projeto propõe o desenvolvimento de um **manual interativo em Realidade Aumentada (RA)** para apoiar usuários já treinados durante os primeiros contatos posteriores com a impressora 3D metálica **OMNISINT 160**, disponível no FabLab Facens.

A aplicação deverá associar informações digitais a controles, indicadores e componentes selecionados da máquina. O objetivo é facilitar a **identificação de controles** e a **recuperação de etapas operacionais previamente apresentadas no treinamento presencial**.

A solução será **complementar ao treinamento e ao manual técnico**. Ela não substitui procedimentos oficiais, EPIs, sensores, dispositivos de segurança, decisões do operador ou acompanhamento técnico quando necessário.

---

## 3. Problema

A OMNISINT 160 possui diversos controles, sensores e procedimentos sequenciais. Mesmo após o treinamento presencial, o usuário pode precisar recuperar informações sobre a localização e a função de determinados controles ou sobre a ordem de uma sequência operacional.

Quando a consulta ocorre em um manual separado da máquina, o usuário precisa relacionar a informação consultada ao elemento físico correspondente. O projeto investiga se a RA pode oferecer uma forma mais contextual de realizar essa consulta.

---

## 4. Objetivos

### 4.1 Objetivo Geral

Desenvolver e avaliar um manual interativo em Realidade Aumentada para apoiar a identificação dos controles e a recuperação de etapas selecionadas de operação da impressora 3D metálica OMNISINT 160 após o treinamento inicial.

### 4.2 Objetivos Técnicos Iniciais

- estruturar um projeto móvel com suporte a Realidade Aumentada;
- reconhecer ou localizar referências visuais/espaciais relacionadas à OMNISINT 160 por técnica a ser validada no protótipo;
- apresentar nome, função e orientação contextual de controles selecionados;
- apresentar uma sequência operacional delimitada a partir do conteúdo oficial do equipamento;
- permitir navegação simples pelas informações exibidas;
- executar testes técnicos em dispositivo compatível;
- documentar configuração, execução, testes, limitações e evolução do projeto.

---

## 5. Público-Alvo

O público-alvo é composto por **alunos e usuários autorizados do FabLab Facens que já tenham recebido treinamento presencial para a OMNISINT 160**.

A aplicação é destinada a consultas pontuais diante do equipamento e não a treinamento autônomo de novos operadores.

---

## 6. Funcionalidades Planejadas

| ID | Funcionalidade | Descrição | Status |
|---|---|---|---|
| RF01 | Iniciar experiência RA | Permitir o acesso à experiência de RA e ativar os recursos necessários do dispositivo | ⬜ Planejado |
| RF02 | Reconhecer referência do equipamento | Identificar a referência visual ou espacial definida para o protótipo | ⬜ Planejado |
| RF03 | Identificar controle/componente | Exibir nome e identificação do elemento selecionado da OMNISINT 160 | ⬜ Planejado |
| RF04 | Consultar função | Exibir a função e informação técnica associada ao controle/componente reconhecido | ⬜ Planejado |
| RF05 | Consultar sequência operacional | Apresentar uma sequência delimitada de etapas previamente validada | ⬜ Planejado |
| RF06 | Navegar entre etapas | Permitir avançar e retornar entre as etapas apresentadas | ⬜ Planejado |
| RF07 | Exibir alertas/contexto de segurança | Apresentar avisos informativos quando o conteúdo exigir atenção especial | ⬜ Planejado |

**Legenda:** ✅ Implementado · 🚧 Em desenvolvimento · ⬜ Planejado · ❌ Cancelado

---

## 7. Requisitos Não Funcionais Iniciais

| ID | Categoria | Requisito |
|---|---|---|
| RNF01 | Usabilidade | A interface deverá permitir acesso simples às informações durante a consulta diante do equipamento |
| RNF02 | Segurança | A aplicação não deverá instruir o usuário a substituir procedimentos oficiais, treinamento, EPIs ou dispositivos de segurança |
| RNF03 | Desempenho | A experiência deverá responder sem travamentos que impeçam a consulta ao conteúdo |
| RNF04 | Rastreamento | O método escolhido deverá apresentar estabilidade suficiente no ambiente do FabLab para o escopo do protótipo |
| RNF05 | Compatibilidade | A compatibilidade será documentada somente para dispositivos efetivamente testados |
| RNF06 | Conteúdo | O conteúdo técnico deverá ser derivado da documentação oficial e validado antes da avaliação com participantes |
| RNF07 | Manutenibilidade | Código, assets e documentação deverão permanecer organizados e versionados no repositório |

---

## 8. Tecnologias Utilizadas

As tecnologias abaixo representam a **direção prevista** para o projeto. As versões exatas serão registradas após a criação do projeto Unity e instalação efetiva das dependências.

| Tecnologia | Versão | Utilização |
|---|---|---|
| Unity | Unity 6.6 (6000.6.4f1) | Desenvolvimento principal |
| C# | Compatível com a versão do Unity | Scripts da aplicação |
| AR Foundation | **A confirmar no projeto** | Camada de abstração para RA |
| Plugin XR de plataforma | **A confirmar** | Recursos de RA do dispositivo-alvo |
| Git | git version 2.51.0.windows.2 | Controle de versão |
| GitHub | — | Hospedagem do código e documentação |

> Não substituir “A confirmar” por versões estimadas. Registrar somente versões efetivamente utilizadas.

---

## 9. Hardware Utilizado

| Equipamento | Modelo | Utilização | Status |
|---|---|---|---|
| OMNISINT 160 | Impressora 3D metálica | Objeto físico/contexto de estudo | Disponível no FabLab Facens |
| Smartphone | A definir após teste | Execução da aplicação | Pendente |
| Computador de desenvolvimento | Equipamentos da equipe/laboratório | Desenvolvimento | A registrar |

---

## 10. Arquitetura Conceitual

```text
Usuário treinado
      ↓
Interface da aplicação móvel
      ↓
Controle da experiência / lógica da consulta
      ↓
Camada de Realidade Aumentada
      ↓
Câmera + rastreamento selecionado
      ↓
OMNISINT 160 / ambiente físico
      ↓
Conteúdo técnico validado
```

Detalhes adicionais: [docs/arquitetura.md](docs/arquitetura.md)

---

## 11. Funcionamento Previsto da Realidade Aumentada

A técnica definitiva ainda será validada pela equipe. Entre as possibilidades discutidas estão referências visuais, marcadores e reconhecimento de superfícies/imagens compatíveis com a tecnologia escolhida.

### Fluxo previsto

1. O usuário abre a aplicação.
2. A aplicação solicita/ativa o acesso à câmera.
3. O usuário inicia a experiência de RA.
4. O sistema procura a referência definida para o protótipo.
5. Ao reconhecer a referência, o sistema associa o conteúdo correspondente.
6. O usuário consulta nome, função e/ou sequência operacional delimitada.
7. O usuário encerra ou retorna à seleção de conteúdo.

---

## 12. Estrutura do Projeto

Após a criação do projeto Unity, a estrutura esperada será semelhante a:

```text
upx8-omnisint160-ra/
├── Assets/
├── Packages/
├── ProjectSettings/
├── docs/
│   ├── diagrams/
│   ├── documents/
│   └── images/
├── .github/
│   └── pull_request_template.md
├── .gitignore
├── CHANGELOG.md
├── CONTRIBUTING.md
└── README.md
```

---

## 13. Principais Scripts

Ainda não existem scripts consolidados nesta etapa. Esta seção deverá ser atualizada somente após a criação dos componentes reais da aplicação.

---

## 14. Dependências

```text
Unity: a confirmar
AR Foundation: a confirmar
Plugin XR: a confirmar
Demais pacotes: a confirmar
```

---

## 15. Configuração do Ambiente

### Pré-requisitos

- Git instalado;
- Unity Hub;
- versão do Unity registrada neste README após definição;
- módulos necessários para a plataforma-alvo;
- dispositivo móvel compatível com a tecnologia de RA selecionada.

### Procedimento inicial

1. Instalar o Git.
2. Instalar o Unity Hub.
3. Instalar a versão do Unity definida pela equipe.
4. Clonar este repositório.
5. Abrir a pasta do projeto no Unity Hub.
6. Aguardar a importação das dependências.
7. Confirmar as configurações de XR antes da primeira execução.

---

## 16. Como Executar

```bash
git clone https://github.com/KhevynGuedes17/upx8-omnisint160-ra.git
cd upx8-omnisint160-ra
```

Depois, abra o projeto no Unity Hub e siga a configuração registrada neste README após o ambiente real ser definido.

---

## 17. Testes Técnicos

Nenhum resultado técnico é declarado nesta versão inicial. Os testes serão registrados conforme forem realizados.

| Teste | Resultado obtido | Status |
|---|---|---|
| Inicialização | Não realizado | ⬜ |
| Câmera | Não realizado | ⬜ |
| Rastreamento | Não realizado | ⬜ |
| Conteúdo contextual | Não realizado | ⬜ |
| Navegação | Não realizado | ⬜ |

---

## 18. Avaliação e Segurança

A avaliação deverá se concentrar em tarefas seguras de identificação de controles e recuperação de sequência, sem operação autônoma de procedimentos de risco.

Detalhes: [docs/avaliacao-seguranca.md](docs/avaliacao-seguranca.md)

---

## 19. Repositórios e Recursos

| Recurso | Link |
|---|---|
| Código-fonte | https://github.com/KhevynGuedes17/upx8-omnisint160-ra |
| Trello | https://trello.com/b/sACk1ZN0/upx8-grupo-todah |
| Documentação complementar | Pasta compartilhada do grupo no Google Drive |
| Vídeo demonstrativo | A adicionar quando existir |

---

## 20. Cronograma Técnico

| Atividade | Responsável principal | Status |
|---|---|---|
| Organização do Git/GitHub | Khevyn + Jeovanni | 🚧 Em andamento |
| Preparação do ambiente de desenvolvimento | Equipe de desenvolvimento | 🚧 Em andamento |
| Desenvolvimento da base de RA | Equipe de desenvolvimento | ⬜ Planejado |
| Desenvolvimento das funcionalidades do MVP | Equipe de desenvolvimento | ⬜ Planejado |
| Interface e integração | Equipe de desenvolvimento | ⬜ Planejado |
| Testes técnicos e correções | Equipe | ⬜ Planejado |
| Preparação da avaliação do MVP | Equipe de documentação/testes | ⬜ Planejado |

---

## 21. Controle de Versão

### Branch principal
- `main`: versão integrada/estável.

### Padrão de branches
```text
feature/nome-da-funcionalidade
fix/nome-do-problema
docs/nome-da-alteracao
test/nome-do-teste
chore/nome-da-tarefa
```

### Padrão de commits
```text
feat: nova funcionalidade
fix: correção
docs: documentação
test: teste
chore: configuração/manutenção
refactor: reorganização
```

Evitar concentrar todo o desenvolvimento em um único commit.

---

## 22. Uso Acadêmico

Projeto acadêmico desenvolvido para fins educacionais na disciplina de **UPX — Realidade Aumentada** do Centro Universitário Facens.

Não publicar neste repositório credenciais, senhas, tokens, chaves privadas ou documentos restritos da instituição.

---

## Checklist de Entrega

- [ ] Código-fonte atualizado
- [ ] README completamente preenchido com informações reais
- [ ] Versões das tecnologias registradas
- [ ] Arquitetura atualizada conforme implementação
- [ ] Funcionamento da RA explicado conforme técnica real utilizada
- [ ] Procedimento de instalação testado
- [ ] Procedimento de execução testado
- [ ] Screenshots reais adicionados
- [ ] Testes registrados
- [ ] Dispositivos testados informados
- [ ] Evidência de funcionamento adicionada
- [ ] Commits representam a evolução do projeto

> Informações ainda não confirmadas são identificadas como pendentes e deverão ser atualizadas a partir da implementação real.
