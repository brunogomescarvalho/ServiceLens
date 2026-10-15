# ServiceLens

Visualizador local de arquivos de log associados a serviços do Windows. MVP em C# / WPF, sem pacotes NuGet externos.

## Executar

Windows 10/11 com SDK .NET 8 e suporte de desenvolvimento desktop .NET no Visual Studio 2022. Abra `ServiceLens/ServiceLens.csproj` ou execute:

```powershell
dotnet run --project .\ServiceLens\ServiceLens.csproj
```

Para gerar executável independente do runtime (necessita acesso aos pacotes de runtime durante a publicação):

```powershell
.\publish.ps1
```

A saída estará em `artifacts/win-x64`. Execute `ServiceLens.exe`.

## Usar

1. Clique em Consultar serviços do Windows e encontre o nome interno (`SERVICE_NAME`).
2. Clique Novo e cadastre nome, serviço, pasta absoluta e arquivo/padrão, por exemplo `app-*.log`.
3. Selecione a aplicação. O visualizador abre uma janela de até 256 KiB do arquivo mais recentemente modificado e acompanha novas linhas completas.
4. Busque texto, filtre o nível ou desmarque Seguir rolagem. A leitura continua durante a pausa da rolagem. Ctrl+C copia as entradas selecionadas.

Configurações: `%LOCALAPPDATA%/ServiceLens/profiles.json`. Nenhum serviço é iniciado, alterado ou interrompido. O usuário precisa de permissão de leitura na pasta dos logs e de consulta ao serviço. Pastas indisponíveis são tentadas novamente automaticamente.

## Comportamento e limites

- UTF-8, com tratamento de caracteres divididos entre leituras e linhas ainda incompletas. A última linha aparece ao receber uma quebra de linha ou na troca do arquivo.
- Leitura compartilhada com escrita/exclusão. Polling de 1 segundo, consulta de serviço a cada 5 segundos.
- Mudança do arquivo mais recente, truncamento e recriação por mudança de data de criação reabrem a leitura. Substituições que preservem caminho, tamanho e data de criação podem não ser detectadas.
- Até 10.000 linhas em memória. A troca de aplicação limpa a visualização; Limpar tela não apaga os arquivos.
- Classificação de níveis por palavras usuais, não por parser específico. JSON é apresentado em texto bruto; stack traces preservam as linhas, mas ainda não são agrupados.
- Uma aplicação por vez; visão conjunta e seleção visual de serviços ficam para uma próxima versão.
- Leitura limitada por ciclo, mas executada no dispatcher: arquivos locais são o alvo; pastas de rede lentas podem bloquear a interface.
- O programa consulta `sc.exe`, cujo texto/estado aparece conforme o idioma do Windows.

## Validação

O ambiente de criação não possui SDK .NET nem Windows; compilação e execução da interface precisam ser validadas no Windows. O projeto inclui um roteiro manual em `tests/acceptance.md`.
