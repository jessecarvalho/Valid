## Como Executar

**Using dotnet**
1. dotnet run

**Using Docker**

1. docker build -t app .
2. docker run -p 8080:80 app

## Teste Online

Para facilitar os testes, a aplicação foi hospedada em:

## Pré-requisitos
- .NET 9.0 ou Docker

## Considerações

Meu esforço foi concentrado em equilibrar simplicidade e boas práticas de mercado, criando uma solução prática que pode ser entregue rapidamente, mas com uma arquitetura preparada para crescimento futuro sem necessidade de grandes refatorações.

## Tradeoffs Conscientes

1. **Persistência**: Optei por MemoryStorage para focar na lógica de negócio;
2. **Concorrência**: Uso de ConcurrentDictionary como solução simples e eficaz;
3. **Validação**: FluentValidation foi um overkill e poderia ser simplificado com simples annotations nesse cenário, mas achei válido implantar;
4. **DTOs**: Optei por não usar DTOs e expor a entidade em algumas responses em prol da praticidade e por conta de não termos nada que precisaria se manter oculto da api;
5. **Tratamento de erros**: Optei por usar um middleware para tratamento de erros visando centralizar todos os erros e ter maior previsibilidade e visibilidade;
6. **Profile Parameters**: Optei por usar um dictionary com <string, string> mesmo podendo ser um <string, bool> para manter flexível como a ideia original;
7. **Mediator Pattern**: Optei por usar esse padrão de projeto para manter maior modularidade e organização mesmo podendo ter facilitado a lógica já que é um projeto muito pequeno;
8. **Arquitetura de projeto**: Mesmo motivo acima, tudo poderia ser feito fácilmente em um único projeto usando minimal api e apenas models e services, mas escolhi ter um pouco mais de organização e pensamento no fúturo da aplicação;
9. **Health Check**: Implementei um endpoint básico de health check (`/health`) para monitoramento da saúde da aplicação, mesmo sendo um MVP. Optei por não incluir verificações complexas de dependências para manter a simplicidade, mas deixei a estrutura preparada para evoluções futuras;
10. **Logs**: Minha única implementação visando observabilidade foi os logs que considero ser extremamente benéficos para correção de bugs em prod e é muito fácil de implementar.
