using System.Diagnostics;
using System.Numerics;
using Knapcode.FactorioTools.OilField;
using Knapcode.FactorioTools.OilField.Steps;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Knapcode.FactorioTools.WebApp.Pages
{
    public class OilModel : PageModel
    {
        private readonly ILogger<OilModel> _logger;

        public OilModel(ILogger<OilModel> logger)
        {
            _logger = logger;
        }


        [BindProperty(SupportsGet = true)]
        public string? Source { get; set; }

        [BindProperty]
        public string? Error { get; set; }

        [BindProperty]
        public string? Status { get; set; }

        [BindProperty]
        public string? OutputBlueprint { get; set; }

        public void OnGet()
        {
            if (string.IsNullOrWhiteSpace(Source))
            {
                return;
            }

            var sw = Stopwatch.StartNew();

            try
            {
                var options = Options.ForMediumElectricPole;
                var bp = ParseBlueprint.Execute(Source);
                var context = Planner.Execute(options, bp);
                OutputBlueprint = GridToBlueprintString.Execute(context);
                Status = "Done: " + sw.Elapsed;
            }
            catch (Exception ex)
            {
                Error = ex.ToString();
                Status = "Error.";
            }

        }
    }
}