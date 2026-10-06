# Corrigir a publicação do Firawynix Dock no Center — 1.1.3

Conferido em 05/10/2026: o catálogo público anuncia `1.1.2`, mas o instalador e o aplicativo publicados têm versão interna `1.1.1`. Isso faz o Center continuar oferecendo atualização depois de instalar. A URL `https://jogos.firawynix.com.br/api/games/firawynix-dock/windows/arquivo` está correta para o instalador único. Ele contém x64 e x86 no mesmo arquivo; não é um baixador pequeno.

## Corrigir os campos do item

Em **Editar Firawynix Dock**, mantenha:

| Campo | Valor |
|---|---|
| Link do instalador | `https://jogos.firawynix.com.br/api/games/firawynix-dock/windows/arquivo` |
| Certificado do instalador (SHA-1) | `9A2EFF2483185C9A900F2797D7A9CBD5E8A12893` |
| Tipo do instalador | `Inno Setup — silencioso (/VERYSILENT)` |
| Chave de registro | `{887A8A7A-D695-43E4-8B76-DEA0E1F67EA2}_is1` |
| Executável | `FirawynixDock.exe` |
| Caminho candidato | `%LOCALAPPDATA%\Programs\Firawynix Dock\FirawynixDock.exe` |
| Tamanho instalado (estimativa) | `164444010` bytes |

**Clique em Salvar depois de corrigir “Executável”.** A imagem mostra `FirawynixDock.exe` no formulário, mas a API pública ainda devolve `Firawynix-Dock-Setup.exe`, então a alteração parece não ter sido salva. O instalador não fica dentro da pasta instalada; ali fica `FirawynixDock.exe`.

Não edite manualmente **Versão**, **Tamanho (bytes)** nem **SHA-256**. O fluxo **Publicar versão nova** preenche esses campos com o arquivo hospedado. Atualmente o tamanho anunciado (`84197376`) também não corresponde ao arquivo servido (`84193888` bytes); a nova publicação deve corrigir isso.

## Publicar 1.1.3 pelo painel

Os três arquivos abaixo foram reconstruídos com versão interna **1.1.3**, assinatura válida `CN=Firawynix` e carimbo de tempo. A versão é maior que a `1.1.2` publicada, sem precisar usar **Voltar**.

| Linha do painel | Arquivo local | SHA-256 | Bytes |
|---|---|---|---:|
| Instalador online | `dist/center/Firawynix-Dock-Setup.exe` | `30A77973D52D3AA5FEA0B9582013B5728DAC557092F9D1F9B8E0CEE65DB5396E` | 84185064 |
| Pacote x64 | `dist/center/Firawynix-Dock-Setup-x64.exe` | `BD65E84767E169A35489B16657999E928B2D6CD31E5925CA863631E4E603AF61` | 44810136 |
| Pacote x86 | `dist/center/Firawynix-Dock-Setup-x86.exe` | `ADCA714B288C9F36AF4E8E28473EB3D75FAEE027209C2D2D5C63BA03CC2ACC68` | 41527816 |

Abra **Publicar versão nova**, envie e confira os três arquivos com seus hashes. Informe **Nova versão** `1.1.3`, digite o código 2FA atual e clique em **Publicar**. O guia geral é `C:\Users\Hugo\firawynix-portfolio\docs\CENTER-PUBLICAR-PELO-PAINEL.md`.

Depois, confira `https://jogos.firawynix.com.br/api/games/firawynix-dock`: versão `1.1.3`, `winExe` igual a `FirawynixDock.exe`, `downloadSha256` igual ao hash do instalador único acima e tamanho `84185064`. Instale pelo Center e confirme que o registro do Windows informa `DisplayVersion` `1.1.3` e que o botão **Abrir** inicia o Dock.
