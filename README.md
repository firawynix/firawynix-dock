# Firawynix Dock

Aplicativo independente para Windows. Mostra jogos, programas e sites do catálogo do Firawynix Center em um painel flutuante ciano acima da barra de tarefas. Cada cartão mostra a imagem e o nome do item, além de indicar se é um site, se está instalado ou se deve ser aberto pelo Center.

## Usar

Compile com o SDK do .NET 10 no Windows usando `dotnet build -c Release` e execute `bin/Release/net10.0-windows10.0.19041.0/FirawynixDock.exe`. O atalho **Firawynix Dock** no menu Iniciar pode ser fixado na barra de tarefas. Um clique no ícone abre ou fecha o painel junto ao ponto clicado, na borda real da barra de tarefas, inclusive em monitores e posições diferentes. O ícone na área de notificação também permite abrir, atualizar ou encerrar o dock.

Use a busca para filtrar a lista. A barra de rolagem à direita pode ser arrastada ou acionada pela roda do mouse. O botão de engrenagem no topo oferece opções de **Transparência do fundo** de 0% a 100%, além de opções independentes para ocultar a moldura externa, a busca e o cabeçalho com seus botões. Essas escolhas ficam salvas para a próxima abertura. Os cartões continuam visíveis, inclusive com o fundo totalmente transparente. Clique com o botão direito em um cartão para abrir as configurações e reativar o cabeçalho quando ele estiver oculto. O menu do ícone na área de notificação também permite alterar a aparência. O painel se fecha após oito segundos sem movimento, tecla ou rolagem; clicar fora o fecha imediatamente.

Na versão 1.1.1, **Ocultar sites** e **Ocultar não instalados** vêm ativos. Os dois botões ao lado da busca permitem controlar cada filtro separadamente. O menu de contexto também oferece **Só instalados** e ambos os filtros; as escolhas ficam salvas. Ao abrir sem estar fixado na barra de tarefas, o Dock apresenta a tela de fixação. Essa checagem é repetida sempre que o painel é aberto, mesmo com o aplicativo ainda ativo na área de notificação. O Windows pede confirmação para fixar; caso a solicitação automática não esteja disponível, fixe pelo menu Iniciar e clique em **Já fixei · verificar**.

A área dos cartões tem cantos arredondados e fundo que acompanha o painel. No fim da lista, o espaço livre ao lado da última linha permanece limpo, sem uma faixa escura retangular.

O dock lê o catálogo público e, quando necessário, o catálogo local salvo pelo Center. Detecta instalações por caminhos, registros do Windows e locais escolhidos no Center. Também mantém seu próprio cache de catálogo e imagens para uso sem internet. Ele não altera arquivos do Firawynix Center e não instala nem baixa programas. Para um item ausente, abre o Center.

## Verificações

- `FirawynixDock.exe --check` grava `verification.json` com os destinos dos itens.
- `FirawynixDock.exe --check-images` grava `images-verification.json` com o resultado de cada imagem.
- `FirawynixDock.exe --preview` grava `preview.png` para conferir o visual.
- `FirawynixDock.exe --preview-bottom` grava `preview-bottom.png` com o fim da lista para conferir a última linha.
- `FirawynixDock.exe --check-placement` verifica o posicionamento nas quatro bordas e em um segundo monitor.
- `FirawynixDock.exe --check-pin` grava `pin-verification.json` com o estado da fixação informado pelo Windows.

## Microsoft Store e site local

Os pacotes MSIX de Windows x64 e x86 ficam em `dist/store/`. Ambos usam a identidade `Firawynix.FirawynixDock` e o ID de produto da Store `9MT74R15N5JQ`. A Microsoft Store assina os pacotes após a certificação; os arquivos locais de canal Store não são instaladores independentes.

O site local fica em `site/index.html`. Ele segue a barra inferior fixa usada nos outros sites Firawynix e inclui uma demonstração com busca e controle de transparência. Basta abrir o arquivo no navegador. A versão anterior já está disponível na Microsoft Store.
