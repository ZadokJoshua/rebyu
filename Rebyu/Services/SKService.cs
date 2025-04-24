using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Rebyu.Services;

public class SKService
{
    public SKService()
	{
        var kernel = App.ServiceProvider.GetRequiredService<Kernel>();
    }
}
