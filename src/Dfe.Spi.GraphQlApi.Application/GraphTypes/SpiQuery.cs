using Dfe.Spi.GraphQlApi.Application.Resolvers;
using GraphQL.Types;

namespace Dfe.Spi.GraphQlApi.Application.GraphTypes
{
    public class SpiQuery : ObjectGraphType
    {
        public SpiQuery(
            ILearningProviderResolver learningProviderResolver,
            ILearningProvidersResolver learningProvidersResolver)
        {
            Field<LearningProviderType>("learningProvider",
                resolve: learningProviderResolver.ResolveAsync,
                arguments: new QueryArguments(new QueryArgument[]
                {
                    new QueryArgument<IntGraphType> {Name = "urn"},
                    new QueryArgument<IntGraphType> {Name = "ukprn"},
                    new QueryArgument<StringGraphType> {Name = "uprn"},
                    new QueryArgument<StringGraphType> {Name = "companiesHouseNumber"},
                    new QueryArgument<StringGraphType> {Name = "charitiesCommissionNumber"},
                    new QueryArgument<StringGraphType> {Name = "dfeNumber"},
                    new QueryArgument<IntGraphType> {Name = "establishmentNumber"},
                    new QueryArgument<IntGraphType> {Name = "previousEstablishmentNumber"},
                }));

            Field<ListGraphType<LearningProviderType>>("learningProviders",
                resolve: learningProvidersResolver.ResolveAsync,
                arguments: new QueryArguments(
                    new QueryArgument<StringGraphType> {Name = "name"}));
        }
    }
}