# Guia de contribuição

Este documento define o fluxo básico de Git/GitHub do projeto UPX — OMNISINT 160.

## Branch principal

- `main`: versão integrada e estável do projeto.

Não desenvolver diretamente na `main`, exceto correções administrativas simples previamente combinadas.

## Padrão de branches

```text
feature/nome-da-funcionalidade
fix/nome-do-problema
docs/nome-da-alteracao
test/nome-do-teste
chore/nome-da-tarefa
```

Exemplos:

```text
feature/image-tracking
feature/interface-inicial
docs/atualiza-readme
fix/corrige-posicionamento-ra
test/rastreamento-dispositivo
```

## Padrão de commits

```text
feat: nova funcionalidade
fix: correção
docs: documentação
test: teste
chore: configuração ou manutenção
refactor: reorganização sem alterar comportamento
```

Exemplos:

```text
feat: adiciona reconhecimento da referencia da OMNISINT
docs: atualiza instrucoes de execucao
fix: corrige posicionamento do conteudo virtual
test: registra teste de rastreamento
chore: configura dependencias iniciais
```

## Fluxo de trabalho

1. Atualizar a `main`.
2. Criar uma branch para a tarefa.
3. Trabalhar em alterações pequenas e coerentes.
4. Fazer commits descritivos ao longo do desenvolvimento.
5. Enviar a branch para o GitHub.
6. Abrir Pull Request para `main`.
7. Revisar as alterações antes do merge.
8. Atualizar Trello e documentação relacionada.

## Regras importantes

- Cada integrante deve usar sua própria conta GitHub.
- Evitar um único commit concentrando várias semanas de trabalho.
- Não publicar senhas, tokens, chaves privadas ou arquivos restritos.
- Não declarar testes ou funcionalidades como concluídos sem evidência real.
- Antes de um merge relevante, confirmar que o projeto abre e não foi comprometido pela alteração.
