# Roteiro de aceitação — Windows

1. Compilar com `dotnet build ServiceLens/ServiceLens.csproj`; executar.
2. Cadastrar serviço real e pasta temporária com padrão `app-*.log`; fechar e reabrir para verificar persistência.
3. Criar `app-01.log`; acrescentar texto UTF-8 com acentos e emoji. Confirmar atualização e ausência de bloqueio ao escritor.
4. Gravar uma linha sem quebra, depois concluir com quebra: deve aparecer uma vez e completa.
5. Acrescentar ERROR, WARN, INFO e stack trace. Verificar cores, filtros, busca, cópia e pausa da rolagem sem interromper captura.
6. Criar `app-02.log` mais recente; confirmar troca. Truncar o arquivo e gravar novamente; verificar retomada.
7. Renomear/remover o arquivo aberto e criar outro; confirmar retomada sem fechar o aplicativo.
8. Usar pasta inexistente, depois criá-la: deve recuperar automaticamente. Testar permissão negada e nome de serviço inexistente.
9. Gerar mais de 10.000 linhas; confirmar limite de memória e funcionamento dos filtros.
10. Alternar rapidamente entre cadastros durante consulta de estado; conferir que o estado não migra para outro cadastro.
11. Remover um cadastro e reiniciar; conferir persistência e preservação dos arquivos de log.
