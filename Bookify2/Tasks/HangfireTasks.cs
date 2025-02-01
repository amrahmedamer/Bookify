namespace Bookify.Tasks
{
    public class HangfireTasks
    {
        private readonly IApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWhatsAppClient _whatsAppClient;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        public HangfireTasks(IApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IWhatsAppClient whatsAppClient, IEmailSender emailSender, IEmailBodyBuilder emailBodyBuilder)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _whatsAppClient = whatsAppClient;
            _emailSender = emailSender;
            _emailBodyBuilder = emailBodyBuilder;
        }
        public async Task PrepareExpirationAlert()
        {
            //I want select last date in subscription and compare between last and today for send message
            var subscribers = _context.Subscribers
                .Include(x => x.Subscriptions)
                .Where(x => !x.IsBlockListed && x.Subscriptions.OrderByDescending(x => x.EndDate).First().EndDate.AddDays(-5) == DateTime.Today)
                .ToList();

            foreach (var subscriber in subscribers)
            {
                var endDate = subscriber.Subscriptions.Last().EndDate.ToString("d MMM,yyyy");

                var placeholders = new Dictionary<string, string>()
                {
                    {  "imageUrl","https://res.cloudinary.com/cloudomar/image/upload/c_scale,h_100,w_250/v1721488517/Online_calendar-cuate_yuitzd.png" },
                    {  "header", $"Hey {subscriber.FirstName}," },
                    {  "body", $"your subscription will be expired by{endDate} " },

                };

                var body = _emailBodyBuilder.GetEmailBody(EmailTemplate.notification, placeholders);

                await _emailSender.SendEmailAsync(subscriber.Email, "Bookify Subscription Expiration", body);

                if (subscriber.HasWhatsApp)
                {
                    var components = new List<WhatsAppComponent>()
                    {
                        new WhatsAppComponent
                        {
                            Type="body",
                            Parameters=new List<object>()
                            {
                                new WhatsAppTextParameter{Text=subscriber.FirstName},
                                new WhatsAppTextParameter{Text=endDate}
                            }
                        }
                    };

                    var number = _webHostEnvironment.IsDevelopment() ? "01029409898" : subscriber.MobileNumber;
                    await _whatsAppClient.SendMessage($"2{number}", WhatsAppLanguageCode.English_US, WhatsAppTemplates.SubscriptionExpirationAlert, components);
                }

            }
        }
    }
}
