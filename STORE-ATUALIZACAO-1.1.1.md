# Microsoft Store — atualização 1.1.1

Em 05/10/2026, a versão 1.1.0 apareceu como publicada no Partner Center (envio 2). Para corrigir os filtros e a checagem de fixação, foi criado o envio 3 (`1152921505702049999`).

Os pacotes 1.1.1 estão prontos em:

| Arquitetura | Arquivo | SHA-256 |
|---|---|---|
| x64 | `dist/store/Firawynix-Dock-1.1.1.0-x64-store.msix` | `94F25FBD82A36BF4DB6CCE35069A7DFECCC7CA1860AAB6E0A45D2D66F10DAD19` |
| x86 | `dist/store/Firawynix-Dock-1.1.1.0-x86-store.msix` | `10B5C118C05E64E23484FF80D7D7F7E4D0D2B0F9BC54FEBEA6AD281130681C2F` |

**Estado:** o envio 3 ainda é rascunho e não foi enviado para certificação. O Partner Center rejeitou repetidas tentativas de upload com a mensagem genérica `Status: OK, FaultCode: undefined`, sem aceitar os arquivos. O rascunho ficou sem pacotes após a retirada dos pacotes 1.1.0 herdados; a versão pública 1.1.0 não foi removida.

Para concluir quando o upload voltar a funcionar: abra `https://partner.microsoft.com/pt-br/dashboard/products/9MT74R15N5JQ/submissions/1152921505702049999/packages`, envie os dois MSIX acima, espere **Validated**, salve e envie o rascunho para certificação. Mantenha preço, mercados e listagens existentes.
