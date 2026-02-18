using KidFit.Shared.TaskRequests;
using TickerQ.Utilities;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Enums;

namespace KidFit.Services
{
    public class TaskScheduleService(MailService mailService, ILogger<TaskScheduleService> logger)
    {
        private readonly MailService _mailService = mailService;
        private readonly ILogger<TaskScheduleService> _logger = logger;

        [TickerFunction(functionName: "SendWelcomeEmail", taskPriority: TickerTaskPriority.High)]
        public async Task SendWelcomeEmail(TickerFunctionContext ctx, CancellationToken canceled)
        {
            _logger.LogInformation($"Start job: Send welcome email");

            // Get parameters from context
            var request = await TickerRequestProvider.GetRequestAsync<SendWelcomeEmailRequest>(ctx, canceled);

            // Prepare email template
            var tmpl = MailService.PrepareWelcomeEmailTemplate(request.Fullname, request.Username, request.Id, request.Token);

            // Send email
            var recipents = new List<(string, string)> { (request.Fullname, request.Email) };
            _mailService.SendEmail(recipents, "Welcome to KidFit", tmpl);

            _logger.LogInformation($"Job complete: Send welcome email");
        }
    }
}
