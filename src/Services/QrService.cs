using QRCoder;

namespace KidFit.Services
{
    public class QrService
    {
        public static byte[] GenerateQr(string url)
        {
            // Create URL payload
            var payload = new PayloadGenerator.Url(url);

            // Create QR data
            var data = QRCodeGenerator.GenerateQrCode(payload, QRCodeGenerator.ECCLevel.Default);

            // Generate byte array
            using var renderer = new PngByteQRCode(data);
            return renderer.GetGraphic(20);
        }
    }
}
