# Atualizar Firawynix Dock no Center para 1.1.1

Siga o caso **A. Atualizar um produto que já existe** em `C:\Users\Hugo\firawynix-portfolio\docs\CENTER-PUBLICAR-PELO-PAINEL.md`. O item atual é `firawynix-dock`; a versão de catálogo consultada em 05/10/2026 era `1.0.1`. A versão `1.1.1` corrige a fixação na barra de tarefas e permite ocultar sites e programas não instalados separadamente.

| Linha do painel | Arquivo | SHA-256 |
|---|---|---|
| Pacote x64 | `dist/center/Firawynix-Dock-Setup-x64.exe` | `F6A3ED5FDD46A4E669E1B175853A835DFFBF838C0CE05BCB9ABD698B7419A245` |
| Pacote x86 | `dist/center/Firawynix-Dock-Setup-x86.exe` | `99F32FD9000119C0C27B474B8CF2F74AD87C01B389CB61B6DBC1731E50D6801E` |
| Instalador único, se necessário | `dist/center/Firawynix-Dock-Setup.exe` | `680D6B9E5A5307D6153ED123552214A7AF327CC0F19D6675B9844F6423AA2620` |

Os três arquivos estão assinados por `CN=Firawynix`, certificado `9A2EFF2483185C9A900F2797D7A9CBD5E8A12893`, com carimbo de tempo e versão interna `1.1.1`. O catálogo atual aponta para o **pacote x64**, então envie o x64 e o x86; o instalador único é opcional.

1. No painel do **Firawynix Center**, abra **Editar** no Firawynix Dock › **Instalador (Windows)** › **Publicar versão nova**.
2. Escolha os pacotes x64 e x86, cole cada SHA-256 acima e clique em **Enviar e conferir** para cada um.
3. Informe **Nova versão** `1.1.1`, seu código **2FA** e clique em **Publicar**. O painel atualizará tamanho e hash do catálogo.
4. Se necessário, ajuste **Tamanho instalado (bytes)** para aproximadamente `164444010` e salve. Confirme o valor real pela barra de progresso num PC de teste.
5. Verifique `https://jogos.firawynix.com.br/api/games/firawynix-dock`: a versão deve ser `1.1.1`. Baixe o pacote público e compare o hash com o x64 acima.

Depois da instalação, o Dock oferece solicitar fixação. O Windows pode exigir que o usuário fixe pelo menu Iniciar; o painel só abre após a verificação da fixação. Faça também o teste de desfixar enquanto o Dock ainda está em execução e tentar reabrir pelo ícone da área de notificação.
