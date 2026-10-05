# Atualizar Firawynix Dock no Center para 1.1.0

Estado consultado em 05/10/2026: o item `firawynix-dock` está publicado como `1.0.1`, usa Inno Setup, certificado `9A2EFF2483185C9A900F2797D7A9CBD5E8A12893` e link principal para o pacote x64. Siga o caso **A. Atualizar um produto que já existe** de `C:\Users\Hugo\firawynix-portfolio\docs\CENTER-PUBLICAR-PELO-PAINEL.md`.

## Pacotes assinados

| Linha do painel | Arquivo local | SHA-256 |
|---|---|---|
| Pacote x64 | `dist/center/Firawynix-Dock-Setup-x64.exe` | `0013D10431BC14FB22120E33FD933EF70065B8DCB2CD088548564E2C43B0EAD6` |
| Pacote x86 | `dist/center/Firawynix-Dock-Setup-x86.exe` | `5106AC1E55954A5745542664C38066EC4DB2A265F1D69A3FD248C1836219F033` |

Ambos têm versão interna `1.1.0`, versão do arquivo Windows `1.1.0.0`, assinatura Authenticode válida de `CN=Firawynix` e carimbo de tempo. Não há `.sig` de Tauri. O instalador único `dist/center/Firawynix-Dock-Setup.exe` também está pronto e assinado (`B400D5AF3A1768ED0B95CE3E47FAC05956CB737DC8DC1563682CD65ECD73B36D`), mas o link atual do catálogo entrega o **pacote x64**. Não é necessário trocar o link para publicar a atualização.

## No painel

1. Em **Firawynix Center**, abra **Editar** no projeto `Firawynix Dock` › **Instalador (Windows)** › **Publicar versão nova**.
2. Nas linhas **Pacote x64** e **Pacote x86**, escolha os dois arquivos acima, cole o SHA-256 correspondente e clique em **Enviar e conferir**. Aguarde a confirmação da assinatura em cada linha.
3. Informe **Nova versão** `1.1.0`, o código atual do **2FA** e clique em **Publicar**. O painel atualizará o tamanho do download e o SHA do catálogo. Não precisa clicar em Salvar só para essa publicação.
4. Como o executável instalado cresceu, ajuste **Tamanho instalado (bytes)** no formulário para cerca de `164435818` e salve. Esse valor considera o executável x64, o catálogo e o desinstalador; confira a barra de progresso em um PC de teste e ajuste se necessário.
5. Verifique instalação, abertura, pedido de fixação na barra de tarefas e desinstalação no Center. O Windows exige confirmação do usuário para fixar; o instalador abre o Dock ao concluir, e o Dock mostra a ação de fixação antes do painel.

Depois de publicar, confira `https://jogos.firawynix.com.br/api/games/firawynix-dock`: a versão deve ser `1.1.0`. Baixe o pacote x64 pelo link público e compare seu SHA-256 com a tabela. Se o painel recusar um arquivo, **não** assine ou altere o mesmo arquivo após calcular o hash; recompile, assine e recalcule o SHA antes de reenviar.
