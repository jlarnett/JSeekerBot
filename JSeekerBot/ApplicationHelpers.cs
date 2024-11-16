using System;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Playwright;

namespace JSeekerBot
{
    public static class ApplicationHelpers
    {
        public static async Task<JobApplicationStatusEntry> GetApplicationStatusEntryDetailsAsync(this IPage Page, bool applicationSubmitted, bool hadEasyApplyButton)
        {
            string? companyTitle = "";

            //Get the dirty company title. Has newlines and excess spaces
            if (await Page.Locator("#jobs-apply-header").IsVisibleAsync())
                companyTitle = await Page.Locator("#jobs-apply-header").TextContentAsync();

            //var compensation = await Page.GetByText(new Regex("K\\/yr")).InnerTextAsync();
            var jobTitle = await Page.Locator(".job-details-jobs-unified-top-card__job-title").TextContentAsync();

            //Get Salary Information for job. May not exists
            var salary = await Page.Locator(".job-details-preferences-and-skills__pill").First.InnerTextAsync();
            if (!salary.Contains("K/yr"))
                salary = "";

            //Cleaned up strings
            var cleanCompanyTitle = "";
            var cleanJobTitle = "";

            //Cleanup company name
            if (companyTitle != null)
                cleanCompanyTitle = companyTitle.Replace("Apply to", "").Replace("\n", "").Trim();

            //Cleanup job title
            if(jobTitle != null)
                cleanJobTitle = jobTitle.Replace("\n", "").Trim();

            //return the entry
            return new JobApplicationStatusEntry(cleanCompanyTitle, applicationSubmitted, DateTime.UtcNow, cleanJobTitle, hadEasyApplyButton, salary);
        }

        public static async Task LoginLinkedInAsync(this IPage Page)
        {
            await Page.Locator("[data-test-id=\"home-hero-sign-in-cta\"]").ClickAsync();
            await Page.WaitForURLAsync("https://www.linkedin.com/login");

            //Login
            var email = TestConfig.GetVarFor<string>("LINKEDIN_USERNAME");
            var pass = TestConfig.GetVarFor<string>("LINKEDIN_PASSWORD");

             
            await Page.GetByLabel("Email or phone").FillAsync(email);
            await Page.GetByLabel("Password").FillAsync(pass);
            await Page.GetByRole(AriaRole.Button, new () { NameString = "Sign in" }).ClickAsync();

            if (await Page.GetByRole(AriaRole.Heading, new() { NameString = "Let’s do a quick security check" })
                    .IsVisibleAsync())
            {
                    await Page.WaitForTimeoutAsync(10000);
                   
                    await Page.ClickAsync("body", new PageClickOptions()
                    {
                        Position = new Position()
                        {
                            X = 625,
                            Y = 480
                        }
                    });
                    await Page.GetByRole(AriaRole.Heading, new() { NameString = "Let’s do a quick security check" }).ClickAsync();
                    await Page.ScreenshotAsync(new()
                    {
                        Path = "verification.png",
                    });

                    var verificationBitmap = Bitmap.FromFile("verification.png");

                    var croppedImage = cropImage(verificationBitmap, new Rectangle(4, 305, 299, 199));
                    croppedImage.Save("verificationCrop.png", ImageFormat.Png);


                    //var image1 = cropImage(croppedImage, new Rectangle(1, 1, 99, 99));
                    //var image2 = cropImage(croppedImage, new Rectangle(100, 1, 99, 99));
                    //var image3 = cropImage(croppedImage, new Rectangle(200, 1, 99, 99));
                    //var image4 = cropImage(croppedImage, new Rectangle(1, 100, 99, 99));
                    //var image5 = cropImage(croppedImage, new Rectangle(100, 100, 99, 99));
                    //var image6 = cropImage(croppedImage, new Rectangle(200, 100, 99, 99));


                    //image1.Save("sample1.png", ImageFormat.Png);
                    //image2.Save("sample2.png", ImageFormat.Png);
                    //image3.Save("sample3.png", ImageFormat.Png);
                    //image4.Save("sample4.png", ImageFormat.Png);
                    //image5.Save("sample5.png", ImageFormat.Png);
                    //image6.Save("sample6.png", ImageFormat.Png);

                    //Load sample data
                    //var imageBytes = File.ReadAllBytes(@"C:\Users\im10g\source\repos\JSeekerBot\JSeekerBot\Captcha_Training_Data\FacingDown\FacingDown1.PNG");
                    //DoggoCaptcha.ModelInput sampleData = new DoggoCaptcha.ModelInput()
                    //{
                    //    ImageSource = imageBytes,
                    //};

                    ////Load model and predict output
                    //var result = DoggoCaptcha.Predict(sampleData);

            }
        }

        private static Image cropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }

    }



}
