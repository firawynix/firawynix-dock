# Firawynix Dock no Center — produto novo

Ficha para preenchimento manual conforme `CENTER-PUBLICAR-PELO-PAINEL.md` do portfólio. Esta é a **primeira publicação** do item `firawynix-dock`, não uma atualização.

## Arquivos prontos

| Campo do painel | Arquivo local | SHA-256 |
|---|---|---|
| Instalador online | `dist/center/Firawynix-Dock-Setup.exe` | `C87D3527462975E998252E58D55CED4EED4CFFB05218A4E02C53A1E684F628D4` |
| Pacote x64 | `dist/center/Firawynix-Dock-Setup-x64.exe` | `76C8E62207EE98F612D72DA3FC76F6118BAF66F04DA9D3DCE979852168EBE3E4` |
| Pacote x86 | `dist/center/Firawynix-Dock-Setup-x86.exe` | `B2F90B42111C6DA9ECFE3C09E120DF13582C431969A9718E1D734EC9F0E03B20` |

Os três arquivos são executáveis Windows assinados com SHA-256, com carimbo de tempo e certificado `CN=Firawynix`, thumbprint `9A2EFF2483185C9A900F2797D7A9CBD5E8A12893`. O instalador online inclui as duas arquiteturas e escolhe a adequada ao Windows; por isso é maior que cada pacote individual. Todos instalam silenciosamente com `/VERYSILENT`. A versão interna do produto é `1.0.0`, e a versão do arquivo Windows é `1.0.0.0`.

Artes para o cadastro:

| Campo | Arquivo local | Dimensões |
|---|---|---|
| Capa | `art/center/capa-600x800.png` | 600 × 800 |
| Banner | `art/center/banner-1920x620.png` | 1920 × 620 |
| Ícone | `art/center/icon-256.png` | 256 × 256, fundo transparente |

## Primeiro cadastro

1. No painel › **Firawynix Center**, clique em **Novo projeto**.
2. Preencha **Nome** `Firawynix Dock`, **Slug** `firawynix-dock`, **Categoria** `Projeto`, **Tipo** `Baixar`, **Subtítulo** `Seus apps a um clique.`, **Gêneros** `Utilitário, Produtividade` e **Cor** `#22d3ee`. Deixe **Publicado** desmarcado.
3. Descrição sugerida: `Acesse jogos, programas e sites do Firawynix Center em um painel flutuante junto à barra de tarefas do Windows. O Dock detecta o que está instalado e mostra cada item com imagem e nome. Personalize a transparência e a aparência do painel.`
4. Em **Arte**, envie os três PNG da tabela acima.
5. Em **Instalador (Windows)**, clique em **Usar o link do instalador online**. Confira os campos:

   | Campo | Valor |
   |---|---|
   | Certificado do instalador (SHA-1) | `9A2EFF2483185C9A900F2797D7A9CBD5E8A12893` |
   | Tipo do instalador | `Inno Setup` |
   | Chave do registro | `{887A8A7A-D695-43E4-8B76-DEA0E1F67EA2}_is1` |
   | Executável | `FirawynixDock.exe` |
   | Caminhos candidatos | `%LOCALAPPDATA%\Programs\Firawynix Dock\FirawynixDock.exe` |
   | Tamanho instalado (bytes) | `139007094` (medido no teste x64) |
   | Limpeza da desinstalação completa | `%LOCALAPPDATA%\FirawynixDock` (cache e preferências do Dock) |
   | Site oficial | `https://lab.firawynix.com.br/dock/` |
   | Repositório, se houver campo | `https://github.com/firawynix/firawynix-dock` |

6. Clique em **Salvar** ainda com **Publicado** desmarcado. Não digite versão, tamanho do download nem SHA-256 do catálogo; a publicação preencherá esses dados.

## Enviar e publicar a primeira versão

1. Abra **Editar** no item recém-criado e vá a **Publicar versão nova**.
2. Em cada uma das três linhas, escolha o `.exe` correspondente e cole o SHA-256 da tabela. Clique em **Enviar e conferir** e aguarde a validação de cada arquivo. Não há `.sig` de Tauri para estes instaladores.
3. Informe **Nova versão** `1.0.0`, um código atual do **2FA** e clique em **Publicar**. Este passo cria a versão inicial; não use o fluxo de atualização de um produto existente.
4. Só depois da confirmação, marque **Publicado** e clique em **Salvar**.
5. Num PC de teste, instale pelo Center, abra o Dock e desinstale. O atalho do menu Iniciar pode ser fixado na barra de tarefas. Confira também os links públicos do item e a exibição no catálogo.

Os instaladores assinados ficam em `dist/center/` e não são versionados no GitHub. As artes e o script de empacotamento estão no repositório. Se recompilar ou assinar novamente, recalcule os três SHA-256 antes de colar no painel.
