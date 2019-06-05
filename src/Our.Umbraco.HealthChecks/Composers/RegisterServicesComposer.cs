using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Our.Umbraco.HealthChecks.Composers
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class RegisterServicesComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<ILocalizedTextService, LocalizedTextService>();
        }
    }
}