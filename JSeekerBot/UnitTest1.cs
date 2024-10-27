using JSeekerBot.Configuration;
using Microsoft.Playwright;

namespace JSeekerBot
{
    [TestFixture]
    public class ApplyForJobsEasily : PageTest
    {
        [Test]
        public async Task ApplyNow()
        {
            await Page.GotoAsync("https://linkedin.com/");

            await Page.Locator("[data-test-id=\"home-hero-sign-in-cta\"]").ClickAsync();
            await Page.WaitForURLAsync("https://www.linkedin.com/login");

            //Login

            var email = TestConfig.GetVarFor<string>("LINKEDIN_USERNAME");
            var pass = TestConfig.GetVarFor<string>("LINKEDIN_PASSWORD");


            await Page.GetByLabel("Email or phone").FillAsync(email);
            await Page.GetByLabel("Password").FillAsync(pass);
            await Page.GetByRole(AriaRole.Button, new () { NameString = "Sign in" }).ClickAsync();
            await Page.WaitForURLAsync("https://www.linkedin.com/feed/");

            //Navigate to the Find Jobs page. 
            await Page.GetByRole(AriaRole.Link, new () { NameString = "Jobs" }).ClickAsync();
            await Page.WaitForURLAsync("https://www.linkedin.com/jobs/");
            await Page.GetByRole(AriaRole.Link, new () { NameString = "Show all {:headerTitle}" }).ClickAsync();


            var currentPageCounter = 1;
            ILocator nextPageButton;

            //Check each job in the job list. Look for easy apply button
            do
            {
                await Page.WaitForTimeoutAsync(5000);
                await CloseJobDetails();
                var jobList = await Page.QuerySelectorAllAsync(".scaffold-layout__list-container .jobs-search-results__list-item");

                foreach (var jobPost in jobList)
                {
                    await jobPost.ClickAsync();
                    var easyApplyButton = Page.Locator(".jobs-apply-button--top-card").First;

                    if (await easyApplyButton.IsVisibleAsync())
                    {
                        if (await easyApplyButton.InnerTextAsync() == "Easy Apply")
                        {
                            await easyApplyButton.ClickAsync();
                            await TryToSubmitApplication();
                            await Page.WaitForTimeoutAsync(1500);
                        }
                    }
                }

                currentPageCounter++;
                nextPageButton = Page.Locator($"[data-test-pagination-page-btn='{currentPageCounter}']");
                await nextPageButton.ClickAsync();

            } while (await nextPageButton.IsVisibleAsync() || await nextPageButton.IsEnabledAsync());

            await Page.PauseAsync();
        }


        /// <summary>
        /// This function contains a loop to pass through the Easy Apply Pages
        /// Calls another function to try to fill the required inputs first. 
        /// Once the Review Application button is present clicks it, then submits the application.
        /// </summary>
        /// <returns></returns>
        private async Task TryToSubmitApplication()
        {
            int tryCounter = 0;
            while (await Page.Locator($"[aria-label='Continue to next step']").IsVisibleAsync() && tryCounter < 15)
            {
                await TryFillEmptyInputs();

                if (await Page.Locator($"[aria-label='Continue to next step']").IsVisibleAsync() && await Page.Locator($"[aria-label='Continue to next step']").CountAsync() > 0 && await Page.Locator($"[aria-label='Continue to next step']").IsEnabledAsync())
                    await Page.Locator($"[aria-label='Continue to next step']").ClickAsync();

                tryCounter++;
            }

            if (tryCounter < 15)
            {
                await TryFillEmptyInputs();
                if (await Page.GetByRole(AriaRole.Button, new() { NameString = "Review your application" }).IsVisibleAsync())
                    await Page.GetByRole(AriaRole.Button, new () { NameString = "Review your application" }).ClickAsync();

                if (await Page.GetByRole(AriaRole.Button, new () { NameString = "Submit application" }).IsVisibleAsync())
                    await Page.GetByRole(AriaRole.Button, new() { NameString = "Submit application" }).ClickAsync();

            }

            await CloseJobDetails();
        }

        /// <summary>
        /// Attempts to fill the easy apply required fields. Currently just puts a 4 in any text input and selects yes to any dropdown.
        /// TODO: Implement a system of tags - value pairs to input more accurate information
        /// </summary>
        /// <returns></returns>
        private async Task TryFillEmptyInputs()
        {
            var inputs = await Page.QuerySelectorAllAsync(".artdeco-text-input--input");

            foreach (var input in inputs)
            {
                if(await input.InputValueAsync() == string.Empty)
                {
                    await input.FillAsync("4");
                }
            }

            var comboBoxes = await Page.QuerySelectorAllAsync("select");

            foreach (var box in comboBoxes)
            {
                if (await box.InputValueAsync() == "" || await box.InputValueAsync() == "Select an option")
                {
                    await box.SelectOptionAsync(new[] { "Yes" });
                }
            }
        }

        private async Task CloseJobDetails()
        {

            if (await Page.GetByRole(AriaRole.Button, new() { NameString = "Dismiss" }).IsVisibleAsync())
            {
                await Page.GetByRole(AriaRole.Button, new () { NameString = "Dismiss" }).ClickAsync();
            }

            if (await Page.GetByRole(AriaRole.Button, new() { NameString = "Discard" }).IsVisibleAsync())
            {
                await Page.GetByRole(AriaRole.Button, new () { NameString = "Discard" }).ClickAsync();
            }
        }
    }
}
