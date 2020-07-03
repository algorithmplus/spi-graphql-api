using System.Threading.Tasks;
using Dfe.Spi.Common.Logging.Definitions;
using Dfe.Spi.GraphQlApi.Domain.Graph;
using GraphQL;
using GraphQL.Types;

namespace Dfe.Spi.GraphQlApi.Application.GraphTypes
{
    public class SpiSchema : Schema
    {
        private readonly ILoggerWrapper _logger;

        public SpiSchema(IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<SpiQuery>();

            _logger = resolver.Resolve<ILoggerWrapper>();
        }

        public async Task<string> ExecuteAsync(GraphRequest request)
        {
            _logger.Info($"Executing query {request.Query}");
            
            var result = await this.ExecuteAsync(_ =>
            {
                _.Query = request.Query;
                _.Inputs = request.Variables.ToInputs();
                // _.Inputs = request.Variables != null ? new GraphQL.Inputs(request.Variables) : null;
            });
            _logger.Info($"Got query result {result}");

            return result;
        }
    }
}