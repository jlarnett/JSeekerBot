using System.Collections;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using JSeekerBot.Configuration;
using Microsoft.Playwright;
using Tensorflow;
using static System.Reflection.Metadata.BlobBuilder;

namespace JSeekerBot
{
    [TestFixture]
    public class ApplyForJobsEasily : PageTest
    {
        private List<JobApplicationStatusEntry> _applicationStatusEntries = new List<JobApplicationStatusEntry>();
        private ResponseInterpreter _responseInterpreter;

        [OneTimeSetUp]
        public void Setup()
        {
            _responseInterpreter = new ResponseInterpreter();
            _responseInterpreter.AddQuestionResponse("C#", "10");
            _responseInterpreter.AddQuestionResponse("bachelor", "No");
            _responseInterpreter.AddQuestionResponse("Bachelor", "No");
            _responseInterpreter.AddQuestionResponse("background check", "No");
            _responseInterpreter.AddQuestionResponse("phone number", "8123455974");
            _responseInterpreter.AddQuestionResponse("email", "im10g@hotmail.com");
            _responseInterpreter.AddQuestionResponse("Email", "im10g@hotmail.com");
            _responseInterpreter.AddQuestionResponse("visa", "No");
            _responseInterpreter.AddQuestionResponse("legally authorized to work", "yes");
            _responseInterpreter.AddQuestionResponse("Automation", "5");
            _responseInterpreter.AddQuestionResponse("automation", "5");
            _responseInterpreter.AddQuestionResponse("JavaScript", "5");
            _responseInterpreter.AddQuestionResponse("XML", "5");
            _responseInterpreter.AddQuestionResponse("onsite", "Yes");
            _responseInterpreter.AddQuestionResponse("SQL", "8");
            _responseInterpreter.AddQuestionResponse("Oracle", "2");
            _responseInterpreter.AddQuestionResponse("Powershell", "3");
            _responseInterpreter.AddQuestionResponse("Jira", "4");
            _responseInterpreter.AddQuestionResponse("hybrid", "4");
            _responseInterpreter.AddQuestionResponse("Software as a Service", "3");
            _responseInterpreter.AddQuestionResponse("SaaS", "3");
            _responseInterpreter.AddQuestionResponse("Java", "3");
            _responseInterpreter.AddQuestionResponse("Full-Stack", "6");
            _responseInterpreter.AddQuestionResponse("Web Application", "10");
            _responseInterpreter.AddQuestionResponse("commuting", "Yes");
            _responseInterpreter.AddQuestionResponse("AWS", "5");
            _responseInterpreter.AddQuestionResponse("Amazon Web Services", "5");
            _responseInterpreter.AddQuestionResponse("JQUERY", "10");
            _responseInterpreter.AddQuestionResponse("HTML", "10");
            _responseInterpreter.AddQuestionResponse(".NET", "10");
            _responseInterpreter.AddQuestionResponse("react", "2");
            _responseInterpreter.AddQuestionResponse("first name", "Johnny");
            _responseInterpreter.AddQuestionResponse("last name", "Arnett");
            _responseInterpreter.AddQuestionResponse("First name", "Johnny");
            _responseInterpreter.AddQuestionResponse("Last name", "Arnett");
            _responseInterpreter.AddQuestionResponse("phone", "8123455974");
            _responseInterpreter.AddQuestionResponse("Phone", "8123455974");
            _responseInterpreter.AddQuestionResponse("LinkedIn", "https://www.linkedin.com/in/johnny-arnett-350959135/");
            _responseInterpreter.AddQuestionResponse("linkedin", "https://www.linkedin.com/in/johnny-arnett-350959135/");
            _responseInterpreter.AddQuestionResponse("LINKEDIN", "https://www.linkedin.com/in/johnny-arnett-350959135/");
            _responseInterpreter.AddQuestionResponse("Secret", "No");
            _responseInterpreter.AddQuestionResponse("Secret Clearance", "No");
            _responseInterpreter.AddQuestionResponse("Clearance", "No");
            _responseInterpreter.AddQuestionResponse("clearance", "No");
            _responseInterpreter.AddQuestionResponse("secret", "No");

        }

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


            await Page.WaitForURLAsync("https://www.linkedin.com/feed/");


            //Navigate to the Find Jobs page. 
            await Page.GetByRole(AriaRole.Link, new () { NameString = "Jobs" }).ClickAsync();
            await Page.WaitForURLAsync("https://www.linkedin.com/jobs/");
            await Page.GetByRole(AriaRole.Link, new () { NameString = "Show all More jobs for you" }).ClickAsync();


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
                        else
                        {
                            string? companyTitle = "";

                            if (await Page.Locator("#jobs-apply-header").IsVisibleAsync())
                            {
                                companyTitle = await Page.Locator("#jobs-apply-header").TextContentAsync();
                            }

                            //var compensation = await Page.GetByText(new Regex("K\\/yr")).InnerTextAsync();

                            var jobTitle = await Page.Locator(".job-details-jobs-unified-top-card__job-title").TextContentAsync();

                            if (companyTitle != null)
                            {
                                string replaceWith = "";
                                var cleanCompanyTitle = companyTitle.Replace("Apply to", "").Replace("\n", replaceWith);
                                var cleanJobTitle = jobTitle.Replace("\n", replaceWith);
                                _applicationStatusEntries.Add(new JobApplicationStatusEntry(cleanCompanyTitle, false, DateTime.UtcNow, cleanJobTitle, false));
                            }
                        }
                    }
                }

                //Increment page counter & open next job page
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
            string? companyTitle = "";

            if (await Page.Locator("#jobs-apply-header").IsVisibleAsync())
            {
                companyTitle = await Page.Locator("#jobs-apply-header").TextContentAsync();
            }

            var jobTitle = await Page.Locator(".job-details-jobs-unified-top-card__job-title").TextContentAsync();
            bool successfullyApplied = false;

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

                if (await Page.GetByRole(AriaRole.Button, new() { NameString = "Submit application" }).IsVisibleAsync())
                {
                    await Page.GetByRole(AriaRole.Button, new() { NameString = "Submit application" }).ClickAsync();
                    successfullyApplied = true;
                }

            }

            if (companyTitle != null)
            {
                string replaceWith = "";
                var cleanCompanyTitle = companyTitle.Replace("Apply to", "").Replace("\n", replaceWith);
                var cleanJobTitle = jobTitle.Replace("\n", replaceWith);
                _applicationStatusEntries.Add(new JobApplicationStatusEntry(cleanCompanyTitle, successfullyApplied, DateTime.UtcNow, cleanJobTitle, true));
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
                //Get the textboxes question
                var inputId = await input.GetAttributeAsync("Id");
                var questionLabelElement = Page.Locator($"[for='{inputId}']");

                string question = "";

                if (questionLabelElement != null)
                {
                    if (await questionLabelElement.IsVisibleAsync())
                        question = await questionLabelElement.InnerTextAsync();
                } 
                
                var response = _responseInterpreter.GetQuestionResponse(question);

                if (response != null)
                {
                    await input.FillAsync(response);
                }
                else
                {
                    await input.FillAsync("5");
                }
            }

            var comboBoxes = await Page.QuerySelectorAllAsync("select");

            foreach (var box in comboBoxes)
            {
                var options = await box.QuerySelectorAllAsync("option");
                //Get the textboxes question
                var inputId = await box.GetAttributeAsync("Id");

                var questionLabelElement = Page.Locator($"[for='{inputId}']");

                string question = "";

                if (questionLabelElement != null)
                {
                    if (await questionLabelElement.IsVisibleAsync())
                        question = await questionLabelElement.InnerTextAsync();
                }

                var response = _responseInterpreter.GetQuestionResponse(question);

                if (await box.InputValueAsync() == "" || await box.InputValueAsync() == "Select an option")
                {
                    if (response != null)
                    {
                        var isInt = int.TryParse(response, out int result);

                        if (!isInt)
                        {
                            await box.SelectOptionAsync(response);
                        }
                        else
                        {
                            await box.SelectOptionAsync(await options[1].GetAttributeAsync("value"));
                        }

                    }
                    else
                    {
                        await box.SelectOptionAsync(await options[1].GetAttributeAsync("value") );
                    }
                }
            }

            var radioButtonQuestionContainer = await Page.QuerySelectorAllAsync("fieldset");

            foreach (var question in radioButtonQuestionContainer)
            {
                //Get the textboxes question

                var questionLabelElement = await question.QuerySelectorAsync("legend");

                string questionText = "";

                if (questionLabelElement != null)
                {
                    if (await questionLabelElement.IsVisibleAsync())
                        questionText = await questionLabelElement.InnerTextAsync();
                }

                var response = _responseInterpreter.GetQuestionResponse(questionText);
                var options = await question.QuerySelectorAllAsync("label");

                if (response != null)
                {
                    if (response == "yes")
                    {
                        await options[0].ClickAsync();
                    }
                    else
                    {
                        await options[1].ClickAsync();
                    }
                }
                else
                {
                    await options.FirstOrDefault().ClickAsync();
                }
            }
        }

        /// <summary>
        /// Closes out of the Easy Apply Form. Used when unable to submit, or after app submission.
        /// Used to keep flow when things don't work exactly as expected.
        /// </summary>
        /// <returns></returns>
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

        [OneTimeTearDown]
        public void GenerateExcelDocument()
        {
            var wb = new XLWorkbook();
            var workSheet = wb.AddWorksheet($"Job_Application_Record");

            workSheet.Cell(1, 1).Value = "Company";
            workSheet.Cell(1, 2).Value = "Job Title";
            workSheet.Cell(1, 3).Value = "Date Application Attempted";
            workSheet.Cell(1, 4).Value = "Application Status";
            workSheet.Cell(1, 5).Value = "Had Easy Apply";

            for (int i = 0; i < _applicationStatusEntries.Count; i++)
            {
                workSheet.Cell(i+2, 1).Value = _applicationStatusEntries[i].CompanyName;
                workSheet.Cell(i+2, 2).Value = _applicationStatusEntries[i].JobTitle;
                workSheet.Cell(i+2, 3).Value = _applicationStatusEntries[i].DateAttempted;
                workSheet.Cell(i+2, 4).Value = _applicationStatusEntries[i].ApplicationStatus;
                workSheet.Cell(i+2, 5).Value = _applicationStatusEntries[i].HadEasyApplyButton;
            }

            wb.SaveAs($"..//..//..//Results//ApplicationStatuses - {DateTime.UtcNow.Ticks}.xlsx");
        }


    }

    public class JobApplicationStatusEntry()
    {

        public string CompanyName { get; set; }
        public bool ApplicationStatus { get; set; }
        public DateTime DateAttempted { get; set; }
        public string JobTitle { get; set; }
        public bool HadEasyApplyButton { get; set; }

        public JobApplicationStatusEntry(string companyName, bool applicationStatus, DateTime dateAttempted, string jobTitle, bool hadEasyApply) : this()
        {
            this.CompanyName = companyName;
            this.ApplicationStatus = applicationStatus;
            this.DateAttempted = dateAttempted;
            this.JobTitle = jobTitle;
            this.HadEasyApplyButton = hadEasyApply;
        }
    }
}
