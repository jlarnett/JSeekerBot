using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
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

            if (!await Page.Locator(".share-box-feed-entry__top-bar").IsVisibleAsync())
            {
                await Page.WaitForTimeoutAsync(30000);

                await Page.ClickAsync("body", new PageClickOptions()
                {
                    Position = new Position()
                    {
                        X = 625,
                        Y = 480
                    }
                });

                await Page.ScreenshotAsync(new()
                {
                    Path = "verification.png",
                });
            }
        }
    }

}
