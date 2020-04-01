using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.GraphQlApi.Application.GraphTypes.Inputs;
using Dfe.Spi.GraphQlApi.Domain.Registry;
using Dfe.Spi.GraphQlApi.Domain.Repository;
using Dfe.Spi.Models.Entities;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace Dfe.Spi.GraphQlApi.Application.Resolvers
{
    public interface IManagementGroupsResolver : IResolver<Models.Entities.ManagementGroup[]>
    {
    }
    public class ManagementGroupsResolver : IManagementGroupsResolver
    {
        private readonly IEntityRepository _entityRepository;
        private readonly IRegistryProvider _registryProvider;
        private readonly ILoggerWrapper _logger;

        public ManagementGroupsResolver(
            IEntityRepository entityRepository,
            IRegistryProvider registryProvider,
            ILoggerWrapper logger)
        {
            _entityRepository = entityRepository;
            _registryProvider = registryProvider;
            _logger = logger;
        }
        
        public async Task<ManagementGroup[]> ResolveAsync<TContext>(ResolveFieldContext<TContext> context)
        {
            try
            {
                var references = await SearchAsync(context, context.CancellationToken);

                var fields = GetRequestedFields(context);
                var entities = await LoadAsync(references, fields, context.CancellationToken);

                return entities;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error resolving learning providers", ex);
                throw;
            }
        }

        private async Task<AggregateEntityReference[]> SearchAsync<T>(ResolveFieldContext<T> context, CancellationToken cancellationToken)
        {
            var searchRequest = GetSearchRequest(context);
            var searchResults = await _registryProvider.SearchManagementGroupsAsync(searchRequest, cancellationToken);
            return searchResults.Results.Select(d =>
                    new AggregateEntityReference {AdapterRecordReferences = d.Entities})
                .ToArray();
        }

        private async Task<ManagementGroup[]> LoadAsync(AggregateEntityReference[] references, string[] fields,
            CancellationToken cancellationToken)
        {
            var request = new LoadManagementGroupsRequest
            {
                EntityReferences = references,
                Fields = fields,
            };
            var loadResult = await _entityRepository.LoadManagementGroupsAsync(request, cancellationToken);

            return loadResult.SquashedEntityResults.Select(x => x.SquashedEntity).ToArray();
        }

        private SearchRequest GetSearchRequest<T>(ResolveFieldContext<T> context)
        {
            var criteria = (ComplexQueryModel) context.GetArgument(typeof(ComplexQueryModel), "criteria");
            var skip = context.HasArgument("skip")
                ? (int) context.Arguments["skip"]
                : 0;
            var take = context.HasArgument("take")
                ? (int) context.Arguments["take"]
                : 50;

            var searchGroups = new List<SearchGroup>();
            foreach (var @group in criteria.Groups)
            {
                var filters = @group.Conditions.Select(c => new SearchFilter
                {
                    Field = MapSearchField(c.Field),
                    Operator = c.Operator,
                    Value = c.Value,
                }).ToArray();
                
                searchGroups.Add(new SearchGroup
                {
                    Filter = filters,
                    CombinationOperator = group.IsOr ? "or" : "and",
                });
            }

            return new SearchRequest
            {
                Groups = searchGroups.ToArray(),
                CombinationOperator = criteria.IsOr ? "or" : "and",
                Skip = skip,
                Take = take,
            };
        }

        private string MapSearchField(string searchValue)
        {
            switch (searchValue.ToLower())
            {
                case "type":
                    return "managementGroupType";
                case "code":
                    return "managementGroupId";
            }

            return searchValue;
        }

        private string[] GetRequestedFields<T>(ResolveFieldContext<T> context)
        {
            var selections = context.FieldAst.SelectionSet.Selections.Select(x => ((Field) x).Name);
            
            // Will need identifiers for resolving sub objects (such as census), so request them from backend
            selections = selections.Concat(new[] {"code"}).Distinct();

            return selections.ToArray();
        }
    }
}