namespace Lactosure_api.Helpers
{
    public class OtpHelper
    {
        public static string GenerateOtp()
        {
            Random random = new Random();

            return random.Next(100000, 999999).ToString();
        }
    }
}
